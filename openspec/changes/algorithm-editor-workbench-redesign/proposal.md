## Why

算法编辑器已经具备节点创建、端口连线、验证和诊断等功能，但当前页面把工作台操作、状态信息和动态图元素挤在同一层级，学习成本高，节点与连线还共用一个运行时模板。需要把它重做成适合游戏内使用的可视化工作台，让玩家能快速理解当前模式、选中对象和下一步操作。

## What Changes

- 将算法编辑器重排为左侧节点库、中间画布、右侧检视器和底部上下文工具条的工作台布局。
- 在专业深色基调上加入游戏化的节点分类色、状态徽标、选中反馈和轻量动效。
- 支持画布缩放、中键平移、节点拖拽和端口拖线，同时保留面向新手的按钮提示。
- **BREAKING** 将算法节点从 Form 内嵌模板迁移为独立 `AlgorithmNodeItem` 预制体。
- 将连线从通用图元素模板中拆出为独立 `AlgorithmEdgeItem` 预制体。
- 端口行继续作为节点 Item 内部模板；节点库行、检视器行和问题行继续由 Form 管理。
- Form 只保存节点和连线 Item 的 prefab 引用，运行时按快照动态实例化和回收。

## Capabilities

### New Capabilities

- `algorithm-editor-workbench`: 游戏内算法可视化编辑工作台的布局、状态反馈、画布交互和动态 Item 生命周期。

### Modified Capabilities

<!-- 当前 openspec/specs 中没有算法编辑器能力，行为合同由本变更新增。 -->

## Impact

- `Assets/Game/Prefabs/UI/Operations/AlgorithmEditorForm.prefab` 的页面布局和序列化引用。
- 新增 `Assets/Game/Prefabs/UI/Item/AlgorithmNodeItem.prefab` 与 `AlgorithmEdgeItem.prefab`。
- `Assets/Game/Scripts/AutoEra/UI/AlgorithmEditorForm.cs` 及字段绑定、画布输入和渲染代码。
- 可能新增算法节点/连线 Item 的专用交互组件和运行时对象池辅助代码。
- 更新算法编辑器 UI 合同、PlayMode 验收和相关资源目录索引。
