# 任务

## 1. 目录对外暴露组件展示行

- [x] 1.1 新增 `ComponentDisplayRow`（行 Id、型号、名字、类别、等级、承载／算力／逻辑算力、
      带行为、购入价、回收价、待机／工作功耗、可用性、预制体），并提供
      `KindLabel`（「核心 1 级」）与 `IsReady`。
      （已实现，放在 `Assets/Game/Scripts/AutoEra/Machines/MachineDefinition.cs` 的
      `ComponentDefinition` 旁边——两者是同一份数据的两个视图，分开文件反而让人以为它们无关。
      说明为什么需要它：`ComponentDefinition` 是**装配用**的最小集（型号、类别、能力），
      名字／价格／功耗只存在于数据表里；没有展示行，界面只能显示「型号 2001 等级 1」。
      行 Id 与数据表一致（`ModelId * 10 + Level`，与目录自己的校验规则同源），
      所以从一份运行时定义可以**权威地**反查回展示行，不需要按名字猜。）
- [x] 1.2 `MachineCatalog` 为每一行建立展示行并暴露三个入口：
      `TryGetComponentRow(int rowId)`、`TryGetComponentRow(ComponentDefinition, out …)`、
      `ComponentRows`。
      （已实现。**刻意包含 `PendingConfiguration` / `PendingResource` 的行**：既有循环只把
      「可装配」的行放进 `_components`，如果展示行也只建那些，玩家与排查的人只会看到
      「目录里少了一件东西」——而那条不可用性恰恰是有信息量的。
      `TryGetComponentRow(ComponentDefinition)` 用 `Id * 10 + Level` 反查，
      与 `ValidateCommon` 里的身份规则是同一个式子，所以这不是猜测而是同一份契约。）

## 2. 组件域读模型

- [x] 2.1 新增 `IComponentReadModel` + `UiComponentRow` + `ComponentDomainSnapshot`
      （散件列表、已安装列表、选中详情、三态）与诚实空实现。
- [x] 2.2 真实实现 `RosterComponentReadModel`：订阅 `MachineRoster.Changed`，
      每次变化重建快照；散件来自 `Components` 里 `OwnerId` 无效的实例，
      已安装来自**逐台机器的槽位枚举**。
      （已实现。为什么安装位置从槽位枚举而不是读 `ComponentInstance.OwnerId` 反查：
      实例上只记「属于哪台机器」，**槽位号与类别只有机器自己的槽位表知道**；
      反过来扫槽位一次就能同时拿到机器名、类别与槽位号，也不必在实例上加字段。
      详情同时给目录规格与实例身份——「型号」来自目录、「位置」来自花名册，
      目录查不到型号时**明说缺行**而不是显示一排 0。
      选中项消失（花名册释放、实例不存在）时清掉选中，不继续展示一个不存在的实例。）
- [x] 2.3 `ComponentReadModels.Create(session)` 逐层给出可辨原因：
      没有会话／没有世界／花名册不可用／组件定义表尚未加载；并提供
      `Create(session, catalog)` 显式传入目录。
      （已实现。显式目录入口是必要的：解析数据表是数据表自己的事，调用方可能已经有一份
      （测试、以及将来的运行时目录缓存）；为了测试让所有人都走 `GF.DataTable`
      会把「界面依赖框架全局状态」悄悄种回去。
      异常处理刻意**只吞「表没加载」与「框架未就绪」，不吞 `FormatException`**——
      数据表里的行有问题必须响亮地失败，而不是显示成空目录。）

## 3. 组件库界面接入

- [x] 3.1 `ComponentLibraryForm` 转为手写接入：三页（散件／已安装／组件详情）按
      `Ready`/`Empty`/`Unavailable` 渲染，列表走对象池，点行按稳定 Id 选中。
      （已实现。**「本页为空」与「整域不可用」分开**：前者换一页就有内容，后者怎么点都没有；
      散件页与已安装页各判自己的条数。组件详情的对比栏陈述「还需要选择第二个组件作为参照」。
      写动作（安装／卸载／改装）仍由 `DisableDomainActions()` 按结构名统一禁用，
      列表行不受影响——只读真实、写明确不可用。）
- [x] 3.2 生成器与回归围栏同步迁移：`ComponentLibraryForm` 从 `NOT_WIRED` 移到 `HANDWRITTEN`；
      `AutoEraNotWiredFormsEditModeTests` 的未接入名单移除它、已接入名单加入它。
      （已实现。`NOT_WIRED` 集合与测试名单必须一一对应，否则围栏会指向一个已经不存在的状态。）

## 4. 验收

- [x] 4.1 编译 0 错。
      （`tools/_unity_compile.py` 通过：程序集已更新且 console 无 CS 错误；工程检查第 1 项同结论。）
- [x] 4.2 门1 契约自检 33/33；`run_project_checks.py` 5/5。
      （门1 通过；`run_project_checks.py --port 8090` **5/5 PASS**：
      编译 0 错、悬空引用 0、AppConfigs 12 表/1 配置/3 语言/6 流程、
      AIData 校验 12 成功 0 失败、框架纯度与项目边界通过。
      **本变更没有改界面契约与预制体结构**，门1 通过即是「结构未被触碰」的证据。）
- [x] 4.3 新增 `ComponentReadModelEditModeTests`：不可用三态各自可辨、花名册为空是 Empty、
      实物是散件且详情带真实规格、装上机器后位置与槽位正确、选中跨分组保留、
      选中项消失后清空、清空后刷新不再选回来、空实现的选中是 no-op，
      以及第 6 节的合并显示四条。
      （**13/13 通过**，见 `Assets/Game/Tests/AutoEra/Editor/UI/ComponentReadModelEditModeTests.cs`。
      目录一律从**真实的** `ComponentDefinitions.txt` 解析，与生产同一条路径。）
