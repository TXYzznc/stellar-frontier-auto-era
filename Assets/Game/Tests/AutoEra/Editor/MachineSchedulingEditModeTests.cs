using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineSchedulingEditModeTests
    {
        private readonly struct FixtureParameters { public readonly int Value; public FixtureParameters(int value) { Value = value; } }

        [Test]
        public void Tasks_RejectFullWithoutEvictionAndRunSeveralContexts()
        {
            var queue = new MachineTaskQueue(new PersistentIdAllocator());
            queue.Submit("first", WorkPriority.Normal, out var first);
            queue.Submit("second", WorkPriority.Normal, out var second);
            queue.Submit("urgent", WorkPriority.Urgent, out var urgent);
            Assert.That(queue.StartNext(), Is.SameAs(urgent));
            Assert.That(queue.StartNext(), Is.SameAs(first));
            Assert.That(queue.StartNext(), Is.SameAs(second));
            for (int i = 0; i < 32; i++) Assert.That(queue.Submit("wait", WorkPriority.Low, out _), Is.EqualTo(QueueAdmission.Accepted));
            Assert.That(queue.Submit("no eviction", WorkPriority.Urgent, out _), Is.EqualTo(QueueAdmission.Full));
            int ended = 0; queue.Ended += _ => ended++;
            queue.AddActivity(first.Id); queue.Cancel(first.Id);
            Assert.That(first.State, Is.EqualTo(MachineTaskState.Cancelling));
            queue.EndActivity(first.Id, false); queue.Cancel(first.Id);
            Assert.That(first.State, Is.EqualTo(MachineTaskState.Cancelled));
            Assert.That(ended, Is.EqualTo(1));
        }

        [Test]
        public void Effectors_IndependentAndSafePointPreemptionPublishesOnce()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids);
            tasks.Submit("parallel", WorkPriority.Normal, out var task); tasks.StartNext();
            var first = new EffectorBehaviorQueue<FixtureParameters>(ids, tasks);
            var second = new EffectorBehaviorQueue<FixtureParameters>(ids, tasks);
            var results = new List<BehaviorOutcome>(); first.Ended += request => { results.Add(request.Outcome.Value); request.MarkFailureHandled(); };
            first.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, new FixtureParameters(1), out var low);
            second.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.CannotInterrupt, new FixtureParameters(2), out _);
            first.Submit(task.Id, default, default, default, WorkPriority.High, InterruptionRule.Immediate, new FixtureParameters(3), out var high);
            Assert.That(first.Current, Is.SameAs(low)); Assert.That(second.Current, Is.Not.Null);
            first.ReachSafePoint(); Assert.That(first.Current, Is.SameAs(high));
            first.Cancel(high.Id); first.Cancel(high.Id);
            Assert.That(results, Is.EqualTo(new[] { BehaviorOutcome.Preempted, BehaviorOutcome.Cancelled }));
            tasks.CloseChain(task.Id); Assert.That(task.State, Is.EqualTo(MachineTaskState.Running));
            second.Finish(BehaviorOutcome.Completed);
            Assert.That(task.State, Is.EqualTo(MachineTaskState.Completed));
        }

        [Test]
        public void Compute_AllocatesWholeRequestsMergesOnlyExplicitStreamsAndNeverOversells()
        {
            var pool = new MachineComputePool(new PersistentIdAllocator(), 50, 40);
            Assert.That(pool.TryApplyLogicCost(41), Is.False);
            Assert.That(pool.TryApplyLogicCost(31), Is.True);
            Assert.That(pool.Submit(40, ComputeClass.Sampling, WorkPriority.Normal, ComputeMergeKind.None, default, 0, 0, true, out var busy), Is.EqualTo(ComputeAdmission.Started));
            Assert.That(pool.Submit(20, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.ContinuousValue, new PersistentId(500), 1, 1, false, out var waiting), Is.EqualTo(ComputeAdmission.Waiting));
            Assert.That(pool.Submit(25, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.ContinuousValue, new PersistentId(500), 2, 2, false, out var merged), Is.EqualTo(ComputeAdmission.Merged));
            Assert.That(merged, Is.SameAs(waiting)); Assert.That(pool.WaitingCount, Is.EqualTo(1));
            Assert.That(pool.YieldAtBoundary(busy.Id), Is.True);
            Assert.That(waiting.State, Is.EqualTo(ComputeState.Running)); Assert.That(pool.Used, Is.EqualTo(25));
            Assert.That(pool.Release(waiting.Id), Is.True); Assert.That(pool.Release(waiting.Id), Is.False);
            Assert.That(pool.Used, Is.EqualTo(40));
            Assert.That(pool.Submit(51, ComputeClass.Safety, WorkPriority.Urgent, ComputeMergeKind.None, default, 0, 3, false, out _), Is.EqualTo(ComputeAdmission.ExceedsCapacity));
            Assert.That(pool.Used, Is.LessThanOrEqualTo(pool.Capacity));
        }
    }
}
