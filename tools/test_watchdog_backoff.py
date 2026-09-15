"""watchdog_backoff 行为验收。"""

import json
import tempfile
from datetime import datetime, timedelta, timezone
from pathlib import Path

import watchdog_backoff as b

tmp = Path(tempfile.mkdtemp())
b.STATE = tmp / 'backoff.json'
observed = {'value': 1000.0}
b.thread_updated_at = lambda tid: observed['value']

now = datetime(2026, 9, 14, 12, 0, tzinfo=timezone.utc)

# 首次投递不受限
assert b.evaluate('art-2d', 'T', now)['defer'] is False

# 第一次投递后进入退避
b.record_attempt('art-2d', 'T', now)
v = b.evaluate('art-2d', 'T', now + timedelta(seconds=10))
assert v['defer'] is True and v['reason'] == 'backoff_active', v

# 冷却期结束后允许再次投递
v = b.evaluate('art-2d', 'T', now + timedelta(seconds=301))
assert v['defer'] is False, v

# 若线程活动时间前进，说明上次唤醒生效，退避清零
observed['value'] = 2000.0
v = b.evaluate('art-2d', 'T', now + timedelta(seconds=310))
assert v['defer'] is False and v['reason'] == 'previous_wake_took_effect', v
assert b.load() == {}, b.load()

# 连续未生效会逐级拉长间隔，并在阈值处转人工
observed['value'] = 3000.0
for i in range(1, b.MANUAL_THRESHOLD + 1):
    entry = b.record_attempt('qa', 'T', now)
    assert entry['attempts'] == i, entry
assert entry['blocked'] is True, entry
v = b.evaluate('qa', 'T', now + timedelta(days=1))
assert v['defer'] is True and v['reason'] == 'manual_intervention_required', v

# 无法观测线程活动时只做固定冷却，不误判成人工阻断
b.reset()
b.thread_updated_at = lambda tid: None
entry = b.record_attempt('client', 'T', now)
assert entry['blocked'] is False, entry
v = b.evaluate('client', 'T', now + timedelta(seconds=10))
assert v['defer'] is True and v['reason'] == 'backoff_active', v

# reset 对单角色生效
b.reset('client')
assert b.load() == {}, b.load()

print('watchdog backoff acceptance: 9/9 passed')
