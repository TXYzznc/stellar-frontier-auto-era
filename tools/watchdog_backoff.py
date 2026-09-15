"""守护器投递退避。

目标窗口被唤醒后没有实际恢复时，继续按固定间隔重发只会放大限流和额度消耗。
这里以线程的真实活动时间为依据判断上一次唤醒是否生效：

- 线程活动时间前进 -> 唤醒生效，退避清零；
- 线程活动时间不动 -> 按连续未生效次数指数退避；
- 连续多次未生效 -> 停止自动投递，等待人工处理。
"""

from __future__ import annotations

import json
import os
import sqlite3
from datetime import datetime, timedelta, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STATE = ROOT / '.ai/dispatch/watchdog-backoff.local.json'
CODEX_HOME = Path(os.environ.get('CODEX_HOME') or (Path.home() / '.codex'))
CATALOG_DB = CODEX_HOME / 'sqlite' / 'codex-dev.db'

# 连续未生效次数 -> 下一次投递前的等待秒数（最后一档封顶）
SCHEDULE = (300, 900, 1800, 3600)
# 达到该次数后停止自动投递，改为人工处理
MANUAL_THRESHOLD = 5
# 无法观测线程活动时使用的固定冷却，不进入人工阻断
UNOBSERVABLE_COOLDOWN_SECONDS = 300


def _now() -> datetime:
    return datetime.now(timezone.utc)


def _parse(value):
    try:
        return datetime.fromisoformat(value)
    except Exception:
        return None


def load() -> dict:
    try:
        data = json.loads(STATE.read_text(encoding='utf8'))
    except Exception:
        return {}
    return data if isinstance(data, dict) else {}


def save(data: dict) -> None:
    STATE.parent.mkdir(parents=True, exist_ok=True)
    tmp = STATE.with_suffix('.tmp')
    tmp.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf8')
    tmp.replace(STATE)


def thread_updated_at(thread_id: str | None) -> float | None:
    """读取线程最近一次实际活动时间；目录库不可用时返回 None。"""
    if not thread_id or not CATALOG_DB.is_file():
        return None
    try:
        con = sqlite3.connect('file:' + CATALOG_DB.as_posix() + '?mode=ro', uri=True)
        try:
            row = con.execute(
                'select source_updated_at from local_thread_catalog where thread_id=?', (thread_id,)
            ).fetchone()
        finally:
            con.close()
    except Exception:
        return None
    if not row or row[0] is None:
        return None
    try:
        return float(row[0])
    except (TypeError, ValueError):
        return None


def cooldown_seconds(attempts: int) -> int:
    if attempts <= 0:
        return 0
    return SCHEDULE[min(attempts, len(SCHEDULE)) - 1]


def evaluate(role: str, thread_id: str | None = None, now: datetime | None = None) -> dict:
    """判断该角色当前是否可以投递唤醒。"""
    now = now or _now()
    entry = load().get(role) or {}
    attempts = int(entry.get('attempts', 0) or 0)
    if not entry:
        return {'defer': False, 'attempts': 0, 'reason': 'first_attempt'}

    observed = thread_updated_at(thread_id)
    recorded = entry.get('observedAt')
    if observed is not None and recorded is not None and observed > float(recorded):
        reset(role)
        return {'defer': False, 'attempts': 0, 'reason': 'previous_wake_took_effect'}

    if entry.get('blocked'):
        return {'defer': True, 'attempts': attempts, 'reason': 'manual_intervention_required',
                'nextAllowedAt': entry.get('nextAllowedAt')}

    next_at = _parse(entry.get('nextAllowedAt'))
    if next_at and now < next_at:
        return {'defer': True, 'attempts': attempts, 'reason': 'backoff_active',
                'nextAllowedAt': entry.get('nextAllowedAt')}
    return {'defer': False, 'attempts': attempts, 'reason': 'cooldown_elapsed'}


def record_attempt(role: str, thread_id: str | None = None, now: datetime | None = None) -> dict:
    """记录一次已投递的唤醒，并安排下一次最早允许投递的时间。"""
    now = now or _now()
    observed = thread_updated_at(thread_id)
    data = load()
    entry = data.get(role) or {}
    attempts = int(entry.get('attempts', 0) or 0) + 1
    if observed is None:
        wait = UNOBSERVABLE_COOLDOWN_SECONDS
        blocked = False
    else:
        wait = cooldown_seconds(attempts)
        blocked = attempts >= MANUAL_THRESHOLD
    entry.update({
        'attempts': attempts,
        'lastAttemptAt': now.isoformat(),
        'nextAllowedAt': (now + timedelta(seconds=wait)).isoformat(),
        'observedAt': observed,
        'blocked': blocked,
    })
    data[role] = entry
    save(data)
    return entry


def record_deferred(role: str, reason: str) -> None:
    data = load()
    entry = data.get(role) or {}
    entry['lastDeferredReason'] = reason
    data[role] = entry
    save(data)


def reset(role: str | None = None) -> None:
    """清除退避状态；role 为空时清空全部。"""
    if role is None:
        save({})
        return
    data = load()
    if role in data:
        data.pop(role, None)
        save(data)
