"""投递器验收：只认最新决策、尊重退避、遵守 --max。"""

import json
import subprocess
import sys
import tempfile
from pathlib import Path

import watchdog_backoff as backoff

ROOT = Path(__file__).resolve().parents[1]
tmp = Path(tempfile.mkdtemp())
decisions = tmp / 'decisions.jsonl'
sent = tmp / 'sent.jsonl'


def decision(role, wake_id):
    return {'decisionId': wake_id, 'at': '2026-09-14T12:00:00+00:00', 'role': role,
            'action': 'wake', 'reason': 'runnable_task_but_window_not_running',
            'threadId': 'T-' + role, 'taskId': 'task-' + role}


# 同一角色 5 条历史唤醒 + 1 条最新唤醒
rows = [decision('art-2d', f'old-{i}') for i in range(5)]
rows.append(decision('art-2d', 'new-1'))
rows.append(decision('qa', 'qa-1'))
decisions.write_text('\n'.join(json.dumps(r) for r in rows) + '\n', encoding='utf8')
sent.write_text(json.dumps({'decisionId': 'old-0'}) + '\n', encoding='utf8')

# 让退避状态落在临时目录，且线程活动时间不可观测（走固定冷却）
backoff.STATE = tmp / 'backoff.json'

out = subprocess.run(
    [sys.executable, str(ROOT / 'tools/agent_watchdog_wake.py'),
     '--dry-run', '--max', '8', '--decisions', str(decisions), '--sent', str(sent)],
    capture_output=True, text=True, encoding='utf8', errors='replace')
lines = [json.loads(x) for x in out.stdout.splitlines() if x.strip()]

role_ids = [(x.get('role'), x.get('action')) for x in lines]
print('plan:', role_ids)

# 每个角色只投递一条最新唤醒，历史不复活
assert role_ids.count(('art-2d', 'would_send')) == 1, role_ids
assert role_ids.count(('qa', 'would_send')) == 1, role_ids
assert not any(x.get('action') == 'would_send' and 'old' in str(x) for x in lines), lines

# --max 生效
out2 = subprocess.run(
    [sys.executable, str(ROOT / 'tools/agent_watchdog_wake.py'),
     '--dry-run', '--max', '1', '--decisions', str(decisions), '--sent', str(sent)],
    capture_output=True, text=True, encoding='utf8', errors='replace')
lines2 = [json.loads(x) for x in out2.stdout.splitlines() if x.strip()]
sent_count = sum(1 for x in lines2 if x.get('action') == 'would_send')
assert sent_count == 1, lines2

print('watchdog wake acceptance: 4/4 passed')
