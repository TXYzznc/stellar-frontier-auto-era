## Why

B20 已把算法编辑器的图结构（节点/连线/校验问题）接到读模型，但编辑器的**诊断模式**仍是空白：运行时在每次求值后已通过 `AlgorithmRunRecord` 记录执行路径、失败节点与错误原因（`AlgorithmRuntime.History()`），读模型却从不暴露它，界面无法高亮已执行路径、区分失败/未执行节点。诊断是编辑器两种模式之一（P3-014），与图展示同属读路径，不依赖写路径，应先补齐。

## What Changes

- `AlgorithmInstanceService` 新增 `ReadHistory(ulong id)`：返回该实例运行时最近运行记录快照（无实例/无运行时为空）。
- 读模型读最近一次运行记录，把图节点标记为「已执行／失败／未执行」，并在快照暴露运行摘要（运行号、错误、失败节点、成本）。
- `AlgorithmEditorForm` 在检视器/节点栏展示诊断摘要与节点执行状态。
- 新增读模型 EditMode 回归：运行后历史非空、节点状态正确、无运行时历史为空。

## Capabilities

### New Capabilities

- `algorithm-diagnostic`: 算法编辑器诊断读路径——暴露运行时运行历史，并标记节点的执行/失败状态。

### Modified Capabilities

（无——不改动已归档 spec 的需求。）

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`（新增 `ReadHistory`）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（节点行增加诊断状态、快照增加运行摘要、机器读模型读历史）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs`（展示诊断摘要与节点状态）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/UI/AlgorithmReadModelEditModeTests.cs`（新增诊断回归）。
- 不改 `AlgorithmRuntime`/`AlgorithmRunRecord`、界面预制体与 `.Fields.cs`、框架。

## Non-Goals

- 不做暂停世界逐节点执行、断点、单步、历史快照重放、跨运行路径选择（设计文档明确第一版不做）。
- 不做「触发时当时值 vs 当前值」的逐值展示（诊断摘要只给最近运行记录），不做应用写路径/绑定重绑。
