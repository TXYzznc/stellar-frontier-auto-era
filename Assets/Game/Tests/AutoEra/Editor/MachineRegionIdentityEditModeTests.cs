using System;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineRegionIdentityEditModeTests
    {
        private static InitialRegion Region(AutoEraWorldSession session)
            => new InitialRegion(session, new Rect(-30, -30, 60, 60));

        private static MachineInstance Machine(AutoEraWorldSession session, bool mobile = true)
            => session.Machines.Create(new MachineDefinition(mobile ? 1 : 2, "Fixture", 1, 2, 1, 1, 10, mobile, true, 100));

        private static RegionObject Bind(InitialRegion region, MachineInstance machine, float x)
        {
            Assert.That(region.DeployMachine(machine.Id, new Vector2(x, 0), Vector2.one, out var model),
                Is.EqualTo(RegionMachineDeploymentResult.Bound));
            return model;
        }

        private static RegionObject Target(InitialRegion region)
            => region.Register(PersistentObjectKind.ResourcePoint, "Target", Vector2.zero, Vector2.one);

        [Test]
        public void Deployment_PreservesIdentityAndAllocator_AndPublishesConsistentState()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var machine = Machine(session);
                var next = session.IdAllocator.NextId;
                int notifications = 0;
                machine.Changed += changed =>
                {
                    notifications++;
                    Assert.That(changed.Deployed, Is.True);
                    Assert.That(region.TryGetMachine(changed.Id, out var same), Is.True);
                    Assert.That(same, Is.SameAs(machine));
                };
                var model = Bind(region, machine, 3);
                Assert.That(model.Id, Is.EqualTo(machine.Id));
                Assert.That(model.Machine, Is.SameAs(machine));
                Assert.That(model.IsRegistered, Is.True);
                Assert.That(session.ObjectRegistry.Count, Is.EqualTo(1));
                Assert.That(session.ObjectRegistry.TryResolve(machine.Id, PersistentObjectKind.Machine, out var resolved),
                    Is.EqualTo(PersistentRegistryResult.Success));
                Assert.That(resolved, Is.SameAs(machine));
                Assert.That(session.IdAllocator.NextId, Is.EqualTo(next));
                Assert.That(notifications, Is.EqualTo(1));
                Assert.That(region.DeployMachine(machine.Id, new Vector2(3, 0), Vector2.one, out var again),
                    Is.EqualTo(RegionMachineDeploymentResult.AlreadyBound));
                Assert.That(again, Is.SameAs(model));
                Assert.That(notifications, Is.EqualTo(1));
                Assert.That(region.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void FailedDeployment_HasNoPartialState_AndCanBeRetried()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                Target(region);
                var machine = Machine(session);
                var next = session.IdAllocator.NextId;
                Assert.That(region.DeployMachine(machine.Id, Vector2.zero, Vector2.one, out var model),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(model, Is.Null);
                Assert.That(region.DeployMachine(machine.Id, new Vector2(float.NaN, 0), Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(region.DeployMachine(machine.Id, new Vector2(100, 0), Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(region.DeployMachine(machine.Id, Vector2.right, Vector2.zero, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(region.DeployMachine(machine.Id, Vector2.right, Vector2.one, out _, float.PositiveInfinity),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(machine.Deployed, Is.False);
                Assert.That(region.TryGet(machine.Id, out _), Is.False);
                Assert.That(region.Count, Is.EqualTo(1));
                Assert.That(session.IdAllocator.NextId, Is.EqualTo(next));
                Bind(region, machine, 3);
            }
        }

        [Test]
        public void InvalidDestroyedOrMismatchedRegistryIdentity_IsRejected()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                Assert.That(region.DeployMachine(PersistentId.Invalid, Vector2.zero, Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidMachine));
                var machine = Machine(session);
                machine.UpdateIntegrity(0);
                Assert.That(region.DeployMachine(machine.Id, Vector2.zero, Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.Destroyed));
                machine.UpdateIntegrity(100);
                session.ObjectRegistry.TryUnregister(machine.Id, PersistentObjectKind.Machine, machine);
                session.ObjectRegistry.TryRegister(machine.Id, PersistentObjectKind.Machine, new object());
                Assert.That(region.DeployMachine(machine.Id, Vector2.zero, Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidMachine));
                Assert.That(machine.Deployed, Is.False);
                Assert.That(region.Count, Is.Zero);
            }
        }

        [Test]
        public void FixedCarrier_RepeatedDeploymentDoesNotMoveOrGrantMovement()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var machine = Machine(session, false);
                var model = Bind(region, machine, 3);
                Assert.That(region.DeployMachine(machine.Id, new Vector2(5, 0), Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(region.DeployMachine(machine.Id, new Vector2(3, 0), Vector2.one, out _, 90),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidPlacement));
                Assert.That(model.Position, Is.EqualTo(new Vector2(3, 0)));
                Assert.That(model.Yaw, Is.Zero);
                Assert.That(machine.Definition.CanMove, Is.False);
            }
        }

        [Test]
        public void OtherRegionIsRejected_UntilProjectionIsReleased()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var first = Region(session))
            using (var second = Region(session))
            {
                var machine = Machine(session);
                var model = Bind(first, machine, 3);
                Assert.That(second.DeployMachine(machine.Id, new Vector2(5, 0), Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.BoundElsewhere));
                Assert.That(second.Count, Is.Zero);
                first.Remove(machine.Id);
                Assert.That(first.Remove(machine.Id), Is.False);
                Assert.That(model.IsRegistered, Is.False);
                Assert.That(machine.Deployed, Is.True);
                Bind(second, machine, 5);
                Assert.That(session.ObjectRegistry.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public void RecoveryHonorsExistingGates_ThenDetachesWithoutDeletingIdentity()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var machine = Machine(session);
                var model = Bind(region, machine, 3);
                machine.UpdateIntegrity(50);
                Assert.That(session.Machines.RecoverToLibrary(machine.Id, ManagementOrigin.Field),
                    Is.EqualTo(MachineManagementResult.RepairRequired));
                Assert.That(region.TryGet(machine.Id, out _), Is.True);
                machine.UpdateIntegrity(100);
                machine.Activate(ManagementOrigin.Field);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                Assert.That(session.Machines.RecoverToLibrary(machine.Id, ManagementOrigin.Field), Is.EqualTo(MachineManagementResult.MustStop));
                Assert.That(machine.Deployed, Is.True);
                machine.SetRunState(ManagementOrigin.Field, MachineRunState.Stopped);
                Assert.That(session.Machines.RecoverToLibrary(machine.Id, ManagementOrigin.Field), Is.EqualTo(MachineManagementResult.Completed));
                Assert.That(region.TryGetMachine(machine.Id, out _), Is.False);
                Assert.That(model.IsRegistered, Is.False);
                Assert.That(machine.Deployed, Is.False);
                Assert.That(session.Machines.TryGet(machine.Id, out var same), Is.True);
                Assert.That(same, Is.SameAs(machine));
                Assert.That(session.ObjectRegistry.Count, Is.EqualTo(1));
                Assert.That(session.Machines.RecoverToLibrary(machine.Id, ManagementOrigin.Field), Is.EqualTo(MachineManagementResult.InvalidOrigin));
                Bind(region, machine, 3);
            }
        }

        [Test]
        public void NameSyncAndRegionUnload_PreserveRosterAndDetachListeners()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            {
                var region = Region(session);
                var machine = Machine(session);
                var model = Bind(region, machine, 3);
                session.Machines.Rename(machine.Id, "Renamed");
                Assert.That(model.Name, Is.EqualTo("Renamed"));
                region.Dispose(); region.Dispose();
                Assert.That(model.IsRegistered, Is.False);
                Assert.That(region.IsActive, Is.False);
                Assert.That(region.Count, Is.Zero);
                Assert.That(machine.Deployed, Is.True);
                Assert.That(session.ObjectRegistry.Count, Is.EqualTo(1));
                session.Machines.Rename(machine.Id, "AfterUnload");
                Assert.That(model.Name, Is.EqualTo("Renamed"));
                Assert.That(region.DeployMachine(machine.Id, Vector2.zero, Vector2.one, out _),
                    Is.EqualTo(RegionMachineDeploymentResult.InvalidRegion));
                using (var another = Region(session)) Assert.That(Bind(another, machine, 3).Name, Is.EqualTo("AfterUnload"));
            }
        }

        [Test]
        public void WorkQueueUsesSameId_PriorityFifoAndQueryDoNotImplyArrival()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var target = Target(region);
                var a = Machine(session); var b = Machine(session); var c = Machine(session); var d = Machine(session);
                var aModel = Bind(region, a, 3); Bind(region, b, 5); Bind(region, c, 7); Bind(region, d, 9);
                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-1, -1, 2, 2)))
                {
                    Assert.That(queue.GetRequestState(a.Id), Is.EqualTo(WorkRequestState.None));
                    Assert.That(queue.Owner.IsValid, Is.False);
                    Assert.That(queue.Request(a.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.Granted));
                    queue.Request(b.Id, Vector2.zero, 1);
                    queue.Request(c.Id, Vector2.zero, 2);
                    queue.Request(d.Id, Vector2.zero, 2);
                    queue.Request(c.Id, Vector2.zero, 2);
                    Assert.That(queue.WaitingCount, Is.EqualTo(3));
                    Assert.That(queue.GetRequestState(a.Id), Is.EqualTo(WorkRequestState.Granted));
                    Assert.That(queue.GetRequestState(c.Id), Is.EqualTo(WorkRequestState.Waiting));
                    Assert.That(aModel.Position, Is.EqualTo(new Vector2(3, 0)), "Reservation is not arrival.");
                    queue.Release(a.Id); Assert.That(queue.Owner, Is.EqualTo(c.Id));
                    queue.Release(c.Id); Assert.That(queue.Owner, Is.EqualTo(d.Id));
                    queue.Release(d.Id); Assert.That(queue.Owner, Is.EqualTo(b.Id));
                    queue.Release(b.Id); Assert.That(queue.Owner.IsValid, Is.False);
                }
            }
        }

        [Test]
        public void WaitingCancelAndOwnerRecovery_ReleaseExactlyTheirReservations()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var target = Target(region);
                var a = Machine(session); var b = Machine(session); var c = Machine(session);
                Bind(region, a, 3); Bind(region, b, 5); Bind(region, c, 7);
                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-1, -1, 2, 2)))
                {
                    queue.Request(a.Id, Vector2.zero); queue.Request(b.Id, Vector2.zero); queue.Request(c.Id, Vector2.zero);
                    Assert.That(queue.Release(b.Id), Is.True);
                    Assert.That(queue.Release(b.Id), Is.False);
                    Assert.That(queue.GetRequestState(b.Id), Is.EqualTo(WorkRequestState.None));
                    Assert.That(queue.WaitingCount, Is.EqualTo(1));
                    session.Machines.RecoverToLibrary(a.Id, ManagementOrigin.Field);
                    Assert.That(queue.Owner, Is.EqualTo(c.Id));
                    Assert.That(queue.GetRequestState(a.Id), Is.EqualTo(WorkRequestState.InvalidRequester));
                    region.Remove(c.Id);
                    Assert.That(queue.Owner.IsValid, Is.False);
                    Assert.That(queue.WaitingCount, Is.Zero);
                }
            }
        }

        [Test]
        public void UnboundRequesterAndRemovedTarget_DoNotAcquireOrKeepReservations()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var target = Target(region);
                var machine = Machine(session);
                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-1, -1, 2, 2)))
                {
                    Assert.That(queue.Request(machine.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.InvalidRequester));
                    Assert.That(queue.GetRequestState(machine.Id), Is.EqualTo(WorkRequestState.InvalidRequester));
                    Bind(region, machine, 3);
                    Assert.That(queue.Request(machine.Id, new Vector2(20, 20)), Is.EqualTo(WorkRequestResult.OutsideWorkArea));
                    queue.Request(machine.Id, Vector2.zero);
                    region.Remove(target.Id);
                    Assert.That(queue.GetRequestState(machine.Id), Is.EqualTo(WorkRequestState.InvalidTarget));
                    Assert.That(queue.Owner.IsValid, Is.False);
                    Assert.That(queue.WaitingCount, Is.Zero);
                    Assert.That(target.WorkSummary, Is.Empty);
                    Assert.That(session.Machines.TryGet(machine.Id, out _), Is.True);
                }
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WorldOrRegionUnload_ClosesQueuesAndProjections(bool worldFirst)
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var target = Target(region);
                var a = Machine(session); var b = Machine(session);
                var model = Bind(region, a, 3); Bind(region, b, 5);
                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-1, -1, 2, 2)))
                {
                    queue.Request(a.Id, Vector2.zero); queue.Request(b.Id, Vector2.zero);
                    if (worldFirst) session.Dispose(); else region.Dispose();
                    Assert.That(region.IsActive, Is.False);
                    Assert.That(model.IsRegistered, Is.False);
                    Assert.That(target.IsRegistered, Is.False);
                    Assert.That(queue.Owner.IsValid, Is.False);
                    Assert.That(queue.WaitingCount, Is.Zero);
                    Assert.That(target.WorkSummary, Is.Empty);
                    Assert.That(queue.GetRequestState(a.Id), Is.EqualTo(WorkRequestState.InvalidTarget));
                    Assert.That(session.ObjectRegistry.Count, Is.EqualTo(worldFirst ? 0 : 2));
                    Assert.That(session.Machines.TryGet(a.Id, out _), Is.EqualTo(!worldFirst));
                    region.Dispose(); session.Dispose();
                }
            }
        }

        [Test]
        public void RosterDisposalAlone_ClosesRegionAndCannotBeRebound()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = Region(session))
            {
                var machine = Machine(session); var model = Bind(region, machine, 3);
                session.Machines.Dispose();
                Assert.That(region.IsActive, Is.False);
                Assert.That(model.IsRegistered, Is.False);
                Assert.Throws<ObjectDisposedException>(() => Region(session));
            }
        }
    }
}
