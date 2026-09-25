## Why

算法读路径（b17 实例服务 / b20 图接线 / b21 诊断）已就绪，但写路径还是空的：机器上的实例只能由测试夹具手工 `Add`，玩家没有「从模板创建实例」的入口——`AlgorithmReadModels.NoInstanceReason` 写的正是「实例由模板实例化创建，而『模板 → 实例』的创建入口尚未接线」。模板（`InitialAlgorithmTemplates`）含 `Input`/`Effector` 节点，`AlgorithmTemplateLibrary.Save` 已清空绑定，所以实例化产物是**未绑定、未编译**的文档，无法走 `Add(AlgorithmRuntime)`（它只接受已编译运行时）。本变更落写路径的第一块：让实例服务能持有「草稿实例」（无运行时、未绑定、未编译），并提供「从模板创建草稿实例」的命令，使库页选中模板 → 创建实例 → 编辑器显示图与「缺少绑定」校验问题成为可能。

## What Changes

- `AlgorithmInstanceService` 新增 `AddDraft(AlgorithmDocument)` 创建草稿实例（`Runtime = null`），并在 `ListInstances`/`SavedDraftRevision`/`Apply`/`Capture`/`Restore`/`Pump`/`TotalCost`/`Dispose` 等处对空运行时做守卫。
- `IAlgorithmReadModel` 新增命令方法 `ulong InstantiateTemplate(ulong templateId)`：库页读模型解析当前选中机器的实例服务，模板库 `Instantiate` 产出未绑定文档后 `AddDraft`，返回新实例 Id（失败返回 0）；机器域/不可用域返回 0。
- `UiAlgorithmInstanceRow` 对「未应用」草稿实例（`AppliedRevision == 0`）展示「未应用」而非「已应用 r0」。
- 机器读模型已能读草稿图与校验问题（b20），本变更确认草稿实例可被选中并在编辑器里显示「缺少绑定」问题。

## Capabilities

### New Capabilities

- `algorithm-draft`: 算法草稿实例——未绑定、未编译、无运行时的实例，只存在于草稿；可被模板实例化创建并在编辑器里显示结构与校验问题。

### Modified Capabilities

- `algorithm-instances`: 实例服务的 `ListInstances` 等只读出口对草稿实例（空运行时）语义化（`AppliedRevision=0`、`LogicCost=0`），不抛异常。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`（`AddDraft` + 空运行时守卫）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（接口命令、库页读模型持 session 并实现实例化、实例行展示、`TryResolveMachine` 提升为 internal）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/UI/AlgorithmReadModelEditModeTests.cs` 与（或）`AlgorithmInstanceEditModeTests.cs`（草稿实例回归）。
- 不改界面预制体、契约 JSON、`.Fields.cs`（库页「创建实例」按钮接线独立成批）。

## Non-Goals

- 不做库页「创建实例」按钮接线（契约 JSON 字段映射 + `.Fields.cs` 重新生成 + 表单 onClick，独立成批）。
- 不做绑定重绑、编译应用（`Apply`/`ConfirmWarnings`/`CancelApply`）、参数编辑；草稿实例在此批只「可读不可应用」。
- 不自动替玩家绑定组件或世界对象（设计文档 DEC 明确禁止）。
