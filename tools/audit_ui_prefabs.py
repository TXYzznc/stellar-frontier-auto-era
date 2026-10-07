#!/usr/bin/env python3
"""静态盘点 Assets/Game/Prefabs/UI，并生成可复跑的结构/架构优化报告。

该脚本只读取 Prefab、契约和 UI 脚本，不打开或重写 Unity 资产。它用于把逐页审查
变成可复核的基线，便于后续每次 Prefab 结构调整后比较对象数量、列表模板和脚本热点。
"""

from __future__ import annotations

import argparse
import re
from collections import Counter
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
PREFAB_ROOT = ROOT / "Assets" / "Game" / "Prefabs" / "UI"
SCRIPT_ROOT = ROOT / "Assets" / "Game" / "Scripts" / "AutoEra" / "UI"
DOC_ROOT = ROOT / "Docs" / "Development" / "UI-PrefabLayouts"
ALLOWED_PREFIXES = {
    "Bg_", "Overlay_", "Panel_", "Grp_", "Txt_", "Img_", "Icon_", "Btn_",
    "Tgl_", "Sld_", "List_", "Viewport_", "Content_", "Item_", "Bar_", "Deco_",
}


def blocks(text: str, marker: str) -> list[str]:
    parts = re.split(r"(?=^--- !u!\d+ )", text, flags=re.MULTILINE)
    return [part for part in parts if part.startswith(marker)]


def game_object_names(text: str) -> list[tuple[str, bool]]:
    result: list[tuple[str, bool]] = []
    for block in blocks(text, "--- !u!1 "):
        name = re.search(r"^  m_Name: (.*)$", block, re.MULTILINE)
        active = re.search(r"^  m_IsActive: (\d+)$", block, re.MULTILINE)
        if name:
            result.append((name.group(1).strip(), active is None or active.group(1) == "1"))
    return result


def script_hotspots(form: str) -> dict[str, int]:
    paths = [SCRIPT_ROOT / f"{form}.cs", SCRIPT_ROOT / f"{form}.Fields.cs"]
    text = "\n".join(path.read_text(encoding="utf-8-sig") for path in paths if path.exists())
    return {
        "loc": len(text.splitlines()),
        "instantiate": len(re.findall(r"\bInstantiate\s*\(", text)),
        "destroy": len(re.findall(r"\bDestroy\s*\(", text)),
        "hierarchy_scans": len(re.findall(r"GetComponentsInChildren|GetComponentInChildren|transform\.Find", text)),
        "updates": len(re.findall(r"\b(?:Update|LateUpdate|FixedUpdate)\s*\(", text)),
    }


def analyze(path: Path) -> dict[str, object]:
    text = path.read_text(encoding="utf-8-sig")
    names = game_object_names(text)
    name_only = [name for name, _ in names]
    form = path.stem
    prefixes = Counter(
        name[: name.find("_") + 1] for name in name_only if "_" in name
    )
    invalid = sorted(
        name for name in name_only
        if name != form
        and ("_" not in name or name[: name.find("_") + 1] not in ALLOWED_PREFIXES)
    )
    page_roots = [name for name in name_only if name.startswith("Panel_Page")]
    states = [name for name in name_only if name.startswith("Grp_") and name.endswith("State")]
    templates = [name for name, active in names if name.startswith("Item_") and name.endswith("Template")]
    hotspots = script_hotspots(form)
    is_dialog = any(token in form.lower() for token in ("dialog", "alert", "feedback", "recovery", "exitflow"))
    recommendations: list[str] = []
    if len(names) >= 300:
        recommendations.append("拆成独立 UIForm 页或子 Form；保留导航壳，避免一次实例化全部页面")
    elif len(names) >= 150:
        recommendations.append("将独立分页/详情区拆为子 Form；长列表继续使用 UIItem 对象池")
    if templates:
        recommendations.append(f"{len(templates)} 个 Item 模板应保持 inactive，并由 SpawnItem/UnspawnItem 管理")
    if hotspots["instantiate"] or hotspots["destroy"]:
        recommendations.append("脚本仍含运行时 Instantiate/Destroy；优先改为 UIItem 或局部复用池")
    if hotspots["hierarchy_scans"] >= 8:
        recommendations.append("OnInit 缓存节点/控件引用，避免刷新路径反复 GetComponentsInChildren/Find")
    if is_dialog:
        recommendations.append("操作确认/提示型页面可迁移到 GF UIDialog 模板，Form 只保留业务回调")
    if not recommendations:
        recommendations.append("结构规模可控；维持契约生成、对象池和统一外壳生命周期")
    return {
        "form": form,
        "path": path.relative_to(ROOT).as_posix(),
        "bytes": path.stat().st_size,
        "objects": len(names),
        "active": sum(1 for _, active in names if active),
        "buttons": len(re.findall(r"^  m_TargetGraphic:", text, re.MULTILINE)),
        "scrolls": len(re.findall(r"^  m_Content:", text, re.MULTILINE)),
        "layout_groups": len(re.findall(r"^  m_ChildAlignment:", text, re.MULTILINE)),
        "templates": templates,
        "page_roots": page_roots,
        "states": len(states),
        "invalid": invalid,
        "prefixes": prefixes,
        "hotspots": hotspots,
        "recommendations": recommendations,
        "contract": (DOC_ROOT / f"{form}.contract.json").exists(),
    }


