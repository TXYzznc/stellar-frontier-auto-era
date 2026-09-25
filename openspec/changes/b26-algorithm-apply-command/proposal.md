## Why

b25 已能「激活草稿」，但玩家在算法工作台点「应用草稿」（`Btn_AlgorithmEditorApply`，契约节点已存在、字段未生成）没有任何反应——写路径停在「领域有命令、界面无按钮」。本变更把「应用」收成一个统一命令：草稿实例第一次应用＝激活（`ActivateDraft`），已激活实例的后续草稿＝`Apply`，并把工作台「应用草稿」按钮接上这条命令链。

## What Changes

- `AlgorithmInstanceService` 新增无 hardware 参数的 `Apply(id, expectedDraft, expectedApplied, out request)` 重载，内部用 `_hardwareRevision()`。
- `IAlgorithmReadModel` 新增 `bool Apply(ulong instanceId)`：机器域按实例状态分支——草稿（`AppliedRevision == 0`）→ `ActivateDraft`；已激活且草稿领先 → `Apply`；否则 false。库页/不可用域返回 false。
- 契约 `AlgorithmEditorForm.contract.json` 把 `Btn_AlgorithmEditorApply` 加入 `bindings`，重新生成 `AlgorithmEditorForm.Fields.cs`；`AlgorithmEditorForm` 接线 onClick → `Apply`（并随 `Changed` 刷新）。

## Capabilities

### New Capabilities

- `algorithm-apply-command`: 统一「应用」命令，草稿实例首应用＝激活、已激活实例草稿应用＝`Apply`，并接入工作台「应用草稿」按钮。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`（`Apply` 重载）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（接口 `Apply` + 机器域实现）。
- 修改 `Docs/Development/UI-PrefabLayouts/AlgorithmEditorForm.contract.json`（加 binding）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.Fields.cs`（生成）与 `AlgorithmEditorForm.cs`（接线）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/AlgorithmInstanceEditModeTests.cs` 与 `UI/AlgorithmReadModelEditModeTests.cs`（应用命令回归）。

## Non-Goals

- 不做 `ConfirmWarnings`/`CancelApply` 的 UI 交互（警告确认对话框后续批）。
- 不做组件选择器、节点画布拖拽、节点库搜索。
