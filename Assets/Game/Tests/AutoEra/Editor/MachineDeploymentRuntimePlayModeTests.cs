using System.Collections;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 「部署 → 运行时 → **真实导航绑定**」这条链路只能在运行期验证：
    /// `NavMeshBuilder` / `NavMeshAgent` 的 `isOnNavMesh` 在 EditMode 里不是真实形态，
    /// 而 `RegionNavigation.Bind` 的成功前置恰好依赖它。
    ///
    /// 这里刻意不重复 EditMode 已经覆盖的纯注册表语义（幂等、Detach、重建），
    /// 只钉住三件只有运行期才成立的事：
    /// ① 区域就绪就建立注册表（即使一台机器都没部署），可移动机器绑定成功后
    ///    `Navigation.BindingCount` 增加、运行时 `HasNavigation == true`；
    /// ② 三种「本可移动却拿不到导航」的原因各自可辨，且**机器保持已部署**、
    ///    区域导航里不留半个绑定（`Bind` 抛异常也必须被吞成降级）；
    /// ③ 固定式机器同样有运行时，`NavigationUnavailableReason` 是 null 而不是原因字符串。
    /// </summary>
    public sealed class MachineDeploymentRuntimePlayModeTests
    {
        private const string Launch = "Assets/Game/Scene/Launch.unity";
        private const string RegionScene = "Assets/Game/Scene/InitialRegion.unity";
        private static readonly float[] FreeX = { 20f, 28f, 12f, 4f, -12f, -20f, -28f };

        [UnityTest]
        public IEnumerator DeployedMachine_GetsARuntimeWithRealNavigation_AndDegradesHonestly()
        {
            if (!SceneManager.GetSceneByPath(Launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(Launch, OpenSceneMode.Additive));
            Assert.That(SceneManager.GetSceneByPath(Launch).isLoaded, Is.True);

            yield return new EnterPlayMode();
            yield return Verify();
            yield return new ExitPlayMode();
        }

        private static IEnumerator Verify()
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            var ui = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
            while ((!MachineCatalog.IsGameDataLoaded || ui == null) && Time.realtimeSinceStartupAsDouble < until)
                yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True, "机器数据表必须在运行期就绪。");

            AsyncOperation loading = EditorSceneManager.LoadSceneAsyncInPlayMode(RegionScene, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!loading.isDone) yield return null;
            Scene scene = SceneManager.GetSceneByPath(RegionScene);
            InitialRegionScene entry = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);

            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                GameObject bare = null;
                try
                {
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);

                    // 注册表随区域就绪建立：算力、任务与传感器不依赖任何一台机器。
                    Assert.That(entry.MachineRuntimes, Is.Not.Null, "区域就绪就必须有注册表。");
                    Assert.That(entry.MachineRuntimes.Count, Is.Zero);
                    Assert.That(entry.Navigation, Is.Not.Null);
                    Assert.That(entry.Navigation.IsReady, Is.True, entry.Navigation.Error);
                    RegionNavigation navigation = entry.Navigation;

                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition wheeled), Is.True);
                    Assert.That(catalog.TryGetMachine(10021, out MachineDefinition fixedBase), Is.True);
                    Assert.That(wheeled.CanMove, Is.True, "10011 轮式载体必须可移动。");
                    Assert.That(fixedBase.CanMove, Is.False, "10021 固定旋转载体必须不可移动。");

                    // ① 可移动机器：部署 → 生成实体 → 运行时 + 真实导航绑定。
                    MachineInstance carrier = session.Machines.Create(wheeled);
                    DeployThroughFlow(session, entry.Region, carrier, new Vector2(20, -25));
                    Assert.That(entry.TrySpawnMachine(carrier.Id, out string spawn), Is.True, spawn);
                    RegionObjectView view = null;
                    until = Time.realtimeSinceStartupAsDouble + 20;
                    while (view == null && Time.realtimeSinceStartupAsDouble < until)
                    {
                        view = entry.FindMachineView(carrier.Id);
                        if (view == null) yield return null;
                    }

                    Assert.That(view, Is.Not.Null, "机器实体必须生成并完成视图绑定。");
                    Assert.That(entry.MachineRuntimes.Count, Is.EqualTo(1));
                    Assert.That(entry.MachineRuntimes.TryGet(carrier.Id, out RegionMachineRuntime runtime), Is.True);
                    Assert.That(runtime.HasNavigation, Is.True, runtime.NavigationUnavailableReason);
                    Assert.That(runtime.IsNavigationDegraded, Is.False);
                    Assert.That(runtime.Adapter.HasNavigation, Is.True);
                    Assert.That(runtime.Context.Tasks, Is.Not.Null);
                    Assert.That(navigation.BindingCount, Is.EqualTo(1), "可移动机器必须真的绑上区域导航。");

                    // ② 可移动但还没有实体：运行时照常建立，导航缺失是可展示的降级。
                    MachineInstance blind = session.Machines.Create(wheeled);
                    DeployAnywhere(entry.Region, blind);
                    Assert.That(entry.TryAttachMachineRuntime(blind.Id, out string blindReason), Is.True, blindReason);
                    Assert.That(entry.MachineRuntimes.TryGet(blind.Id, out RegionMachineRuntime degraded), Is.True);
                    Assert.That(degraded.HasNavigation, Is.False);
                    Assert.That(degraded.IsNavigationDegraded, Is.True);
                    StringAssert.Contains("没有实体视图", degraded.NavigationUnavailableReason);
                    Assert.That(entry.MachineRuntimes.Count, Is.EqualTo(2));
                    Assert.That(navigation.BindingCount, Is.EqualTo(1), "降级不得在区域导航上留下半个绑定。");

                    // ③ 同一个降级机器：换成「有视图但预制体没有 MotionRig」，
                    //    用来证明 Bind 抛出的异常被吞成降级，而不是打断部署或漏掉订阅。
                    entry.MachineRuntimes.Detach(blind.Id);
                    Assert.That(entry.Region.TryGet(blind.Id, out RegionObject blindModel), Is.True);
                    bare = new GameObject("BareMachineWithoutMotionRig");
                    bare.transform.position = new Vector3(blindModel.Position.x, 0f, blindModel.Position.y);
                    SceneManager.MoveGameObjectToScene(bare, scene);
                    Assert.That(entry.MachineRuntimes.TryAttach(blind, bare, out RegionMachineRuntime unusable, out string bindReason),
                        Is.True, bindReason);
                    Assert.That(unusable.HasNavigation, Is.False);
                    Assert.That(unusable.IsNavigationDegraded, Is.True);
                    StringAssert.Contains("导航绑定失败", unusable.NavigationUnavailableReason);
                    Assert.That(navigation.BindingCount, Is.EqualTo(1), "绑定失败不得在区域导航里留下残项。");
                    Assert.That(entry.Region.TryGet(blind.Id, out _), Is.True, "绑不上导航不是部署失败。");
                    Assert.That(blind.Deployed, Is.True);

                    // ④ 固定式机器：有运行时，没有导航，而且这**不是**降级。
                    MachineInstance stationary = session.Machines.Create(fixedBase);
                    DeployAnywhere(entry.Region, stationary);
                    Assert.That(entry.TryAttachMachineRuntime(stationary.Id, out string stationaryReason), Is.True, stationaryReason);
                    Assert.That(entry.MachineRuntimes.TryGet(stationary.Id, out RegionMachineRuntime fixedRuntime), Is.True);
                    Assert.That(fixedRuntime.HasNavigation, Is.False);
                    Assert.That(fixedRuntime.NavigationUnavailableReason, Is.Null,
                        "固定式机器「没有导航」是正常形态，不是降级——写成原因会让界面把它显示成故障。");
                    Assert.That(fixedRuntime.Context.Sensors, Is.Not.Null);
                    Assert.That(entry.MachineRuntimes.Count, Is.EqualTo(3));

                    // ⑤ 区域释放：运行时整体销毁，导航绑定清空，不留订阅。
                    entry.Release();
                    Assert.That(entry.MachineRuntimes, Is.Null, "区域释放必须整体拆掉运行时注册表。");
                    Assert.That(entry.MachineEntityCount, Is.Zero);
                    Assert.That(navigation.BindingCount, Is.Zero, "区域释放必须清掉全部导航绑定。");
                    Assert.That(navigation.IsReady, Is.False);
                }
                finally
                {
                    entry.Release();
                    if (bare != null) Object.DestroyImmediate(bare);
                }
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        private static void DeployThroughFlow(AutoEraWorldSession session, InitialRegion region, MachineInstance machine, Vector2 position)
        {
            using (var flow = new MachineDeploymentFlow(session, region))
            {
                Assert.That(flow.TryBegin(machine.Id, out string begin), Is.True, begin);
                flow.Preview.Move(position);
                Assert.That(flow.Preview.IsValid, Is.True, "落位点不可用：" + flow.Preview.Reason);
                Assert.That(flow.TryCommit(out _, out string commit), Is.True, commit);
            }
        }

        /// <summary>部署到第一个可用点上；只用于不考察落位流程的从属机器，避免写死坐标。</summary>
        private static Vector2 DeployAnywhere(InitialRegion region, MachineInstance machine)
        {
            foreach (float x in FreeX)
            {
                var candidate = new Vector2(x, -25f);
                if (region.DeployMachine(machine.Id, candidate, machine.Definition.Footprint, out _)
                    == RegionMachineDeploymentResult.Bound) return candidate;
            }

            Assert.Fail("找不到可用落位点：" + machine.Definition.Name);
            return Vector2.zero;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }
    }
}
