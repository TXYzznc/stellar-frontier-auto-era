using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class ProductionAlgorithmDriveEditModeTests
    {
        private static AlgorithmDocument Graph(ulong id, AlgorithmNodeKind command = AlgorithmNodeKind.Log)
        {
            var graph = new AlgorithmDocument { DocumentId = id };
            graph.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup });
            graph.Nodes.Add(new AlgorithmNode { Id = 2, Kind = command, Field = "production-drive" });
            graph.Edges.Add(new AlgorithmEdge { From = 1, Output = "event", To = 2, Input = "event" });
            return graph;
        }

        [Test]
        public void WorldStep_StartsOnce_AndRestoringDoesNotReplayStartup()
        {
            using (var fixture = new Fixture())
            {
                var runtime = fixture.Attach(fixture.CreateMachine());
                Assert.That(runtime.Instances.AddDraft(Graph(100)), Is.True);
                Assert.That(runtime.TryActivateDraft(100, out string reason), Is.True, reason);
                fixture.Registry.AdvanceWorldStep(0, 0);
                Assert.That(runtime.Instances.ReadHistory(100).Length, Is.EqualTo(1));
                Assert.That(runtime.Instances.Capture(100, 0, out var checkpoint), Is.True);
                Assert.That(runtime.RestoreInstance(100, checkpoint, 1000), Is.True);
                for (int i = 1; i <= 5; i++) fixture.Registry.AdvanceWorldStep(1000 + i, i);
                Assert.That(runtime.Instances.ReadHistory(100).Length, Is.EqualTo(1));
                Assert.That(runtime.TryActivateDraft(100, out reason), Is.False);
                Assert.That(reason, Is.EqualTo("AlreadyActivated"));
                Assert.That(runtime.Adapter.HasRuntime, Is.True, "Rejected repeat activation must not detach the existing instance.");
            }
        }

        [Test]
        public void CapacityRejection_LeavesNoAttachment_AndCanRetry()
        {
            using (var fixture = new Fixture())
            {
                var runtime = fixture.Attach(fixture.CreateMachine(logicCapacity: 2));
                var oversized = Graph(100);
                oversized.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Log });
                oversized.Edges.Add(new AlgorithmEdge { From = 1, Output = "event", To = 3, Input = "event" });
                Assert.That(runtime.Instances.AddDraft(oversized), Is.True);
                Assert.That(runtime.TryActivateDraft(100, out _), Is.False);
                Assert.That(runtime.Adapter.HasRuntime, Is.False);
                Assert.That(runtime.Context.Compute.AppliedLogicCost, Is.Zero);
                Assert.That(runtime.Context.Compute.Used, Is.Zero);
                Assert.That(runtime.Instances.Edit(100, 1, Graph(100)), Is.True);
                Assert.That(runtime.TryActivateDraft(100, out string reason), Is.True, reason);
                fixture.Registry.AdvanceWorldStep(1, 0);
                Assert.That(runtime.Instances.ReadHistory(100).Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void RevisionChangesDuringCommit_RollBackAndRemainRetryable()
        {
            using (var fixture = new Fixture())
            {
                var runtime = fixture.Attach(fixture.CreateMachine());
                runtime.Instances.AddDraft(Graph(100));
                bool changed = false;
                runtime.Context.Compute.Changed += pool =>
                {
                    if (changed || pool.AppliedLogicCost == 0) return;
                    changed = true;
                    Assert.That(runtime.Instances.Edit(100, 1, Graph(100)), Is.True);
                };
                Assert.That(runtime.TryActivateDraft(100, out string reason), Is.False);
                Assert.That(reason, Is.EqualTo("StaleRevision"));
                Assert.That(runtime.Adapter.HasRuntime, Is.False);
                Assert.That(runtime.Context.Compute.AppliedLogicCost, Is.Zero);
                Assert.That(runtime.TryActivateDraft(100, out reason), Is.True, reason);
                fixture.Registry.AdvanceWorldStep(1, 1);
                Assert.That(runtime.Instances.ReadHistory(100).Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void MultipleInstances_DeleteOneWithoutCancellingPeer()
        {
            using (var fixture = new Fixture())
            {
                var runtime = fixture.Attach(fixture.CreateMachine());
                Assert.That(runtime.Instances.AddDraft(Graph(100, AlgorithmNodeKind.SubmitTask)), Is.True);
                Assert.That(runtime.Instances.AddDraft(Graph(200)), Is.True);
                Assert.That(runtime.TryActivateDraft(100, out string a), Is.True, a);
                Assert.That(runtime.TryActivateDraft(200, out string b), Is.True, b);
                fixture.Registry.AdvanceWorldStep(0, 0);
                Assert.That(runtime.Instances.ReadHistory(100).Length, Is.EqualTo(1));
                Assert.That(runtime.Instances.ReadHistory(200).Length, Is.EqualTo(1));
                Assert.That(runtime.RemoveInstance(100), Is.True);
                Assert.That(runtime.Instances.ListInstances().Length, Is.EqualTo(1));
                Assert.That(runtime.Adapter.HasRuntime, Is.True);
                fixture.Registry.AdvanceWorldStep(1, 1);
                Assert.That(runtime.Instances.ReadHistory(200).Length, Is.EqualTo(1));
                Assert.That(runtime.Context.Compute.AppliedLogicCost, Is.EqualTo(2));
                Assert.That(runtime.RemoveInstance(200), Is.True);
                Assert.That(runtime.Context.Compute.AppliedLogicCost, Is.Zero);
                Assert.That(runtime.Context.Compute.Used, Is.Zero);
                Assert.That(runtime.Adapter.HasRuntime, Is.False);
            }
        }

        [Test]
        public void CallbackChangesRegistry_UseStableOrderAndStepBoundary()
        {
            using (var fixture = new Fixture())
            {
                var first = fixture.CreateMachine();
                var second = fixture.CreateMachine();
                var b = fixture.Attach(second); // Attach in reverse order.
                var a = fixture.Attach(first);
                var sequence = new List<PersistentId>();
                RegionMachineRuntime added = null;
                a.Adapter.Result += (node, task, port) =>
                {
                    sequence.Add(a.MachineId);
                    Assert.That(fixture.Registry.Detach(b.MachineId), Is.True);
                    added = fixture.Attach(fixture.CreateMachine());
                    Assert.That(added.Instances.AddDraft(Graph(300)), Is.True);
                    Assert.That(added.TryActivateDraft(300, out _), Is.True);
                };
                b.Adapter.Result += (node, task, port) => sequence.Add(b.MachineId);
                a.Instances.AddDraft(Graph(100)); b.Instances.AddDraft(Graph(200));
                Assert.That(a.TryActivateDraft(100, out _), Is.True);
                Assert.That(b.TryActivateDraft(200, out _), Is.True);
                Assert.DoesNotThrow(() => fixture.Registry.AdvanceWorldStep(0, 0));
                CollectionAssert.AreEqual(new[] { first.Id, second.Id }, sequence);
                Assert.That(fixture.Registry.TryGet(second.Id, out _), Is.False);
                Assert.That(added.Instances.ReadHistory(300), Is.Empty, "New arrivals start next step.");
                fixture.Registry.AdvanceWorldStep(1, 1);
                Assert.That(added.Instances.ReadHistory(300).Length, Is.EqualTo(1));
            }
        }

        [Test]
        public void ApplicationPause_DoesNotClearPowerPause_AndDraftPeerDoesNotBlockSafePoint()
        {
            using (var fixture = new Fixture())
            {
                var machine = fixture.CreateMachine();
                var runtime = fixture.Attach(machine);
                runtime.Instances.AddDraft(Graph(100)); runtime.Instances.AddDraft(Graph(200));
                Assert.That(runtime.TryActivateDraft(100, out _), Is.True);
                fixture.Registry.AdvanceWorldStep(0, 0);
                var changed = runtime.Instances.ReadDraft(100);
                changed.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Log });
                changed.Edges.Add(new AlgorithmEdge { From = 1, Output = "event", To = 3, Input = "event" });
                Assert.That(runtime.Instances.Edit(100, 1, changed), Is.True);
                Assert.That(runtime.Instances.Apply(100, 2, 1, out _), Is.True);
                fixture.Registry.AdvanceWorldStep(1, 1);
                Assert.That(runtime.Instances.ReadRequest(100).State, Is.EqualTo(AlgorithmApplyState.Applying));
                machine.UpdateSupply(false);
                fixture.Registry.AdvanceWorldStep(2, 2);
                Assert.That(runtime.Instances.ReadRequest(100).State, Is.EqualTo(AlgorithmApplyState.Succeeded));
                fixture.Registry.AdvanceWorldStep(3, 3);
                Assert.That(runtime.Instances.ReadHistory(100).Length, Is.EqualTo(1), "Still paused for power.");
                machine.UpdateSupply(true);
                fixture.Registry.AdvanceWorldStep(4, 4);
                Assert.That(runtime.Instances.ReadHistory(100).Length, Is.EqualTo(2));
            }
        }

        [Test]
        public void StableIdleWorldSteps_DoNotAllocate()
        {
            using (var fixture = new Fixture())
            {
                var runtime = fixture.Attach(fixture.CreateMachine());
                runtime.Instances.AddDraft(Graph(100));
                Assert.That(runtime.TryActivateDraft(100, out _), Is.True);
                for (int i = 0; i < 20; i++) fixture.Registry.AdvanceWorldStep(i, i);
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 20; i < 1020; i++) fixture.Registry.AdvanceWorldStep(i, i);
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(allocated, Is.Zero);
            }
        }

        [Test]
        public void Detach_ClearsOwnedRequestsAndCompute_AndLateAdvanceIsHarmless()
        {
            using (var fixture = new Fixture())
            {
                var runtime = fixture.Attach(fixture.CreateMachine());
                runtime.Instances.AddDraft(Graph(100, AlgorithmNodeKind.SubmitTask));
                Assert.That(runtime.TryActivateDraft(100, out _), Is.True);
                fixture.Registry.AdvanceWorldStep(0, 0);
                Assert.That(fixture.Registry.Detach(runtime.MachineId), Is.True);
                Assert.That(runtime.Adapter.HasRuntime, Is.False);
                Assert.That(runtime.Context.Compute.AppliedLogicCost, Is.Zero);
                Assert.That(runtime.Context.Compute.Used, Is.Zero);
                Assert.DoesNotThrow(() => fixture.Registry.AdvanceWorldStep(100, 100));
            }
        }

        private sealed class Fixture : IDisposable
        {
            internal readonly AutoEraWorldSession Session = new AutoEraWorldSessionFactory().Create(0);
            internal readonly InitialRegion Region;
            internal readonly RegionMachineRuntimeRegistry Registry;
            private int _position;
            internal Fixture()
            {
                Region = new InitialRegion(Session, new Rect(-30, -30, 60, 60));
                Registry = new RegionMachineRuntimeRegistry(Session, Region, null, new MachineNavigationSettings(), .32f, 1.8f);
            }
            internal MachineInstance CreateMachine(int logicCapacity = 100)
            {
                var machine = Session.Machines.Create(new MachineDefinition(1, "Drive fixture", 1, 1, 1, 1, 20, false, false, 100, 2, 2));
                var core = Session.Machines.CreateComponent(new ComponentDefinition(20011, HardwareKind.Core, 1, 0, 100, logicCapacity, false));
                Assert.That(Session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(Region.DeployMachine(machine.Id, new Vector2(-20 + _position++ * 5, -20), machine.Definition.Footprint, out _), Is.EqualTo(RegionMachineDeploymentResult.Bound));
                Assert.That(machine.Activate(ManagementOrigin.Field), Is.EqualTo(MachineManagementResult.Completed));
                machine.UpdateEnvironment(true, true);
                Assert.That(machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running), Is.EqualTo(MachineManagementResult.Completed));
                return machine;
            }
            internal RegionMachineRuntime Attach(MachineInstance machine)
            {
                Assert.That(Registry.TryAttach(machine, null, out var runtime, out string reason), Is.True, reason);
                return runtime;
            }
            public void Dispose() { Registry.Dispose(); Region.Dispose(); Session.Dispose(); }
        }
    }
}
