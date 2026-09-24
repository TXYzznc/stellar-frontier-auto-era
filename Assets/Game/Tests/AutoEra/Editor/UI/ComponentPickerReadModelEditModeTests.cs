using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoEra.Application;
using AutoEra.DataTable;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 组件选择器读模型的状态契约（规格 12-选择与绑定 · ComponentPicker）。
    ///
    /// 这一页要回答的是「**这一格该装哪一件**」，所以断言落在三件事上：
    /// <list type="bullet">
    /// <item>候选＝散件，且**兼容判据只有「类别与槽位类别一致」一条**——界面不许自定第二套
    ///       「什么时候能装」的规则（那是领域 `HardwareGate` 的职责），所以不兼容的候选
    ///       依然列出来、依然可以预选，只是不能确认，并给出具体原因；</item>
    /// <item>比较栏的数字是**当前值 → 装有之后的值**，不是孤零零一个增量；</item>
    /// <item>预选项必须始终是「当前候选里还找得到的那一件」——被装到别处之后不得再拿着它提交。</item>
    /// </list>
    /// 「域不可用」与「本页为空」也必须是两种状态：前者怎么点都没有数据，后者买一件／拆一件就会出现。
    /// </summary>
    public sealed class ComponentPickerReadModelEditModeTests
    {
        [Test]
        public void MissingRequest_SaysThePageIsOpenedWithParameters()
        {
            using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(null, null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("参数"));
                Assert.That(model.Snapshot.CandidateCount, Is.Zero, "不可用时不得伪造候选。");
                Assert.That(model.Snapshot.CanConfirm, Is.False);
            }
        }

        [Test]
        public void MissingSession_SaysComponentsBelongToAWorld()
        {
            var request = new AutoEraComponentPickRequest(new PersistentId(1), HardwareKind.Core, 0);
            using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(null, request))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("区域").Or.Contain("世界"));
            }
        }

        [Test]
        public void SlotOutsideTheMachine_SaysTheSlotDoesNotExist()
        {
            using (var fixture = new Fixture())
            {
                fixture.CreateMachine();
                // 夹具机器只有 1 个核心槽，所以 3 号核心槽不存在；界面必须说明而不是显示一个空列表。
                var request = new AutoEraComponentPickRequest(fixture.Machine.Id, HardwareKind.Core, 3);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                    Assert.That(model.Snapshot.UnavailableReason, Does.Contain("核心槽 3"));
                    Assert.That(model.Snapshot.CanConfirm, Is.False);
                }
            }
        }

        [Test]
        public void UnknownMachine_SaysTheMachineIsGone()
        {
            using (var fixture = new Fixture())
            {
                var request = new AutoEraComponentPickRequest(new PersistentId(4242), HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                    Assert.That(model.Snapshot.UnavailableReason, Does.Contain("花名册"));
                }
            }
        }

        [Test]
        public void NoLoosePart_IsEmptyNotUnavailable()
        {
            using (var fixture = new Fixture())
            {
                fixture.CreateMachine();
                var request = new AutoEraComponentPickRequest(fixture.Machine.Id, HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty),
                        "库里没有散件是 Empty——域接线了，买进或拆下就会出现。");
                    Assert.That(model.Snapshot.UnavailableReason,
                        Is.EqualTo(ComponentPickerReadModels.NoCandidateReason));
                    Assert.That(model.Snapshot.OccupantLabel, Is.EqualTo("空"));
                }
            }
        }

        [Test]
        public void IncompatibleCandidates_AreStillListed_WithAConcreteReason_AndCompatibleComeFirst()
        {
            using (var fixture = new Fixture())
            {
                fixture.CreateMachine();
                ComponentInstance arm = fixture.CreateArm();     // 执行器：与本槽位类别不符
                ComponentInstance core = fixture.CreateCore();   // 核心：兼容

                var request = new AutoEraComponentPickRequest(fixture.Machine.Id, HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    ComponentPickerSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.State, Is.EqualTo(UiDataState.Ready));
                    Assert.That(snapshot.CandidateCount, Is.EqualTo(2),
                        "不兼容的候选也要列出来——面向上帝视角说明为什么不能装，"
                        + "而不是让它从列表里凭空消失。");
                    Assert.That(snapshot.Candidates[0].Id, Is.EqualTo(core.Id),
                        "兼容的候选排在前面，否则玩家要先点过一串不能装的。");
                    Assert.That(snapshot.Candidates[0].Compatible, Is.True);
                    Assert.That(snapshot.Candidates[1].Id, Is.EqualTo(arm.Id));
                    Assert.That(snapshot.Candidates[1].Compatible, Is.False);
                    StringAssert.Contains("执行器", snapshot.Candidates[1].IncompatibleReason);
                    StringAssert.Contains("核心", snapshot.Candidates[1].IncompatibleReason);
                    StringAssert.Contains("不兼容", snapshot.Candidates[1].Value);
                    StringAssert.Contains("容量 +", snapshot.Candidates[0].Value,
                        "兼容候选的副标题要给规格增量。");
                }
            }
        }

        [Test]
        public void SelectingACompatibleCandidate_ComparesAgainstTheEmptySlot()
        {
            using (var fixture = new Fixture())
            {
                fixture.CreateMachine();
                ComponentInstance core = fixture.CreateCore();

                var request = new AutoEraComponentPickRequest(fixture.Machine.Id, HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    Assert.That(model.Snapshot.CanConfirm, Is.False);
                    StringAssert.Contains("预选", model.Snapshot.ConfirmBlockedReason);

                    Assert.That(model.Select(core.Id), Is.True);
                    ComponentPickerSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.HasSelection, Is.True);
                    Assert.That(snapshot.SelectedId, Is.EqualTo(core.Id));
                    Assert.That(snapshot.CanConfirm, Is.True);

                    // 比较栏必须给「当前值 → 装有后」，只给增量回答不了「装完还剩多少」。
                    StringAssert.Contains("当前", Value(snapshot.Comparison, "容量"));
                    StringAssert.Contains("→", Value(snapshot.Comparison, "容量"));
                    StringAssert.Contains("50", Value(snapshot.Comparison, "算力"));
                    StringAssert.Contains("核心槽 0", Value(snapshot.Comparison, "槽位"));
                    StringAssert.Contains("算法", Value(snapshot.Comparison, "算法绑定影响"));
                    StringAssert.Contains("兼容", Value(snapshot.Comparison, "不兼容原因"));
                    Assert.That(Value(snapshot.Comparison, "注意"), Is.Null,
                        "空格位没有要先拆的东西，不该出现「注意」一行。");
                }
            }
        }

        [Test]
        public void SelectingAnIncompatibleCandidate_BlocksConfirmWithItsOwnReason()
        {
            using (var fixture = new Fixture())
            {
                fixture.CreateMachine();
                ComponentInstance arm = fixture.CreateArm();

                var request = new AutoEraComponentPickRequest(fixture.Machine.Id, HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    Assert.That(model.Select(arm.Id), Is.True);
                    Assert.That(model.Snapshot.HasSelection, Is.True);
                    Assert.That(model.Snapshot.CanConfirm, Is.False);
                    StringAssert.Contains("执行器", model.Snapshot.ConfirmBlockedReason);
                    StringAssert.Contains("执行器", Value(model.Snapshot.Comparison, "不兼容原因"));
                }
            }
        }

        [Test]
        public void SelectingSomethingThatIsNotACandidate_IsRefusedAndKeepsThePreviousChoice()
        {
            using (var fixture = new Fixture())
            {
                fixture.CreateMachine();
                ComponentInstance core = fixture.CreateCore();

                var request = new AutoEraComponentPickRequest(fixture.Machine.Id, HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    Assert.That(model.Select(core.Id), Is.True);
                    Assert.That(model.Select(new PersistentId(9999)), Is.False);
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(core.Id),
                        "无效预选不得把已经选好的候选清掉。");

                    model.ClearSelection();
                    Assert.That(model.Snapshot.HasSelection, Is.False);
                    Assert.That(model.Snapshot.CanConfirm, Is.False);
                }
            }
        }

        [Test]
        public void InstallingTheCandidateElsewhere_ClearsThePreselection()
        {
            using (var fixture = new Fixture())
            {
                MachineInstance machine = fixture.CreateMachine();
                ComponentInstance core = fixture.CreateCore();

                var request = new AutoEraComponentPickRequest(machine.Id, HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    model.Select(core.Id);
                    Assert.That(model.Snapshot.CanConfirm, Is.True);

                    // 同一件组件被装到这台机器上：它不再算散件，预选必须自动作废——
                    // 否则界面会拿着一个已经有归属的实例去提交。
                    Assert.That(fixture.Roster.Install(machine.Id, ManagementOrigin.Library, core.Id, 0),
                        Is.EqualTo(MachineManagementResult.Completed));

                    Assert.That(model.Snapshot.HasSelection, Is.False);
                    Assert.That(model.Snapshot.CanConfirm, Is.False);
                    Assert.That(model.Snapshot.CandidateCount, Is.Zero);
                    Assert.That(model.Snapshot.OccupantLabel, Does.Not.EqualTo("空"),
                        "槽位里现在装着东西，占用栏要说出来。");
                }
            }
        }

        [Test]
        public void OccupiedSlot_StillOffersHigherLevelCandidates_AndWarnsAboutReplacing()
        {
            using (var fixture = new Fixture())
            {
                MachineInstance machine = fixture.CreateMachine();
                ComponentInstance installed = fixture.CreateCore();
                Assert.That(fixture.Roster.Install(machine.Id, ManagementOrigin.Library, installed.Id, 0),
                    Is.EqualTo(MachineManagementResult.Completed));

                // 第二件核心：可以预选（玩家可以「先拆后装」），但界面必须提示槽位已被占用。
                ComponentInstance spare = fixture.CreateCore();

                var request = new AutoEraComponentPickRequest(machine.Id, HardwareKind.Core, 0);
                using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(
                    fixture.Session(), request, Catalog()))
                {
                    Assert.That(model.Snapshot.CandidateCount, Is.EqualTo(1),
                        "已经装上的那一件不再是候选。");
                    Assert.That(model.Snapshot.Candidates[0].Id, Is.EqualTo(spare.Id));
                    Assert.That(model.Snapshot.OccupantLabel, Does.Not.EqualTo("空"));

                    model.Select(spare.Id);
                    Assert.That(model.Snapshot.CanConfirm, Is.True,
                        "界面不替玩家决定顺序：先拆后装是允许的，由领域判定占用。");
                    StringAssert.Contains("先拆", Value(model.Snapshot.Comparison, "注意"));
                }
            }
        }

        [Test]
        public void UnavailableModel_SelectionIsNoOpAndDisposeIsSafe()
        {
            using (IComponentPickerReadModel model = ComponentPickerReadModels.Create(null, null))
            {
                int notifications = 0;
                model.Changed += () => notifications++;

                Assert.That(model.Select(new PersistentId(1)), Is.False);
                model.ClearSelection();
                model.Dispose();

                Assert.That(notifications, Is.Zero);
            }
        }

        private static string Value(IReadOnlyList<UiDetailField> fields, string label)
        {
            if (fields == null)
            {
                return null;
            }

            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Label == label)
                {
                    return fields[i].Value;
                }
            }

            return null;
        }

        /// <summary>真实数据表里的机器与组件目录；与生产走同一条解析路径。</summary>
        private static MachineCatalog Catalog()
        {
            MachineDefinitions[] machines = File.ReadAllLines("Assets/Game/DataTable/Machines/MachineDefinitions.txt")
                .Skip(4).Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => { var row = new MachineDefinitions(); row.ParseDataRow(s, null); return row; }).ToArray();
            ComponentDefinitions[] components = File.ReadAllLines("Assets/Game/DataTable/Machines/ComponentDefinitions.txt")
                .Skip(4).Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => { var row = new ComponentDefinitions(); row.ParseDataRow(s, null); return row; }).ToArray();
            return new MachineCatalog(machines, components, key => true, key => key);
        }

        private sealed class Fixture : IDisposable
        {
            public Fixture()
            {
                Context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
                Assert.That(Context.TryCreateWorldSession(0L, out AutoEraWorldSession world), Is.True);
                World = world;
                Roster = World.Machines;
            }

            public AutoEraApplicationContext Context { get; }
            public AutoEraWorldSession World { get; }
            public MachineRoster Roster { get; }

            /// <summary>核心 1 槽、传感器 2 槽、执行器 1 槽的夹具机。</summary>
            public MachineInstance Machine { get; private set; }

            public MachineInstance CreateMachine() => Machine ??= Roster.Create(
                new MachineDefinition(1, "选择器夹具机", 1, 2, 1, 1, 20, false, false, 100, 2d, 2d));

            /// <summary>2001 是数据表里真实存在的核心型号（Id 20011 对应的 ModelId）。</summary>
            public ComponentInstance CreateCore() => Roster.CreateComponent(
                new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 50, 40, false));

            /// <summary>2201 = 机械臂（效应器，带行为）。</summary>
            public ComponentInstance CreateArm() => Roster.CreateComponent(
                new ComponentDefinition(2201, HardwareKind.Effector, 1, 0, 0, 0, true));

            public AutoEraUiSession Session() => AutoEraUiSession.ForWorld(Context, World);

            public void Dispose()
            {
                Roster.Dispose();
                Context.ReleaseActiveWorldSession();
                Context.Dispose();
            }
        }
    }
}
