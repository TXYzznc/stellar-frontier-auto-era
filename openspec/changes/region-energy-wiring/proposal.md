# 区域电网接线（初始基地真的开始供电）

## Why

批次 13 把电网结算器做出来了，批次 14 把机器接成了负载——但**没有任何地方建过一张电网**。
机器有没有电仍然由调用方手动 `UpdateEnvironment(true, true)` 决定，而 `RegionInputModule`
之外没有任何生产路径会去问「这台机器现在有没有电」。

这一批补的就是那一句「谁拥有电网、谁参与、结论怎么落回机器」。

侦察发现两件让这一批比预想小的事：

| 原以为 | 实际 |
|---|---|
| 初始基地的发电设施还不存在，要先做成世界对象 | **场景里早就有**：`InitialRegionSceneBuilder` 建的 7 个对象里就有「生物质发电机」与「太阳能阵列」，而且是 `PersistentObjectKind.Building` 的区域对象 |
| 需要新建承载设施参数的组件与数据 | 只需要一个**声明型组件**：额定功率／初始燃料／容量都是内容，挂在实体预制体上即可 |

另外一条必须遵守的结构事实：场景里那批 `_objects` 只是**建造期模板**，
`InitializeRuntime` 会把它们整批 `SetActive(false)`、再按 `_entityPrefabs` 实例化真正的对象。
所以能力声明必须挂在**实体预制体**上——挂在场景对象上会得到「场景里配了、运行时没有」。

## What Changes

### 声明型组件

- 新增 `RegionEnergyFacility`（MonoBehaviour，挂在实体预制体上）：`Kind`（环境能源／燃料发电／
  蓄电）＋ 额定功率／初始生物质／容量／初始电量／充电许可与目标比例。
  默认值就是设计数值（太阳能 15、生物质 60 与 10 生物质、蓄电池 240），
  并在初始化时校验（功率非负有限、容量为正）。
  燃料是**运行期状态**：`Initialize` 幂等，实体重新显示不会把烧掉的燃料加回来。

### 区域电网服务

- 新增 `RegionEnergyService`：
  - **由区域持有**（不是全局单例）——否则「前线电网独立运行、每个区域有自己的电网」立刻不成立；
  - `HasSupply` 以「有没有设施」为准：**没有设施就不建电网**，`Tick` 什么都不做、
    机器供电状态维持原样。接线不该顺手把还没接内容的世界改成一片漆黑；
  - `Register(facility)` 按对象身份登记，重复登记幂等；
  - `TrackMachine(machine)` 只接受**已部署**的机器（库中机器不占区域功率）；
  - `Reconcile(roster)` 对账：新部署的接进来、回收或移除的摘掉，**只从队列中段移除那一条**，
    其余参与方的进入队列顺序不变（那是同级停机顺序的判据，重建会让它变成时间的函数）；
  - `Tick(worldMs, seconds)` 先按世界时间更新环境能源出力，再按日照状态结算；
  - `ApplySupply()` 把结论落回机器，返回真正发生变化的台数。
- `EnergyGrid` 增加 `RemoveConsumer(id)`（对账需要）。
- 机器／建筑的默认供电优先级定为 `PowerPriority.Production`（规格说「系统提供默认优先级」
  但没逐个列出；取「普通生产」，中枢与信号塔属于更高一档），常量只有这一处。

### 区域接线

- `InitialRegionScene`：实体显示时看它身上有没有 `RegionEnergyFacility`，
  有就 `Initialize(区域对象身份)` 并登记进电网；**第一个设施出现时才建立服务**。
  `Advance` 里按「先对账 → 再结算 → 再落结论」推进电网；`Release` 释放。
  新增只读 `Energy` 供测试与调试。

### 场景声明

- 新增编辑器菜单 `Game Framework/AutoEra/Region/声明初始基地能源设施`（另有一条只读检查菜单）：
  给 `InitialRegion/Generator.prefab` 声明燃料发电（额定 60、初始 10 生物质）、
  给 `InitialRegion/SolarArray.prefab` 声明环境能源（白天 15）。
  字段只能经 `SerializedObject` 写——它们是 private 序列化字段，直接赋 C# 属性不会落盘。

## Capabilities

### New Capabilities

- `region-energy-wiring`：区域电网的持有者、参与方、对账与结论回写，以及初始基地的设施声明。

## Impact

- 产品层：`World/Region/RegionEnergyFacility.cs`（新）、`World/Region/RegionEnergyService.cs`（新）、
  `World/Region/InitialRegionScene.cs`、`Energy/EnergyGrid.cs`（`RemoveConsumer`）。
- 工具：`Editor/RegionEnergyFacilitySetup.cs`（新菜单）。
- 资产：`Assets/Game/Prefabs/Entity/InitialRegion/Generator.prefab`、
  `SolarArray.prefab`（各加一个组件；**不改 UI 预制体与契约**）。
- 验收：门1 必须持续全绿（33 契约）；`run_project_checks.py` 5/5（引用检查会覆盖这次预制体改动）；
  EditMode 全类、PlayMode **14 套**全绿；
  新增 `RegionEnergyServiceEditModeTests` **13/13** 与 `RegionEnergyPlayModeTests` **1/1**。
- **不在本变更内**：建筑/设施的建造与拆除流程、能源界面（P5-010）、蓄电设施进场景
  （规格把蓄电池留到蓄电任务之后）、燃料与电量的存档落盘、跨区域输电。
- **风险点**：这一批唯一会影响既有玩法的地方是「机器现在真的需要电」。
  风险由 `HasSupply` 这条规则兜住：**没有设施的区域照旧不受影响**；
  而初始基地有 15（白天）＋ 60（燃料）功率与 600 电量，足够带动第一版的所有机器。
