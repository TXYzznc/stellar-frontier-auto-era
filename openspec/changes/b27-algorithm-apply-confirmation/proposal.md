## Why

b26 已把「应用草稿」按钮接到统一应用命令，但 `Apply` 可能进入 `AwaitingWarningConfirmation`（有警告需玩家确认）——此时玩家无从确认，应用链卡在等待。本变更补上确认/取消两条命令，并把工作台「应用草稿」按钮升级为「应用/确认并应用」两用：无请求＝应用，有待确认警告＝确认。

## What Changes

- `IAlgorithmReadModel` 新增 `bool ConfirmWarnings(ulong instanceId, ulong requestId)` 与 `bool CancelApply(ulong instanceId, ulong requestId)`；机器域转发到实例服务，库页/不可用域返回 false。
- `AlgorithmEditorForm.OnApplyClicked` 按请求状态分支：`AwaitingWarningConfirmation` → `ConfirmWarnings`，否则 → `Apply`；`SetApplyButtonInteractable` 在 `WaitingSafePoint`/`Applying` 时禁用按钮（等待安全点）。

## Capabilities

### New Capabilities

- `algorithm-apply-confirmation`: 应用请求的确认与取消命令，以及工作台按钮的「应用/确认」动态行为。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（接口 + 三个实现）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs`（`OnApplyClicked`/`SetApplyButtonInteractable`）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/UI/AlgorithmReadModelEditModeTests.cs`（命令分发回归）。

## Non-Goals

- 不做「确认警告」的独立对话框（复用「应用草稿」按钮动态标签，后续批可加独立确认 UI）。
- 不做 `AwaitingWarningConfirmation` 时对「取消」的独立按钮（取消经 `CancelApply` 命令，UI 入口后续批）。
