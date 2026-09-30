## Why

b32 交付了算法草稿图编辑命令层，但界面仍不能编辑：画布只能查看与拖拽节点。同时发现两个真实缺口：(1) b26/b29/b30 增加的四个字段（应用按钮、节点搜索、画布内容、画布元素模板）从未写回 AlgorithmEditorForm 预制体，真实游戏里静默空转；(2) 规格定义的草稿工具条（撤销/重做/删除选中）从未落地。节点栏按规格本应是「节点库」，当前渲染的是图节点列表（b20 权宜）。

## What Changes

- 修复 AlgorithmEditorForm 预制体缺失的四个序列化字段绑定，并新增「添加」按钮绑定。
- 按规格补建 Grp_AlgorithmDraftTools 草稿工具条：删除选中可用，撤销/重做为禁用占位（命令历史属后续批次）。
- 服务与读模型补 `DeleteNode` 命令（删除草稿节点并级联断开其关联边；修订保护）。
- 读模型快照新增节点种类目录（节点库数据）与每节点的输入/输出端口行（含类型与已连接状态）。
- AlgorithmEditorForm：节点栏改为节点库（分类+成本+搜索过滤），点击行或「添加或连接节点」按钮在画布创建节点；画布节点元素渲染端口列表，两步端口连线（先点输出、再点输入）；边以中点元素渲染，选中后可经「删除选中」断开；选中节点后可经「删除选中」删除；不兼容连线在问题栏呈现原因（类型/单位/能力）。

## Capabilities

### Modified Capabilities

- `algorithm-editor-canvas`: 画布从只读+拖拽升级为可编辑（添加节点、端口连线、断开、删除）。

### New Capabilities

- `algorithm-node-library`: 节点种类目录（分类、成本、端口能力）供界面节点库展示。

## Impact

- `Docs/Development/UI-PrefabLayouts/AlgorithmEditorForm.contract.json`（bindings 追加）
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.Fields.cs`（生成）
- `Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab`（绑定修复+工具条补建，经 UnitySkills）
- `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`、`AlgorithmDocument.cs`（DeleteNode 级联）
- `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`（NodeKinds/端口行/DeleteNode）
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs`（交互）
- 对应 EditMode 测试。

## Non-Goals

- 撤销/重做命令历史（按钮以禁用占位落地，后续批次）。
- 连线的视觉形状（Img_AlgorithmEdge 保持既有 Image；端点计算与贝塞尔表现属美术/后续）。
- 节点参数编辑（「修改节点和参数」按钮仍禁用，属后续批次）。
- 自由拖线画线交互（规格允许两步端口选择建立/断开，本批采用两步模式）。
