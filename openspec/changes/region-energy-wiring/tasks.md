# 任务

## 1. 声明型组件：场景对象说自己是什么设施

- [x] 1.1 新增 `RegionEnergyFacility`（MonoBehaviour）：`Kind`（环境能源／燃料发电／蓄电）
      ＋ 额定功率／初始生物质／容量／初始电量／充电许可与目标比例。
      默认值就是设计数值，并在初始化时校验（功率非负有限、容量为正、燃料量非负）。
- [x] 1.2 `Initialize(区域对象身份)` 幂等：实体在一次区域生命周期里可能被重新显示，
      **烧掉的燃料不该被加回来**。重复传入不同身份会被拒绝（身份是持久标识，不是随便一个 id）。
- [x] 1.3 提供 `Generator` / `Storage` / `UpdateEnvironment(worldMs)` / `RemainingBiomass` / `Charge`
      给服务与调试用。燃料只活在实例里（存档接入时再落盘），这里不假装它已经被持久化。

## 2. 区域电网服务

- [x] 2.1 新增 `RegionEnergyService`：**由区域持有**，不是全局单例
      （否则「每个区域有自己的电网、富余电力不外送」立刻不成立）。
- [x] 2.2 `HasSupply` 以「有没有设施」为准：**没有设施的区域不建电网**，
      `Tick` 什么都不做、`ApplySupply` 返回 0、机器供电状态维持原样。
- [x] 2.3 `Register(facility)` 按对象身份登记，重复登记幂等（同一台设施不能被算两次功率）。
- [x] 2.4 `TrackMachine(machine)` 只接受**已部署**的机器；`Reconcile(roster)` 对账：
      新部署的接进来、回收或不在花名册里的摘掉，**只从队列中段移除那一条**，
      其余参与方的进入队列顺序不变——那是同级停机顺序的判据。
- [x] 2.5 `Tick(worldMs, seconds)`：先按世界时间更新环境能源出力（日照周期），再按日照状态结算；
      `ApplySupply()` 把结论落回机器并返回真正变化的台数。
- [x] 2.6 `EnergyGrid` 增加 `RemoveConsumer(id)`（对账需要）。
- [x] 2.7 机器／建筑的默认供电优先级定为 `PowerPriority.Production` 并放在**唯一一处常量**：
      规格说「系统提供默认优先级」但没逐个列出；中枢与信号塔属于更高一档。

## 3. 区域接线

- [x] 3.1 `InitialRegionScene`：实体显示后检查它身上有没有 `RegionEnergyFacility`，
      有就按区域对象身份初始化并登记；**第一个设施出现时才建立服务**。
- [x] 3.2 `Advance` 的推进顺序固定为「先对账 → 再结算 → 再落结论」：
      先结算再对账会让本帧新部署的机器白等一个节拍才通电，先落结论再对账会让离开的机器带着旧结论留下。
- [x] 3.3 `Release` 释放服务；新增只读 `Energy` 供测试与调试。

## 4. 场景声明（编辑器工具 + 预制体）

- [x] 4.1 新增菜单 `Game Framework/AutoEra/Region/声明初始基地能源设施`（幂等）：
      `InitialRegion/Generator.prefab` → 燃料发电（额定 60、初始 10 生物质）；
      `InitialRegion/SolarArray.prefab` → 环境能源（白天 15）。
      另加一条只读检查菜单，报告两个预制体当前声明了什么。
- [x] 4.2 字段只经 `SerializedObject` 写：它们是 private 序列化字段，
      直接赋 C# 属性不会落盘（那会得到「编译通过、预制体里却是默认值」这种最难查的偏差）。
- [x] 4.3 为什么挂**实体预制体**而不是场景对象：场景里那批 `_objects` 只是建造期模板，
      `InitializeRuntime` 会把它们整批停用、再按 `_entityPrefabs` 实例化真正的对象。
      挂在场景对象上会得到「场景里配了、运行时没有」。

## 5. 验收

