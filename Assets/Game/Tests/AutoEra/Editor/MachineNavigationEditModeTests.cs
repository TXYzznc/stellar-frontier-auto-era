using System;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineNavigationEditModeTests
    {
        private sealed class Driver : IMachineNavigationDriver
        {
            public Vector3 Position { get; set; }
            public float Yaw { get; set; }
            public float Speed { get; set; }
            public bool PathValid { get; set; } = true;
            public bool PlanSucceeds = true;
            public int Plans, Stops;
            public bool Moving;
            public bool TryPlan(MachineNavigationTarget target, MachineInstance machine, out Vector3 destination)
            { Plans++; destination = target.Candidates[0]; return PlanSucceeds && target.Allows(machine, destination); }
            public void Resume() { Moving = true; }
            public void Stop() { Stops++; Moving = false; Speed = 0; }
            public void Align(float yaw, float degreesPerSecond, float deltaSeconds)
                => Yaw = Mathf.MoveTowardsAngle(Yaw, yaw, degreesPerSecond * deltaSeconds);
        }
        private sealed class Fixture : IDisposable
        {
            public readonly AutoEraWorldSession Session = new AutoEraWorldSessionFactory().Create(0);
            public readonly InitialRegion Region;
            public readonly MachineInstance Machine;
            public readonly MachineExecutionContext Context;
            public readonly MachineNavigation Navigation;
            public readonly Driver Driver = new Driver();
            public readonly MachineTaskRecord Task;
            public Fixture(int compute = 20, bool mobile = true)
            {
                Region = new InitialRegion(Session, new Rect(-50, -50, 100, 100));
                Machine = Session.Machines.Create(new MachineDefinition(1, "Fixture", 1, 1, 1, 1, 30, mobile, true, 100));
                if (compute > 0)
                {
                    var core = Session.Machines.CreateComponent(new ComponentDefinition(2, HardwareKind.Core, 1, 0, compute, 100, false));
                    Session.Machines.Install(Machine.Id, ManagementOrigin.Library, core.Id, 0);
                }
                Region.DeployMachine(Machine.Id, Vector2.zero, Vector2.one, out _);
                Machine.Activate(ManagementOrigin.Field); Machine.UpdateEnvironment(true, true);
                Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                Context = new MachineExecutionContext(Machine, Session.IdAllocator);
                Context.Tasks.Submit("move", WorkPriority.Normal, out Task); Context.Tasks.StartNext();
                Navigation = new MachineNavigation(Context, Driver, new MachineNavigationSettings());
            }
            public NavigationAdmission Start(float? yaw = null) => Navigation.Start(Task.Id, new MachineNavigationTarget(Region, new Vector3(8, 0, 0), yaw), 0);
            public void Dispose() { Navigation.Dispose(); Context.Tasks.CloseChain(Task.Id); Context.Dispose(); Region.Dispose(); Session.Dispose(); }
        }

        [Test]
        public void Immobile_IsRejectedWithoutComputeOrActivity()
        {
            using (var f = new Fixture(20, false))
            { Assert.That(f.Start(), Is.EqualTo(NavigationAdmission.Immobile)); Assert.That(f.Context.Compute.Used, Is.Zero); Assert.That(f.Machine.HasActiveBehavior, Is.False); }
        }
        [Test]
        public void Arrival_RequiresRealPositionAndFacing_ThenReleasesCompute()
        {
            using (var f = new Fixture())
            {
                f.Start(90); f.Navigation.Tick(0, 0);
                Assert.That(f.Context.Compute.Used, Is.EqualTo(5)); Assert.That(f.Navigation.Outcome, Is.Null);
                Assert.That(f.Machine.HasActiveBehavior, Is.True);
                f.Driver.Position = new Vector3(8, 0, 0); f.Navigation.Tick(1, 0.1f);
                Assert.That(f.Navigation.State, Is.EqualTo(MachineNavigationState.Aligning));
                f.Navigation.Tick(2, 1);
                Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed));
                Assert.That(f.Context.Compute.Used, Is.Zero); Assert.That(f.Machine.HasActiveBehavior, Is.False);
                f.Region.TryGet(f.Machine.Id, out var body); Assert.That(body.Position.x, Is.EqualTo(8)); Assert.That(body.Yaw, Is.EqualTo(90));
            }
        }
        [Test]
        public void PlanningWait_DeduplicatesAndDoesNotTimeoutWithoutBudget()
        {
            using (var f = new Fixture())
            {
                f.Context.Compute.Submit(20, ComputeClass.Safety, WorkPriority.Urgent, ComputeMergeKind.None, default, 0, 0, false, out var other);
                f.Start(); for (int i = 0; i <= 40; i++) f.Navigation.Tick(i, 1);
                Assert.That(f.Context.Compute.WaitingCount, Is.EqualTo(1)); Assert.That(f.Driver.Plans, Is.Zero);
                Assert.That(f.Navigation.Outcome, Is.Null); Assert.That(f.Driver.Moving, Is.False);
                f.Context.Compute.Release(other.Id); f.Navigation.Tick(41, 1);
                Assert.That(f.Driver.Plans, Is.EqualTo(1)); Assert.That(f.Context.Compute.Used, Is.EqualTo(5));
            }
        }
        [Test]
        public void NoCore_RemainsSafeAndAllocatesNoRequests()
        {
            using (var f = new Fixture(0))
            { f.Start(); f.Navigation.Tick(40, 40); Assert.That(f.Driver.Moving, Is.False); Assert.That(f.Driver.Plans, Is.Zero); Assert.That(f.Context.Compute.WaitingCount, Is.Zero); }
        }
        [Test]
        public void BlockedPath_WarnsAtTenAndFailsAtThirty()
        {
            using (var f = new Fixture())
            {
                f.Driver.PlanSucceeds = false; f.Start(); f.Navigation.Tick(0, 0);
                f.Navigation.Tick(9, 9); Assert.That(f.Navigation.PathWarning, Is.False);
                f.Navigation.Tick(10, 1); Assert.That(f.Navigation.PathWarning, Is.True);
                f.Navigation.Tick(30, 20); Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Failed));
                Assert.That(f.Context.Compute.Used, Is.Zero);
            }
        }
        [Test]
        public void NoDisplacement_ReplansAfterThreeAndDoesNotResetFailureClock()
        {
            using (var f = new Fixture())
            {
                f.Start(); f.Navigation.Tick(0, 0); f.Navigation.Tick(2.9, 2.9f); Assert.That(f.Driver.Plans, Is.EqualTo(1));
                for (int i = 3; i <= 30; i++) f.Navigation.Tick(i, 0.1f);
                Assert.That(f.Driver.Plans, Is.GreaterThan(1)); Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Failed));
            }
        }
        [Test]
        public void PowerLoss_StopsSynchronouslyAndRestoresThroughPlanning()
        {
            using (var f = new Fixture())
            {
                f.Start(); f.Navigation.Tick(0, 0); f.Machine.UpdateEnvironment(false, true);
                Assert.That(f.Driver.Moving, Is.False); Assert.That(f.Context.Compute.Used, Is.Zero);
                f.Navigation.Tick(60, 60); Assert.That(f.Navigation.Outcome, Is.Null);
                f.Machine.UpdateEnvironment(true, true); f.Navigation.Tick(61, 1);
                Assert.That(f.Driver.Plans, Is.EqualTo(2)); Assert.That(f.Driver.Moving, Is.True);
            }
        }
        [Test]
        public void ReplanWithoutSpareCompute_StopsAndOwnsOnlyOneWaitingRequest()
        {
            using (var f = new Fixture(10))
            {
                f.Start(); f.Navigation.Tick(0, 0); f.Driver.PathValid = false; f.Navigation.Tick(1, 1);
                for (int i = 3; i < 8; i++) f.Navigation.Tick(i, 1);
                Assert.That(f.Driver.Moving, Is.False); Assert.That(f.Driver.Plans, Is.EqualTo(1));
                Assert.That(f.Context.Compute.Used, Is.EqualTo(5)); Assert.That(f.Context.Compute.WaitingCount, Is.EqualTo(1));
            }
        }
        [Test]
        public void Cancellation_IsImmediateAndExactlyOnceThroughDispose()
        {
            using (var f = new Fixture())
            {
                int ended = 0; f.Navigation.Ended += _ => ended++;
                f.Start(); f.Navigation.Tick(0, 0); f.Context.Tasks.Cancel(f.Task.Id); f.Navigation.Cancel(); f.Navigation.Dispose();
                Assert.That(ended, Is.EqualTo(1)); Assert.That(f.Driver.Moving, Is.False);
                Assert.That(f.Context.Compute.Used + f.Context.Compute.WaitingCount, Is.Zero);
                Assert.That(f.Task.State, Is.EqualTo(MachineTaskState.Cancelled));
            }
        }
        [Test]
        public void RegionExit_PreservesRosterIdentityAndEndsOnce()
        {
            using (var f = new Fixture())
            {
                int ended = 0; f.Navigation.Ended += _ => ended++;
                f.Start(); f.Navigation.Tick(0, 0); f.Region.Remove(f.Machine.Id); f.Navigation.Tick(1, 1);
                Assert.That(ended, Is.EqualTo(1)); Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.TargetInvalid));
                Assert.That(f.Session.ObjectRegistry.TryResolve(f.Machine.Id, PersistentObjectKind.Machine, out var same), Is.EqualTo(PersistentRegistryResult.Success));
                Assert.That(same, Is.SameAs(f.Machine));
            }
        }
        [Test]
        public void WorkWait_DoesNotPlanOrFail_ThenHandoffsAndRetainsReservationAfterArrival()
        {
            using (var f = new Fixture())
            {
                var target = f.Region.Register(PersistentObjectKind.ResourcePoint, "mine", new Vector2(8, 0), new Vector2(4, 4), 0, false);
                var other = f.Region.Register(PersistentObjectKind.Machine, "other", new Vector2(-8, 0), Vector2.one);
                using (var queue = new RegionWorkQueue(f.Region, target.Id, new Rect(6, -2, 4, 4)))
                {
                    queue.Request(other.Id, queue.WorkArea.center);
                    var goal = new MachineNavigationTarget(f.Region, target.Id, queue, queue.WorkArea, 0, Vector2.one,
                        (machine, p) => true);
                    f.Navigation.Start(f.Task.Id, goal, 0); f.Navigation.Tick(0, 0); f.Navigation.Tick(31, 31);
                    Assert.That(f.Driver.Plans, Is.Zero); Assert.That(f.Navigation.WorkWaitPrompt, Is.True); Assert.That(f.Navigation.Outcome, Is.Null);
                    queue.Release(other.Id); f.Navigation.Tick(32, 1); Assert.That(f.Driver.Plans, Is.EqualTo(1));
                    f.Driver.Position = goal.Candidates[0]; f.Navigation.Tick(33, 1);
                    Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed)); Assert.That(queue.Owner, Is.EqualTo(f.Machine.Id));
                    f.Navigation.ReleaseWorkReservation(); Assert.That(queue.Owner.IsValid, Is.False);
                }
            }
        }
        [Test]
        public void ExplicitWaitingPosition_IsVisitedOnce_AndIsNotWorkArrival()
        {
            using (var f = new Fixture())
            {
                var target = f.Region.Register(PersistentObjectKind.ResourcePoint, "work", new Vector2(8, 0), Vector2.one, 0, false);
                var other = f.Region.Register(PersistentObjectKind.Machine, "owner", new Vector2(-8, 0), Vector2.one);
                using (var queue = new RegionWorkQueue(f.Region, target.Id, new Rect(6, -2, 4, 4)))
                {
                    queue.Request(other.Id, queue.WorkArea.center);
                    var goal = new MachineNavigationTarget(f.Region, target.Id, queue, queue.WorkArea, 0, Vector2.one,
                        (m, p) => true, waitingPosition: new Vector3(-3, 0, 0));
                    f.Navigation.Start(f.Task.Id, goal, 0); f.Navigation.Tick(0, 0);
                    f.Driver.Position = new Vector3(-3, 0, 0); f.Navigation.Tick(1, 1); f.Navigation.Tick(35, 34);
                    Assert.That(f.Driver.Plans, Is.EqualTo(1)); Assert.That(f.Navigation.Outcome, Is.Null);
                    Assert.That(f.Context.Compute.Used, Is.Zero); Assert.That(f.Navigation.WorkWaitPrompt, Is.True);
                    queue.Release(other.Id); f.Navigation.Tick(36, 1);
                    Assert.That(f.Driver.Plans, Is.EqualTo(2)); Assert.That(f.Navigation.State, Is.EqualTo(MachineNavigationState.Moving));
                }
            }
        }
        [Test]
        public void InvalidWorkTarget_CleansWaitingAndRejectsOversizeOrEnvelope()
        {
            using (var f = new Fixture())
            {
                var target = f.Region.Register(PersistentObjectKind.ResourcePoint, "water target", new Vector2(8, 0), Vector2.one, 0, false);
                using (var queue = new RegionWorkQueue(f.Region, target.Id, new Rect(6, -2, 4, 4)))
                {
                    var goal = new MachineNavigationTarget(f.Region, target.Id, queue, queue.WorkArea, 0, Vector2.one, (m, p) => false);
                    Assert.That(goal.Allows(f.Machine, new Vector3(8, 0, 0)), Is.False);
                    f.Navigation.Start(f.Task.Id, goal, 0); f.Navigation.Tick(0, 0); f.Region.Remove(target.Id);
                    Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.TargetInvalid));
                    Assert.That(queue.Owner.IsValid, Is.False); Assert.That(f.Context.Compute.Used, Is.Zero);
                }
            }
        }
        [Test]
        public void ManagementSafeStop_DrainsNavigationActivityRatherThanBlockingHardwareForever()
        {
            using (var f = new Fixture())
            {
                f.Start(); f.Navigation.Tick(0, 0);
                f.Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Stopped);
                Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Partial));
                Assert.That(f.Machine.HasActiveBehavior, Is.False); Assert.That(f.Context.Compute.Used, Is.Zero);
            }
        }
        [Test]
        public void PreemptionAndDuplicateBinding_AreBounded()
        {
            using (var f = new Fixture())
            {
                Assert.Throws<InvalidOperationException>(() => new MachineNavigation(f.Context, new Driver(), new MachineNavigationSettings()));
                f.Start(); Assert.That(f.Start(), Is.EqualTo(NavigationAdmission.Busy)); f.Navigation.Tick(0, 0); f.Navigation.Preempt();
                Assert.That(f.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Preempted)); Assert.That(f.Context.Compute.Used, Is.Zero);
            }
        }
    }
}
