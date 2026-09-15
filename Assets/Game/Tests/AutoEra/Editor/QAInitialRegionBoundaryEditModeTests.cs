using System;
using System.Linq;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// Independent B10 boundary coverage.  These cases deliberately do not repeat the
    /// client-owned InitialRegionEditModeTests scenarios.
    /// </summary>
    public sealed class QAInitialRegionBoundaryEditModeTests
    {
        [Test]
        public void SessionEnd_InvalidatesRegionExposure_AndLaterDisposeIsIdempotent()
        {
            var session = new AutoEraWorldSessionFactory().Create(0);
            var region = new InitialRegion(session, new Rect(-10, -10, 20, 20));
            RegionObject machine = region.Register(PersistentObjectKind.Machine, "machine", Vector2.zero, Vector2.one);

            session.Dispose();

            Assert.That(region.TryGet(machine.Id, out _), Is.False);
            Assert.That(region.Select(machine.Id, false), Is.False);
            Assert.That(region.CanPlace(Vector2.one, Vector2.one, 0, PersistentId.Invalid, out _), Is.False);
            Assert.That(region.Count, Is.Zero, "失效会话不得继续公开区域对象。");
            Assert.That(region.Objects.Any(), Is.False, "失效会话不得通过 Objects 暴露陈旧对象。");
            Assert.Throws<ObjectDisposedException>(() => region.Register(PersistentObjectKind.Machine, "later", Vector2.one, Vector2.one));

            Assert.DoesNotThrow(region.Dispose);
            Assert.DoesNotThrow(region.Dispose);
        }

        [Test]
        public void SeparateTargets_AllowIndependentWorkChannelsInParallel()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-40, -40, 80, 80)))
            {
                RegionObject leftTarget = region.Register(PersistentObjectKind.ResourcePoint, "left", new Vector2(-10, 0), new Vector2(4, 4), 0, false);
                RegionObject rightTarget = region.Register(PersistentObjectKind.ResourcePoint, "right", new Vector2(10, 0), new Vector2(4, 4), 0, false);
                RegionObject leftMachine = region.Register(PersistentObjectKind.Machine, "left-machine", new Vector2(-25, 0), Vector2.one);
                RegionObject rightMachine = region.Register(PersistentObjectKind.Machine, "right-machine", new Vector2(25, 0), Vector2.one);

                using (var left = new RegionWorkQueue(region, leftTarget.Id, new Rect(-12, -2, 4, 4)))
                using (var right = new RegionWorkQueue(region, rightTarget.Id, new Rect(8, -2, 4, 4)))
                {
                    Assert.That(left.Request(leftMachine.Id, new Vector2(-10, 0)), Is.EqualTo(WorkRequestResult.Granted));
                    Assert.That(right.Request(rightMachine.Id, new Vector2(10, 0)), Is.EqualTo(WorkRequestResult.Granted));
                    Assert.That(left.Owner, Is.EqualTo(leftMachine.Id));
                    Assert.That(right.Owner, Is.EqualTo(rightMachine.Id));
                    Assert.That(left.WaitingCount, Is.Zero);
                    Assert.That(right.WaitingCount, Is.Zero);
                }
            }
        }

        [Test]
        public void RemovedWaiter_IsSkippedWhenOwnershipTransfers()
        {
            using (var session = new AutoEraWorldSessionFactory().Create(0))
            using (var region = new InitialRegion(session, new Rect(-40, -40, 80, 80)))
            {
                RegionObject target = region.Register(PersistentObjectKind.ResourcePoint, "target", Vector2.zero, new Vector2(4, 4), 0, false);
                RegionObject owner = region.Register(PersistentObjectKind.Machine, "owner", new Vector2(-20, 0), Vector2.one);
                RegionObject removedWaiter = region.Register(PersistentObjectKind.Machine, "removed", new Vector2(-24, 0), Vector2.one);
                RegionObject successor = region.Register(PersistentObjectKind.Machine, "successor", new Vector2(-28, 0), Vector2.one);

                using (var queue = new RegionWorkQueue(region, target.Id, new Rect(-2, -2, 4, 4)))
                {
                    Assert.That(queue.Request(owner.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.Granted));
                    Assert.That(queue.Request(removedWaiter.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.Waiting));
                    Assert.That(queue.Request(successor.Id, Vector2.zero), Is.EqualTo(WorkRequestResult.Waiting));
                    Assert.That(region.Remove(removedWaiter.Id), Is.True);
                    Assert.That(queue.WaitingCount, Is.EqualTo(1));

                    Assert.That(queue.Release(owner.Id), Is.True);
                    Assert.That(queue.Owner, Is.EqualTo(successor.Id));
                    Assert.That(queue.WaitingCount, Is.Zero);
                }
            }
        }

        [Test]
        public void RotatedPlacement_UsesEdgeTouchAsNonOverlap_AndRejectsOutsideBoundary()
        {
            float rotatedHalfExtent = Mathf.Sqrt(2f);
            var bounds = new Rect(-5, -5, 10, 10);

            Assert.That(RegionPlacement.Inside(bounds, new Vector2(5f - rotatedHalfExtent, 0), new Vector2(2, 2), 45), Is.True);
            Assert.That(RegionPlacement.Inside(bounds, new Vector2(5.01f - rotatedHalfExtent, 0), new Vector2(2, 2), 45), Is.False);

            float edgeTouchDistance = 2f * rotatedHalfExtent;
            Assert.That(RegionPlacement.Overlaps(Vector2.zero, new Vector2(2, 2), 45,
                new Vector2(edgeTouchDistance, 0), new Vector2(2, 2), -45), Is.False);
            Assert.That(RegionPlacement.Overlaps(Vector2.zero, new Vector2(2, 2), 45,
                new Vector2(edgeTouchDistance - .01f, 0), new Vector2(2, 2), -45), Is.True);
        }
    }
}
