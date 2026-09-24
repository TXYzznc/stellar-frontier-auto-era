# 区域电网结算（能源域的第一个切片）

## Why

能源是「其余 12 类未接入领域」里**设计最完整、依赖最少**的一个：`06-能源储存与物流.md`
（415 行）把公式、供电顺序、设施基准数值、优先级、昼夜、缺电停机与恢复都定死了，
而代码里一行都没有——`MachineInstance.UpdateEnvironment(supply, signal)` 的注释写着
「External adapters supply facts; **this is not an energy grid or signal simulator**」。

也就是说：**现在「机器有没有电」这件事在实现里根本不存在**，只有外部喂进来的两个 bool。
这一批补的就是那个缺失的结算器本身。

选它作为一批而不是「顺手多做一点」，是因为它的每一条规则都可以手算复核
（电量 ＝ 功率 × 秒 ÷ 60），因此可以给出真正硬的验收；而它依赖的数据（功率／电量）
也已经由设计表给出，不需要先补内容。

## What Changes

### 领域：`Assets/Game/Scripts/AutoEra/Energy/`

- `EnergyContracts.cs`：`PowerPriority`（关键设备／普通生产／次要生产／可暂停设备）、
  `GeneratorKind`（环境能源／燃料）、`StorageState`，以及三个参与方接口
  `IEnergyGenerator` / `IEnergyStorage` / `IEnergyConsumer`。
- `EnergyGrid.cs`：
  - **结算公式** 电量变化 ＝（发电功率 − 耗电功率）× 经过秒数 ÷ 60，按真实经过时间连续结算，
    **不在 4 分钟标准周期边界跳变**；
  - **固定供电顺序**：免费环境能源 → 已开启的燃料发电站（按建造顺序依次承担剩余需求，
    实际输出 ＝ min(额定功率, 分配到的需求)）→ 蓄电池放电 → 按供电优先级停机；
  - 燃料发电**默认不主动为储能充电**；开启许可并设置目标比例后，只充到目标比例为止，随后自动
    回到只满足实时负载；**实时负载始终优先于任何充电需求**；
  - **同一时刻蓄电池不能同时充放电**；
  - 缺电时从最低优先级开始停机，同级按进入供电队列的时间**后进先停**（于是先进的先恢复）；
    每次结算重新判定，**缺电结论不延续**；
  - 输出 `EnergyGridSnapshot`：发电／耗电／净功率／储电／上限／充电／放电／舍弃的盈余／
    因缺电停机的对象／是否缺电，以及按净功率估算的耗尽或充满时间
    （**净功率非负时「剩余耗尽时间」如实给 null**，规格原文如此）。
- `DaylightCycle`：一昼夜 24 分钟，**16 分钟日照、8 分钟无日照**；
  周期与世界时间同起点（世界时间 0 即日出），所以「新存档从白天早期开始」不需要偏移量。
- `FirstVersionEnergy` 与三个设施：初始太阳能发电器（白天 15 功率）、
  基础生物质发电机（额定 60 功率、1 生物质 ＝ 60 电量、保存已投入未消耗的燃料）、
  基础蓄电池（240 电量、无充放电功率上限与损耗）。

### 刻意没做的事

- **不接场景、不接数据表、不做能源界面**：设施还是纯领域对象，没有实体、没有建造流程。
  「把机器的功率数据接进来」需要先给 `MachineDefinitions`／`ComponentDefinitions` 加功率列
  （设计表已给出数值），那是下一批的事——本批先把结算器做成能被手算复核的东西。
- 不实现跨区域输电、电线、损耗、充放电功率上限、最低储能保护、玩家可编辑发电顺序
  （规格明确第一版都不做）。

## Capabilities

### New Capabilities

- `energy-grid-settlement`：区域电网的连续结算、固定供电顺序、储能与确定性缺电停机。

### Modified Capabilities

无（这一批不碰界面与契约）。

## Impact

- 产品层：新增 `Assets/Game/Scripts/AutoEra/Energy/`（3 个文件，纯 C#，无 Unity 依赖）。
- 验收：门1 必须持续全绿（**不改契约、不改预制体**）；`run_project_checks.py` 5/5；
  EditMode 全类、PlayMode **13 套**全绿；
  新增 `EnergyGridEditModeTests` **17/17**。
- 对应任务：P5-006（区域功率与电量连续结算）实现；P5-008（储能）与 P5-009（供电优先级）
  的**结算部分**随同落地，但它们的设施对象与界面仍未接。
- **风险点**：这一批的错法都是「不报错但算错」——
  把储能当第六路并列（不先扣负载）、燃料发电机被当成「开着就满额输出」、
  充电需求与实时负载抢电、缺电结论跨结算延续导致对象永久停机。
  用例逐条钉住：`FuelGeneratorOnlyCoversWhatEnvironmentEnergyMissed`、
  `LoadIsServedBeforeAnyChargingNeed`、`FuelChargingIsOffByDefault_AndOnlyFillsUpToTheTargetRatio`、
  `SamePriorityStopsTheLatestQueuedFirst_AndTheConclusionDoesNotPersist`。
