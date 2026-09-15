using AutoEra.Machines;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineControlFlowEditModeTests
    {
        [Test]
        public void PauseReasons_AccumulateAndParentCancellationWaitsForSafePoint()
        {
            var ids = new PersistentIdAllocator(); var tasks = new MachineTaskQueue(ids);
            tasks.SetPaused(true, MachineWaitReason.Power); tasks.SetPaused(true, MachineWaitReason.Connection);
            tasks.Submit("T", WorkPriority.Normal, out var task);
            tasks.SetPaused(false, MachineWaitReason.Power); Assert.That(tasks.StartNext(), Is.Null);
            tasks.SetPaused(false, MachineWaitReason.Connection); Assert.That(tasks.StartNext(), Is.SameAs(task));
            var queue = new EffectorBehaviorQueue<int>(ids, tasks);
            queue.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, 1, out var request);
            int ends = 0; queue.Ended += _ => ends++;
            tasks.Cancel(task.Id); Assert.That(task.State, Is.EqualTo(MachineTaskState.Cancelling));
            Assert.That(request.Outcome.HasValue, Is.False);
            queue.ReachSafePoint(); queue.ReachSafePoint();
            Assert.That(ends, Is.EqualTo(1)); Assert.That(task.State, Is.EqualTo(MachineTaskState.Cancelled));
            Assert.That(queue.TryDetach(), Is.True);
        }

        [Test]
        public void HardwareIntent_WaitsCancelsWithoutRestartAndRechecksCapacity()
        {
            var ids = new PersistentIdAllocator();
            var roster = new MachineRoster(ids, new PersistentObjectRegistry(ids));
            var machine = roster.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 2, 30, true, true, 100));
            var component = roster.CreateComponent(new ComponentDefinition(2, HardwareKind.Sensor, 1, 0, 0, 0, false));
            roster.Deploy(machine.Id); machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true);
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running); machine.UpdateBehaviorActivity(true);
            using (var operation = new MachineHardwareOperation(roster))
            {
                Assert.That(operation.Begin(machine.Id, ManagementOrigin.Hub, false, HardwareKind.Sensor, 0, component.Id), Is.True);
                Assert.That(operation.Result, Is.EqualTo(MachineManagementResult.InvalidOrigin));
                Assert.That(machine.RequestedRunState, Is.EqualTo(MachineRunState.Running));
                operation.Begin(machine.Id, ManagementOrigin.Field, false, HardwareKind.Sensor, 0, component.Id);
                ulong version = operation.RequestVersion;
                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Waiting));
                Assert.That(operation.Cancel(version - 1), Is.False);
                Assert.That(operation.Cancel(version), Is.True);
                machine.UpdateBehaviorActivity(false);
                Assert.That(component.OwnerId.IsValid, Is.False);
                Assert.That(machine.RequestedRunState, Is.EqualTo(MachineRunState.Stopped));
                operation.Begin(machine.Id, ManagementOrigin.Field, false, HardwareKind.Sensor, 0, component.Id);
                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Completed));
                Assert.That(component.OwnerId, Is.EqualTo(machine.Id));
            }
            var core = roster.CreateComponent(new ComponentDefinition(3, HardwareKind.Core, 1, 0, 50, 40, false));
            roster.Install(machine.Id, ManagementOrigin.Field, core.Id, 0);
            machine.UpdateComputeUsage(10, 20);
            Assert.That(roster.Remove(machine.Id, ManagementOrigin.Field, HardwareKind.Core, 0), Is.EqualTo(MachineManagementResult.ComputeInUse));
            machine.UpdateComputeUsage(0, 20);
            Assert.That(roster.Remove(machine.Id, ManagementOrigin.Field, HardwareKind.Core, 0), Is.EqualTo(MachineManagementResult.LogicCapacityInUse));
            machine.UpdateComputeUsage(0, 0);
            Assert.That(roster.Remove(machine.Id, ManagementOrigin.Field, HardwareKind.Core, 0), Is.EqualTo(MachineManagementResult.Completed));
        }
    }
}
