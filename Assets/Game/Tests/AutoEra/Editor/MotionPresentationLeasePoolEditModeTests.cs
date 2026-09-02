using AutoEra.Motion;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionPresentationLeasePoolEditModeTests
    {
        [Test]
        public void Pool_ReusesLeaseAndRestoresBaselineForSceneReentry()
        {
            var pool = new MotionPresentationLeasePool();
            MotionPresentationLease first = pool.Acquire("arm");
            first.SetPresentation(0.8f, MotionPresentationUpdateLevel.Invisible);
            pool.Release(first);

            MotionPresentationLease reentered = pool.Acquire("arm");

            Assert.That(reentered, Is.SameAs(first));
            Assert.That(reentered.IsActive, Is.True);
            Assert.That(reentered.NormalizedProgress, Is.Zero);
            Assert.That(reentered.UpdateLevel, Is.EqualTo(MotionPresentationUpdateLevel.Near));
            Assert.That(reentered.PlaybackPass, Is.EqualTo(2));
        }

        [Test]
        public void Pool_ConsecutivePlaybackDoesNotAccumulatePresentationDrift()
        {
            var pool = new MotionPresentationLeasePool();
            MotionPresentationLease lease = pool.Acquire("conveyor");
            lease.SetPresentation(1f, MotionPresentationUpdateLevel.Far);
            pool.Release(lease);
            lease = pool.Acquire("conveyor");

            Assert.That(lease.NormalizedProgress, Is.Zero);
            Assert.That(lease.UpdateLevel, Is.EqualTo(MotionPresentationUpdateLevel.Near));
            Assert.That(lease.PrototypeId, Is.EqualTo("conveyor"));
        }
    }
}
