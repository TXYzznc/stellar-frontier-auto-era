using AutoEra.Motion;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionPreviewTimelineEditModeTests
    {
        [Test]
        public void AdvanceProgress_AppliesSpeedMultiplierAndLoopsAtCycleBoundary()
        {
            Assert.That(MotionPreviewTimelineCore.AdvanceProgress(0f, 1f, 1f), Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(MotionPreviewTimelineCore.AdvanceProgress(0f, 1f, 2f), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void Timeline_ClampsSpeedAndMapsScrubProgressToRepresentativeElapsedTime()
        {
            Assert.That(MotionPreviewTimelineCore.ClampSpeed(0f), Is.EqualTo(MotionPreviewTimelineCore.MinimumSpeed));
            Assert.That(MotionPreviewTimelineCore.ClampSpeed(99f), Is.EqualTo(MotionPreviewTimelineCore.MaximumSpeed));
            Assert.That(MotionPreviewTimelineCore.ElapsedForProgress(0.75f), Is.EqualTo(1.5f).Within(0.0001f));
        }
    }
}
