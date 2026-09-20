#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""从 `<Form>.contract.json` 生成 Form 脚本。

每个 Form 产出两个文件，构成同一个 partial 类：

  `<Form>.Fields.cs`  契约绑定字段与只读属性（完全由契约决定，永远生成）
  `<Form>.cs`         类逻辑（页序切换、取消意图、默认焦点）

这样切分的原因：节点引用是纯粹的机械内容且数量大（33 个 Form 共 1291 个字段），
必须由契约生成；而业务逻辑要手写。手写脚本（HANDWRITTEN）只跳过 `.cs`，
`.Fields.cs` 照常生成，因此它们也能随契约自动获得新增的节点引用。
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

CONTRACT_DIR = Path("Docs/Development/UI-PrefabLayouts")
SCRIPT_DIR = Path("Assets/Game/Scripts/AutoEra/UI")

# 契约 kind → C# 字段类型
TYPE_BY_KIND = {
    "GameObject": "GameObject",
    "TextMeshProUGUI": "TMP_Text",
    "Image": "Image",
    "Button": "Button",
    "RectTransform": "RectTransform",
    "Transform": "Transform",
}

# 手写脚本：它们的 .cs 带流程接入点，生成器不碰；但 .Fields.cs 仍然生成。
# 逐域接入时把该域的 Form 加进来即可——生成器继续供字段，业务逻辑手写。
HANDWRITTEN = {
    "MainMenuForm", "SaveSlotsForm", "SaveRecoveryForm", "BaseCommandHubForm", "FieldHudForm",
    "SystemMenuForm", "ExitFlowForm", "MachineLibraryForm",
    "AlgorithmEditorForm", "AlgorithmLibraryForm", "AlgorithmBindingForm", "NodeComponentPickerForm",
    "WorldPlacementForm", "WorldObjectPickerForm", "RecordReaderForm",
}

# 领域尚未接入运行路径的 Form：生成器会给它们注入统一的「整页未就绪」渲染。
#
# 判断依据是「该界面依赖的领域服务在生产里没有创建者」，而不是「界面没人写」：
#   * 组件域：ComponentLibraryForm / ComponentPickerForm / UpgradeForm
#   * 库存与交易：WarehouseForm / ShopForm
#   * 建造与工坊：BuildCatalogForm / WorkshopForm
#   * 任务与警报：QuestForm / AlertForm
#   * 用户设置：SettingsForm
#   * 作物知识与帮助系：CropKnowledgeForm / TutorialForm / FeatureHelpForm / HelpForm / RuleHelpForm
#
# 不在此列的参数化界面（OperationDialogForm / OperationFeedbackForm / ProgressReportForm）
# 由调用方传参驱动，不依赖任何领域服务，因此保持空骨架即可。
NOT_WIRED: dict[str, str] = {
    "ComponentLibraryForm": "组件域尚未接入运行路径：组件定义、库存与装配都还没有创建者。",
    "ComponentPickerForm": "组件域尚未接入运行路径：候选组件没有数据来源。",
    "UpgradeForm": "机器改装与组件域尚未接入运行路径：改装项与代价无法计算。",
    "WarehouseForm": "库存域尚未接入运行路径：物品定义与库存账本都还没有创建者。",
    "ShopForm": "交易域尚未接入运行路径：报价与结算服务都还没有创建者。",
    "BuildCatalogForm": "建造图纸域尚未接入运行路径：图纸目录与解锁状态没有数据来源。",
    "WorkshopForm": "工坊与配方域尚未接入运行路径：配方、队列与产出都没有创建者。",
    "QuestForm": "任务域尚未接入运行路径：任务状态与领取结算都还没有创建者。",
    "AlertForm": "警报域尚未接入运行路径：警报分类与处理动都没有数据来源。",
    "SettingsForm": "用户设置域尚未接入运行路径：声音、操作与显示设置还没有持久化载体。",
    "CropKnowledgeForm": "作物知识尚未接入运行路径：作物图鉴数据没有来源。",
    "TutorialForm": "教程内容尚未接入运行路径：步骤与触发条件还没有配置载体。",
    "FeatureHelpForm": "功能说明尚未接入运行路径：说明文本还没有录入本地化表。",
    "HelpForm": "帮助内容尚未接入运行路径：帮助条目还没有录入本地化表。",
    "RuleHelpForm": "通用规则说明尚未接入运行路径：规则条目还没有录入本地化表。",
}

STATE_SUFFIXES = ("LoadingState", "EmptyState", "ErrorState", "SuccessState", "DisabledState")


