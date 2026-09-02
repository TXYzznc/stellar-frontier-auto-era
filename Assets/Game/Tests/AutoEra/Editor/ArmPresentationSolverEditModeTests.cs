using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class ArmPresentationSolverEditModeTests
    {
        [Test]
        public void Solver_ReturnsPoseForReachableAndRejectsUnreachableTarget()
        {
            ArmPresentationSolution reachable = ArmPresentationSolver.Solve(new Vector3(1f, 0f, 2f), 2f, 2f);
            ArmPresentationSolution unreachable = ArmPresentationSolver.Solve(new Vector3(10f, 0f, 0f), 2f, 2f);
            Assert.That(reachable.Reachable, Is.True);
            Assert.That(reachable.YawDegrees, Is.GreaterThan(0f));
            Assert.That(unreachable.Reachable, Is.False);
        }

        [Test]
        public void Solver_PreservesWristTargetAndRequestsRepositionForKeepOut()
        {
            Quaternion targetRotation = Quaternion.Euler(0f, 90f, 0f);
            ArmPresentationSolution reachable = ArmPresentationSolver.Solve(
                new Pose(new Vector3(0f, 0.5f, 3f), targetRotation), 2f, 2f, 0.5f);
            ArmPresentationSolution keepOut = ArmPresentationSolver.Solve(
                new Pose(new Vector3(0.1f, 0f, 0f), targetRotation), 2f, 2f, 0.5f);

            Assert.That(reachable.Reachable, Is.True);
            Assert.That(reachable.RequiresReposition, Is.False);
            Assert.That(Quaternion.Angle(reachable.WristLocalRotation, targetRotation), Is.LessThan(0.001f));
            Assert.That(keepOut.Reachable, Is.False);
            Assert.That(keepOut.RequiresReposition, Is.True);
        }

        [Test]
        public void SafeRetract_ZeroesExtensionAndNeutralizesPitchAndWrist()
        {
            ArmPresentationSolution retract = ArmPresentationSolver.CreateSafeRetract(15f);

            Assert.That(retract.IsSafeRetract, Is.True);
            Assert.That(retract.Extension, Is.Zero);
            Assert.That(retract.PitchDegrees, Is.Zero);
            Assert.That(retract.YawDegrees, Is.EqualTo(15f));
            Assert.That(Quaternion.Angle(retract.WristLocalRotation, Quaternion.identity), Is.LessThan(0.001f));
        }
    }
}
