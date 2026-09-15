using System.Collections;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AutoEra.Tests.Editor
{
    public sealed class RegionNavigationIntegrationTests
    {
        [UnityTest]
        public IEnumerator FormalRegion_BuildsBindsMovesRebuildsAndReenters()
        {
            if (!SceneManager.GetSceneByPath("Assets/Game/Scene/Launch.unity").isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive);
                SceneManager.SetActiveScene(launch);
            }
            Assert.That(SceneManager.GetSceneByPath("Assets/Game/Scene/Launch.unity").isLoaded, Is.True, "Use the saved Launch scene; do not overwrite another scene.");
            yield return new EnterPlayMode();
            yield return VerifyRuntime();
            yield return new ExitPlayMode();
        }
        private static IEnumerator VerifyRuntime()
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scene/InitialRegion.unity", new LoadSceneParameters(LoadSceneMode.Additive));
            until = Time.realtimeSinceStartupAsDouble + 15;
            while (!loading.isDone && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(loading.isDone, Is.True, "Formal scene load timeout.");
            var scene = SceneManager.GetSceneByPath("Assets/Game/Scene/InitialRegion.unity");
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True, "Formal scene not loaded.");
            InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null, "Formal roots=" + scene.rootCount);
            var catalog = MachineCatalog.FromLoadedGameData();
            Assert.That(catalog.TryGetMachine(10011, out var wheel), Is.True);
            Assert.That(catalog.TryGetComponent(20011, out var coreDefinition), Is.True);
            for (int pass = 0; pass < 2; pass++)
            {
                using (var session = new AutoEraWorldSessionFactory().Create(0))
                {
                    bool ready = false; string failure = null; MachineExecutionContext context = null; GameObject view = null;
                    try
                    {
                        entry.InitializeRuntime(session, () => ready = true, error => failure = error);
                        until = Time.realtimeSinceStartupAsDouble + 20;
                        while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                        Assert.That(failure, Is.Null); Assert.That(ready, Is.True);
                        Assert.That(entry.Navigation, Is.Not.Null); Assert.That(entry.Navigation.IsReady, Is.True, entry.Navigation.Error);
                        Assert.That(entry.Navigation.BuildCount, Is.EqualTo(1));
                        var navigation = entry.Navigation;
                        var obstacle = entry.Region.Register(PersistentObjectKind.Building, "B13 temporary topology probe", new Vector2(30, 30), new Vector2(4, 4));
                        entry.Advance(.01); Assert.That(navigation.BuildCount, Is.EqualTo(2));
                        Assert.That(NavMesh.SamplePosition(new Vector3(30, 0, 30), out _, 2f, NavMesh.AllAreas), Is.False, "Obstacle centre must remain blocked, including voxel height tolerance.");
                        entry.Region.Remove(obstacle.Id); entry.Advance(.01);
                        Assert.That(navigation.BuildCount, Is.EqualTo(3));
                        Assert.That(NavMesh.SamplePosition(new Vector3(30, 0, 30), out var restoredGround, 2f, NavMesh.AllAreas), Is.True, "Removed obstacle must restore navigable ground.");
                        Assert.That(Vector2.Distance(new Vector2(restoredGround.position.x, restoredGround.position.z), new Vector2(30, 30)), Is.LessThan(.1f), "Do not accept a neighbouring surface instead of restored ground.");
                        Assert.That(NavMesh.SamplePosition(new Vector3(30, 0, 30), out _, .25f, NavMesh.AllAreas), Is.True, "Surface precision must fit the driver's target sampling tolerance.");

                        var machine = session.Machines.Create(wheel);
                        var core = session.Machines.CreateComponent(coreDefinition);
                        session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);
                        Assert.That(entry.Region.DeployMachine(machine.Id, new Vector2(20, -25), new Vector2(1.8f, 2.4f), out _, 90), Is.EqualTo(RegionMachineDeploymentResult.Bound));
                        machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                        entry.Advance(.01);
                        context = new MachineExecutionContext(machine, session.IdAllocator);
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab");
                        view = Object.Instantiate(prefab, new Vector3(20, 0, -25), Quaternion.Euler(0, 90, 0));
                        SceneManager.MoveGameObjectToScene(view, scene);
                        var binding = navigation.Bind(context, view, new MachineNavigationSettings(), .32f, 1.8f);
                        context.Tasks.Submit("B13 formal entry regression", WorkPriority.Normal, out var task); context.Tasks.StartNext();
                        binding.Navigation.Start(task.Id, new MachineNavigationTarget(entry.Region, new Vector3(28, 0, -25), 90), Time.realtimeSinceStartupAsDouble);
                        until = Time.realtimeSinceStartupAsDouble + 15;
                        while (binding.Navigation.IsActive && Time.realtimeSinceStartupAsDouble < until) { entry.Advance(Time.unscaledDeltaTime); yield return null; }
                        Assert.That(binding.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed), "State=" + binding.Navigation.State + "; plans=" + binding.Navigation.PlanCount + "; position=" + view.transform.position);
                        Assert.That(Vector2.Distance(new Vector2(view.transform.position.x, view.transform.position.z), new Vector2(28, -25)), Is.LessThan(.2f));
                        Assert.That(session.ObjectRegistry.TryResolve(machine.Id, PersistentObjectKind.Machine, out var authority), Is.EqualTo(PersistentRegistryResult.Success));
                        Assert.That(authority, Is.SameAs(machine));
                        context.Tasks.CloseChain(task.Id);
                        int count = navigation.BuildCount; entry.Advance(.01); Assert.That(navigation.BuildCount, Is.EqualTo(count), "No per-frame topology rebuild.");
                        entry.Release(); Assert.That(navigation.IsReady, Is.False); Assert.That(navigation.BindingCount, Is.Zero);
                        Assert.That(NavMesh.SamplePosition(new Vector3(30, 0, 30), out _, 2f, NavMesh.AllAreas), Is.False);
                    }
                    finally { entry.Release(); context?.Dispose(); if (view != null) Object.DestroyImmediate(view); }
                }
                yield return null;
            }
            yield return SceneManager.UnloadSceneAsync(scene);
        }
        [UnityTearDown] public IEnumerator Cleanup() { if (EditorApplication.isPlaying) yield return new ExitPlayMode(); }
    }
}
