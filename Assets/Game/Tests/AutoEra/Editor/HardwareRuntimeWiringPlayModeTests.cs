using System.Collections;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.Editor
{
    public sealed class HardwareRuntimeWiringPlayModeTests
    {
        [UnityTest, Timeout(600000)]
        public IEnumerator FormalHost_CreatesAndDrivesInstalledHardware_WithoutFixtureServices()
        {
            const string launch = "Assets/Game/Scene/Launch.unity";
            if (!SceneManager.GetSceneByPath(launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch, OpenSceneMode.Additive));
            Assert.That(SceneManager.GetSceneByPath(launch).isLoaded, Is.True);
            yield return new EnterPlayMode(); yield return Verify(); yield return new ExitPlayMode();
        }
        private static IEnumerator Verify()
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);
            const string path = "Assets/Game/Scene/InitialRegion.unity";
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!loading.isDone) yield return null;
            var scene = SceneManager.GetSceneByPath(path); InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                try
                {
                    bool ready = false; string error = null;
                    entry.InitializeRuntime(session, () => ready = true, failure => error = failure);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && error == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(error, Is.Null); Assert.That(ready, Is.True);
                    var catalog = MachineCatalog.FromLoadedGameData();
                    catalog.TryGetMachine(10011, out var definition); catalog.TryGetComponent(20011, out var coreDefinition);
                    catalog.TryGetComponent(21011, out var sensorDefinition); catalog.TryGetComponent(22031, out var sawDefinition);
                    var machine = session.Machines.Create(definition);
                    var core = session.Machines.CreateComponent(coreDefinition);
                    var component = session.Machines.CreateComponent(sensorDefinition);
                    var saw = session.Machines.CreateComponent(sawDefinition);
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, component.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, saw.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    entry.Region.DeployMachine(machine.Id, new Vector2(20, -25), definition.Footprint, out _);
                    Assert.That(entry.TrySpawnMachine(machine.Id, out var reason), Is.True, reason);
                    until = Time.realtimeSinceStartupAsDouble + 20; RegionMachineRuntime runtime = null;
                    while (!entry.MachineRuntimes.TryGet(machine.Id, out runtime) && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(runtime, Is.Not.Null); Assert.That(runtime.Hardware.SensorCount, Is.EqualTo(1));
                    Assert.That(runtime.Hardware.EffectorCount, Is.EqualTo(1));
                    machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                    // Only public initial input and a user endpoint-binding command; no context/provider/queue construction.
                    var target = entry.Region.Register(PersistentObjectKind.Building, "B41 public input", new Vector2(24, -25), Vector2.one);
                    target.SetPublicState("可采集", 7);
                    Assert.That(runtime.Hardware.TryBindEndpoint(component.Id, target.Id, out var generation, out reason), Is.True, reason);
                    session.IdAllocator.TryAllocate(out var id); session.IdAllocator.TryAllocate(out var input); session.IdAllocator.TryAllocate(out var log);
                    var graph = new AlgorithmDocument { DocumentId = id.Value };
                    graph.Bindings.Add(new AlgorithmBinding { Key = "read", ComponentId = component.Id.Value, TargetId = target.Id.Value, Generation = generation, Type = AlgorithmType.Of(AlgorithmValueKind.Number) });
                    graph.Nodes.Add(new AlgorithmNode { Id = input.Value, Kind = AlgorithmNodeKind.Input, BindingKey = "read", Field = "resource" });
                    graph.Nodes.Add(new AlgorithmNode { Id = log.Value, Kind = AlgorithmNodeKind.Log });
                    graph.Edges.Add(new AlgorithmEdge { From = input.Value, Output = "sampled", To = log.Value, Input = "event" });
                    runtime.Instances.AddDraft(graph); Assert.That(runtime.TryActivateDraft(id.Value, out reason), Is.True, reason);
                    runtime.Context.Sensors.TryGet(component.Id, out var sensor);
                    entry.BindHud(null); entry.Advance(.02);
                    Assert.That(sensor.TryRead(out var sample), Is.True); Assert.That(sample.ResourceAmount, Is.EqualTo(7));
                    Assert.That(runtime.Instances.ReadHistory(id.Value).Length, Is.EqualTo(1));
                    int samples = 0; sensor.Sampled += _ => samples++;
                    for (int i = 0; i < 20; i++) entry.Advance(.01);
                    Assert.That(samples, Is.Zero, "Sampling is not duplicated every render/world step.");
                    target.SetPublicState("可采集", 6);
                    for (int i = 0; i < 120; i++) entry.Advance(.01);
                    Assert.That(sensor.TryRead(out sample), Is.True); Assert.That(sample.ResourceAmount, Is.EqualTo(6));
                    Assert.That(samples, Is.GreaterThan(0));
                    entry.Region.Remove(target.Id); entry.Advance(.01);
                    Assert.That(sensor.TryRead(out _), Is.False); Assert.That(runtime.Context.Compute.Used, Is.Zero);
                    Assert.That(runtime.Hardware.PresentationUnavailableReason, Is.Null);
                    entry.Release(); Assert.That(runtime.Hardware.EffectorCount, Is.Zero);
                    Assert.That(runtime.Context.Compute.Used, Is.Zero);
                }
                finally { entry.Release(); }
            }
            var unload = SceneManager.UnloadSceneAsync(scene); if (unload != null) while (!unload.isDone) yield return null;
        }
    }
}
