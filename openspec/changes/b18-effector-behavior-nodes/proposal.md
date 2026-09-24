## Why

五套初始算法模板（P3-015）的作业核心是效应器行为：基础灌溉的水枪控制、农田作业的机械臂播种/收获/清理、生长资源采集的切割、储量资源开采的钻探。但 B15 只交付了 Navigate（移动）与 Log（日志）两种世界行为，`AlgorithmNodeKind` 没有效应器行为种类，`AlgorithmMachineAdapter.Submit` 只支持导航与日志。这使五套模板无法录入、无法运行，G3「自动化可配置」缺了让机器真正干活的最后一环。B15 proposal 亦明确「五生产模板及成本独立门禁」。

## What Changes

- 新增效应器行为节点种类（`AlgorithmNodeKind.Effector`）：通过绑定效应器组件 + 动作类型 + 强类型动作参数表达「水枪喷射 / 机械臂播种收获清理 / 切割 / 钻探」等世界行为。
- 扩展 `AlgorithmIntent` 携带效应器指令（动作类型、目标引用、强类型参数、优先级、安全中断规则），替换现有单一 `Value` 无法表达多参数的限制。
- `AlgorithmCatalog` 为效应器行为节点定义输入参数端口与统一结果事件端口。
- `AlgorithmEvaluation` 求值产生效应器意图；`AlgorithmMachineAdapter` 通过 `EffectorBehaviorQueue.Submit` 提交，并把结果事件回传算法。
- 补齐任务控制节点（提交/查询/取消机器任务，成本 4），支撑固定路线运输模板的显式防重。
- 不改 B15 已交付的传感器端点、导航与求值批次语义；不实现效应器物理表现（动画/播放），只接通「算法 → 效应器队列」的权威链路。

## Capabilities

### New Capabilities

- `effector-behavior-nodes`: 效应器行为节点的表达、端口、求值与权威提交。
- `task-control-nodes`: 机器任务控制节点（提交/查询/取消）的表达、求值与权威提交。

### Modified Capabilities

- `algorithm-graph-core`: `AlgorithmNodeKind` / `AlgorithmNode` / `AlgorithmIntent` 扩展（新增效应器与任务控制节点种类、指令负载）。【B15 该能力尚未同步进 `openspec/specs/`，本变更作为其增量定义】

## Impact

- 修改 `AlgorithmDocument.cs`（`AlgorithmNodeKind`、`AlgorithmNode`）、`AlgorithmEvaluation.cs`（`AlgorithmIntent` + 求值）、`AlgorithmCatalog.cs`（端口）、`AlgorithmMachineAdapter.cs`（效应器/任务提交）。
- 不改 `AlgorithmRuntime` 的批次/算力/事件语义，不改 `MachineTaskQueue` / `EffectorBehaviorQueue` 本身，不改界面预制体与 `.Fields.cs`，不改 xlsx。
- 效应器物理执行（表现层动画、工作桥接消费 `BehaviorRequest`）独立于本变更，后续单独评估。