def build_not_wired(contract: dict) -> tuple[str, str]:
    """为未接入的 Form 生成（常量块, 打开时渲染块）；已接入的 Form 返回空串。"""
    form = contract["form"]
    reason = NOT_WIRED.get(form)
    if reason is None:
        return "", ""

    const_block = (
        "        /// <summary>本界面所属领域尚未接入运行路径；原因写在每一页的空态里。</summary>\n"
        f'        public const string NotWiredReason =\n            "{reason}";\n\n'
        "        /// <summary>\n"
        "        /// 本域是否已接入运行路径。领域接入后请把本 Form 从生成器的 NOT_WIRED 集合移除，\n"
        "        /// 生成器就不会再注入「整页未就绪」这段，届时改为接读模型。\n"
        "        /// </summary>\n"
        "        public bool IsDomainWired => false;\n\n"
    )

    pages: dict[str, dict] = {}
    for binding in contract.get("bindings", []):
        node = binding.get("node", "")
        marker = "/Panel_Page"
        index = node.find(marker)
        if index < 0:
            continue

        page_id = node[index + len(marker):].split("/")[0]
        entry = pages.setdefault(page_id, {"states": {}, "bodies": []})
        path = binding["path"]
        kind = binding.get("kind")

        for suffix in STATE_SUFFIXES:
            if kind == "GameObject" and path.endswith(suffix):
                entry["states"][suffix] = path
        if kind == "TextMeshProUGUI" and path.endswith("Body"):
            entry["bodies"].append(path)

    lines = [
        "\n            // 本界面所属领域尚未接入运行路径：把原因写到每一页的状态组与说明文本上，\n"
        "            // 并禁用本域的业务动作（安全出口、页导航与列表项不受影响）。\n"
    ]
    for page_id, entry in pages.items():
        states = entry["states"]
        if not states:
            continue
        args = ["NotWiredReason"]
        for suffix in STATE_SUFFIXES:
            args.append(states.get(suffix, "null"))
        args.extend(entry["bodies"] or ["null"])
        lines.append("            ShowPageUnavailable(" + ", ".join(args) + ");\n")

    lines.append("            DisableDomainActions();\n")
    return const_block, "".join(lines)


FIELDS_TEMPLATE = """using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{{
    /// <summary>
    /// {form} 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/{form}.contract.json 生成，请勿手改）。
    ///
    /// 与 {form}.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// {form}.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class {form}
    {{
{fields}    }}
}}
"""

LOGIC_TEMPLATE = """using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{{
    /// <summary>
    /// {form} 的 GF 桥。结构由 Docs/Development/UI-PrefabLayouts/{form}.contract.json 生成
    /// （设计来源：Docs/GameDesign/03-玩家体验/界面规格）。
    ///
    /// 绑定字段在同名的 {form}.Fields.cs 里（同一 partial 类）；本文件只有类逻辑：
    /// 规格页序切换、取消意图与默认焦点。Grp_PageHost 下的内容页顺序即规格页序。
    /// </summary>
    public sealed partial class {form} : AutoEraShellFormBase
    {{
{notwired_const}{navigation}        protected override void OnInit(object userData)
        {{
            base.OnInit(userData);
{navigate}{cancel}        }}

        protected override void OnAutoEraOpen()
        {{
{open}{notwired}        }}

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);
{cancel_method}
        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) {{ }}
    }}
}}
"""

NAVIGATION_FIELD = """        /// <summary>
        /// 导航按钮 → 规格页索引（来源：00-共享外壳-prefab-layout.md 中该 Form 的导航表）。
        /// -1 表示该按钮没有对应内容页（二级详情从页内进入而非顶栏导航）。
        /// </summary>
        private static readonly int[] NavigationPageIndex = {{ {index} }};

"""

NAVIGATE_BLOCK = """            if (_navButtons != null)
            {
                for (int i = 0; i < _navButtons.Length; i++)
                {
                    Button button = _navButtons[i];
                    if (button == null || i >= NavigationPageIndex.Length) continue;
                    int page = NavigationPageIndex[i];
                    if (page < 0) continue;
                    button.onClick.AddListener(() => ShowFormPage(page));
                }
            }

"""

CANCEL_BUTTON_BLOCK = """            if ({field} != null)
            {{
                {field}.onClick.AddListener(RequestCancel);
            }}

"""

CANCEL_METHOD = """
        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);
"""


def property_name(field: str) -> str:
    name = field.lstrip("_")
    return name[:1].upper() + name[1:] if name else field


def first_button_name(root: dict, skip: set[str]) -> str | None:
    for comp in root.get("components", []):
        if comp.get("type") == "Button" and root["name"] not in skip:
            return root["name"]
    for child in root.get("children", []):
        hit = first_button_name(child, skip)
        if hit:
            return hit
    return None


