using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class ConveyorLoopPresentationEditModeTests
    {
        [Test]
        public void LoopPose_IsClosedAndContainsUpperAndLowerWorkingRuns()
        {
            ConveyorLoopPresentation.EvaluateLoopPose(0f, out Vector3 startPosition, out Quaternion startRotation);
            ConveyorLoopPresentation.EvaluateLoopPose(1f, out Vector3 endPosition, out Quaternion endRotation);
            // 0.5 falls exactly on a curved-to-lower-run boundary; sample inside the lower run instead.
            ConveyorLoopPresentation.EvaluateLoopPose(0.6f, out Vector3 lowerPosition, out Quaternion lowerRotation);

            Assert.That(Vector3.Distance(startPosition, endPosition), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(startRotation, endRotation), Is.LessThan(0.01f));
            Assert.That(startPosition.y, Is.GreaterThan(0f));
            Assert.That(lowerPosition.y, Is.LessThan(0f));
            Assert.That(Quaternion.Angle(lowerRotation, Quaternion.Euler(180f, 0f, 0f)), Is.LessThan(0.01f));
        }

        [Test]
        public void CargoPosition_StaysOnUpperLoadRunAndRollerSpeedMatchesBeltSpeed()
        {
            Vector3 start = ConveyorLoopPresentation.EvaluateCargoPosition(0f);
            Vector3 end = ConveyorLoopPresentation.EvaluateCargoPosition(1f);

            Assert.That(start.y, Is.GreaterThan(ConveyorLoopPresentation.ReturnRadius));
            Assert.That(end.z, Is.EqualTo(start.z).Within(0.0001f));
            Assert.That(ConveyorLoopPresentation.EvaluateRollerDegrees(1f), Is.EqualTo(ConveyorLoopPresentation.TreadSpeedMetersPerSecond / ConveyorLoopPresentation.DriveRollerRadius * Mathf.Rad2Deg).Within(0.0001f));
            Assert.That(ConveyorLoopPresentation.EvaluateRollerDegrees(1f, ConveyorLoopPresentation.DriveRollerRadius),
                Is.EqualTo(ConveyorLoopPresentation.TreadSpeedMetersPerSecond / ConveyorLoopPresentation.DriveRollerRadius * Mathf.Rad2Deg).Within(0.0001f));
            Assert.That(ConveyorLoopPresentation.EvaluateRollerDegrees(1f, ConveyorLoopPresentation.SupportRollerRadius),
                Is.EqualTo(ConveyorLoopPresentation.TreadSpeedMetersPerSecond / ConveyorLoopPresentation.SupportRollerRadius * Mathf.Rad2Deg).Within(0.0001f));
            Assert.That(ConveyorLoopPresentation.EvaluateRollerDegrees(1f, ConveyorLoopPresentation.SupportRollerRadius),
                Is.GreaterThan(ConveyorLoopPresentation.EvaluateRollerDegrees(1f, ConveyorLoopPresentation.DriveRollerRadius)));
            Assert.That(ConveyorLoopPresentation.EvaluateRollerDegrees(1f) * ConveyorLoopPresentation.DriveRollerRadius * Mathf.Deg2Rad,
                Is.EqualTo(ConveyorLoopPresentation.TreadSpeedMetersPerSecond).Within(0.0001f));
            Assert.That(ConveyorLoopPresentation.EvaluateRollerDegrees(1f, ConveyorLoopPresentation.SupportRollerRadius) * ConveyorLoopPresentation.SupportRollerRadius * Mathf.Deg2Rad,
                Is.EqualTo(ConveyorLoopPresentation.TreadSpeedMetersPerSecond).Within(0.0001f));
        }
    }
}
