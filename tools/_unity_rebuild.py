# -*- coding: utf-8 -*-
"""通过 unity-skills REST 触发「从契约重建所有页面」并回读结果。

可选：用 TARGETS 限定本次要重建的契约，其余契约会临时改名避让；
TARGETS 为空集时表示目录下每一份契约都重建（当前全部契约都由规格文档生成）。
"""
import json
import sys
import time
from pathlib import Path

# 输出编码交给 PYTHONIOENCODING=utf-8；此处不包装 sys.stdout，
# 否则 unity_skills 导入后再写会落到已关闭的 wrapper 上。
sys.path.insert(0, '.agents/skills/unity-skills/scripts')

import unity_skills as u  # noqa: E402

CONTRACT_DIR = Path('Docs/Development/UI-PrefabLayouts')
HOLD = {p: p.with_suffix('.json.hold') for p in CONTRACT_DIR.glob('*.contract.json')}
TARGETS: set[str] = set()

MENU = 'Game Framework/AutoEra/UI/从契约重建所有页面'


def rebuild_with_retry(attempts=5):
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
            last = json.dumps(result, ensure_ascii=False)[:200]
        time.sleep(4)
    return {'success': False, 'error': last}, attempts


held = []
try:
    for src, dst in HOLD.items():
        if TARGETS and src.stem.split('.')[0] not in TARGETS:
            src.rename(dst)
            held.append((src, dst))
    print('临时避让的旧契约：', [s.name for s, _ in held])

    result, tries = rebuild_with_retry()
    print(f'菜单调用结果（第 {tries} 次尝试）：', json.dumps(result, ensure_ascii=False)[:400])
finally:
    for src, dst in held:
        if dst.exists():
            dst.rename(src)
    print('已还原：', [s.name for s, _ in held])

    left = sorted(p.name for p in CONTRACT_DIR.glob('*.contract.json'))
    print('目录现有契约：', left)
