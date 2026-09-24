# 区域机器落位与运行时接线

## Why

`ui-game-system-integration` 的 8.1 补记记录了一个缺口：**机器在生产里既落不了位、也拿不到运行时**。
按该补记动手前的代码侦察把范围收窄成了一句话——**每一块零件都已存在，但彼此没有接上**：

| 零件 | 状态 | 生产调用者 |
|---|---|---|
| `RegionPlacement`（吸附、旋转、越界、重叠判定） | 完整 | 被 `CanPlace` / 预览 / 输入模块使用 |
| `RegionPlacementPreview`（校验型落位事务：`Move`/`Rotate`/`IsValid`/`Reason`/`Confirm`） | 完整 | 被输入模块使用 |
| `RegionInputModule.BeginPlacement(size, confirmed)`（指针驱动、画轮廓、合法性着色、点击确认） | 完整，`Confirmed` 回调留空 | **只有编辑器证据工具**，且回调写着「Validated preview only; no construction transaction」 |
| `InitialRegion.DeployMachine`（空间＋身份绑定并把花名册转入已部署） | 完整 | **只有测试** |
| `RegionNavigation.Bind(context, instance, …)`（给实例加 `NavMeshAgent` 并建 `MachineNavigation`） | 完整 | **只有测试** |
| `MachineExecutionContext` / `AlgorithmInstanceService` / `AlgorithmMachineAdapter` | 完整，有集成测试演示全链路 | **无** |
| 机器实体预制体（`Assets/Game/Prefabs/Entity/Machines/*`，移动类含 `MotionRig`） | 已交付 | — |
| 机器定义 → 预制体映射（`MachineDefinitions` 数据表的 `Prefab` 列，值如 `Machines/WheeledCarrier`） | 已交付 | **无消费方** |

直接后果：`WorldPlacementForm` 三页整页不可用、`MachineLibraryForm` 的部署按钮被禁用、
四个算法界面只能呈现「域未接线」——**同一个根因**。

本变更只做「接线」，不新造领域能力：把已有零件接成一条
「选定机器 → 落位校验 → 部署 → 机器视图 → 导航绑定 → 运行时创建 → 界面回填」的生产链路。

## What Changes

- 新增**机器落位流程**：把 `RegionInputModule` 的落位预览从「只校验」变成可提交的部署事务，
  失败时给出可展示的原因而不是抛异常。
- 新增**机器视图绑定路径**：`RegionObjectView` 现有 `Initialize` 是**注册**新区域对象，
  不能用于已部署机器（会重复注册）。新增「绑定已部署对象」的通路，其释放**不得**删除领域对象
  ——机器的部署状态属于领域，不属于视图。
- 新增**区域机器运行时**：按已决策的候选 A，在部署成功后创建
  `MachineExecutionContext` + `AlgorithmInstanceService` + `AlgorithmMachineAdapter`，
  在视图就绪时执行 `RegionNavigation.Bind`；生命周期随区域释放。
  运行时**不进存档**：存档只记机器的部署事实（`machine.Deployed` + 区域绑定），
  每次区域就绪重建运行时。这与现有 `DeployMachine` 的语义一致。
- **回填界面**：`WorldPlacementForm` 的机器部署页、`MachineLibraryForm` 的部署按钮、
  四个算法界面的数据源。

## Capabilities

### New Capabilities

- `region-machine-deployment-runtime`：机器从落位校验到运行时创建的生产链路。

### Modified Capabilities

- 无。本变更不改界面契约、不改预制体结构、不改 `UIViews` 登记。

## Impact

- 代码：`Assets/Game/Scripts/AutoEra/World/Region/`、`Assets/Game/Scripts/AutoEra/Input/`、
  `Assets/Game/Scripts/AutoEra/Machines/`、`Assets/Game/Scripts/AutoEra/Algorithms/`，
  以及 `Assets/Game/Scripts/AutoEra/UI/` 的落位与机器库界面。
- 验收：门1 必须持续 33/33；`run_project_checks.py` 5/5；EditMode/PlayMode 回归全绿；
  新增「部署 → 视图 → 导航绑定 → 出现算例 → 算法界面 Ready」的数据流测试。
- **不在本变更内**：建造放置（图纸→建筑）、世界绑定页、机器的库存/经济结算、离线推进。
- **风险点**：区域视图与机器实体的所有权边界（谁在视图销毁时负责撤收）、导航面未就绪时的降级表现。
