using System.Collections;
using AutoEra.Machines;
using AutoEra.Motion;
using AutoEra.Motion.Adapter;
using AutoEra.UI;
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
    /// <summary>Composition fixture using real services and formal assets, without production settlement.</summary>
    public sealed class MotionWorkBridgeIntegrationTests
    {
        [UnityTest]
        public IEnumerator FormalNavMesh_Bridge_WaterEffector_CloseUi_Power_Cancel_Exit_Twice()
        {
            if (!SceneManager.GetSceneByPath("Assets/Game/Scene/Launch.unity").isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive));
            Assert.That(SceneManager.GetSceneByPath("Assets/Game/Scene/Launch.unity").isLoaded, Is.True);
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
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scene/InitialRegion.unity", new LoadSceneParameters(LoadSceneMode.Additive));
            while (!loading.isDone) yield return null;
            var scene = SceneManager.GetSceneByPath("Assets/Game/Scene/InitialRegion.unity");
            InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                MachineExecutionContext context = null;
                EffectorBehaviorQueue<EffectorWorkKind> effector = null;
                MotionWorkBridge bridge = null;
                RegionWorkQueue work = null;
                MachineTaskRecord task = null;
                GameObject carrierView = null, toolView = null;
                int serial = -1;
                try
                {
                    bool ready = false;
                    string error = null;
                    entry.InitializeRuntime(session, () => ready = true, message => error = message);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && error == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(error, Is.Null);
                    Assert.That(ready && entry.Navigation.IsReady, Is.True);
                    var catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out var wheel), Is.True);
                    Assert.That(catalog.TryGetComponent(20011, out var coreDef), Is.True);
                    Assert.That(catalog.TryGetComponent(22021, out var waterDef), Is.True);
                    var machine = session.Machines.Create(wheel);
                    var core = session.Machines.CreateComponent(coreDef);
                    var water = session.Machines.CreateComponent(waterDef);
                    session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);
                    session.Machines.Install(machine.Id, ManagementOrigin.Library, water.Id, 0);
                    Assert.That(water.OwnerId, Is.EqualTo(machine.Id));
                    Assert.That(entry.Region.DeployMachine(machine.Id, new Vector2(20, -25), new Vector2(1.8f, 2.4f), out _, 90), Is.EqualTo(RegionMachineDeploymentResult.Bound));
                    machine.Activate(ManagementOrigin.Field);
                    machine.UpdateEnvironment(true, true);
                    machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                    context = new MachineExecutionContext(machine, session.IdAllocator);
                    effector = context.BindEffector<EffectorWorkKind>(water);
                    carrierView = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab"), new Vector3(20, 0, -25), Quaternion.Euler(0, 90, 0));
                    toolView = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Entity/Machines/WaterCannon.prefab"));
                    SceneManager.MoveGameObjectToScene(carrierView, scene);
                    SceneManager.MoveGameObjectToScene(toolView, scene);
                    toolView.transform.SetParent(carrierView.transform, false);
                    var rig = carrierView.GetComponent<MotionRig>();
                    var toolRig = toolView.GetComponent<MotionRig>();
                    Assert.That(rig.TryValidate(out error), Is.True, error);
                    Assert.That(toolRig.TryValidate(out error), Is.True, error);
                    var graph = AssetDatabase.LoadAssetAtPath<MotionGraphAsset>("Assets/Game/MotionGraphs/Entity/water_aim_spray.asset");
                    Assert.That(graph.IsCompatibleWith(toolRig), Is.True);
                    var executor = carrierView.GetComponent<MotionExecutor>() ?? carrierView.AddComponent<MotionExecutor>();
                    executor.Configure(rig);
                    var binding = entry.Navigation.Bind(context, carrierView, new MachineNavigationSettings(), .32f, 1.8f);
                    var navigation = binding.Navigation;
                    var contender = entry.Region.Register(PersistentObjectKind.Machine, "B16 reservation contender", new Vector2(15, -32), Vector2.one);
                    var target = entry.Region.Register(PersistentObjectKind.ResourcePoint, "B16 cancellation fixture", new Vector2(28, -25), Vector2.one, 0, false);
                    var joints = new string[rig.JointBindings.Count];
                    for (int i = 0; i < joints.Length; i++) joints[i] = rig.JointBindings[i].StableId;
                    work = new RegionWorkQueue(entry.Region, target.Id, new Rect(26, -27, 4, 4));

                    // Two cycles reuse the same carrier, navigation service, effector, graph and execution ID.
                    for (int cycle = 0; cycle < 2; cycle++)
                    {
                        Assert.That(context.Tasks.Submit("B16 water cancellation", WorkPriority.Normal, out task), Is.EqualTo(QueueAdmission.Accepted));
                        Assert.That(context.Tasks.TryStart(task.Id), Is.True);
                        var motion = new MachineNavigationMotionAdapter(rig, carrierView.transform.position, carrierView.transform.eulerAngles.y, .32f, 1.8f);
                        bridge = new MotionWorkBridge(navigation, executor, motion, "b16-real-joint", joints);
                        Assert.That(work.Request(contender.Id, work.WorkArea.center), Is.EqualTo(WorkRequestResult.Granted));
                        Assert.That(bridge.RequestWork(work, machine.Id, work.WorkArea.center), Is.EqualTo(WorkRequestResult.Waiting));
                        Assert.That(bridge.WorkState, Is.EqualTo(WorkRequestState.Waiting));
                        var goal = new MachineNavigationTarget(entry.Region, target.Id, work, work.WorkArea, 0, new Vector2(3, 3), (m, p) => water.OwnerId == m.Id);
                        Assert.That(navigation.Start(task.Id, goal, Time.realtimeSinceStartupAsDouble), Is.EqualTo(NavigationAdmission.Accepted));
                        Assert.That(bridge.Begin(task.Id), Is.True);
                        entry.Advance(.02f);
                        yield return null;
                        Assert.That(navigation.State, Is.EqualTo(MachineNavigationState.WaitingWork));
                        Assert.That(bridge.TaskId, Is.EqualTo(task.Id));

                        FieldHudForm hud = null;
                        var args = UIParams.Create(false);
                        args.OpenCallback = logic => hud = (FieldHudForm)logic;
                        serial = ui.OpenUIForm(UIViews.FieldHudForm, args);
                        until = Time.realtimeSinceStartupAsDouble + 20;
                        while (hud == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                        Assert.That(hud, Is.Not.Null);
                        entry.BindHud(hud);
                        hud.SetFieldAccess(true, false);
                        Assert.That(hud.OpenMachine(machine.Id, ManagementOrigin.Field), Is.True);
                        var panel = hud.GetComponentInChildren<MachineHardwarePanel>(true);
                        panel.Close();
                        Assert.That(navigation.IsActive, Is.True);
                        Assert.That(work.GetRequestState(machine.Id), Is.EqualTo(WorkRequestState.Waiting));

                        Assert.That(work.Release(contender.Id), Is.True);
                        Assert.That(bridge.RequestWork(work, machine.Id, work.WorkArea.center), Is.EqualTo(WorkRequestResult.Granted));
                        Vector3 departure = carrierView.transform.position;
                        until = Time.realtimeSinceStartupAsDouble + 20;
                        while (navigation.IsActive && Time.realtimeSinceStartupAsDouble < until)
                        {
                            entry.Advance(Time.unscaledDeltaTime);
                            bridge.Sample(carrierView.transform.position, carrierView.transform.eulerAngles.y);
                            yield return null;
                        }
                        Assert.That(navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed));
                        Assert.That(bridge.State, Is.EqualTo(MotionExecutionState.Completed));
                        Assert.That(work.Owner, Is.EqualTo(machine.Id));
                        if (cycle == 0) Assert.That(Vector3.Distance(departure, carrierView.transform.position), Is.GreaterThan(1));
                        Assert.That(executor.TryGetState("b16-real-joint", out var state), Is.True);
                        Assert.That(state, Is.EqualTo(MotionExecutionState.Completed));
                        Assert.That(context.Tasks.TryGet(task.Id, out _), Is.True, "Presentation completion must not close the task.");

                        var reference = new PersistentObjectReference(target.Id, PersistentObjectKind.ResourcePoint);
                        Assert.That(effector.Submit(task.Id, default, default, reference, WorkPriority.Normal, InterruptionRule.SafePoint,
                            EffectorWorkKind.WaterSpray, out var current), Is.EqualTo(QueueAdmission.Accepted));
                        Assert.That(effector.Submit(task.Id, default, default, reference, WorkPriority.Normal, InterruptionRule.SafePoint,
                            EffectorWorkKind.WaterSpray, out var waiting), Is.EqualTo(QueueAdmission.Accepted));
                        Assert.That(effector.Current, Is.SameAs(current));
                        Assert.That(effector.WaitingCount, Is.EqualTo(1));
                        Assert.That(current.Id, Is.Not.EqualTo(waiting.Id));
                        Assert.That(current.TaskId, Is.EqualTo(bridge.TaskId));
                        Assert.That(current.Target.Id, Is.EqualTo(target.Id));
                        Assert.That(hud.OpenMachine(machine.Id, ManagementOrigin.Field), Is.True);
                        panel.Close();
                        ui.CloseUIForm(serial);
                        serial = -1;
                        for (int i = 0; i < 3; i++) yield return null;
                        Assert.That(effector.Current, Is.SameAs(current), "Closing the whole UI form must not cancel work.");
                        Assert.That(effector.WaitingCount, Is.EqualTo(1));
                        Assert.That(work.Owner, Is.EqualTo(machine.Id));
                        Assert.That(MotionPreviewEvaluator.Apply(graph, toolRig, .5f, 1), Is.True);
                        Assert.That(current.Outcome, Is.Null, "Motion presentation cannot settle effector work.");

                        machine.UpdateEnvironment(false, true);
                        Assert.That(effector.CanExecuteCurrent, Is.False);
                        Assert.That(effector.Current, Is.SameAs(current));
                        Assert.That(current.Outcome, Is.Null);
                        machine.UpdateEnvironment(true, true);
                        Assert.That(effector.CanExecuteCurrent, Is.True);
                        navigation.ReleaseWorkReservation();
                        Assert.That(work.Owner.IsValid, Is.False, "Explicit safe release must release the navigation reservation.");
                        context.Tasks.Cancel(task.Id);
                        Assert.That(waiting.Outcome, Is.EqualTo(BehaviorOutcome.Cancelled));
                        Assert.That(effector.WaitingCount, Is.Zero);
                        Assert.That(current.Outcome, Is.Null, "Running SafePoint work must retain its tail.");
                        Assert.That(work.Owner.IsValid, Is.False, "Task cancellation must leave no navigation reservation.");
                        effector.ReachSafePoint();
                        Assert.That(current.Outcome, Is.EqualTo(BehaviorOutcome.Cancelled));
                        Assert.That(effector.Current, Is.Null);
                        Assert.That(task.State, Is.EqualTo(MachineTaskState.Cancelled));
                        Assert.That(bridge.ReleaseWork(work, machine.Id), Is.False);
                        Assert.That(bridge.ReleaseWork(work, machine.Id), Is.False);
                        navigation.ReleaseWorkReservation();
                        bridge.Dispose();
                        bridge.Dispose();
                        bridge = null;
                        Assert.That(work.Owner.IsValid, Is.False);
                        Assert.That(work.WaitingCount, Is.Zero);
                        Assert.That(executor.TryGetState("b16-real-joint", out _), Is.False);
                        MotionPreviewEvaluator.RestoreBindPose(toolRig);
                        foreach (var joint in toolRig.JointBindings)
                        {
                            Assert.That(Vector3.Distance(joint.JointTransform.localPosition, joint.BindLocalPosition), Is.LessThan(.0001f));
                            Assert.That(Quaternion.Angle(joint.JointTransform.localRotation, joint.BindLocalRotation), Is.LessThan(.001f));
                        }
                        Debug.Log($"B16 cycle={cycle} machine={machine.Id} task={task.Id} request={current.Id} queued={waiting.Id}: NavMesh arrival, UI close, power pause, safe cancel and release passed.");
                    }
                    entry.Region.Remove(machine.Id);
                    entry.Advance(.02f);
                    Assert.That(entry.Navigation.BindingCount, Is.Zero);
                    Assert.That(context.Compute.Used + context.Compute.WaitingCount, Is.Zero);
                    Assert.That(effector.Current, Is.Null);
                    Assert.That(effector.WaitingCount, Is.Zero);
                    Assert.That(work.Owner.IsValid, Is.False);
                }
                finally
                {
                    if (serial >= 0) ui.CloseUIForm(serial);
                    if (task != null) context?.Tasks.Cancel(task.Id);
                    effector?.ReachSafePoint();
                    bridge?.Dispose();
                    work?.Dispose();
                    entry.Release();
                    context?.Dispose();
                    if (toolView != null) Object.Destroy(toolView);
                    if (carrierView != null) Object.Destroy(carrierView);
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
