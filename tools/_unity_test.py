# -*- coding: utf-8 -*-
"""运行指定的 Unity 测试类并汇总结果。

用法：python tools/_unity_test.py <TestMode> <TypeName> [<TypeName> ...]
"""
import json
import os
import pathlib
import sys
import time
from xml.etree import ElementTree

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


def ensure_clean_scene():
    """PlayMode 运行前把「脏场景」清掉。

    实测踩坑（2026-09-21）：Test Runner 的 `SaveModifiedSceneTask` 会在运行开始时调用
    `EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()`，而那个 API **在 Play Mode 中会抛
    `InvalidOperationException: This cannot be used during play mode`**。结果是整次 PlayMode 运行
    在 4 秒内以「Unhandled log message」失败（一条断言都没跑），而失败信息看起来完全不像
    「场景没保存」——它只说「不能在 play mode 里用」。

    触发条件很普通：上一次 PlayMode 运行里测试在场景中建了对象（本仓的界面测试都会），
    场景因此变脏；下一次运行开始时编辑器如果还在 Play Mode（上一次没干净退出），
    保存请求就会撞上这条禁令。所以这里先退出 Play Mode，再执行一次 File/Save。
    """
    ensure_edit_ready()
    for _ in range(10):
        try:
            u.call_skill('editor_execute_menu', menuPath='File/Save')
            return
        except Exception:                                  # noqa: BLE001
            time.sleep(4)


if mode.lower() == 'playmode':
    ensure_clean_scene()


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


def latest_results_xml(since):
    """Unity 自己写的 TestResults.xml —— 独立于 unity-skills 插件的第二证据源。

    实测踩坑（2026-09-21）：带 `EnterPlayMode` 且会触发框架重启／长域重载序列的类
    （`AutoEraStartupFlowEditModeTests`）跑完后，插件可能报
    `Failed to reconnect test callbacks: Sharing violation on path Library/UnitySkills/batch_state.json`
    且 `totalTests = 0`——**但测试其实已经全部跑完并通过**：Unity 在同一时刻把结果写进了
    `%USERPROFILE%/AppData/LocalLow/<公司>/<产品>/TestResults.xml`（项目里是
    `ZZNC/星际拓荒：自动纪元/TestResults.xml`）。
    所以「插件报 totalTests=0」不等于「测试失败」，必须用这个文件复核，否则会把
    绿色基线误判成红色（这一条已经骗过两次）。
    只取运行开始之后被改写的文件，避免读到上一次运行的结果。
    """
    low = pathlib.Path(os.environ.get('USERPROFILE', '')) / 'AppData' / 'LocalLow'
    best = None
    for path in low.glob('*/*/TestResults.xml'):
        try:
            stamp = path.stat().st_mtime
        except OSError:
            continue
        if stamp < since:
            continue
        if best is None or stamp > best[0]:
            best = (stamp, path)
    return best[1] if best else None


def summarize_xml(path, expect_name=None):
    """把 TestResults.xml 汇总成和插件同形的结果。

    **必须核对夹具名**：这个文件是全产品共用的一个路径，任何一次测试运行都会覆盖它。
    实测踩坑（2026-09-21）：上一次 EditMode 运行因为域重载丢了回调，它的作业在后台继续跑，
    结果在我跑 PlayMode 时写下了**另一个夹具**的结果；当时只按「文件比本次开始时间新」判，
    于是把「启动流程 EditMode 的 5/6」当成了「PlayMode 那一套的结果」——差一点就变成
    一次假失败（也差点把真正的信号埋掉）。所以除了时间戳，还要确认夹具名字对得上。
    """
    root = ElementTree.parse(str(path)).getroot()
    suites = [s for s in root.iter('test-suite') if s.get('type') == 'TestFixture']
    if not suites:
        return None
    if expect_name is not None:
        wanted = expect_name.split('.')[-1]
        matching = [s for s in suites if wanted in (s.get('name') or '') or wanted in (s.get('fullname') or '')]
        if not matching:
            return None
        suites = matching
    suite = suites[0]
    failures = [case.get('fullname') for case in suite.iter('test-case') if case.get('result') != 'Passed']
    messages = []
    for case in suite.iter('test-case'):
        if case.get('result') == 'Passed':
            continue
        for node in case.iter('message'):
            if node.text:
                messages.append(node.text.strip())
    return {
        'success': True,
        'status': 'completed',
        'source': 'TestResults.xml',
        'totalTests': int(suite.get('total') or 0),
        'passedTests': int(suite.get('passed') or 0),
        'failedTests': int(suite.get('failed') or 0),
        'skippedTests': int(suite.get('skipped') or 0),
        'failedTestNames': failures,
        'failedTestDetails': messages,
        'resultSummary': f"Test run completed: {suite.get('passed')}/{suite.get('total')} passed. (TestResults.xml)",
        'error': None,
    }


