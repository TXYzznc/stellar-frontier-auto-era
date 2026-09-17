import json
import tempfile
from datetime import datetime, timezone
from pathlib import Path

from AgentWatchdogUI import (
    build_team_registry,
    dsh_freshness,
    dsh_member_rows,
    dsh_mode_active,
    dsh_task_rows,
    load_dsh_state,
    queue_task_rows,
    split_completed_queue_rows,
    write_dsh_inbox_message,
)


def test_queue_task_rows_uses_lifecycle_status_over_queue_bucket():
    entry = {
        'active': {'taskId': 'active', 'title': '执行任务'},
        'pending': [{'taskId': 'pending', 'title': '排队任务'}],
        'suspended': [{'taskId': 'waiting', 'title': '协作任务'}],
        'completed': [{'taskId': 'done', 'title': '历史任务'}],
    }
    lifecycle = {
        'waiting-key': {'taskId': 'waiting', 'state': 'AwaitingCollaboration'},
        'done-key': {'taskId': 'done', 'state': 'AwaitingProducerAcceptance'},
    }
    assert queue_task_rows(entry, lifecycle) == [
        ('done', '历史任务', '待验收'),
        ('active', '执行任务', '执行中'),
        ('pending', '排队任务', '排队中'),
        ('waiting', '协作任务', '待协作'),
    ]


def test_split_completed_queue_rows_separates_completed_from_active():
    rows = [
        ('done', '历史任务', '已完成'),
        ('active', '执行任务', '执行中'),
        ('pending', '排队任务', '排队中'),
    ]
    completed, remaining = split_completed_queue_rows(rows)
    assert completed == [('done', '历史任务', '已完成')]
    assert remaining == [('active', '执行任务', '执行中'), ('pending', '排队任务', '排队中')]


def test_build_team_registry_only_contains_selected_pinned_windows():
    catalog = {
        't1': {'title': 'AutoEra｜客户端', 'cwd': 'D:/project', 'projectId': 'project-1'},
        't2': {'title': 'AutoEra｜测试', 'cwd': 'D:/project', 'projectId': 'project-1'},
    }
    old = {
        'legacy': {'threadId': 't1', 'hostId': 'remote', 'unityPort': 8100, 'status': 'ready'},
        'excluded': {'threadId': 'not-selected'},
    }
    result = build_team_registry(['t1'], catalog, old, {'t1': 'client'})
    assert list(result) == ['client']
    assert result['client']['threadId'] == 't1'
    assert result['client']['hostId'] == 'remote'
    assert result['client']['unityPort'] == 8100
    assert 't2' not in {item['threadId'] for item in result.values()}


def test_dsh_mode_active_detects_state_file():
    tmp = Path(tempfile.mkdtemp())
    path = tmp / 'dsh-team-state.local.json'
    assert dsh_mode_active(path) is False
    path.write_text('{}', encoding='utf8')
    assert dsh_mode_active(path) is True


def test_load_dsh_state_parses_members_and_tasks():
    tmp = Path(tempfile.mkdtemp())
    path = tmp / 'dsh-team-state.local.json'
    path.write_text(json.dumps({
        'schemaVersion': 1,
        'generatedAt': '2026-09-17T15:52:22+08:00',
        'team': 'autoera-dsh',
        'members': [{'name': 'dev', 'role': '程序·工具工程师', 'activity': 'working'}],
        'tasks': [{'id': 't1', 'subject': '守护器适配 DSH', 'status': 'in_progress', 'assignee': 'dev', 'output': ''}],
    }, ensure_ascii=False), encoding='utf8')
    state = load_dsh_state(path)
    assert state['team'] == 'autoera-dsh'
    assert dsh_member_rows(state) == [('dev', '程序·工具工程师', 'working')]
    assert dsh_task_rows(state) == [('t1', '守护器适配 DSH', 'in_progress', 'dev')]
    # 无状态文件时返回空字典，保持旧行为
    assert load_dsh_state(tmp / 'missing.json') == {}
    assert dsh_member_rows({}) == []
    assert dsh_task_rows({}) == []


def test_dsh_freshness_stale_detection():
    now = datetime(2026, 9, 17, 16, 0, 0, tzinfo=timezone.utc)
    fresh = dsh_freshness('2026-09-17T15:58:00+00:00', now=now)
    assert fresh['stale'] is False, fresh
    assert fresh['label'] == '2分钟前', fresh
    stale = dsh_freshness('2026-09-17T10:00:00+00:00', now=now)
    assert stale['stale'] is True, stale
    future = dsh_freshness('2026-09-17T16:05:00+00:00', now=now)
    assert future['stale'] is True, future
    broken = dsh_freshness('not-a-date', now=now)
    assert broken['stale'] is True and broken['label'] == '无法解析生成时间', broken


def test_write_dsh_inbox_message_appends_jsonl():
    tmp = Path(tempfile.mkdtemp())
    inbox = tmp / 'captain.jsonl'
    record = write_dsh_inbox_message('  给队长的测试消息  ', inbox_path=inbox)
    assert record['to'] == 'captain' and record['content'] == '给队长的测试消息'
    lines = [json.loads(line) for line in inbox.read_text(encoding='utf8').splitlines() if line.strip()]
    assert len(lines) == 1
    assert lines[0]['from'] == 'watchdog-ui'
    assert lines[0]['to'] == 'captain'
    assert lines[0]['content'] == '给队长的测试消息'
    assert isinstance(lines[0]['ts'], int)
    # 空内容拒绝写入
    try:
        write_dsh_inbox_message('   ', inbox_path=inbox)
        raise AssertionError('空内容应当拒绝')
    except ValueError:
        pass


if __name__ == '__main__':
    test_queue_task_rows_uses_lifecycle_status_over_queue_bucket()
    test_split_completed_queue_rows_separates_completed_from_active()
    test_build_team_registry_only_contains_selected_pinned_windows()
    test_dsh_mode_active_detects_state_file()
    test_load_dsh_state_parses_members_and_tasks()
    test_dsh_freshness_stale_detection()
    test_write_dsh_inbox_message_appends_jsonl()
    print('watchdog UI helpers: 7/7 passed')
