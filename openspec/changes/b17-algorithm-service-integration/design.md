## Context

B15 已交付算法域完整服务层（`AlgorithmDocument` / `AlgorithmValidator` / `AlgorithmRuntime` / `AlgorithmInstanceService` / `AlgorithmTemplateLibrary` / `AlgorithmMachineAdapter`），26/26 测试通过，但服务只在测试里被 `new` 出来。生产运行路径（`AutoEraWorldSession` → `InitialRegion` → `MachineExecutionContext`）从未创建它们。

三个算法界面（`AlgorithmLibraryForm` / `AlgorithmEditorForm` / `AlgorithmBindingForm`）的视觉预制体与绑定字段（`.Fields.cs`）已就绪，但 `AlgorithmReadModels.Create` 固定返回 `UnavailableAlgorithmReadModel`，界面整页显示「不可用」。

现有骨架：
- `AutoEraWorldSession`：持有 `IdAllocator` / `ObjectRegistry` / `Clock` / `Events` / `Machines`（MachineRoster），无模板库。
- `MachineExecutionContext`：持有 `Tasks` / `Compute` / `Sensors`，无算法实例服务。
- `AutoEraUiSession`：持有 `Application` / `World` / `Region`，可访问 `World`。

## Goals / Non-Goals

**Goals:**
- 世界级 `AlgorithmTemplateLibrary` 接入 `AutoEraWorldSession`，随世界生命周期创建/释放。
- `AlgorithmReadModels.Create` 返回真实读模型（模板列表 + 详情），替换空实现。
- 录入五套系统模板，让算法库浏览/公开参数页展示真实数据。
- 机器级 `AlgorithmInstanceService` 接入执行上下文，让编辑/诊断/绑定界面可用。

**Non-Goals:**
- 不改 B15 服务层（`AlgorithmDocument` / `AlgorithmRuntime` / `AlgorithmInstanceService` / `AlgorithmTemplateLibrary` 等）。
- 不改界面预制体与 `.Fields.cs`（读模型接口已冻结，只替换 `Create` 分支）。
- 不做节点画布拖拽/连线交互（P3-010 的画布数据接线是目标，交互手势后续）。
- 不做磁盘存档、离线推进、真实农林生产提供者（B15 边界）。
- 不做五模板成本最终定值（需用户确认后另定）。

## Decisions

1. **世界级模板库挂 `AutoEraWorldSession`**：模板跨机器共享、生命周期随世界，构造时 `new AlgorithmTemplateLibrary(IdAllocator)`，`Dispose` 随世界清理。理由：模板库是无场景依赖的纯 C# 数据源，与 `MachineRoster` 同级。

2. **五套系统模板在世界创建时录入**：模板是纯 `AlgorithmDocument` 数据，录入逻辑放在模板库初始化（世界构造时或首次访问时）。基础灌溉（成本 9）与农田基础作业（成本 22）按 DEC-121 已确认值录入；生长资源采集/储量资源开采/固定路线运输按结构录入、成本标记「待重算」，重算结果列用户确认。

3. **UI 读模型只替换 `Create` 分支**：`AlgorithmReadModels.Create(session)` 从 `session.World.AlgorithmTemplates` 读模板列表与详情，构造真实 `IAlgorithmReadModel`。接口不扩展，四个算法界面零改动。

4. **机器实例服务分阶段（阶段 B，已交付）**：`AlgorithmInstanceService` 依赖 `MachineComputePool`（执行上下文有）与 `bindingsValid`（来自 `AlgorithmMachineAdapter`，而 adapter 依赖 navigation + region）。因此实例服务在区域「部署机器 + 绑定导航」处创建——落地在 `RegionMachineRuntimeRegistry.TryAttach`（`new AlgorithmMachineAdapter(...)` + `new AlgorithmInstanceService(IdAllocator, Compute, () => (ulong)machine.Revision, adapter.ValidateBindings)`），而非在 `MachineExecutionContext` 构造时。

5. **分阶段交付（已交付）**：阶段 A（世界模板库 + 五模板 + 实例读模型）与阶段 B（机器实例服务 + 编辑/诊断读模型）均落地。机器运行时（`RegionMachineRuntime`）同时持有 `Adapter` 与 `Instances`，编辑/诊断界面从 `Instances.ListInstances()` 读版本三元组、逻辑算力与应用请求状态。

## Risks / Trade-offs

- [机器实例依赖导航绑定链] → **已解决**：接入点落在 `RegionMachineRuntimeRegistry.TryAttach`，实例服务与适配器在同一处按顺序创建（运行时创建 → 导航绑定 → 适配器 → 实例服务），不存在假依赖方向。
- [五模板成本需按新端口重算] → **已定稿（DEC-202）**：灌溉 9、开采 15、采集 17、农田 14、固定运输 35，口径 DEC-117；偏差为粒度差异，非缺陷。
- [读模型接口冻结] → 只替换 `Create` 分支，不扩接口；实例状态快照（版本三元组/逻辑算力/应用请求）已满足编辑/诊断只读需求。
- [模板库与实例服务的世界退出顺序] → 模板库随 `AutoEraWorldSession.Dispose` 清理，实例服务随 `RegionMachineRuntime.Dispose`（Instances → Adapter → 导航绑定 → Context）释放，遵循既有对称生命周期约定。

## Open Questions

（无待决事项。原「模板列表 UI 欠账（2.x 未落地）」已补齐：新增 `TemplateAlgorithmReadModel`（读 `AutoEraWorldSession.AlgorithmTemplates`），`AlgorithmReadModels.Create` 按 `AlgorithmReadModelDomain.Library/Machine` 分域，`IAlgorithmReadModel` 增加 `SelectTemplate`，`AlgorithmLibraryForm` 渲染系统/玩家模板目录与选中模板详情，回归 12/12 通过。）