def wait_for_results_xml(since, expect_name=None, timeout=1500):
    """等 Unity 把本次运行的 TestResults.xml 写完。

    为什么必须等：插件在长域重载序列上会在 ~50 秒时放弃并报重连失败，而测试还要再跑一分钟
    才结束、才写结果文件。所以「插件没拿到计数」时必须**继续等文件出现**，而不是立刻去读一个
    还不存在的文件（第一次实现就是这样漏掉的）。
    文件出现后再等编辑器回到空闲（不在 Play Mode、不在编译），确保读到的是完整结果。

    `expect_name` 是夹具核对（见 summarize_xml）：等到的文件可能属于**另一个**仍在后台跑的
    作业，写进来的夹具名对不上就继续等，而不是把它当成本次结果。

    超时值按实测定：`AutoEraStartupFlowEditModeTests`（6 个用例各自 EnterPlayMode ＋ 框架重启）
    在插件 95~130 秒放弃之后**还要再跑很久**才写结果文件。实测两次：
    10:15 启动 / 10:19:22 落盘（约 4 分钟），12:08 启动 / 12:24:41 落盘（约 16 分钟）。
    先给 360 秒、后给 660 秒都读到过「没有结果」并把一次干净的 6/6 当成失败，
    所以现在统一给 1500 秒——宁可让真正的失败慢一点报，也不要把绿色基线报成红色。
    """
    deadline = time.time() + timeout
    found = None
    while time.time() < deadline:
        candidate = latest_results_xml(since)
        if candidate is not None:
            probe = summarize_xml(candidate, expect_name)
            if probe is not None:
                found = candidate
                break
        time.sleep(8)

    if found is None:
        return None

    while time.time() < deadline:
        try:
            state = u.call_skill('editor_get_state')
        except Exception:                          # noqa: BLE001
            time.sleep(6)
            continue
        if not state.get('isPlaying') and not state.get('isCompiling'):
            return found
        time.sleep(6)

    return found


for name in names:
    print(f'===== {mode} :: {name} =====')
    if mode.lower() == 'editmode':
        # 每个类之前都确认一次：上一个类可能自己进入了 Play Mode。
        ensure_edit_ready()
    started = time.time()
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
    plugin_total = result.get('totalTests')
    plugin_text = json.dumps(result, ensure_ascii=False)
    if not plugin_total:
        # 插件没拿到计数（重连失败／超时）：不猜，等 Unity 自己的结果文件写完再读。
        xml = wait_for_results_xml(started - 5, name)
        if xml is not None:
            fallback = summarize_xml(xml, name)
            if fallback is not None:
                print(f'  插件返回：{plugin_text[:200]}')
                print(f'  插件未拿到计数，改用 Unity 结果文件：{xml}')
                result = fallback
    # 完整结果落盘：控制台只印摘要，失败详情（含测试期间的 output 日志）从这里读。
    pathlib.Path(f'tools/_test_result_{name}.json').write_text(
        json.dumps(result, ensure_ascii=False, indent=2), encoding='utf-8')
    print('  完整返回：', json.dumps(result, ensure_ascii=False)[:2500])
