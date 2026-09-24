# -*- coding: utf-8 -*-
"""汇总 tools/_test_result_<TypeName>.json 的最近结果（只读，不跑测试）。

用法：python tools/_test_summary.py <TypeName> [<TypeName> ...]

当某个类的 JSON 里没有计数（插件丢了回调、等待窗口已过）时，**回退去读 Unity 自己写的
`TestResults.xml` 并核对夹具名**——那是唯一还在的第二证据源。实测踩坑：插件丢回调后
结果文件可能十几分钟后才落盘，比任何等待窗口都晚，于是 JSON 里只有「0 个测试」，
而测试其实全过了。
"""
import json
import os
import pathlib
import sys
import xml.etree.ElementTree as ElementTree

names = sys.argv[1:]
if not names:
    raise SystemExit('用法：python tools/_test_summary.py <TypeName> [...]')


def results_xml():
    low = pathlib.Path(os.environ.get('USERPROFILE', '')) / 'AppData' / 'LocalLow'
    best = None
    for path in low.glob('*/*/TestResults.xml'):
        try:
            stamp = path.stat().st_mtime
        except OSError:
            continue
        if best is None or stamp > best[0]:
            best = (stamp, path)
    return best[1] if best else None


def from_xml(path, name):
    root = ElementTree.parse(str(path)).getroot()
    wanted = name.split('.')[-1]
    for suite in root.iter('test-suite'):
        if suite.get('type') != 'TestFixture':
            continue
        if wanted not in (suite.get('name') or '') and wanted not in (suite.get('fullname') or ''):
            continue
        failures = [c.get('fullname') for c in suite.iter('test-case') if c.get('result') != 'Passed']
        return {
            'passedTests': int(suite.get('passed') or 0),
            'totalTests': int(suite.get('total') or 0),
            'failedTestNames': failures,
            'source': 'TestResults.xml',
            'status': 'completed',
        }
    return None


bad = 0
for name in names:
    path = f'tools/_test_result_{name}.json'
    data = None
    if os.path.exists(path):
        with open(path, encoding='utf-8') as handle:
            data = json.load(handle)

    total = (data or {}).get('totalTests')
    if not total:
        xml = results_xml()
        fallback = from_xml(xml, name) if xml is not None else None
        if fallback is not None:
            data = fallback

    if data is None:
        print(f'{name}: (无结果文件)')
        bad += 1
        continue

    passed = data.get('passedTests')
    total = data.get('totalTests')
    failed = data.get('failedTestNames') or []
    ok = bool(total) and not failed and passed == total
    print(f"{name}: {'OK ' if ok else 'BAD'} {passed}/{total} "
          f"src={data.get('source', 'plugin')} status={data.get('status')} fail={failed}")
    if not ok:
        bad += 1
        details = data.get('failedTestDetails') or []
        for item in details[:5]:
            print('    ', str(item)[:300])

raise SystemExit(1 if bad else 0)
