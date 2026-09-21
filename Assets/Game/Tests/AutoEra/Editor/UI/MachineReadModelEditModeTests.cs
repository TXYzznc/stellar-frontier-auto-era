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

        // ------------------------------------------------------------ 整备页三栏

        [Test]
        public void PreparationCarrierRows_DescribeTheRealDefinition()
        {
            using (AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world))
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                MachineInstance machine = world.Machines.Create(Definition());
                Assert.That(model.Select(machine.Id), Is.True);

                IReadOnlyList<UiDetailField> carrier = model.Snapshot.Carrier;
                Assert.That(Value(carrier, "型号"), Is.EqualTo("Fixture"));
                Assert.That(Value(carrier, "等级"), Is.EqualTo("1"));
                StringAssert.Contains("库中", Value(carrier, "部署"));
                Assert.That(Value(carrier, "总通用容量"), Does.Contain("30"), "容量必须来自定义而不是常量。");
                Assert.That(Value(carrier, "兼容安装位"), Is.EqualTo("传感器 2 ／ 核心 1 ／ 执行器 2"),
                    "安装位必须来自定义，界面据此才知道该往哪装。");
            }
        }

        [Test]
        public void PreparationAssemblyRows_ListEverySlotAndItsOccupant()
        {
            using (AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world))
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                MachineInstance machine = world.Machines.Create(Definition());
                ComponentInstance core = world.Machines.CreateComponent(
                    new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 50, 40, false));
                Assert.That(world.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0),
                    Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(model.Select(machine.Id), Is.True);

                IReadOnlyList<UiDetailField> assembly = model.Snapshot.Assembly;
                // 逐个槽位列出——只显示「已装 1 件」会让人无法回答「哪一格空着」。
                Assert.That(Value(assembly, "传感器槽 0"), Is.EqualTo("空"));
                Assert.That(Value(assembly, "核心槽 0"), Is.Not.EqualTo("空"));
                Assert.That(Value(assembly, "执行器槽 0"), Is.EqualTo("空"));
                Assert.That(Value(assembly, "已装组件"), Is.EqualTo("1 件"));
                Assert.That(Value(assembly, "一键卸下影响"), Does.Contain("1 件组件"));
                Assert.That(Value(assembly, "库存候选"), Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void PreparationReadiness_SellQualificationFollowsTheCarrierState()
        {
            using (AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world))
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                MachineInstance machine = world.Machines.Create(Definition());
                Assert.That(model.Select(machine.Id), Is.True);
                StringAssert.Contains("资格成立", Value(model.Snapshot.Readiness, "出售资格"),
                    "空载、完好、未部署的载体按规格就是可出售的。");

                // 装上组件之后资格必须消失，并说清为什么（规格：出售空载完好载体）。
                ComponentInstance core = world.Machines.CreateComponent(
                    new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 50, 40, false));
                world.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);

                StringAssert.Contains("先一键卸下", Value(model.Snapshot.Readiness, "出售资格"));

                // 部署之后同样不可出售（必须先撤收回库）。
                world.Machines.Remove(machine.Id, ManagementOrigin.Library, HardwareKind.Core, 0);
                world.Machines.Deploy(machine.Id);
                StringAssert.Contains("先撤收回库", Value(model.Snapshot.Readiness, "出售资格"));

                // 解锁域没有创建者，所以这一行必须**明说**而不是编一个「已解锁」。
                StringAssert.Contains("成长解锁", Value(model.Snapshot.Readiness, "部署解锁条件"));
            }
        }

        [Test]
        public void PreparationRows_AreEmptyWithoutSelection()
        {
            using (AutoEraApplicationContext context = ContextWithWorld(out AutoEraWorldSession world))
            using (IMachineReadModel model = MachineReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
            {
                world.Machines.Create(Definition());
                Assert.That(model.Snapshot.Carrier, Is.Empty, "未选中时三栏必须为空，不能拿别的机器凑数。");
                Assert.That(model.Snapshot.Assembly, Is.Empty);
                Assert.That(model.Snapshot.Readiness, Is.Empty);
            }
        }

        private static string Value(IReadOnlyList<UiDetailField> fields, string label)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Label == label)
                {
                    return fields[i].Value;
                }
            }

            Assert.Fail("整备页缺少行：" + label);
            return null;
        }

    }
}
