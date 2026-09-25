## Why

b24 已能「看」待绑定端点并 `Rebind` 草稿绑定，但草稿实例（`Runtime = null`）仍**无法编译为运行时**——`Add(AlgorithmRuntime)` 只接受外部已编译运行时且要求实例不存在，`Apply` 又要求 `Runtime != null`。写路径因此断在「绑定完成 → 编译激活」。本变更补上这最后一环：实例服务能接收编译好的运行时替换空运行时，读模型提供 `ActivateDraft` 命令，把「绑定完成且校验通过的草稿」推进成可运行实例。

## What Changes

- `AlgorithmInstanceService` 新增 `CompileDraft(ulong id, AlgorithmRuntime runtime)`：仅对 `Runtime == null` 的草稿实例，校验运行时 Id 一致、`IsSafe`、算力容量，然后挂接运行时并触发 `Changed`。
- `IAlgorithmReadModel` 新增 `bool ActivateDraft(ulong instanceId)`：机器域读模型读草稿 → `TryCompile(draft, 真实算力容量, …, template:false)`（有错误即返回 false）→ 构造 `AlgorithmRuntime`（adapter 作 sink）→ `Adapter.Attach` → `CompileDraft`；库页/不可用域返回 false。
- 应用请求链（`Apply`/`Pump`）在激活后即可对草稿实例生效（b22 的空运行时守卫已就位）。

## Capabilities

### New Capabilities

- `algorithm-draft-activation`: 草稿实例经「绑定完整 + 校验通过」后编译为运行时，进入可应用状态。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`（`CompileDraft`）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（接口 `ActivateDraft` + 机器域实现）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/AlgorithmInstanceEditModeTests.cs` 与 `UI/AlgorithmReadModelEditModeTests.cs`（激活回归）。

## Non-Goals

- 不做编辑器「应用」按钮接线（`Btn_AlgorithmEditorApply` onClick → 应用请求链，涉及确认/等待安全点 UI，后续批）。
- 不做 `ConfirmWarnings`/`CancelApply` 的 UI 交互；激活后应用请求的确认链由领域层 `Apply`/`Pump` 承载，UI 后续批接。
- 不做参数编辑、节点画布拖拽。
