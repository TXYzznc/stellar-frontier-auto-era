# -*- coding: utf-8 -*-
"""通过 unity-skills REST 触发「按契约刷新所有页面绑定（不改结构）」并回读结果。

用途：给契约加了绑定（`Tools/ui_spec_to_contract.py` 的 EXTRA_BINDINGS）之后，
必须把这个绑定写进预制体的 SerializeField 引用里，否则门 1 的 L3 会报「绑定为空」。
这个菜单**不改预制体结构**，是手工调完外观后刷新绑定的正式入口。

注意：原有的「从契约重建预制体」「按契约刷新绑定（不改结构）」两个菜单只作用于
DefaultContractPath（BaseCommandHubForm），对别的 Form 跑它们会静默地什么都不做。
"""
import json
import sys
import time

# 输出编码交给 PYTHONIOENCODING=utf-8；此处不包装 sys.stdout，
# 否则 unity_skills 导入后再写会落到已关闭的 wrapper 上。
sys.path.insert(0, '.agents/skills/unity-skills/scripts')

import unity_skills as u  # noqa: E402

MENU = 'Game Framework/AutoEra/UI/按契约刷新所有页面绑定（不改结构）'


def call_with_retry(attempts=5):
    """脚本改动会触发域重载，重载窗口内 REST 请求会被中止（Thread was being aborted）。
    这是可恢复的瞬时状态，等编译稳定后重试即可。"""
    last = None
    for i in range(attempts):
        try:
            result = u.call_skill('editor_execute_menu', menuPath=MENU)
        except Exception as ex:                     # noqa: BLE001 - 连接层异常一律重试
            last = str(ex)
        else:
            if result.get('success'):
                return result, i + 1
            last = json.dumps(result, ensure_ascii=False)[:300]
        time.sleep(4)
    return {'success': False, 'error': last}, attempts


if __name__ == '__main__':
    result, tries = call_with_retry()
    print(f'菜单调用结果（第 {tries} 次尝试）：', json.dumps(result, ensure_ascii=False)[:600])
    sys.exit(0 if result.get('success') else 1)
