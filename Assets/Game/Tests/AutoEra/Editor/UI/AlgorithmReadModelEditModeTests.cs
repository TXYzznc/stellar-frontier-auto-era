using AutoEra.Algorithms;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using AutoEra.World.Time;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 算法域读取模型的状态契约。
    ///
    /// 接线前后这条链的性质完全不同，所以断言也换了对象：
    /// **接线前**「世界已就绪」也只能报 Unavailable（服务没有创建者）；
    /// **接线后**机器运行时是真实的（`RegionMachineRuntimeRegistry` 在部署时创建
    /// `MachineExecutionContext` 与 `AlgorithmInstanceService`），于是读模型必须区分三种状态：
    /// Unavailable（缺能力，各自可辨）／Empty（运行时在、实例还没有）／Ready（有真实实例）。
    ///
    /// 「各自可辨」是这些用例的核心：把「没选机器」「选了但没运行时」「域没接线」混成一句
    /// 笼统的不可用，排查的人就得回到代码里找接线点。
    /// </summary>
    public sealed class AlgorithmReadModelEditModeTests
    {
        private sealed class Sink : IAlgorithmCommandSink
        {
            public bool Safe = true;
            public bool IsSafe => Safe;
            public ulong Submit(AlgorithmTrigger t, AlgorithmIntent i) => t.TaskId;
            public void EndBatch(AlgorithmTrigger t) { }
            public void Cancel() { }
        }

        [Test]
        public void MissingSession_ReportsUnavailableWithReason()
        {
            using (IAlgorithmReadModel model = AlgorithmReadModels.Create(null))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Is.Not.Null.And.Not.Empty);
                Assert.That(model.Snapshot.Count, Is.Zero, "不可用时不得伪造模板。");
                Assert.That(model.Snapshot.InstanceCount, Is.Zero, "不可用时不得伪造实例。");
            }
        }

        [Test]
        public void SessionWithoutWorld_SaysAlgorithmBelongsToAWorld()
        {
            using (var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory()))
            using (IAlgorithmReadModel model = AlgorithmReadModels.Create(AutoEraUiSession.ForApplication(context)))
            {
                Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(model.Snapshot.UnavailableReason, Does.Contain("区域").Or.Contain("世界"),
                    "世界外的原因要说清「算法属于某个世界」，而不是笼统报不可用。");
            }
        }

        [Test]
        public void WorldWithoutRegion_SaysTheRegionIsMissing()
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            using (context)
            {
                Assert.That(context.TryCreateWorldSession(0L, out AutoEraWorldSession world), Is.True);
                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(AutoEraUiSession.ForWorld(context, world)))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                    Assert.That(model.Snapshot.UnavailableReason, Does.Contain("现场区域"),
                        "有了世界但没有区域时，缺的是区域——原因必须指到这一层。");
                }
            }
        }

        [Test]
        public void RegionWithoutRuntimes_SaysTheRuntimeRegistryIsMissing()
        {
            using (var fixture = new Fixture())
            {
                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(
                    AutoEraUiSession.ForWorld(fixture.Context, fixture.World, fixture.Region)))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                    Assert.That(model.Snapshot.UnavailableReason, Does.Contain("机器运行时"),
                        "区域在但运行时注册表不在时，缺的是运行时。");
                }
            }
        }

        [Test]
        public void NoSelectedMachine_SaysAMachineMustBeSelected()
        {
            using (var fixture = new Fixture())
            {
                fixture.Deploy();
                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                    Assert.That(model.Snapshot.UnavailableReason, Does.Contain("选中的机器"),
                        "没选机器时必须说「先选一台」，而不是随便挑一台展示。");
                    Assert.That(model.Snapshot.MachineId, Is.EqualTo(PersistentId.Invalid));
                }
            }
        }

        [Test]
        public void SelectedMachineWithoutRuntime_SaysTheRuntimeIsRequired()
        {
            using (var fixture = new Fixture())
            {
                // 部署并选中，但**不建运行时**：这正是「区域刚重建、机器还没接上」的形态。
                fixture.Deploy();
                fixture.Select();

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                    Assert.That(model.Snapshot.UnavailableReason, Does.Contain("还没有运行时"));
                }
            }
        }

        [Test]
        public void RuntimeWithoutInstances_IsEmptyNotUnavailable_AndStillCarriesTheMachine()
        {
            using (var fixture = new Fixture())
            {
                fixture.Deploy();
                fixture.AttachRuntime();
                fixture.Select();

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    // Empty 而不是 Unavailable：领域接线了，只是这台机器还没有算法实例。
                    Assert.That(snapshot.State, Is.EqualTo(UiDataState.Empty),
                        "「域没接线」与「接线了但还没有实例」必须分开——混成一个状态界面就在撒谎。");
                    Assert.That(snapshot.InstanceCount, Is.Zero);
                    Assert.That(snapshot.MachineId, Is.EqualTo(fixture.MachineId));
                    Assert.That(snapshot.MachineName, Is.Not.Null.And.Not.Empty);
                    Assert.That(snapshot.UnavailableReason, Is.EqualTo(AlgorithmReadModels.NoInstanceReason));
                    Assert.That(snapshot.Detail, Is.Not.Null.And.Not.Empty,
                        "即使没有实例，机器与运行时的真实状态也必须可展示。");
                    Assert.That(model.Select(999999), Is.False, "不存在的实例不得被选中。");
                    Assert.That(model.SelectedIndex, Is.EqualTo(-1));
                }
            }
        }

        [Test]
        public void RuntimeWithAnInstance_IsReadyAndExposesVersionAndCost()
        {
            using (var fixture = new Fixture())
            {
                // 装机必须在**部署前**：`HardwareGate` 规定已部署机器只接受 Field 来源的改动，
                // 库来源的装配只能发生在未部署状态。运行时（以及算力池）在部署后才建立，
                // 所以这个顺序同时也保证了池子拿到的是装好核心之后的容量。
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity,
                    out AlgorithmPlan plan, out _), Is.True, "夹具图必须能在本机的逻辑算力预算内编译。");
                var instance = new AlgorithmRuntime(new PersistentId(700), plan, runtime.Context.Compute, runtime.Adapter);
                Assert.That(runtime.Instances.Add(instance), Is.True, "实例服务必须接受这个运行时。");

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.State, Is.EqualTo(UiDataState.Ready));
                    Assert.That(snapshot.InstanceCount, Is.EqualTo(1));
                    Assert.That(snapshot.SelectedIndex, Is.EqualTo(0), "有实例时默认选中第一行，详情栏才有内容。");
                    Assert.That(snapshot.SelectedInstance, Is.Not.Null);
                    Assert.That(snapshot.SelectedInstance.Value.Id, Is.EqualTo(700UL));
                    Assert.That(snapshot.SelectedInstance.Value.AppliedRevision, Is.EqualTo(plan.Revision));
                    Assert.That(snapshot.HasSelection, Is.False, "模板列表仍为空，HasSelection 说的是模板。");

                    // 选中是按**稳定 Id** 的：行号变了也不会选错对象。
                    Assert.That(model.Select(700), Is.True);
                    Assert.That(model.SelectedIndex, Is.EqualTo(0));
                    model.ClearSelection();
                    Assert.That(model.SelectedIndex, Is.EqualTo(-1));
                }
            }
        }

        [Test]
        public void UnavailableModel_SelectionIsNoOpAndDisposeIsSafe()
        {
            using (IAlgorithmReadModel model = AlgorithmReadModels.Create(null))
            {
                int notifications = 0;
                model.Changed += _ => notifications++;

                Assert.That(model.Select(1), Is.False, "没有数据时选中必须失败而不是假装成功。");
                model.ClearSelection();
                model.Refresh();
                model.Dispose();

                Assert.That(model.SelectedIndex, Is.EqualTo(-1));
                Assert.That(notifications, Is.Zero);
            }
        }

        /// <summary>
        /// 一套「世界 + 区域 + 一台不可移动机器」的夹具。
        /// 用不可移动型号是刻意的：它不需要导航面，也就把「算力与传感器与移动无关」这条
        /// 顺带钉住——算法域在这台机器上同样成立。
        /// </summary>
        private sealed class Fixture : System.IDisposable
        {
            private readonly RegionMachineRuntimeRegistry _runtimes;

            public Fixture()
            {
                Context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
                Assert.That(Context.TryCreateWorldSession(0L, out AutoEraWorldSession world), Is.True);
                World = world;
                Region = new InitialRegion(World, new Rect(-30, -30, 60, 60));
                _runtimes = new RegionMachineRuntimeRegistry(World, Region, null, new MachineNavigationSettings(), .32f, 1.8f);
                Machine = World.Machines.Create(new MachineDefinition(1, "算法夹具机", 1, 2, 1, 1, 20, false, false, 100, 2d, 2d));
            }

            public AutoEraApplicationContext Context { get; }
            public AutoEraWorldSession World { get; }
            public InitialRegion Region { get; }
            public MachineInstance Machine { get; }
            public PersistentId MachineId => Machine.Id;

            public void Deploy() => Assert.That(
                Region.DeployMachine(Machine.Id, new Vector2(5, 5), Machine.Definition.Footprint, out _),
                Is.EqualTo(RegionMachineDeploymentResult.Bound));

            /// <summary>
            /// 装一颗核心。**必须的**：机器的算力／逻辑算力是「已装组件之和」，
            /// 裸机是 0/0，于是 `MachineComputePool` 一分预算都没有，任何算法实例都加不进去。
            /// </summary>
            public void InstallCore()
            {
                ComponentInstance core = World.Machines.CreateComponent(
                    new ComponentDefinition(20011, HardwareKind.Core, 1, 0, 10, 10, false));
                Assert.That(World.Machines.Install(Machine.Id, ManagementOrigin.Library, core.Id, 0),
                    Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(Machine.LogicCapacity, Is.GreaterThan(0));
            }

            public RegionMachineRuntime AttachRuntime()
            {
                Assert.That(_runtimes.TryAttach(Machine, null, out RegionMachineRuntime runtime, out string reason),
                    Is.True, reason);
                return runtime;
            }

            public void Select() => Assert.That(Region.Select(Machine.Id, false), Is.True);

            public AutoEraUiSession Session() => AutoEraUiSession.ForWorld(Context, World, Region, null, _runtimes);

            public void Dispose()
            {
                _runtimes.Dispose();
                Region.Dispose();
                Context.ReleaseActiveWorldSession();
                Context.Dispose();
            }
        }
    }
}
