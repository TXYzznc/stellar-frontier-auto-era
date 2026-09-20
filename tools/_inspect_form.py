# -*- coding: utf-8 -*-
"""巡检单个 Form：内容页、业务按钮、绑定字段。

用法：python tools/_inspect_form.py <FormName> [<FormName> ...]
"""
import json
import pathlib
import sys

CONTRACT_DIR = pathlib.Path("Docs/Development/UI-PrefabLayouts")
UI_DIR = pathlib.Path("Assets/Game/Scripts/AutoEra/UI")


def walk(node, path=""):
    here = f"{path}/{node['name']}" if path else node["name"]
    yield here, node
    for child in node.get("children", []):
        yield from walk(child, here)


for form in sys.argv[1:]:
    data = json.loads((CONTRACT_DIR / f"{form}.contract.json").read_text(encoding="utf-8"))
    print(f"===== {form} =====")

    host = None
    for path, node in walk(data["root"]):
        if node["name"] == "Grp_PageHost":
            host = node
            break

    pages = [c["name"] for c in (host or {}).get("children", []) if c["name"].startswith("Panel_Page")]
    print(f"  页序（{len(pages)}）：" + "、".join(p[len("Panel_Page"):] for p in pages))

    print(f"  导航页索引：{data.get('navigationPageIndex')}")

    in_template = []
    buttons = []
    for path, node in walk(data["root"]):
        name = node["name"]
        if not name.startswith("Btn_"):
            continue
        if "Template/" in path:
            in_template.append(name)
        else:
            buttons.append(name)

    print(f"  业务按钮（{len(buttons)}）：" + "、".join(buttons))
    print(f"  列表行按钮（{len(in_template)}，由 Item 逻辑处理）：" + "、".join(sorted(set(in_template))))

    fields_file = UI_DIR / f"{form}.Fields.cs"
    if fields_file.exists():
        import re
        names = re.findall(r"private\s+[\w\[\]<>]+?\s+(_\w+);", fields_file.read_text(encoding="utf-8"))
        print(f"  已有绑定（{len(names)}）")
    print()
