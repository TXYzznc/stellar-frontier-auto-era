using System.Collections;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
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
    public sealed class AlgorithmServicesIntegrationTests
    {
        [UnityTest]
        public IEnumerator FormalRegionSensor_GraphTaskNavigation_HardwareObservationAndTeardown()
        {
            const string launch="Assets/Game/Scene/Launch.unity";
            if (!SceneManager.GetSceneByPath(launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch,OpenSceneMode.Additive));
            Assert.That(SceneManager.GetSceneByPath(launch).isLoaded,Is.True,"Requires saved Launch; never replaces a user scene.");
            yield return new EnterPlayMode();
            yield return Verify();
            yield return new ExitPlayMode();
        }
        private static IEnumerator Verify()
        {
            double until=Time.realtimeSinceStartupAsDouble+30;
            var uiComponent=UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
            while ((!MachineCatalog.IsGameDataLoaded || uiComponent==null || !uiComponent.HasUIGroup("Default")) && Time.realtimeSinceStartupAsDouble<until)
            {yield return null;uiComponent=UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();}
            Assert.That(MachineCatalog.IsGameDataLoaded,Is.True);
            var loading=EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Game/Scene/InitialRegion.unity",new LoadSceneParameters(LoadSceneMode.Additive));
            while(!loading.isDone)yield return null;
            var scene=SceneManager.GetSceneByPath("Assets/Game/Scene/InitialRegion.unity");InitialRegionScene entry=null;
            foreach(var root in scene.GetRootGameObjects())if(root.TryGetComponent(out InitialRegionScene found))entry=found;
            Assert.That(entry,Is.Not.Null);
            using(var session=new AutoEraWorldSessionFactory().Create(0))
            {
                MachineExecutionContext context=null;AlgorithmMachineAdapter adapter=null;AlgorithmRuntime runtime=null;
                RegionSensorEnvironment environment=null;RegionSensorReadProvider provider=null;GameObject view=null;FieldHudForm hud=null;int serial=-1;
                try
                {
                    bool ready=false;string failure=null;entry.InitializeRuntime(session,()=>ready=true,e=>failure=e);
                    until=Time.realtimeSinceStartupAsDouble+25;
                    while(!ready&&failure==null&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                    Assert.That(failure,Is.Null);Assert.That(ready,Is.True);Assert.That(entry.Navigation.IsReady,Is.True);
                    var catalog=MachineCatalog.FromLoadedGameData();catalog.TryGetMachine(10011,out var wheel);catalog.TryGetComponent(20011,out var coreDef);catalog.TryGetComponent(21012,out var sensorDef);
                    var machine=session.Machines.Create(wheel);var core=session.Machines.CreateComponent(coreDef);var component=session.Machines.CreateComponent(sensorDef);
                    session.Machines.Install(machine.Id,ManagementOrigin.Library,core.Id,0);session.Machines.Install(machine.Id,ManagementOrigin.Library,component.Id,0);
                    Assert.That(entry.Region.DeployMachine(machine.Id,new Vector2(20,-25),new Vector2(1.8f,2.4f),out _,90),Is.EqualTo(RegionMachineDeploymentResult.Bound));
                    machine.Activate(ManagementOrigin.Field);machine.UpdateEnvironment(true,true);machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
                    context=new MachineExecutionContext(machine,session.IdAllocator);
                    view=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab"),new Vector3(20,0,-25),Quaternion.Euler(0,90,0));
                    SceneManager.MoveGameObjectToScene(view,scene);
                    var nav=entry.Navigation.Bind(context,view,new MachineNavigationSettings(),.32f,1.8f).Navigation;
                    var target=entry.Region.Register(PersistentObjectKind.Building,"B15 public read fixture",new Vector2(20,-22),Vector2.one,0,false);
                    target.SetPublicState("Ready",1);provider=new RegionSensorReadProvider(target,p=>new Vector3(20,0,-22));
                    environment=new RegionSensorEnvironment(entry.Region);environment.Register(provider);
                    SensorCatalog.FromLoadedGameData().TryGet(21012,out var profile);
                    var sensor=context.Sensors.Bind(component,profile,environment,new TransformSensorAnchor(view.transform));sensor.Bind(provider.Target);
                    entry.AttachSensors(context.Sensors);
                    session.IdAllocator.TryAllocate(out var instanceId);
                    var graph=Graph(instanceId.Value,component.Id.Value,target.Id.Value,sensor.Generation);
                    var nodeIds=new System.Collections.Generic.Dictionary<ulong,ulong>();
                    foreach(var node in graph.Nodes){session.IdAllocator.TryAllocate(out var permanentNode);nodeIds.Add(node.Id,permanentNode.Value);node.Id=permanentNode.Value;}
                    foreach(var edge in graph.Edges){edge.From=nodeIds[edge.From];edge.To=nodeIds[edge.To];}
                    Assert.That(AlgorithmValidator.TryCompile(graph,machine.LogicCapacity,out var plan,out _),Is.True);
                    adapter=new AlgorithmMachineAdapter(context,nav,entry.Region);runtime=new AlgorithmRuntime(instanceId,plan,context.Compute,adapter);adapter.Attach(runtime);adapter.BindResourceAmount(sensor,nodeIds[1],"amount");
                    var ui=UIParams.Create(false);ui.OpenCallback=logic=>hud=(FieldHudForm)logic;serial=uiComponent.OpenUIForm(UIViews.FieldHudForm,ui);
                    until=Time.realtimeSinceStartupAsDouble+20;while(hud==null&&Time.realtimeSinceStartupAsDouble<until)yield return null;
                    Assert.That(hud,Is.Not.Null);entry.BindHud(hud);hud.SetFieldAccess(true,false);
                    long now=entry.WorldMilliseconds;sensor.Tick(now);Assert.That(sensor.TryRead(out _),Is.True);
                    runtime.Pump(now);adapter.Pump(now,Time.realtimeSinceStartupAsDouble);Assert.That(nav.IsActive,Is.True);
                    // Closing the observer UI cannot cancel the accepted navigation.
                    uiComponent.CloseUIForm(serial);serial=-1;Assert.That(nav.IsActive,Is.True);
                    // Same target, new binding generation: old graph subscription cannot fire again.
                    int acceptedEvents=runtime.WaitingCount;sensor.Bind(provider.Target);sensor.Tick(now);Assert.That(runtime.WaitingCount,Is.EqualTo(acceptedEvents));
                    until=Time.realtimeSinceStartupAsDouble+20;
                    while(nav.IsActive&&Time.realtimeSinceStartupAsDouble<until)
                    { entry.Advance(Time.unscaledDeltaTime);adapter.Pump(entry.WorldMilliseconds,Time.realtimeSinceStartupAsDouble);runtime.Pump(entry.WorldMilliseconds);yield return null; }
                    Assert.That(nav.Outcome,Is.EqualTo(BehaviorOutcome.Completed),"Actual NavMesh outcome: "+nav.State);
                    runtime.Pump(entry.WorldMilliseconds);
                    Assert.That(Vector2.Distance(new Vector2(view.transform.position.x,view.transform.position.z),new Vector2(28,-25)),Is.LessThan(.2f));
                    var history=runtime.History();Assert.That(history.Length,Is.EqualTo(4));Assert.That(history[3].CopyTrigger().Port,Is.EqualTo("completed"));
                    Assert.That(history[3].CopyTrigger().TaskId,Is.Not.Zero);
                    int finished=0;foreach(var task in context.Tasks.History){Assert.That(task.State,Is.EqualTo(MachineTaskState.Completed));finished++;}Assert.That(finished,Is.EqualTo(1));
                    Assert.That(session.ObjectRegistry.TryResolve(machine.Id,PersistentObjectKind.Machine,out var authority),Is.EqualTo(PersistentRegistryResult.Success));Assert.That(authority,Is.SameAs(machine));
                    System.IO.Directory.CreateDirectory("Temp/AutoEraTestResults");
                    ScreenCapture.CaptureScreenshot("Temp/AutoEraTestResults/b15-joint-lifecycle.png");yield return new WaitForEndOfFrame();yield return null;
                    runtime.Dispose();adapter.Dispose();entry.Release();
                    Assert.That(context.Compute.Used,Is.Zero);Assert.That(context.Compute.WaitingCount,Is.Zero);Assert.That(sensor.TryRead(out _),Is.False);
                }
                finally
                {
                    if(serial>=0)uiComponent.CloseUIForm(serial);runtime?.Dispose();adapter?.Dispose();entry.Release();context?.Dispose();provider?.Dispose();environment?.Dispose();if(view!=null)Object.Destroy(view);
                }
            }
            yield return SceneManager.UnloadSceneAsync(scene);
        }
        private static AlgorithmDocument Graph(ulong id,ulong component,ulong target,ulong generation)
        {
            var g=new AlgorithmDocument { DocumentId=id };var number=AlgorithmType.Of(AlgorithmValueKind.Number,"resource");
            g.Bindings.Add(new AlgorithmBinding { Key="amount",ComponentId=component,TargetId=target,Generation=generation,Type=number.Copy() });
            g.Nodes.Add(new AlgorithmNode { Id=1,Kind=AlgorithmNodeKind.Input,BindingKey="amount",ValueType=number.Copy() });
            g.Nodes.Add(new AlgorithmNode { Id=2,Kind=AlgorithmNodeKind.Constant,ValueType=number.Copy(),Default=AlgorithmValue.Numeric(0,"resource") });
            g.Nodes.Add(new AlgorithmNode { Id=3,Kind=AlgorithmNodeKind.Compare,Operator=AlgorithmOperator.Greater,ValueType=number.Copy() });
            g.Nodes.Add(new AlgorithmNode { Id=4,Kind=AlgorithmNodeKind.Branch });g.Nodes.Add(new AlgorithmNode { Id=5,Kind=AlgorithmNodeKind.Navigate });
            g.Nodes.Add(new AlgorithmNode { Id=6,Kind=AlgorithmNodeKind.Constant,ValueType=AlgorithmType.Of(AlgorithmValueKind.Position),Default=new AlgorithmValue { Type=AlgorithmType.Of(AlgorithmValueKind.Position),X=28,Z=-25 } });
            g.Nodes.Add(new AlgorithmNode { Id=7,Kind=AlgorithmNodeKind.Log });
            g.Edges.Add(new AlgorithmEdge { From=1,To=3,Input="a" });g.Edges.Add(new AlgorithmEdge { From=2,To=3,Input="b" });
            g.Edges.Add(new AlgorithmEdge { From=1,To=4,Output="sampled",Input="event" });g.Edges.Add(new AlgorithmEdge { From=3,To=4,Input="condition" });
            g.Edges.Add(new AlgorithmEdge { From=4,To=5,Output="true",Input="event" });g.Edges.Add(new AlgorithmEdge { From=6,To=5,Input="target" });
            g.Edges.Add(new AlgorithmEdge { From=5,To=7,Output="completed",Input="event" });return g;
        }
        [UnityTearDown] public IEnumerator Cleanup(){if(EditorApplication.isPlaying)yield return new ExitPlayMode();}
    }
}
