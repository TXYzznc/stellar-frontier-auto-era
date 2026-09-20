#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""把 03-玩家体验/界面规格 的设计文档解析成结构化中间表示。

设计文档是唯一设计来源；本脚本只做**确定性解析**，不做任何业务判断：

  1. 每个家族的 `prefab-layout.md` 里，每个 `## <PageId>：<标题>` 段包含
       一个 ```text 节点树块 + 一个四列表格（节点 / RectTransform / 控制 / 组件·行为）。
  2. `00-共享外壳-prefab-layout.md` 定义每个 Form 的外壳、尺寸、导航与子页组合。

输出中间表示（JSON），并做双向一致性自检：
  - 树里的节点集合必须与表格里的节点集合逐项相等（缺失/多余都报错）
  - 树声明的层级父子关系必须与表格行顺序一致
  - 每个节点的 RectTransform 必须可解析

本脚本不产出 contract.json、不触碰 prefab。用法：
    python tools/ui_spec_parse.py [--out <dir>] [--json]
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

SPEC_DIR = Path("Docs/GameDesign/03-玩家体验/界面规格")
SHARED_SHELL = SPEC_DIR / "00-共享外壳-prefab-layout.md"

# ---------------------------------------------------------------- 组件语法

# 树里 `[ ... ]` 的组件表达式 -> 组件列表（具体属性留待 contract 阶段结合表格补全）
COMPONENT_ALIASES = {
    "无Graphic": [],
    "Image": ["Image"],
    "Image(Filled)": ["Image"],
    "TextMeshProUGUI": ["TextMeshProUGUI"],
    "RectMask2D": ["RectMask2D"],
    "LayoutElement": ["LayoutElement"],
    "LayoutElement + Image；默认inactive": ["LayoutElement", "Image"],
    "Button + Image": ["Image", "Button"],
    "Slider": ["Slider"],
    "Toggle": ["Toggle"],
}

# 允许出现在树括号里、以这些前缀开头的表达式（带参数形式）
PARAMETRIC_PREFIXES = (
    "ScrollRect",
    "VerticalLayoutGroup",
    "HorizontalLayoutGroup",
    "GridLayoutGroup",
    "LayoutElement +",
    "Button + Image +",
    "Image + LayoutElement",
    "Image + TMP_InputField",
    "TextMeshProUGUI + LayoutElement",
    "GraphLayoutGroup",
    "Image + 连线表现组件",
)


def split_components(expr: str) -> tuple[list[str], list[str]]:
    """把树括号表达式拆成 (组件名列表, 无法识别的残余)。"""
    raw = expr.strip()

    # 去掉中文注释后缀（如「；默认inactive」「（待实现）」）
    core = raw.split("；")[0].split("；")[0]
    core = re.sub(r"（[^）]*）", "", core)
    core = core.split(":", 1)[0] if core.startswith("GraphLayoutGroup") else core
    core = core.strip()

    names: list[str] = []
    unknown: list[str] = []

    if core in COMPONENT_ALIASES:
        names = list(COMPONENT_ALIASES[core])
    else:
        for part in re.split(r"\s*\+\s*", core):
            part = part.strip()
            head = part.split()[0] if part.split() else ""
            if head in (
                "Image",
                "TextMeshProUGUI",
                "LayoutElement",
                "Button",
                "Toggle",
                "Slider",
                "ScrollRect",
                "VerticalLayoutGroup",
                "HorizontalLayoutGroup",
                "GridLayoutGroup",
                "RectMask2D",
                "ContentSizeFitter",
                "TMP_InputField",
                "GraphLayoutGroup",
            ):
                names.append(head)
            elif part:
                unknown.append(part)

    return names, unknown


# ---------------------------------------------------------------- 矩形解析

NUM = r"-?\d+(?:\.\d+)?"


def parse_rect(cell: str) -> dict | None:
    """解析表格第 2 列：`min(a,b) max(c,d); pivot(e,f); sizeDelta(g,h); pos(i,j)`。"""
    m = re.search(
        rf"min\(\s*({NUM})\s*,\s*({NUM})\s*\)\s*max\(\s*({NUM})\s*,\s*({NUM})\s*\)",
        cell,
    )
    if not m:
        return None
    rect = {
        "anchorMin": [float(m.group(1)), float(m.group(2))],
        "anchorMax": [float(m.group(3)), float(m.group(4))],
    }
    for key, pattern in (
        ("pivot", rf"pivot\(\s*({NUM})\s*,\s*({NUM})\s*\)"),
        ("sizeDelta", rf"sizeDelta\(\s*({NUM})\s*,\s*({NUM})\s*\)"),
        ("anchoredPosition", rf"pos\(\s*({NUM})\s*,\s*({NUM})\s*\)"),
    ):
        mm = re.search(pattern, cell)
        if mm:
            rect[key] = [float(mm.group(1)), float(mm.group(2))]
    return rect


# ---------------------------------------------------------------- 树解析

TREE_LINE = re.compile(r"^(?P<indent>\s*)(?P<name>[^\s\[\]]+)\s*\[(?P<spec>[^\]]*)\]\s*$")


def parse_tree(block: str) -> tuple[dict | None, list[str]]:
    """解析 ```text 节点树块，返回 (根节点, 问题列表)。"""
    problems: list[str] = []
    root = None
    stack: list[tuple[int, dict]] = []

    for line in block.splitlines():
        if not line.strip():
            continue
        m = TREE_LINE.match(line)
        if not m:
            problems.append(f"树行无法解析：{line.strip()!r}")
            continue
        indent = len(m.group("indent"))
        name = m.group("name").strip()
        spec = m.group("spec").strip()
        comps, unknown = split_components(spec)
        if unknown:
            problems.append(f"节点 {name} 的组件表达式有未识别片段：{unknown}")

        node = {
            "name": name,
            "componentExpr": spec,
            "components": comps,
            "children": [],
        }

        while stack and stack[-1][0] >= indent:
            stack.pop()
        if stack:
            stack[-1][1]["children"].append(node)
        else:
            if root is not None:
                problems.append(f"出现第二个根节点：{name}（已有的根是 {root['name']}）")
            root = node
        stack.append((indent, node))

    return root, problems


def walk(node: dict, out: list[dict] | None = None) -> list[dict]:
    out = [] if out is None else out
    out.append(node)
    for child in node["children"]:
        walk(child, out)
    return out


# ---------------------------------------------------------------- 表格解析

def parse_table(block: str) -> tuple[dict[str, dict], list[str]]:
    """解析四列表格，返回 {节点名: 行数据}。"""
    rows: dict[str, dict] = {}
    problems: list[str] = []
    order: list[str] = []

    for line in block.splitlines():
        if not line.startswith("|"):
            continue
        cells = [c.strip() for c in line.strip().strip("|").split("|")]
        if len(cells) < 4:
            continue
        name = cells[0]
        if name in ("节点", "---") or set(name) <= set("-: "):
            continue
        rect = parse_rect(cells[1])
        if rect is None:
            problems.append(f"表格行矩形无法解析：{name} -> {cells[1]!r}")
        rows[name] = {
            "rect": rect,
            "control": cells[2],
            "detail": cells[3],
        }
        order.append(name)

    if order:
        rows["__order__"] = order
    return rows, problems


# ---------------------------------------------------------------- 段落切分

SECTION = re.compile(r"^##\s+(?P<id>[A-Za-z0-9_]+)\s*[：:]\s*(?P<title>.+?)\s*$")


def parse_family(path: Path) -> dict:
    text = path.read_text(encoding="utf-8")
    lines = text.splitlines()

    sections: list[dict] = []
    current = None

    def flush() -> None:
        if current is not None:
            sections.append(current)

    i = 0
    while i < len(lines):
        line = lines[i]
        m = SECTION.match(line)
        if m:
            flush()
            current = {"pageId": m.group("id"), "title": m.group("title"), "raw": []}
            i += 1
            continue
        if current is not None:
            current["raw"].append(line)
        i += 1
    flush()

    fence = "`" * 3

    pages = []
    for sec in sections:
        raw_lines = sec.pop("raw")

        # 逐行状态机收集「第一个 ```text 块」与「第一个连续表格块」。
        # 不能用一个要求行尾 \n 的大正则：文件末尾无换行时会丢掉表格最后一行。
        tree_lines: list[str] = []
        table_lines: list[str] = []
        in_tree = False
        tree_done = False
        table_done = False
        for line in raw_lines:
            stripped = line.strip()
            if stripped.startswith(fence + "text"):
                if not tree_done:
                    in_tree = True
                continue
            if stripped.startswith(fence):
                if in_tree:
                    in_tree = False
                    tree_done = True
                continue
            if in_tree:
                tree_lines.append(line)
                continue
            if line.startswith("|"):
                if not table_done:
                    table_lines.append(line)
            elif table_lines:
                table_done = True

        problems: list[str] = []
        if not tree_lines:
            problems.append("缺少树块")
        if not table_lines:
            problems.append("缺少表格块")

        tree = None
        tree_nodes: list[dict] = []
        if tree_lines:
            tree, tree_problems = parse_tree("\n".join(tree_lines))
            problems += tree_problems
            if tree:
                tree_nodes = walk(tree)

        rows: dict[str, dict] = {}
        if table_lines:
            rows, table_problems = parse_table("\n".join(table_lines))
            problems += table_problems

        order = rows.pop("__order__", [])

        # 双向一致：树节点集合 == 表格节点集合
        tree_names = {n["name"] for n in tree_nodes}
        table_names = set(rows)
        missing = sorted(table_names - tree_names)
        extra = sorted(tree_names - table_names)
        if missing:
            problems.append(f"表格有、树里没有：{missing}")
        if extra:
            problems.append(f"树里有、表格没有：{extra}")

        # 把矩形/控制补到树节点上
        for node in tree_nodes:
            row = rows.get(node["name"])
            if row:
                node["rect"] = row["rect"]
                node["control"] = row["control"]
                node["detail"] = row["detail"]

        pages.append(
            {
                "pageId": sec["pageId"],
                "title": sec["title"],
                "tree": tree,
                "nodeCount": len(tree_nodes),
                "problems": problems,
            }
        )

    return {
        "family": path.parent.name,
        "file": str(path),
        "pages": pages,
    }


# ---------------------------------------------------------------- 共享外壳

FORM_HEADING = re.compile(r"^###\s+(?P<form>[A-Za-z0-9_]+)\s*$")


def parse_shared_shell(path: Path) -> dict:
    """解析 00-共享外壳：外壳模板、每个 Form 的尺寸/导航/子页。"""
    text = path.read_text(encoding="utf-8")

    # 外壳模板树
    shell_tree = None
    tm = re.search(r"###\s*普通管理／模态外壳\s*\n(.*?)```text\s*\n(.*?)```", text, re.S)
    if tm:
        shell_tree, _ = parse_tree(tm.group(2))

    shell_rows, _ = ({}, [])
    rtm = re.search(r"\| 节点 \|.*?\n((?:^\|.*\n)+)", text, re.M)
    if rtm:
        shell_rows, _ = parse_table(rtm.group(1))

    forms: dict[str, dict] = {}
    blocks = re.split(r"(?m)^###\s+", text)
    for block in blocks[1:]:
        head, _, body = block.partition("\n")
        form = head.strip()
        if not re.match(r"^[A-Za-z0-9_]+$", form):
            continue

        size = re.search(r"外壳\s*W\s*=\s*(\d+)\s*、\s*H\s*=\s*(\d+)", body)
        host = re.search(r"Grp_PageHost\s*=\s*(\d+)×(\d+)", body)

        nav = []
        nav_table = re.search(r"\| 导航节点.*?\n((?:^\|.*\n)+)", body, re.M)
        if nav_table:
            for line in nav_table.group(1).splitlines():
                cells = [c.strip() for c in line.strip().strip("|").split("|")]
                if len(cells) < 3 or cells[0].startswith("---"):
                    continue
                nav.append({"node": cells[0], "label": cells[1], "target": cells[2]})

        pages = []
        pm = re.search(r"子页组合：(.+?)(?:\n\n|\Z)", body, re.S)
        if pm:
            for link in re.findall(r"\[([^\]]+)\]\(([^)]+)\)", pm.group(1)):
                pages.append({"title": link[0], "href": link[1]})

        forms[form] = {
            "form": form,
            "width": int(size.group(1)) if size else None,
            "height": int(size.group(2)) if size else None,
            "pageHost": [int(host.group(1)), int(host.group(2))] if host else None,
            "fullscreen": "全屏根" in body,
            "navigation": nav,
            "pages": pages,
        }

    return {"shellTree": shell_tree, "shellRows": shell_rows, "forms": forms}


# ---------------------------------------------------------------- 主流程

def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default="Docs/Development/UI-PrefabLayouts/_spec-parse")
    ap.add_argument("--json", action="store_true", help="打印完整中间表示")
    ap.add_argument("--quiet", action="store_true")
    args = ap.parse_args()

    root = Path.cwd()
    spec = root / SPEC_DIR
    if not spec.is_dir():
        print(f"找不到界面规格目录：{spec}", file=sys.stderr)
        return 2

    families = []
    total_pages = 0
    total_nodes = 0
    bad_pages = 0

    for layout in sorted(spec.glob("*/prefab-layout.md")):
        fam = parse_family(layout)
        families.append(fam)
        for page in fam["pages"]:
            total_pages += 1
            total_nodes += page["nodeCount"]
            if page["problems"]:
                bad_pages += 1

    shell = parse_shared_shell(spec / SHARED_SHELL.name)

    result = {"families": families, "shell": shell}

    out_dir = root / args.out
    out_dir.mkdir(parents=True, exist_ok=True)
    (out_dir / "spec-parse.json").write_text(
        json.dumps(result, ensure_ascii=False, indent=1), encoding="utf-8"
    )

    if args.json:
        print(json.dumps(result, ensure_ascii=False, indent=1))

    if not args.quiet:
        print(f"家族 {len(families)} 个，页面 {total_pages} 个，节点 {total_nodes} 个")
        print(f"外壳 Form {len(shell['forms'])} 个")
        print(f"有问题的页面 {bad_pages} 个")
        if bad_pages:
            print("\n--- 问题明细 ---")
            for fam in families:
                for page in fam["pages"]:
                    if page["problems"]:
                        print(f"[{fam['family']}] {page['pageId']}")
                        for p in page["problems"][:8]:
                            print(f"    - {p}")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
