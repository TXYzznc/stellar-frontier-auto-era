## Context

`AlgorithmInstanceService.Entry` 目前是 `{ Runtime, Draft, Saved, Request }`，其中 `Runtime` 恒非空——`Add(AlgorithmRuntime)` 只接受已编译运行时，且 `ListInstances`/`Pump`/`TotalCost`/`Dispose`/`Apply`/`Validate`/`Capture`/`Restore` 都直接解引用 `entry.Runtime`。模板实例化的产物是**未绑定、未编译**的 `AlgorithmDocument`（`AlgorithmTemplateLibrary.Instantiate` 只重排节点/连线 Id、清空绑定），无法编译为 `AlgorithmRuntime`，因此需要一个「草稿实例」状态：有草稿、无运行时。

## Goals / Non-Goals

**Goals:**

- 实例服务能持有草稿实例（`Runtime == null`），只读出口对空运行时语义化而非抛异常。
- 提供「从模板创建草稿实例」的命令，返回新实例 Id 或失败。
- 草稿实例在机器读模型里可被选中并显示图结构与「缺少绑定」校验问题。

**Non-Goals:**

- 库页「创建实例」按钮接线、绑定、编译应用、参数编辑（后续批）。

## Decisions

1. **`AddDraft(AlgorithmDocument)` 复用 `DocumentId` 作为实例 Id**：`AlgorithmTemplateLibrary.Instantiate` 用 `_ids.TryAllocate` 分配 `DocumentId`，而模板库与实例服务共享同一个 `session.IdAllocator`（`AutoEraWorldSession.AlgorithmTemplates` 与 `RegionMachineRuntimeRegistry.TryAttach` 都传 `_session.IdAllocator`），故 `DocumentId` 与 `Add` 的 `InstanceId` 同一 Id 空间，不会撞号。`AddDraft` 校验 `DocumentId != 0` 且 `Nodes/Edges/Bindings` 非空、Id 不重复。
2. **草稿实例初始 `Saved = Draft.Copy()`**：`Saved` 表示「玩家保存过的草稿」，新建草稿视为已保存，`SavedDraftRevision` 无需额外空值分支（`Saved` 恒非空）。
3. **空运行时守卫清单**：`ListInstances` 用 `entry.Runtime?.Revision ?? 0`、`entry.Runtime?.LogicCost ?? 0`；`Apply`/`Capture`/`Restore` 在 `entry.Runtime == null` 时返回 false（草稿实例不可应用/捕获/恢复）；`Validate` 仅由 `Apply`/`Pump` 的请求分支调用，草稿实例无请求故不会触达，但 `Apply` 已前置 `Runtime == null` 守卫；`Pump` 的逐实例 `entry.Runtime.Pump` 与「同机其他实例 IsSafe」两处跳过空运行时；`TotalCost` 用 `?.LogicCost ?? 0`；`Dispose` 用 `entry.Runtime?.Dispose()`。
4. **命令挂 `IAlgorithmReadModel.InstantiateTemplate`**：库页表单已持有 `IAlgorithmReadModel`，最小改动即把「写」命令挂进读模型接口（读模型本就有 `Select` 这类改选中状态的方法）。`TemplateAlgorithmReadModel` 构造函数增加 `AutoEraUiSession`，`InstantiateTemplate` 解析当前选中机器（复用 `AlgorithmReadModels.TryResolveMachine`，从 private 提为 internal）→ 取实例服务 → `_library.Instantiate` → `AddDraft`。`MachineAlgorithmReadModel`/`UnavailableAlgorithmReadModel` 恒返回 0。
5. **实例行展示**：`UiAlgorithmInstanceRow.Status` 对 `AppliedRevision == 0` 显示「未应用（草稿）」，其余保持「已应用 rN」。

## Risks / Trade-offs

- **草稿实例不可应用/不可诊断**：`Runtime == null` 时 `ReadHistory` 返回空、`Apply` 返回 false，编辑器问题栏显示「缺少绑定」错误。这是正确的中间态——写路径后续批次（绑定 → 编译 → 应用）会把它推进到可应用。
- **命令挂在读模型上**：牺牲了严格的 CQRS 分离换取最小改动。若后续命令增多（绑定、应用、参数编辑），应评估拆出独立命令接口；本批只加一个命令，先不引入新接口。
- **空运行时守卫的完整性**：`_entries.Values` 的遍历点（`Pump` 两处、`TotalCost`、`Dispose`）是主要 NRE 风险，测试用「加草稿实例后 Pump/Dispose/ListInstances 不抛」覆盖。
