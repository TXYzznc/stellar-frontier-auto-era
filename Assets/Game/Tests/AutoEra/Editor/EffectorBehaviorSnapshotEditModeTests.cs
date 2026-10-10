using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class EffectorBehaviorSnapshotEditModeTests
    {
        private sealed class Data
        {
            public MachineTaskQueueSnapshot Tasks;
            public EffectorBehaviorSnapshot<EffectorBehaviorParameters> Effector;
        }
        private static EffectorBehaviorParameters Parameters(double count = 2)
        {
            var value = new EffectorBehaviorParameters("Transfer") { TransferMode = "load", Target = new PersistentObjectReference(new PersistentId(777), PersistentObjectKind.ResourcePoint) };
            value.Numbers.Add("count", count); value.Objects.Add("source", value.Target); return value;
        }
        private static Data Json(MachineTaskQueue tasks, EffectorBehaviorQueue<EffectorBehaviorParameters> queue)
        {
            Assert.That(queue.TryCapturePersistent(EffectorParameterSnapshot.Copy, out var snapshot), Is.True);
            string json = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(10, 1000, 1, "Fixture",
                new[] { new WorldSnapshotSection("behavior", 1, new Data { Tasks = tasks.Capture(), Effector = snapshot }) }));
            Assert.That(WorldSnapshotCodec.TryRead(json, new Dictionary<string, int> { { "behavior", 1 } }, out var file, out var reason), Is.True, reason);
            Assert.That(file.TryReadSection<Data>("behavior", out var data, out reason), Is.True, reason); return data;
        }
        private static EffectorBehaviorQueue<EffectorBehaviorParameters> Restore(Data data, out MachineTaskQueue tasks, out PersistentIdAllocator ids)
        {
            ids = new PersistentIdAllocator(); tasks = new MachineTaskQueue(ids); tasks.Restore(data.Tasks);
            var queue = new EffectorBehaviorQueue<EffectorBehaviorParameters>(ids, tasks);
            Assert.That(queue.RestorePersistent(data.Effector, EffectorParameterSnapshot.Valid, EffectorParameterSnapshot.Copy), Is.True); return queue;
        }
        [Test] public void InFlightAndWaiting_ResumeSameIds_WithoutDuplicateActivityOrStart()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids); tasks.Submit("load", WorkPriority.Normal, out var task); tasks.StartNext();
            var source = new EffectorBehaviorQueue<EffectorBehaviorParameters>(ids, tasks);
            source.Submit(task.Id, new PersistentId(100), new PersistentId(101), Parameters().Target, WorkPriority.Normal, InterruptionRule.SafePoint, Parameters(), out var current);
            source.Submit(task.Id, new PersistentId(100), new PersistentId(102), Parameters().Target, WorkPriority.Normal, InterruptionRule.SafePoint, Parameters(3), out var next);
            tasks.CloseChain(task.Id);
            var data = Json(tasks, source); var restoredTasks = new MachineTaskQueue(new PersistentIdAllocator()); restoredTasks.Restore(data.Tasks);
            var restored = new EffectorBehaviorQueue<EffectorBehaviorParameters>(new PersistentIdAllocator(), restoredTasks); int started = 0, ended = 0;
            restored.Started += _ => started++; restoredTasks.Ended += _ => ended++;
            Assert.That(restored.RestorePersistent(data.Effector, EffectorParameterSnapshot.Valid, EffectorParameterSnapshot.Copy), Is.True);
            Assert.That(started, Is.Zero); Assert.That(restored.Current.Id, Is.EqualTo(current.Id)); Assert.That(restored.Current.Target.Id.Value, Is.EqualTo(777), "A missing target remains the exact original reference.");
            restored.Finish(BehaviorOutcome.Completed); Assert.That(restored.Current.Id, Is.EqualTo(next.Id)); Assert.That(started, Is.EqualTo(1)); Assert.That(ended, Is.Zero);
            restored.Finish(BehaviorOutcome.Completed); Assert.That(ended, Is.EqualTo(1)); Assert.That(restoredTasks.TryGet(task.Id, out _), Is.False);
            Assert.That(source.Current.Id, Is.EqualTo(current.Id));
        }
        [Test] public void CancellationAndDisabledExecution_ArePreservedUntilSafePoint()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids); tasks.Submit("cut", WorkPriority.Normal, out var task); tasks.StartNext();
            var source = new EffectorBehaviorQueue<EffectorBehaviorParameters>(ids, tasks);
            source.Submit(task.Id, default, default, Parameters().Target, WorkPriority.Normal, InterruptionRule.SafePoint, Parameters(), out var request);
            source.Cancel(request.Id); source.SetExecutionPermission(false, false);
            var restored = Restore(Json(tasks, source), out _, out _);
            Assert.That(restored.CanExecuteCurrent, Is.False); Assert.That(restored.Current.Id, Is.EqualTo(request.Id));
            int ended = 0; restored.Ended += r => { ended++; Assert.That(r.Outcome, Is.EqualTo(BehaviorOutcome.Cancelled)); };
            restored.ReachSafePoint(); restored.ReachSafePoint(); Assert.That(ended, Is.EqualTo(1)); Assert.That(restored.Current, Is.Null);
        }
        [Test] public void WaitingPriorityAndFifo_RestoreWithoutMutableParameterAliases()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids); tasks.Submit("parallel", WorkPriority.Normal, out var task); tasks.StartNext();
            var source = new EffectorBehaviorQueue<EffectorBehaviorParameters>(ids, tasks); source.SetDispatchEnabled(false);
            source.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, Parameters(1), out var first);
            source.Submit(task.Id, default, default, default, WorkPriority.High, InterruptionRule.SafePoint, Parameters(2), out var high);
            source.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, Parameters(3), out var second);
            var data = Json(tasks, source); var restored = Restore(data, out _, out var restoredIds);
            data.Effector.Waiting[0].Parameters.Numbers["count"] = 99;
            restored.SetDispatchEnabled(true); Assert.That(restored.Current.Id, Is.EqualTo(high.Id)); restored.Finish(BehaviorOutcome.Completed);
            Assert.That(restored.Current.Id, Is.EqualTo(first.Id)); Assert.That(restored.Current.Parameters.Numbers["count"], Is.EqualTo(1)); restored.Finish(BehaviorOutcome.Completed);
            Assert.That(restored.Current.Id, Is.EqualTo(second.Id)); Assert.That(restoredIds.NextId.Value, Is.GreaterThan(second.Id.Value));
        }
        [Test] public void DuplicateBehaviorOrInsufficientActivity_RejectsBeforeMutation()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids); tasks.Submit("work", WorkPriority.Normal, out var task); tasks.StartNext();
            var source = new EffectorBehaviorQueue<EffectorBehaviorParameters>(ids, tasks);
            source.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, Parameters(), out _);
            var data = Json(tasks, source); var restoredTasks = new MachineTaskQueue(new PersistentIdAllocator()); restoredTasks.Restore(data.Tasks);
            var candidateIds = new PersistentIdAllocator(); var candidate = new EffectorBehaviorQueue<EffectorBehaviorParameters>(candidateIds, restoredTasks);
            data.Effector.Waiting = new[] { data.Effector.Current };
            Assert.That(candidate.RestorePersistent(data.Effector, EffectorParameterSnapshot.Valid, EffectorParameterSnapshot.Copy), Is.False);
            Assert.That(candidate.Current, Is.Null); Assert.That(candidate.WaitingCount, Is.Zero); Assert.That(candidateIds.NextId.Value, Is.EqualTo(1));
            data.Effector.Waiting = new[] { new BehaviorRequestSnapshot<EffectorBehaviorParameters> {
                Id = new PersistentId(888), TaskId = task.Id, Priority = WorkPriority.Normal, Interruption = InterruptionRule.SafePoint, Parameters = Parameters() } };
            Assert.That(candidate.RestorePersistent(data.Effector, EffectorParameterSnapshot.Valid, EffectorParameterSnapshot.Copy), Is.False);
            Assert.That(candidate.Current, Is.Null);
        }
        [Test] public void UnknownActionAndNonFiniteParameters_AreRejected()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids); tasks.Submit("work", WorkPriority.Normal, out var task); tasks.StartNext();
            var source = new EffectorBehaviorQueue<EffectorBehaviorParameters>(ids, tasks);
            source.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, Parameters(), out _);
            var data = Json(tasks, source); var restoredTasks = new MachineTaskQueue(new PersistentIdAllocator()); restoredTasks.Restore(data.Tasks);
            var candidate = new EffectorBehaviorQueue<EffectorBehaviorParameters>(new PersistentIdAllocator(), restoredTasks);
            data.Effector.Current.Parameters.Action = "999";
            Assert.That(candidate.RestorePersistent(data.Effector, EffectorParameterSnapshot.Valid, EffectorParameterSnapshot.Copy), Is.False);
            data.Effector.Current.Parameters.Action = "Transfer"; data.Effector.Current.Parameters.Numbers["count"] = double.NaN;
            Assert.That(candidate.RestorePersistent(data.Effector, EffectorParameterSnapshot.Valid, EffectorParameterSnapshot.Copy), Is.False);
        }
    }
}
