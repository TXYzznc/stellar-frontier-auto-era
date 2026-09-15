using System;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class InitialRegionEditModeTests
    {
        [Test]
        public void Selection_IsBlockedByUi_AndClearedOnRemoval_IdsAreNotReused()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                RegionObject machine = region.Register(PersistentObjectKind.Machine, "机器", Vector2.zero, Vector2.one);
                Assert.That(region.Select(machine.Id, true), Is.False);
                Assert.That(region.SelectedId.IsValid, Is.False);
                Assert.That(region.Select(machine.Id, false), Is.True);
                region.Remove(machine.Id);
                Assert.That(region.SelectedId.IsValid, Is.False);
                Assert.That(session.ObjectRegistry.TryResolve(machine.Id, machine.Kind, out _), Is.EqualTo(PersistentRegistryResult.Missing));
                RegionObject replacement = region.Register(PersistentObjectKind.Machine, "机器", Vector2.zero, Vector2.one);
                Assert.That(replacement.Id, Is.Not.EqualTo(machine.Id));
                Assert.That(region.Select(machine.Id, false), Is.False);
                Assert.That(region.Select(replacement.Id, false), Is.True);
                region.Select(PersistentId.Invalid, false);
                Assert.That(region.SelectedId.IsValid, Is.False);
            }
        }

        [Test]
        public void ThreeKindsUseWorldRegistry_MineIsWalkableButBlocksConstruction()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                region.Register(PersistentObjectKind.Machine, "机器", new Vector2(-10, 0), Vector2.one);
                region.Register(PersistentObjectKind.Building, "仓库", new Vector2(10, 0), new Vector2(8, 6));
                RegionObject mine = region.Register(PersistentObjectKind.ResourcePoint, "矿脉", Vector2.zero, new Vector2(10, 8), 0, false);
                Assert.That(session.ObjectRegistry.Count, Is.EqualTo(3));
                Assert.That(mine.BlocksNavigation, Is.False);
                Assert.That(region.CanPlace(Vector2.zero, Vector2.one, 0, PersistentId.Invalid, out _), Is.False);
                Assert.That(mine.PublicResourceAmount, Is.Null);
                mine.SetPublicState("无限水源", null, true);
                Assert.That(mine.ResourceIsInfinite, Is.True);
            }
        }

        [Test]
        public void Placement_SnapsAndChecksRotatedEdges_RejectsInvalidNumbers()
        {
            Assert.That(RegionPlacement.SnapPosition(new Vector2(.26f, -.26f)), Is.EqualTo(new Vector2(.5f, -.5f)));
            Assert.That(RegionPlacement.SnapYaw(359), Is.Zero);
            Assert.That(RegionPlacement.SnapYaw(22), Is.EqualTo(15));
            Assert.That(RegionPlacement.Overlaps(Vector2.zero, new Vector2(4, 1), 90,
                new Vector2(0, 1.5f), Vector2.one, 0), Is.True);
            Assert.That(RegionPlacement.Overlaps(Vector2.zero, Vector2.one, 0, Vector2.right, Vector2.one, 0), Is.False);
            Assert.That(RegionPlacement.Inside(new Rect(-2, -2, 4, 4), new Vector2(1, 0), new Vector2(3, 1), 45), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => RegionPlacement.SnapYaw(float.NaN));
        }

        [Test]
        public void WorkQueue_IsFifo_Idempotent_AndReleasesInvalidOwnerAndTarget()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                RegionObject target = region.Register(PersistentObjectKind.ResourcePoint, "矿脉", Vector2.zero, new Vector2(10, 8), 0, false);
                RegionObject a = region.Register(PersistentObjectKind.Machine, "A", new Vector2(-10, 0), Vector2.one);
                RegionObject b = region.Register(PersistentObjectKind.Machine, "B", new Vector2(-12, 0), Vector2.one);
                RegionObject c = region.Register(PersistentObjectKind.Machine, "C", new Vector2(-14, 0), Vector2.one);
                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-5, -4, 10, 8)))
                {
                    Assert.That(queue.Request(a.Id, new Vector2(20, 20)), Is.EqualTo(WorkRequestResult.OutsideWorkArea));
                    Assert.That(queue.Request(a.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.Granted));
                    Assert.That(queue.Request(b.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.Waiting));
                    queue.Request(b.Id, Vector2.zero); queue.Request(c.Id, Vector2.zero);
                    Assert.That(queue.WaitingCount, Is.EqualTo(2));
                    region.Remove(a.Id);
                    Assert.That(queue.Owner, Is.EqualTo(b.Id));
                    queue.Release(b.Id);
                    Assert.That(queue.Owner, Is.EqualTo(c.Id));
                    region.Remove(target.Id);
                    Assert.That(queue.Owner.IsValid, Is.False);
                    Assert.That(queue.Request(c.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.InvalidTarget));
                }
            }
        }

        [Test]
        public void WorkQueue_PriorityDoesNotPreemptOwner_AndProjectsLiveSummary()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-30, -30, 60, 60)))
            {
                var target = region.Register(PersistentObjectKind.ResourcePoint, "农田", Vector2.zero, Vector2.one);
                var a = region.Register(PersistentObjectKind.Machine, "A", new Vector2(3,0), Vector2.one);
                var b = region.Register(PersistentObjectKind.Machine, "B", new Vector2(5,0), Vector2.one);
                var c = region.Register(PersistentObjectKind.Machine, "C", new Vector2(7,0), Vector2.one);
                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-1,-1,2,2), "综合"))
                using (var care = new RegionWorkQueue(region, target.Id, new Rect(-1,-1,2,2), "照料"))
                {
                    queue.Request(a.Id, Vector2.zero);
                    queue.Request(b.Id, Vector2.zero, 1);
                    queue.Request(c.Id, Vector2.zero, 2);
                    Assert.That(queue.Owner, Is.EqualTo(a.Id));
                    Assert.That(target.WorkSummary, Does.Contain("等待 2"));
                    Assert.That(care.Request(b.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.Granted));
                    queue.Release(a.Id);
                    Assert.That(queue.Owner, Is.EqualTo(c.Id));
                    Assert.That(target.WorkSummary, Does.Contain("等待 1"));
                    region.Remove(c.Id);
                    Assert.That(queue.Owner, Is.EqualTo(b.Id));
                }
                Assert.That(target.WorkSummary, Is.Empty);
            }
        }
    }
}
