# 存档：三份滚动备份与内容校验（P7-002 剩余部分）

## Why

`SaveSlotService` 早就有了「临时文件 ＋ `File.Replace` 原子替换 ＋ 读取回退备份」，
所以 P7-002 的「原子写入」这一半是真的做完的。剩下的两半没有做，而任务名里都写着：

| 规格原文（`02-系统设计/10-存档与状态持久化.md`） | 实现现状 | 结论 |
|---|---|---|
| 「每个进度槽内部保存 1 份当前正式存档和**最近 3 份滚动自动备份**」 | 只有一份 `slot_n.json.bak` | **真缺口**：连续两次保存都写坏时，唯一还能救回进度的东西正是第 2、3 份 |
| 「每份存档包含格式版本、槽位ID、保存UTC时间、数据长度、**校验值**和离线结算进度标记」 | 记录里只有版本、世界时间、摘要、内容 | **真缺口**：没有校验值，手改过的存档与位翻转都会被当成好存档读进来 |
| 「读取依次尝试正式存档和三份由新到旧的备份。**使用备份时告知玩家恢复时间点，不静默回退**」 | `Read` 回退到备份后仍然报 `Success` | **真缺口**：列表、详情与状态灯会把「正在读一份旧备份」显示成「一切正常」——那正是静默回退 |
| 「删除进度必须二次确认，并明确正式存档与**三份内部备份都会被删除**」 | `Delete` 删了主文件、`.bak` 与 `.tmp` | 随备份数量一起修 |

另外两条也顺手对齐：`RestoreFromBackup` 原来只认「唯一那份备份」，
现在要认「**最新一份可用**备份」——存在一个读不出来的备份文件不代表恢复可行，
界面据此点亮按钮就会让玩家点下去才发现救不回来。

## What Changes

### 数据格式（版本 1 → 2）

- `SaveSlotRecord` 增加 `SlotIndex`、`SavedUtcTicks`、`ContentLength`、`Checksum`、
  `OfflineSettlementPending`，`CurrentVersion = 2`。
- 新增 `SaveChecksum`：SHA-256 取小写十六进制，**按显式规范化载荷**计算而不是哈希序列化后的 JSON。
  理由：JSON 的字段顺序、缩进与转义都会随序列化器实现变化，那样算出来的校验值会在一次无害的
  重序列化之后全部失效——把「换个写法」误报成「存档损坏」。
- **旧格式仍然能读**：`Version < 2` 的记录没有校验值，按迁移入口接受。
  一次格式升级不该把玩家所有旧存档判成损坏。

### 服务（三份滚动备份）

- `SaveSlotService.BackupCount = 3`，路径 `slot_n.json.bak1|bak2|bak3`（**1 最新**）。
- 写入顺序按规格：序列化 → 写临时文件 → **校验临时文件** → 轮转备份 → 原子替换。
  校验发生在替换之前，所以一个写坏的临时文件永远不会变成正式存档。
- 滚动规则：`bak2→bak3`、`bak1→bak2`（`bak3` 丢弃），被替换下来的正式存档成为新的 `bak1`。
  第一次保存不产生备份（没有「被替换下来的旧内容」）。
- `Read`：正式存档 → 三份备份由新到旧；**用备份时返回独立状态 `RecoveredFromBackup`
  并带上 `BackupIndex`**。`IsSuccess` 涵盖它（调用方问「有没有内容」），
  `IsPrimary` 只对正式存档为真（调用方问「是不是没回退」）。
  版本过新是**确定结论**，不允许被备份悄悄盖过去。
- 逐份校验：JSON 可解析、版本不高于当前、`SlotIndex` 与文件名一致、
  以及（版本 2 起）`ContentLength` 与校验值都对得上。
- `Delete` 删主文件、临时文件与三份备份（否则「已删除的进度」还能被恢复出来）。
- `HasBackup`（存在任何一份）与 `HasUsableBackup`（可读且通过校验）分开：
  后者才是「恢复」能不能点的判据。
- `ListBackups(slot)` 给恢复页逐份列出时间与可用性。
- `RestoreFromBackup` 提升**最新一份可用**备份；提升后删掉比它新的备份（按定义都不可用）、
  比它旧的依次前移，于是「序号 1 最新」与「链上都是可用内容」同时成立。
- 构造函数增加可注入的 UTC 时钟（默认 `DateTime.UtcNow`），便于测试断言保存时间。

### 界面（不静默回退的落点）

- 新增 `SaveSlotNarrative`：存档状态的**统一说法**（状态、行摘要、来源、备份时间、
  恢复说明）。它放在存档域而不是某个 Form 里——这些句子会同时出现在存档列表、槽位详情与
  恢复页三处，分散写就会三处各说一套，而任意一处说成「正常」都构成静默回退。
- `SaveSlotReadModel`：列表行对 `RecoveredFromBackup` 给出「可读，但来自备份 N（时间）」并标
  `NeedsRecovery`；详情增加「保存时间」「本次读取来源」「可用备份」三项。
- `SaveRecoveryForm`：诊断栏逐份列出三份备份（规格要求说明「有效备份时间」）；
  候选栏给出摘要、世界时间、保存时间与来源；正文由
  `SaveSlotNarrative.DescribeRecovery` 生成，**说清恢复到哪一份、时间点、预计损失范围**；
  「恢复」可点性改用 `HasUsableBackup`。

## Capabilities

### New Capabilities

- `save-rolling-backups-and-checksum`：三份滚动备份、内容校验、旧格式迁移入口与「不静默回退」。

### Modified Capabilities

- `ui-game-system-integration` 的 `SaveSlotsForm`/`SaveRecoveryForm` 部分：存档状态的说法统一到
  `SaveSlotNarrative`，且在读备份时必须显式说出来。

## Impact

- 存档域：`Save/SaveSlotRecord.cs`、`Save/SaveSlotReadResult.cs`、`Save/SaveSlotService.cs`、
  `Save/SaveChecksum.cs`（新）、`Save/SaveSlotNarrative.cs`（新）。
- 界面：`UI/Integration/SaveSlotReadModel.cs`、`UI/SaveRecoveryForm.cs`。
- 验收：门1 必须持续全绿（**不改契约**）；`run_project_checks.py` 5/5；
  EditMode 全类、PlayMode 12 套全绿；
  新增 `SaveRollingBackupEditModeTests` **15/15**，更新
  `SaveSlotServiceEditModeTests`（12/12，备份路径改为 `bak1`）与
  `SaveSlotReadModelEditModeTests`（10/10，新增「读备份不得显示成正常」一条）。
- **不在本变更内**：P7-003…P7-012 的状态快照与离线推进（那些要等领域系统完成）、
  云存档与跨版本迁移系统（规格明确第一版不做）。
- **风险点**：把 `RecoveredFromBackup` 折进 `Success` 是最容易犯的错，
  它不会报错，只会让玩家以为在玩最新进度。用例
  `Read_CorruptMainFile_FallsBackToBackupAndSaysSo`、
  `ReadingFromBackup_IsVisibleOnTheRowAndInTheDetail_NotHiddenAsNormal` 与
  `Narrative_TellsThePlayerWhichBackupAndWhatWillBeLost` 三处各守一层。
