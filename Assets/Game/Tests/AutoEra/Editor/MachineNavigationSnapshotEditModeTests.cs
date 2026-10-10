using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineNavigationSnapshotEditModeTests
    {
        private sealed class Driver : IMachineNavigationDriver
        {
            public Vector3 Position { get; set; }
            public float Yaw { get; set; }
            public float Speed => 0;
            public bool PathValid => true;
            internal bool PlanSucceeds = true;
            public bool TryPlan(MachineNavigationTarget target, MachineInstance machine, out Vector3 destination)
            { destination = target.Candidates[0]; return PlanSucceeds && target.Allows(machine, destination); }
            public void Resume() { }
            public void Stop() { }
            public void Align(float yaw, float speed, float seconds) { Yaw = Mathf.MoveTowardsAngle(Yaw, yaw, speed * seconds); }
        }
        private sealed class Fixture : IDisposable
        {
            internal readonly AutoEraWorldSession World = new AutoEraWorldSessionFactory().Create(0);
            internal readonly InitialRegion Region;
            internal readonly MachineInstance Machine;
            internal readonly MachineExecutionContext Context;
            internal readonly Driver Driver = new Driver();
            internal readonly MachineNavigation Navigation;
            internal Fixture()
            {
                Region = new InitialRegion(World, new Rect(-50, -50, 100, 100));
                Machine = World.Machines.Create(new MachineDefinition(1, "Fixture", 1, 1, 1, 1, 30, true, true, 100));
                var core = World.Machines.CreateComponent(new ComponentDefinition(2, HardwareKind.Core, 1, 0, 20, 100, false));
                World.Machines.Install(Machine.Id, ManagementOrigin.Library, core.Id, 0);
                Region.DeployMachine(Machine.Id, Vector2.zero, Vector2.one, out _);
                Machine.Activate(ManagementOrigin.Field); Machine.UpdateEnvironment(true, true); Machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                Context = new MachineExecutionContext(Machine, World.IdAllocator); Navigation = new MachineNavigation(Context, Driver, new MachineNavigationSettings());
            }
            internal MachineTaskRecord Start()
            {
                Context.Tasks.Submit("move", WorkPriority.Normal, out var task); Context.Tasks.StartNext();
                Assert.That(Navigation.Start(task.Id, new MachineNavigationTarget(Region, new Vector3(8, 0, 0), 90), 0), Is.EqualTo(NavigationAdmission.Accepted));
                Context.Tasks.CloseChain(task.Id); return task;
            }
            internal bool Restore(Data data, double now)
            {
                Assert.That(Context.Compute.RestorePersistent(data.Compute, 10000), Is.True);
                Context.Tasks.Restore(data.Tasks); Driver.Position = data.Navigation.Position; Driver.Yaw = data.Navigation.Yaw;
                Region.TryUpdateMachinePose(Machine.Id, new Vector2(Driver.Position.x, Driver.Position.z), Driver.Yaw);
                return Navigation.RestorePersistent(data.Navigation, now, target => MachineNavigationTarget.RestorePersistent(target, Region));
            }
            public void Dispose() { Navigation.Dispose(); Context.Dispose(); Region.Dispose(); World.Dispose(); }
        }
        private sealed class Data
        {
            public MachineTaskQueueSnapshot Tasks;
            public MachineNavigationSnapshot Navigation;
            public MachineComputeSnapshot Compute;
        }
        private static Data Json(Fixture source, double now)
        {
            Assert.That(source.Navigation.TryCapturePersistent(now, out var snapshot), Is.True);
            Assert.That(source.Context.Compute.TryCapturePersistent(out var compute), Is.True);
            string json = WorldSnapshotCodec.Serialize(new WorldSnapshotDocument(10000, 1000, 1, "Fixture",
                new[] { new WorldSnapshotSection("navigation", 1, new Data { Tasks = source.Context.Tasks.Capture(), Navigation = snapshot, Compute = compute }) }));
            Assert.That(WorldSnapshotCodec.TryRead(json, new Dictionary<string, int> { { "navigation", 1 } }, out var file, out var reason), Is.True, reason);
            Assert.That(file.TryReadSection<Data>("navigation", out var data, out reason), Is.True, reason); return data;
        }
        [Test] public void MovingCheckpoint_ResumesOriginalTaskAtActualPose_AndCompletesOnce()
        {
            using (var source = new Fixture())
            using (var target = new Fixture())
            {
                var task = source.Start(); source.Navigation.Tick(0, 0); source.Driver.Position = new Vector3(3, 0, 0); source.Navigation.Tick(1, .1f);
                int ended = 0, tasksEnded = 0; target.Navigation.Ended += _ => ended++; target.Context.Tasks.Ended += _ => tasksEnded++;
                Assert.That(target.Restore(Json(source, 1), 0), Is.True); Assert.That(target.Navigation.Position.x, Is.EqualTo(3));
                Assert.That(target.Navigation.CurrentTaskId, Is.EqualTo(task.Id)); Assert.That(ended, Is.Zero); Assert.That(target.Machine.HasActiveBehavior, Is.True);
                target.Navigation.Tick(0, 0); target.Driver.Position = new Vector3(8, 0, 0); target.Navigation.Tick(1, 1);
                Assert.That(target.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed)); Assert.That(ended, Is.EqualTo(1)); Assert.That(tasksEnded, Is.EqualTo(1));
                target.Navigation.Tick(2, 1); Assert.That(ended, Is.EqualTo(1)); Assert.That(target.Context.Compute.Used, Is.Zero);
                Assert.That(source.Navigation.IsActive, Is.True);
            }
        }
        [Test] public void BlockedAge_SurvivesRestartAtZeroRealtime_AndDoesNotResetThirtySecondFailure()
        {
            using (var source = new Fixture())
            using (var target = new Fixture())
            {
                source.Driver.PlanSucceeds = false; source.Start(); source.Navigation.Tick(0, 0); source.Navigation.Tick(10, 1);
                target.Driver.PlanSucceeds = false;
                Assert.That(target.Restore(Json(source, 10), 0), Is.True); Assert.That(target.Navigation.PathWarning, Is.True);
                target.Navigation.Tick(19, 1); Assert.That(target.Navigation.Outcome, Is.Null);
                target.Navigation.Tick(20, 1); Assert.That(target.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Failed));
            }
        }
        [Test] public void Pause_RestoresResponsibilityWithoutMovement_ThenUsesNormalPowerResume()
        {
            using (var source = new Fixture())
            using (var target = new Fixture())
            {
                source.Start(); source.Navigation.Tick(0, 0); source.Machine.UpdateEnvironment(false, true); source.Navigation.Tick(5, 5);
                target.Machine.UpdateEnvironment(false, true); Assert.That(target.Restore(Json(source, 5), 0), Is.True);
                target.Navigation.Tick(50, 50); Assert.That(target.Navigation.IsActive, Is.True); Assert.That(target.Navigation.State, Is.EqualTo(MachineNavigationState.Paused));
                target.Machine.UpdateEnvironment(true, true); target.Navigation.Tick(51, 1); Assert.That(target.Navigation.State, Is.EqualTo(MachineNavigationState.Moving));
            }
        }
        [Test] public void InvalidPoseOrMissingTaskActivity_IsRejectedBeforeCandidateChanges()
        {
            using (var source = new Fixture())
            using (var target = new Fixture())
            {
                source.Start(); var data = Json(source, 0); target.Context.Tasks.Restore(data.Tasks);
                data.Navigation.Position = new Vector3(5, 0, 0);
                Assert.That(target.Navigation.RestorePersistent(data.Navigation, 0, s => MachineNavigationTarget.RestorePersistent(s, target.Region)), Is.False);
                Assert.That(target.Navigation.IsActive, Is.False); Assert.That(target.Navigation.State, Is.EqualTo(MachineNavigationState.Idle));
                data.Navigation.Position = Vector3.zero; data.Navigation.TaskId = new PersistentId(999);
                Assert.That(target.Navigation.RestorePersistent(data.Navigation, 0, s => MachineNavigationTarget.RestorePersistent(s, target.Region)), Is.False);
                Assert.That(target.Navigation.CurrentTaskId.IsValid, Is.False);
            }
        }
        [Test] public void MissingObjectTarget_KeepsOriginalIdentityAndDoesNotGuessAnotherObject()
        {
            using (var target = new Fixture())
            {
                var snapshot = new MachineNavigationTargetSnapshot { ObjectId = new PersistentId(999), Candidates = new[] { new Vector3(8, 0, 0) },
                    HasApproachArea = true, ApproachPosition = new Vector2(7, -1), ApproachSize = new Vector2(2, 2), MaximumSize = Vector2.one };
                var restored = MachineNavigationTarget.RestorePersistent(snapshot, target.Region);
                Assert.That(restored, Is.Not.Null); Assert.That(restored.ObjectId.Value, Is.EqualTo(999)); Assert.That(restored.IsValid, Is.False);
                snapshot.Candidates[0] = Vector3.zero; Assert.That(restored.Candidates[0].x, Is.EqualTo(8));
            }
        }
    }
}
