## Context

B15 交付的算法执行层只有两种世界行为：`Navigate`（移动，提交到 `MachineNavigation`）与 `Log`（日志）。`AlgorithmNodeKind` 枚举 14 种里没有效应器行为，`AlgorithmMachineAdapter.Submit` 对非 Navigate/Log 意图直接 `throw InvalidOperationException("Unsupported authority endpoint.")`。

机器域已经存在效应器队列：`MachineExecutionContext.BindEffector<T>(ComponentInstance)` 返回 `EffectorBehaviorQueue<T>`，其 `Submit(task, algorithm, node, target, priority, interruption, parameters, out request)` 已具备完整的「指令 → 优先级等待队列 → 结果回传」能力，但目前**没有任何调用方**——算法层从未向它提交，表现层也还没有消费它的 `Started` 事件。

五套模板（DEC-121/122/123）的作业核心都是效应器行为：水枪喷射、机械臂播种/收获/清理、切割、钻探。因此本变更要打通「算法图 → 效应器队列」这条权威链路，并补齐任务控制节点。

## Goals / Non-Goals

**Goals:**
- 新增效应器行为节点种类，能表达目标、动作类型与强类型动作参数。
- 扩展算法意图负载，让一个意图携带多参数 + 动作类型，而不是只带单个 `Value`。
- `AlgorithmEvaluation` 求值产生效应器意图；`AlgorithmMachineAdapter` 把它提交到 `EffectorBehaviorQueue`，并把结果事件回传算法。
- 补齐机器任务控制节点（提交/查询/取消，成本 4），支撑固定运输模板显式防重。

**Non-Goals:**
- 不实现效应器物理表现（动画、工作桥接消费 `BehaviorRequest`、`Finish` 调用方）——那是表现层，独立评估。
- 不实现货舱读取节点、通信请求节点、集合筛选/排序节点。
- 不录入五套模板（P3-015 是本变更之后的独立步骤）。
- 不改 `AlgorithmRuntime` 批次/算力/事件语义，不改 `MachineTaskQueue`/`EffectorBehaviorQueue` 内部。

## Decisions

### 1. 节点种类：单一 `Effector` + 动作类型枚举

效应器行为是**一个节点种类** `AlgorithmNodeKind.Effector`，具体行为由 `AlgorithmNode.Action`（新增 `AlgorithmEffectorAction` 枚举）区分，目标与参数由端口提供。理由：DEC-117 明确「组件通过强类型端点提供…行为能力」，行为不是按效应器型号硬编码的节点种类；单一节点 + 动作枚举避免节点种类爆炸。

`AlgorithmEffectorAction`：`Spray`（启动喷射）、`ModifySpray`（改流量）、`StopSpray`、`Sow`、`Harvest`、`Clean`、`Transfer`、`Cut`、`Drill`。第一版只有这九种，后续新增动作扩枚举。

任务控制用三个独立节点种类（它们不是效应器，有独立生命周期）：`SubmitTask`、`QueryTask`、`CancelTask`。

### 2. 意图负载：`AlgorithmIntent` 增加 `Action` + `Parameters`

现有 `AlgorithmIntent` 只带单个 `Value`，无法表达效应器多参数。扩展为：

- `Value` 保留（`Navigate` 的 Position、`Delay` 的秒数）。
- 新增 `Action`（`AlgorithmEffectorAction?`，仅效应器意图使用）。
- 新增 `Parameters`（`Dictionary<string, AlgorithmValue>`，效应器参数；目标对象以 `"target"` 键携带）。

不引入独立 `EffectorInstruction` 类型：意图列表 `Intents` 是批次内统一提交单元，保持单一容器更简单。

### 3. 端口：效应器行为按动作类型动态返回端口

`AlgorithmCatalog.Inputs(node)` 在 `node.Kind == Effector` 时，根据 `node.Action` 返回「event + target + 动作专属参数」；`Outputs(node)` 返回统一结果事件（`accepted`/`started`/`completed`/`failed`/`cancelled`/`preempted`/`targetInvalid`/`rejected`/`partial`）。

参数类型一律用既有 `AlgorithmType`（Number 流量/数量/功率、Boolean 开关、Object 目标、Enumeration 物品类型），不新增类型种类。必填参数端口 `Required=true`，可选参数由 `AlgorithmNode.Default` 提供默认值。

动作 → 参数映射（第一版）：
- `Spray`：`target`(Object)、`flow`(Number, 可选默认标准流量)
- `ModifySpray`：`target`、`flow`(Number)
- `StopSpray`：`target`
- `Sow`/`Harvest`：`target`、`count`(Number)
- `Clean`：`target`
- `Transfer`：`target`、`count`(Number)、`item`(Enumeration)
- `Cut`：`target`、`targetValue`(Number)
- `Drill`：`target`、`power`(Number, 可选)

### 4. 求值与提交

`AlgorithmEvaluation.Emit` 对 `Effector` 节点：读入全部参数端口（含 `target`），组装一个携带 `Action` + `Parameters` 的意图；`TaskControl` 节点同理组装单参数意图。

`AlgorithmMachineAdapter.Submit` 对 `Effector` 意图：解析目标对象的效应器组件 → 取得对应 `EffectorBehaviorQueue<T>`（由 `BindEffector` 在部署/装配时预先注册的队列索引）→ `Submit` 提交，`Started`/`Ended` 结果事件经 `Result` 回传算法。效应器队列尚未注册或参数不匹配时，发布 `rejected` 而非抛异常。

### 5. 任务控制节点

`SubmitTask`（创建显式任务、可设名称/优先级/目标）、`QueryTask`（查询本机符合条件的任务并输出引用/状态）、`CancelTask`（请求取消任务）。它们提交到 `MachineTaskQueue`（不是效应器队列），成本 4（DEC-117）。第一版只实现五模板用到的最小面：`SubmitTask` + `QueryTask`；`CancelTask` 随实现但五模板暂不用。

## Risks / Trade-offs

- [效应器队列目前无消费者] → 本变更只保证「算法能提交、队列能接受、结果能回传」，效应器物理执行由表现层独立接入；测试用桩消费者验证提交链路。
- [动作类型驱动的动态端口] → 端口由 `AlgorithmCatalog` 按 `Action` 返回，`Validator` 的必填端口检查要同步覆盖新端口；加回归用例防漏。
- [意图负载扩展影响既有 Navigate/Delay 路径] → 保持 `Value` 语义不变，`Parameters` 仅在效应器/任务控制意图填充，既有 26 个 B15 测试作为回归围栏。
