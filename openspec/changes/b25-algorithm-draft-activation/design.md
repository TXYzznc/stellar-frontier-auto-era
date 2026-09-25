## Context

`AlgorithmInstanceService` 没有 sink 引用（ctor 只有 `ids`/`compute`/`hardwareRevision`/`bindingsValid`），运行时必须由外部构造（sink 即 `AlgorithmMachineAdapter`，挂在 `RegionMachineRuntime.Adapter`）。`Add` 只接受「实例不存在 + 已编译运行时」，草稿实例的 `Entry.Runtime = null` 挡住了它。`Apply` 在 b22 加了 `entry.Runtime == null` 守卫，故草稿实例不可应用。激活 = 外部构造运行时 → 挂进草稿实例 Entry。

## Goals / Non-Goals

**Goals:** `CompileDraft`（领域层挂接运行时）+ `ActivateDraft`（命令层编译+挂接），草稿实例激活后可应用。

**Non-Goals:** 编辑器「应用」按钮、`ConfirmWarnings`/`CancelApply` UI、参数编辑。

## Decisions

1. **`CompileDraft(id, runtime)` 是 `Add` 的「就地激活」变体**：校验 `runtime.InstanceId.Value == id`、`entry.Runtime == null`、`runtime.IsSafe`、`TryApplyLogicCost(TotalCost() + runtime.LogicCost)`；成功则 `entry.Runtime = runtime` 并 `Changed`。与 `Add` 一致不订阅运行时事件（运行时 `AppliedChanged` 由 `AlgorithmMachineAdapter.Attach` 消费）。
2. **`ActivateDraft` 挂 `IAlgorithmReadModel`（机器域）**：读草稿 → `TryCompile(draft, Runtime.Context.Compute.LogicCapacity, …, template:false)`（返回 false＝有错误，如 `RequiredBinding`，则拒绝激活）→ `new AlgorithmRuntime(new PersistentId(instanceId), plan, Runtime.Context.Compute, Runtime.Adapter)` → `Runtime.Adapter.Attach(runtime)` → `_instances.CompileDraft`。真实算力容量刻意用机器的 `LogicCapacity`——超容量即不可激活（符合 DEC「容量不足可存草稿不可应用」）。
3. **激活与后续应用分层**：`ActivateDraft` 只把 `Runtime` 从 null 变非空；后续「应用新配置」仍走既有 `Apply`/`Pump` 安全点链。草稿实例激活后 `ListInstances.AppliedRevision` 变为运行时的 `Revision`（1）。
4. **库页/不可用域返回 false**：它们不持有实例服务与 adapter。

## Risks / Trade-offs

- **`Adapter.Attach` 只允许一次**：`AlgorithmMachineAdapter.Attach` 对已 attach 抛异常，故 `ActivateDraft` 必须只在 `Runtime == null`（即从未 attach 过该实例）时调用——由 `CompileDraft` 的 `entry.Runtime == null` 守卫兜底。
- **真实容量校验**：模板逻辑成本（基础灌溉 9 等）小于夹具算力容量（100），测试不触发容量拒绝；超容量路径由 `TryCompile` 返回 false 拒绝，后续批补容量不足的专项 UI 提示。
- **激活即 `AppliedRevision = 1`**：运行时 `CopyApplied()` 的 `Revision` 为 1，与「首次应用」语义一致；后续 `Apply` 以 `expectedApplied = 1` 起算。
