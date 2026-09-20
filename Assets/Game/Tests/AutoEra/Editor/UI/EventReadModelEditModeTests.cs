using System.Collections.Generic;
using AutoEra.Application;
using AutoEra.Events;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 事件域读取模型的数据流：三种不可用原因、空日志是 Empty、按域分组、选中取追溯。
    ///
    /// 事件域与算法域不同——它是**活的**：世界会话创建时就会建好事件服务与日志。
    /// 所以「世界已就绪」这条路径期望的是 Empty/Ready，而不是 Unavailable。
    /// </summary>
    public sealed class EventReadModelEditModeTests
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
            using (IEventReadModel model = EventReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Count, Is.Zero);
            }
        }

        [Test]
        public void EmptyJournal_ReportsEmptyStateNotUnavailable()
        {
            using (IEventReadModel model = Model())
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty),
                    "日志存在但还没有记录时是 Empty——没有记录是正常状态，不是不可用。");
                Assert.That(model.Snapshot.EnergyRecords, Is.Empty,
                    "能源历史当前没有数据来源，必须是空而不是编造。");
            }
        }

        [Test]
        public void Records_AreGroupedIntoDisplayDomains()
        {
            var correlation = new CorrelationId(7);
            Append(EventDomain.Task, "移动到位", correlation, terminal: true, EventOutcome.Succeeded, sequence: 1);
            Append(EventDomain.Effector, "夹取", correlation, terminal: false, EventOutcome.None, sequence: 2);
            Append(EventDomain.Algorithm, "应用草稿", new CorrelationId(8), terminal: true, EventOutcome.Succeeded, sequence: 3);
            Append(EventDomain.Resource, "采集", new CorrelationId(9), terminal: false, EventOutcome.None, sequence: 4);

            using (IEventReadModel model = Model())
            {
                EventDomainSnapshot snapshot = model.Snapshot;

                Assert.That(snapshot.State, Is.EqualTo(UiDataState.Ready));
                Assert.That(snapshot.AllRecords.Count, Is.EqualTo(4), "统计页要看全部记录。");
                Assert.That(snapshot.MachineRecords.Count, Is.EqualTo(2),
                    "机器历史取任务域与执行域——机器做的事以这两类记账。");
                Assert.That(snapshot.AlgorithmRecords.Count, Is.EqualTo(1));
                Assert.That(snapshot.EnergyRecords, Is.Empty,
                    "资源域不等于能源域，不得被塞进能源历史。");
            }
        }

        [Test]
        public void Select_ResolvesTraceForTheCorrelation()
        {
            var correlation = new CorrelationId(11);
            Append(EventKind.Command, EventDomain.Task, "开始", correlation, terminal: false, EventOutcome.None, sequence: 1);
            Append(EventKind.Fact, EventDomain.Task, "移动到位", correlation, terminal: true, EventOutcome.Succeeded, sequence: 2);
            Append(EventKind.Fact, EventDomain.Algorithm, "无关记录", new CorrelationId(12), terminal: false, EventOutcome.None, sequence: 3);

            using (IEventReadModel model = Model())
            {
                Assert.That(model.Select(2), Is.True);
                Assert.That(model.Snapshot.HasSelection, Is.True);

                List<UiDetailField> detail = new List<UiDetailField>(model.Snapshot.Detail);
                Assert.That(Contains(detail, "移动到位"), Is.True, "详情要显示这条记录本身。");
                Assert.That(Contains(detail, "已结案"), Is.True,
                    "追溯链是日志本就能给的东西：命令 + 事实 + 终态。");
                Assert.That(Contains(detail, "事实 1"), Is.True, "追溯要给出该关联下的事实条数。");
            }
        }

        [Test]
        public void Select_UnknownSequence_IsRejected()
        {
            Append(EventDomain.Task, "唯一记录", new CorrelationId(21), terminal: false, EventOutcome.None, sequence: 1);

            using (IEventReadModel model = Model())
            {
                Assert.That(model.Select(999), Is.False, "不存在的序号必须被拒绝，而不是假装选中。");
                Assert.That(model.Snapshot.HasSelection, Is.False);
            }
        }

        [Test]
        public void Refresh_KeepsSelectionWhenRecordStillPresent()
        {
            var correlation = new CorrelationId(31);
            Append(EventDomain.Task, "第一条", correlation, terminal: false, EventOutcome.None, sequence: 1);

            using (IEventReadModel model = Model())
            {
                model.Select(1);
                Append(EventDomain.Task, "第二条", new CorrelationId(32), terminal: false, EventOutcome.None, sequence: 2);
                model.Refresh();

                Assert.That(model.Snapshot.SelectedSequence, Is.EqualTo(1UL),
                    "刷新不该把仍然存在的选中项丢掉。");
                Assert.That(model.Snapshot.Count, Is.EqualTo(2));
            }
        }

        private IEventReadModel Model() =>
            EventReadModels.Create(AutoEraUiSession.ForWorld(_context, _world));

        private void Append(EventDomain domain, string action, CorrelationId correlation, bool terminal, EventOutcome outcome, ulong sequence) =>
            Append(EventKind.Fact, domain, action, correlation, terminal, outcome, sequence);

        private void Append(EventKind kind, EventDomain domain, string action, CorrelationId correlation, bool terminal, EventOutcome outcome, ulong sequence) =>
            _world.Events.Journal.Append(new EventJournalRecord(
                kind, domain, correlation, CorrelationId.Invalid, PersistentId.Invalid, action, (long)sequence * 1000L, sequence, terminal, outcome));

        private static bool Contains(IReadOnlyList<UiDetailField> fields, string fragment)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if ((fields[i].Label != null && fields[i].Label.Contains(fragment))
                    || (fields[i].Value != null && fields[i].Value.Contains(fragment)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
