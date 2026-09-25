## Why

算法工作台的画布（`Panel_AlgorithmEditorCanvas`）此前只有占位 LayoutGroup，节点只出现在左侧节点栏，玩家无法在画布上看到或调整节点位置。节点的画布坐标（`LayoutX`/`LayoutY`）在领域文档里本就存在，缺的是「移动节点」写命令与画布界面入口。本变更补齐写命令（领域 + 实例服务 + 读模型）与画布渲染 + 拖拽回写。

## What Changes

- `AlgorithmDocument.MoveNode(id, x, y)`：更新节点画布坐标并 `Revision++`；未找到/已删除返回 false。
- `AlgorithmInstanceService.MoveNode(id, expectedRevision, nodeId, x, y)`：草稿入口校验 + 复制草稿移动 + `Changed`。
- 读模型 `MoveNode(instanceId, nodeId, x, y)`：机器域读草稿修订后转发实例服务；库页/不可用域返回 false。
- `UiAlgorithmNodeRow` 暴露 `LayoutX`/`LayoutY`（画布定位数据来源）。
- 契约新增 `_algorithmGraphContent` + `_algorithmGraphElementTemplate` 绑定；`AlgorithmEditorForm.RenderGraphCanvas` 按布局坐标实例化画布节点并填节点名。
- 新增 `AlgorithmGraphNodeDragHandler`（拖拽时临时禁用父 ScrollRect、按 delta 平移、松手回写 `MoveNode`）。

## Capabilities

### New Capabilities

- `algorithm-node-layout`: 移动算法草稿节点的画布坐标，并在画布上定位与拖拽。

## Impact

- 修改 `Assets/Game/Scripts/AutoEra/Algorithms/AlgorithmDocument.cs`、`AlgorithmInstanceService.cs`。
- 修改 `Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs`。
- 新增 `Assets/Game/Scripts/AutoEra/UI/AlgorithmGraphNodeDragHandler.cs`。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs` + 契约 + `AlgorithmEditorForm.Fields.cs`。
- 修改 `Assets/Game/Tests/AutoEra/Editor/AlgorithmInstanceEditModeTests.cs`、`AlgorithmReadModelEditModeTests.cs`。

## Non-Goals

- 不做连线（边）拖拽。
- 不做节点端口的连线交互（只读端口模板仍为占位）。
