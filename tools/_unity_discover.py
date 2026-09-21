# -*- coding: utf-8 -*-
"""刷新 unity-skills 的测试发现结果。

新增测试类之后必须刷新一次，否则 `test_run_by_name` 会用上一次的发现快照，
报「找不到该类」或跑出过期计数（实测踩过）。发现是异步的，需要等它跑完。

用法：python tools/_unity_discover.py [EditMode|PlayMode ...]   （缺省两个都刷）
"""
import json
import sys
import time

sys.path.insert(0, '.agents/skills/unity-skills/scripts')
import unity_skills as u  # noqa: E402

mode = sys.argv[1:] or ['EditMode', 'PlayMode']

for test_mode in mode:
    print(f'===== 刷新发现 :: {test_mode} =====')
    try:
        job = u.call_skill('test_discover_start', testMode=test_mode)
    except Exception as ex:                       # noqa: BLE001
        print('  启动失败：', ex)
        continue

    job_id = job.get('jobId') or job.get('id')
    print('  job:', job_id, json.dumps(job, ensure_ascii=False)[:200])
    if not job_id:
        continue

    # 发现要等编辑器扫完所有程序集；轮询期间 REST 可能因域重载短暂不可用。
    for _ in range(40):
        time.sleep(5)
        try:
            result = u.call_skill('test_discover_get_result', jobId=job_id)
        except Exception as ex:                   # noqa: BLE001
            print('  轮询中断（可恢复）：', ex)
            continue

        status = str(result.get('status', '')).lower()
        print('  状态：', status, json.dumps(result, ensure_ascii=False)[:200])
        if status not in ('running', 'pending', 'inprogress', 'notstarted', ''):
            break
