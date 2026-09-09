using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class ArmPresentationSolverEditModeTests
    {
        [Test]
        public void Solver_ProducesFiveDofPoseForReachableTargetAndRejectsOutOfReachTarget()
        {
            ArmPresentationConfiguration configuration = ArmPresentationConfiguration.Default;
            ArmPresentationSolution reachable = ArmPresentationSolver.Solve(new Pose(new Vector3(1f, 0.5f, 2f), Quaternion.Euler(10f, 0f, 25f)), configuration);
            ArmPresentationSolution unreachable = ArmPresentationSolver.Solve(new Pose(new Vector3(10f, 0f, 0f), Quaternion.identity), configuration);
            Assert.That(reachable.Reachable, Is.True);
            Assert.That(reachable.BaseYawDegrees, Is.GreaterThan(0f));
            Assert.That(reachable.ShoulderPitchDegrees, Is.InRange(configuration.ShoulderMinimumDegrees, configuration.ShoulderMaximumDegrees));
            Assert.That(reachable.ElbowPitchDegrees, Is.InRange(configuration.ElbowMinimumDegrees, configuration.ElbowMaximumDegrees));
            Assert.That(reachable.WristPitchDegrees, Is.InRange(configuration.WristPitchMinimumDegrees, configuration.WristPitchMaximumDegrees));
            Assert.That(reachable.WristRollDegrees, Is.InRange(configuration.WristRollMinimumDegrees, configuration.WristRollMaximumDegrees));
            Assert.That(unreachable.Reachable, Is.False);
            Assert.That(unreachable.RequiresReposition, Is.True);
        }

        [Test]
        public void Solver_UsesConfiguredLimitsInsteadOfHardCodedJointAngles()
        {
            ArmPresentationConfiguration configuration = ArmPresentationConfiguration.Default;
            configuration.ShoulderMinimumDegrees = -5f;
            configuration.ShoulderMaximumDegrees = 5f;
            configuration.ElbowMinimumDegrees = -15f;
            configuration.ElbowMaximumDegrees = -10f;
            configuration.WristPitchMinimumDegrees = -8f;
            configuration.WristPitchMaximumDegrees = 8f;
            configuration.WristRollMinimumDegrees = -12f;
            configuration.WristRollMaximumDegrees = 12f;
            ArmPresentationSolution solution = ArmPresentationSolver.Solve(new Pose(new Vector3(0f, 1f, 2.5f), Quaternion.Euler(80f, 0f, 90f)), configuration);

            Assert.That(solution.Reachable, Is.True);
            Assert.That(solution.ShoulderPitchDegrees, Is.InRange(-5f, 5f));
            Assert.That(solution.ElbowPitchDegrees, Is.InRange(-15f, -10f));
            Assert.That(solution.WristPitchDegrees, Is.InRange(-8f, 8f));
            Assert.That(solution.WristRollDegrees, Is.InRange(-12f, 12f));
        }

        [Test]
        public void SafeRetract_UsesConfiguredRestPoseForTheFourLimitedJoints()
        {
            ArmPresentationConfiguration configuration = ArmPresentationConfiguration.Default;
            ArmPresentationSolution retract = ArmPresentationSolver.CreateSafeRetract(15f, configuration);

            Assert.That(retract.IsSafeRetract, Is.True);
            Assert.That(retract.BaseYawDegrees, Is.EqualTo(15f));
            Assert.That(retract.ShoulderPitchDegrees, Is.EqualTo(configuration.SafeShoulderDegrees));
            Assert.That(retract.ElbowPitchDegrees, Is.EqualTo(configuration.SafeElbowDegrees));
            Assert.That(retract.WristPitchDegrees, Is.EqualTo(configuration.SafeWristPitchDegrees));
            Assert.That(retract.WristRollDegrees, Is.EqualTo(configuration.SafeWristRollDegrees));
        }
    }
}
