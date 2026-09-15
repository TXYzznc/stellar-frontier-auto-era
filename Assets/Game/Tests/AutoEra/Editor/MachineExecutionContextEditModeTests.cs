using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineExecutionContextEditModeTests
    {
        [Test]
        public void SafeStop_AllowsPhysicalTailAndOnlyThenCommitsHardwareRemoval()
        {
            var ids = new PersistentIdAllocator(); var roster = new MachineRoster(ids, new PersistentObjectRegistry(ids));
            var machine = roster.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 2, 30, true, true, 100));
            var tool = roster.CreateComponent(new ComponentDefinition(3, HardwareKind.Effector, 1, 0, 0, 0, true));
            roster.Install(machine.Id, ManagementOrigin.Library, tool.Id, 0); roster.Deploy(machine.Id);
            machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true);
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
            using (var context = new MachineExecutionContext(machine, ids))
            using (var operation = new MachineHardwareOperation(roster))
            {
                var queue = context.BindEffector<int>(tool);
                context.Tasks.Submit("T", WorkPriority.Normal, out var task); context.Tasks.StartNext();
                queue.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, 1, out var request);
                operation.Begin(machine.Id, ManagementOrigin.Field, true, HardwareKind.Effector, 0, default);
                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Waiting));
                Assert.That(queue.SafeStopRequested, Is.True); Assert.That(queue.CanExecuteCurrent, Is.True);
                queue.ReachSafePoint();
                Assert.That(request.Outcome, Is.EqualTo(BehaviorOutcome.Partial));
                Assert.That(operation.State, Is.EqualTo(HardwareOperationState.Completed));
                Assert.That(tool.OwnerId.IsValid, Is.False);
                context.Tasks.CloseChain(task.Id);
            }
        }

        [Test]
        public void DetachedHardware_InvalidatesQueuedTargetsAndCanRebindAfterReinstall()
        {
            var ids = new PersistentIdAllocator(); var roster = new MachineRoster(ids, new PersistentObjectRegistry(ids));
            var machine = roster.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 2, 30, true, true, 100));
            var tool = roster.CreateComponent(new ComponentDefinition(3, HardwareKind.Effector, 1, 0, 0, 0, true));
            roster.Install(machine.Id, ManagementOrigin.Library, tool.Id, 0);
            roster.Deploy(machine.Id); machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true);
            machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
            using (var context = new MachineExecutionContext(machine, ids))
            {
                var queue = context.BindEffector<int>(tool);
                machine.SetComponentEnabled(ManagementOrigin.Field, HardwareKind.Effector, 0, false);
                context.Tasks.Submit("T", WorkPriority.Normal, out var task); context.Tasks.StartNext();
                queue.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.Immediate, 1, out var request);
                Assert.That(queue.Current, Is.Null); Assert.That(queue.WaitingCount, Is.EqualTo(1));
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Stopped);
                Assert.That(roster.Remove(machine.Id, ManagementOrigin.Field, HardwareKind.Effector, 0), Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(request.Outcome, Is.EqualTo(BehaviorOutcome.TargetInvalid));
                context.Tasks.CloseChain(task.Id);
                Assert.That(task.State, Is.EqualTo(MachineTaskState.Failed));
                roster.Install(machine.Id, ManagementOrigin.Field, tool.Id, 0);
                Assert.That(context.BindEffector<int>(tool), Is.Not.SameAs(queue));
            }
        }

        [Test]
        public void StateProjection_PausesWithoutLosingRequestAndTracksReservations()
        {
            var ids = new PersistentIdAllocator(); var roster = new MachineRoster(ids, new PersistentObjectRegistry(ids));
            var machine = roster.Create(new MachineDefinition(1, "Fixture", 1, 2, 1, 2, 30, true, true, 100));
            var core = roster.CreateComponent(new ComponentDefinition(2, HardwareKind.Core, 1, 0, 50, 40, false));
            var tool = roster.CreateComponent(new ComponentDefinition(3, HardwareKind.Effector, 1, 0, 0, 0, true));
            roster.Install(machine.Id, ManagementOrigin.Library, core.Id, 0);
            roster.Install(machine.Id, ManagementOrigin.Library, tool.Id, 0);
            roster.Deploy(machine.Id); machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true);
            using (var context = new MachineExecutionContext(machine, ids))
            {
                var queue = context.BindEffector<int>(tool);
                context.Tasks.Submit("fixture", WorkPriority.Normal, out var task);
                Assert.That(context.Tasks.StartNext(), Is.Null);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                context.Tasks.StartNext();
                queue.Submit(task.Id, default, default, default, WorkPriority.Normal, InterruptionRule.SafePoint, 1, out var request);
                Assert.That(machine.HasActiveBehavior, Is.True); Assert.That(queue.CanExecuteCurrent, Is.True);
                context.Compute.Submit(10, ComputeClass.Sampling, WorkPriority.Normal, ComputeMergeKind.None, default, 0, 0, true, out var lease);
                Assert.That(machine.ReservedCompute, Is.EqualTo(10));
                machine.UpdateEnvironment(false, false);
                Assert.That(queue.CanExecuteCurrent, Is.False); Assert.That(queue.Current, Is.SameAs(request));
                Assert.That(task.State, Is.EqualTo(MachineTaskState.Paused));
                machine.UpdateEnvironment(true, false);
                Assert.That(queue.CanExecuteCurrent, Is.True, "Local tasks do not require a simulated remote signal.");
                Assert.That(task.State, Is.EqualTo(MachineTaskState.Running));
                queue.Finish(BehaviorOutcome.Completed); context.Tasks.CloseChain(task.Id);
                Assert.That(machine.HasActiveBehavior, Is.False);
                context.Compute.Release(lease.Id); Assert.That(machine.ReservedCompute, Is.Zero);
            }
        }
    }
}
