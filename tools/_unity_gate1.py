# -*- coding: utf-8 -*-
"""触发 Unity 内的「门1 契约自检」菜单并回读结果。

完整清单由 Unity 侧的 checker 自己写进 `tools/_gate1_report.txt`——控制台消息会被
unity-skills 的摘要机制截断（长消息只回开头几行），所以这里**不再**用 console 文本
覆写那个文件，只读它。console 只用来给出通过/未通过的判定行。
"""
import json
import pathlib
import sys

sys.path.insert(0, '.agents/skills/unity-skills/scripts')
import unity_skills as u  # noqa: E402

MENU = sys.argv[1] if len(sys.argv) > 1 else 'Game Framework/AutoEra/UI/门1 契约自检'
FILTER = sys.argv[2] if len(sys.argv) > 2 else 'Gate1'
REPORT = pathlib.Path('tools/_gate1_report.txt')

print('菜单：', json.dumps(u.call_skill('editor_execute_menu', menuPath=MENU), ensure_ascii=False))

# checker 自己落盘；若它没能写（旧程序集），退回到 console 文本。
if REPORT.exists():
    print('--- 报告（由 checker 落盘，完整） ---')
    lines = REPORT.read_text(encoding='utf-8').splitlines()
    for line in lines[:40]:
        print('   ', line)
    if len(lines) > 40:
        print(f'    … 其余 {len(lines) - 40} 行见 {REPORT}')
    sys.exit(0)

for kind in ('Error', 'Warning', 'Log'):
    logs = u.call_skill('console_get_logs', type=kind, filter=FILTER, limit=6)
    items = logs.get('logs') or []
    if not items:
        continue
    print(f'--- {kind} ({len(items)}) ---   （报告文件缺失，回退到 console 文本，可能被截断）')
    for item in items[:1]:
        body = [ln for ln in item['message'].split('\n')
                if not ln.startswith(('UnityEngine.', 'UnityEditor.', 'AutoEra.', 'UnitySkills.'))]
        for line in body[:20]:
            print('   ', line)
