using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Buildings;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.ResourcePoints;
using AutoEra.Save;
using AutoEra.World.Identity;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmPersistentSnapshotEditModeTests
    {
        private sealed class Sink : IAlgorithmCommandSink
        {
            internal bool Safe = true;
            internal int Commands, Cancelled;
            internal ulong Task;
            internal double? CargoAmount;
            public bool IsSafe => Safe;
            public ulong Submit(AlgorithmTrigger trigger, AlgorithmIntent intent) { Commands++; return Task == 0 ? trigger.TaskId : Task; }
            public void EndBatch(AlgorithmTrigger trigger) { }
            public void Cancel() { Cancelled++; }
            public bool TryReadCargo(string field, string item, out AlgorithmValue value)
            { value = CargoAmount.HasValue ? AlgorithmValue.Numeric(CargoAmount.Value) : null; return value != null; }
            public bool TryQueryTask(string name, out AlgorithmValue value) { value = null; return false; }
        }
        private sealed class ServiceData { public AlgorithmInstanceSnapshot[] Rows; }
        private sealed class TemplateData { public AlgorithmTemplateSnapshot[] Rows; }
        private sealed class GridData { public AlgorithmValue Value; }
        private static T Json<T>(T data, long now = 5000) where T : class
        {
            string json = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(now, 1000, 1, "Fixture",
                new[] { new WorldSnapshotSection("algorithm", 1, data) }));
            Assert.That(WorldSnapshotCodec.TryRead(json, new Dictionary<string, int> { { "algorithm", 1 } }, out var file, out var reason), Is.True, reason);
            Assert.That(file.TryReadSection<T>("algorithm", out var result, out reason), Is.True, reason);
            return result;
        }
        private static AlgorithmDocument Graph(bool delay = false)
        {
            var graph = AlgorithmExecutionEditModeTests.Graph(); graph.DocumentId = 100;
            graph.Nodes[1].Kind = AlgorithmNodeKind.Parameter;
            if (delay)
            {
                graph.Nodes.Add(new AlgorithmNode { Id = 8, Kind = AlgorithmNodeKind.Constant, ValueType = AlgorithmType.Of(AlgorithmValueKind.Number, "s"), Default = AlgorithmValue.Numeric(1, "s") });
                graph.Nodes.Add(new AlgorithmNode { Id = 9, Kind = AlgorithmNodeKind.Delay });
                graph.Nodes.Add(new AlgorithmNode { Id = 10, Kind = AlgorithmNodeKind.Log });
                graph.Edges.Add(new AlgorithmEdge { From = 1, To = 9, Output = "event", Input = "event" });
                graph.Edges.Add(new AlgorithmEdge { From = 8, To = 9, Input = "seconds" });
                graph.Edges.Add(new AlgorithmEdge { From = 9, To = 10, Output = "event", Input = "event" });
            }
            return graph;
        }
        private static AlgorithmRuntime Runtime(AlgorithmDocument graph, MachineComputePool pool, Sink sink)
        {
            Assert.That(AlgorithmValidator.TryCompile(graph, 100, out var plan, out _), Is.True);
            return new AlgorithmRuntime(new PersistentId(graph.DocumentId), plan, pool, sink);
        }
        private static void Start(AlgorithmRuntime runtime, long now = 0)
        { Assert.That(runtime.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1 }), Is.True); runtime.Pump(now); }

        private static void RestoreCompute(MachineComputePool source, MachineComputePool target)
        { Assert.That(source.TryCapturePersistent(out var snapshot), Is.True); Assert.That(target.RestorePersistent(Json(snapshot), 5000), Is.True); }

        [Test] public void Json_RestoresVariablesDelayedTaskAndExecutedHistory_WithoutStartupOrCancel()
        {
            var sourceSink = new Sink { Task = 44 };
            var sourcePool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            using (var source = Runtime(Graph(true), sourcePool, sourceSink))
            {
                Start(source); sourceSink.Safe = false;
                Assert.That(source.TryCapture(400, out _), Is.False);
                Assert.That(source.TryCapturePersistent(400, out var snapshot), Is.True);
                var saved = Json(snapshot); Assert.That(saved.Delays[0].Trigger.TaskId, Is.EqualTo(44));
                var sink = new Sink();
                var targetPool = new MachineComputePool(new PersistentIdAllocator(), 100, 100); RestoreCompute(sourcePool, targetPool);
                using (var target = Runtime(Graph(true), targetPool, sink))
                {
                    Assert.That(target.RestorePersistent(saved, 5000), Is.True);
                    Assert.That(sink.Cancelled, Is.Zero); Assert.That(sink.Commands, Is.Zero);
                    Assert.That(target.CopyState()["counter"].Number, Is.EqualTo(12)); Assert.That(target.WaitingCount, Is.Zero);
                    var history = target.History()[0];
                    Assert.That(history.CopyPath(), Is.EqualTo(source.History()[0].CopyPath()));
                    Assert.That(history.CopyNodeValues()[2].Number, Is.EqualTo(12));
                    Assert.That(history.CopyExecutedDocument().Nodes[1].Default.Number, Is.EqualTo(12));
                    target.Pump(5599); Assert.That(sink.Commands, Is.Zero);
                    target.Pump(5600); Assert.That(sink.Commands, Is.EqualTo(1));
                    Assert.That(target.History()[1].RunId, Is.EqualTo(2)); Assert.That(target.History()[1].CopyTrigger().TaskId, Is.EqualTo(44));
                    snapshot.State["counter"].Number = -1; Assert.That(source.CopyState()["counter"].Number, Is.EqualTo(12));
                }
            }
        }

        [Test] public void PausedDelay_RemainsFrozen_AndQueuedInputIsDetachedAndRemapped()
        {
            using (var source = Runtime(Graph(true), new MachineComputePool(new PersistentIdAllocator(), 100, 100), new Sink()))
            {
                Start(source); source.SetPaused(true, 200, AlgorithmPauseReason.User);
                var trigger = new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = 1, TaskId = 45, Time = 300 };
                trigger.Inputs.Add("payload", AlgorithmValue.Numeric(7)); source.Enqueue(trigger);
                source.TryCapturePersistent(3000, out var snapshot);
                var sink = new Sink();
                using (var target = Runtime(Graph(true), new MachineComputePool(new PersistentIdAllocator(), 100, 100), sink))
                {
                    Assert.That(target.RestorePersistent(Json(snapshot), 5000), Is.True);
                    target.Pump(8000); Assert.That(sink.Commands, Is.Zero);
                    target.TryCapturePersistent(8000, out var frozen); Assert.That(frozen.Delays[0].RemainingMilliseconds, Is.EqualTo(800));
                    Assert.That(frozen.Events[0].Generation, Is.EqualTo(target.Generation));
                    Assert.That(frozen.Events[0].Inputs["payload"].Number, Is.EqualTo(7));
                    Assert.That(target.Enqueue(trigger), Is.False);
                }
            }
        }

        [Test] public void WaitingPureBatch_IsCapturedWithPreparedWrites_WithoutCancellationOrReevaluation()
        {
            var ids = new PersistentIdAllocator(); var pool = new MachineComputePool(ids, 100, 100); var sink = new Sink();
            pool.Submit(100, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None, new PersistentId(90), 1, 0, false, out var held);
            using (var source = Runtime(Graph(), pool, sink))
            {
                Start(source); Assert.That(source.TryCapture(0, out _), Is.False);
                Assert.That(source.TryCapturePersistent(0, out var snapshot), Is.True); Assert.That(snapshot.Prepared, Is.Not.Null);
                var restoredIds = new PersistentIdAllocator(); var restoredPool = new MachineComputePool(restoredIds, 100, 100);
                RestoreCompute(pool, restoredPool); var restoredSink = new Sink();
                using (var target = Runtime(Graph(), restoredPool, restoredSink))
                {
                    Assert.That(target.RestorePersistent(Json(snapshot), 5000), Is.True);
                    target.Pump(5000); Assert.That(target.CopyState(), Is.Empty); Assert.That(restoredSink.Commands, Is.Zero);
                    restoredPool.Release(held.Id); target.Pump(5001);
                    Assert.That(target.CopyState()["counter"].Number, Is.EqualTo(12)); Assert.That(restoredSink.Commands, Is.EqualTo(1));
                    target.Pump(5002); Assert.That(restoredSink.Commands, Is.EqualTo(1));
                }
                Assert.That(sink.Cancelled, Is.Zero); pool.Release(held.Id); source.Pump(1);
                Assert.That(source.TryCapturePersistent(1, out _), Is.True);
            }
        }

        [Test] public void InvalidHistoryOrDelay_DoesNotPartiallyMutateFreshCandidate()
        {
            var sourcePool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            using (var source = Runtime(Graph(true), sourcePool, new Sink()))
            {
                Start(source); source.TryCapturePersistent(0, out var snapshot);
                var targetPool = new MachineComputePool(new PersistentIdAllocator(), 100, 100); RestoreCompute(sourcePool, targetPool);
                using (var target = Runtime(Graph(), targetPool, new Sink()))
                {
                    var bad = Json(snapshot); bad.History[0].Path[0] = 999;
                    Assert.That(target.RestorePersistent(bad, 5000), Is.False); Assert.That(target.CopyState(), Is.Empty); Assert.That(target.Generation, Is.EqualTo(1));
                    bad = Json(snapshot); bad.Delays[0].RemainingMilliseconds = -1;
                    Assert.That(target.RestorePersistent(bad, 5000), Is.False); Assert.That(target.History(), Is.Empty);
                    Assert.That(target.RestorePersistent(Json(snapshot), 5000), Is.True);
                    Assert.That(target.RestorePersistent(Json(snapshot), 5000), Is.False, "Persistent restore is only permitted once on a fresh candidate.");
                }
            }
        }

        [Test] public void PreparedCargoRead_IsCommittedAsSaved_EvenWhenLiveCargoHasChanged()
        {
            var graph = Graph(); graph.Nodes[1].Kind = AlgorithmNodeKind.Cargo; graph.Nodes[1].Default = null; graph.Nodes[1].Field = ResourceItemCatalog.Wood;
            graph.Edges.Find(e => e.From == 2).Output = "amount";
            var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100); var sink = new Sink { CargoAmount = 5 };
            pool.Submit(100, ComputeClass.Safety, WorkPriority.Urgent, ComputeMergeKind.None, default, 1, 0, false, out var held);
            using (var source = Runtime(graph, pool, sink))
            {
                Start(source); sink.CargoAmount = 99;
                Assert.That(source.TryCapturePersistent(0, out var snapshot), Is.True); Assert.That(snapshot.Prepared.Writes["counter"].Number, Is.EqualTo(5));
                var targetPool = new MachineComputePool(new PersistentIdAllocator(), 100, 100); RestoreCompute(pool, targetPool); var targetSink = new Sink { CargoAmount = 777 };
                using (var target = Runtime(graph, targetPool, targetSink))
                {
                    Assert.That(target.RestorePersistent(Json(snapshot), 5000), Is.True); targetPool.Release(held.Id); target.Pump(5001);
                    Assert.That(target.CopyState()["counter"].Number, Is.EqualTo(5)); Assert.That(target.History().Length, Is.EqualTo(1));
                    Assert.That(target.Enqueue(new AlgorithmTrigger { NodeId = 1, Revision = 1, Generation = target.Generation }), Is.True); target.Pump(5002);
                    Assert.That(target.CopyState()["counter"].Number, Is.EqualTo(777));
                }
            }
        }

        [TestCase("cost")] [TestCase("write")] public void ConsistentlyTamperedPreparedBatchAndLease_AreRejectedAgainstTheAppliedGraph(string field)
        {
            var pool = new MachineComputePool(new PersistentIdAllocator(), 100, 100);
            pool.Submit(100, ComputeClass.Safety, WorkPriority.Urgent, ComputeMergeKind.None, default, 1, 0, false, out _);
            using (var source = Runtime(Graph(), pool, new Sink()))
            {
                Start(source); source.TryCapturePersistent(0, out var snapshot); pool.TryCapturePersistent(out var compute);
                var saved = Json(snapshot); var savedCompute = Json(compute);
                if (field == "cost") { saved.Prepared.Cost++; savedCompute.Waiting[0].Cost++; }
                else { saved.Prepared.Writes["foreign"] = AlgorithmValue.Numeric(5); savedCompute.Waiting[0].Cost += 2; }
                var targetPool = new MachineComputePool(new PersistentIdAllocator(), 100, 100); Assert.That(targetPool.RestorePersistent(savedCompute, 5000), Is.True);
                using (var target = Runtime(Graph(), targetPool, new Sink()))
                {
                    Assert.That(target.RestorePersistent(saved, 5000), Is.False);
                    Assert.That(target.Generation, Is.EqualTo(1)); Assert.That(target.CopyState(), Is.Empty); Assert.That(target.History(), Is.Empty);
                }
            }
        }

        [Test] public void InstanceService_JsonPreservesInactiveDraftSavedRevisionAndPendingApply()
        {
            var ids = new PersistentIdAllocator(); ids.TryRestore(new PersistentId(400));
            var sourcePool = new MachineComputePool(ids, 100, 100); var sourceSink = new Sink();
            using (var source = new AlgorithmInstanceService(ids, sourcePool, () => 1, d => true))
            {
                var runtime = Runtime(Graph(), sourcePool, sourceSink); source.Add(runtime); Start(runtime);
                var draft = source.ReadDraft(100); draft.Nodes[1].Default.Number = 20; source.Edit(100, 1, draft); source.SaveDraft(100, 2);
                sourceSink.Safe = false; source.Apply(100, 2, 1, 1, out var request);
                var inactive = Graph(); inactive.DocumentId = 300; inactive.Nodes[0].Deleted = true; source.AddDraft(inactive);
                Assert.That(source.TryCapturePersistent(10, out var rows), Is.True);
                var restoredIds = new PersistentIdAllocator(); var pool = new MachineComputePool(restoredIds, 100, 100); var sink = new Sink { Safe = false };
                RestoreCompute(sourcePool, pool);
                using (var target = new AlgorithmInstanceService(restoredIds, pool, () => 1, d => true))
                {
                    AlgorithmRuntime restored = null;
                    Assert.That(target.RestorePersistent(Json(new ServiceData { Rows = rows }).Rows, 5000,
                        (id, plan) => restored = new AlgorithmRuntime(new PersistentId(id), plan, pool, sink), out var reason), Is.True, reason);
                    Assert.That(target.ListInstances().Length, Is.EqualTo(2)); Assert.That(target.ListInstances()[1].AppliedRevision, Is.Zero);
                    Assert.That(target.ReadDraft(100).Nodes[1].Default.Number, Is.EqualTo(20)); Assert.That(target.SavedDraftRevision(100), Is.EqualTo(2));
                    Assert.That(target.ReadRequest(100).RequestId, Is.EqualTo(request.RequestId)); Assert.That(restored.CopyState()["counter"].Number, Is.EqualTo(12));
                    target.Pump(5000); Assert.That(restored.Revision, Is.EqualTo(1));
                    sink.Safe = true; target.Pump(5001); target.Pump(5002); Assert.That(restored.Revision, Is.EqualTo(2));
                    Assert.That(restoredIds.NextId.Value, Is.GreaterThan(request.RequestId));
                }
            }
        }

        [Test] public void DuplicateInstance_LeavesCandidateAndAllocatorEmpty()
        {
            var ids = new PersistentIdAllocator(); var pool = new MachineComputePool(ids, 100, 100);
            using (var service = new AlgorithmInstanceService(ids, pool, () => 1, d => true))
            {
                var row = new AlgorithmInstanceSnapshot { InstanceId = 100, Draft = Graph(), Saved = Graph() };
                Assert.That(service.RestorePersistent(new[] { row, row }, 0, (id, plan) => null, out _), Is.False);
                Assert.That(service.ListInstances(), Is.Empty); Assert.That(ids.NextId.Value, Is.EqualTo(1)); Assert.That(pool.AppliedLogicCost, Is.Zero);
            }
        }

        [Test] public void TemplateLibrary_RestoresExactIdentitiesAndSystemProtectionWithoutSeeding()
        {
            var original = new AlgorithmTemplateLibrary(new PersistentIdAllocator());
            ulong system = original.Save("系统", Graph(), true); ulong player = original.CopyAsPlayer(system, "玩家"); original.Rename(player, 1, "改名");
            var ids = new PersistentIdAllocator(); var restored = new AlgorithmTemplateLibrary(ids);
            Assert.That(restored.RestorePersistent(Json(new TemplateData { Rows = original.CapturePersistent() }).Rows), Is.True);
            Assert.That(restored.List()[1].Version, Is.EqualTo(2)); Assert.That(restored.List()[1].Name, Is.EqualTo("改名"));
            Assert.That(restored.Delete(system, 1), Is.False); Assert.That(ids.NextId.Value, Is.GreaterThan(player));
            Assert.That(restored.Instantiate(player).Nodes[1].Default.Number, Is.EqualTo(12));
        }

        [Test] public void ImmutableTreeGrid_JsonPreservesReadoutsAndFinitePositions()
        {
            var ids = new PersistentIdAllocator(); var registry = new PersistentObjectRegistry(ids);
            var cargo = new CargoOwnershipAuthority(ids, new ResourceItemCatalog(new[] {
                new ResourceItemDefinition(ResourceItemCatalog.Wood, CargoItemClass.CommonResource) }), registry);
            var config = ScriptableObject.CreateInstance<ForestMineralProductionConfig>();
            var rules = config.Read(); Object.DestroyImmediate(config);
            using (cargo)
            using (var forest = new ForestProduction(new PersistentId(900), ids, registry, rules, cargo,
                new[] { new Vector3(3, 0, -7), new Vector3(4, 0, -6) }, 0))
            {
                var value = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.TreeGrid), Trees = new AlgorithmTreeGrid(forest.CaptureTrees()) };
                var restored = Json(new GridData { Value = value }).Value;
                Assert.That(restored.Trees.Cells.Count, Is.EqualTo(2)); var cell = restored.Trees.Cells[0];
                Assert.That(cell.Id, Is.EqualTo(forest.ReadAt(0).Id)); Assert.That(cell.Position, Is.EqualTo(forest.ReadAt(0).Position));
                Assert.That(cell.Height, Is.EqualTo(forest.ReadAt(0).Height)); Assert.That(cell.HP, Is.EqualTo(cell.Height)); Assert.That(cell.Stage, Is.EqualTo(TreeStage.Mature));
                Assert.That(AlgorithmGridOperations.Select(restored).ObjectId, Is.EqualTo(cell.Id.Value));
            }
        }
    }
}
