using System.Collections;
using AutoEra.Algorithms;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 批次 1 的**垂直切片**验收（变更 `region-machine-deployment-runtime` 6.4）：
    ///
    /// 部署 → 区域对象 → 视图不重复注册 → 导航绑定 → 运行时出现 → 算法界面 Ready，
    /// 全部在**同一次运行**里走一遍，并在尾部覆盖三条导航降级路径。
    ///
    /// 为什么要有这一条，尽管每一段都有自己的用例：分段用例各自绿并不等于**它们连起来是通的**——
    /// 这条链的接点（部署后谁建运行时、视图绑定会不会重复注册、算法界面从哪台机器取数）
    /// 恰好都是「两边都对但中间没接」最容易出问题的地方。所以这里断言的是**事实的传递**：
    /// 部署产生的那一个区域对象，就是视图指着的那个对象、就是运行时对应的那台机器、
    /// 就是算法界面正在读的那台机器。
    /// </summary>
    public sealed class MachineDeploymentDataFlowPlayModeTests
    {
        private const string LaunchSceneName = "Launch";
        private const string RegionSceneName = "InitialRegion";

        [UnityTest]
        public IEnumerator DeployToAlgorithm_KeepsOneIdentityAcrossEveryLayer_AndDegradesHonestly()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();
            yield return Verify();
        }

        private static IEnumerator Verify()
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True, "机器数据表必须在运行期就绪。");

            AsyncOperation loading = SceneManager.LoadSceneAsync(RegionSceneName, LoadSceneMode.Additive);
            Assert.That(loading, Is.Not.Null, "InitialRegion 必须在 Build Settings 里启用。");
            while (!loading.isDone) yield return null;
            Scene scene = SceneManager.GetSceneByName(RegionSceneName);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            InitialRegionScene entry = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            var input = entry.GetComponent<AutoEra.Input.RegionInputModule>();
            Assert.That(input, Is.Not.Null);

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int editorFormId = 0;
                bool editorOpen = false;
                GameObject bare = null;
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);

                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition wheeled), Is.True);
                    MachineInstance carrier = session.Machines.Create(wheeled);

                    // 装一颗核心（库来源必须在部署前）：逻辑算力是「已装组件之和」，
                    // 裸机是 0，算法实例就加不进去——这条链的末端需要它。
                    ComponentInstance core = session.Machines.CreateComponent(
                        new ComponentDefinition(20011, HardwareKind.Core, 1, 0, 10, 10, false));
                    Assert.That(session.Machines.Install(carrier.Id, ManagementOrigin.Library, core.Id, 0),
                        Is.EqualTo(MachineManagementResult.Completed));

                    // ① 部署：产生**一个**区域对象（场景自身还有种子对象，所以比的是增量）。
                    int objectsBefore = entry.Region.Count;
                    using (var flow = new MachineDeploymentFlow(session, entry.Region))
                    {
                        Assert.That(flow.TryBegin(carrier.Id, out string begin), Is.True, begin);
                        flow.Preview.Move(new Vector2(20, -25));
                        Assert.That(flow.Preview.IsValid, Is.True, flow.Preview.Reason);
                        Assert.That(flow.TryCommit(out RegionObject deployed, out string commit), Is.True, commit);
                        Assert.That(deployed, Is.Not.Null);
                    }

                    int objectsAfterDeploy = entry.Region.Count;
                    Assert.That(objectsAfterDeploy, Is.EqualTo(objectsBefore + 1), "部署只多出这一台机器的对象。");
                    Assert.That(carrier.Deployed, Is.True);

                    // ② 表现：实体生成、视图指向**那一个**对象（不得再注册一个）。
                    Assert.That(entry.TrySpawnMachine(carrier.Id, out string spawn), Is.True, spawn);
                    RegionObjectView view = null;
                    until = Time.realtimeSinceStartupAsDouble + 20;
                    while (view == null && Time.realtimeSinceStartupAsDouble < until)
                    {
                        view = entry.FindMachineView(carrier.Id);
                        if (view == null) yield return null;
                    }

                    Assert.That(view, Is.Not.Null, "机器实体必须生成并完成视图绑定。");
                    Assert.That(entry.Region.Count, Is.EqualTo(objectsAfterDeploy),
                        "绑定视图绝不能新建区域对象——那正是重复注册这个陷阱。");
                    Assert.That(view.Model.Id, Is.EqualTo(carrier.Id), "视图必须指着部署产生的那个对象。");

                    // ③ 运行时 + 真实导航绑定。
                    Assert.That(entry.MachineRuntimes, Is.Not.Null);
                    Assert.That(entry.MachineRuntimes.TryGet(carrier.Id, out RegionMachineRuntime runtime), Is.True,
                        "部署成功 + 视图就绪之后必须出现运行时。");
                    Assert.That(runtime.HasNavigation, Is.True, runtime.NavigationUnavailableReason);
                    Assert.That(runtime.IsNavigationDegraded, Is.False);
                    Assert.That(entry.Navigation.BindingCount, Is.EqualTo(1));

                    // ④ 算法界面：机器身份来自区域选中对象；先 Empty（没有实例），后 Ready（有实例）。
                    Assert.That(entry.Region.Select(carrier.Id, false), Is.True,
                        "算法界面按稳定身份取机器，所以必须先选中它。");

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(context, session, entry.Region, input,
                        entry.MachineRuntimes);
                    UIParams parameters = uiSession.WriteTo(UIParams.Create());
                    parameters.Set(AutoEraUiParamKeys.Request, new AutoEraUiPageRequest(AlgorithmEditorForm.PageEditor));
                    editorFormId = GF.UI.OpenUIForm(UIViews.AlgorithmEditorForm, parameters);
                    editorOpen = true;
                    Assert.That(editorFormId, Is.GreaterThanOrEqualTo(0));
                    yield return WaitForForm(editorFormId, expectedLoaded: true);

                    var editor = GF.UI.GetUIForm(editorFormId).Logic as AlgorithmEditorForm;
                    Assert.That(editor, Is.Not.Null);
                    Assert.That(editor.AlgorithmDataState, Is.EqualTo(UiDataState.Empty),
                        "运行时在、还没有实例时是 Empty——不是 Unavailable，域并没有缺能力。");

                    AlgorithmPlan plan = CompileFixturePlan(runtime);
                    var instance = new AlgorithmRuntime(new PersistentId(700), plan, runtime.Context.Compute, runtime.Adapter);
                    Assert.That(runtime.Instances.Add(instance), Is.True, "实例服务必须接受这个运行时。");
                    yield return null;
                    Assert.That(editor.AlgorithmDataState, Is.EqualTo(UiDataState.Ready),
                        "加入实例之后算法界面必须变成 Ready——它是**订阅**服务变化，不是轮询。");

                    // ⑤ 三条降级路径：本可移动却拿不到导航的三种原因各自可辨，且部署不下来。
                    MachineInstance blind = session.Machines.Create(wheeled);
                    Assert.That(entry.Region.DeployMachine(blind.Id, new Vector2(28, -25), blind.Definition.Footprint, out _),
                        Is.EqualTo(RegionMachineDeploymentResult.Bound));
                    Assert.That(entry.TryAttachMachineRuntime(blind.Id, out _), Is.True);
                    Assert.That(entry.MachineRuntimes.TryGet(blind.Id, out RegionMachineRuntime noView), Is.True);
                    Assert.That(noView.HasNavigation, Is.False);
                    StringAssert.Contains("没有实体视图", noView.NavigationUnavailableReason);
                    Assert.That(blind.Deployed, Is.True, "拿不到导航不是部署失败。");

                    entry.MachineRuntimes.Detach(blind.Id);
                    Assert.That(entry.Region.TryGet(blind.Id, out RegionObject blindModel), Is.True);
                    bare = new GameObject("BareMachineWithoutMotionRig");
                    bare.transform.position = new Vector3(blindModel.Position.x, 0f, blindModel.Position.y);
                    SceneManager.MoveGameObjectToScene(bare, scene);
                    Assert.That(entry.MachineRuntimes.TryAttach(blind, bare, out RegionMachineRuntime unusable, out _),
                        Is.True);
                    Assert.That(unusable.IsNavigationDegraded, Is.True);
                    StringAssert.Contains("导航绑定失败", unusable.NavigationUnavailableReason);
                    Assert.That(entry.Navigation.BindingCount, Is.EqualTo(1), "绑定失败不得留下半个绑定。");

                    Assert.That(catalog.TryGetMachine(10021, out MachineDefinition fixedBase), Is.True);
                    MachineInstance stationary = session.Machines.Create(fixedBase);
                    Assert.That(entry.Region.DeployMachine(stationary.Id, new Vector2(12, -25), stationary.Definition.Footprint, out _),
                        Is.EqualTo(RegionMachineDeploymentResult.Bound));
                    Assert.That(entry.TryAttachMachineRuntime(stationary.Id, out _), Is.True);
                    Assert.That(entry.MachineRuntimes.TryGet(stationary.Id, out RegionMachineRuntime immobile), Is.True);
                    Assert.That(immobile.HasNavigation, Is.False);
                    Assert.That(immobile.NavigationUnavailableReason, Is.Null,
                        "不可移动不是降级：这里是 null，能不能动看 HasNavigation。");
                    Assert.That(immobile.Context.Sensors, Is.Not.Null, "传感器与能不能移动无关。");
                }
                finally
                {
                    if (editorOpen && GF.UI != null && GF.UI.HasUIForm(editorFormId)) GF.UI.CloseUIForm(editorFormId);
                    if (bare != null) Object.DestroyImmediate(bare);
                    entry.Release();
                    context.ReleaseActiveWorldSession();
                }
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>编译一张最小可用的算法图；预算取自这台机器真实的逻辑算力。</summary>
        private static AlgorithmPlan CompileFixturePlan(RegionMachineRuntime runtime)
        {
            var graph = new AlgorithmDocument { DocumentId = 700 };
            graph.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            graph.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(12) });
            graph.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.SetVariable, StateKey = "counter" });
            graph.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Log });
            graph.Edges.Add(new AlgorithmEdge { From = 1, To = 3, Output = "event", Input = "event" });
            graph.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Input = "value" });
            graph.Edges.Add(new AlgorithmEdge { From = 1, To = 4, Output = "event", Input = "event" });
            Assert.That(AlgorithmValidator.TryCompile(graph, runtime.Context.Compute.LogicCapacity,
                out AlgorithmPlan plan, out _), Is.True, "夹具图必须能在本机的逻辑算力预算内编译。");
            return plan;
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            if (SceneManager.GetActiveScene().name == LaunchSceneName)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(LaunchSceneName, LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null, "Launch 必须在 Build Settings 里启用。");
            yield return loadOperation;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(LaunchSceneName));
            yield return null;
        }

        private static IEnumerator WaitForRuntimeReady()
        {
            const int maxFrames = 600;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI != null && GF.DataTable != null &&
                    GF.DataTable.HasDataTable<UITable>() &&
                    GF.DataTable.HasDataTable<UIGroupTable>() &&
                    GF.UI.HasUIGroup("Default"))
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("GF UI runtime or required UI data tables did not become ready within 600 frames.");
        }

        private static IEnumerator WaitForForm(int serialId, bool expectedLoaded)
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI.HasUIForm(serialId) == expectedLoaded)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"UI form serial {serialId} did not reach loaded={expectedLoaded} within 300 frames.");
        }
    }
}
