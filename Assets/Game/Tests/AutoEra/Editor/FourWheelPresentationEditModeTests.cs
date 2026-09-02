using AutoEra.Motion;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class FourWheelPresentationEditModeTests
    {
        [Test]
        public void SteeringModes_AssignIndependentFrontAndRearAngles()
        {
            FourWheelPresentationState normal = FourWheelPresentation.Evaluate(20f, FourWheelSteeringMode.Normal, 1f, 1f);
            FourWheelPresentationState counter = FourWheelPresentation.Evaluate(20f, FourWheelSteeringMode.CounterSteer, 1f, 1f);
            FourWheelPresentationState crab = FourWheelPresentation.Evaluate(20f, FourWheelSteeringMode.Crab, 1f, 1f);
            Assert.That(normal.RearLeft, Is.EqualTo(0f));
            Assert.That(counter.RearLeft, Is.EqualTo(-20f));
            Assert.That(crab.RearLeft, Is.EqualTo(20f));
            Assert.That(crab.WheelRotationDegrees, Is.EqualTo(360f));
        }
    }
}
