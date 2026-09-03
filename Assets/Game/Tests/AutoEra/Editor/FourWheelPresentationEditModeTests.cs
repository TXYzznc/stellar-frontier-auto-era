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

        [Test]
        public void Kinematics_UsesTravelDistanceAndWheelRadiusForEveryWheel()
        {
            var input = new FourWheelKinematicsInput
            {
                SteeringDegrees = 30f,
                SteeringMode = FourWheelSteeringMode.CounterSteer,
                TravelDistanceMeters = Mathf.PI,
                WheelRadiusMeters = 0.5f,
                FrontLeftSuspensionMeters = -0.05f,
                FrontRightSuspensionMeters = -0.02f,
                RearLeftSuspensionMeters = 0.03f,
                RearRightSuspensionMeters = 0.06f
            };

            FourWheelKinematicsState state = FourWheelPresentation.EvaluateKinematics(input);

            Assert.That(state.FrontLeft.SteeringDegrees, Is.EqualTo(30f));
            Assert.That(state.RearRight.SteeringDegrees, Is.EqualTo(-30f));
            Assert.That(state.FrontLeft.RollDegrees, Is.EqualTo(360f).Within(0.001f));
            Assert.That(state.RearLeft.SuspensionMeters, Is.EqualTo(0.03f));
            Assert.That(state.RearRight.SuspensionMeters, Is.EqualTo(0.06f));
        }
    }
}
