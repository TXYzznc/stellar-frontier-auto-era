using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineComputeSnapshotEditModeTests
    {
        private static MachineComputeSnapshot Json(MachineComputePool pool)
        {
            Assert.That(pool.TryCapturePersistent(out var snapshot), Is.True);
            string json = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(1000, 1000, 1, "Fixture", new[] { new WorldSnapshotSection("compute", 1, snapshot) }));
            Assert.That(WorldSnapshotCodec.TryRead(json, new Dictionary<string, int> { { "compute", 1 } }, out var file, out var reason), Is.True, reason);
            Assert.That(file.TryReadSection<MachineComputeSnapshot>("compute", out var saved, out reason), Is.True, reason); return saved;
        }
        [Test] public void RunningAndWaiting_KeepIdsPriorityAndFifo_WithNoReplay()
        {
            var source = new MachineComputePool(new PersistentIdAllocator(), 20, 50); source.TryApplyLogicCost(12);
            source.Submit(20, ComputeClass.Safety, WorkPriority.Normal, ComputeMergeKind.None, new PersistentId(100), 1, 1, false, out var held);
            source.Submit(10, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None, new PersistentId(101), 1, 2, false, out var first);
            source.Submit(10, ComputeClass.Continuation, WorkPriority.High, ComputeMergeKind.None, new PersistentId(102), 1, 3, false, out var high);
            source.Submit(10, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None, new PersistentId(103), 1, 4, false, out var second);
            var ids = new PersistentIdAllocator(); var restored = new MachineComputePool(ids, 20, 50); var started = new List<PersistentId>(); int changed = 0;
            restored.Started += r => started.Add(r.Id); restored.Changed += _ => changed++;
            Assert.That(restored.RestorePersistent(Json(source), 1000), Is.True);
            Assert.That(started, Is.Empty); Assert.That(changed, Is.Zero); Assert.That(restored.Used, Is.EqualTo(20)); Assert.That(restored.AppliedLogicCost, Is.EqualTo(12));
            restored.Release(held.Id); Assert.That(started, Is.EqualTo(new[] { high.Id, first.Id }));
            restored.Release(high.Id); Assert.That(started[2], Is.EqualTo(second.Id));
            Assert.That(ids.NextId.Value, Is.GreaterThan(second.Id.Value)); Assert.That(source.WaitingCount, Is.EqualTo(3));
        }
        [Test] public void DisabledDispatch_AndExplicitMergeGenerationSurviveRestoration()
        {
            var source = new MachineComputePool(new PersistentIdAllocator(), 20, 20); source.SetDispatchEnabled(false);
            source.Submit(10, ComputeClass.Sampling, WorkPriority.Normal, ComputeMergeKind.SensorSample, new PersistentId(100), 1, 0, true, out var pending);
            var restored = new MachineComputePool(new PersistentIdAllocator(), 20, 20);
            Assert.That(restored.RestorePersistent(Json(source), 1000), Is.True);
            Assert.That(restored.Submit(12, ComputeClass.Sampling, WorkPriority.Normal, ComputeMergeKind.SensorSample, new PersistentId(100), 2, 1000, true, out var merged), Is.EqualTo(ComputeAdmission.Merged));
            Assert.That(merged.Id, Is.EqualTo(pending.Id)); Assert.That(restored.Used, Is.Zero); Assert.That(restored.WaitingCount, Is.EqualTo(1));
            restored.SetDispatchEnabled(true); Assert.That(merged.State, Is.EqualTo(ComputeState.Running)); Assert.That(restored.Used, Is.EqualTo(12));
        }
        [Test] public void DuplicateAndOversubscribedLeases_RejectBeforeCandidateOrAllocatorChanges()
        {
            var source = new MachineComputePool(new PersistentIdAllocator(), 20, 20);
            source.Submit(20, ComputeClass.Safety, WorkPriority.Normal, ComputeMergeKind.None, default, 1, 0, false, out _);
            var ids = new PersistentIdAllocator(); var restored = new MachineComputePool(ids, 20, 20); var duplicate = Json(source);
            duplicate.Running = new[] { duplicate.Running[0], duplicate.Running[0] };
            Assert.That(restored.RestorePersistent(duplicate, 1000), Is.False); Assert.That(restored.Used, Is.Zero); Assert.That(ids.NextId.Value, Is.EqualTo(1));
            var bad = Json(source); bad.Running = new[] { bad.Running[0], new ComputeRequestSnapshot { Id = new PersistentId(888), Cost = 20,
                State = ComputeState.Running, Class = ComputeClass.Safety, Priority = WorkPriority.Normal } };
            Assert.That(restored.RestorePersistent(bad, 1000), Is.False); Assert.That(restored.Used, Is.Zero);
            Assert.That(restored.RestorePersistent(Json(source), 1000), Is.True);
        }
        [Test] public void CaptureDuringDispatchCallback_IsDeferredInsteadOfTakingHalfQueue()
        {
            var source = new MachineComputePool(new PersistentIdAllocator(), 20, 20); bool? capture = null;
            source.Started += request => capture = source.TryCapturePersistent(out _);
            source.Submit(10, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None, default, 1, 0, false, out _);
            Assert.That(capture, Is.False); Assert.That(source.TryCapturePersistent(out _), Is.True);
        }
    }
}
