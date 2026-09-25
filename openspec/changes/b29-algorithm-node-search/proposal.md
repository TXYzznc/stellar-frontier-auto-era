## Why

算法工作台的节点栏列出了选中实例的全部图节点，但「搜索节点」输入框（`Panel_AlgorithmEditorNodeSearch`）此前没有接线——玩家无法在节点多时快速定位某个节点。本变更把搜索框接成真实的节点过滤：按输入即时筛选节点栏（匹配节点名或状态，忽略大小写）。

## What Changes

- 契约生成器 `TYPE_BY_KIND` 新增 `TMP_InputField → TMP_InputField`，支持搜索框这类输入组件绑定。
- `AlgorithmEditorForm.contract.json` 新增 `_algorithmEditorNodeSearch` 绑定（`TMP_InputField`）。
- `AlgorithmEditorForm` 监听搜索框 `onValueChanged`，按过滤词即时筛选节点栏（命中计数回显到节点栏说明），点选仍按过滤后行的稳定 Id 选中。

## Capabilities

### New Capabilities

- `algorithm-node-search`: 算法工作台按名称/状态过滤图节点。

## Impact

- 修改 `tools/ui_contract_to_form_script.py`（`TMP_InputField` kind 映射）。
- 修改 `Docs/Development/UI-PrefabLayouts/AlgorithmEditorForm.contract.json`（搜索框绑定）。
- 重新生成 `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.Fields.cs`（新增字段）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs`（过滤 + 搜索回调）。

## Non-Goals

- 不做节点库（可用节点类型目录）的独立面板：本变更只接「搜索框过滤当前图节点」，节点类型目录属后续。
- 不做搜索高亮/模糊匹配排序。
