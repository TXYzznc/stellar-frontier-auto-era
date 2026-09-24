# Design: 本机货舱读取与任务查询节点

## Context

固定路线运输模板（DEC-123）需要两项算法运行时能力：本机货舱读取（DEC-111）和任务查询防重（DEC-123）。当前 `AlgorithmMachineAdapter` 对 QueryTask/CancelTask 只发布 `rejected` 桩，代码库没有机器货舱数据结构。B15 已交付传感器采样（`SensorSubscription` → `trigger.Inputs` → `Input` 节点）与效应器提交（`EffectorBehaviorQueue`），本变更复用"数据节点 + 查询接口"模式补齐缺口。

## Goals / Non-Goals

**Goals:**
- 建立机器货舱契约（统一容量单位、剩余空间、物品明细、变化事件）。
- 算法图本地读取本机货舱（不需传感器绑定）。
- QueryTask 完整返回 `found`/`task` 供运输模板显式防重。
- 录入固定路线运输模板（DEC-123），收口五套系统模板。

**Non-Goals:**
- 不实现资源原子结算、多机并发去重、实体物品容量占用（DEC-111/114 由后续物流版本负责）。
- 不实现 CancelTask 完整取消（仍为桩，DEC-123 基础模板不取消）。
- 不做货舱 UI、动画表现与效应器物理执行。

## Decisions

### D1: MachineCargo 作为机器上下文成员，统一容量单位
`MachineCargo`（`AutoEra.Machines`）持有 `int Capacity/Used`，`Remaining => Capacity - Used`，物品明细用 `Dictionary<string,int>`（物品类型→数量，统一容量单位），`event Action Changed`。挂接到 `MachineExecutionContext`，与 Tasks/Compute 并列。选择统一容量单位符合 DEC-111"第一版使用统一容量单位，不增加体积、重量和特殊容器"。备选：给每台机器独立货舱组件槽——被否，第一版机器天然带舱，简化绑定。

### D2: 货舱读取用独立数据节点 `AlgorithmNodeKind.Cargo`
新增 `Cargo` 节点（成本 1，属 DEC-117"组件数据端点"）：输出 `capacity`/`remaining`/`amount`/`has_item` 数据端口 + `changed` 事件端口，通过查询接口 `IAlgorithmCommandSink.TryReadCargo(field, itemType, out value)` 求值，不需绑定；`itemType` 取自节点 `Field`（物品类型）。备选：复用 `Input` 节点加特殊字段——被否，货舱是本机组件而非区域传感器，走传感器采样会错误地要求目标绑定。

### D3: QueryTask 升级为纯数据节点（found/task）
QueryTask 无 `event` 触发端口（纯数据节点，`Inputs` 为空），新增查询接口 `IAlgorithmCommandSink.TryQueryTask(name, out task)`；求值 `Value()` 时同步查询本机 `MachineTaskQueue`，返回 `found`（Boolean）与 `task`（Object 引用），供下游 `Branch` 防重。任务名取自节点 `Field`（复用字段，语义=查询名）。选择"数据节点+查询接口"而非"Submit 回传事件"：防重判断必须发生在同一求值批次内（DEC-118 单批次确定性），异步回传会拆成两个批次破坏原子判断。CancelTask 保持桩。

### D5: 提交任务与查询防重按稳定任务名对齐
`SubmitTask` 意图新增 `Field` 字段，`AlgorithmMachineAdapter.SubmitTaskNode` 以 `intent.Field`（缺省回落 `"Algorithm " + NodeId`）作为任务名；QueryTask 以相同 `Field` 查询。两者共享同一稳定名即可在运输模板里显式防重（DEC-123"绑定到自身提交节点的任务查询节点防重"）。选择稳定名而非节点 ID 字符串：可读且模板内可维护。

### D6: 比较节点补充 ≤/≥ 运算符
`AlgorithmOperator` 新增 `LessOrEqual`/`GreaterOrEqual`，`AlgorithmEvaluation.Compare` 与 `AlgorithmValidator` 同步放行。固定运输模板的"达到最低出发量"（≥ 10）需要 ≥ 判断，不引入整型转换或减法绕行。非 Number 的比较仍只允许 Equal/NotEqual。

### D4: 查询接口挂在 IAlgorithmCommandSink
`IAlgorithmCommandSink` 新增 `TryReadCargo` 与 `TryQueryTask` 两个同步查询方法，由 `AlgorithmMachineAdapter` 实现（它已持有 `MachineExecutionContext`）。选择挂在 Sink 而非给 `AlgorithmEvaluation` 注入执行上下文：保持求值器与机器状态解耦，测试用假 Sink 即可。

## Risks / Trade-offs

- [查询接口破坏 IAlgorithmCommandSink 实现方] → 新增方法给默认实现（返回 false），仅 `AlgorithmMachineAdapter` 覆写；测试假 Sink 补默认返回，不破坏既有测试。
- [货舱字段语义随物流版本演进] → 第一版仅 `capacity/remaining/items`，物品明细用字符串键，后续版本扩展类型契约时再迁移。
- [QueryTask 查询非原子快照] → 查询在单批次内同步执行，任务队列变化只影响下一批次，符合 DEC-118 冻结快照语义。

## Open Questions

- 无待决事项。五套模板实际逻辑成本已定稿（用户确认接受实际值）：灌溉 9、开采 15、采集 17、农田 14、运输 35，见 DEC-202。