- [x] 4.4 新增 `ComponentLibraryFormPlayModeTests`：真实运行时下打开组件库，
      断言状态是 `Ready`（不是旧的 Disabled）、散件页渲染出真实行、点行按稳定 Id 选中、
      装上机器后跨页跟随并带上位置。
      （**1/1 通过**。这条用例的价值在于它断言的是**数据**而不是「界面能打开」——
      旧的「未接入」状态同样能让界面打开并通过一个只检查生命周期的用例。）
- [x] 4.5 EditMode / PlayMode 全量回归。
      （EditMode **21 类**全绿：原 20 类 + `ComponentReadModelEditModeTests` 13/13，
      其中 `AutoEraNotWiredFormsEditModeTests` 由 16 项变为 15 项——`ComponentLibraryForm`
      已迁出未接入名单并加入「已接入界面不得带未接入脚手架」那条断言。
      PlayMode **7 套**全绿：原 6 套 + `ComponentLibraryFormPlayModeTests` 1/1。
      进 Play Mode 的三个 EditMode 类全绿：1/1、1/1、6/6。
      门1 **33/33**、`run_project_checks` **5/5**、编译 0 错。）

## 7. 侦察中发现、但不在本变更内的问题（留痕）

- **建筑的定义表引用了不存在的本地化 key。** 为批次 3 的成本评估侦察 `BuildingDefinitions`
  时发现：7 行的 `NameKey` 是 `Building.Warehouse` / `Building.Workshop` / … 这样的键，
  而本地化数据里**只有** `Machines/Hardware`（`Component.*`、`Machine.*`）与
  `Foundation/Startup` 两个字典，**没有任何 `Building.*` 条目**。也就是说：
  ① 按既有的 `MachineCatalog` 口径（`NameKey` 必须能被 `GF.Localization.HasRawString` 解析，
  否则抛 `FormatException`）根本无法为建筑建目录；
  ② 界面拿不到任何建筑显示名。
  显示名确实存在于另一处——`FirstVersionObjects` 表的 7 行 `Category=Building` 有中文名
  （基础仓库／基础制造工坊／生物质发电机／太阳能发电器／基础蓄电池／岸边水泵／传送带），
  顺序与 Prefab 都与定义表一一对应；但那张表自我声明是
  「Resource coverage index only; never grants machine instantiation or gameplay readiness」，
  是**内部覆盖索引**而不是显示来源。
  因此这是一个**内容权威性决策**，不是代码缺陷：要么补齐 7 条 `Building.*` 本地化条目
  （并注册到 `AppConfigs.mLanguages`，因为字典名是显式登记的而不是目录扫描），
  要么明确 `FirstVersionObjects.Name` 就是显示来源。两者都改变数据/内容约定，
  所以本变更**没有**顺手做，而是把它留给用户决策（见下）。
- 结论：`BuildCatalogForm` 的只读接入被这一条挡住；批次 3 因此改做组件域的规格补全（第 6 节）。

## 5. 边界

本变更不实现：安装／卸载／改装的写路径与界面入口、`ComponentPickerForm`、
`UpgradeForm`、组件经济（购入／回收）。

不修改界面契约与预制体结构；不改 `UIViews` 登记；不改数据表。

## 6. 同类未安装组件合并显示（规格补全）

设计依据：`Docs/GameDesign/02-系统设计/01-机器自动化与算法.md` 的组件库一节——
「完全相同的未安装组件可以在组件库列表中按**类型、型号和等级**合并显示数量，
展开后仍保留各组件实例；已安装组件必须显示所在机器和槽位」。
第 1–4 节只做到了后半句（已安装带机器与槽位），前半句没有做。

- [x] 6.1 读模型新增 `UiComponentGroup`（合并键 ＝ 类型＋型号＋等级）与
      `ComponentDomainSnapshot.LooseGroups`，并新增 `SelectGroup(kind, modelId, level)`。
      （已实现。**合并键的三段都必要**：同一型号的不同等级是两种东西（一级可升二级），
      按型号合并会把升级关系显示成「堆了 6 件」。组内成员的稳定身份**逐个保留**
      （`Members`），而不是只留一个计数——只留计数就无法回答「哪一件装到哪台机器」。
      选组与选实例共用同一条选中槽：选一个必然清掉另一个，不会出现两个都「选中」的矛盾态。
      组整组消失（最后一件被装走）时清掉组选中；失败的选择不改变已有选中。）
- [x] 6.2 `ComponentLibraryForm` 散件页改用合并行渲染（`Label` ＝「名字 × 数量」），
      组详情列出成员实例；两页正文改为陈述**本页真实构成与规则**，
      写动作的说明改为引用设计原文（未部署机器在整备环境可安装／拆卸；部署后必须现场交互）。
      （已实现。原先「有内容时」正文写的是「写动作为什么不可用」，对玩家没有信息量；
      现在分别是「散件 N 件（合并为 M 行）…」与「已安装 N 件：每件都显示所在机器与槽位…」。）
- [x] 6.3 测试：合并成一行且个体数不变、同型号不同等级**不**合并、选组后详情给出数量与全部成员、
      选实例让位组选中、未知组选择失败且不改动已有选中、空实现新增方法同样是 no-op。
      （`ComponentReadModelEditModeTests` 由 10 项升至 **13/13 通过**；
      `ComponentLibraryFormPlayModeTests` 改为造两件相同散件，断言合并行数 1／个体数 2、
      点合并行激活组选中、装走一件后仍是 1 行，**1/1 通过**。）
