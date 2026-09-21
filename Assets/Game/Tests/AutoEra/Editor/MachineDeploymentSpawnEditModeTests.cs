using System.Collections;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 落位 → 实体 → 视图绑定 这条生产链路必须在**真实框架运行期**验证：
    /// `GF.Entity.ShowEntity` 要先加载预制体再回调，EditMode 里没有这条路径。
    ///
    /// 断言的核心是「视图指向已部署对象而不是又注册一个」——机器实体预制体不带
    /// `RegionObjectView`，本链路给它补上并走 `BindDeployed`，所以区域对象数必须**不变**。
    /// </summary>
    public sealed class MachineDeploymentSpawnEditModeTests
    {
        private const string Launch = "Assets/Game/Scene/Launch.unity";
        private const string RegionScene = "Assets/Game/Scene/InitialRegion.unity";

        [UnityTest]
        public IEnumerator DeployedMachine_SpawnsOnceAndBindsItsViewToTheExistingObject()
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
                try
                {
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);

                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition definition), Is.True);
                    Assert.That(definition.HasFootprint, Is.True, "轮式载体必须在数据表里配了交互占地。");
                    Assert.That(definition.HasPrefab, Is.True, "轮式载体必须配了实体预制体。");
                    MachineInstance machine = session.Machines.Create(definition);

                    // 1) 未部署就先生成实体：必须被明确拒绝，而不是先加载预制体再说。
                    Assert.That(entry.TrySpawnMachine(machine.Id, out string notDeployed), Is.False);
                    StringAssert.Contains("尚未部署", notDeployed);
                    Assert.That(entry.MachineEntityCount, Is.Zero);

                    // 2) 走落位流程部署（占地来自定义，不是写死的常量）。
                    using (var flow = new MachineDeploymentFlow(session, entry.Region))
                    {
                        Assert.That(flow.TryBegin(machine.Id, out string begin), Is.True, begin);
                        flow.Preview.Move(new Vector2(20, -25));
                        Assert.That(flow.TryCommit(out _, out string commit), Is.True, commit);
                    }

                    int objectsAfterDeploy = entry.Region.Count;

                    // 3) 生成实体并把视图绑定到已部署对象。
                    Assert.That(entry.TrySpawnMachine(machine.Id, out string spawn), Is.True, spawn);
                    until = Time.realtimeSinceStartupAsDouble + 20;
                    RegionObjectView view = null;
                    while (view == null && Time.realtimeSinceStartupAsDouble < until)
                    {
                        view = entry.FindMachineView(machine.Id);
                        yield return null;
                    }

                    Assert.That(view, Is.Not.Null, "机器实体必须生成并完成视图绑定。");
                    Assert.That(entry.MachineEntityCount, Is.EqualTo(1));
                    Assert.That(entry.Region.Count, Is.EqualTo(objectsAfterDeploy),
                        "绑定视图绝不能新建区域对象——那正是 Re-注册这个陷阱。");
                    Assert.That(view.Model, Is.Not.Null);
                    Assert.That(view.Model.Id, Is.EqualTo(machine.Id), "视图必须指向这台机器自己的区域对象。");
                    Assert.That(view.Model.Kind, Is.EqualTo(PersistentObjectKind.Machine));

                    // 4) 同一台机器不得生成第二个实体。
                    Assert.That(entry.TrySpawnMachine(machine.Id, out string again), Is.False);
                    StringAssert.Contains("已经有实体", again);
                    Assert.That(entry.MachineEntityCount, Is.EqualTo(1));

                    // 5) 未配置预制体的定义：给出可展示原因，且不影响已成立的部署。
                    MachineInstance bare = session.Machines.Create(new MachineDefinition(7, "无预制体验证机", 1, 1, 1, 0, 5, false, true, 50, 2, 2));
                    Assert.That(entry.Region.DeployMachine(bare.Id, new Vector2(28, -25), new Vector2(2, 2), out _),
                        Is.EqualTo(RegionMachineDeploymentResult.Bound));
                    Assert.That(entry.TrySpawnMachine(bare.Id, out string noPrefab), Is.False);
                    StringAssert.Contains("未配置实体预制体", noPrefab);
                    Assert.That(entry.Region.TryGet(bare.Id, out _), Is.True,
                        "没有预制体只意味着没有表现，部署本身仍然成立。");

                    // 6) 释放区域：机器实体必须一并清掉。
                    entry.Release();
                    Assert.That(entry.MachineEntityCount, Is.Zero, "区域释放必须回收全部机器实体。");
                }
                finally
                {
                    entry.Release();
                }
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }
    }
}
