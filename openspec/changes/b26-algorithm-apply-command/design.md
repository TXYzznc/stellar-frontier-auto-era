## Context

`Apply(id, expectedDraft, expectedApplied, hardware, out request)` 的 `hardware` 由调用方从 `_hardwareRevision()` 取，但读模型不持有硬件修订源（`RegionMachineRuntime` 不暴露 `machine.Revision`）。工作台「应用草稿」按钮（契约节点 `Btn_AlgorithmEditorApply`，line 889）未在 `bindings` 数组（line 6604，仅 4 项 back/close/nav）里，故字段未生成。

## Goals / Non-Goals

**Goals:** 统一 `Apply` 命令（草稿首应用＝激活 / 已激活草稿＝`Apply`）、工作台「应用草稿」按钮接线。

**Non-Goals:** `ConfirmWarnings`/`CancelApply` UI、组件选择器、画布拖拽。

## Decisions

1. **`Apply` 重载封装 hardware**：`Apply(id, expectedDraft, expectedApplied, out request)` 内部调 `_hardwareRevision()`，读模型不再需要硬件修订源；4 参数重载保留（测试与既有调用不动）。
2. **读模型 `Apply(instanceId)` 按状态分支**：查 `ListInstances` 定位实例；`AppliedRevision == 0` → `ActivateDraft`；否则 `Apply(instanceId, DraftRevision, AppliedRevision, out _)`（`DraftRevision <= AppliedRevision` 时由领域层拒绝，返回 false）。命令是幂等的（无未应用草稿＝无事可做）。
3. **按钮接线**：`Btn_AlgorithmEditorApply` 加进 contract `bindings`（`kind: "Button"`），重新生成 `.Fields.cs`（`_algorithmEditorApplyButton`）；`OnInit` 订阅 onClick → 取 `Snapshot.SelectedInstance` → `_algorithms.Apply(id)` → 依赖 `Changed` 事件刷新状态（不手动重建快照）。
4. **EOL 纪律**：生成器会重写全部 33 个 `.Fields.cs`（CRLF→LF），生成后 `git checkout` 无关文件，只保留 `AlgorithmEditorForm.Fields.cs` + contract JSON。

## Risks / Trade-offs

- **应用请求的异步确认**：`Apply` 可能进入 `AwaitingWarningConfirmation`，需玩家确认——本批只触发，确认 UI 后续批；界面靠 `Changed` 事件刷新 `RequestState` 展示当前状态。
- **硬件修订变更竞态**：读模型分支里先查 `ListInstances` 再 `Apply`，两次之间硬件可能变——领域层 `Apply` 用 `_hardwareRevision()` 在服务内部取当前值，天然串行化，竞态由 `Validate` 的 `HardwareOrBindingChanged` 兜底。
- **草稿激活的 `HasRuntime` 限制**：单实例激活（b25 已记录），多实例后续批。