def build_fields(contract: dict) -> tuple[str, list[str], list[str]]:
    """返回 (字段声明文本, 字段名列表, 警告列表)。"""
    lines: list[str] = []
    names: list[str] = []
    warnings: list[str] = []

    for binding in contract.get("bindings", []):
        field = binding["path"]
        kind = binding.get("kind", "GameObject")
        cs_type = TYPE_BY_KIND.get(kind)
        if cs_type is None:
            warnings.append(f"未知 kind {kind}（字段 {field}），已按 GameObject 处理")
            cs_type = "GameObject"
        lines.append(f"        [SerializeField] private {cs_type} {field};")
        lines.append(f"        public {cs_type} {property_name(field)} => {field};")
        names.append(field)

    for field, items in (contract.get("formArrays") or {}).items():
        kinds = {item.get("kind", "GameObject") for item in items}
        if len(kinds) != 1:
            warnings.append(f"{field} 的元素 kind 不一致 {sorted(kinds)}，已按 GameObject 处理")
        kind = kinds.pop() if len(kinds) == 1 else "GameObject"
        cs_type = TYPE_BY_KIND.get(kind, "GameObject")
        lines.append(f"        [SerializeField] private {cs_type}[] {field};")
        lines.append(f"        public {cs_type}[] {property_name(field)} => {field};")
        names.append(field)

    return "\n".join(lines) + "\n", names, warnings


def build_fields_file(contract: dict, fields_text: str) -> str:
    return FIELDS_TEMPLATE.format(form=contract["form"], fields=fields_text)


def build_logic_file(contract: dict, names: list[str], has_pages: bool) -> tuple[str, list[str]]:
    form = contract["form"]
    root = contract["root"]
    warnings: list[str] = []

    nav_index = contract.get("navigationPageIndex") or []
    has_nav = "_navButtons" in names
    navigation = NAVIGATION_FIELD.format(index=", ".join(str(i) for i in nav_index)) if (has_nav and nav_index) else ""
    navigate = NAVIGATE_BLOCK if (has_nav and nav_index) else ""

    cancel = ""
    for field in ("_backButton", "_closeButton"):
        if field in names:
            cancel += CANCEL_BUTTON_BLOCK.format(field=field)
    cancel_method = CANCEL_METHOD if cancel else ""

    open_lines: list[str] = []
    if has_pages:
        open_lines.append("            ShowPage(_pageRoots, 0);\n")

    safe = "_backButton" if "_backButton" in names else None
    if has_nav and nav_index:
        first = "_navButtons"
        first_expr = ("_navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null"
                      " ? _navButtons[0].gameObject : null")
    elif "_firstInteractable" in names:
        first = "_firstInteractable"
        first_expr = "_firstInteractable != null ? _firstInteractable.gameObject : null"
    else:
        first = None
        first_expr = "null"
        fallback = first_button_name(root, {"Btn_FormBack", "Btn_FormClose"})
        if fallback:
            for binding in contract.get("bindings", []):
                if binding["node"].rsplit("/", 1)[-1] == fallback:
                    first = binding["path"]
                    first_expr = f"{first} != null ? {first}.gameObject : null"
                    break

    if first is None and safe is None:
        warnings.append("既无安全返回也无页面内交互，该页不会设置默认焦点")

    safe_expr = f"{safe} != null ? {safe}.gameObject : null" if safe else "null"
    open_lines.append(f"            ApplyDefaultFocus({safe_expr}, {first_expr});\n")

    notwired_const, notwired = build_not_wired(contract)

    text = LOGIC_TEMPLATE.format(
        form=form,
        notwired_const=notwired_const,
        notwired=notwired,
        navigation=navigation,
        navigate=navigate,
        cancel=cancel,
        open="".join(open_lines),
        cancel_method=cancel_method,
    )
    return text, warnings


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--force", action="store_true", help="覆盖已存在的非手写逻辑脚本")
    ap.add_argument("--out", default=str(SCRIPT_DIR))
    args = ap.parse_args()

    out_dir = Path(args.out)
    out_dir.mkdir(parents=True, exist_ok=True)

    contracts = sorted(CONTRACT_DIR.glob("*.contract.json"))
    if not contracts:
        print(f"未找到契约：{CONTRACT_DIR}", file=sys.stderr)
        return 2

    fields_written = logic_written = logic_skipped = 0
    for path in contracts:
        contract = json.loads(path.read_text(encoding="utf-8"))
        form = contract["form"]
        fields_text, names, warnings = build_fields(contract)

        (out_dir / f"{form}.Fields.cs").write_text(
            build_fields_file(contract, fields_text), encoding="utf-8")
        fields_written += 1

        logic_path = out_dir / f"{form}.cs"
        if form in HANDWRITTEN:
            logic_skipped += 1
        elif logic_path.exists() and not args.force:
            logic_skipped += 1
        else:
            text, logic_warnings = build_logic_file(contract, names, "_pageRoots" in names)
            logic_path.write_text(text, encoding="utf-8")
            logic_written += 1
            warnings += logic_warnings

        for warning in warnings:
            print(f"  [警告] {form}: {warning}")

    print(f"\n字段文件 {fields_written} 个；逻辑脚本生成 {logic_written} 个、跳过 {logic_skipped} 个（手写或已存在）")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
