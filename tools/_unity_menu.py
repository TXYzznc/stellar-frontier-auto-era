# -*- coding: utf-8 -*-
"""通过 unity-skills REST 跑一个 Unity 菜单（超时自动重试）。

用法：python Tools/_unity_menu.py "Game Framework/GameTools/Refresh All Excels【刷新所有数据表】"

为什么要脚本而不是手点：脚本改动会触发域重载，重载窗口内 REST 请求会被中止
（Thread was being aborted）——那是可恢复的瞬时状态，重试即可；而菜单执行结果
必须回读到，否则「菜单没跑成」会被误当成「跑成了但没变化」。
"""
import json
import sys
import time

sys.path.insert(0, '.agents/skills/unity-skills/scripts')
import unity_skills as u  # noqa: E402


def run(menu: str, attempts: int = 5) -> dict:
    last = None
    for i in range(attempts):
        try:
            result = u.call_skill('editor_execute_menu', menuPath=menu)
        except Exception as ex:                     # noqa: BLE001 - 连接层异常一律重试
            last = str(ex)
        else:
            if result.get('success'):
                return result
            last = json.dumps(result, ensure_ascii=False)[:400]
        time.sleep(4)
    return {'success': False, 'error': last}


if __name__ == '__main__':
    if len(sys.argv) < 2:
        print('用法：python Tools/_unity_menu.py "<菜单路径>"')
        raise SystemExit(2)

    menu_path = sys.argv[1]
    outcome = run(menu_path)
    print(json.dumps(outcome, ensure_ascii=False)[:800])
    raise SystemExit(0 if outcome.get('success') else 1)
