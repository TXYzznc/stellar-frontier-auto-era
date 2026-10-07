## Why

AlgorithmEditorForm 的功能链路已经可用，但工作台仍缺少明确的视觉层级、空间参照和操作反馈。玩家进入画布时不容易判断可拖拽区域、缩放方式、当前节点状态和危险动作的优先级。

## What Changes

- 建立深空控制台视觉主题：蓝黑分层面板、青色画布、紫色检视器、琥珀主动作和红色风险动作。
- 将画布背景封装为 `AlgorithmGraphBackdrop`，提供低对比网格、中心轴和角标，不参与射线命中。
- 将视觉策略封装为 `AlgorithmEditorVisualStyle`，集中处理面板强调线、阴影、按钮状态、进入过渡和交互提示。
- 增强节点 Item 的卡片层级：顶部／侧边状态色带、阴影、悬停／按压／选中轮廓反馈，端口文本改为单行省略；节点尺寸不随状态缩放，保持端口几何稳定。
- 移除连线预制体中的矩形命中代理 `Grp_AlgorithmEdge`，改由运行时 `AlgorithmGraphEdgeGraphic` 绘制带水平切线的贝塞尔曲线，并按曲线距离命中。
- 节点拖拽时只刷新与该节点相连的边，避免无关边重算和视觉跳动。
- 首次进入有图数据时自动按节点包围盒适配画布，之后保留玩家的平移、缩放和定位状态。
- 调整工作台三列 RectTransform 比例与节点 Item 尺寸，降低节点文字裁切和空白浪费。

## Impact

- `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs`
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorVisualStyle.cs`
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmGraphBackdrop.cs`
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmNodeItem.cs`
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmPortSocketGraphic.cs`
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmEdgeItem.cs`
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmGraphEdgeGraphic.cs`
- `Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab`
- `Assets/Game/Prefabs/UI/Item/AlgorithmNodeItem.prefab`
- 算法编辑器界面规格与 UI 合同文档
