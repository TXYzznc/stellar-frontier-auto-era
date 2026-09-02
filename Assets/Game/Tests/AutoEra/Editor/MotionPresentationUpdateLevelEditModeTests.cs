using AutoEra.Motion;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionPresentationUpdateLevelEditModeTests
    {
        [Test]
        public void UpdateLevels_DegradeOnlyPresentationEvaluation()
        {
            Assert.That(MotionPresentationUpdatePolicy.ShouldEvaluate(MotionPresentationUpdateLevel.Near), Is.True);
            Assert.That(MotionPresentationUpdatePolicy.GetMinimumIntervalSeconds(MotionPresentationUpdateLevel.Near), Is.EqualTo(0f));
            Assert.That(MotionPresentationUpdatePolicy.GetMinimumIntervalSeconds(MotionPresentationUpdateLevel.Mid), Is.GreaterThan(0f));
            Assert.That(MotionPresentationUpdatePolicy.GetMinimumIntervalSeconds(MotionPresentationUpdateLevel.Far), Is.GreaterThan(MotionPresentationUpdatePolicy.GetMinimumIntervalSeconds(MotionPresentationUpdateLevel.Mid)));
            Assert.That(MotionPresentationUpdatePolicy.ShouldEvaluate(MotionPresentationUpdateLevel.Invisible), Is.False);
        }
    }
}
