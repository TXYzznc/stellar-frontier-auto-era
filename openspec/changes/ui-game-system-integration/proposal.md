# 界面与游戏系统接入

## Why

第一版 33 个 UIForm 预制体已按界面规格产出并通过门1（结构／命名／布局／绑定）验收，但它们仍是**结构原型**：

- 生产环境只有 3 处 `OpenUIForm` 调用（`AutoEraMainMenuProcedure`、`AutoEraWorldProcedure`，以及仅存在于 Editor 工具里的中枢入口）；
- 33 个预制体里 `UIStringKey` 与 `UIItemObject` 组件数均为 0（本地化与列表复用未接）；
- 33 个 Form 的 `OnOperationPresentationChanged` 全为空实现，`AutoEraUiOperationSnapshot` 通道零生产调用。

界面因此既拿不到领域数据，也没有把玩家意图送进领域。同时，界面背后的领域系统只实现了一部分：机器、算法、区域、传感、存档、时间、事件齐备；能源、库存、配方、工坊、升级、图纸、交易、交付、任务、警报、作物、离线结算、用户设置这 13 类只有界面与数据表。

## What Changes

- 新增**界面接入层**六段：服务通道、读取契约、意图契约、状态与格式化、导航与返回、列表池与本地化。
- 服务通道使用 `UIParams`（其基类 `RefParams` 已经提供 `Set/TryGet` 键值通道，底层为 `GF.VariablePool`）携带会话句柄，**不引入全局服务定位器，也不引入 DI 容器**。
- 读取契约**按数据域划分而非按页面**：只对「被两个以上页面复用」或「需要跨多个领域聚合」的数据域建立只读模型。
- `UiDataState` 的 `Unavailable` 态把「该数据域的系统尚未接入」建模为可展示的正常状态，使未就绪界面可以先接成诚实空态。
- 先接领域已就绪的部分，未就绪的部分接空态；领域系统实现后逐个回填数据源。
- 接入层**不得改变界面结构**：结构只由 `Docs/Development/UI-PrefabLayouts/*.contract.json` 决定，门1 必须持续 33/33。

## Capabilities

### New Capabilities

- `ui-game-system-integration`：界面与领域系统之间的服务通道、读取／意图契约、状态呈现与导航返回。

### Modified Capabilities

无。本变更不修改已冻结的界面规格与预制体结构。

## Impact

- 代码：`Assets/Game/Scripts/AutoEra/UI` 新增接入层与读取模型；Form 脚本增量补齐订阅与意图实现。
- 数据登记：无新增 Form（33 个已登记 `UIViews` 6000—6032）。
- 验收：现有门1（33/33）、`run_project_checks.py`（5/5）与全界面 PlayMode 冒烟必须保持通过；新增「领域变化 → 界面更新 → 关闭退订」的数据流测试。
- **不在本变更内**：13 类领域系统本身的实现（另立变更，按 `Docs/GameDesign/02-系统设计` 推进）。
