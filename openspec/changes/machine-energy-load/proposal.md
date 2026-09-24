# 机器接成电网负载（功率数据接通与供电回写）

## Why

批次 13 把区域电网的结算器做出来了，但它当时**没有任何真实负载**：参加结算的对象只能是
测试里手搓的 `EnergyConsumer`。这一批把机器接上去。

侦察发现的第一件事让这一批比预想的小得多：**功率数据早就在数据表里**。
`MachineDefinitions` 与 `ComponentDefinitions` 都有 `IdlePower`／`WorkingPower` 两列，
值也对得上设计（旋转载体 0.1／1.0、计算核心 0.3／3.0、传感器 0.2／2.0），
`MachineCatalog` 甚至在 `ValidateCommon` 里校验了它们（含 `working < idle` 判非法）——
**但构造运行时定义时没有传下去**：`new MachineDefinition(...)` 到 `prefab` 为止，
`new ComponentDefinition(...)` 到 `hasBehavior` 为止。于是「这台机器耗多少电」在工程里根本算不出来。

所以这一批做两件事：把已经存在的功率数据接通，并把机器变成电网的负载（含结论回写）。

## What Changes

### 数据接通（不改数据、只改解析）

- `MachineDefinition` 增加 `IdlePower`／`WorkingPower`（构造参数带默认值，既有调用不受影响）；
  `ComponentDefinition` 同理。
- `MachineCatalog` 把已经校验过的两列传进运行时定义。
- 定义构造时同样校验：非负、有限、`working >= idle`（与目录的规则同一套）。

### 机器耗电：按部件逐项求和

- `MachineInstance` 新增：
  - `IsMoving`（由执行上下文推送）与 `SetComponentActivity(componentId, active)`／
    `IsComponentActive`（每个效应器各自的执行状态）；
  - `IdlePowerDraw`（全部部件按待机的耗电）与 `CurrentPowerDraw`（按**各部件当前活动**求和）。
  - 规则：载体在移动时用稳定功率；传感器在未休眠且启用时**持续采样**用稳定功率；
    计算核心在有算力占用（正在运行算法）时用稳定功率；效应器在正在执行动作时用稳定功率；
    机器休眠时全部按待机；已损坏时为 0。
  - `CurrentPowerDraw` **不看当前供电状态**：如果它按「现在有没有电」算功率，
    一台断电的机器就会请求 0 功率、于是永远拿不回供电——一个闭环死锁。
    「断电机器能耗归零」说的是**实际消耗**，那由电网在结算时置 0。
  - `UpdateSupply(bool)`：只写供电，不碰区域信号。
- `MachineExecutionContext.Synchronize()` 推送两件事：移动状态（`_navigationActive`）与
  每个效应器绑定各自的执行状态。

### 电网契约的一处修正

- `IEnergyConsumer` 从「`StandbyPower` ＋ `WorkingPower` ＋ `IsWorking`」改为**单一
  `RequestedPower`**。
  理由：规格明确写「整台机器是否处于工作负载必须按照各部件当前活动分别求和，
  **不能用一个整机倍率替代**」——一台机器完全可能是「核心在跑算法、效应器待机、传感器持续采样」
  这种混合状态，两个点表示不了它。旧契约会把实现推向规格禁止的做法。
- `EnergyConsumer`（建筑用的简易实现）保留待机／稳定两个字段，并把 `RequestedPower` 派生出来。

### 适配层

- 新增 `AutoEra.Energy.MachineEnergyConsumer`：把一台机器接成电网负载。
  - `IsDemandActive` ＝ 已部署 ＋ 现场电源开关未关（库中机器与已断电机器的**请求**为 0，
    否则电网会把「玩家关掉的设备」当成真实缺口去停别的设备）；
  - `RequestedPower` 直接取自 `MachineInstance.CurrentPowerDraw`（适配层只转交，不重新解释）；
  - `ApplySupply()` 把电网结论写回机器，**只在结论变化时**写（机器上挂着一堆订阅者，
    无条件通知会把「供电没变」变成一次全量刷新）。

## Capabilities

### New Capabilities

- `machine-energy-load`：机器耗电的逐部件求和模型与它作为区域电网负载的接入。

## Impact

- 机器域：`Machines/MachineDefinition.cs`、`MachineCatalog.cs`、`MachineInstance.cs`、
  `MachineExecutionContext.cs`。
- 能源域：`Energy/EnergyContracts.cs`（契约修正）、`Energy/FirstVersionEnergyFacilities.cs`、
  `Energy/MachineEnergyConsumer.cs`（新）。
- 验收：门1 必须持续全绿（**不改契约、不改预制体**）；`run_project_checks.py` 5/5；
  EditMode 全类、PlayMode **13 套**全绿；
  新增 `MachineEnergyConsumerEditModeTests` **17/17**；`EnergyGridEditModeTests` 17/17 保持全绿。
- **不在本变更内**：把电网**建成区域服务**并接进场景（初始基地那台太阳能与生物质发电机
  目前还不是世界对象；在它们存在之前把电网设成供电权威，会让所有机器一夜之间断电）。
  那是下一批的事，而且它取决于「初始基地设施」这个内容项。
- **风险点**：这一批的错法同样是「不报错但算错」——
  用整机倍率代替逐部件求和（用例 `PerComponentSummingIsNotAMachineWideMultiplier` 守着）、
  让请求功率依赖当前供电状态（会造成断电机器永远无法恢复的死锁，
  `UnpoweredMachineStillRequests_SoItCanRecover` 守着）、
  回写时顺手覆盖区域信号（`ApplySupplyNeverTouchesTheRegionalSignal` 守着）。
