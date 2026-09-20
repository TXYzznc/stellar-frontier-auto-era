# -*- coding: utf-8 -*-
"""运行指定的 Unity 测试类并汇总结果。

用法：python tools/_unity_test.py <TestMode> <TypeName> [<TypeName> ...]
"""
import json
import pathlib
import sys
import time

sys.path.insert(0, '.agents/skills/unity-skills/scripts')
import unity_skills as u  # noqa: E402

mode = sys.argv[1]
names = sys.argv[2:]


def ensure_edit_ready():
    """EditMode 运行前必须不在 Play Mode。

    实测踩坑：Unity Test Runner 的第一步是 `RestoreSceneSetupTask`，它调用
    `EditorSceneManager.RestoreSceneManagerSetup`，而这个 API **在 Play Mode 中会抛
    `InvalidOperationException: This cannot be used during play mode`**。结果是整次运行
    在任何测试执行之前就失败（`totalTests=0`），报一句笼统的
    「An unexpected error happened while running tests」，同时把编辑器继续留在 Play Mode——
    于是下一次运行也失败，形成自锁。只要前一次运行（尤其是 PlayMode 测试或带
    `EnterPlayMode` 的 EditMode 测试）没干净退出，所有 EditMode 运行都会连锁失败。
    """
    for _ in range(40):
        try:
            state = u.call_skill('editor_get_state')
        except Exception:                          # noqa: BLE001
            time.sleep(5)
            continue

        if state.get('isPlaying'):
            try:
                u.call_skill('editor_stop')
            except Exception as ex:                # noqa: BLE001
                print('  退出 Play Mode 失败：', ex)
            time.sleep(8)
            continue

        if not state.get('isCompiling'):
            return state
        time.sleep(5)

    print('  警告：编辑器在等待后仍未就绪（可能要手动介入）')
    return None


if mode.lower() == 'editmode':
    ensure_edit_ready()


def wait(job_id, timeout=1200):
    """EditMode 测试会触发域重载，轮询期间 REST 可能短暂不可用——这是可恢复的，不是失败。"""
    start = time.time()
    last_error = None
    while time.time() - start < timeout:
        try:
            result = u.call_skill('test_get_result', jobId=job_id)
        except Exception as ex:                   # noqa: BLE001
            last_error = str(ex)
            time.sleep(6)
            continue

        state = str(result.get('status', '')).lower()
        if state not in ('running', 'pending', 'inprogress', 'notstarted', ''):
            return result
        time.sleep(5)
    return {'status': 'timeout', 'lastError': last_error}


for name in names:
    print(f'===== {mode} :: {name} =====')
    if mode.lower() == 'editmode':
        # 每个类之前都确认一次：上一个类可能自己进入了 Play Mode。
        ensure_edit_ready()
    try:
        job = u.call_skill('test_run_by_name', testName=name, testMode=mode)
    except Exception as ex:                       # noqa: BLE001
        print('  启动失败：', ex)
        continue

    job_id = job.get('jobId') or job.get('id')
    print('  job:', job_id)
    if not job_id:
        print('  返回：', json.dumps(job, ensure_ascii=False)[:300])
        continue

    result = wait(job_id)
    # 完整结果落盘：控制台只印摘要，失败详情（含测试期间的 output 日志）从这里读。
    pathlib.Path(f'tools/_test_result_{name}.json').write_text(
        json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
    print('  完整返回：', json.dumps(result, ensure_ascii=False)[:2500])
