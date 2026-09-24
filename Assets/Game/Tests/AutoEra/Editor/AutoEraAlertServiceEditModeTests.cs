using System.Collections.Generic;
using AutoEra.Alerts;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 警报账本的状态机（规格 15-警报列表与详情：「同类型、来源与目标合并；恢复后转历史」，
    /// 「标记已读只改变阅读状态，不解决问题」，「无手动清除故障或重置真实状态按钮」）。
    ///
    /// 这一层最要紧的一条不变式是**次数是「发生过的段数」，不是轮询次数**：
    /// 监视器每帧都会报告「问题还在」，若每一次都算一次发生，一条整夜停机能把次数刷到几万，
    /// 历史也就没法读了。所以「条件持续存在」必须是零写入。
    /// </summary>
    public sealed class AutoEraAlertServiceEditModeTests
    {
        private static readonly PersistentId MachineA = new PersistentId(701);
        private static readonly PersistentId MachineB = new PersistentId(702);

        [Test]
        public void FirstRaiseCreatesAnActiveUnreadEntry()
        {
            var alerts = new AutoEraAlertService();

            Assert.That(alerts.Raise(AlertKind.EnergyShortage, MachineA, 1000L), Is.True,
                "第一次报告是一次真实的跨越，调用方据此写日志。");
            Assert.That(alerts.Count, Is.EqualTo(1));
            Assert.That(alerts.ActiveCount, Is.EqualTo(1));
            Assert.That(alerts.UnreadCount, Is.EqualTo(1));
            Assert.That(alerts.HighestActiveSeverity, Is.EqualTo(AlertCatalog.Severity(AlertKind.EnergyShortage)));

            var rows = new List<AlertEntry>();
            alerts.CopyInto(rows);

            Assert.That(rows[0].State, Is.EqualTo(AlertState.Active));
            Assert.That(rows[0].Count, Is.EqualTo(1));
            Assert.That(rows[0].FirstMilliseconds, Is.EqualTo(1000L));
            Assert.That(rows[0].LastMilliseconds, Is.EqualTo(1000L));
            Assert.That(rows[0].Read, Is.False, "新出现的问题默认未读。");
        }

        [Test]
        public void RepeatedRaisesWhileActive_AreNotWrites()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.EnergyShortage, MachineA, 1000L);

            int notifications = 0;
            alerts.Changed += () => notifications++;

            // 模拟监视器每帧都报告「问题还在」。
            for (long t = 2000L; t <= 20000L; t += 1000L)
            {
                Assert.That(alerts.Raise(AlertKind.EnergyShortage, MachineA, t), Is.False,
                    "条件持续存在不是一次新的发生。");
            }

            var rows = new List<AlertEntry>();
            alerts.CopyInto(rows);

            Assert.That(notifications, Is.Zero, "持续存在期间不该通知界面——那会让每帧都重绘。");
            Assert.That(rows[0].Count, Is.EqualTo(1), "次数是发生过的段数，不是轮询次数。");
            Assert.That(rows[0].LastMilliseconds, Is.EqualTo(1000L), "时间也不该被每帧刷新。");
        }

        [Test]
        public void ResolveMovesTheEntryToHistory()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.FuelExhausted, MachineA, 1000L);
            alerts.Raise(AlertKind.FuelExhausted, MachineA, 2000L);

            int notifications = 0;
            alerts.Changed += () => notifications++;

            Assert.That(alerts.Resolve(AlertKind.FuelExhausted, MachineA, 9000L), Is.True);
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(alerts.ActiveCount, Is.Zero);
            Assert.That(alerts.HighestActiveSeverity, Is.Null, "没有活跃警报时不得假装有。");

            var rows = new List<AlertEntry>();
            alerts.CopyInto(rows);
            Assert.That(rows[0].State, Is.EqualTo(AlertState.Recovered));
            Assert.That(rows[0].RecoveredMilliseconds, Is.EqualTo(9000L));
            Assert.That(rows[0].Count, Is.EqualTo(1), "重复报告在活跃期里被合并，次数仍是 1。");

            Assert.That(alerts.Resolve(AlertKind.FuelExhausted, MachineA, 9500L), Is.False,
                "已经恢复的条目不能再恢复一次。");
            Assert.That(alerts.Resolve(AlertKind.FuelExhausted, MachineB, 9500L), Is.False,
                "别的来源没有这条警报。");
        }

        [Test]
        public void RecurrenceReopensTheSameEntry_AndCountsASecondOccurrence()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.EnergyShortage, MachineA, 1000L);
            alerts.Resolve(AlertKind.EnergyShortage, MachineA, 5000L);

            Assert.That(alerts.Raise(AlertKind.EnergyShortage, MachineA, 6000L), Is.True,
                "恢复之后再次发生是一次新的跨越。");
            Assert.That(alerts.Count, Is.EqualTo(1), "同类型同来源合并成一行。");

            var rows = new List<AlertEntry>();
            alerts.CopyInto(rows);

            Assert.That(rows[0].IsActive, Is.True);
            Assert.That(rows[0].Count, Is.EqualTo(2));
            Assert.That(rows[0].FirstMilliseconds, Is.EqualTo(1000L), "首次时间保留最早那一次。");
            Assert.That(rows[0].LastMilliseconds, Is.EqualTo(6000L));
            Assert.That(rows[0].RecoveredMilliseconds, Is.Zero, "重新活跃后恢复时间必须清掉。");
            Assert.That(rows[0].Read, Is.False, "新的一次发生重新变成未读。");
        }

        [Test]
        public void MarkReadOnlyChangesTheReadingState()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.MachineDestroyed, MachineA, 1000L);

            var rows = new List<AlertEntry>();
            alerts.CopyInto(rows);
            int id = rows[0].Id;

            Assert.That(alerts.MarkRead(id), Is.True);
            Assert.That(alerts.UnreadCount, Is.Zero);

            alerts.CopyInto(rows = new List<AlertEntry>());
            Assert.That(rows[0].Read, Is.True);
            Assert.That(rows[0].State, Is.EqualTo(AlertState.Active),
                "标记已读不解决问题：它仍然是活跃警报。");
            Assert.That(alerts.ActiveCount, Is.EqualTo(1));

            Assert.That(alerts.MarkRead(id), Is.False, "已经已读的再标记没有变化。");
            Assert.That(alerts.MarkRead(9999), Is.False, "不存在的标识被拒。");
        }

        [Test]
        public void OrderingPutsActiveFirst_ThenBySeverityAndRecency()
        {
            var alerts = new AutoEraAlertService();
            // 严重（损坏）先报，但历史里也有一条；活跃里再放一条警告。
            alerts.Raise(AlertKind.MachineDestroyed, MachineA, 1000L);
            alerts.Resolve(AlertKind.MachineDestroyed, MachineA, 2000L);
            alerts.Raise(AlertKind.EnergyShortage, MachineA, 3000L);
            alerts.Raise(AlertKind.FuelExhausted, MachineB, 4000L);

            var rows = new List<AlertEntry>();
            alerts.CopyInto(rows);

            Assert.That(rows[0].IsActive, Is.True);
            Assert.That(rows[1].IsActive, Is.True);
            Assert.That(rows[2].IsActive, Is.False, "历史排在活跃之后。");
            Assert.That(rows[0].Severity, Is.GreaterThanOrEqualTo(rows[1].Severity),
                "活跃内部按等级从高到低——中枢与 HUD 都取第一条当「最要紧的那条」。");
            Assert.That(rows[0].LastMilliseconds, Is.GreaterThan(rows[1].LastMilliseconds),
                "同等级按最近发生在前——最要紧的那条应当排在最上面。");
        }

        [Test]
        public void CopyIntoDoesNotClearTheDestination()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.EnergyShortage, MachineA, 1000L);

            var rows = new List<AlertEntry> { default };
            alerts.CopyInto(rows);

            Assert.That(rows.Count, Is.EqualTo(2), "调用方负责复用列表，账本只追加。");
        }

        [Test]
        public void ClearEmptiesTheLedger()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.EnergyShortage, MachineA, 1000L);

            int notifications = 0;
            alerts.Changed += () => notifications++;
            alerts.Clear();

            Assert.That(alerts.Count, Is.Zero);
            Assert.That(alerts.ActiveCount, Is.Zero);
            Assert.That(notifications, Is.EqualTo(1));
            alerts.Clear();
            Assert.That(notifications, Is.EqualTo(1), "已经空了不再通知。");
        }

        [Test]
        public void CatalogGivesEveryKindASeverityAndARecoveryCondition()
        {
            foreach (AlertKind kind in new[]
                     {
                         AlertKind.EnergyShortage, AlertKind.StorageDepleted,
                         AlertKind.FuelExhausted, AlertKind.MachineDestroyed,
                     })
            {
                Assert.That(AlertCatalog.Label(kind), Is.Not.Null.And.Not.Empty);
                Assert.That(AlertCatalog.Reason(kind), Is.Not.Null.And.Not.Empty);
                Assert.That(AlertCatalog.Impact(kind), Is.Not.Null.And.Not.Empty);
                Assert.That(AlertCatalog.Target(kind), Is.Not.Null.And.Not.Empty);
                Assert.That(AlertCatalog.RecoveryCondition(kind), Is.Not.EqualTo("—"),
                    $"每一类警报都必须写得出真实恢复条件（{kind}）——写不出来就不该做成警报。");
            }

            Assert.That(AlertCatalog.Severity(AlertKind.MachineDestroyed), Is.EqualTo(AlertSeverity.Critical),
                "完全损坏是严重级；其余噪声更低的问题取警告。");
        }
    }
}
