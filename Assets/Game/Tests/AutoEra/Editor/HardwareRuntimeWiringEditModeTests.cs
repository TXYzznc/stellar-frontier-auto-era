using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.DataTable;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class HardwareRuntimeWiringEditModeTests
    {
        [Test]
        public void RepeatedReconciliation_UsesOneInstalledEndpoint_AndLiveProviderDirectory()
        {
            using (var f = new Fixture())
            {
                f.Runtime.Context.Sensors.TryGet(f.Sensor.Id, out var first);
                f.Runtime.Hardware.TryGetEffector(f.Tools[0].Id, out var queue);
                for (int i = 0; i < 50; i++) { f.Runtime.Hardware.Reconcile(); f.Machine.UpdateEnvironment(true, i % 2 == 0); }
                Assert.That(f.Runtime.Context.Sensors.Count, Is.EqualTo(2));
                Assert.That(f.Runtime.Hardware.SensorCount, Is.EqualTo(2));
                Assert.That(f.Runtime.Hardware.EffectorCount, Is.EqualTo(2));
                f.Runtime.Context.Sensors.TryGet(f.Sensor.Id, out var second);
                f.Runtime.Hardware.TryGetEffector(f.Tools[0].Id, out var sameQueue);
                Assert.That(second, Is.SameAs(first)); Assert.That(sameQueue, Is.SameAs(queue));
                var target = new PersistentObjectReference(f.Target.Id, f.Target.Kind);
                Assert.That(f.Registry.SensorEnvironment.TryGetProvider(target, out _), Is.True);
                f.Region.Remove(f.Target.Id);
                Assert.That(f.Registry.SensorEnvironment.TryGetProvider(target, out _), Is.False);
            }
        }
        [Test]
        public void ProductionSensorStep_ReadsRealPublicState_AndUnsupportedSoilIsUnavailable()
        {
            using (var f = new Fixture())
            {
                Assert.That(f.Runtime.Hardware.TryBindEndpoint(f.Sensor.Id, f.Target.Id, out _, out _), Is.True);
                Assert.That(f.Runtime.Hardware.TryBindEndpoint(f.Soil.Id, f.Target.Id, out _, out _), Is.True);
                f.Registry.AdvanceWorldStep(0, 0);
                f.Runtime.Context.Sensors.TryGet(f.Sensor.Id, out var sensor);
                f.Runtime.Context.Sensors.TryGet(f.Soil.Id, out var soil);
                Assert.That(sensor.TryRead(out var snapshot), Is.True); Assert.That(snapshot.ResourceAmount, Is.EqualTo(12));
                Assert.That(soil.TryRead(out _), Is.False); Assert.That(soil.Reason, Is.EqualTo(SensorReadReason.UnsupportedTarget));
                Assert.That(f.Runtime.Context.Compute.Used, Is.EqualTo(10));
                f.Target.SetPublicState("可采集", 8); f.Registry.AdvanceWorldStep(1000, 1);
                Assert.That(sensor.TryRead(out snapshot), Is.True); Assert.That(snapshot.ResourceAmount, Is.EqualTo(8));
                f.Region.Remove(f.Target.Id); f.Registry.AdvanceWorldStep(1001, 1);
                Assert.That(sensor.Reason, Is.EqualTo(SensorReadReason.TargetMissing)); Assert.That(f.Runtime.Context.Compute.Used, Is.Zero);
            }
        }
        [Test]
        public void MissingField_IsInvalidAndDoesNotExecuteDownstreamAction()
        {
            using (var f = new Fixture())
            {
                f.Target.SetPublicState("待机");
                var graph = f.SensorGraph(100);
                graph.Edges.Clear();
                graph.Nodes.Add(new AlgorithmNode { Id = 103, Kind = AlgorithmNodeKind.Constant, Default = AlgorithmValue.Numeric(0) });
                graph.Nodes.Add(new AlgorithmNode { Id = 104, Kind = AlgorithmNodeKind.Compare, Operator = AlgorithmOperator.GreaterOrEqual });
                graph.Nodes.Add(new AlgorithmNode { Id = 105, Kind = AlgorithmNodeKind.Branch });
                graph.Edges.Add(new AlgorithmEdge { From = 101, To = 104, Input = "a" });
                graph.Edges.Add(new AlgorithmEdge { From = 103, To = 104, Input = "b" });
                graph.Edges.Add(new AlgorithmEdge { From = 101, Output = "sampled", To = 105, Input = "event" });
                graph.Edges.Add(new AlgorithmEdge { From = 104, To = 105, Input = "condition" });
                graph.Edges.Add(new AlgorithmEdge { From = 105, Output = "true", To = 102, Input = "event" });
                Assert.That(f.Runtime.Instances.AddDraft(graph), Is.True);
                Assert.That(f.Runtime.TryActivateDraft(100, out var reason), Is.True, reason);
                int commands = 0; f.Runtime.Adapter.Result += (n, t, p) => commands++;
                f.Registry.AdvanceWorldStep(0, 0);
                Assert.That(commands, Is.Zero, "The invalid field must not become a valid numeric zero.");
                Assert.That(f.Runtime.Instances.ReadHistory(100)[0].CopyTrigger().Inputs["read"].IsValid, Is.False);
            }
        }
        [Test]
        public void TargetRebind_InvalidatesOldAppliedGeneration_UntilDraftIsRebound()
        {
            using (var f = new Fixture())
            {
                var graph = f.SensorGraph(100); f.Runtime.Instances.AddDraft(graph);
                Assert.That(f.Runtime.TryActivateDraft(100, out _), Is.True);
                f.Registry.AdvanceWorldStep(0, 0);
                int previous = f.Runtime.Instances.ReadHistory(100).Length;
                var other = f.Region.Register(PersistentObjectKind.Building, "new target", new Vector2(-4, 2), Vector2.one);
                other.SetPublicState("可采集", 99);
                Assert.That(f.Runtime.Hardware.TryBindEndpoint(f.Sensor.Id, other.Id, out ulong generation, out _), Is.True);
                Assert.That(generation, Is.GreaterThan(graph.Bindings[0].Generation));
                Assert.That(f.Runtime.Adapter.ValidateBindings(graph), Is.False);
                f.Registry.AdvanceWorldStep(1000, 1);
                Assert.That(f.Runtime.Instances.ReadHistory(100).Length, Is.EqualTo(previous));
                var draft = f.Runtime.Instances.ReadDraft(100);
                f.Runtime.Instances.Rebind(100, draft.Revision, "read", f.Sensor.Id.Value, other.Id.Value, generation);
                draft = f.Runtime.Instances.ReadDraft(100);
                Assert.That(f.Runtime.TryApplyDraft(100, draft.Revision, 1, out _), Is.True);
                f.Registry.AdvanceWorldStep(1001, 1); f.Registry.AdvanceWorldStep(2000, 2);
                f.Registry.AdvanceWorldStep(3000, 3);
                Assert.That(f.Runtime.Instances.ReadHistory(100).Length, Is.GreaterThan(previous));
            }
        }
        [Test]
        public void AlreadyQueuedOldSample_IsDiscardedWhenEndpointRebinds()
        {
            using (var f = new Fixture())
            {
                var graph = f.SensorGraph(100); f.Runtime.Instances.AddDraft(graph);
                Assert.That(f.Runtime.TryActivateDraft(100, out _), Is.True);
                f.Runtime.Context.Sensors.TryGet(f.Sensor.Id, out var sensor);
                // Unit-level queued callback boundary, separate from the formal-host proof.
                sensor.Tick(0);
                var other = f.Region.Register(PersistentObjectKind.Building, "replacement", new Vector2(-4, 2), Vector2.one);
                other.SetPublicState("可采集", 99);
                f.Runtime.Hardware.TryBindEndpoint(f.Sensor.Id, other.Id, out _, out _);
                f.Registry.AdvanceWorldStep(1, 0);
                Assert.That(f.Runtime.Instances.ReadHistory(100), Is.Empty);
            }
        }
        [Test]
        public void UnknownAction_IsRejectedBeforeQueueOrTaskCreation()
        {
            using (var f = new Fixture())
            {
                f.AddEffectorGraph(100, 0);
                f.Registry.AdvanceWorldStep(0, 0); f.Registry.AdvanceWorldStep(1, 0);
                f.Runtime.Hardware.TryGetEffector(f.Tools[0].Id, out var queue);
                Assert.That(queue.Current, Is.Null); Assert.That(queue.WaitingCount, Is.Zero);
                Assert.That(f.Runtime.Adapter.LastCommandUnavailableReason, Does.Contain("尚未接入"));
                Assert.That(f.HasPort(100, "rejected"), Is.True); Assert.That(f.HasPort(100, "completed"), Is.False);
                Assert.That(f.Runtime.Context.Tasks.History, Is.Empty);
            }
        }
        [Test]
        public void TwoEffectors_ExecuteInParallel_SameEffectorWaits_OneCompletionPerRequest()
        {
            using (var f = new Fixture())
            {
                var executor = new TestDomainExecutor(); f.Registry.Effectors.Register(executor);
                f.AddEffectorGraph(100, 0); f.AddEffectorGraph(200, 0); f.AddEffectorGraph(300, 1);
                f.Registry.AdvanceWorldStep(0, 0);
                f.Runtime.Hardware.TryGetEffector(f.Tools[0].Id, out var one);
                f.Runtime.Hardware.TryGetEffector(f.Tools[1].Id, out var two);
                Assert.That(one.WaitingCount, Is.EqualTo(1)); Assert.That(two.WaitingCount, Is.Zero);
                Assert.That(one.Current.AlgorithmId.Value, Is.EqualTo(100));
                Assert.That(two.Current.AlgorithmId.Value, Is.EqualTo(300));
                f.Registry.AdvanceWorldStep(1, 0); Assert.That(executor.Starts, Is.EqualTo(2));
                Assert.That(f.Runtime.Hardware.PresentationUnavailableReason, Is.Not.Null);
                f.Registry.AdvanceWorldStep(101, 0); Assert.That(executor.Completed.Count, Is.EqualTo(2));
                f.Registry.AdvanceWorldStep(102, 0); Assert.That(executor.Starts, Is.EqualTo(3));
                f.Registry.AdvanceWorldStep(202, 0); f.Registry.AdvanceWorldStep(203, 0);
                Assert.That(executor.Completed.Count, Is.EqualTo(3)); Assert.That(executor.Disposals, Is.EqualTo(3));
                foreach (ulong id in new ulong[] { 100, 200, 300 })
                { Assert.That(f.PortCount(id, "completed"), Is.EqualTo(1)); Assert.That(f.PortCount(id, "started"), Is.EqualTo(1)); }
                Assert.That(f.Target.PublicResourceAmount, Is.EqualTo(12), "The fixture executor and presentation never invent product settlement.");
            }
        }
        [Test]
        public void SafeStopAndHardwareRemoval_DrainOnce_ReleaseLease_KeepPeerQueue()
        {
            using (var f = new Fixture())
            {
                var executor = new TestDomainExecutor(); f.Registry.Effectors.Register(executor);
                f.AddEffectorGraph(100, 0); f.AddEffectorGraph(200, 1);
                f.Registry.AdvanceWorldStep(0, 0); f.Registry.AdvanceWorldStep(1, 0);
                f.Runtime.Hardware.TryGetEffector(f.Tools[1].Id, out var peer);
                Assert.That(f.Runtime.Context.Compute.Used, Is.EqualTo(2));
                using (var operation = new MachineHardwareOperation(f.Session.Machines))
                {
                    operation.Begin(f.Machine.Id, ManagementOrigin.Field, true, HardwareKind.Effector, 0, default);
                    Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Waiting));
                    f.Registry.AdvanceWorldStep(2, 0);
                    Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Completed));
                }
                Assert.That(f.Runtime.Hardware.EffectorCount, Is.EqualTo(1));
                f.Runtime.Hardware.TryGetEffector(f.Tools[1].Id, out var stillPeer); Assert.That(stillPeer, Is.SameAs(peer));
                Assert.That(f.Runtime.Context.Compute.Used, Is.Zero); Assert.That(executor.Completed, Is.Empty);
                Assert.That(f.Runtime.Hardware.TryBindEndpoint(f.Tools[0].Id, f.Target.Id, out _, out _), Is.False);
                f.Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                f.AddEffectorGraph(300, 1); f.Registry.AdvanceWorldStep(3, 0); f.Registry.AdvanceWorldStep(4, 0);
                Assert.That(peer.Current, Is.Not.Null);
                Assert.DoesNotThrow(() => f.Registry.Detach(f.Machine.Id));
                Assert.That(f.Runtime.Context.Compute.Used, Is.Zero); Assert.That(executor.Disposals, Is.EqualTo(3));
            }
        }
        [Test]
        public void PowerResume_DoesNotCatchUpPausedWorkTime()
        {
            using (var f = new Fixture())
            {
                var executor = new TestDomainExecutor(); f.Registry.Effectors.Register(executor);
                f.AddEffectorGraph(100, 0); f.Registry.AdvanceWorldStep(0, 0); f.Registry.AdvanceWorldStep(1, 0);
                f.Machine.UpdateEnvironment(false, true); f.Registry.AdvanceWorldStep(500, 0);
                f.Registry.SetWorldTime(1000); f.Machine.UpdateEnvironment(true, true);
                f.Registry.AdvanceWorldStep(1000, 0); Assert.That(executor.Completed, Is.Empty);
                f.Registry.AdvanceWorldStep(1100, 0); Assert.That(executor.Completed.Count, Is.EqualTo(1));
            }
        }
        [Test]
        public void PowerPauseAndTargetRemoval_DoNotCommitLateOperation()
        {
            using (var f = new Fixture())
            {
                var executor = new TestDomainExecutor(); f.Registry.Effectors.Register(executor);
                f.AddEffectorGraph(100, 0); f.Registry.AdvanceWorldStep(0, 0); f.Registry.AdvanceWorldStep(1, 0);
                f.Machine.UpdateEnvironment(false, true); f.Registry.AdvanceWorldStep(500, 0);
                Assert.That(executor.Completed, Is.Empty);
                f.Region.Remove(f.Target.Id); f.Registry.AdvanceWorldStep(501, 0);
                Assert.That(f.Runtime.Context.Compute.Used, Is.Zero); Assert.That(executor.Disposals, Is.EqualTo(1));
                f.Machine.UpdateEnvironment(true, true);
                for (int i = 0; i < 4; i++) f.Registry.AdvanceWorldStep(1000 + i, 0);
                Assert.That(f.HasPort(100, "targetInvalid"), Is.True); Assert.That(executor.Completed, Is.Empty);
            }
        }

        private sealed class Fixture : IDisposable
        {
            internal readonly AutoEraWorldSession Session = new AutoEraWorldSessionFactory().Create(0);
            internal readonly InitialRegion Region;
            internal readonly RegionMachineRuntimeRegistry Registry;
            internal readonly MachineInstance Machine;
            internal readonly ComponentInstance Sensor, Soil;
            internal readonly ComponentInstance[] Tools = new ComponentInstance[2];
            internal readonly RegionObject Target;
            internal readonly RegionMachineRuntime Runtime;
            internal Fixture()
            {
                Region = new InitialRegion(Session, new Rect(-30, -30, 60, 60));
                var state = new SensorDefinitions(); state.ParseDataRow("\t21011\t\t2101\t1\tObjectState\t1000\t6\t10", null);
                var soil = new SensorDefinitions(); soil.ParseDataRow("\t21021\t\t2102\t1\tSoil\t1000\t4\t10", null);
                Registry = new RegionMachineRuntimeRegistry(Session, Region, null, new MachineNavigationSettings(), .32f, 1.8f, new SensorCatalog(new[] { state, soil }));
                Machine = Session.Machines.Create(new MachineDefinition(1, "hardware fixture", 1, 2, 1, 2, 30, false, false, 100));
                var core = Session.Machines.CreateComponent(new ComponentDefinition(2001, HardwareKind.Core, 1, 0, 100, 100, false));
                Sensor = Session.Machines.CreateComponent(new ComponentDefinition(2101, HardwareKind.Sensor, 1, 0, 0, 0, false));
                Soil = Session.Machines.CreateComponent(new ComponentDefinition(2102, HardwareKind.Sensor, 1, 0, 0, 0, false));
                Session.Machines.Install(Machine.Id, ManagementOrigin.Library, core.Id, 0);
                Session.Machines.Install(Machine.Id, ManagementOrigin.Library, Sensor.Id, 0);
                Session.Machines.Install(Machine.Id, ManagementOrigin.Library, Soil.Id, 1);
                for (int i = 0; i < 2; i++)
                { Tools[i] = Session.Machines.CreateComponent(new ComponentDefinition(2201, HardwareKind.Effector, 1, 0, 0, 0, true)); Session.Machines.Install(Machine.Id, ManagementOrigin.Library, Tools[i].Id, i); }
                Region.DeployMachine(Machine.Id, new Vector2(-4, -4), Vector2.one, out _);
                Machine.Activate(ManagementOrigin.Field); Machine.UpdateEnvironment(true, true); Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                Target = Region.Register(PersistentObjectKind.Building, "public fixture", new Vector2(-4, 0), Vector2.one); Target.SetPublicState("可采集", 12);
                Assert.That(Registry.TryAttach(Machine, null, out Runtime, out var reason), Is.True, reason);
            }
            internal AlgorithmDocument SensorGraph(ulong id)
            {
                Runtime.Hardware.TryBindEndpoint(Sensor.Id, Target.Id, out var generation, out _);
                var graph = new AlgorithmDocument { DocumentId = id };
                graph.Bindings.Add(new AlgorithmBinding { Key = "read", ComponentId = Sensor.Id.Value, TargetId = Target.Id.Value, Generation = generation, Type = AlgorithmType.Of(AlgorithmValueKind.Number) });
                graph.Nodes.Add(new AlgorithmNode { Id = id + 1, Kind = AlgorithmNodeKind.Input, Field = "resource", BindingKey = "read" });
                graph.Nodes.Add(new AlgorithmNode { Id = id + 2, Kind = AlgorithmNodeKind.Log });
                graph.Edges.Add(new AlgorithmEdge { From = id + 1, Output = "sampled", To = id + 2, Input = "event" }); return graph;
            }
            internal void AddEffectorGraph(ulong id, int slot)
            {
                Assert.That(Runtime.Hardware.TryBindEndpoint(Tools[slot].Id, Target.Id, out var generation, out _), Is.True);
                var graph = new AlgorithmDocument { DocumentId = id };
                graph.Bindings.Add(new AlgorithmBinding { Key = "tool", ComponentId = Tools[slot].Id.Value, TargetId = Target.Id.Value, Generation = generation, Type = AlgorithmType.Of(AlgorithmValueKind.Object) });
                graph.Nodes.Add(new AlgorithmNode { Id = id + 1, Kind = AlgorithmNodeKind.Startup });
                graph.Nodes.Add(new AlgorithmNode { Id = id + 2, Kind = AlgorithmNodeKind.Effector, BindingKey = "tool", Action = AlgorithmEffectorAction.Clean });
                graph.Edges.Add(new AlgorithmEdge { From = id + 1, Output = "event", To = id + 2, Input = "event" });
                Assert.That(Runtime.Instances.AddDraft(graph), Is.True); Assert.That(Runtime.TryActivateDraft(id, out var reason), Is.True, reason);
            }
            internal int PortCount(ulong id, string port)
            { int count = 0; foreach (var item in Runtime.Instances.ReadHistory(id)) if (item.CopyTrigger().Port == port) count++; return count; }
            internal bool HasPort(ulong id, string port) => PortCount(id, port) > 0;
            public void Dispose() { Registry.Dispose(); Region.Dispose(); Session.Dispose(); }
        }
        /// <summary>Tests only the executor boundary. It never alters product inventories or fabricates production proof.</summary>
        private sealed class TestDomainExecutor : IRegionEffectorExecutor
        {
            internal int Starts, Disposals;
            internal readonly HashSet<PersistentId> Completed = new HashSet<PersistentId>();
            public bool Supports(ComponentDefinition component, AlgorithmEffectorAction action) => action == AlgorithmEffectorAction.Clean;
            public bool TryStart(MachineExecutionContext context, ComponentInstance component, BehaviorRequest<EffectorBehaviorParameters> request,
                long now, out IRegionEffectorOperation operation, out string reason)
            { Starts++; operation = new Operation(this, context, request.Id, now); reason = null; return true; }
            private sealed class Operation : IRegionEffectorOperation
            {
                private readonly TestDomainExecutor _owner;
                private readonly MachineExecutionContext _context;
                private readonly PersistentId _id;
                private long _start, _pauseAt;
                private bool _paused;
                private readonly ComputeRequest _lease;
                private bool _disposed;
                public bool IsCompleted { get; private set; }
                public BehaviorOutcome Outcome { get; private set; }
                internal Operation(TestDomainExecutor owner, MachineExecutionContext context, PersistentId id, long now)
                {
                    _owner = owner; _context = context; _id = id; _start = now;
                    context.Compute.Submit(1, ComputeClass.Continuation, WorkPriority.Normal, ComputeMergeKind.None, id, 1, now, false, out _lease);
                }
                public void Advance(long now)
                { if (!IsCompleted && now >= _start + 100) { _owner.Completed.Add(_id); Stop(BehaviorOutcome.Completed); } }
                public void SetPaused(bool paused, long now)
                {
                    if (_paused == paused) return;
                    _paused = paused;
                    if (paused) _pauseAt = now; else _start += Math.Max(0, now - _pauseAt);
                }
                public void Stop(BehaviorOutcome outcome)
                { if (IsCompleted) return; IsCompleted = true; Outcome = outcome; if (_lease != null) _context.Compute.Release(_lease.Id); }
                public void Dispose() { if (_disposed) return; _disposed = true; Stop(BehaviorOutcome.Cancelled); _owner.Disposals++; }
            }
        }
    }
}
