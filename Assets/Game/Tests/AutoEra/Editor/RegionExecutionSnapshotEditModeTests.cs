using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.DataTable;
using AutoEra.Events;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    // Composition test with a controllable domain operation. Real production operation checkpoints have separate tests.
    public sealed class RegionExecutionSnapshotEditModeTests
    {
        private sealed class Operation : IRegionEffectorOperation, IRegionPersistentEffectorOperation
        {
            internal long Last;
            internal double Elapsed;
            private bool _paused;
            public bool IsCompleted { get; private set; }
            public BehaviorOutcome Outcome { get; private set; }=BehaviorOutcome.Completed;
            internal Operation(long now) { Last=now; }
            public void Advance(long now) { if(!_paused)Elapsed+=(now-Last)/1000d;Last=now;if(Elapsed>=1)IsCompleted=true; }
            public void SetPaused(bool value,long now) { if(_paused!=value)Last=now;_paused=value; }
            public void Stop(BehaviorOutcome outcome) { IsCompleted=true;Outcome=outcome; }
            public void Dispose() { }
            public bool TryCapturePersistent(long now,out RegionEffectorOperationSnapshot snapshot)
            {
                snapshot=new RegionEffectorOperationSnapshot { Kind=RegionEffectorCheckpointKind.ResourceProduction,
                    Production=new ProductionEffectorOperationSnapshot { LastWorldMilliseconds=Last,Elapsed=Elapsed,Paused=_paused,Outcome=Outcome } };return !IsCompleted;
            }
            internal static Operation Restore(ProductionEffectorOperationSnapshot state) => new Operation(state.LastWorldMilliseconds) { Elapsed=state.Elapsed,_paused=state.Paused,Outcome=state.Outcome };
        }
        private sealed class Executor : IRegionPersistentEffectorExecutor
        {
            internal int Starts,Restores;
            internal Operation Last;
            internal Action DuringStart;
            public bool Supports(ComponentDefinition component,AlgorithmEffectorAction action) => component.Id==2201 && action==AlgorithmEffectorAction.Clean;
            public bool TryStart(MachineExecutionContext context,ComponentInstance component,BehaviorRequest<EffectorBehaviorParameters> request,long now,out IRegionEffectorOperation operation,out string reason)
            { Starts++;Last=new Operation(now);operation=Last;reason=null;DuringStart?.Invoke();return true; }
            public bool TryRestorePersistent(MachineExecutionContext context,ComponentInstance component,BehaviorRequest<EffectorBehaviorParameters> request,RegionEffectorOperationSnapshot snapshot,long now,out IRegionEffectorOperation operation,out string reason)
            {
                operation=null;reason="InvalidFixtureOperation";
                if(snapshot?.Version!=1 || snapshot.Kind!=RegionEffectorCheckpointKind.ResourceProduction || snapshot.Production==null || snapshot.Production.LastWorldMilliseconds>now)return false;
                Restores++;Last=Operation.Restore(snapshot.Production);operation=Last;reason=null;return true;
            }
        }
        private sealed class Data
        {
            public MachineRosterSnapshot Roster;
            public InitialRegionSnapshot Region;
            public RegionExecutionSnapshot Execution;
            public EventServiceSnapshot Events;
        }
        private sealed class Fixture : IDisposable
        {
            internal readonly AutoEraWorldSession World;
            internal readonly InitialRegion Region;
            internal readonly RegionMachineRuntimeRegistry Registry;
            internal readonly RegionMachineRuntime Runtime;
            internal readonly MachineInstance Machine;
            internal readonly ComponentInstance Sensor,Tool;
            internal readonly RegionObject Target;
            internal readonly Executor Executor=new Executor();
            internal bool Restored;
            internal string RestoreReason;
            private static readonly MachineDefinition Definition=new MachineDefinition(1,"Fixture",1,2,1,1,30,false,false,100);
            private static readonly ComponentDefinition CoreDefinition=new ComponentDefinition(2001,HardwareKind.Core,1,0,100,100,false);
            private static readonly ComponentDefinition SensorDefinition=new ComponentDefinition(2101,HardwareKind.Sensor,1,0,0,0,false);
            private static readonly ComponentDefinition ToolDefinition=new ComponentDefinition(2201,HardwareKind.Effector,1,0,0,0,true);
            internal Fixture(Data restored=null)
            {
                World=restored==null ? new AutoEraWorldSessionFactory().Create(0) : new AutoEraWorldSessionFactory().CreateRestoreCandidate(500,restored.Roster.AllocatedThrough);
                Region=new InitialRegion(World,new Rect(-30,-30,60,60));
                if(restored==null)
                {
                    Machine=World.Machines.Create(Definition);
                    var core=World.Machines.CreateComponent(CoreDefinition);Sensor=World.Machines.CreateComponent(SensorDefinition);Tool=World.Machines.CreateComponent(ToolDefinition);
                    World.Machines.Install(Machine.Id,ManagementOrigin.Library,core.Id,0);World.Machines.Install(Machine.Id,ManagementOrigin.Library,Sensor.Id,0);World.Machines.Install(Machine.Id,ManagementOrigin.Library,Tool.Id,0);
                    Region.DeployMachine(Machine.Id,new Vector2(-4,-4),Vector2.one,out _);
                    Target=Region.Register(PersistentObjectKind.Building,"Target",new Vector2(-4,0),Vector2.one);Target.SetPublicState("有效",12);
                    World.IdAllocator.TryRestore(new PersistentId(400));
                    Machine.Activate(ManagementOrigin.Field);Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
                }
                else
                {
                    World.Events.Restore(restored.Events);
                    World.Machines.RestoreConfiguration(restored.Roster,(id,level)=>Definition,(id,level)=>id==2001 ? CoreDefinition : id==2101 ? SensorDefinition : ToolDefinition);
                    Region.RestorePersistentState(restored.Region);
                    World.Machines.TryGet(restored.Roster.Machines[0].Id==0 ? default : new PersistentId(restored.Roster.Machines[0].Id),out Machine);
                    Sensor=Machine.GetComponent(HardwareKind.Sensor,0);Tool=Machine.GetComponent(HardwareKind.Effector,0);
                    foreach(var obj in Region.Objects)if(obj.Kind==PersistentObjectKind.Building)Target=obj;
                }
                Machine.UpdateEnvironment(true,true);
                var row=new SensorDefinitions();row.ParseDataRow("\t21011\t\t2101\t1\tObjectState\t1000\t6\t10",null);
                Registry=new RegionMachineRuntimeRegistry(World,Region,null,new MachineNavigationSettings(),.32f,1.8f,new SensorCatalog(new[] {row}));
                Registry.Effectors.Register(Executor);Assert.That(Registry.TryAttach(Machine,null,out Runtime,out var reason),Is.True,reason);
                if(restored!=null)
                {
                    Restored=Registry.RestorePersistent(restored.Execution,500,0,null,out RestoreReason);
                    if(Restored)World.Machines.RestorePersistentHardwareIntents(restored.Roster.PendingHardware);
                }
            }
            internal void Start()
            {
                Assert.That(Runtime.Hardware.TryBindEndpoint(Sensor.Id,Target.Id,out var sensorGeneration,out _),Is.True);
                var sensorGraph=new AlgorithmDocument { DocumentId=200 };
                sensorGraph.Bindings.Add(new AlgorithmBinding { Key="read",ComponentId=Sensor.Id.Value,TargetId=Target.Id.Value,Generation=sensorGeneration,Type=AlgorithmType.Of(AlgorithmValueKind.Number) });
                sensorGraph.Nodes.Add(new AlgorithmNode { Id=201,Kind=AlgorithmNodeKind.Input,BindingKey="read" });sensorGraph.Nodes.Add(new AlgorithmNode { Id=202,Kind=AlgorithmNodeKind.Log });
                sensorGraph.Edges.Add(new AlgorithmEdge { From=201,To=202,Output="sampled",Input="event" });
                Runtime.Instances.AddDraft(sensorGraph);Assert.That(Runtime.TryActivateDraft(200,out var reason),Is.True,reason);
                Runtime.Hardware.TryBindEndpoint(Tool.Id,Target.Id,out var generation,out _);
                var graph=new AlgorithmDocument { DocumentId=100 };
                graph.Bindings.Add(new AlgorithmBinding { Key="tool",ComponentId=Tool.Id.Value,TargetId=Target.Id.Value,Generation=generation,Type=AlgorithmType.Of(AlgorithmValueKind.Object) });
                graph.Nodes.Add(new AlgorithmNode { Id=101,Kind=AlgorithmNodeKind.Startup });graph.Nodes.Add(new AlgorithmNode { Id=102,Kind=AlgorithmNodeKind.Effector,BindingKey="tool",Action=AlgorithmEffectorAction.Clean });graph.Nodes.Add(new AlgorithmNode { Id=104,Kind=AlgorithmNodeKind.Log });
                graph.Edges.Add(new AlgorithmEdge { From=101,To=102,Output="event",Input="event" });graph.Edges.Add(new AlgorithmEdge { From=102,To=104,Output="completed",Input="event" });
                Runtime.Instances.AddDraft(graph);Assert.That(Runtime.TryActivateDraft(100,out reason),Is.True,reason);
                Registry.AdvanceWorldStep(0,0);Registry.AdvanceWorldStep(100,0);Registry.AdvanceWorldStep(500,0);
            }
            internal Data Capture()
            {
                Assert.That(Registry.TryCapturePersistent(500,0,out var execution),Is.True);
                return Json(new Data { Roster=World.Machines.CapturePersistentConfiguration(),Region=Region.CapturePersistentState(),Execution=execution,Events=World.Events.Capture() });
            }
            public void Dispose() { World.Machines.Dispose();Registry.Dispose();Region.Dispose();World.Dispose(); }
        }
        private static Data Json(Data data)
        {
            string content=WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(500,data.Roster.AllocatedThrough,1,"Fixture",new[] {new WorldSnapshotSection("execution",1,data)}));
            Assert.That(WorldSnapshotCodec.TryRead(content,new Dictionary<string,int>{{"execution",1}},out var file,out var reason),Is.True,reason);
            Assert.That(file.TryReadSection<Data>("execution",out var result,out reason),Is.True,reason);return result;
        }
        [Test] public void CompositeRestore_PreservesHardwareGenerationSensorSequenceAndOperationWithoutStartup()
        {
            using(var source=new Fixture())
            {
                source.Start();var data=source.Capture();int history=source.Runtime.Instances.ReadHistory(100).Length;
                using(var target=new Fixture(data))
                {
                    Assert.That(target.Restored,Is.True,target.RestoreReason);Assert.That(target.Executor.Starts,Is.Zero);Assert.That(target.Executor.Restores,Is.EqualTo(1));
                    Assert.That(target.Runtime.Instances.ReadHistory(100).Length,Is.EqualTo(history));
                    target.Runtime.Hardware.TryBindEndpoint(target.Tool.Id,target.Target.Id,out var generation,out _);
                    Assert.That(generation,Is.EqualTo(data.Execution.Machines[0].Hardware.Effectors[0].Generation));
                    int completed=0;target.Runtime.Adapter.Result+=(node,task,port)=> { if(port=="completed")completed++; };
                    target.Registry.AdvanceWorldStep(1100,0);target.Registry.AdvanceWorldStep(1101,0);target.Registry.AdvanceWorldStep(1102,0);
                    Assert.That(completed,Is.EqualTo(1));Assert.That(target.Executor.Starts,Is.Zero);
                    Assert.That(target.Registry.TryCapturePersistent(1102,0,out var after),Is.True);
                    Assert.That(after.Machines[0].Hardware.Sensors[0].Sequence,Is.EqualTo(data.Execution.Machines[0].Hardware.Sensors[0].Sequence+1));
                    Assert.That(source.Executor.Last.IsCompleted,Is.False);
                }
            }
        }
        [Test] public void PublicReadout_RestoresCachedVersionAndPendingChangeInsteadOfFalseReset()
        {
            using(var source=new Fixture())
            {
                source.Start();source.Target.SetPublicState("新的状态",16);var data=source.Capture();
                using(var target=new Fixture(data))
                {
                    Assert.That(target.Restored,Is.True,target.RestoreReason);
                    source.Registry.AdvanceWorldStep(1000,0);target.Registry.AdvanceWorldStep(1000,0);
                    source.Runtime.Context.Sensors.TryGet(source.Sensor.Id,out var a);target.Runtime.Context.Sensors.TryGet(target.Sensor.Id,out var b);
                    Assert.That(b.LastSample.Version,Is.EqualTo(a.LastSample.Version));Assert.That(b.LastSample.ResourceAmount,Is.EqualTo(16));
                    Assert.That(b.Generation,Is.EqualTo(a.Generation));
                }
            }
        }
        [Test] public void SnapshotDuringExecutorCallback_IsDeferredUntilWorldStepEnds()
        {
            using(var source=new Fixture())
            {
                bool attempted=false;source.Executor.DuringStart=()=> { attempted=true;Assert.That(source.Registry.TryCapturePersistent(100,0,out _),Is.False); };
                source.Start();Assert.That(attempted,Is.True);Assert.That(source.Registry.TryCapturePersistent(500,0,out _),Is.True);
            }
        }
        [Test] public void FailedAlgorithmCandidate_ReleasesUnboundLeasesWithoutTouchingOriginal()
        {
            using(var source=new Fixture())
            {
                source.Start();var data=source.Capture();data.Execution.Machines[0].Algorithms[0].Runtime.Applied.Nodes.Clear();
                var candidate=new Fixture(data);Assert.That(candidate.Restored,Is.False);Assert.That(candidate.RestoreReason,Is.Not.Empty);
                Assert.DoesNotThrow(()=>candidate.Dispose());Assert.That(candidate.Runtime.Context.Compute.Used,Is.Zero);
                Assert.That(source.Runtime.Context.Compute.Used,Is.GreaterThan(0));Assert.That(source.Executor.Last.IsCompleted,Is.False);
            }
        }
        [Test] public void UnclaimedComputeReservation_IsRejectedBeforeRuntimeRestoration()
        {
            using(var source=new Fixture())
            {
                source.Start();var data=source.Capture();var original=data.Execution.Machines[0].Compute.Running;
                var leases=new List<ComputeRequestSnapshot>(original) { new ComputeRequestSnapshot { Id=new PersistentId(999),Source=source.Machine.Id,Cost=1,Class=ComputeClass.Evaluation,State=ComputeState.Running } };
                data.Execution.Machines[0].Compute.Running=leases.ToArray();
                using(var candidate=new Fixture(data)) { Assert.That(candidate.Restored,Is.False);Assert.That(candidate.Runtime.Context.Compute.Used,Is.Zero); }
                Assert.That(source.Executor.Last.IsCompleted,Is.False);
            }
        }

        [Test] public void PendingHardwareRemoval_RestoresOriginalIntentAndCompletesAfterOriginalBehaviorOnce()
        {
            using(var source=new Fixture())
            {
                source.Start();var operation=source.World.Machines.GetHardwareOperation(source.Machine.Id);
                Assert.That(operation.Begin(source.Machine.Id,ManagementOrigin.Field,true,HardwareKind.Effector,0,default),Is.True);
                Assert.That(operation.State,Is.EqualTo(HardwareOperationState.Waiting));var data=source.Capture();
                Assert.That(data.Roster.PendingHardware[0].Component,Is.EqualTo(source.Tool.Id.Value));
                using(var target=new Fixture(data))
                {
                    Assert.That(target.Restored,Is.True,target.RestoreReason);
                    var resumed=target.World.Machines.GetHardwareOperation(target.Machine.Id);
                    Assert.That(resumed.State,Is.EqualTo(HardwareOperationState.Waiting));
                    Assert.That(resumed.RequestVersion,Is.EqualTo(operation.RequestVersion));
                    Assert.That(target.Tool.OwnerId,Is.EqualTo(target.Machine.Id));Assert.That(target.Executor.Starts,Is.Zero);
                    int commits=0;resumed.Changed+=change=> {if(change.State==HardwareOperationState.Completed)commits++;};
                    target.Registry.AdvanceWorldStep(1100,0);target.Registry.AdvanceWorldStep(1101,0);target.Registry.AdvanceWorldStep(1200,0);
                    Assert.That(resumed.State,Is.EqualTo(HardwareOperationState.Completed));Assert.That(commits,Is.EqualTo(1));
                    Assert.That(target.Tool.OwnerId.IsValid,Is.False);Assert.That(target.World.Machines.CapturePersistentConfiguration().PendingHardware,Is.Empty);
                    Assert.That(source.Tool.OwnerId,Is.EqualTo(source.Machine.Id));
                }
            }
        }

        [Test] public void PendingHardwareInstall_DoesNotApplyDuringRestoreOrRestartTheOperation()
        {
            using(var source=new Fixture())
            {
                source.Start();var spare=source.World.Machines.CreateComponent(new ComponentDefinition(2101,HardwareKind.Sensor,1,0,0,0,false));
                var operation=source.World.Machines.GetHardwareOperation(source.Machine.Id);
                operation.Begin(source.Machine.Id,ManagementOrigin.Field,false,HardwareKind.Sensor,1,spare.Id);
                var data=source.Capture();using(var target=new Fixture(data))
                {
                    Assert.That(target.Restored,Is.True,target.RestoreReason);Assert.That(target.Machine.GetComponent(HardwareKind.Sensor,1),Is.Null);
                    Assert.That(target.Executor.Restores,Is.EqualTo(1));Assert.That(target.Executor.Starts,Is.Zero);
                    target.Registry.AdvanceWorldStep(1100,0);target.Registry.AdvanceWorldStep(1101,0);
                    Assert.That(target.Machine.GetComponent(HardwareKind.Sensor,1)?.Id,Is.EqualTo(spare.Id));
                    Assert.That(target.World.Machines.GetHardwareOperation(target.Machine.Id).State,Is.EqualTo(HardwareOperationState.Completed));
                }
            }
        }

        [Test] public void RestoredHardwareCancel_RequiresOriginalVersionAndPersistsTheCancellation()
        {
            using(var source=new Fixture())
            {
                source.Start();var operation=source.World.Machines.GetHardwareOperation(source.Machine.Id);
                operation.Begin(source.Machine.Id,ManagementOrigin.Field,true,HardwareKind.Effector,0,default);
                using(var target=new Fixture(source.Capture()))
                {
                    int dirty=0;target.World.Machines.PersistentConfigurationChanged+=()=>dirty++;
                    var resumed=target.World.Machines.GetHardwareOperation(target.Machine.Id);
                    Assert.That(resumed.Cancel(resumed.RequestVersion+1),Is.False);Assert.That(dirty,Is.Zero);
                    Assert.That(resumed.Cancel(resumed.RequestVersion),Is.True);Assert.That(dirty,Is.EqualTo(1));
                    target.Registry.AdvanceWorldStep(1100,0);target.Registry.AdvanceWorldStep(1101,0);
                    Assert.That(target.Tool.OwnerId,Is.EqualTo(target.Machine.Id));
                    Assert.That(target.Machine.RequestedRunState,Is.EqualTo(MachineRunState.Stopped));
                    Assert.That(target.World.Machines.CapturePersistentConfiguration().PendingHardware,Is.Empty);
                }
            }
        }

        [Test] public void InvalidHardwareBatch_AddsNoPartialIntentAndCannotAffectTheOriginal()
        {
            using(var source=new Fixture())
            {
                source.Start();var data=source.Capture();
                using(var target=new Fixture(data))
                {
                    target.Machine.SetRunState(ManagementOrigin.Field,MachineRunState.Stopped);
                    var valid=new HardwareOperationSnapshot {Machine=target.Machine.Id.Value,Component=target.Tool.Id.Value,RequestVersion=7,
                        Remove=true,Kind=HardwareKind.Effector,Slot=0,Origin=ManagementOrigin.Field};
                    var invalid=new HardwareOperationSnapshot {Machine=999,RequestVersion=8};
                    Assert.That(()=>target.World.Machines.RestorePersistentHardwareIntents(new[] {valid,invalid}),Throws.ArgumentException);
                    Assert.That(target.World.Machines.CapturePersistentConfiguration().PendingHardware,Is.Empty);
                    Assert.DoesNotThrow(()=>target.World.Machines.RestorePersistentHardwareIntents(new[] {valid}));
                    Assert.That(target.World.Machines.GetHardwareOperation(target.Machine.Id).RequestVersion,Is.EqualTo(7));
                    Assert.That(source.Machine.RequestedRunState,Is.EqualTo(MachineRunState.Running));
                }
            }
        }
    }
}
