using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoEra.Save;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 三份滚动备份与内容校验（规格 10-存档与状态持久化：P7-002 的剩余部分）。
    ///
    /// 这一批要修掉的旧行为是「只有一份备份」：规格写的是
    /// 「每个进度槽内部保存 1 份当前正式存档和**最近 3 份滚动自动备份**」，
    /// 而旧实现只留 `slot_n.json.bak` 一份。少掉的两份不是冗余——它们正是「连续两次保存都写坏了」
    /// 这种真实故障下唯一还能救回进度的东西。
    ///
    /// 另外三条守则各自的错法都很具体：
    ///   * **写入顺序**：规格是「写临时文件 → 校验 → 轮转备份 → 原子替换」。
    ///     先替换再校验的话，一个写坏的临时文件会直接变成正式存档。
    ///   * **校验值**：它必须覆盖内容，也必须覆盖「内容被截断且长度字段没跟着改」这种情况。
    ///   * **不静默回退**：读备份必须是独立状态。报 Success 的话，
    ///     列表、详情与状态灯会把「正在读一份旧备份」显示成「一切正常」。
    /// </summary>
    public sealed class SaveRollingBackupEditModeTests
    {
        private string _root;
        private SaveSlotService _service;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            _root = Path.Combine(Path.GetTempPath(), "AutoEraRollingBackupTests", Guid.NewGuid().ToString("N"));
            _now = new DateTime(2026, 9, 21, 10, 0, 0, DateTimeKind.Utc);
            _service = new SaveSlotService(_root, () => _now);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        /// <summary>连续保存 n 次，每次内容不同，便于识别落在哪一份备份里。</summary>
        private void SaveVersions(int count)
        {
            for (int version = 1; version <= count; version++)
            {
                _now = _now.AddMinutes(1);
                Assert.That(_service.Overwrite(0, "v" + version, version * 100L, "c" + version), Is.True);
            }
        }

        [Test]
        public void FirstSaveCreatesNoBackup_BecauseThereIsNothingToBackUp()
        {
            Assert.That(_service.Overwrite(0, "v1", 100L, "c1"), Is.True);

            Assert.That(_service.HasBackup(0), Is.False,
                "第一次保存没有「被替换下来的旧内容」，凭空造一份备份只会是同一份内容的副本。");
            Assert.That(_service.Read(0).IsPrimary, Is.True);
        }

        [Test]
        public void RollingBackupsKeepTheThreeMostRecentPreviousSaves()
        {
            SaveVersions(5);

            // 正式存档 = v5；备份 1/2/3 = v4/v3/v2；v1 已被滚动丢弃。
            Assert.That(_service.Read(0).Record.Summary, Is.EqualTo("v5"));
            Assert.That(SummaryOfBackup(1), Is.EqualTo("v4"), "备份 1 永远是最新的一份。");
            Assert.That(SummaryOfBackup(2), Is.EqualTo("v3"));
            Assert.That(SummaryOfBackup(3), Is.EqualTo("v2"));

            string fourth = _service.GetSlotPath(0) + ".bak4";
            Assert.That(File.Exists(fourth), Is.False, "只保留三份，不无限增长。");
        }

        [Test]
        public void Read_SkipsUnusableNewestBackup_AndReportsWhichOneItUsed()
        {
            SaveVersions(4);
            // 备份 1 是 v3，故意写坏它；可用的最新一份应是备份 2（v2）。
            File.WriteAllText(_service.GetBackupPath(0, 1), "garbage");
            File.WriteAllText(_service.GetSlotPath(0), "garbage");

            SaveSlotReadResult result = _service.Read(0);

            Assert.That(result.Status, Is.EqualTo(SaveSlotReadStatus.RecoveredFromBackup));
            Assert.That(result.BackupIndex, Is.EqualTo(2));
            Assert.That(result.Record.Summary, Is.EqualTo("v2"));
        }

        [Test]
        public void Checksum_RejectsContentThatWasEditedByHand()
        {
            SaveVersions(1);
            string main = _service.GetSlotPath(0);

            // 只改内容、不动校验值：这正是「手改存档」与「磁盘位翻转」的共同形态。
            string tampered = File.ReadAllText(main).Replace("\"c1\"", "\"c1-tampered\"");
            File.WriteAllText(main, tampered);

            Assert.That(_service.Read(0).Status, Is.EqualTo(SaveSlotReadStatus.Corrupt),
                "校验值不覆盖内容，等于没有校验。");
        }

        [Test]
        public void Checksum_RejectsATruncatedContentLength()
        {
            SaveVersions(1);
            string main = _service.GetSlotPath(0);
            string tampered = File.ReadAllText(main).Replace("\"ContentLength\": 2", "\"ContentLength\": 99");
            File.WriteAllText(main, tampered);

            Assert.That(_service.Read(0).Status, Is.EqualTo(SaveSlotReadStatus.Corrupt));
        }

        [Test]
        public void RecordCarriesSlotIdentityTimeAndSettlementFlag()
        {
            Assert.That(_service.Overwrite(1, "带标记", 250L, "content", offlineSettlementPending: true), Is.True);

            SaveSlotRecord record = _service.Read(1).Record;
            Assert.That(record.Version, Is.EqualTo(SaveSlotRecord.CurrentVersion));
            Assert.That(record.SlotIndex, Is.EqualTo(1));
            Assert.That(record.ContentLength, Is.EqualTo("content".Length));
            Assert.That(record.HasChecksum, Is.True);
            Assert.That(record.OfflineSettlementPending, Is.True);
            Assert.That(record.SavedUtc, Is.EqualTo(_now),
                "保存UTC时间必须可读：恢复界面要告诉玩家恢复到哪个时间点。");
        }

        [Test]
        public void SlotIdentityIsChecked_SoACopiedFileIsNotAcceptedAsAnotherSlot()
        {
            SaveVersions(1);
            string main = _service.GetSlotPath(0);
            File.Copy(main, _service.GetSlotPath(2));

            Assert.That(_service.Read(2).Status, Is.EqualTo(SaveSlotReadStatus.Corrupt),
                "把别的槽位的文件复制过来不是「一份能读的存档」——那等于用一个文件冒充两个世界。");
        }

        [Test]
        public void LegacyVersionOneRecord_IsStillReadable()
        {
            // 格式 1 的记录没有校验值、没有槽位ID。格式升级不该把玩家的旧存档判成损坏。
            Directory.CreateDirectory(_root);
            File.WriteAllText(_service.GetSlotPath(0),
                "{\"Version\":1,\"WorldTimeMilliseconds\":42,\"Summary\":\"旧存档\",\"ContentJson\":\"legacy\"}");

            SaveSlotReadResult result = _service.Read(0);

            Assert.That(result.IsSuccess, Is.True, "迁移入口必须接受旧格式。");
            Assert.That(result.Record.Summary, Is.EqualTo("旧存档"));
            Assert.That(result.Record.HasChecksum, Is.False);
        }

        [Test]
        public void RestoreFromBackup_PromotesTheNewestUsableOneAndKeepsTheChainOrdered()
        {
            SaveVersions(4);
            File.WriteAllText(_service.GetBackupPath(0, 1), "garbage");
            File.WriteAllText(_service.GetSlotPath(0), "garbage");

            Assert.That(_service.HasUsableBackup(0), Is.True,
                "存在一个读不出来的备份文件，不代表「恢复」可行——可点性看的是可用备份。");
            Assert.That(_service.RestoreFromBackup(0), Is.True);

            SaveSlotReadResult after = _service.Read(0);
            Assert.That(after.IsPrimary, Is.True, "提升之后正式存档自己就该是好的。");
            Assert.That(after.Record.Summary, Is.EqualTo("v2"), "提升的是最新一份**可用**备份。");

            // 被提升的是备份 2（v2）；比它新的那份坏备份已经从链上删掉，比它旧的 v1 前移。
            Assert.That(File.Exists(_service.GetBackupPath(0, 1)), Is.False,
                "比被提升的那份更新的备份按定义都不可用，留在链上只会占位置。");
            Assert.That(SummaryOfBackup(2), Is.EqualTo("v1"));
            Assert.That(File.Exists(_service.GetBackupPath(0, 3)), Is.False);
        }

        [Test]
        public void RestoreFromBackup_WithoutAnyUsableBackup_LeavesEverythingAlone()
        {
            SaveVersions(1);
            File.WriteAllText(_service.GetSlotPath(0), "garbage");

            Assert.That(_service.HasUsableBackup(0), Is.False);
            Assert.That(_service.RestoreFromBackup(0), Is.False);
            Assert.That(File.ReadAllText(_service.GetSlotPath(0)), Is.EqualTo("garbage"),
                "恢复失败不得动主文件——那会让玩家丢掉更多东西。");
        }

        [Test]
        public void ListBackups_ReportsTimeAndUsabilityForEachOfTheThree()
        {
            SaveVersions(4);

            IReadOnlyList<SaveSlotBackupInfo> backups = _service.ListBackups(0);

            Assert.That(backups.Count, Is.EqualTo(SaveSlotService.BackupCount));
            Assert.That(backups.Select(b => b.Index), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(backups.All(b => b.Exists && b.Usable), Is.True);
            Assert.That(backups[0].Record.SavedUtc, Is.GreaterThan(backups[1].Record.SavedUtc),
                "序号越小越新——恢复界面要按这个顺序告诉玩家会退回到什么时候。");
        }

        [Test]
        public void MissingMainFileWithOnlyBackups_IsRecoverableRatherThanEmpty()
        {
            SaveVersions(2);
            File.Delete(_service.GetSlotPath(0));

            SaveSlotReadResult result = _service.Read(0);

            Assert.That(result.Status, Is.EqualTo(SaveSlotReadStatus.RecoveredFromBackup),
                "正式存档不见了但备份还在：这是可恢复，不是「空槽位」。");
            Assert.That(result.Record.Summary, Is.EqualTo("v1"));
        }

        [Test]
        public void Narrative_TellsThePlayerWhichBackupAndWhatWillBeLost()
        {
            SaveVersions(3);
            File.WriteAllText(_service.GetSlotPath(0), "garbage");

            SaveSlotReadResult result = _service.Read(0);
            IReadOnlyList<SaveSlotBackupInfo> backups = _service.ListBackups(0);
            string text = SaveSlotNarrative.DescribeRecovery(result, backups);

            Assert.That(result.Status, Is.EqualTo(SaveSlotReadStatus.RecoveredFromBackup));
            StringAssert.Contains("备份 1", text, "必须说清恢复到哪一份。");
            StringAssert.Contains("预计损失范围", text, "必须说清会丢多少。");
            StringAssert.Contains("2026-", text, "必须给出时间点。");

            Assert.That(SaveSlotNarrative.DescribeStatus(SaveSlotReadStatus.RecoveredFromBackup),
                Does.Not.Contain("正常"), "读备份不能显示成正常。");
            Assert.That(SaveSlotNarrative.DescribeRow(result), Does.Not.Contain("正常"));
        }

        [Test]
        public void Narrative_WithoutAnyUsableBackup_SaysRecoveryIsImpossible()
        {
            SaveVersions(2);
            File.WriteAllText(_service.GetSlotPath(0), "garbage");
            File.WriteAllText(_service.GetBackupPath(0, 1), "garbage");

            SaveSlotReadResult result = _service.Read(0);
            string text = SaveSlotNarrative.DescribeRecovery(result, _service.ListBackups(0));

            Assert.That(result.Status, Is.EqualTo(SaveSlotReadStatus.Corrupt));
            StringAssert.Contains("无法恢复", text);
        }

        [Test]
        public void Checksum_IsStableAcrossRecordsWithIdenticalFields()
        {
            var a = new SaveSlotRecord
            {
                Version = SaveSlotRecord.CurrentVersion, SlotIndex = 0, SavedUtcTicks = 111,
                WorldTimeMilliseconds = 222, Summary = "s", ContentJson = "c", ContentLength = 1,
            };
            var b = new SaveSlotRecord
            {
                Version = SaveSlotRecord.CurrentVersion, SlotIndex = 0, SavedUtcTicks = 111,
                WorldTimeMilliseconds = 222, Summary = "s", ContentJson = "c", ContentLength = 1,
            };

            Assert.That(SaveChecksum.Compute(a), Is.EqualTo(SaveChecksum.Compute(b)));
            Assert.That(SaveChecksum.Matches(SaveChecksum.Compute(a), SaveChecksum.Compute(b)), Is.True);

            b.Summary = "s2";
            Assert.That(SaveChecksum.Matches(SaveChecksum.Compute(a), SaveChecksum.Compute(b)), Is.False,
                "摘要也在校验范围内：它同样是玩家看到的状态。");
        }

        /// <summary>第 index 份备份里的摘要（不存在或不可读时断言失败，避免静默通过）。</summary>
        private string SummaryOfBackup(int index)
        {
            IReadOnlyList<SaveSlotBackupInfo> backups = _service.ListBackups(0);
            SaveSlotBackupInfo info = backups[index - 1];
            Assert.That(info.Usable, Is.True, "备份 " + index + " 应该是可用的。");
            return info.Record.Summary;
        }
    }
}
