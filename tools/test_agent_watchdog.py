import json
import subprocess
import tempfile
from pathlib import Path

p = Path(tempfile.mkdtemp())
q = p / 'q'
r = p / 'r'
l = p / 'l'
NO_DSH = p / 'no-dsh-state.json'  # 不存在的状态文件：验证旧行为不变


def run(qv, lv, st, backend=None, dsh_state_file=None):
    q.write_text(json.dumps({'roles': {'x': qv}}))
    r.write_text(json.dumps({'windows': {'x': {'threadId': 'TEST'}}}))
    l.write_text(json.dumps(lv))
    cmd = ['python', 'tools/agent_watchdog.py', '--role', 'x', '--window-state', f'x={st}',
           '--queue-file', str(q), '--registry-file', str(r), '--lifecycle-file', str(l),
           '--dsh-state-file', str(dsh_state_file or NO_DSH)]
    if backend:
        cmd += ['--backend', backend]
    return json.loads(subprocess.check_output(cmd, text=True))[0]


# 旧行为（无 dsh 状态文件）：判定逻辑与历史版本一致
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Active'}}, 'idle')['action'] == 'wake'
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Active'}}, 'running')['action'] == 'none'
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'AwaitingProducerAcceptance'}}, 'idle')['action'] == 'none'
assert run({'active': None, 'pending': [{'taskId': 'p', 'state': 'queued'}]}, {}, 'notLoaded')['action'] == 'wake'
assert run({'active': None, 'pending': [{'taskId': 'p', 'state': 'queued'}]}, {}, 'paused')['action'] == 'none'
assert run({'active': None, 'pending': []}, {}, 'idle')['action'] == 'none'
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Cancelled'}}, 'idle')['action'] == 'none'
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Cancelled'}}, 'notLoaded')['action'] == 'none'
assert run({'active': {'taskId': 'a'}, 'pending': [{'taskId': 'p', 'state': 'queued'}]}, {'a': {'taskId': 'a', 'state': 'Cancelled'}}, 'idle')['action'] == 'wake'
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Rework'}}, 'idle')['action'] == 'wake'

# DSH 模式（显式指定）：action 固定为 record，不生成唤醒目标（threadId 为空）
d = run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Active'}}, 'idle', backend='dsh')
assert d['action'] == 'record', d
assert d.get('threadId') is None, d
assert d.get('backend') == 'dsh', d
# 即便窗口本应运行、无需唤醒，DSH 模式也固定 record
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Active'}}, 'running', backend='dsh')['action'] == 'record'
# 即便原本无可执行任务，DSH 模式也固定 record（记录性决策不落空）
assert run({'active': None, 'pending': []}, {}, 'idle', backend='dsh')['action'] == 'record'

# dsh 状态文件存在时自动进入 DSH 模式（无需显式 --backend）
dsh_file = p / 'dsh-team-state.local.json'
dsh_file.write_text(json.dumps({
    'schemaVersion': 1, 'generatedAt': '2026-09-17T15:52:22+08:00', 'team': 'autoera-dsh',
    'members': [], 'tasks': [],
}), encoding='utf8')
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Active'}}, 'idle', dsh_state_file=dsh_file)['action'] == 'record'

# 显式 --backend codex 时，即使状态文件存在也保持旧行为
assert run({'active': {'taskId': 'a'}, 'pending': []}, {'a': {'taskId': 'a', 'state': 'Active'}}, 'running', backend='codex', dsh_state_file=dsh_file)['action'] == 'none'

print('watchdog acceptance: 15/15 passed')
