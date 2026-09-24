using System.Collections.Generic;
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

            // 合并适配：b19 在 IAlgorithmCommandSink 上新增的货物/任务查询成员。
            // 本文件用例不覆盖货舱与任务查询，桩实现按接口语义返回 false。
            public bool TryReadCargo(string field, string itemType, out AlgorithmValue value)
            {
                value = null;
                return false;
            }

            public bool TryQueryTask(string name, out AlgorithmValue task)
            {
                task = null;
                return false;
            }
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
        public void MachineDomain_WithInstance_ExposesGraphNodesEdgesAndIssues()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity,
                    out AlgorithmPlan plan, out _), Is.True, "夹具图必须能编译。");
                runtime.Instances.Add(new AlgorithmRuntime(new PersistentId(700), plan, runtime.Context.Compute, runtime.Adapter));

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.State, Is.EqualTo(UiDataState.Ready));

                    // Graph() = Startup/Constant/SetVariable/Log 四节点，三条连线。
                    Assert.That(snapshot.GraphNodes, Is.Not.Null);
                    Assert.That(snapshot.GraphNodeCount, Is.EqualTo(4));
                    Assert.That(snapshot.GraphEdges, Is.Not.Null);
                    Assert.That(snapshot.GraphEdgeCount, Is.EqualTo(3));

                    // 干净图编译通过 → 无错误无警告。
                    Assert.That(snapshot.Issues, Is.Not.Null);
                    Assert.That(snapshot.IssueCount, Is.Zero);

                    // 节点行带可展示标签。
                    Assert.That(snapshot.GraphNodes[0].Label, Does.Contain("Startup"));
                }
            }
        }

        [Test]
        public void MachineDomain_SelectNode_SetsNodeDetail_ClearSelectionClearsIt()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity, out AlgorithmPlan plan, out _);
                runtime.Instances.Add(new AlgorithmRuntime(new PersistentId(700), plan, runtime.Context.Compute, runtime.Adapter));

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;

                    // 默认未选中节点：详情为空。
                    Assert.That(snapshot.SelectedNode, Is.Null);
                    Assert.That(snapshot.NodeDetail, Is.Null.Or.Empty);

                    // 选中存在的节点 #3（SetVariable）。
                    Assert.That(model.SelectNode(3), Is.True);
                    snapshot = model.Snapshot;
                    Assert.That(snapshot.SelectedNode, Is.Not.Null);
                    Assert.That(snapshot.SelectedNode.Value.Id, Is.EqualTo(3UL));
                    Assert.That(snapshot.NodeDetail, Is.Not.Null.And.Not.Empty, "选中节点必须给出属性详情。");

                    // 不存在的节点不得被选中。
                    Assert.That(model.SelectNode(999), Is.False);

                    // 清空实例选中同时清空节点选中与图快照。
                    model.ClearSelection();
                    snapshot = model.Snapshot;
                    Assert.That(snapshot.SelectedNode, Is.Null);
                    Assert.That(snapshot.GraphNodeCount, Is.Zero, "未选中实例时图快照应为空。");
                }
            }
        }

        [Test]
        public void MachineDomain_InvalidDraft_ReportsValidationIssues()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity, out AlgorithmPlan plan, out _);
                runtime.Instances.Add(new AlgorithmRuntime(new PersistentId(700), plan, runtime.Context.Compute, runtime.Adapter));

                // 把草稿改成一条指向不存在节点的悬空连线（DanglingEdge）。
                var invalid = new AlgorithmDocument { DocumentId = 700 };
                invalid.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
                invalid.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Log });
                invalid.Edges.Add(new AlgorithmEdge { From = 1, To = 2, Output = "event", Input = "event" });
                invalid.Edges.Add(new AlgorithmEdge { From = 2, To = 999 });
                Assert.That(runtime.Instances.Edit(700, plan.Revision, invalid), Is.True, "把草稿改成悬空连线图必须成功。");

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.IssueCount, Is.GreaterThan(0), "悬空连线草稿必须有校验问题。");
                    bool hasError = false;
                    for (int i = 0; i < snapshot.Issues.Count; i++)
                    {
                        if (snapshot.Issues[i].IsError)
                        {
                            hasError = true;
                            break;
                        }
                    }

                    Assert.That(hasError, Is.True, "悬空连线应是错误而非警告。");
                }
            }
        }

        [Test]
        public void MachineDomain_AfterRun_ExposesLatestRun_AndMarksExecutedNodes()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity, out AlgorithmPlan plan, out _), Is.True);
                // 独立算力池 + 桩 sink：规避机器运行时既有算力占用对「运行产出历史」断言的干扰。
                var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
                var instance = new AlgorithmRuntime(new PersistentId(700), plan, pool, new Sink());
                Assert.That(runtime.Instances.Add(instance), Is.True, "实例服务必须接受这个运行时。");

                // 运行一次：从 Startup 触发整批求值。
                Assert.That(instance.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }), Is.True);
                instance.Pump(0);
                Assert.That(instance.History().Length, Is.GreaterThan(0), "夹具运行必须产出历史。");

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.LatestRun, Is.Not.Null, "有运行历史时快照必须暴露最近运行摘要。");
                    Assert.That(snapshot.LatestRun.Value.Succeeded, Is.True, "夹具图运行应正常无错误。");
                    Assert.That(snapshot.LatestRun.Value.FailedNode, Is.Zero);

                    int executed = 0;
                    for (int i = 0; i < snapshot.GraphNodes.Count; i++)
                    {
                        if (snapshot.GraphNodes[i].Diagnostic == UiAlgorithmNodeDiagnostic.Executed)
                        {
                            executed++;
                        }
                    }

                    Assert.That(executed, Is.GreaterThan(0), "执行路径至少命中一个图节点。");
                }
            }
        }

        [Test]
        public void MachineDomain_FailedRun_MarksFailedNode()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph(divideByZero: true);
                graph.DocumentId = 700;
                Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity, out AlgorithmPlan plan, out _), Is.True);
                var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
                var instance = new AlgorithmRuntime(new PersistentId(700), plan, pool, new Sink());
                Assert.That(runtime.Instances.Add(instance), Is.True, "实例服务必须接受这个运行时。");

                Assert.That(instance.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }), Is.True);
                instance.Pump(0);
                Assert.That(instance.History().Length, Is.GreaterThan(0));
                Assert.That(instance.History()[0].FailedNode, Is.EqualTo(6UL), "除零图应在节点 6 失败。");

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.LatestRun, Is.Not.Null);
                    Assert.That(snapshot.LatestRun.Value.Succeeded, Is.False);
                    Assert.That(snapshot.LatestRun.Value.FailedNode, Is.EqualTo(6UL));

                    UiAlgorithmNodeRow? failed = null;
                    for (int i = 0; i < snapshot.GraphNodes.Count; i++)
                    {
                        if (snapshot.GraphNodes[i].Id == 6UL)
                        {
                            failed = snapshot.GraphNodes[i];
                        }
                    }

                    Assert.That(failed.HasValue, Is.True, "除零图应包含节点 6。");
                    Assert.That(failed.Value.Diagnostic, Is.EqualTo(UiAlgorithmNodeDiagnostic.Failed));
                }
            }
        }

        [Test]
        public void MachineDomain_NoHistory_LatestRunIsNull()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity, out AlgorithmPlan plan, out _), Is.True);
                var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
                Assert.That(runtime.Instances.Add(new AlgorithmRuntime(new PersistentId(700), plan, pool, new Sink())), Is.True);

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.LatestRun, Is.Null, "未运行时不得伪造运行记录。");
                    for (int i = 0; i < snapshot.GraphNodes.Count; i++)
                    {
                        Assert.That(snapshot.GraphNodes[i].Diagnostic, Is.EqualTo(UiAlgorithmNodeDiagnostic.None));
                    }
                }
            }
        }

        [Test]
        public void LibraryDomain_WithWorld_ReturnsSeededSystemTemplates_AndDefaultsToFirst()
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            using (context)
            {
                Assert.That(context.TryCreateWorldSession(0L, out AutoEraWorldSession world), Is.True);
                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(
                    AutoEraUiSession.ForWorld(context, world), AlgorithmReadModelDomain.Library))
                {
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.State, Is.EqualTo(UiDataState.Ready),
                        "进了世界就有模板库——库页应是 Ready 而不是 Unavailable。");
                    Assert.That(snapshot.Count, Is.GreaterThanOrEqualTo(5), "世界创建时应种子化五套系统模板。");

                    int systemCount = 0;
                    for (int i = 0; i < snapshot.Templates.Count; i++)
                    {
                        if (snapshot.Templates[i].IsSystem) systemCount++;
                    }
                    Assert.That(systemCount, Is.GreaterThanOrEqualTo(5), "五套开局模板都应是系统模板。");

                    Assert.That(snapshot.SelectedTemplateIndex, Is.EqualTo(0), "有模板时默认选中第一行。");
                    Assert.That(snapshot.SelectedTemplate, Is.Not.Null);
                    Assert.That(snapshot.TemplateDetail, Is.Not.Null.And.Not.Empty, "选中模板必须给出可展示的详情。");
                    Assert.That(snapshot.Instances, Is.Null, "库页不读机器实例，实例列表应为空。");
                }
            }
        }

        [Test]
        public void LibraryDomain_SelectTemplate_ChangesDetail_ClearSelectionAndUnknownId()
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            using (context)
            {
                Assert.That(context.TryCreateWorldSession(0L, out AutoEraWorldSession world), Is.True);
                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(
                    AutoEraUiSession.ForWorld(context, world), AlgorithmReadModelDomain.Library))
                {
                    IReadOnlyList<UiAlgorithmTemplateRow> templates = model.Snapshot.Templates;
                    Assert.That(templates.Count, Is.GreaterThanOrEqualTo(2));
                    ulong firstId = templates[0].Id;
                    ulong secondId = templates[1].Id;

                    Assert.That(model.SelectTemplate(secondId), Is.True, "存在的模板必须可选中。");
                    Assert.That(model.Snapshot.SelectedTemplate.Value.Id, Is.EqualTo(secondId));

                    model.ClearSelection();
                    Assert.That(model.Snapshot.SelectedTemplate, Is.Null, "清空后不得再替他选回来。");

                    Assert.That(model.SelectTemplate(999999UL), Is.False, "不存在的模板不得被选中。");
                    Assert.That(model.Select(firstId), Is.False, "库页 Select 是实例语义，按实例 Id 选中恒为 false。");
                }
            }
        }

        [Test]
        public void LibraryDomain_WithoutWorld_ReportsUnavailable()
        {
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory());
            using (context)
            {
                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(
                    AutoEraUiSession.ForApplication(context), AlgorithmReadModelDomain.Library))
                {
                    Assert.That(model.Snapshot.State, Is.EqualTo(UiDataState.Unavailable));
                    Assert.That(model.Snapshot.Count, Is.Zero, "不可用时不得伪造模板。");
                    Assert.That(model.Snapshot.UnavailableReason, Does.Contain("世界").Or.Contain("区域"));
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
