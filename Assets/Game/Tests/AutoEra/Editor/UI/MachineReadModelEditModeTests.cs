using System.Collections.Generic;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 机器域读取模型的数据流：快照、按区域通知、选中、退订与不可用态。
    ///
    /// 这是接入层的纯逻辑验证（不需要场景）；界面渲染的端到端验证由 PlayMode 承担。
    /// 测试走正式入口 <see cref="AutoEraUiSession"/>，因此顺带覆盖了「会话 → 域模型」这条链。
    /// </summary>
    public sealed class MachineReadModelEditModeTests
    {
        private static AutoEraApplicationContext ContextWithWorld(out AutoEraWorldSession world)
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            Assert.That(context.TryCreateWorldSession(0L, out world), Is.True, "应能创建世界会话。");
            return context;
        }

        private static MachineDefinition Definition() =>
            new MachineDefinition(1001, "Fixture", 1, 2, 1, 2, 30, true, true, 100);

        [Test]
        public void MissingSession_ReportsUnavailableWithReason()
        {
            using (IMachineReadModel model = MachineReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.Not.Null.And.Not.Empty,
                    "不可用态必须给出可展示的原因。");
                Assert.That(model.Snapshot.Count, Is.Zero, "不可用时不得伪造数据。");
            }
        }

        [Test]
        public void SessionWithoutWorld_ReportsUnavailable()
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            using (context)
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForApplication(context)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
            }
        }

        [Test]
        public void WorldWithoutMachines_ReportsEmptyNotUnavailable()
        {
            AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world);
            using (context)
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty),
                    "领域就绪但没有记录，应是 Empty 而不是 Unavailable。");
                Assert.That(model.Snapshot.Count, Is.Zero);
            }
        }

        [Test]
        public void CreatingMachine_NotifiesListAndUpdatesSnapshot()
        {
            AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world);
            using (context)
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                var sections = new List<MachineDomainSection>();
                model.Changed += sections.Add;

                MachineInstance machine = world.Machines.Create(Definition());

                Assert.That(sections, Contains.Item(MachineDomainSection.List),
                    "创建机器必须按「列表」区域通知，界面才能只刷索引栏。");
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Ready));
                Assert.That(model.Snapshot.Count, Is.EqualTo(1));
                Assert.That(model.Snapshot.Machines[0].Id, Is.EqualTo(machine.Id));
                Assert.That(model.Snapshot.Machines[0].Name, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Machines[0].Status, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void SelectingMachine_NotifiesDetailAndFillsFields()
        {
            AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world);
            using (context)
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                MachineInstance machine = world.Machines.Create(Definition());
                var sections = new List<MachineDomainSection>();
                model.Changed += sections.Add;

                Assert.That(model.Select(machine.Id), Is.True);
                Assert.That(sections, Contains.Item(MachineDomainSection.Detail));
                Assert.That(model.SelectedId, Is.EqualTo(machine.Id));
                Assert.That(model.Snapshot.HasSelection, Is.True);
                Assert.That(model.Snapshot.Detail.Count, Is.GreaterThan(0));
                Assert.That(model.Snapshot.Detail[0].Label, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Detail[0].Value, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void ClearingSelection_NotifiesDetailAndEmptiesDetail()
        {
            AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world);
            using (context)
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                MachineInstance machine = world.Machines.Create(Definition());
                Assert.That(model.Select(machine.Id), Is.True);
                Assert.That(model.Snapshot.HasSelection, Is.True);

                var sections = new List<MachineDomainSection>();
                model.Changed += sections.Add;

                model.ClearSelection();

                Assert.That(sections, Contains.Item(MachineDomainSection.Detail));
                Assert.That(model.SelectedId.IsValid, Is.False);
                Assert.That(model.Snapshot.HasSelection, Is.False, "清空选择后详情栏必须为空，不得残留上一台。");
            }
        }

        [Test]
        public void DisposedModel_StopsReceivingNotifications()
        {
            AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world);
            using (context)
            {
                IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world));
                int notifications = 0;
                model.Changed += _ => notifications++;

                model.Dispose();
                int afterDispose = notifications;

                world.Machines.Create(Definition());

                Assert.That(notifications, Is.EqualTo(afterDispose),
                    "释放后不得再收到领域变化——关闭界面必须干净退订。");
            }
        }

        [Test]
        public void Rows_CarryDeploymentState_SoLibraryCanSplitPages()
        {
            using (AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world))
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                MachineInstance first = world.Machines.Create(Definition());
                MachineInstance second = world.Machines.Create(Definition());
                world.Machines.Deploy(second.Id);

                MachineDomainSnapshot snapshot = model.Snapshot;
                int undeployed = 0;
                int deployed = 0;
                for (int i = 0; i < snapshot.Count; i++)
                {
                    if (snapshot.Machines[i].Deployed)
                    {
                        deployed++;
                    }
                    else
                    {
                        undeployed++;
                    }
                }

                Assert.That(undeployed, Is.EqualTo(1),
                    "机器库的「库中机器」与「已部署机器」看的是同一个花名册，必须能按行状态分开过滤。");
                Assert.That(deployed, Is.EqualTo(1));
                Assert.That(first.Id, Is.Not.EqualTo(second.Id));
            }
        }

    }
}
