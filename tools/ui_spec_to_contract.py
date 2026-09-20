#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""界面规格 → `<Form>.contract.json`（schema v3）。

链路：prefab-layout.md（设计唯一来源）
      → 本脚本（确定性转换）
      → <Form>.contract.json（契约，单一事实来源）
      → AutoEraUiPrefabGenerator（生成预制体）

设计文档的每个页面只描述「内容子树」（Panel_Page*）；Form 的根、外壳、导航与
Grp_PageHost 由 00-共享外壳-prefab-layout.md 唯一定义。本脚本把两者拼接：

    <FormName>
      Bg_InputBlocker
      Panel_Frame
        Deco_Frame
        Txt_FormTitle
        Grp_Navigation        ← 该 Form 的导航按钮
        Grp_PageHost          ← 该 Form 的各页面内容子树
        Btn_FormBack / Btn_FormClose
      （FieldHudForm / WorldPlacementForm 为全屏根，无 Panel_Frame）

用法：
    python tools/ui_spec_to_contract.py --family 01 --out Docs/Development/UI-PrefabLayouts
    python tools/ui_spec_to_contract.py --form MainMenuForm
    python tools/ui_spec_to_contract.py --all --dry-run
"""

from __future__ import annotations

import argparse
import json
import re
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))

import ui_spec_parse as P  # noqa: E402

# ---------------------------------------------------------------- 常量

# 结构期中性 token（00-通用合同「布局和资源」）。原型占位，不是正式美术风格。
TOKENS = {
    "Surface": (0x20, 0x24, 0x2A),
    "Inset": (0x16, 0x1A, 0x1F),
    "Text": (0xF0, 0xF2, 0xF4),
    "Muted": (0xAD, 0xB4, 0xBE),
    "Action": (0x52, 0x7F, 0xA3),
    "Focus": (0xB6, 0xD9, 0xF2),
    "Danger": (0xA4, 0x3C, 0x43),
    "Warning": (0xBD, 0x98, 0x54),
    "Success": (0x56, 0x8C, 0x73),
    "Disabled": (0x62, 0x68, 0x70),
}

FONT_PATH = "Assets/Game/Fonts/UI/SIMHEI SDF.asset"

# 字号：标题28、区标题24、正文22、辅助20、按钮22（00-通用合同）
FS_TITLE, FS_SECTION, FS_BODY, FS_AUX, FS_BUTTON = 28, 24, 22, 20, 22

PREFAB_ROOT = "Assets/Game/Prefabs/UI"
SCRIPT_ROOT = "Assets/Game/Scripts/AutoEra/UI"

# 家族 → 预制体子目录
FAMILY_DIR = {
    "01": "Startup",
    "02": "System",
    "03": "Hud",
}
DEFAULT_DIR = "Operations"


def rgb(name: str, alpha: float = 1.0):
    r, g, b = TOKENS[name]
    return [round(r / 255.0, 6), round(g / 255.0, 6), round(b / 255.0, 6), alpha]


def c(r, g, b, a=1.0):
    return [round(r / 255.0, 6), round(g / 255.0, 6), round(b / 255.0, 6), a]


# ---------------------------------------------------------------- 组件语法

def parse_component_expr(expr: str) -> tuple[list[dict], bool, list[str]]:
    """把树括号里的组件表达式转成契约组件列表。

    返回 (组件列表, 是否默认inactive, 未识别片段)。
    """
    raw = expr.strip()
    inactive = "默认inactive" in raw

    # 去掉中文注释与「待实现」标注
    core = re.split(r"[；（(]", raw, 1)[0].strip() if raw.startswith("GraphLayoutGroup") else raw
    core = core.split("；")[0].strip()
    core = re.sub(r"（[^）]*）", "", core).strip()

    comps: list[dict] = []
    unknown: list[str] = []

    def add(t, **kw):
        d = {"type": t}
        d.update(kw)
        comps.append(d)

    for part in re.split(r"\s*\+\s*", core):
        part = part.strip()
        if not part:
            continue
        head = part.split()[0]

        if head == "无Graphic":
            continue
        if part == "Image(Filled)":
            add("Image", fillMethod=0, fillAmount=1)
            continue
        if head in ("Image", "TextMeshProUGUI", "RectMask2D", "Toggle", "Slider", "TMP_InputField"):
            add(head)
            continue
        if head == "Button":
            add("Button", transition=1, interactable=True)
            continue
        if head == "ScrollRect":
            add(
                "ScrollRect",
                horizontal=("horizontal=true" in part),
                vertical=("vertical=true" in part),
                movementType=2,
            )
            continue
        if head in ("VerticalLayoutGroup", "HorizontalLayoutGroup"):
            kw = {}
            m = re.search(r"spacing=(\d+(?:\.\d+)?)", part)
            if m:
                kw["spacing"] = float(m.group(1))
            m = re.search(r"padding=(\d+)", part)
            if m:
                v = int(m.group(1))
                kw["padding"] = [v, v, v, v]
            if "childControlWidth/Height=true" in part:
                kw["ChildControlWidth"] = True
                kw["ChildControlHeight"] = True
            kw["ChildForceExpandWidth"] = True
            kw["ChildForceExpandHeight"] = False
            add(head, **kw)
            continue
        if head == "GridLayoutGroup":
            kw = {"constraint": 1, "ChildForceExpandWidth": False, "ChildForceExpandHeight": False}
            m = re.search(r"fixedColumns=(\d+)", part)
            if m:
                kw["constraintCount"] = int(m.group(1))
            m = re.search(r"cell=\(([\d.]+),([\d.]+)\)", part)
            if m:
                kw["cellSize"] = [float(m.group(1)), float(m.group(2))]
            m = re.search(r"spacing=\(([\d.]+),([\d.]+)\)", part)
            if m:
                kw["spacing"] = [float(m.group(1)), float(m.group(2))]
            add("GridLayoutGroup", **kw)
            continue
        if head == "ContentSizeFitter":
            kw = {}
            if "vertical=Preferred" in part:
                kw["verticalFit"] = 2
            if "horizontal=Preferred" in part:
                kw["horizontalFit"] = 2
            elif "horizontal=Unconstrained" in part:
                kw["horizontalFit"] = 0
            if not kw:
                kw["verticalFit"] = 2
            add("ContentSizeFitter", **kw)
            continue
        if head == "LayoutElement":
            kw = {}
            m = re.search(r"preferredHeight=(\d+)", part)
            if m:
                kw["preferredHeight"] = float(m.group(1))
            m = re.search(r"minHeight=(\d+)", part)
            if m:
                kw["minHeight"] = float(m.group(1))
            m = re.search(r"preferredWidth=(\d+)", part)
            if m:
                kw["preferredWidth"] = float(m.group(1))
            m = re.search(r"preferred=\(([\d.]+),([\d.]+)\)", part)
            if m:
                kw["preferredWidth"] = float(m.group(1))
                kw["preferredHeight"] = float(m.group(2))
            add("LayoutElement", **kw)
            continue
        if head == "GraphLayoutGroup":
            # 规格声明它是 LayoutGroup 的子类但「待实现」，且明确「不与手写 Transform 争夺」。
            # 原型阶段挂 AutoEraGraphLayoutGroup 占位（不做任何排列），既满足 Content_ 的
            # 结构契约，又把画布坐标留给图快照与未来的完整布局算法。
            add("AutoEraGraphLayoutGroup")
            continue
        if part in ("连线表现组件",):
            unknown.append("连线表现组件（待实现，已略过）")
            continue

        unknown.append(part)

    # Graphic 必须排在 Button 之前：Button 的 targetGraphic 指向同节点的 Image，
    # 若 Button 先添加则该引用落空（生成器按顺序 AddComponent）。
    comps.sort(key=lambda c: 0 if c["type"] in ("Image", "RawImage") else 1)

    return comps, inactive, unknown


# ---------------------------------------------------------------- 画节点

def is_interactive_name(name: str) -> bool:
    return name.startswith(("Btn_", "Tgl_", "Sld_", "Bg_", "Overlay_"))


def image_style(name: str) -> dict:
    """按前缀给出 Image 的占位外观（颜色 / 类型 / raycast）。结构与布局不受此影响。"""
    interactive = name.startswith(("Btn_", "Bg_", "Overlay_"))
    if name.startswith("Bg_"):
        color = c(0, 0, 0, 0.65)
    elif name.startswith("Overlay_"):
        color = c(0, 0, 0, 0.65)
    elif name.startswith("Btn_"):
        color = rgb("Action")
    elif name.startswith("Panel_"):
        color = rgb("Surface")
    elif name.startswith("Deco_"):
        color = c(0.5, 0.5, 0.5, 0.25)
    elif name.startswith(("Img_", "Icon_")):
        color = c(1, 1, 1, 1)
    else:
        color = rgb("Surface")

    if name.startswith(("Img_", "Icon_")):
        image_type, preserve = 0, True
    else:
        image_type, preserve = 1, False

    return {
        "type": "Image",
        "color": color,
        "imageType": image_type,
        "raycastTarget": interactive,
        "preserveAspect": preserve,
    }


def text_style(name: str, text: str, muted: bool = False) -> dict:
    if name.endswith("Title"):
        size = FS_TITLE
    elif name.endswith("Heading"):
        size = FS_SECTION
    elif name.endswith("Body"):
        size = FS_BODY
    elif name.endswith(("Label",)) and not name.endswith(("RowLabel", "StateLabel")):
        size = FS_BUTTON
    elif name.endswith(("Value", "Message", "Hint", "Note", "Status")):
        size = FS_AUX
    else:
        size = FS_BODY

    return {
        "type": "TextMeshProUGUI",
        "text": text,
        "fontSize": float(size),
        "color": rgb("Muted") if muted else rgb("Text"),
        "raycastTarget": False,
        "wordWrap": True,
        "fontPath": FONT_PATH,
    }


TEXT_DEFAULT = "—"


def detail_text(detail: str) -> str:
    """从表格第 4 列抽出文本样例（第一个「；」之后），没有则用占位符。"""
    if not detail:
        return TEXT_DEFAULT
    parts = detail.split("；")
    if len(parts) >= 2:
        for seg in parts[1:]:
            seg = seg.strip()
            if seg and "默认inactive" not in seg and "状态背景" not in seg:
                return seg
        return TEXT_DEFAULT
    return TEXT_DEFAULT


def build_page_node(node: dict, *, is_root: bool = False) -> dict:
    """页面树节点 → 契约节点。rect / control / detail 来自同页表格。"""
    name = node["name"]
    comps, inactive, unknown = parse_component_expr(node["componentExpr"])

    rect = node.get("rect") or {}
    control = node.get("control", "absolute")
    detail = node.get("detail", "")

    # 组件补全：Image / TMP 的外观占位
    final: list[dict] = []
    for comp in comps:
        if comp["type"] == "Image":
            merged = dict(image_style(name))
            merged.update({k: v for k, v in comp.items() if k != "type"})
            final.append(merged)
        elif comp["type"] == "TextMeshProUGUI":
            merged = text_style(name, detail_text(detail))
            merged.update({k: v for k, v in comp.items() if k != "type"})
            final.append(merged)
        else:
            final.append(comp)

    out: dict = {"name": name, "components": final}

    # 控制方式必须进契约（正式字段，不能像 _control 那样被清理掉）：被 LayoutGroup 驱动的
    # 子节点，其 anchor/pivot/位置/尺寸由布局组计算（规格：最终位置/尺寸由组驱动），
    # 手写坐标对它没有意义，门 1 也据此跳过 rect 比对。
    if control == "group":
        out["controlledBy"] = "group"

    if rect:
        out["anchorMin"] = rect["anchorMin"]
        out["anchorMax"] = rect["anchorMax"]
        out["pivot"] = rect.get("pivot", [0.5, 0.5])
        out["sizeDelta"] = rect.get("sizeDelta", [0, 0])
        out["anchoredPosition"] = rect.get("anchoredPosition", [0, 0])

    if inactive or name.endswith("State"):
        out["active"] = False

    children = [build_page_node(ch) for ch in node["children"]]
    if children:
        out["children"] = children

    out["_control"] = control
    if unknown:
        out["_unresolved"] = unknown
    return out


def strip_meta(node: dict) -> dict:
    """去掉解析期的辅助字段，产出干净契约。"""
    node.pop("_control", None)
    node.pop("_unresolved", None)
    for ch in node.get("children", []):
        strip_meta(ch)
    return node


# ---------------------------------------------------------------- 外壳

SHELL_TREE_FALLBACK = [
    ("Bg_InputBlocker", 0),
    ("Panel_Frame", 0),
    ("Deco_Frame", 1),
    ("Txt_FormTitle", 1),
    ("Grp_Navigation", 1),
    ("Grp_PageHost", 1),
    ("Btn_FormBack", 1),
    ("Btn_FormClose", 1),
]


def eval_shell_number(token: str, width: int, height: int) -> float:
    # 先剥掉「初始化」「，中枢pos=」之类的说明字，只留可求值的表达式字符。
    t = re.sub(r"[^0-9WH+\-*/.()\s]", "", token.replace("−", "-")).strip()
    if not t:
        return 0.0
    try:
        return float(t)
    except ValueError:
        pass
    if re.fullmatch(r"[WH\d\s+\-*/.()]+", t):
        try:
            return float(eval(t, {"__builtins__": {}}, {"W": width, "H": height}))
        except Exception:
            return 0.0
    return 0.0


def parse_shell_rect(cell: str, width: int, height: int) -> dict | None:
    """解析外壳表格。

    外壳表与家族表格式不同：锚点段写成 `(a,b)→(c,d)`，其余三段是**位置式**的
    `... / pivot / sizeDelta / pos`，没有字段标签，所以按出现顺序取括号对。
    """
    m = re.search(
        r"\(\s*([^(),]+?)\s*,\s*([^(),]+?)\s*\)\s*→\s*\(\s*([^(),]+?)\s*,\s*([^(),]+?)\s*\)",
        cell,
    )
    if not m:
        return None

    rect = {
        "anchorMin": [eval_shell_number(m.group(1), width, height),
                      eval_shell_number(m.group(2), width, height)],
        "anchorMax": [eval_shell_number(m.group(3), width, height),
                      eval_shell_number(m.group(4), width, height)],
    }

    pairs = re.findall(r"\(\s*([^()]*?)\s*,\s*([^()]*?)\s*\)", cell[m.end():])
    for key, idx in (("pivot", 0), ("sizeDelta", 1), ("anchoredPosition", 2)):
        if len(pairs) > idx:
            rect[key] = [eval_shell_number(pairs[idx][0], width, height),
                         eval_shell_number(pairs[idx][1], width, height)]

    # 「中枢pos=(0,-12)」是**中枢外壳专属**的例外，不能套到别的 Form 上；
    # 先记为独立字段，由 build_form_contract 在确认是中枢时再覆盖。
    mid = re.search(r"中枢pos=\s*\(\s*([^(),]+?)\s*,\s*([^(),]+?)\s*\)", cell)
    if mid:
        rect["_hubAnchoredPosition"] = [eval_shell_number(mid.group(1), width, height),
                                        eval_shell_number(mid.group(2), width, height)]
    return rect


def parse_shell_layout(path: Path) -> dict[str, dict]:
    """解析 00-共享外壳 的外壳节点表：{节点名: {rect, componentText}}。"""
    text = path.read_text(encoding="utf-8")
    rows: dict[str, dict] = {}
    collecting = False
    for line in text.splitlines():
        if line.startswith("| 节点 |"):
            collecting = True
            continue
        if not collecting:
            continue
        if not line.startswith("|"):
            if rows:
                break
            continue
        cells = [x.strip() for x in line.strip().strip("|").split("|")]
        if len(cells) < 3 or set(cells[0]) <= set("-: "):
            continue
        rows[cells[0]] = {"componentText": cells[1], "rectText": cells[2]}
    return rows


# 当前正在构建的 Form；仅用于判定「中枢pos=」这类**专属**外壳例外。
_ACTIVE_FORM = ""


def shell_rect(rows: dict, name: str, width: int, height: int, fallback: dict) -> dict:
    row = rows.get(name)
    if row:
        rect = parse_shell_rect(row["rectText"], width, height)
        if rect:
            hub_pos = rect.pop("_hubAnchoredPosition", None)
            if hub_pos is not None and _ACTIVE_FORM == "BaseCommandHubForm":
                rect["anchoredPosition"] = hub_pos
            return rect
    return dict(fallback)


# ---------------------------------------------------------------- Form 契约

def build_field_close() -> dict:
    """现场侧栏关闭按钮（规格 00-共享外壳 FieldHudForm 段）。

    挂在 Grp_PageHost 下、默认隐藏：关闭按钮只在侧栏开启时显示，且位于导航末位。
    """
    return {
        "name": "Btn_FieldClose",
        "components": [
            image_style("Btn_FieldClose"),
            {"type": "Button", "transition": 1, "interactable": True},
        ],
        "anchorMin": [1.0, 1.0],
        "anchorMax": [1.0, 1.0],
        "pivot": [1.0, 1.0],
        "sizeDelta": [64.0, 48.0],
        "anchoredPosition": [-40.0, -128.0],
        "active": False,
        "children": [
            {
                "name": "Txt_FieldCloseLabel",
                "components": [text_style("Txt_FieldCloseLabel", "关闭")],
                "anchorMin": [0.0, 0.0],
                "anchorMax": [1.0, 1.0],
                "pivot": [0.5, 0.5],
                "sizeDelta": [-8.0, -8.0],
                "anchoredPosition": [0.0, 0.0],
            }
        ],
    }


def page_body(node: dict) -> dict:
    """页面根在不同宿主下的差异（保持规格声明的 RectTransform）。"""
    return node


def build_form_contract(
    form_name: str,
    form_def: dict,
    pages: list[dict],
    shell_rows: dict,
    family_key: str,
) -> dict:
    width = form_def.get("width") or 1600
    height = form_def.get("height") or 900
    host = form_def.get("pageHost") or [width - 112, height - 170]
    fullscreen = bool(form_def.get("fullscreen"))

    global _ACTIVE_FORM
    _ACTIVE_FORM = form_name

    root: dict = {
        "name": form_name,
        "components": [
            {"type": form_name},
            {"type": "CanvasGroup", "alpha": 1.0, "interactable": True, "blocksRaycasts": True},
        ],
        "anchorMin": [0.0, 0.0],
        "anchorMax": [1.0, 1.0],
        "pivot": [0.5, 0.5],
        "sizeDelta": [0.0, 0.0],
        "anchoredPosition": [0.0, 0.0],
    }
    children: list[dict] = []

    page_host_children: list[dict] = []

    if fullscreen:
        # 全屏根（FieldHudForm / WorldPlacementForm）：根 → Grp_PageHost 全 Stretch。
        # FieldHudForm 的 HUD 五模块**同时常驻**，只有现场内容页互斥；现场侧栏关闭
        # 按钮也挂在 Grp_PageHost 下，默认隐藏（规格：关闭按钮只在侧栏开启时显示）。
        for page in pages:
            page["active"] = str(page.get("_pageId", "")).startswith("Hud")

        page_host_children = list(pages)
        if form_name == "FieldHudForm":
            page_host_children.append(build_field_close())

        children.append(
            {
                "name": "Grp_PageHost",
                "components": [],
                "anchorMin": [0.0, 0.0],
                "anchorMax": [1.0, 1.0],
                "pivot": [0.5, 0.5],
                "sizeDelta": [0.0, 0.0],
                "anchoredPosition": [0.0, 0.0],
                "children": page_host_children,
            }
        )
        root["children"] = children
        return assemble(root, form_name, form_def, family_key)

    # ---- 普通管理／模态外壳
    children.append(
        {
            "name": "Bg_InputBlocker",
            "components": [image_style("Bg_InputBlocker")],
            **shell_rect(shell_rows, "Bg_InputBlocker", width, height,
                         {"anchorMin": [0, 0], "anchorMax": [1, 1], "pivot": [0.5, 0.5],
                          "sizeDelta": [0, 0], "anchoredPosition": [0, 0]}),
        }
    )

    frame_children: list[dict] = []

    frame_children.append(
        {
            "name": "Deco_Frame",
            "components": [image_style("Deco_Frame")],
            **shell_rect(shell_rows, "Deco_Frame", width, height,
                         {"anchorMin": [0, 0], "anchorMax": [1, 1], "pivot": [0.5, 0.5],
                          "sizeDelta": [0, 0], "anchoredPosition": [0, 0]}),
        }
    )

    title_text = pages[0]["_title"] if pages else TEXT_DEFAULT
    frame_children.append(
        {
            "name": "Txt_FormTitle",
            "components": [text_style("Txt_FormTitle", title_text)],
            **shell_rect(shell_rows, "Txt_FormTitle", width, height,
                         {"anchorMin": [0, 1], "anchorMax": [0, 1], "pivot": [0, 1],
                          "sizeDelta": [width - 300, 40], "anchoredPosition": [56, -20]}),
        }
    )

    # 导航
    nav = form_def.get("navigation") or []
    nav_node: dict = {
        "name": "Grp_Navigation",
        "components": [
            {
                "type": "HorizontalLayoutGroup",
                "spacing": 8.0,
                "padding": [0, 0, 0, 0],
                "ChildForceExpandWidth": False,
                "ChildForceExpandHeight": False,
            }
        ],
        **shell_rect(shell_rows, "Grp_Navigation", width, height,
                     {"anchorMin": [0, 1], "anchorMax": [1, 1], "pivot": [0.5, 1],
                      "sizeDelta": [-200, 52], "anchoredPosition": [-44, -68]}),
    }
    nav_children: list[dict] = []

    # 00-共享外壳 给导航按钮的命名可能与家族页面内的节点撞车（实测 AlgorithmEditorForm：
    # 顶栏「编辑」与页内「修改节点和参数」都叫 Btn_AlgorithmEditorEdit）。同一 Form 内节点名
    # 必须唯一；冲突时给**外壳导航**插入 Nav 中缀——家族页面规格描述更细，保持原样。
    page_names: set[str] = set()
    for page in pages:
        collect_names(page, page_names)
    semantic = form_name[:-4] if form_name.endswith("Form") else form_name

    for item in nav:
        btn_name = item["node"]
        if btn_name in page_names:
            btn_name = btn_name.replace("Btn_" + semantic, "Btn_" + semantic + "Nav", 1)
        label_name = btn_name.replace("Btn_", "Txt_") + "Label"
        nav_children.append(
            {
                "name": btn_name,
                # 规格 00-共享外壳：分页按钮「由组布局」——位置与尺寸由 Grp_Navigation 计算。
                "controlledBy": "group",
                "components": [
                    image_style(btn_name),
                    {"type": "Button", "transition": 1, "interactable": True},
                    {"type": "LayoutElement", "preferredWidth": 136.0, "preferredHeight": 48.0},
                ],
                "anchorMin": [0, 1], "anchorMax": [0, 1], "pivot": [0, 1],
                "sizeDelta": [136.0, 48.0], "anchoredPosition": [0.0, 0.0],
                "children": [
                    {
                        "name": label_name,
                        "components": [text_style(label_name, item["label"])],
                        "anchorMin": [0, 0], "anchorMax": [1, 1], "pivot": [0.5, 0.5],
                        "sizeDelta": [-16.0, -8.0], "anchoredPosition": [0.0, 0.0],
                    }
                ],
            }
        )
    nav_node["children"] = nav_children
    if not nav_children:
        nav_node["active"] = False
    frame_children.append(nav_node)

    # 页面宿主
    page_host_children = pages
    frame_children.append(
        {
            "name": "Grp_PageHost",
            "components": [],
            **shell_rect(shell_rows, "Grp_PageHost", width, height,
                         {"anchorMin": [0, 1], "anchorMax": [0, 1], "pivot": [0, 1],
                          "sizeDelta": [host[0], host[1]], "anchoredPosition": [56, -140]}),
            "children": page_host_children,
        }
    )

    for name, label, text in (("Btn_FormBack", "Txt_FormBackLabel", "返回"),
                              ("Btn_FormClose", "Txt_FormCloseLabel", "关闭")):
        frame_children.append(
            {
                "name": name,
                "components": [image_style(name), {"type": "Button", "transition": 1, "interactable": True}],
                **shell_rect(shell_rows, name, width, height,
                             {"anchorMin": [1, 1], "anchorMax": [1, 1], "pivot": [1, 1],
                              "sizeDelta": [88, 48], "anchoredPosition": [-112, -20]}),
                "children": [
                    {
                        "name": label,
                        "components": [text_style(label, text)],
                        "anchorMin": [0, 0], "anchorMax": [1, 1], "pivot": [0.5, 0.5],
                        "sizeDelta": [-8.0, -8.0], "anchoredPosition": [0.0, 0.0],
                    }
                ],
            }
        )

    frame = {
        "name": "Panel_Frame",
        "components": [image_style("Panel_Frame")],
        **shell_rect(shell_rows, "Panel_Frame", width, height,
                     {"anchorMin": [0.5, 0.5], "anchorMax": [0.5, 0.5], "pivot": [0.5, 0.5],
                      "sizeDelta": [width, height], "anchoredPosition": [0, 0]}),
        "children": frame_children,
    }
    children.append(frame)
    root["children"] = children

    return assemble(root, form_name, form_def, family_key)


def find_path(node: dict, target: str, prefix: str = "") -> str | None:
    """在契约树里按节点名找出**根相对**路径；重名视为错误由调用方发现。"""
    path = f"{prefix}/{node['name']}" if prefix else node["name"]
    if node["name"] == target:
        return path
    for ch in node.get("children", []):
        hit = find_path(ch, target, path)
        if hit:
            return hit
    return None


# 每个 Form 除外壳公共字段外的额外绑定：字段名 → (节点名, 组件种类)
#
# 业务按钮按需在此声明，不放进 auto_page_bindings 自动推导——那会为确认弹窗那类
# 多页 Form 造出上百个用不到的字段。这里只声明「导航/接入必需」的按钮。
EXTRA_BINDINGS: dict[str, list[tuple[str, str, str]]] = {
    "MainMenuForm": [
        ("_enterButton", "Btn_MainMenuContinue", "Button"),
        ("_newButton", "Btn_MainMenuNew", "Button"),
        ("_slotsButton", "Btn_MainMenuSlots", "Button"),
        ("_settingsButton", "Btn_MainMenuSettings", "Button"),
        ("_exitButton", "Btn_MainMenuExit", "Button"),
        ("_status", "Txt_MainMenuIdentityBody", "TextMeshProUGUI"),
    ],
    "SaveSlotsForm": [
        ("_slotListContent", "Content_SaveSlotsSlots", "RectTransform"),
        ("_slotRowTemplate", "Item_SaveSlotsSlotsTemplate", "GameObject"),
        ("_selectButton", "Btn_SaveSlotsSelect", "Button"),
        ("_detailsButton", "Btn_SaveSlotsDetails", "Button"),
        ("_createButton", "Btn_SaveSlotsCreate", "Button"),
        ("_continueButton", "Btn_SaveSlotsContinue", "Button"),
        ("_saveDetailContinueButton", "Btn_SaveDetailContinue", "Button"),
        ("_saveDetailDeleteButton", "Btn_SaveDetailDelete", "Button"),
        ("_saveDetailRecoverButton", "Btn_SaveDetailRecover", "Button"),
        ("_newProgressCreateButton", "Btn_NewProgressCreate", "Button"),
        ("_newProgressBackButton", "Btn_NewProgressBack", "Button"),
    ],
    "SaveRecoveryForm": [
        ("_restoreButton", "Btn_RecoveryRestore", "Button"),
        ("_returnButton", "Btn_RecoveryReturn", "Button"),
    ],
    # BaseCommandHubForm 不再手写页面绑定：它的页面引用由 auto_page_bindings 按命名规则
    # 自动推导（阶段 1 的样板页已用实物验证过这套规则的可用性）。
    "FieldHudForm": [
        ("_fieldCloseButton", "Btn_FieldClose", "Button"),
        ("_statusPanel", "Panel_PageHudStatus", "GameObject"),
        ("_trackerPanel", "Panel_PageHudTracker", "GameObject"),
        ("_alertsPanel", "Panel_PageHudAlerts", "GameObject"),
        ("_navigationPanel", "Panel_PageHudNavigation", "GameObject"),
        ("_savePanel", "Panel_PageHudSave", "GameObject"),
        ("_statusSummary", "Txt_HudStatusSystems", "TextMeshProUGUI"),
        # 主导航：世界链的入口（HUD → 各集中界面）。来源见 00-页面关系与复用 的主要入口表。
        ("_hudHubButton", "Btn_HudNavigationHub", "Button"),
        ("_hudTasksButton", "Btn_HudNavigationTasks", "Button"),
        ("_hudMachinesButton", "Btn_HudNavigationMachines", "Button"),
        ("_hudBuildButton", "Btn_HudNavigationBuild", "Button"),
        ("_hudShopButton", "Btn_HudNavigationShop", "Button"),
        ("_hudComponentsButton", "Btn_HudNavigationComponents", "Button"),
        ("_hudSystemButton", "Btn_HudNavigationSystem", "Button"),
        # 表内其余入口（来源：00-页面关系与复用 的主要入口表）。
        # 「现场／HUD → 记录阅读」：机器诊断与四个资源观察页的记录入口都指向同一个
        # RecordReaderForm 的机器历史页——事件域已经活着，这些入口点开就有真实数据。
        ("_machineOverviewDiagnosticButton", "Btn_MachineOverviewDiagnostic", "Button"),
        ("_machineDiagnosticsTaskRecordButton", "Btn_MachineDiagnosticsTaskRecord", "Button"),
        ("_machineDiagnosticsRunRecordButton", "Btn_MachineDiagnosticsRunRecord", "Button"),
        ("_farmRecordButton", "Btn_FarmRecord", "Button"),
        ("_forestRecordButton", "Btn_ForestRecord", "Button"),
        ("_mineralRecordButton", "Btn_MineralRecord", "Button"),
        ("_waterRecordButton", "Btn_WaterRecord", "Button"),
        ("_warehouseBuildingRecordsButton", "Btn_WarehouseBuildingRecords", "Button"),
        # 「现场机器 → 算法编辑」：算法域未接运行时，编辑器与模板库会整页切 Disabled 并写明原因。
        # 仍然接线，是因为「未就绪」是设计里可展示的正常状态，而死按钮不是。
        ("_machineOverviewAlgorithmButton", "Btn_MachineOverviewAlgorithm", "Button"),
        ("_machineAlgorithmEditButton", "Btn_MachineAlgorithmEdit", "Button"),
        ("_machineAlgorithmTemplateButton", "Btn_MachineAlgorithmTemplate", "Button"),
        # 「作物知识入口放农田详情」：四个资源观察页的知识入口。
        ("_farmKnowledgeButton", "Btn_FarmKnowledge", "Button"),
        ("_forestKnowledgeButton", "Btn_ForestKnowledge", "Button"),
        ("_mineralKnowledgeButton", "Btn_MineralKnowledge", "Button"),
        ("_waterKnowledgeButton", "Btn_WaterKnowledge", "Button"),
        # 「警报→活跃警报列表」「主线追踪→任务详情」。
        ("_hudAlertsOpenButton", "Btn_HudAlertsOpen", "Button"),
        ("_hudTrackerTaskButton", "Btn_HudTrackerTask", "Button"),
    ],
    "SystemMenuForm": [
        ("_resumeButton", "Btn_SystemMenuResume", "Button"),
        ("_settingsButton", "Btn_SystemMenuSettings", "Button"),
        ("_helpButton", "Btn_SystemMenuHelp", "Button"),
        ("_returnToMenuButton", "Btn_SystemMenuReturn", "Button"),
        ("_quitButton", "Btn_SystemMenuQuit", "Button"),
    ],
    "ExitFlowForm": [
        ("_retryButton", "Btn_ExitFlowRetry", "Button"),
        ("_resumeButton", "Btn_ExitFlowResume", "Button"),
        ("_forceButton", "Btn_ExitFlowForce", "Button"),
    ],
    # 机器库：库中机器与已部署机器两页，加整备页。列表行按钮由 Item 逻辑处理，不在此声明。
    "MachineLibraryForm": [
        ("_preparationRenameButton", "Btn_MachinePreparationRename", "Button"),
        ("_preparationInstallButton", "Btn_MachinePreparationInstall", "Button"),
        ("_preparationUnloadButton", "Btn_MachinePreparationUnload", "Button"),
        ("_preparationUpgradeButton", "Btn_MachinePreparationUpgrade", "Button"),
        ("_preparationSellButton", "Btn_MachinePreparationSell", "Button"),
        ("_preparationDeployButton", "Btn_MachinePreparationDeploy", "Button"),
        ("_undeployedPrepareButton", "Btn_UndeployedMachinesPrepare", "Button"),
        ("_undeployedRenameButton", "Btn_UndeployedMachinesRename", "Button"),
        ("_undeployedDeployButton", "Btn_UndeployedMachinesDeploy", "Button"),
        ("_undeployedSellButton", "Btn_UndeployedMachinesSell", "Button"),
        ("_undeployedViewDeployedButton", "Btn_UndeployedMachinesDeployed", "Button"),
        ("_deployedLocateButton", "Btn_DeployedMachinesLocate", "Button"),
        ("_deployedHubButton", "Btn_DeployedMachinesHub", "Button"),
        ("_deployedRenameButton", "Btn_DeployedMachinesRename", "Button"),
        ("_deployedReturnButton", "Btn_DeployedMachinesUndeployed", "Button"),
    ],
    # 世界对象选择器：选择与确认是它唯一的真实动作，因此这四个按钮必须绑进契约。
    "WorldObjectPickerForm": [
        ("_selectButton", "Btn_WorldObjectPickerSelect", "Button"),
        ("_worldButton", "Btn_WorldObjectPickerWorld", "Button"),
        ("_confirmButton", "Btn_WorldObjectPickerConfirm", "Button"),
        ("_cancelButton", "Btn_WorldObjectPickerCancel", "Button"),
    ],
    # 世界放置（放置／部署／绑定三页）：整域未接线，按钮全部由 DisableDomainActions 处置，
    # 不需要逐个绑定——取消类按钮会被安全出口规则自动保留。
}


def descend(root: dict, path: str) -> dict | None:
    """按根相对（或含根名）路径找到节点。"""
    segments = path.split("/")
    if segments and segments[0] == root["name"]:
        segments = segments[1:]
    node = root
    for seg in segments:
        node = next((c for c in node.get("children", []) if c["name"] == seg), None)
        if node is None:
            return None
    return node


def named_children(node: dict, parent_path: str, prefix: str, kind: str,
                   require_component: str | None = None) -> list[dict]:
    out = []
    for ch in node.get("children", []):
        if not ch["name"].startswith(prefix):
            continue
        if require_component and not any(c["type"] == require_component for c in ch.get("components", [])):
            continue
        out.append({"node": f"{parent_path}/{ch['name']}", "kind": kind})
    return out


PAGE_STATE_NAMES = ("Loading", "Empty", "Error", "Success", "Disabled")


def camel(name: str) -> str:
    return (name[:1].lower() + name[1:]) if name else name


def walk_paths(node: dict, prefix: str = "") -> list[tuple[str, dict]]:
    """返回子树里每个节点的 (完整路径, 节点)，含自身。"""
    path = f"{prefix}/{node['name']}" if prefix else node["name"]
    out: list[tuple[str, dict]] = [(path, node)]
    for child in node.get("children", []):
        out.extend(walk_paths(child, path))
    return out


def auto_page_bindings(root: dict, used_nodes: set[str]) -> list[tuple[str, str, str]]:
    """按命名规则为每个页面自动推导「接入数据与状态」所需的节点绑定。

    规则（字段名 = `_` + camelCase(页面语义 + 剩余语义)）：

      Txt_<PageId><Rest>Body      → TextMeshProUGUI
      Content_<PageId><Rest>      → RectTransform
      Item_<PageId><Rest>Template → GameObject
      Grp_<PageId><State>State    → GameObject（Loading/Empty/Error/Success/Disabled）

    只覆盖接入必需的节点。页面内按钮的意图带业务语义，按需在 EXTRA_BINDINGS 显式声明，
    不在这里自动推导——否则会为确认弹窗那类多页 Form 造出上百个用不到的字段。
    """
    out: list[tuple[str, str, str]] = []
    seen_fields: set[str] = set()

    host_path = find_path(root, "Grp_PageHost")
    if host_path is None:
        return out

    host = descend(root, host_path)
    if host is None:
        return out

    for page in host.get("children", []):
        page_name = page["name"]
        if not page_name.startswith("Panel_Page"):
            continue

        page_id = page_name[len("Panel_Page"):]
        prefix = "_" + camel(page_id)

        for path, node in walk_paths(page, host_path):
            name = node["name"]
            if name == page_name or path in used_nodes:
                continue

            field: str | None = None
            kind: str | None = None

            if name.startswith("Txt_" + page_id) and name.endswith("Body"):
                # 末尾一律带类型后缀：Txt_<Page><Rest>Body 与 Content_<Page><Rest> 的
                # 剩余语义可能同名（实测 HubObjectsIndex 两者都有），不加后缀会互相去重。
                field = prefix + name[len("Txt_" + page_id):-len("Body")] + "Body"
                kind = "TextMeshProUGUI"
            elif name.startswith("Content_" + page_id):
                field = prefix + name[len("Content_" + page_id):] + "Content"
                kind = "RectTransform"
            elif name.startswith("Item_" + page_id) and name.endswith("Template"):
                field = prefix + name[len("Item_" + page_id):-len("Template")] + "Template"
                kind = "GameObject"
            elif name.startswith("Grp_" + page_id) and name.endswith("State"):
                state = name[len("Grp_" + page_id):-len("State")]
                if state in PAGE_STATE_NAMES:
                    field = prefix + state + "State"
                    kind = "GameObject"

            if field is None or field in seen_fields:
                continue

            seen_fields.add(field)
            out.append((field, path, kind))

    return out


def build_bindings(root: dict, form_name: str) -> tuple[list[dict], dict, list[str]]:
    """外壳公共绑定 + Form 额外绑定；返回 (bindings, formArrays, 未解析的字段)。"""
    bindings: list[dict] = []
    form_arrays: dict[str, list[dict]] = {}
    missing: list[str] = []

    def bind(field: str, node_name: str, kind: str) -> None:
        path = find_path(root, node_name)
        if path is None:
            missing.append(f"{field} -> {node_name}")
            return
        bindings.append({"path": field, "node": path, "kind": kind})

    def bind_optional(field: str, node_name: str, kind: str) -> None:
        """外壳差异用：全屏根（FieldHudForm/WorldPlacementForm）本就没有返回/关闭按钮。"""
        path = find_path(root, node_name)
        if path is not None:
            bindings.append({"path": field, "node": path, "kind": kind})

    def first_button(node: dict) -> str | None:
        for comp in node.get("components", []):
            if comp.get("type") == "Button" and node["name"] not in ("Btn_FormBack", "Btn_FormClose"):
                return node["name"]
        for ch in node.get("children", []):
            hit = first_button(ch)
            if hit:
                return hit
        return None

    bind("_pageHost", "Grp_PageHost", "RectTransform")
    bind_optional("_backButton", "Btn_FormBack", "Button")
    bind_optional("_closeButton", "Btn_FormClose", "Button")

    # 全屏外壳（WorldPlacementForm）没有安全返回按钮，首焦点必须落到页面内首个可用交互；
    # 把它显式绑进契约，脚本才有可引用的落点（L3 也要求每个 SerializeField 非空）。
    if find_path(root, "Btn_FormBack") is None:
        fallback = first_button(root)
        if fallback:
            bind("_firstInteractable", fallback, "GameObject")

    # 页面根数组：Grp_PageHost 下的内容页，顺序即规格页序。
    # 必须按前缀过滤——FieldHudForm 的 Grp_PageHost 还挂着侧栏关闭按钮。
    host_path = find_path(root, "Grp_PageHost")
    if host_path:
        host = descend(root, host_path)
        if host is not None:
            roots = named_children(host, host_path, "Panel_Page", "GameObject")
            if roots:
                form_arrays["_pageRoots"] = roots

    # 导航按钮数组：Grp_Navigation 的分页按钮（无分页的 Form 为空分组，不生成）。
    nav_path = find_path(root, "Grp_Navigation")
    if nav_path:
        nav = descend(root, nav_path)
        if nav is not None:
            buttons = named_children(nav, nav_path, "Btn_", "Button", require_component="Button")
            if buttons:
                form_arrays["_navButtons"] = buttons

    for field, node_name, kind in EXTRA_BINDINGS.get(form_name, []):
        bind(field, node_name, kind)

    # 页面级节点引用：接入层必须持有引用才能写数据与切状态组，而 33 个 Form 靠手写不现实，
    # 因此按命名规则从树里自动推导。已在 bindings / formArrays 里出现过的节点不重复绑定。
    already = {b["node"] for b in bindings}
    for items in form_arrays.values():
        for item in items:
            already.add(item["node"])

    for field, node_path, kind in auto_page_bindings(root, already):
        bindings.append({"path": field, "node": node_path, "kind": kind})

    return bindings, form_arrays, missing


def collect_names(node: dict, out: set[str]) -> None:
    """收集一棵契约子树里的全部节点名（用于检测外壳导航与页面节点重名）。"""
    out.add(node["name"])
    for child in node.get("children", []):
        collect_names(child, out)


def build_navigation_index(form_def: dict, nav_buttons: list[dict], page_roots: list[dict]) -> list[int]:
    """导航按钮 → 规格页索引。

    导航节点与目标页由 00-共享外壳-prefab-layout.md 的导航表唯一给出（如
    Btn_BaseCommandHubOverview → HubOverview）；页面根名为 `Panel_Page<pageId>`，
    因此目标页索引可在 _pageRoots 里定位。定位不到的记 -1，生成器据此跳过该按钮。
    """
    if not nav_buttons or not page_roots:
        return []

    page_names = [r["node"].rsplit("/", 1)[-1] for r in page_roots]
    targets = [str(item.get("target", "")) for item in (form_def.get("navigation") or [])]

    index: list[int] = []
    for i in range(len(nav_buttons)):
        target = targets[i] if i < len(targets) else ""
        want = "Panel_Page" + target
        index.append(page_names.index(want) if want in page_names else -1)
    return index


def assemble(root: dict, form_name: str, form_def: dict, family_key: str) -> dict:
    sub = FAMILY_DIR.get(family_key, DEFAULT_DIR)
    bindings, form_arrays, missing = build_bindings(root, form_name)

    contract = {
        "schemaVersion": 3,
        "form": form_name,
        "namespace": "AutoEra.UI",
        "prefabPath": f"{PREFAB_ROOT}/{sub}/{form_name}.prefab",
        "scriptPath": f"{SCRIPT_ROOT}/{form_name}.cs",
        "referenceResolution": [1920, 1080],
        "root": root,
        "bindings": bindings,
        "scalars": [],
        "scriptFields": [],
        "generatedFrom": "界面规格/prefab-layout.md（tools/ui_spec_to_contract.py）",
    }
    if form_arrays:
        contract["formArrays"] = form_arrays

    nav_index = build_navigation_index(
        form_def, form_arrays.get("_navButtons", []), form_arrays.get("_pageRoots", []))
    if nav_index:
        contract["navigationPageIndex"] = nav_index

    if missing:
        contract["_unresolvedBindings"] = missing
    return contract


# ---------------------------------------------------------------- 主流程

def collect_pages(spec_dir: Path) -> tuple[dict[str, dict], dict[str, dict], dict]:
    """返回 ({pageId: 页面树}, {family: {pageId: 标题}}, 外壳定义)。"""
    page_trees: dict[str, dict] = {}
    page_titles: dict[str, str] = {}
    for layout in sorted(spec_dir.glob("*/prefab-layout.md")):
        fam = P.parse_family(layout)
        fam_key = layout.parent.name[:2]
        for page in fam["pages"]:
            if page["tree"] is None:
                continue
            page_trees[page["pageId"]] = page["tree"]
            page_titles[page["pageId"]] = page["title"]

    shell = P.parse_shared_shell(spec_dir / "00-共享外壳-prefab-layout.md")
    shell_rows = parse_shell_layout(spec_dir / "00-共享外壳-prefab-layout.md")
    return page_trees, page_titles, {**shell, "shellRows": shell_rows, "pageTitles": page_titles}


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--out", default="Docs/Development/UI-PrefabLayouts")
    ap.add_argument("--family", help="只处理某个家族的 Form（如 01）")
    ap.add_argument("--form", help="只处理某个 Form")
    ap.add_argument("--all", action="store_true")
    ap.add_argument("--dry-run", action="store_true")
    args = ap.parse_args()

    root_dir = Path.cwd()
    spec_dir = root_dir / P.SPEC_DIR

    page_trees, _page_titles, shell = collect_pages(spec_dir)
    forms = shell["forms"]

    # 页面 → 归属家族（用于选择预制体目录）
    page_family: dict[str, str] = {}
    for layout in sorted(spec_dir.glob("*/prefab-layout.md")):
        fam_key = layout.parent.name[:2]
        for page in P.parse_family(layout)["pages"]:
            page_family.setdefault(page["pageId"], fam_key)

    targets: list[str] = []
    if args.form:
        targets = [args.form]
    elif args.family:
        for name, d in forms.items():
            for pg in d["pages"]:
                pid = Path(pg["href"]).stem
                if page_family.get(pid) == args.family:
                    targets.append(name)
                    break
    else:
        targets = list(forms)

    out_dir = root_dir / args.out
    written = 0
    report = []

    for form_name in targets:
        fdef = forms.get(form_name)
        if not fdef:
            report.append((form_name, "外壳文档未定义该 Form", 0))
            continue

        page_nodes = []
        for i, pg in enumerate(fdef["pages"]):
            pid = Path(pg["href"]).stem
            tree = page_trees.get(pid)
            if tree is None:
                continue
            node = build_page_node(tree)
            node["_title"] = pg["title"]
            node["_pageId"] = pid
            if i > 0:
                node["active"] = False
            page_nodes.append(node)

        fam_key = page_family.get(Path(fdef["pages"][0]["href"]).stem, "00") if fdef["pages"] else "00"
        contract = build_form_contract(form_name, fdef, page_nodes, shell["shellRows"], fam_key)

        # 清理辅助字段
        for pg in contract["root"].get("children", []):
            strip_meta(pg)
        def clean(n):
            n.pop("_title", None)
            n.pop("_pageId", None)
            n.pop("_control", None)
            n.pop("_unresolved", None)
            for ch in n.get("children", []):
                clean(ch)
        clean(contract["root"])

        n = count_nodes(contract["root"])
        report.append((form_name, contract["prefabPath"], n))
        unresolved = contract.get("_unresolvedBindings") or []
        for field in unresolved:
            print(f"  [警告] {form_name} 绑定目标不存在：{field}")

        if not args.dry_run:
            out_dir.mkdir(parents=True, exist_ok=True)
            (out_dir / f"{form_name}.contract.json").write_text(
                json.dumps(contract, ensure_ascii=False, indent=1), encoding="utf-8"
            )
            written += 1

    for name, path, n in report:
        print(f"{name:34} {n:5} 节点  {path}")
    print(f"\n{'干跑' if args.dry_run else '已写出'} {written if not args.dry_run else len(report)} 个契约")
    return 0


def count_nodes(node: dict) -> int:
    return 1 + sum(count_nodes(ch) for ch in node.get("children", []))


if __name__ == "__main__":
    raise SystemExit(main())
