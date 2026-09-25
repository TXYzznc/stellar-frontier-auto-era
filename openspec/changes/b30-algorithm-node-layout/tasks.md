## 1. 领域与服务

- [x] 1.1 `AlgorithmDocument.MoveNode(id, x, y)`（更新坐标 + `Revision++`）
- [x] 1.2 `AlgorithmInstanceService.MoveNode(id, expectedRevision, nodeId, x, y)`（入口校验 + 复制草稿移动 + `Changed`）

## 2. 读模型

- [x] 2.1 接口 `MoveNode(instanceId, nodeId, x, y)` + 机器域实现（读草稿修订转发）
- [x] 2.2 库页/不可用域返回 false
- [x] 2.3 `UiAlgorithmNodeRow` 暴露 `LayoutX`/`LayoutY`

## 3. 画布渲染与拖拽

- [x] 3.1 契约 `_algorithmGraphContent` + `_algorithmGraphElementTemplate` 绑定 + 生成 Fields
- [x] 3.2 `RenderGraphCanvas` 按布局坐标实例化节点并填名
- [x] 3.3 `AlgorithmGraphNodeDragHandler`（拖拽禁用父 ScrollRect + delta 平移 + 松手回写 `MoveNode`）

## 4. 回归与收口

- [x] 4.1 `AlgorithmInstanceEditModeTests` 10/10（新增 `MoveNode_UpdatesLayoutAndIncrementsRevision`）
- [x] 4.2 `AlgorithmReadModelEditModeTests` 27/27（新增 `MachineDomain_MoveNode_DispatchToService` + `MachineDomain_GraphNodes_ExposeLayoutCoordinates`）
- [x] 4.3 普通编译 0 错误 0 警告、Console 0 错误
- [x] 4.4 `openspec validate b30-algorithm-node-layout --strict` 通过
- [x] 4.5 更新本 tasks 收口，回传结果
