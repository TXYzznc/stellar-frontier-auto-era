using System.Collections.Generic;
using AutoEra.Alerts;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 警报读模型：三种不可用原因、真实行与名字解析、选中、标记已读，以及
    /// **值类型快照必须每次重建**（这条规则在存档域、区域域、能源域各踩过一次）。
    ///
    /// 界面唯一的写入口是「标记已读」，而且它只改阅读状态——规格明确「无手动清除故障或
    /// 重置真实状态按钮」，所以读模型里没有 Resolve：界面既不能制造恢复，也不能清除问题。
    /// </summary>
    public sealed class AlertReadModelEditModeTests
    {
        private AutoEraApplicationContext _context;
        private AutoEraWorldSession _world;

        [SetUp]
        public void SetUp()
        {
            _context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            Assert.That(_context.TryCreateWorldSession(0L, out _world), Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _context = null;
        }

        [Test]
        public void MissingSession_ReportsUnavailableWithReason()
        {
            using (IAlertReadModel model = AlertReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.Reason, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Alerts, Is.Empty, "不可用时不得伪造警报。");
                Assert.That(model.Snapshot.HighestActive, Is.Null, "也不得假装「一切正常」。");
            }
        }

        [Test]
        public void SessionWithoutWorld_SaysItIsOutsideTheWorld()
        {
            // 另造一个没有活动世界会话的应用上下文：SetUp 里那个已经建了世界，
            // 用它测「世界外」会测到别的分支去。
            using (var application = new AutoEraApplicationContext(
                       new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory()))
            using (IAlertReadModel model = AlertReadModels.Create(AutoEraUiSession.ForApplication(application)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.Reason, Does.Contain("世界"));
            }
        }

        [Test]
        public void WorldWithoutRegionLedger_SaysTheRegionIsNotReady()
        {
            using (IAlertReadModel model = AlertReadModels.Create(AutoEraUiSession.ForWorld(_context, _world)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.Reason, Does.Contain("区域"));
            }
        }

        [Test]
        public void EmptyLedger_IsReadyWithNoAlerts()
        {
            var alerts = new AutoEraAlertService();

            using (IAlertReadModel model = Model(alerts))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Ready),
                    "账本活着但一条警报都没有，是「当前没有问题」，不是「不可用」。");
                Assert.That(model.Snapshot.Alerts, Is.Empty);
                Assert.That(model.Snapshot.ActiveCount, Is.Zero);
                Assert.That(model.Snapshot.HighestActive, Is.Null);
            }
        }

        [Test]
        public void AlertsCarryTheSourceNameAndCounts()
        {
            MachineInstance machine = _world.Machines.Create(Definition());
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.EnergyShortage, machine.Id, 60000L);

            using (IAlertReadModel model = Model(alerts))
            {
                Assert.That(model.Snapshot.Alerts.Count, Is.EqualTo(1));
                UiAlertRow row = model.Snapshot.Alerts[0];

                Assert.That(row.SourceName, Is.EqualTo(machine.Name),
                    "列表行必须说清是谁在报——只剩一个身份数字等于没告诉玩家。");
                Assert.That(row.Title, Does.Contain("警告"));
                Assert.That(row.Title, Does.Contain("缺电停机"));
                Assert.That(row.Describe(), Does.Contain("活跃"));
                Assert.That(row.Describe(), Does.Contain("未读"));
                Assert.That(model.Snapshot.UnreadCount, Is.EqualTo(1));
                Assert.That(model.Snapshot.HighestActive, Is.EqualTo(AlertSeverity.Warning));
            }
        }

        [Test]
        public void SelectionPublishesDetailFieldsForTheChosenAlert()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.MachineDestroyed, PersistentId.Invalid, 60000L);

            using (IAlertReadModel model = Model(alerts))
            {
                Assert.That(model.Snapshot.HasSelection, Is.False, "刚打开时不该替玩家选中一条。");

                int id = model.Snapshot.Alerts[0].Id;
                Assert.That(model.Select(id), Is.True);
                Assert.That(model.Snapshot.HasSelection, Is.True);
                Assert.That(model.Snapshot.Selected.Id, Is.EqualTo(id));

                var detail = new List<UiDetailField>();
                AlertDetails.Build(model.Snapshot.Selected, detail);

                Assert.That(ContainsField(detail, "真实恢复条件"), Is.True,
                    "详情必须给出真实恢复条件——那是玩家唯一能做的事。");
                Assert.That(ContainsField(detail, "触发原因"), Is.True);
                Assert.That(ContainsField(detail, "影响"), Is.True);
                Assert.That(FieldValue(detail, "状态"), Does.Contain("活跃"));
                Assert.That(FieldValue(detail, "恢复时间"), Is.EqualTo("尚未恢复"));

                Assert.That(model.Select(9999), Is.False, "不存在的标识必须被拒。");
            }
        }

        [Test]
        public void MarkReadChangesOnlyTheReadingState()
        {
            var alerts = new AutoEraAlertService();
            alerts.Raise(AlertKind.FuelExhausted, PersistentId.Invalid, 1000L);

            using (IAlertReadModel model = Model(alerts))
            {
                int id = model.Snapshot.Alerts[0].Id;

                Assert.That(model.MarkRead(id), Is.True);
                Assert.That(model.Snapshot.UnreadCount, Is.Zero, "标记已读后未读数归零。");
                Assert.That(model.Snapshot.ActiveCount, Is.EqualTo(1),
                    "标记已读不解决问题：它仍然是活跃警报（界面没有清除故障的入口）。");
                Assert.That(model.Snapshot.Alerts[0].Read, Is.True);
            }
        }

        [Test]
        public void LedgerChangesRebuildTheSnapshotAndNotify()
        {
            var alerts = new AutoEraAlertService();

            using (IAlertReadModel model = Model(alerts))
            {
                int notifications = 0;
                model.Changed += () => notifications++;

                alerts.Raise(AlertKind.EnergyShortage, PersistentId.Invalid, 1000L);

                Assert.That(notifications, Is.EqualTo(1), "账本变化要通知界面。");
                Assert.That(model.Snapshot.Alerts.Count, Is.EqualTo(1),
                    "通知之外还必须换掉快照本身——只发事件不重建快照是这块踩过的老坑。");

                alerts.Resolve(AlertKind.EnergyShortage, PersistentId.Invalid, 2000L);

                Assert.That(model.Snapshot.ActiveCount, Is.Zero, "恢复后快照要跟着变。");
                Assert.That(model.Snapshot.Alerts[0].IsActive, Is.False);
            }
        }

        [Test]
        public void DisposeStopsListeningToTheLedger()
        {
            var alerts = new AutoEraAlertService();
            IAlertReadModel model = Model(alerts);
            int notifications = 0;
            model.Changed += () => notifications++;
            model.Dispose();

            alerts.Raise(AlertKind.EnergyShortage, PersistentId.Invalid, 1000L);

            Assert.That(notifications, Is.Zero, "释放之后不得再回调已销毁的界面。");
        }

        private IAlertReadModel Model(AutoEraAlertService alerts) =>
            AlertReadModels.Create(AutoEraUiSession.ForWorld(_context, _world, regionAlerts: alerts));

        private static MachineDefinition Definition() =>
            new MachineDefinition(1001, "警报界面验证机", 1, 2, 1, 2, 30, true, true, 100d, 1.8d, 2.6d,
                "Machines/WheeledCarrier", 0.2d, 2d);

        private static bool ContainsField(IReadOnlyList<UiDetailField> fields, string label)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Label == label) return true;
            }

            return false;
        }

        private static string FieldValue(IReadOnlyList<UiDetailField> fields, string label)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Label == label) return fields[i].Value;
            }

            return null;
        }
    }
}
