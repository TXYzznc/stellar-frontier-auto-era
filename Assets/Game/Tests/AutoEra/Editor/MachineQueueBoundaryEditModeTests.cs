using System.Collections.Generic;
using System.Linq;
using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineQueueBoundaryEditModeTests
    {
        [Test]
        public void EffectorFull_RejectsUrgentWithoutEvictionAndCancelsExactlyOnce()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids);
            tasks.Submit("T", WorkPriority.Normal, out var task); tasks.StartNext();
            var queue = new EffectorBehaviorQueue<int>(ids, tasks);
            queue.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.CannotInterrupt, 0, out var active);
            int ended = 0; queue.Ended += _ => ended++;
            for (int i = 0; i < 16; i++)
                Assert.That(queue.Submit(task.Id, default, default, default, WorkPriority.Low, InterruptionRule.SafePoint, i, out _), Is.EqualTo(QueueAdmission.Accepted));
            Assert.That(queue.Submit(task.Id, default, default, default, WorkPriority.Urgent, InterruptionRule.Immediate, 99, out _), Is.EqualTo(QueueAdmission.Full));
            Assert.That(queue.Current, Is.SameAs(active)); Assert.That(queue.WaitingCount, Is.EqualTo(16));
            tasks.Cancel(task.Id); Assert.That(ended, Is.EqualTo(16)); Assert.That(queue.WaitingCount, Is.Zero);
            queue.ReachSafePoint(); Assert.That(queue.Current, Is.SameAs(active));
            queue.Finish(BehaviorOutcome.Completed); queue.Finish(BehaviorOutcome.Completed);
            Assert.That(ended, Is.EqualTo(17)); Assert.That(task.State, Is.EqualTo(MachineTaskState.Cancelled));
        }

        [Test]
        public void ComputeFull_ProtectsInFlightAndPreservesEveryUnmergedRequest()
        {
            var pool = new MachineComputePool(new PersistentIdAllocator(), 1, 40);
            pool.Submit(1, ComputeClass.Sampling, WorkPriority.Low, ComputeMergeKind.None, default, 0, 0, false, out var current);
            var pending = new List<ComputeRequest>();
            for (int i = 0; i < 64; i++)
            {
                Assert.That(pool.Submit(1, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None, default, 0, i, false, out var request), Is.EqualTo(ComputeAdmission.Waiting));
                pending.Add(request);
            }
            Assert.That(pool.Submit(1, ComputeClass.Safety, WorkPriority.Urgent, ComputeMergeKind.None, default, 0, 65, false, out _), Is.EqualTo(ComputeAdmission.Full));
            Assert.That(pool.YieldAtBoundary(current.Id), Is.False);
            pool.Release(current.Id);
            foreach (var request in pending)
            {
                Assert.That(request.State, Is.EqualTo(ComputeState.Running)); Assert.That(pool.Used, Is.EqualTo(1));
                pool.Release(request.Id);
            }
            Assert.That(pool.Used, Is.Zero); Assert.That(pool.WaitingCount, Is.Zero);
        }

        [Test]
        public void Replay_IsDeterministicAndTaskHistoryIsBounded()
        {
            Assert.That(Replay(), Is.EqualTo(Replay()));
            var tasks = new MachineTaskQueue(new PersistentIdAllocator());
            for (int i = 0; i < 150; i++)
            { tasks.Submit("T", WorkPriority.Normal, out var task); tasks.StartNext(); tasks.CloseChain(task.Id); }
            Assert.That(tasks.History.Count(), Is.EqualTo(100));
            Assert.That(tasks.History.First().Id.Value, Is.EqualTo(51));
        }

        private static string Replay()
        {
            var pool = new MachineComputePool(new PersistentIdAllocator(), 10, 40);
            var order = new List<ulong>();
            pool.Submit(10, ComputeClass.Sampling, WorkPriority.Low, ComputeMergeKind.None, default, 0, 0, false, out var first);
            pool.Submit(10, ComputeClass.Evaluation, WorkPriority.Normal, ComputeMergeKind.None, default, 0, 1, false, out _);
            pool.Submit(10, ComputeClass.Safety, WorkPriority.Urgent, ComputeMergeKind.None, default, 0, 2, false, out _);
            pool.Started += request => { order.Add(request.Id.Value); pool.Release(request.Id); };
            pool.Release(first.Id);
            Assert.That(order, Is.EqualTo(new ulong[] { 3, 2 }));
            Assert.That(pool.Used, Is.Zero);
            return string.Join(",", order);
        }
    }
}
