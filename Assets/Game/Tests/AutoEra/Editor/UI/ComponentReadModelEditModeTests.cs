using System;
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
    /// 组件域读取模型的状态契约。
    ///
    /// 这一域的特点是**两半都已在生产里存在**：型号规格来自 `ComponentDefinitions` 表，
    /// 实例与安装位置来自机器花名册。所以这里要钉的不是「有没有数据」，而是
    /// **两半有没有被正确地合起来**：
    /// <list type="bullet">
    /// <item>散件 = 花名册里没有归属机器的组件；</item>
    /// <item>已安装 = 从每台机器的**槽位**枚举出来的组件（槽位是位置的权威来源，含机器名与槽位号）；</item>
    /// <item>详情 = 目录给规格、花名册给实例与位置。</item>
    /// </list>
    /// 另外必须守住「域不可用」与「本页为空」是两种状态：前者怎么点都没有数据，
    /// 后者换一台机器／装一件组件就会出现。
    /// </summary>
    public sealed class ComponentReadModelEditModeTests
    {
        [Test]
        public void MissingSession_ReportsUnavailableWithReason()
        {
            using (IComponentReadModel model = ComponentReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.LooseCount, Is.Zero, "不可用时不得伪造散件。");
                Assert.That(model.Snapshot.InstalledCount, Is.Zero, "不可用时不得伪造已安装组件。");
            }
        }

        [Test]
        public void SessionWithoutWorld_SaysComponentsBelongToAWorld()
        {
            using (var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory()))
            using (IComponentReadModel model = ComponentReadModels.Create(
                AutoEraUiSession.ForApplication(context), Catalog()))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("区域").Or.Contain("世界"));
            }
        }

        [Test]
        public void WorldWithoutACatalog_SaysTheDefinitionTableIsMissing()
        {
            using (var fixture = new Fixture())
            using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session()))
            {
                // 显式传 null 目录 = 模拟「数据表还没加载」：原因是可辨的，而不是抛异常或显示空目录。
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("组件定义表"));
            }
        }

        [Test]
        public void EmptyRoster_IsEmptyNotUnavailable()
        {
            using (var fixture = new Fixture())
            using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Empty),
                    "花名册里没有组件是 Empty——域接线了，只是这次没有内容。");
                Assert.That(model.Snapshot.UnavailableReason, Is.EqualTo(ComponentReadModels.NoComponentReason));
                Assert.That(model.Snapshot.LooseCount, Is.Zero);
                Assert.That(model.Snapshot.InstalledCount, Is.Zero);
            }
        }

        [Test]
        public void CreatedComponent_ShowsUpAsALoosePartWithRealSpecification()
        {
            using (var fixture = new Fixture())
            {
                ComponentInstance core = fixture.CreateCore();
                using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Ready));
                    Assert.That(model.Snapshot.LooseCount, Is.EqualTo(1));
                    Assert.That(model.Snapshot.InstalledCount, Is.Zero);

                    UiComponentRow row = model.Snapshot.Loose[0];
                    Assert.That(row.Id, Is.EqualTo(core.Id));
                    Assert.That(row.Installed, Is.False);
                    Assert.That(row.Definition, Is.Not.Null, "目录里必须能查到这件组件的型号行。");
                    Assert.That(row.Definition.Name, Is.EqualTo("Component.Core"), "目录（本夹具的 localize）给出的名字。");
                    Assert.That(row.Definition.ComputeCapacity, Is.EqualTo(50), "规格必须来自数据表，不是硬编码。");
                    StringAssert.Contains("散件", row.Status);

                    // 选中后详情必须同时带目录规格与实例身份。
                    Assert.That(model.Select(core.Id), Is.True);
                    Assert.That(model.Snapshot.HasSelection, Is.True);
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(core.Id));
                    Assert.That(Labels(model.Snapshot.Detail), Does.Contain("算力"));
                    Assert.That(Value(model.Snapshot.Detail, "型号"), Is.EqualTo("Component.Core"));
                    Assert.That(Value(model.Snapshot.Detail, "位置"), Does.Contain("散件"));
                }
            }
        }

        [Test]
        public void InstalledComponent_ReportsItsSlotAndOwner_AndLeavesTheLooseList()
        {
            using (var fixture = new Fixture())
            {
                MachineInstance machine = fixture.CreateMachine();
                ComponentInstance core = fixture.CreateCore();
                Assert.That(fixture.Roster.Install(machine.Id, ManagementOrigin.Library, core.Id, 0),
                    Is.EqualTo(MachineManagementResult.Completed));

                using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
                {
                    Assert.That(model.Snapshot.LooseCount, Is.Zero, "装上机器之后它就不再是散件。");
                    Assert.That(model.Snapshot.InstalledCount, Is.EqualTo(1));

                    UiComponentRow row = model.Snapshot.Installed[0];
                    Assert.That(row.Id, Is.EqualTo(core.Id));
                    Assert.That(row.Installed, Is.True);
                    Assert.That(row.OwnerName, Is.EqualTo(machine.Name), "安装位置必须带机器名。");
                    Assert.That(row.SlotKind, Is.EqualTo(HardwareKind.Core));
                    Assert.That(row.SlotIndex, Is.Zero);
                    StringAssert.Contains("核心槽 0", row.Status);
                }
            }
        }

        [Test]
        public void SelectionSurvivesMovingFromLooseToInstalled_ButNotItsDisappearance()
        {
            using (var fixture = new Fixture())
            {
                MachineInstance machine = fixture.CreateMachine();
                ComponentInstance core = fixture.CreateCore();
                using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
                {
                    Assert.That(model.Select(core.Id), Is.True);
                    Assert.That(fixture.Roster.Install(machine.Id, ManagementOrigin.Library, core.Id, 0),
                        Is.EqualTo(MachineManagementResult.Completed));

                    // 安装换了分组，但实例还在——选中必须保留，不能因为「换了列表」就丢。
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(core.Id));
                    Assert.That(model.Snapshot.InstalledCount, Is.EqualTo(1));

                    // 领域整体释放后，界面刷新必须清掉已经不存在的内容。
                    // （组件实例目前没有单独的移除入口，所以这里走「花名册整体释放」这条真实路径。）
                    fixture.Roster.Dispose();
                    model.Refresh();
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(PersistentId.Invalid),
                        "实例没了就必须清掉选中——不能继续展示一个不存在的组件。");
                    Assert.That(model.Snapshot.LooseCount, Is.Zero);
                    Assert.That(model.Snapshot.InstalledCount, Is.Zero);
                }
            }
        }

        [Test]
        public void ClearSelection_IsHonouredAndStaysCleared()
        {
            using (var fixture = new Fixture())
            {
                ComponentInstance core = fixture.CreateCore();
                using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
                {
                    Assert.That(model.Select(core.Id), Is.True);
                    model.ClearSelection();
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(PersistentId.Invalid));

                    // 刷新不得把选中偷偷选回来（算法域踩过同一个坑）。
                    model.Refresh();
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(PersistentId.Invalid));
                }
            }
        }

        [Test]
        public void IdenticalLooseComponents_MergeIntoOneRow_AndKeepEveryInstance()
        {
            using (var fixture = new Fixture())
            {
                ComponentInstance a = fixture.CreateCore();
                ComponentInstance b = fixture.CreateCore();
                ComponentInstance c = fixture.CreateCore();
                // 一件不同型号的散件（同型号不同等级也是不同的东西，见下一条用例）。
                fixture.CreateArm();

                using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
                {
                    ComponentDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.LooseCount, Is.EqualTo(4), "个体数：4 件散件。");
                    Assert.That(snapshot.LooseGroupCount, Is.EqualTo(2),
                        "显示行数：3 件核心合并成 1 行，机械臂单独 1 行。");

                    UiComponentGroup core = snapshot.LooseGroups[0];
                    Assert.That(core.Name, Is.EqualTo("Component.Core"));
                    Assert.That(core.Count, Is.EqualTo(3));
                    Assert.That(core.Label, Does.Contain("× 3"), "合并行必须显示数量。");
                    CollectionAssert.AreEquivalent(new[] { a.Id, b.Id, c.Id }, core.Members,
                        "展开后必须保留每一个实例的稳定身份，而不是只留一个计数。");

                    // 选中一整组：详情给出数量与成员实例逐个列出。
                    Assert.That(model.SelectGroup(core.Kind, core.ModelId, core.Level), Is.True);
                    Assert.That(model.Snapshot.HasGroupSelection, Is.True);
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(PersistentId.Invalid),
                        "选组与选实例是同一条选中槽，选组时实例选中必须清掉。");
                    Assert.That(Value(model.Snapshot.Detail, "数量"), Is.EqualTo("3"));
                    string members = Value(model.Snapshot.Detail, "实例");
                    StringAssert.Contains(a.Id.Value.ToString(), members);
                    StringAssert.Contains(c.Id.Value.ToString(), members);
                    Assert.That(Value(model.Snapshot.Detail, "算力"), Is.EqualTo("50"), "组详情仍要带目录规格。");

                    // 选具体实例：组选中让位。
                    Assert.That(model.Select(b.Id), Is.True);
                    Assert.That(model.Snapshot.HasGroupSelection, Is.False);
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(b.Id));
                }
            }
        }

        [Test]
        public void SameModelDifferentLevels_AreNotMerged()
        {
            using (var fixture = new Fixture())
            {
                fixture.CreateComponent(new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 50, 40, false));
                fixture.CreateComponent(new ComponentDefinition(2001, HardwareKind.Core, 2, 0, 75, 50, false));

                using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
                {
                    Assert.That(model.Snapshot.LooseGroupCount, Is.EqualTo(2),
                        "同一型号的不同等级是两种东西——按型号合并会把升级关系显示成重复堆叠。");
                    Assert.That(model.Snapshot.LooseGroups[0].Count, Is.EqualTo(1));
                    Assert.That(model.Snapshot.LooseGroups[1].Count, Is.EqualTo(1));
                    Assert.That(model.Snapshot.LooseGroups[0].Level, Is.Not.EqualTo(model.Snapshot.LooseGroups[1].Level));
                }
            }
        }

        [Test]
        public void SelectingAnUnknownGroup_FailsAndKeepsTheSelection()
        {
            using (var fixture = new Fixture())
            {
                ComponentInstance core = fixture.CreateCore();
                using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
                {
                    Assert.That(model.Select(core.Id), Is.True);
                    Assert.That(model.SelectGroup(HardwareKind.Core, 9999, 1), Is.False);
                    Assert.That(model.Snapshot.SelectedId, Is.EqualTo(core.Id), "失败的选组不得改变已有选中。");
                    Assert.That(model.Snapshot.HasGroupSelection, Is.False);
                }
            }
        }

        [Test]
        public void SelectingAnUnknownComponent_FailsAndKeepsTheCurrentSelection()
        {
            using (var fixture = new Fixture())
            using (IComponentReadModel model = ComponentReadModels.Create(fixture.Session(), Catalog()))
            {
                Assert.That(model.Select(new PersistentId(9999)), Is.False);
                Assert.That(model.Snapshot.SelectedId, Is.EqualTo(PersistentId.Invalid));
                Assert.That(model.Snapshot.HasSelection, Is.False);
            }
        }

        [Test]
        public void UnavailableModel_SelectionIsNoOpAndDisposeIsSafe()
        {
            using (IComponentReadModel model = ComponentReadModels.Create(null))
            {
                int notifications = 0;
                model.Changed += _ => notifications++;

                Assert.That(model.Select(new PersistentId(1)), Is.False);
                model.ClearSelection();
                model.Refresh();
                model.Dispose();

                Assert.That(notifications, Is.Zero);
            }
        }

        private static string[] Labels(System.Collections.Generic.IReadOnlyList<UiDetailField> fields)
        {
            var labels = new string[fields.Count];
            for (int i = 0; i < fields.Count; i++) labels[i] = fields[i].Label;
            return labels;
        }

        private static string Value(System.Collections.Generic.IReadOnlyList<UiDetailField> fields, string label)
        {
            for (int i = 0; i < fields.Count; i++) if (fields[i].Label == label) return fields[i].Value;
            return null;
        }

        /// <summary>真实数据表里的组件目录；与生产走同一条解析路径。</summary>
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

            public MachineInstance CreateMachine() => Roster.Create(
                new MachineDefinition(1, "组件夹具机", 1, 2, 1, 1, 20, false, false, 100, 2d, 2d));

            /// <summary>2001 是数据表里真实存在的核心型号（Id 20011 对应的 ModelId）。</summary>
            public ComponentInstance CreateCore() => Roster.CreateComponent(
                new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 50, 40, false));

            /// <summary>2201 = 机械臂（效应器，带行为）。不同型号用来验证「不会被合并」。</summary>
            public ComponentInstance CreateArm() => Roster.CreateComponent(
                new ComponentDefinition(2201, HardwareKind.Effector, 1, 0, 0, 0, true));

            public ComponentInstance CreateComponent(ComponentDefinition definition) =>
                Roster.CreateComponent(definition);

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