- [x] 5.1 编译 0 错。
- [x] 5.2 门1 契约自检全绿（**33/33**）；`run_project_checks.py` 5/5（含引用检查，覆盖这次预制体改动）。
- [x] 5.3 新增 `RegionEnergyServiceEditModeTests`（**13/13**）：设施默认值就是设计值；
      燃料与蓄电池用设计数值；没有设施就没有电网；登记幂等；
      燃料初始化幂等（不会把烧掉的燃料加回来）；只有已部署机器参与；
      对账接新摘旧；白天／夜间日照驱动太阳能出力；待命不烧燃料、有负载才按实际输出消耗；
      结论写回机器且**没变化不重播**；空电池也算供电能力（于是缺电停机真的落到机器上）；
      默认优先级取「普通生产」。
- [x] 5.4 新增 `RegionEnergyPlayModeTests`（**1/1，PlayMode 套数 14 → 15**）：
      从 InitialRegion 场景实例化后，区域**自己**建起了电网（2 台设施）；
      部署一台机器后它进入电网、拿到供电、耗电读数与机器逐部件求和一致；
      把世界时间推到夜间 → 太阳能归零、燃料发电机补上缺口、**机器不能因此断电**。
      （第 ④ 步的界面断言见 9.3。）
- [x] 5.5 回归：EditMode 全类、PlayMode **15 套**全绿；门1 33/33 持续绿。

## 6. 边界

本变更不实现：建造／拆除流程（设施目前是场景内容）、
`Btn_HubEnergyLocate`／`Btn_HubEnergyHistory` 两条跨界面入口（进入对应现场／能源停机记录）、
蓄电设施进场景（规格把蓄电池留到蓄电任务之后）、燃料与电量的存档落盘、跨区域输电。

## 7. 风险与兜底

这一批唯一会影响既有玩法的地方是「机器现在真的需要电」。
兜底是 `HasSupply` 这条规则：**没有设施的区域照旧不受影响**；
而初始基地有 15（白天）＋ 60（燃料）功率与 600 电量，足够带动第一版的所有机器。
实测这条兜底有效：32 个 EditMode 类与 15 套 PlayMode 全部保持绿色。

## 8. 界面接线：中枢能源系统详情页（批次 16）

- [x] 8.1 新增 `EnergyReadModel`（`Assets/Game/Scripts/AutoEra/UI/Integration/EnergyReadModel.cs`）：
      供需概要、发电与蓄电设施、用电对象三栏读**同一份结算快照**
      （`RegionEnergyService.Snapshot`）。界面不自己再算一遍功率——两处各算一次，
      迟早会出现「界面说 8.2、停机判定说 5.5」。
- [x] 8.2 `EnergyReadModels.Create(session)` 在会话／世界／电网缺失时返回**诚实空实现**，
      并逐层给出缺什么（缺会话／不在世界里／区域没有声明设施），不伪造供需数字。
      区域没有任何设施时是 `Unavailable` 而不是 `Empty`：「没设施」意味着这一页在建起设施之前
      永远不会有数据，与「域活着但这次没有内容」是两件事。
- [x] 8.3 `AutoEraUiSession` 增加 `RegionEnergy`（第 6 个构造参数），
      `AutoEraWorldProcedure` 把 `InitialRegionScene.Energy` 传进去——界面拿数据的路仍然只有
      「打开参数里的会话」这一条，没有旁路注入点。
- [x] 8.4 写入口只有两个：**燃料设施**的充电许可与目标储电比例。按规格
      「字段控制与确认按钮职责分离」做成**草稿 ＋ `Btn_HubEnergyConfigure` 提交**：
      开关与滑条只改草稿，点配置按钮才写入并由领域复核权限；被拒时保留草稿并写明原因。
      `SupportsChargingPolicy` 为假（环境能源／储能）时控件禁用，原因写在概要正文里。
- [x] 8.5 `Btn_HubOverviewEnergy` 接成这张二级页的入口（能源页没有一级导航按钮）。
- [x] 8.6 契约绑定：`Tools/ui_spec_to_contract.py` 的 `EXTRA_BINDINGS["BaseCommandHubForm"]`
      新增 4 个接入点。Toggle／Slider 不是 `Btn_` 命名，命名规则推导覆盖不到，必须显式声明；
      加完绑定后按既定顺序跑「契约 → 字段脚本 → 编译 → 按契约刷新所有页面绑定 → 门1」。
