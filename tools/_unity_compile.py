# -*- coding: utf-8 -*-
"""强制 Unity 重编译，等到程序集**真的**比源码新，再回读 CS 错误。

为什么不能只看 console：`debug_force_recompile` 返回时编译可能尚未开始或尚未落盘，
此时立刻读 console 会得到「0 错误」的假绿。实测过一次代价：预制体重建跑在旧程序集上，
门1 报出 2278 项失败（新字段在类型里还不存在，生成器把它们全丢了）。

因此判据是双重的：
  ① Library/ScriptAssemblies/Hotfix.dll 的写入时间晚于 Assets/Game/Scripts 下最新的 .cs；
  ② console 里没有 `error CS`。
两者同时成立才算编译通过。AutoEra 运行时代码属于 Assets/Game/Scripts/Hotfix.asmdef。
"""
from __future__ import annotations

import pathlib
import sys
import time

SCRIPTS_DIR = pathlib.Path("Assets/Game")
ASSEMBLIES_DIR = pathlib.Path("Library/ScriptAssemblies")
REPORT = pathlib.Path("tools/_compile_report.txt")

sys.path.insert(0, ".agents/skills/unity-skills/scripts")
import unity_skills as u  # noqa: E402


def newest_source_mtime() -> float:
    latest = 0.0
    for path in SCRIPTS_DIR.rglob("*.cs"):
        try:
            latest = max(latest, path.stat().st_mtime)
        except OSError:
            continue
    return latest


def newest_assembly_mtime() -> float:
    """取所有程序集里最新的一个：改运行时脚本产物是 Hotfix.dll，改测试是 *Tests.dll。"""
    latest = 0.0
    for path in ASSEMBLIES_DIR.glob("*.dll"):
        try:
            latest = max(latest, path.stat().st_mtime)
        except OSError:
            continue
    return latest


def wait_rest(timeout: float) -> bool:
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            if u.health():
                return True
        except Exception:  # noqa: BLE001
            pass
        time.sleep(3)
    return False


def read_errors(limit: int = 200) -> list[str]:
    try:
        logs = u.call_skill('console_get_logs', type='Error', limit=limit)
    except Exception:  # noqa: BLE001
        return []
    return [item.get('message', '') for item in (logs.get('logs') or []) if 'error CS' in item.get('message', '')]


def main() -> int:
    source_time = newest_source_mtime()
    print(f"源码最新写入：{time.strftime('%H:%M:%S', time.localtime(source_time))}")

    if not wait_rest(180):
        print("Unity REST 在 180 秒内没有恢复；可能编辑器正忙或已关闭。")
        return 2

    try:
        # 后台运行的 Unity 不会自动重编译，必须显式刷新；实测 Assets/Refresh 比
        # debug_force_recompile 更快落地（后者会走一遍完整域重载）。
        u.call_skill('editor_execute_menu', menuPath='Assets/Refresh')
        print('已触发 Assets/Refresh')
    except Exception as exc:  # noqa: BLE001
        print('刷新请求失败（通常在域重载中，继续等待）：', exc)

    deadline = time.time() + 600
    while time.time() < deadline:
        if newest_assembly_mtime() > source_time:
            break
        time.sleep(5)
    else:
        errors = read_errors()
        if errors:
            REPORT.write_text('\n\n'.join(errors), encoding='utf-8')
            print(f"编译未产出新程序集，且 console 有 {len(errors)} 条 CS 错误：")
            for message in errors[:40]:
                print('  ', message.split('\n')[0][:220])
            return 1
        print('程序集在 600 秒内没有更新，且 console 无 CS 错误——请人工确认 Unity 是否卡在导入/编译。')
        return 2

    settle = time.time() + 20
    while time.time() < settle:
        time.sleep(4)

    errors = read_errors()
    REPORT.write_text('\n\n'.join(errors), encoding='utf-8')
    if errors:
        print(f"编译错误 {len(errors)} 条（完整清单 tools/_compile_report.txt）")
        for message in errors[:60]:
            print('  ', message.split('\n')[0][:220])
        return 1

    print('编译通过：程序集已更新且 console 无 CS 错误')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
