using System;
using System.Collections;
using AutoEra.Algorithms;
using AutoEra.Application;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.ResourcePoints;
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
    public sealed class ForestMineralProductionPlayModeTests
    {
        [UnityTest, Timeout(300000)]
        public IEnumerator FormalHost_ActualInstalledToolsProduceCargo_PowerCancelGeometryAndViewRelease()
        {
            const string launch = "Assets/Game/Scene/Launch.unity";
            if (!SceneManager.GetSceneByPath(launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch, OpenSceneMode.Additive));
            yield return new EnterPlayMode(); yield return VerifyDrill(); yield return VerifySaw(); yield return new ExitPlayMode();
        }
        private static IEnumerator VerifyDrill()
        {
            yield return Wait(null, () => MachineCatalog.IsGameDataLoaded);
            Assert.That(Screen.width, Is.EqualTo(1920)); Assert.That(Screen.height, Is.EqualTo(1080));
            const string path = "Assets/Game/Scene/InitialRegion.unity";
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!loading.isDone) yield return null;
            var scene = SceneManager.GetSceneByPath(path); InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            using (var context = new AutoEraApplicationCompositionRoot().Create())
            {
                context.TryCreateWorldSession(0, out var session);
                try
                {
                    bool ready = false; string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    yield return Wait(null, () => ready || failure != null); Assert.That(failure, Is.Null);
                    RegionObject depositObject = null; MineralProduction deposit = null;
                    foreach (var obj in entry.Region.Objects)
                        if (session.Production.TryGetMineral(obj.Id, out var candidate)) { depositObject = obj; deposit = candidate; }
                    Assert.That(deposit, Is.Not.Null); Assert.That(deposit.TotalUnits, Is.EqualTo(100));
                    Assert.That(depositObject.BlocksNavigation, Is.False);
                    var catalog = MachineCatalog.FromLoadedGameData(); catalog.TryGetMachine(10011, out var definition);
                    catalog.TryGetComponent(20011, out var coreDefinition); catalog.TryGetComponent(21011, out var sensorDefinition); catalog.TryGetComponent(22041, out var drillDefinition);
                    var machine = session.Machines.Create(definition); var core = session.Machines.CreateComponent(coreDefinition);
                    var sensorComponent = session.Machines.CreateComponent(sensorDefinition); var drill = session.Machines.CreateComponent(drillDefinition);
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, sensorComponent.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, drill.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    using (var flow = new MachineDeploymentFlow(session, entry.Region))
                    {
                        Assert.That(flow.TryBegin(machine.Id, out var reason), Is.True, reason); flow.Preview.Move(new Vector2(20,-25));
                        Assert.That(flow.TryCommit(out _, out reason), Is.True, reason);
                    }
                    Assert.That(entry.TrySpawnMachine(machine.Id, out var spawn), Is.True, spawn);
                    yield return Wait(null, () => entry.FindMachineView(machine.Id) != null);
                    Assert.That(entry.MachineRuntimes.TryGet(machine.Id, out var runtime), Is.True);
                    machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true,true); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                    yield return Wait(entry, () => entry.ProductionTools.ReadyCount == 1, component: drill.Id, phase: "工具加载");
                    Assert.That(runtime.Hardware.TryBindEndpoint(drill.Id, deposit.Id, out var generation, out var bindingReason), Is.True, bindingReason);
                    Assert.That(runtime.Hardware.TryBindEndpoint(sensorComponent.Id, deposit.Id, out _, out bindingReason), Is.True, bindingReason);
                    var graph = DrillGraph(session, drill.Id, deposit.Id, generation, depositObject.Position);
                    Assert.That(runtime.Instances.AddDraft(graph), Is.True); Assert.That(runtime.TryActivateDraft(graph.DocumentId, out var activationReason), Is.True, activationReason);
                    // Inputs/assembly/graph are test preparation. Entry.Advance owns energy, navigation, algorithms, queues and real production.
                    yield return Wait(entry, () => deposit.FractionalContribution > .05, runtime, drill.Id);
                    double contribution = deposit.FractionalContribution; int units = deposit.ProducedUnits;
                    Assert.That(machine.SetPowerSwitch(ManagementOrigin.Field, false), Is.EqualTo(MachineManagementResult.Completed));
                    for (int i = 0; i < 100; i++) { entry.Advance(.05); yield return null; }
                    Assert.That(deposit.FractionalContribution, Is.EqualTo(contribution).Within(1e-9)); Assert.That(deposit.ProducedUnits, Is.EqualTo(units));
                    Assert.That(machine.SetPowerSwitch(ManagementOrigin.Field, true), Is.EqualTo(MachineManagementResult.Completed));
                    yield return Wait(entry, () => deposit.ProducedUnits == 2, runtime, drill.Id);
                    Assert.That(deposit.CachedUnits, Is.EqualTo(2)); Assert.That(deposit.RemainingUnits, Is.EqualTo(98));
                    Assert.That(session.Resources.Authority.TryFindAvailableLot(deposit.GroundOwner, ResourceItemCatalog.Ore, out var lot), Is.True);
                    Assert.That(lot.Units, Is.EqualTo(2)); Assert.That(lot.Owner, Is.EqualTo(deposit.GroundOwner));
                    for (int i = 0; i < 30; i++) { entry.Advance(.05); yield return null; }
                    runtime.Context.Sensors.TryGet(sensorComponent.Id, out var sensor);
                    Assert.That(sensor.TryRead(out var sample), Is.True); Assert.That(sample.ResourceAmount, Is.EqualTo(98)); Assert.That(sample.CachedAmount, Is.EqualTo(2));
                    Assert.That(session.Resources.Authority.TryCapture(out var snapshot), Is.True);
                    Assert.That(snapshot.ProductionReceipts.Count, Is.EqualTo(2));
                    var journal = new System.Collections.Generic.List<AutoEra.Events.EventJournalRecord>(); session.Events.Journal.CopyRecent(journal);
                    Assert.That(journal.Exists(f => f.Action.StartsWith("ProductionCommitted task=") && f.Action.Contains(" units=1")), Is.True, "Actual task/behavior facts must be journaled.");
                    yield return Capture("drill-production-1920.png", new Vector3(depositObject.Position.x, 0, depositObject.Position.y));
                    entry.Release(); Assert.That(deposit.CachedUnits, Is.EqualTo(2)); Assert.That(deposit.RemainingUnits, Is.EqualTo(98));
                }
                finally { entry.Release(); }
            }
            var unloading = SceneManager.UnloadSceneAsync(scene); while (unloading != null && !unloading.isDone) yield return null;
        }
        private static IEnumerator VerifySaw()
        {
            const string path = "Assets/Game/Scene/InitialRegion.unity";
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive)); while (!loading.isDone) yield return null;
            var scene = SceneManager.GetSceneByPath(path); InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            using (var context = new AutoEraApplicationCompositionRoot().Create())
            {
                context.TryCreateWorldSession(0, out var session);
                try
                {
                    bool ready = false; string error = null; entry.InitializeRuntime(session, () => ready = true, e => error = e);
                    yield return Wait(null, () => ready || error != null); Assert.That(error, Is.Null);
                    ForestProduction forest = null;
                    foreach (var obj in entry.Region.Objects) if (session.Production.TryGetForest(obj.Id, out var candidate)) forest = candidate;
                    Assert.That(forest, Is.Not.Null); var tree = forest.ReadAt(0);
                    var catalog = MachineCatalog.FromLoadedGameData(); catalog.TryGetMachine(10011, out var definition); catalog.TryGetComponent(20011, out var coreDefinition); catalog.TryGetComponent(22031, out var sawDefinition);
                    var machine = session.Machines.Create(definition); var core = session.Machines.CreateComponent(coreDefinition); var saw = session.Machines.CreateComponent(sawDefinition);
                    session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0); session.Machines.Install(machine.Id, ManagementOrigin.Library, saw.Id, 0);
                    using (var flow = new MachineDeploymentFlow(session, entry.Region))
                    { Assert.That(flow.TryBegin(machine.Id, out var reason), Is.True, reason); flow.Preview.Move(new Vector2(20,-25)); Assert.That(flow.TryCommit(out _, out reason), Is.True, reason); }
                    Assert.That(entry.TrySpawnMachine(machine.Id, out var spawn), Is.True, spawn); yield return Wait(null, () => entry.FindMachineView(machine.Id) != null);
                    Assert.That(entry.MachineRuntimes.TryGet(machine.Id, out var runtime), Is.True); machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true,true); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                    yield return Wait(entry, () => entry.ProductionTools.ReadyCount == 1, component: saw.Id, phase: "锯盘加载");
                    Assert.That(runtime.Hardware.TryBindEndpoint(saw.Id, forest.Id, out var generation, out var bindingReason), Is.True, bindingReason);
                    // Two real navigation legs align the body toward the trunk and keep its centre
                    // inside the authored work area. The installed rig then verifies reach itself.
                    var graph = DrillGraph(session, saw.Id, forest.Id, generation, new Vector2(tree.Position.x + 2.6f, tree.Position.z));
                    graph.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Constant, ValueType = AlgorithmType.Of(AlgorithmValueKind.Position), Default = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Position), X = tree.Position.x + 6, Y = 0, Z = tree.Position.z } });
                    graph.Nodes.Add(new AlgorithmNode { Id = 7, Kind = AlgorithmNodeKind.Navigate });
                    graph.Edges[0].From = 7; graph.Edges[0].Output = "completed";
                    graph.Edges.Add(new AlgorithmEdge { From = 1, Output = "event", To = 7, Input = "event" });
                    graph.Edges.Add(new AlgorithmEdge { From = 6, To = 7, Input = "target" });
                    graph.Nodes[3].ValueType = new AlgorithmType { Kind = AlgorithmValueKind.Object, ObjectCategory = "Tree" };
                    graph.Nodes[3].Default = new AlgorithmValue { Type = graph.Nodes[3].ValueType.Copy(), ObjectId = tree.Id.Value };
                    graph.Nodes[4].Action = AlgorithmEffectorAction.Cut; graph.Edges[3].Input = "tree";
                    runtime.Instances.AddDraft(graph); Assert.That(runtime.TryActivateDraft(graph.DocumentId, out var activationReason), Is.True, activationReason);
                    yield return Wait(entry, () => forest.ReadAt(0).HP < forest.ReadAt(0).Height - .05, runtime, saw.Id);
                    machine.SetPowerSwitch(ManagementOrigin.Field, false); double hp = forest.ReadAt(0).HP;
                    for (int i = 0; i < 20; i++) { entry.Advance(.05); yield return null; }
                    Assert.That(forest.ReadAt(0).HP, Is.GreaterThanOrEqualTo(hp), "Standing trees may heal while the saw is powered off.");
                    Assert.That(forest.CachedUnits, Is.Zero); machine.SetPowerSwitch(ManagementOrigin.Field, true);
                    Assert.That(runtime.Hardware.TryGetEffector(saw.Id, out var queue), Is.True); var cancelled = queue.Current;
                    Assert.That(cancelled, Is.Not.Null); queue.Cancel(cancelled.Id); entry.Advance(.05);
                    Assert.That(cancelled.Outcome, Is.EqualTo(BehaviorOutcome.Cancelled));
                    double cancelledHp = forest.ReadAt(0).HP;
                    for (int i = 0; i < 20; i++) { entry.Advance(.05); yield return null; }
                    Assert.That(forest.ReadAt(0).HP, Is.GreaterThanOrEqualTo(cancelledHp)); Assert.That(forest.CachedUnits, Is.Zero);
                    var resumed = graph.Copy(); session.IdAllocator.TryAllocate(out var resumedId); resumed.DocumentId = resumedId.Value;
                    Assert.That(runtime.Instances.AddDraft(resumed), Is.True); Assert.That(runtime.TryActivateDraft(resumed.DocumentId, out activationReason), Is.True, activationReason);
                    yield return Wait(entry, () => forest.ReadAt(0).Stage == TreeStage.Falling, runtime, saw.Id);
                    var fallen = forest.ReadAt(0); Assert.That(fallen.Id, Is.EqualTo(tree.Id));
                    ProductionTreePresentation presentation = null;
                    foreach (var root in scene.GetRootGameObjects()) foreach (var candidate in root.GetComponentsInChildren<ProductionTreePresentation>(true))
                        if (candidate.TreeId == tree.Id) presentation = candidate;
                    // GF entity roots live in the framework scene; inspect the actual loaded forest view.
                    if (presentation == null)
                        foreach (var candidate in UnityEngine.Object.FindObjectsOfType<ProductionTreePresentation>()) if (candidate.TreeId == tree.Id) presentation = candidate;
                    Assert.That(presentation, Is.Not.Null); Assert.That(presentation.FallingUpper, Is.Not.Null);
                    Assert.That(presentation.FallingUpper.scene.GetPhysicsScene(), Is.Not.EqualTo(Physics.defaultPhysicsScene));
                    var hinge = presentation.FallingUpper.GetComponent<ConfigurableJoint>(); Assert.That(hinge, Is.Not.Null);
                    Assert.That(hinge.xMotion, Is.EqualTo(ConfigurableJointMotion.Locked)); Assert.That(hinge.connectedAnchor.y, Is.EqualTo((float)fallen.FractureHeight).Within(.01));
                    foreach (var filter in presentation.GetComponentsInChildren<MeshFilter>(true))
                        foreach (var vertex in filter.sharedMesh.vertices)
                            Assert.That(filter.transform.TransformPoint(vertex).y, Is.LessThanOrEqualTo(tree.Position.y + fallen.FractureHeight + .001), "Lower mesh must end at the actual fracture.");
                    yield return Capture("tree-felling-1920.png", tree.Position);
                    var view = entry.FindMachineView(machine.Id); Assert.That(view, Is.Not.Null);
                    yield return Wait(entry, () => forest.CachedUnits >= 2, runtime, saw.Id);
                    Assert.That(forest.ReadAt(0).Stage, Is.EqualTo(TreeStage.Stump)); Assert.That(forest.ReadAt(0).FellingSequence, Is.EqualTo(1));
                    Assert.That(session.Resources.Authority.TryFindAvailableLot(forest.GroundOwner, ResourceItemCatalog.Wood, out var lot), Is.True); Assert.That(lot.Units, Is.EqualTo(2));
                    entry.Release(); Assert.That(forest.CachedUnits, Is.EqualTo(2));
                }
                finally { entry.Release(); }
            }
            var unloading = SceneManager.UnloadSceneAsync(scene); while (unloading != null && !unloading.isDone) yield return null;
        }
        private static IEnumerator Capture(string file, Vector3 target)
        {
            string directory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath), "openspec/changes/b44-p4002-p4003-p4009-p4017-forest-mineral-production/evidence");
            System.IO.Directory.CreateDirectory(directory);
            var view = new GameObject("ProductionEvidenceCamera"); var camera = view.AddComponent<Camera>();
            var texture = new RenderTexture(1920,1080,24); camera.targetTexture = texture; camera.orthographic = true; camera.orthographicSize = 5;
            camera.nearClipPlane = .1f; camera.farClipPlane = 100; camera.cullingMask = ~(1 << 5); // world-only evidence, not a UI acceptance claim.
            view.transform.position = target + new Vector3(6,8,-7); view.transform.LookAt(target + Vector3.up);
            yield return null; yield return null;
            var prior = RenderTexture.active; var image = new Texture2D(1920,1080,TextureFormat.RGB24,false);
            try { RenderTexture.active = texture; image.ReadPixels(new Rect(0,0,1920,1080),0,0); image.Apply(); System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory,file),image.EncodeToPNG()); }
            finally { RenderTexture.active = prior; camera.targetTexture = null; UnityEngine.Object.Destroy(view); UnityEngine.Object.Destroy(image); UnityEngine.Object.Destroy(texture); }
        }
        private static AlgorithmDocument DrillGraph(AutoEraWorldSession session, PersistentId tool, PersistentId point, ulong generation, Vector2 work)
        {
            session.IdAllocator.TryAllocate(out var id);
            var graph = new AlgorithmDocument { DocumentId = id.Value };
            graph.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            graph.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Constant, ValueType = AlgorithmType.Of(AlgorithmValueKind.Position), Default = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Position), X = work.x, Y = 0, Z = work.y } });
            graph.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Navigate });
            graph.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(2) });
            graph.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Effector, Action = AlgorithmEffectorAction.Drill, BindingKey = "drill" });
            graph.Bindings.Add(new AlgorithmBinding { Key = "drill", ComponentId = tool.Value, TargetId = point.Value, Generation = generation, Type = AlgorithmType.Of(AlgorithmValueKind.Number) });
            graph.Edges.Add(new AlgorithmEdge { From = 1, Output = "event", To = 3, Input = "event" });
            graph.Edges.Add(new AlgorithmEdge { From = 2, To = 3, Input = "target" });
            graph.Edges.Add(new AlgorithmEdge { From = 3, Output = "completed", To = 5, Input = "event" });
            graph.Edges.Add(new AlgorithmEdge { From = 4, To = 5, Input = "count" });
            return graph;
        }
        private static IEnumerator Wait(InitialRegionScene entry, Func<bool> predicate, RegionMachineRuntime runtime = null, PersistentId component = default, string phase = null)
        {
            double until = Time.realtimeSinceStartupAsDouble + 90;
            while (!predicate() && Time.realtimeSinceStartupAsDouble < until) { if (entry != null) entry.Advance(.05); yield return null; }
            string reason = null; runtime?.Hardware.TryGetUnavailableReason(component, out reason);
            if (string.IsNullOrEmpty(reason) && runtime != null) reason = runtime.NavigationUnavailableReason;
            if (phase != null) reason = phase + ": " + entry?.ProductionTools?.GetReadinessReason(component);
            if (!predicate() && entry != null && component.IsValid) reason += "; " + entry.ProductionTools.GetContactGeometry(component);
            Assert.That(predicate(), Is.True, "Formal production timed out: " + reason);
        }
    }
}
