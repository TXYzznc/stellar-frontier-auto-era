# 警报系统（三级警报、合并与恢复）

## Why

警报在仓库里有两处空壳：`EventDomain.Alert` 从来没有生产者，`AlertForm` 是一张「未接线」的页面，
HUD 的警报摘要模块与中枢的「活跃警报」栏都没有数据源。而 P6-011 的完成定义写得很具体：
**「同源警报不刷屏，恢复后保留历史且可定位责任对象」**。

规格同时划了三条硬边界，这一批按它们设计：

| 规格原文 | 对实现的约束 |
|---|---|
| 同类型、来源与目标合并 | 账本按（种类，来源）保存**一行一问题**，条件持续存在期间零写入 |
| 恢复后转历史 | 恢复由**真实条件**决定，不是玩家点掉 |
| 无手动清除故障或重置真实状态按钮 | 界面唯一能改的是「已读」，账本不提供 Resolve 接口给界面 |

另外一条来自本次侦察：**只有已经存在的领域事实才能报警**。能源侧（缺电停机、电量耗尽、燃料耗尽）
与机器完整度都已经能读出真值，所以当时挑了这四类。**这个挑选后来被证明是错的**：
2026-09-21 核对权威文档后发现判据一直都在（队列容量与两档预警见 `01-机器自动化与算法` §队列容量与溢出
与 DEC-102；算法变红边界见 DEC-104），真正缺的是**一份警报目录**——文案在文案规格、判据散在各系统文档与
决策记录里，没有一处能让人一次读全「哪些问题算警报、哪一级、什么时候算恢复」。
目录现已补齐：`Docs/GameDesign/02-系统设计/15-警报系统.md`（17 条），
并修正了这批实现里的四处自造（见 `tasks.md` 第 8 节与 DEC-201）。

## What Changes

### 警报目录（文案与元数据的唯一来源）

- 新增 `AlertSeverity`（提醒／警告／严重，数值可比大小：中枢与 HUD 都要取「最高等级」）、
  `AlertState`（活跃／已恢复）、`AlertKind`（缺电停机／电量耗尽／燃料耗尽／机器完全损坏）。
- `AlertCatalog` 给每一类警报：等级、来源、目标、触发原因、影响，以及**真实恢复条件**。
  写不出真实恢复条件的问题就不该被做成警报——那是这一层的准入条件，也有测试钉住。

### 警报账本

- 新增 `AutoEraAlertService`：**一行一问题**（合并键＝种类＋来源）。
  - `Raise`：条件出现。持续存在期间**零写入**（次数是「发生过的段数」，不是轮询次数——
    否则一条整夜停机能把次数刷到几万）；恢复之后再次出现＝同一行重新活跃、次数 +1、重新未读。
  - `Resolve`：真实条件不成立，转历史并记下恢复时间。
  - `MarkRead`：只改阅读状态，不改变警报本身。
  - 排序由账本给（先活跃，活跃内部等级降序、最近发生在前；历史按恢复时间倒序），
    界面不自己排——否则中枢与 HUD 会排出两个不同的「最要紧的那条」。
- `HighestActiveSeverity` / `ActiveCount` / `UnreadCount` 供 HUD 摘要与中枢复用。

### 生产者（警报的判据是真值，不是消息）

- 新增 `RegionAlertMonitor`：每个节拍读一遍机器与设施，只在跨越时让账本变化。
  它不订阅任何事件、也不缓存判断结果——漏一条消息不会让警报停在错误状态。
- 缺电停机用「电网因缺电把它停了」（`MachineEnergyConsumer.IsStoppedByShortage`），
  而不是「它现在没电」：后者在刚部署、还没轮到结算时也成立，会把「还没接上」报成一次事故。
- `InitialRegionScene` 在结算之后推进警报，并把真实的跨越写进事件日志（`EventDomain.Alert`）；
  账本与区域同寿命（跨区域沿用会把 A 区的故障算到 B 区头上）。
- 区域警报通过会话（`AutoEraUiSession.RegionAlerts`）交给界面，仍是「打开参数」这一条唯一通路。

### 读模型

- 新增 `AlertReadModel`：三态分明（缺会话／不在世界里／区域未就绪各说各的；账本活着但没有警报是
  Ready＋空列表，不是 Unavailable）；行给出来源名、等级、状态、次数与时间；`AlertDetails` 给出规格
  要求的详情字段。写入口**只有标记已读**。

## Capabilities

### New Capabilities

- `alert-system`：警报目录、账本、生产者与读模型（界面接线见 tasks 第 5 节）。

## Impact

- 产品层：`Alerts/AlertContracts.cs`（新）、`Alerts/AutoEraAlertService.cs`（新）、
  `Alerts/RegionAlertMonitor.cs`（新）、`World/Region/InitialRegionScene.cs`、
  `UI/Integration/AutoEraUiSession.cs`、`UI/Integration/AlertReadModel.cs`（新）、
  `UI/Integration/AutoEraUiFormat.cs`（等级显示名）、`Procedures/AutoEraWorldProcedure.cs`。
- 验收：编译 0 错；门1 33/33；`run_project_checks.py` 5/5；
  新增 `AutoEraAlertServiceEditModeTests` **9/9**、`RegionAlertMonitorEditModeTests` **9/9**、
  `AlertReadModelEditModeTests` **9/9**；EditMode／PlayMode 回归全绿。
- **不在本变更内**（留给界面批次）：`AlertForm` 页面接线（筛选、标记已读、定位、相关详情）、
  HUD 警报摘要模块、中枢总览「最近活跃警报」、中枢待办「活跃警报」分类、中枢统计的警报历史栏。
- **明确不做**：队列溢出与算法异常的警报（缺判据）、任何形式的「清除故障」入口。
