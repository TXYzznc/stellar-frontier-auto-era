using System.Collections;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.UI;
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
    public sealed class ProductionAlgorithmDrivePlayModeTests
    {
        private const string Launch = "Assets/Game/Scene/Launch.unity";
        private const string RegionScene = "Assets/Game/Scene/InitialRegion.unity";

        [UnityTest, Timeout(600000)]
        public IEnumerator FormalHost_DrivesRealNavigation_AfterObserverCloses()
        {
            if (!SceneManager.GetSceneByPath(Launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(Launch, OpenSceneMode.Additive));
            Assert.That(SceneManager.GetSceneByPath(Launch).isLoaded, Is.True, "Requires saved Launch; preserves the user's scene setup.");
            yield return new EnterPlayMode();
            yield return Verify();
            yield return new ExitPlayMode();
        }

        private static IEnumerator Verify()
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            var ui = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
            while ((!MachineCatalog.IsGameDataLoaded || ui == null || !ui.HasUIGroup("Default")) && Time.realtimeSinceStartupAsDouble < until)
            {
                yield return null;
                ui = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
            }
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);
            Assert.That(ui, Is.Not.Null);
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode(RegionScene, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!loading.isDone) yield return null;
            var scene = SceneManager.GetSceneByPath(RegionScene);
            InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects())
                if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);

            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                int serial = -1;
                try
                {
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, error => failure = error);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);
                    var catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out var wheel), Is.True);
                    Assert.That(catalog.TryGetComponent(20011, out var coreDefinition), Is.True);
                    var machine = session.Machines.Create(wheel);
                    var core = session.Machines.CreateComponent(coreDefinition);
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(entry.Region.DeployMachine(machine.Id, new Vector2(20, -25), wheel.Footprint, out _), Is.EqualTo(RegionMachineDeploymentResult.Bound));
                    Assert.That(entry.TrySpawnMachine(machine.Id, out string reason), Is.True, reason);
                    until = Time.realtimeSinceStartupAsDouble + 20;
                    RegionMachineRuntime runtime = null;
                    while (!entry.MachineRuntimes.TryGet(machine.Id, out runtime) && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(runtime, Is.Not.Null);
                    Assert.That(runtime.HasNavigation, Is.True, runtime.NavigationUnavailableReason);
                    var view = entry.FindMachineView(machine.Id);
                    Assert.That(view, Is.Not.Null);
                    machine.Activate(ManagementOrigin.Field);
                    machine.UpdateEnvironment(true, true);
                    Assert.That(machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running), Is.EqualTo(MachineManagementResult.Completed));

                    session.IdAllocator.TryAllocate(out var instanceId);
                    var graph = new AlgorithmDocument { DocumentId = instanceId.Value };
                    var nodes = new PersistentId[4];
                    for (int i = 0; i < nodes.Length; i++) session.IdAllocator.TryAllocate(out nodes[i]);
                    graph.Nodes.Add(new AlgorithmNode { Id = nodes[0].Value, Kind = AlgorithmNodeKind.Startup });
                    graph.Nodes.Add(new AlgorithmNode { Id = nodes[1].Value, Kind = AlgorithmNodeKind.Constant,
                        ValueType = AlgorithmType.Of(AlgorithmValueKind.Position),
                        Default = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Position), X = 28, Y = 0, Z = -25, IsValid = true } });
                    graph.Nodes.Add(new AlgorithmNode { Id = nodes[2].Value, Kind = AlgorithmNodeKind.Navigate });
                    graph.Nodes.Add(new AlgorithmNode { Id = nodes[3].Value, Kind = AlgorithmNodeKind.Log });
                    graph.Edges.Add(new AlgorithmEdge { From = nodes[0].Value, Output = "event", To = nodes[2].Value, Input = "event" });
                    graph.Edges.Add(new AlgorithmEdge { From = nodes[1].Value, Output = "value", To = nodes[2].Value, Input = "target" });
                    graph.Edges.Add(new AlgorithmEdge { From = nodes[2].Value, Output = "completed", To = nodes[3].Value, Input = "event" });
                    Assert.That(runtime.Instances.AddDraft(graph), Is.True);
                    Assert.That(runtime.TryActivateDraft(instanceId.Value, out reason), Is.True, reason);

                    FieldHudForm hud = null;
                    var parameters = UIParams.Create(false);
                    parameters.OpenCallback = logic => hud = (FieldHudForm)logic;
                    serial = ui.OpenUIForm(UIViews.FieldHudForm, parameters);
                    until = Time.realtimeSinceStartupAsDouble + 20;
                    while (hud == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(hud, Is.Not.Null);
                    entry.BindHud(hud);
                    entry.Advance(.02);
                    Assert.That(runtime.Navigation.IsActive, Is.True, "Only the production host starts navigation.");
                    ui.CloseUIForm(serial); serial = -1;
                    entry.BindHud(null);

                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (runtime.Navigation.IsActive && Time.realtimeSinceStartupAsDouble < until)
                    { entry.Advance(Time.unscaledDeltaTime); yield return null; }
                    Assert.That(runtime.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed));
                    for (int i = 0; i < 5; i++) { entry.Advance(.02); yield return null; }
                    Assert.That(Vector2.Distance(new Vector2(view.transform.position.x, view.transform.position.z), new Vector2(28, -25)), Is.LessThan(.2f));
                    var history = runtime.Instances.ReadHistory(instanceId.Value);
                    int startups = 0, completions = 0;
                    foreach (var record in history)
                    {
                        var trigger = record.CopyTrigger();
                        if (trigger.NodeId == nodes[0].Value) startups++;
                        if (trigger.NodeId == nodes[2].Value && trigger.Port == "completed") completions++;
                    }
                    Assert.That(startups, Is.EqualTo(1));
                    Assert.That(completions, Is.EqualTo(1));
                    int finished = 0;
                    foreach (var task in runtime.Context.Tasks.History)
                    { Assert.That(task.State, Is.EqualTo(MachineTaskState.Completed)); finished++; }
                    Assert.That(finished, Is.EqualTo(1), "One real navigation task, without manual service Pump.");
                }
                finally
                {
                    if (serial >= 0) ui.CloseUIForm(serial);
                    entry.Release();
                }
            }
            var unload = SceneManager.UnloadSceneAsync(scene);
            if (unload != null) while (!unload.isDone) yield return null;
        }
    }
}
