## Why

B17 已把算法实例服务与读模型接入生产运行路径，算法编辑器能列出「本机有哪些实例」并展示机器检视信息，但**选中一个实例后看不到它的图结构**：节点栏仍显示实例列表（而非图节点），检视器显示机器详情（而非节点属性），问题栏固定「诊断尚未运行」（而非真实校验问题）。编辑器的核心功能——看清并校验算法图——尚未兑现。这属于 S4 算法 UI 收尾（P3-010 画布数据接线 / P3-011 检查器与问题栏），且只读，不涉及交互手势与写路径。

## What Changes

- 算法读模型为选中实例暴露草稿图快照：图节点（Id/Kind/Operator/类型/默认值/绑定键/字段）、图连线（From→To）、校验问题（错误/警告，含节点定位）。
- `IAlgorithmReadModel` 新增 `SelectNode`（选中节点供检视器展示），快照新增 `GraphNodes`/`GraphEdges`/`Issues`/`NodeDetail`。
- `AlgorithmEditorForm` 用真实数据渲染：节点栏列图节点（可点选）、检视器列选中节点属性、问题栏列校验问题。
- 新增读模型 EditMode 回归：选中实例后图快照非空、校验问题与文档一致性、选中节点后详情非空、无实例/未选中时图快照为空。

## Capabilities

### New Capabilities

- `algorithm-editor-graph`: 算法编辑器展示选中实例的真实图结构（节点/连线）与校验问题清单。

### Modified Capabilities

（无——不改动已归档 spec 的需求，本变更新增能力。）

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（快照新增图/问题/节点详情字段，机器读模型读草稿并编译，`SelectNode` 语义）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs`（节点栏/检视器/问题栏真实渲染）。
- 修改 `Assets/Game/Tests/AutoEra/Editor/UI/AlgorithmReadModelEditModeTests.cs`（新增图快照回归）。
- 不改服务层（`AlgorithmInstanceService`/`AlgorithmValidator`/`AlgorithmRuntime`）、界面预制体与 `.Fields.cs`、框架；新增程序集仍为既有 Hotfix。

## Non-Goals

- 不做节点画布拖拽/连线交互、节点库搜索、应用写路径（Apply/ConfirmWarnings）、诊断运行历史与路径高亮、绑定重绑（留待后续批次）。
