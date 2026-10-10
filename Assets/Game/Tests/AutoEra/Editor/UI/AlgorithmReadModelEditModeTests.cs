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
        public void MachineDomain_AfterRun_SelectNode_ExposesThenValue()
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
                var instance = new AlgorithmRuntime(new PersistentId(700), plan, pool, new Sink());
                Assert.That(runtime.Instances.Add(instance), Is.True);

                Assert.That(instance.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }), Is.True);
                instance.Pump(0);

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    Assert.That(model.Select(700), Is.True);
                    Assert.That(model.SelectNode(2), Is.True, "Constant 节点应可选中。");
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.NodeDetail, Is.Not.Null);

                    bool found = false;
                    for (int i = 0; i < snapshot.NodeDetail.Count; i++)
                    {
                        if (snapshot.NodeDetail[i].Label == "当时值")
                        {
                            found = true;
                            Assert.That(snapshot.NodeDetail[i].Value, Does.Contain("12"), "Constant 当时值应为 12。");
                        }
                    }

                    Assert.That(found, Is.True, "选中执行过的值节点应显示当时值。");
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

        [Test]
        public void LibraryDomain_InstantiateTemplate_CreatesDraftInstance_VisibleInMachineDomain()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                fixture.AttachRuntime();
                fixture.Select();

                ulong instanceId;
                using (IAlgorithmReadModel library = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    AlgorithmDomainSnapshot templates = library.Snapshot;
                    Assert.That(templates.Count, Is.GreaterThan(0), "世界应有种子模板。");
                    instanceId = library.InstantiateTemplate(templates.Templates[0].Id);
                    Assert.That(instanceId, Is.Not.Zero, "选中机器 + 有效模板必须能创建草稿实例。");
                }

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    AlgorithmDomainSnapshot snapshot = machine.Snapshot;
                    Assert.That(snapshot.State, Is.EqualTo(UiDataState.Ready));
                    Assert.That(snapshot.InstanceCount, Is.EqualTo(1));
                    Assert.That(snapshot.Instances[0].Id, Is.EqualTo(instanceId));
                    Assert.That(snapshot.Instances[0].AppliedRevision, Is.Zero, "草稿实例未应用。");
                    Assert.That(snapshot.Instances[0].Status, Does.Contain("未应用"),
                        "实例行应显示「未应用」而非「已应用 r0」。");

                    // 草稿实例可被选中，并显示图结构与「缺少绑定」校验问题。
                    Assert.That(snapshot.GraphNodeCount, Is.GreaterThan(0), "模板实例化后应有图节点。");
                    Assert.That(snapshot.IssueCount, Is.GreaterThan(0),
                        "未绑定模板实例化后必须有校验问题（缺少绑定）。");
                }
            }
        }

        [Test]
        public void MachineDomain_DraftInstance_ExposesPendingBindings_AndRejectsInventedEndpoints()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                fixture.AttachRuntime();
                fixture.Select();

                ulong instanceId;
                using (IAlgorithmReadModel library = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    instanceId = library.InstantiateTemplate(library.Snapshot.Templates[0].Id);
                    Assert.That(instanceId, Is.Not.Zero);
                }

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    AlgorithmDomainSnapshot snapshot = machine.Snapshot;
                    Assert.That(snapshot.State, Is.EqualTo(UiDataState.Ready));
                    Assert.That(snapshot.PendingBindings, Is.Not.Null);
                    Assert.That(snapshot.PendingBindingCount, Is.GreaterThan(0), "模板实例化后必须有待绑定端点。");

                    for (int i = 0; i < snapshot.PendingBindings.Count; i++)
                    {
                        Assert.That(snapshot.PendingBindings[i].Bound, Is.False, "初始端点全部待绑定。");
                    }

                    string key = snapshot.PendingBindings[0].BindingKey;
                    Assert.That(machine.Rebind(instanceId, key, 90, 91, 1), Is.False, "不存在的组件和目标不能写入草稿绑定。");
                    Assert.That(machine.Snapshot.CommandUnavailableReason, Is.Not.Empty);

                    snapshot = machine.Snapshot;
                    bool found = false;
                    for (int i = 0; i < snapshot.PendingBindings.Count; i++)
                    {
                        if (snapshot.PendingBindings[i].BindingKey == key)
                        {
                            Assert.That(snapshot.PendingBindings[i].Bound, Is.False);
                            Assert.That(snapshot.PendingBindings[i].ComponentId, Is.Zero);
                            found = true;
                        }
                    }

                    Assert.That(found, Is.True, "被拒绝的端点仍在清单里等待真实绑定。");
                }
            }
        }

        [Test]
        public void MachineDomain_ActivateDraft_CompilesBoundDraft_AndRejectsUnbound()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                ulong instanceId;
                using (IAlgorithmReadModel library = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    instanceId = library.InstantiateTemplate(library.Snapshot.Templates[0].Id);
                    Assert.That(instanceId, Is.Not.Zero);
                }

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    // 未绑定草稿：RequiredBinding → 拒绝激活。
                    Assert.That(machine.ActivateDraft(instanceId), Is.False, "未绑定草稿必须拒绝激活。");

                    // 绑定所有端点。
                    AlgorithmDomainSnapshot snapshot = machine.Snapshot;
                    for (int i = 0; i < snapshot.PendingBindings.Count; i++)
                    {
                        Assert.That(machine.Rebind(instanceId, snapshot.PendingBindings[i].BindingKey, 90 + (ulong)i, 91, 1), Is.False);
                    }

                    // 只有数值身份而无真实硬件/提供者，仍必须拒绝。
                    Assert.That(machine.ActivateDraft(instanceId), Is.False, "虚构绑定不能通过生产激活。");
                    var valid = AlgorithmExecutionEditModeTests.Graph();
                    valid.DocumentId = instanceId;
                    Assert.That(runtime.Instances.Edit(instanceId, runtime.Instances.ReadDraft(instanceId).Revision, valid), Is.True);
                    // 有效无绑定图经读模型委托机器级激活。
                    Assert.That(machine.ActivateDraft(instanceId), Is.True, "绑定完整草稿应能激活。");
                    Assert.That(machine.Snapshot.CommandUnavailableReason, Is.Null, "Successful retry must clear an earlier command error.");

                    snapshot = machine.Snapshot;
                    Assert.That(snapshot.InstanceCount, Is.EqualTo(1));
                    Assert.That(snapshot.Instances[0].AppliedRevision, Is.GreaterThan(0UL), "激活后 AppliedRevision 应为非 0（不再是草稿）。");
                    Assert.That(snapshot.Instances[0].HasUnappliedDraft, Is.False, "激活后草稿与已应用一致。");
                }
            }
        }

        [Test]
        public void MachineDomain_Apply_DispatchesActivationThenApply()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                ulong instanceId;
                using (IAlgorithmReadModel library = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    instanceId = library.InstantiateTemplate(library.Snapshot.Templates[0].Id);
                    Assert.That(instanceId, Is.Not.Zero);
                }

                // 库页 Apply → false（不持有实例服务）。
                using (IAlgorithmReadModel lib = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    Assert.That(lib.Apply(instanceId), Is.False, "库页 Apply 恒 false。");
                }

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    // 未绑定草稿 → Apply 拒绝（走 ActivateDraft 的 RequiredBinding）。
                    Assert.That(machine.Apply(instanceId), Is.False, "未绑定草稿 Apply 必须拒绝。");

                    // 绑定所有端点 → Apply（草稿首应用）激活。
                    AlgorithmDomainSnapshot snapshot = machine.Snapshot;
                    for (int i = 0; i < snapshot.PendingBindings.Count; i++)
                    {
                        Assert.That(machine.Rebind(instanceId, snapshot.PendingBindings[i].BindingKey, 90 + (ulong)i, 91, 1), Is.False);
                    }

                    Assert.That(machine.Apply(instanceId), Is.False, "虚构绑定 Apply 必须拒绝。");
                    var valid = AlgorithmExecutionEditModeTests.Graph();
                    valid.DocumentId = instanceId;
                    Assert.That(runtime.Instances.Edit(instanceId, runtime.Instances.ReadDraft(instanceId).Revision, valid), Is.True);
                    Assert.That(machine.Apply(instanceId), Is.True, "有效草稿 Apply 应激活。");
                    snapshot = machine.Snapshot;
                    Assert.That(snapshot.Instances[0].AppliedRevision, Is.GreaterThan(0UL));

                    // 激活后改数值（草稿领先）→ Apply 走应用请求。
                    var changed = runtime.Instances.ReadDraft(instanceId);
                    changed.Nodes[1].Default.Number = 20;
                    Assert.That(runtime.Instances.Edit(instanceId, changed.Revision, changed), Is.True);
                    Assert.That(machine.Apply(instanceId), Is.True, "已激活实例草稿领先 Apply 应创建应用请求。");
                }
            }
        }

        [Test]
        public void MachineDomain_ConfirmWarningsAndCancelApply_DispatchToService()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                // 干净图（无绑定）：Apply 无警告 → WaitingSafePoint（bindingsValid 对空 Bindings 返回 true）。
                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity, out var plan, out _), Is.True);
                runtime.Instances.Add(new AlgorithmRuntime(new PersistentId(700), plan, runtime.Context.Compute, runtime.Adapter));

                var draft = runtime.Instances.ReadDraft(700);
                draft.Nodes[1].Default.Number = 20;
                runtime.Instances.Edit(700, draft.Revision, draft);
                Assert.That(runtime.Instances.Apply(700, 2, 1, out var request), Is.True);
                Assert.That(request.State, Is.EqualTo(AlgorithmApplyState.WaitingSafePoint), "干净图无警告，应进入等待安全点。");

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    // CancelApply 取消等待安全点的请求。
                    Assert.That(machine.CancelApply(700, request.RequestId), Is.True);
                    Assert.That(machine.Snapshot.SelectedInstance.Value.RequestState, Is.EqualTo(AlgorithmApplyState.Cancelled));

                    // 已取消的请求不能再确认。
                    Assert.That(machine.ConfirmWarnings(700, request.RequestId), Is.False);
                }

                using (IAlgorithmReadModel lib = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    // 库页不持有实例服务：命令恒 false。
                    Assert.That(lib.ConfirmWarnings(700, 1), Is.False);
                    Assert.That(lib.CancelApply(700, 1), Is.False);
                }
            }
        }

        [Test]
        public void MachineDomain_ExposesInstalledComponentCandidates()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                fixture.AttachRuntime();
                fixture.Select();

                // 只装 Core 时，候选（Sensor/Effector）为空列表——不是 null。
                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    Assert.That(machine.Snapshot.ComponentCandidates, Is.Not.Null);
                    Assert.That(machine.Snapshot.ComponentCandidates.Count, Is.EqualTo(0));
                }

                // 装一个传感器 → 新读模型的候选应包含它。机器已部署，装组件须走现场来源（Field）。
                ComponentInstance sensor = fixture.World.Machines.CreateComponent(
                    new ComponentDefinition(30011, HardwareKind.Sensor, 1, 0, 0, 0, false));
                Assert.That(fixture.World.Machines.Install(fixture.Machine.Id, ManagementOrigin.Field, sensor.Id, 0),
                    Is.EqualTo(MachineManagementResult.Completed));

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    AlgorithmDomainSnapshot snapshot = machine.Snapshot;
                    Assert.That(snapshot.ComponentCandidates.Count, Is.GreaterThanOrEqualTo(1));
                    bool found = false;
                    for (int i = 0; i < snapshot.ComponentCandidates.Count; i++)
                    {
                        if (snapshot.ComponentCandidates[i].ComponentId == sensor.Id.Value)
                        {
                            found = true;
                            Assert.That(snapshot.ComponentCandidates[i].Kind, Is.EqualTo(HardwareKind.Sensor));
                            break;
                        }
                    }

                    Assert.That(found, Is.True, "候选应包含刚装的传感器。");
                }
            }
        }

        [Test]
        public void MachineDomain_MoveNode_DispatchToService()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(runtime.Instances.AddDraft(graph), Is.True);

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    Assert.That(machine.MoveNode(700, 1, 12.5f, -3.25f), Is.True);
                    AlgorithmDocument draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Revision, Is.EqualTo(2UL));
                    Assert.That(draft.Nodes.Find(n => n.Id == 1).LayoutX, Is.EqualTo(12.5f));
                    Assert.That(draft.Nodes.Find(n => n.Id == 1).LayoutY, Is.EqualTo(-3.25f));
                }

                using (IAlgorithmReadModel lib = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    Assert.That(lib.MoveNode(700, 1, 1, 1), Is.False, "库页不持有实例服务。");
                }
            }
        }

        [Test]
        public void MachineDomain_CreateConnectDisconnect_DispatchToService()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(runtime.Instances.AddDraft(graph), Is.True);

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    Assert.That(machine.Select(700), Is.True, "先选中实例，图快照才有内容。");

                    // 节点库添加：机器域转发实例服务，分配稳定 Id 并落到草稿。
                    ulong mathNode = machine.CreateNode(700, AlgorithmNodeKind.Arithmetic, 40f, -60f);
                    Assert.That(mathNode, Is.GreaterThan(0UL), "机器域创建节点应转发实例服务。");
                    AlgorithmDocument draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Nodes.Find(n => n.Id == mathNode), Is.Not.Null);
                    Assert.That(draft.Nodes.Find(n => n.Id == mathNode).LayoutX, Is.EqualTo(40f));
                    Assert.That(draft.Nodes.Find(n => n.Id == mathNode).LayoutY, Is.EqualTo(-60f));

                    // 强类型连线：Constant(2) "value" → 新 Arithmetic "a"。
                    Assert.That(machine.Connect(700, 2, "value", mathNode, "a"), Is.True, "兼容端口连接应转发服务。");
                    Assert.That(runtime.Instances.ReadDraft(700).Edges.Find(e => e.From == 2 && e.To == mathNode && e.Input == "a"), Is.Not.Null, "草稿应包含新边。");

                    // 读侧快照真实呈现写侧结果（图连线包含新边）。
                    machine.Refresh();
                    bool edgeVisible = false;
                    for (int i = 0; i < machine.Snapshot.GraphEdges.Count; i++)
                    {
                        if (machine.Snapshot.GraphEdges[i].From == 2 && machine.Snapshot.GraphEdges[i].To == mathNode && machine.Snapshot.GraphEdges[i].Input == "a")
                        {
                            edgeVisible = true;
                            break;
                        }
                    }
                    Assert.That(edgeVisible, Is.True, "图快照应包含新连接的边。");

                    // 不兼容连线在机器域同样被拒绝（Number → Boolean）。
                    ulong boolNode = machine.CreateNode(700, AlgorithmNodeKind.Boolean, 80f, -60f);
                    Assert.That(boolNode, Is.GreaterThan(0UL));
                    Assert.That(machine.Connect(700, 2, "value", boolNode, "a"), Is.False, "类型不兼容必须拒绝。");

                    // 精确断开：边从草稿与快照消失。
                    Assert.That(machine.Disconnect(700, 2, "value", mathNode, "a"), Is.True, "精确断开应转发服务。");
                    Assert.That(runtime.Instances.ReadDraft(700).Edges.Find(e => e.From == 2 && e.To == mathNode && e.Input == "a"), Is.Null, "断开后草稿不应再包含该边。");
                    machine.Refresh();
                    for (int i = 0; i < machine.Snapshot.GraphEdges.Count; i++)
                    {
                        if (machine.Snapshot.GraphEdges[i].To == mathNode && machine.Snapshot.GraphEdges[i].Input == "a")
                        {
                            Assert.Fail("断开后图快照不应再包含该边。");
                        }
                    }
                }

                using (IAlgorithmReadModel lib = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    Assert.That(lib.CreateNode(700, AlgorithmNodeKind.Log, 0, 0), Is.EqualTo(0UL), "库页不持有实例服务。");
                    Assert.That(lib.Connect(700, 2, "value", 3, "value"), Is.False, "库页不持有实例服务。");
                    Assert.That(lib.Disconnect(700, 2, "value", 3, "value"), Is.False, "库页不持有实例服务。");
                }
            }
        }

        [Test]
        public void MachineDomain_DeleteNode_DispatchesAndCascades()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(runtime.Instances.AddDraft(graph), Is.True);

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    Assert.That(machine.Select(700), Is.True);
                    ulong mathNode = machine.CreateNode(700, AlgorithmNodeKind.Arithmetic, 0, 0);
                    Assert.That(mathNode, Is.GreaterThan(0UL));
                    Assert.That(machine.Connect(700, 2, "value", mathNode, "a"), Is.True);

                    // 删除节点：转发服务并级联移除关联边，既有无关边保留。
                    Assert.That(machine.DeleteNode(700, mathNode), Is.True, "机器域删除节点应转发服务。");
                    AlgorithmDocument draft = runtime.Instances.ReadDraft(700);
                    Assert.That(draft.Nodes.Find(n => n.Id == mathNode).Deleted, Is.True, "节点应软删。");
                    Assert.That(draft.Edges.Find(e => e.To == mathNode), Is.Null, "关联边必须级联移除。");
                    Assert.That(draft.Edges.Find(e => e.From == 2 && e.To == 3 && e.Input == "value"), Is.Not.Null, "无关边不受影响。");

                    // 不存在节点拒绝。
                    Assert.That(machine.DeleteNode(700, 999), Is.False, "不存在的节点必须拒绝。");
                }

                using (IAlgorithmReadModel lib = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                {
                    Assert.That(lib.DeleteNode(700, 2), Is.False, "库页不持有实例服务。");
                }
            }
        }

        [Test]
        public void NodeKindLibrary_CatalogRows_IdenticalAcrossDomains_CoverEveryKind()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(runtime.Instances.AddDraft(graph), Is.True);

                using (var machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                using (var lib = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Library))
                using (var unavailable = AlgorithmReadModels.Create(null, AlgorithmReadModelDomain.Machine))
                {
                    // 目录全覆盖：每个枚举种类恰好一行。
                    var kinds = (AlgorithmNodeKind[])System.Enum.GetValues(typeof(AlgorithmNodeKind));
                    Assert.That(AlgorithmNodeLibrary.Rows.Length, Is.EqualTo(kinds.Length), "目录行数必须等于枚举种类数。");
                    for (int i = 0; i < kinds.Length; i++)
                    {
                        bool found = false;
                        for (int r = 0; r < AlgorithmNodeLibrary.Rows.Length; r++)
                        {
                            if (AlgorithmNodeLibrary.Rows[r].Kind == kinds[i]) { found = true; break; }
                        }
                        Assert.That(found, Is.True, "目录缺少种类 " + kinds[i]);
                    }

                    // 三个域读到同一份目录（能力清单而非实例数据）。
                    var machineRows = machine.Snapshot.NodeKinds;
                    var libRows = lib.Snapshot.NodeKinds;
                    var unavailableRows = unavailable.Snapshot.NodeKinds;
                    Assert.That(machineRows.Count, Is.EqualTo(AlgorithmNodeLibrary.Rows.Length), "机器域应携带目录。");
                    Assert.That(libRows.Count, Is.EqualTo(AlgorithmNodeLibrary.Rows.Length), "库域应携带目录。");
                    Assert.That(unavailableRows.Count, Is.EqualTo(AlgorithmNodeLibrary.Rows.Length), "不可用域也应携带目录。");
                    Assert.That(machineRows[0].Kind, Is.EqualTo(libRows[0].Kind));
                    Assert.That(machineRows[0].Label, Is.EqualTo(libRows[0].Label));
                    Assert.That(machineRows[0].Category, Is.Not.Null.And.Not.Empty, "目录行必须带分类。");
                }
            }
        }

        [Test]
        public void GraphNodeRows_ExposePortRowsWithConnectedState()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                Assert.That(runtime.Instances.AddDraft(graph), Is.True);

                using (IAlgorithmReadModel machine = AlgorithmReadModels.Create(fixture.Session(), AlgorithmReadModelDomain.Machine))
                {
                    Assert.That(machine.Select(700), Is.True);
                    AlgorithmDomainSnapshot snapshot = machine.Snapshot;

                    // 图：1=Startup → 3=SetVariable(event)；2=Constant → 3(value)。
                    // SetVariable 的输入端口行：event 与 value 都已连接。
                    UiAlgorithmNodeRow? setVar = null;
                    for (int i = 0; i < snapshot.GraphNodes.Count; i++)
                    {
                        if (snapshot.GraphNodes[i].Id == 3) { setVar = snapshot.GraphNodes[i]; break; }
                    }
                    Assert.That(setVar.HasValue, Is.True, "快照应包含节点 3。");

                    UiAlgorithmPortRow eventPort = default, valuePort = default; bool eventFound = false, valueFound = false;
                    for (int i = 0; i < setVar.Value.InputPorts.Count; i++)
                    {
                        if (setVar.Value.InputPorts[i].Key == "event") { eventPort = setVar.Value.InputPorts[i]; eventFound = true; }
                        if (setVar.Value.InputPorts[i].Key == "value") { valuePort = setVar.Value.InputPorts[i]; valueFound = true; }
                    }
                    Assert.That(eventFound && valueFound, Is.True, "SetVariable 应有 event 与 value 输入端口。");
                    Assert.That(eventPort.Connected, Is.True, "event 端口应有边指向。");
                    Assert.That(valuePort.Connected, Is.True, "value 端口应有边指向。");
                    Assert.That(eventPort.TypeLabel, Is.Not.Null.And.Not.Empty, "端口行必须带类型标签。");

                    // Constant(2) 的输出端口 value 已连接。
                    UiAlgorithmNodeRow? constant = null;
                    for (int i = 0; i < snapshot.GraphNodes.Count; i++)
                    {
                        if (snapshot.GraphNodes[i].Id == 2) { constant = snapshot.GraphNodes[i]; break; }
                    }
                    Assert.That(constant.HasValue, Is.True);
                    bool outputConnected = false;
                    for (int i = 0; i < constant.Value.OutputPorts.Count; i++)
                    {
                        if (constant.Value.OutputPorts[i].Key == "value" && constant.Value.OutputPorts[i].Connected) { outputConnected = true; }
                    }
                    Assert.That(outputConnected, Is.True, "Constant 的 value 输出端口应有边离开。");

                    // 无端口的种类（QueryTask）给空数组而不是 null。
                    ulong query = machine.CreateNode(700, AlgorithmNodeKind.QueryTask, 0, 0);
                    Assert.That(query, Is.GreaterThan(0UL));
                    machine.Refresh();
                    UiAlgorithmNodeRow? queryRow = null;
                    for (int i = 0; i < machine.Snapshot.GraphNodes.Count; i++)
                    {
                        if (machine.Snapshot.GraphNodes[i].Id == query) { queryRow = machine.Snapshot.GraphNodes[i]; break; }
                    }
                    Assert.That(queryRow.HasValue, Is.True);
                    Assert.That(queryRow.Value.InputPorts, Is.Not.Null);
                    Assert.That(queryRow.Value.InputPorts.Count, Is.Zero, "QueryTask 无输入端口应为空列表。");
                }
            }
        }

        [Test]
        public void MachineDomain_GraphNodes_ExposeLayoutCoordinates()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore();
                fixture.Deploy();
                RegionMachineRuntime runtime = fixture.AttachRuntime();
                fixture.Select();

                var graph = AlgorithmExecutionEditModeTests.Graph();
                graph.DocumentId = 700;
                graph.Nodes[0].LayoutX = 12.5f;
                graph.Nodes[0].LayoutY = -3.25f;
                Assert.That(runtime.Instances.AddDraft(graph), Is.True);

                using (IAlgorithmReadModel model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    Assert.That(model.Select(700), Is.True);
                    AlgorithmDomainSnapshot snapshot = model.Snapshot;
                    Assert.That(snapshot.GraphNodes, Is.Not.Null);
                    Assert.That(snapshot.GraphNodes[0].LayoutX, Is.EqualTo(12.5f));
                    Assert.That(snapshot.GraphNodes[0].LayoutY, Is.EqualTo(-3.25f));
                }
            }
        }

        [Test]
        public void AlgorithmBindingPickRequest_CarriesInstanceAndEndpoint()
        {
            var request = new AutoEraAlgorithmBindingPickRequest(700, "sensor", AlgorithmNodeKind.Input);
            Assert.That(request.InstanceId, Is.EqualTo(700UL));
            Assert.That(request.BindingKey, Is.EqualTo("sensor"));
            Assert.That(request.Kind, Is.EqualTo(AlgorithmNodeKind.Input));
            Assert.That(request.Intent, Does.Contain("sensor"));

            var effector = new AutoEraAlgorithmBindingPickRequest(701, "arm", AlgorithmNodeKind.Effector);
            Assert.That(effector.Kind, Is.EqualTo(AlgorithmNodeKind.Effector));
            Assert.That(effector.Intent, Does.Contain("效应器"));
        }

        /// <summary>
        /// 一套「世界 + 区域 + 一台不可移动机器」的夹具。
        /// 用不可移动型号是刻意的：它不需要导航面，也就把「算力与传感器与移动无关」这条
        /// 顺带钉住——算法域在这台机器上同样成立。
        /// </summary>
        [Test]
        public void DiagnosticRecord_RetainsExecutedRevisionAndValues_WhenDraftChanges()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore(); fixture.Deploy(); var runtime = fixture.AttachRuntime(); fixture.Select();
                fixture.Machine.Activate(ManagementOrigin.Field); fixture.Machine.UpdateEnvironment(true, true);
                fixture.Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                var graph = AlgorithmExecutionEditModeTests.Graph(); graph.DocumentId = 700;
                runtime.Instances.AddDraft(graph); Assert.That(runtime.TryActivateDraft(700, out _), Is.True);
                using (var model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    fixture.Advance(10);
                    Assert.That(model.Snapshot.LatestRun.HasValue, Is.True, "Run notification must update the UI without a manual Refresh.");
                    var record = runtime.Instances.ReadHistory(700)[0];
                    var old = record.CopyExecutedDocument(); old.Nodes.Clear();
                    Assert.That(record.CopyExecutedDocument().Nodes, Is.Not.Empty, "Returned documents must not mutate recorded history.");
                    var draft = runtime.Instances.ReadDraft(700); draft.Nodes[1].Default.Number = 42;
                    Assert.That(runtime.Instances.Edit(700, draft.Revision, draft), Is.True);
                    Assert.That(model.Snapshot.GraphRevision, Is.EqualTo(2));
                    foreach (var node in model.Snapshot.GraphNodes) Assert.That(node.Diagnostic, Is.EqualTo(UiAlgorithmNodeDiagnostic.None));
                    model.SetDiagnosticView(true);
                    Assert.That(model.Snapshot.GraphRevision, Is.EqualTo(record.Revision));
                    Assert.That(model.Snapshot.LatestRun.Value.WorldMilliseconds, Is.EqualTo(record.CopyTrigger().Time));
                    Assert.That(model.SelectNode(2), Is.True);
                    Assert.That(model.Snapshot.NodeDetail, Has.Some.Matches<UiDetailField>(f => f.Label == "当时值"));
                    model.SetDiagnosticView(false);
                    Assert.That(model.Snapshot.NodeDetail, Has.None.Matches<UiDetailField>(f => f.Label == "当时值"));
                }
            }
        }

        [Test]
        public void InvalidDraftApply_PreservesRunningRevisionAndReportsReadableReason()
        {
            using (var fixture = new Fixture())
            {
                fixture.InstallCore(); fixture.Deploy(); var runtime = fixture.AttachRuntime(); fixture.Select();
                var graph = AlgorithmExecutionEditModeTests.Graph(); graph.DocumentId = 700;
                runtime.Instances.AddDraft(graph); Assert.That(runtime.TryActivateDraft(700, out _), Is.True);
                using (var model = AlgorithmReadModels.Create(fixture.Session()))
                {
                    Assert.That(model.Connect(700, 2, "value", 3, "event"), Is.False, "Number cannot connect to Event.");
                    var draft = runtime.Instances.ReadDraft(700); draft.Edges.RemoveAll(e => e.To == 3 && e.Input == "event");
                    draft.Edges.Add(new AlgorithmEdge { From = 2, Output = "value", To = 3, Input = "event" });
                    Assert.That(runtime.Instances.Edit(700, draft.Revision, draft), Is.True);
                    Assert.That(model.Apply(700), Is.True, "A request is created; its validation result must be Rejected.");
                    Assert.That(model.Snapshot.SelectedInstance.Value.AppliedRevision, Is.EqualTo(1));
                    Assert.That(model.Snapshot.SelectedInstance.Value.RequestState, Is.EqualTo(AlgorithmApplyState.Rejected));
                    Assert.That(model.Snapshot.Issues, Has.Some.Matches<UiAlgorithmIssueRow>(r => r.Label.Contains("兼容")));
                }
            }
        }

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
            public void Advance(long now) => _runtimes.AdvanceWorldStep(now, 1);

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
