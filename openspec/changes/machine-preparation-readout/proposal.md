# 机器整备页接入（只读真实数据）

## Why

`MachineLibraryForm` 的整备页（规格 05-机器整备）从接入第一天起就显示
「装配、载体与就绪数据尚未接入：本栏暂无可显示内容」，并且**没有任何入口能到达它**
（`NavigationPageIndex` 只映射未部署／已部署两页，`Btn_UndeployedMachinesPrepare` 被禁用）。

侦察后的结论是：这一页需要的三栏数据**全部已经在生产里**——

| 规格要求的分区 | 数据来源 | 状态 |
|---|---|---|
| 载体概况：独立实例名称、型号、等级、部署状态；容量与兼容安装位 | `MachineInstance.Name` / `Definition` / `TotalCapacity` / `SlotCount(kind)` | 真实 |
| 组件整备：**实际槽位及组件**；库存候选摘要；一键卸下影响 | `MachineInstance.GetComponent(kind,index)` / `IsComponentWorking` / `MachineRoster.Components` | 真实 |
| 部署准备：硬件配置；算法能力需求；部署解锁条件；出售资格 | `ComputeCapacity`/`LogicCapacity`/`TotalCapacity`；`Integrity`/`Deployed` | 真实（解锁条件除外） |

所以这不是「新建系统」，而是**把已经存在的事实按规格的栏目摆出来**，
外加把它接到入口上。唯一真的缺口是「部署解锁条件」——成长解锁域
（`02-系统设计/08-成长解锁与奖励`）没有创建者，这一行必须**明说**，而不是编一个「已解锁」。

## What Changes

- `MachineDomainSnapshot` 增加整备页三栏（`Carrier` / `Assembly` / `Readiness`），
  由 `MachineReadModel` 从**同一份机器快照**派生。
  为什么放在同一个读模型而不是新建一个：三栏是同一台机器的三个切面，
  让它们各自去问花名册会造出第二条数据路，而这一域的原则是「数据来源唯一」。
- `MachineReadModel` 增加**可选**的 `MachineCatalog`（只为把槽位里的型号显示成名字；
  拿不到就退化成型号编号，**不编造名字**），并提供显式目录的 `MachineReadModels.Create(session, catalog)`。
- `MachineLibraryForm`：整备页三栏渲染真实行；`Btn_UndeployedMachinesPrepare` 接上入口；
  `Btn_MachinePreparationDeploy` 复用已接线的部署去向；
  其余写动作（安装／拆卸／升级／改名／出售）**按真实的下一步**禁用并说明
  （12-选择器＋17-硬件确认 ／ 11-载体升级 ／ 17-重命名 ／ 17-交易确认），而不是一句「未接入」。

## Capabilities

### New Capabilities

- `machine-preparation-readout`：整备页三栏的只读数据与入口。

### Modified Capabilities

- 无。本变更不改界面契约、不改预制体结构、不改 `UIViews` 登记、不改数据表。

## Impact

- 代码：`Assets/Game/Scripts/AutoEra/UI/Integration/MachineReadModel.cs`、
  `Assets/Game/Scripts/AutoEra/UI/MachineLibraryForm.cs`。
- 验收：门1 必须持续 33/33；`run_project_checks.py` 5/5；EditMode/PlayMode 回归全绿；
  新增整备三栏的 EditMode 用例与「整备页渲染真实槽位行」的 PlayMode 用例。
- **不在本变更内**：安装／拆卸／升级／改名／出售的实际执行
  （它们各自要经 12／17／11 家族的对话框，是下一批的垂直切片）。
- **风险点**：整备页在写动作接上之前是**只读**的，所以页面必须把这一点讲清楚，
  不能让玩家以为按钮坏了；这由 `AssemblyMissing` 的原因文本与用例共同保证。
