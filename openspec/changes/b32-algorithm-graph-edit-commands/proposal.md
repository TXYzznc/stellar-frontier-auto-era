## Why

P3-010 与 P3-011 已具备图读取、节点画布定位、拖拽和诊断，但草稿图没有受修订保护的创建节点、连接或断开连接命令。画布因此只能查看和移动，不能安全编辑其结构。

## What Changes

- 为算法文档提供创建节点、连接端口与断开连线的聚焦命令。
- 在实例服务和算法读模型提供以草稿修订保护的对应写入口。
- 命令在写入前验证端点存在、端口存在、类型/单位/能力兼容以及目标输入未占用。
- 保持 UI 端口交互与自由画线为后续表现层批次；本批先闭合可测试的领域命令链。

## Capabilities

### New Capabilities

- `algorithm-graph-edit-commands`: 草稿节点创建、强类型连接和断开连接。

## Impact

- `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmDocument.cs`
- `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmInstanceService.cs`
- `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`
- 对应 EditMode 测试。

## Non-Goals

- 不新增自由画线、端口拖拽或边的视觉表现。
- 不在本批实现节点参数编辑器或节点库视觉设计。