def render(records: list[dict[str, object]]) -> str:
    records = sorted(records, key=lambda item: str(item["form"]))
    total_objects = sum(int(item["objects"]) for item in records)
    largest = sorted(records, key=lambda item: int(item["objects"]), reverse=True)[:5]
    lines = [
        "# UI Prefab 全量结构与优化审查",
        "",
        "> 生成命令：`python tools/audit_ui_prefabs.py --write`。本报告只读扫描，不修改 Unity 资产。",
        "",
        f"扫描范围：`Assets/Game/Prefabs/UI`，共 **{len(records)}** 个 Prefab，合计 **{total_objects}** 个 GameObject。",
        "契约存在性、节点命名和绑定仍以门 1 检查为准；本报告额外关注运行时对象生命周期、分页耦合和脚本热点。",
        "",
        "## 最高复杂度页面",
        "",
        "| 页面 | GameObject | Button | ScrollRect | Item 模板 | 脚本 LOC | 主要动作 |",
        "|---|---:|---:|---:|---:|---:|---|",
    ]
    for item in largest:
        hot = item["hotspots"]
        actions = ", ".join(item["recommendations"][:2])
        lines.append(
            f"| `{item['form']}` | {item['objects']} | {item['buttons']} | {item['scrolls']} | "
            f"{len(item['templates'])} | {hot['loc']} | {actions} |"
        )
    lines += ["", "## 逐页审查", "", "| 页面 | 资产规模 | 结构 | 脚本热点 | 结论 |", "|---|---:|---|---|---|"]
    for item in records:
        hot = item["hotspots"]
        structure = (
            f"{item['objects']} GO / {item['buttons']} Btn / {item['scrolls']} Scroll / "
            f"{len(item['templates'])} Item / {len(item['page_roots'])} 页 / {item['states']} 状态"
        )
        script = (
            f"{hot['loc']} LOC；Instantiate {hot['instantiate']}；Destroy {hot['destroy']}；"
            f"层级查找 {hot['hierarchy_scans']}；Update {hot['updates']}"
        )
        flags = []
        if not item["contract"]:
            flags.append("缺契约")
        if item["invalid"]:
            flags.append(f"命名 {len(item['invalid'])} 项")
        flags.extend(item["recommendations"][:2])
        lines.append(f"| `{item['form']}` | {item['bytes'] / 1024:.1f} KB | {structure} | {script} | {'；'.join(flags)} |")
    lines += ["", "## 统一架构结论", "", "- 所有页面继续保持一个 GF UIForm 入口；列表行通过 `UIItemObject` 对象池复用。", "- `AlgorithmEditorForm`、`BaseCommandHubForm`、`FieldHudDetailForm` 是拆分优先级最高的三个页面；先按完整分页/详情域拆子 Form，再迁移动态模板。", "- `AlertForm`、`OperationDialogForm`、`OperationFeedbackForm`、`SaveRecoveryForm`、`ExitFlowForm` 属于提示/确认语义，建议逐步迁移为 UIDialog；迁移前保留现有 UIViews ID 与回调合同。", "- 不建议机械拆出每个装饰节点；只有跨页面复用、独立交互和独立生命周期同时成立时才新增 UIItem 资产。", "- 本轮代码级低风险优化集中在 `UIItemBase` 空绑定安全、`AutoEraShellFormBase` 控件扫描缓存和现有对象池路径；分页拆分与 UIDialog 迁移需要对应契约和运行测试后分批进行。", ""]
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--write", action="store_true", help="写入 Docs/Development/UI-PrefabLayouts/UIOptimizationAudit.md")
    args = parser.parse_args()
    records = [analyze(path) for path in PREFAB_ROOT.rglob("*.prefab")]
    report = render(records)
    if args.write:
        output = DOC_ROOT / "UIOptimizationAudit.md"
        output.write_text(report, encoding="utf-8")
        print(output.relative_to(ROOT).as_posix())
    else:
        print(report)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
