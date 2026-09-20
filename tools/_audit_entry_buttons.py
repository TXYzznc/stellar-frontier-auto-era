# -*- coding: utf-8 -*-
"""审计每个 Form 的入口/工具栏按钮接线情况。

**先读这条限制**：契约只为生成器 `EXTRA_BINDINGS` 里显式声明的按钮产出绑定字段，
页面里其余 `Btn_*` 节点**根本没有 SerializeField**（未就绪域的界面靠
`DisableDomainActions()` 按结构名在运行期统一禁用，不需要逐个声明）。
所以本脚本回答的是「**已声明的**入口按钮有没有人处理」，不是「页面上所有按钮都能用」。
要回答后者得逐个节点比对树与 `EXTRA_BINDINGS`——`tools/_inspect_form.py` 打印的
「业务按钮（N）」是树里的节点数，两者相差的数目就是「有节点、无绑定」的按钮。

判定口径（三条豁免，每条都对应一个"已经有人统一处理"的事实）：

1. `_backButton` / `_closeButton` —— 生成器在骨架里就挂了 `RequestCancel`，
   或换成 `_firstInteractable` 的首焦点路径；不需要业务代码再管。
2. `_navButtons` / `_pageRoots` —— 生成器统一装配页导航。
3. 生成器 `NOT_WIRED` 集合里的界面 —— 它注入的 `DisableDomainActions()` 按结构名
   统一禁用本域按钮，因此不需要逐个声明绑定。

剩下真正"声明了但没人管"的按钮，才是待接线的入口。

用法：python tools/_audit_entry_buttons.py [FormName ...]
"""
import json
import pathlib
import sys

ROOT = pathlib.Path('.')
CONTRACT_DIR = ROOT / 'Docs/Development/UI-PrefabLayouts'
LOGIC_DIR = ROOT / 'Assets/Game/Scripts/AutoEra/UI'

BASE_HANDLED = {
    '_navButtons', '_backButton', '_closeButton', '_firstInteractable', '_pageRoots',
}

BASE_FILES = [
    LOGIC_DIR / 'AutoEraShellFormBase.cs',
    LOGIC_DIR / 'AutoEraUiFormBase.cs',
]
base_text = '\n'.join(p.read_text(encoding='utf-8') for p in BASE_FILES if p.exists())


def not_wired_forms():
    """从生成器源码里读出 NOT_WIRED 集合（它就是未就绪域的唯一名单）。"""
    src = (ROOT / 'tools/ui_contract_to_form_script.py').read_text(encoding='utf-8')
    start = src.find('NOT_WIRED')
    if start < 0:
        return set()
    brace = src.find('{', start)
    end = src.find('}', brace)
    body = src[brace + 1:end]
    return {tok.strip().strip('"\'') for tok in body.replace('\n', ',').split(',') if tok.strip()}


NOT_WIRED = not_wired_forms()


def walk(node, prefix='', in_item=False):
    path = prefix + '/' + node.get('name', '') if prefix else node.get('name', '')
    name = node.get('name', '')
    now_in_item = in_item or name.startswith('Item_')
    yield node, path, now_in_item
    for child in node.get('children', []) or []:
        yield from walk(child, path, now_in_item)


def main():
    names = set(sys.argv[1:])
    total = 0
    rows = []
    for path in sorted(CONTRACT_DIR.glob('*.contract.json')):
        contract = json.loads(path.read_text(encoding='utf-8'))
        form = contract['form']
        if names and form not in names:
            continue

        by_node = {b['node']: b['path'] for b in contract.get('bindings', []) or []}
        logic = LOGIC_DIR / f'{form}.cs'
        text = logic.read_text(encoding='utf-8') if logic.exists() else ''
        disabled_domain = form in NOT_WIRED

        entries, unwired = [], []
        for node, node_path, in_item in walk(contract['root']):
            if in_item or not node.get('name', '').startswith('Btn_'):
                continue
            kinds = {c.get('type') for c in node.get('components', []) or []}
            if 'Button' not in kinds:
                continue
            field = by_node.get(node_path)
            if field is None or field in BASE_HANDLED:
                continue
            entries.append((node['name'], field))
            # NOT_WIRED 界面的本域按钮由 DisableDomainActions 统一禁用；
            # 但安全出口（返回/关闭/取消）与页导航它刻意不碰，那些仍需各自处理。
            exempt = field in text or field in base_text
            if not exempt and disabled_domain:
                exempt = True
            if not exempt:
                unwired.append((node['name'], field))

        total += len(unwired)
        rows.append((form, len(entries), len(unwired), disabled_domain, unwired))

    for form, wired, unw, disabled, unwired in rows:
        tag = '  [未就绪域：统一禁用]' if disabled else ''
        print(f'===== {form} =====  入口按钮 {wired}，已接线 {wired - unw}，未接线 {unw}{tag}')
        for node_name, field in unwired:
            print(f'    - {node_name}  ({field})')

    print(f'\n合计未接线入口按钮：{total}')


if __name__ == '__main__':
    main()
