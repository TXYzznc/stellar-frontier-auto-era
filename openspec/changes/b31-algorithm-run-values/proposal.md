## Why

算法工作台的诊断只显示「最近一次运行的结果/失败节点/成本」，看不到「运行时各值节点到底读到了什么」。诊断「当时值 vs 当前值」是定位算法错误（例如输入类型漂移、常量算错）的关键——本变更让运行时在执行时记录每个值节点的「当时值」，选中节点后在检视器与「默认值」（当前值）并列展示。

## What Changes

- `AlgorithmBatch` 新增 `NodeValues`（节点 Id → 主输出值快照）；`AlgorithmEvaluation.Value` 在计算主输出端口时记录。
- `AlgorithmRunRecord` 新增 `CopyNodeValues()`（深拷贝节点值快照）。
- 读模型 `UiAlgorithmRunRow` 新增 `NodeValues`；机器域 `_latestRun` 携带最近运行的节点值。
- `BuildNodeDetail` 在选中节点且最近运行有该节点当时值时，追加「当时值」行（与「默认值」并列）。

## Capabilities

### New Capabilities

- `algorithm-run-values`: 诊断读路径暴露运行时节点「当时值」，与当前草稿「默认值」对比。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmEvaluation.cs`、`AlgorithmRuntime.cs`。
- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`。
- 修改 `Assets/Game/Tests/AutoEra/Editor/AlgorithmExecutionEditModeTests.cs`、`AlgorithmReadModelEditModeTests.cs`。

## Non-Goals

- 不做多运行历史的逐条值快照浏览（仅最近一次运行）。
- 不做执行轨迹回放动画。
