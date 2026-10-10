using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmAdapterSnapshotEditModeTests
    {
        private sealed class Driver : IMachineNavigationDriver
        {
            public Vector3 Position { get; set; }
            public float Yaw { get; set; }
            public float Speed=>0;
            public bool PathValid=>true;
            public bool TryPlan(MachineNavigationTarget target,MachineInstance machine,out Vector3 destination)
            { destination=target.Candidates[0];return target.Allows(machine,destination); }
            public void Resume() { }
            public void Stop() { }
            public void Align(float yaw,float speed,float seconds) { Yaw=yaw; }
        }
        private sealed class Data
        {
            public MachineComputeSnapshot Compute;
            public MachineTaskQueueSnapshot Tasks;
            public AlgorithmPersistentSnapshot Runtime;
            public AlgorithmMachineAdapterSnapshot Adapter;
            public MachineNavigationSnapshot Navigation;
            public EffectorBehaviorSnapshot<EffectorBehaviorParameters> Effector;
        }
        private sealed class Fixture : IDisposable
        {
            internal readonly AutoEraWorldSession World=new AutoEraWorldSessionFactory().Create(0);
            internal readonly InitialRegion Region;
            internal readonly MachineInstance Machine;
            internal readonly MachineExecutionContext Context;
            internal readonly MachineNavigation Navigation;
            internal readonly Driver Driver=new Driver();
            internal readonly AlgorithmMachineAdapter Adapter;
            internal readonly AlgorithmRuntime Runtime;
            internal readonly EffectorBehaviorQueue<EffectorBehaviorParameters> Effector;
            internal Fixture(bool effector)
            {
                Region=new InitialRegion(World,new Rect(-30,-30,60,60));
                Machine=World.Machines.Create(new MachineDefinition(1,"Fixture",1,1,1,1,30,true,true,100));
                var core=World.Machines.CreateComponent(new ComponentDefinition(2,HardwareKind.Core,1,0,100,100,false));
                var tool=World.Machines.CreateComponent(new ComponentDefinition(3,HardwareKind.Effector,1,0,0,0,true));
                World.Machines.Install(Machine.Id,ManagementOrigin.Library,core.Id,0);
                World.Machines.Install(Machine.Id,ManagementOrigin.Library,tool.Id,0);
                Region.DeployMachine(Machine.Id,Vector2.zero,Vector2.one,out _);
                var target=Region.Register(PersistentObjectKind.ResourcePoint,"Point",new Vector2(12,0),Vector2.one);
                Machine.Activate(ManagementOrigin.Field);Machine.UpdateEnvironment(true,true);Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
                World.IdAllocator.TryRestore(new PersistentId(200));
                Context=new MachineExecutionContext(Machine,World.IdAllocator);
                Navigation=new MachineNavigation(Context,Driver,new MachineNavigationSettings());
                Effector=Context.BindEffector<EffectorBehaviorParameters>(tool);
                Adapter=new AlgorithmMachineAdapter(Context,Navigation,Region);Adapter.RegisterEffector(tool,Effector,7);
                var graph=new AlgorithmDocument { DocumentId=100 };
                graph.Nodes.Add(new AlgorithmNode { Id=101,Kind=AlgorithmNodeKind.Startup });
                graph.Nodes.Add(new AlgorithmNode { Id=102,Kind=effector ? AlgorithmNodeKind.Effector : AlgorithmNodeKind.Navigate,
                    Action=AlgorithmEffectorAction.StopSpray,BindingKey=effector ? "tool" : null });
                graph.Nodes.Add(new AlgorithmNode { Id=104,Kind=AlgorithmNodeKind.Log });
                graph.Edges.Add(new AlgorithmEdge { From=101,To=102,Output="event",Input="event" });
                graph.Edges.Add(new AlgorithmEdge { From=102,To=104,Output="completed",Input="event" });
                if(effector) graph.Bindings.Add(new AlgorithmBinding { Key="tool",ComponentId=tool.Id.Value,TargetId=target.Id.Value,Generation=7,Type=AlgorithmType.Of(AlgorithmValueKind.Object) });
                else
                {
                    graph.Nodes.Add(new AlgorithmNode { Id=103,Kind=AlgorithmNodeKind.Constant,
                        ValueType=AlgorithmType.Of(AlgorithmValueKind.Position),
                        Default=new AlgorithmValue { Type=AlgorithmType.Of(AlgorithmValueKind.Position),X=8,Y=0,Z=0 } });
                    graph.Edges.Add(new AlgorithmEdge { From=103,To=102,Input="target" });
                }
                Assert.That(AlgorithmValidator.TryCompile(graph,100,out var plan,out var issues),Is.True,string.Join(";",issues.ConvertAll(issue=>issue.Code)));
                Runtime=new AlgorithmRuntime(new PersistentId(100),plan,Context.Compute,Adapter.CreateInstanceSink(100));Adapter.Attach(Runtime);
            }
            internal void Start()
            {
                Assert.That(Runtime.Enqueue(new AlgorithmTrigger { NodeId=101,Revision=1,Generation=1 }),Is.True);Runtime.Pump(0);
            }
            internal Data Capture()
            {
                Assert.That(Context.Compute.TryCapturePersistent(out var compute),Is.True);
                Assert.That(Runtime.TryCapturePersistent(1000,out var runtime),Is.True);
                Assert.That(Adapter.TryCapturePersistent(1000,out var adapter),Is.True);
                Assert.That(Navigation.TryCapturePersistent(0,out var navigation),Is.True);
                Assert.That(Effector.TryCapturePersistent(EffectorParameterSnapshot.Copy,out var effector),Is.True);
                return Json(new Data { Compute=compute,Tasks=Context.Tasks.Capture(),Runtime=runtime,Adapter=adapter,Navigation=navigation,Effector=effector });
            }
            internal void RestoreDomains(Data data)
            {
                Assert.That(Context.Compute.RestorePersistent(data.Compute,1000),Is.True);
                Context.Tasks.Restore(data.Tasks);
                Assert.That(Runtime.RestorePersistent(data.Runtime,1000),Is.True);
                Driver.Position=data.Navigation.Position;Driver.Yaw=data.Navigation.Yaw;
                Assert.That(Navigation.RestorePersistent(data.Navigation,0,t=>MachineNavigationTarget.RestorePersistent(t,Region)),Is.True);
                Assert.That(Effector.RestorePersistent(data.Effector,EffectorParameterSnapshot.Valid,EffectorParameterSnapshot.Copy),Is.True);
            }
            public void Dispose()
            {
                Adapter.Dispose();Runtime.Dispose();Navigation.Dispose();
                Effector.InvalidateUnstarted(); if(Effector.Current!=null) Effector.Finish(BehaviorOutcome.Cancelled);
                Context.Dispose();Region.Dispose();World.Dispose();
            }
        }
        private static Data Json(Data data)
        {
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1000,1000,1,"Fixture",new[] {new WorldSnapshotSection("adapter",1,data)}));
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"adapter",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<Data>("adapter",out var copy,out reason),Is.True,reason);return copy;
        }
        [Test] public void ActiveNavigation_ReturnsCompletionToOriginalGenerationExactlyOnce()
        {
            using(var source=new Fixture(false))
            using(var target=new Fixture(false))
            {
                source.Start();source.Adapter.Pump(0,0);source.Navigation.Tick(0,0);
                var data=source.Capture();target.RestoreDomains(data);
                int completed=0,logs=0;
                target.Adapter.Result+=(node,task,port)=> { if(port=="completed") completed++;if(port=="Recorded") logs++; };
                Assert.That(target.Adapter.RestorePersistent(data.Adapter,1000),Is.True);
                Assert.That(completed,Is.Zero);Assert.That(target.Navigation.CurrentTaskId.Value,Is.EqualTo(data.Adapter.ActiveNavigation.Task));
                target.Navigation.Tick(0,0);target.Driver.Position=new Vector3(8,0,0);target.Navigation.Tick(1,1);
                target.Adapter.BeginWorldStep(1000);target.Runtime.Pump(1000);
                target.Adapter.BeginWorldStep(1001);target.Runtime.Pump(1001);
                target.Adapter.BeginWorldStep(1002);target.Runtime.Pump(1002);
                Assert.That(completed,Is.EqualTo(1),"Completion callback");Assert.That(logs,Is.EqualTo(1),"Completion continuation after accepted/started FIFO");
                Assert.That(target.Context.Tasks.TryGet(new PersistentId(data.Adapter.ActiveNavigation.Task),out _),Is.False);
                Assert.That(source.Navigation.IsActive,Is.True);
            }
        }
        [Test] public void WaitingNavigation_ResumesSameTaskWithoutAllocatingDuplicate()
        {
            using(var source=new Fixture(false))
            using(var target=new Fixture(false))
            {
                source.Start();var data=source.Capture();target.RestoreDomains(data);
                Assert.That(data.Adapter.ActiveNavigation,Is.Null);Assert.That(data.Adapter.WaitingNavigation.Length,Is.EqualTo(1));
                Assert.That(target.Adapter.RestorePersistent(data.Adapter,1000),Is.True);
                var before=target.World.IdAllocator.NextId.Value;
                target.Adapter.Pump(1000,0);
                Assert.That(target.Navigation.CurrentTaskId.Value,Is.EqualTo(data.Adapter.WaitingNavigation[0].Task));
                Assert.That(target.World.IdAllocator.NextId.Value,Is.EqualTo(before),"Starting original navigation does not allocate a new task or behavior.");
                Assert.That(target.Adapter.TryCapturePersistent(1000,out var state),Is.True);Assert.That(state.OwnedTasks.Length,Is.EqualTo(1));
            }
        }
        [Test] public void EffectorCompletion_RebindsOriginalBehaviorAndDeliversPendingResultsOnNextStep()
        {
            using(var source=new Fixture(true))
            using(var target=new Fixture(true))
            {
                source.Start();var data=source.Capture();target.RestoreDomains(data);
                int completed=0,logs=0;target.Adapter.Result+=(node,task,port)=> { if(port=="completed") completed++;if(port=="Recorded") logs++; };
                Assert.That(target.Adapter.RestorePersistent(data.Adapter,1000),Is.True);
                Assert.That(target.Effector.Current.Id,Is.EqualTo(source.Effector.Current.Id));
                target.Effector.Finish(BehaviorOutcome.Completed);Assert.That(completed,Is.Zero);
                target.Adapter.BeginWorldStep(1000);target.Runtime.Pump(1000);
                target.Adapter.BeginWorldStep(1001);target.Runtime.Pump(1001);
                target.Adapter.BeginWorldStep(1002);target.Runtime.Pump(1002);
                Assert.That(completed,Is.EqualTo(1),"Completion callback");Assert.That(logs,Is.EqualTo(1),"Completion continuation after accepted/started FIFO");
                Assert.That(target.Runtime.Generation,Is.EqualTo(data.Runtime.Generation+1));
                Assert.That(source.Effector.Current,Is.Not.Null);
            }
        }
        [TestCase("generation")] [TestCase("component")] [TestCase("missing_behavior")] [TestCase("result")]
        public void InvalidResponsibility_IsRejectedBeforeAdapterChanges_ThenOriginalCanRestore(string damage)
        {
            using(var source=new Fixture(true))
            using(var target=new Fixture(true))
            {
                source.Start();var data=source.Capture();target.RestoreDomains(data);var damaged=Json(data).Adapter;
                if(damage=="generation") damaged.Effectors[0].Trigger.Generation++;
                if(damage=="component") damaged.Effectors[0].ComponentGeneration++;
                if(damage=="missing_behavior") damaged.Effectors=Array.Empty<AlgorithmAdapterEffectorSnapshot>();
                if(damage=="result") damaged.Results[0].Trigger.Port="invented";
                Assert.That(target.Adapter.RestorePersistent(damaged,1000),Is.False);
                Assert.That(target.Adapter.TryCapturePersistent(1000,out var empty),Is.True);
                Assert.That(empty.OwnedTasks,Is.Empty);Assert.That(empty.Effectors,Is.Empty);Assert.That(empty.Results,Is.Empty);
                Assert.That(target.Adapter.RestorePersistent(data.Adapter,1000),Is.True);
            }
        }
    }
}
