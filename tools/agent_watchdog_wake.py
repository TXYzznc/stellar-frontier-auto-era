"""投递守护器唤醒消息。

只处理尚未投递的成功决策；投递前先经过退避检查，避免对没有恢复的窗口
反复重发、把上游额度打穿。参数 --max 限制单轮实际投递条数。
"""

import argparse
import json
import subprocess
from pathlib import Path

import watchdog_backoff as backoff

ROOT = Path(__file__).resolve().parents[1]
D = ROOT / '.ai/dispatch/watchdog-decisions.local.jsonl'
S = ROOT / '.ai/dispatch/watchdog-sent.local.jsonl'


def load_decision_records(path: Path) -> list[dict]:
    if not path.exists():
        return []
    records = []
    for line in path.read_text(encoding='utf8').splitlines():
        try:
            records.append(json.loads(line))
        except Exception:
            continue
    return records


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument('--dry-run', action='store_true')
    ap.add_argument('--max', type=int, default=8, help='单轮最多投递的唤醒条数')
    ap.add_argument('--decisions', default=str(D))
    ap.add_argument('--sent', default=str(S))
    a = ap.parse_args()
    dp, sp = Path(a.decisions), Path(a.sent)

    # 只认最近一次决策，避免历史累积的旧唤醒被反复复活。
    latest: dict[str, dict] = {}
    for d in load_decision_records(dp):
        role = d.get('role')
        if role:
            latest[role] = d
    sent = {d.get('decisionId') for d in load_decision_records(sp)}

    delivered = 0
    for role, d in sorted(latest.items()):
        if d.get('action') != 'wake' or d.get('decisionId') in sent or not d.get('threadId'):
            continue
        if delivered >= a.max:
            break
        verdict = backoff.evaluate(role, d.get('threadId'))
        if verdict.get('defer'):
            backoff.record_deferred(role, verdict.get('reason', ''))
            if a.dry_run:
                print(json.dumps({'role': role, 'action': 'would_defer',
                                  'reason': verdict.get('reason'),
                                  'nextAllowedAt': verdict.get('nextAllowedAt')}, ensure_ascii=False))
            continue
        msg = (f"守护器唤醒：请读取自己的任务队列并继续执行。"
               f"wakeId={d['decisionId']} taskId={d.get('taskId', '')}")
        if a.dry_run:
            print(json.dumps({'role': role, 'action': 'would_send',
                              'threadId': d['threadId']}, ensure_ascii=False))
            delivered += 1
            continue
        flags = getattr(subprocess, 'CREATE_NO_WINDOW', 0)
        p = subprocess.run(['codex', 'queue', '--thread', d['threadId'], '--message', msg],
                           capture_output=True, text=True, creationflags=flags)
        ok = p.returncode == 0
        d['delivery'] = 'sent' if ok else 'failed'
        d['error'] = p.stderr[-500:]
        sp.parent.mkdir(parents=True, exist_ok=True)
        with sp.open('a', encoding='utf8') as fh:
            fh.write(json.dumps(d, ensure_ascii=False) + '\n')
        sent.add(d['decisionId'])
        delivered += 1
        # 无论投递成功与否都安排退避：失败也可能来自上游限流，立即重试只会加重问题。
        backoff.record_attempt(role, d.get('threadId'))


if __name__ == '__main__':
    main()