- [x] 8.7 修掉一个真实缺陷：`EnergyGridSnapshot.StoppedByShortage` 在第一次结算之前是
      `default` 构造出来的 null 列表（该结构自己的构造函数会把 null 规整成空表，
      而 `default` 跳过了构造函数、绕过这条不变式），界面**首次打开就会 `NullReferenceException`**；
      既有解算路径都先 `Tick` 过，所以一直没暴露。改为在取值处兜底、永不返回 null。
- [x] 8.8 状态卡片不再说谎：五个 `Grp_*State` 是**覆盖在内容区上的不透明卡片**
      （520×120、Image alpha=1），而预制体里卡片文案是规格说明列的占位（「Disabled：—」等）。
      只激活状态组、把原因写进正文的旧写法，结果是**占位话盖住真实原因**。
      新增基类 `WriteStateCard(状态组, 原因)`，按结构（状态组内唯一文本）写入，
      由 `ShowPageUnavailable` / `ShowPageEmpty` 直接调用——所有走这两个 helper 的页面
      （本仓大多数「未接线」页面）一次性修好，无需给 33 个 Form 各加五个 SerializeField。
      中枢的能源页与统计页是手写 `SetState` 的，改为显式调用它。
- [x] 8.9 顺带修掉「读取点亮成功卡片」：读一次快照不是提交（规格：未发提交不伪造 success），
      而成功卡片同样是不透明覆盖层，点亮就会把「Success：—」盖在真实数据上。
      中枢能源页与统计页不再点亮成功组；`RenderStatistics` 原先 `!unavailable && !empty`
      就点亮 success，正是这一类错误。
      **未覆盖**：其余手写 `SetState` 的页面（ComponentLibraryForm／ComponentPickerForm／
      ExitFlowForm／MachineLibraryForm／SaveRecoveryForm／SaveSlotsForm／WorldObjectPickerForm／
      AlgorithmEditorForm）仍有同类问题，且其中 `OperationDialogForm`（硬件确认完成）、
      `WorldPlacementForm`（部署成功）、`SaveRecoveryForm`（恢复成功）的 success 是**真实提交结果**、
      属于正确用法——需要逐个判断而不是机械替换，因此留给独立批次：见 §10。

## 10. 后续批次（本变更内部记账，不在本批实现）

- [ ] 10.1 状态卡片文案系统化：给上面列出的手写 `SetState` 站点按语义分别处理——
      读态（Empty／Disabled／Error）写真实原因；success 只在权威提交结果上点亮，并写入真实结果文案。
- [ ] 10.2 `Btn_HubEnergyLocate`（进入对应现场）与 `Btn_HubEnergyHistory`（能源停机记录）
      两条跨界面入口。
- [ ] 10.3 蓄电设施进场景（实体、建造／部署），这是 P5-008／P5-010 的 DoD 依赖。

## 9. 验收（批次 16）

- [x] 9.1 编译 0 错。
- [x] 9.2 新增 `EnergyReadModelEditModeTests`（**14/14**）：三种不可用原因各自可辨且不伪造数据；
      设施／用电对象来自真实电网（用电对象只含**已部署**机器）；概要取结算快照（白天 15、夜间 0）；
      没有储能时说清原因而不是显示 0；选中项带 `SupportsChargingPolicy`；写入真的落到领域；
      不该写时带原因被拒；目标比例钳制到 0..1；刷新**重建快照**并发事件；
      释放后不再回调；重复选中同一台不重绘。
- [x] 9.3 `RegionEnergyPlayModeTests` 扩展第 ④ 步（**1/1**）：中枢能源页读到同一份快照
      （`Ready`、2 台设施、1 个用电对象、设施列表真的渲染出行）；
      拨动开关**不落盘**、点「配置选中发电设施」才写入并落回 `RegionEnergyFacility.Generator`；
      提交成功后草稿清空、按钮回到「无内容可提交」的禁用状态。
- [x] 9.4 门1 契约自检全绿（33/33）。
- [x] 9.5 `AutoEraOperationsUiFormPlayModeTests` 补一条断言（**1/1**）：不带会话打开中枢时，
      能源页走「不可用」通道**并且卡片上是真实原因**，不是预制体里的占位「Disabled：—」；
      成功卡片不得被点亮。
