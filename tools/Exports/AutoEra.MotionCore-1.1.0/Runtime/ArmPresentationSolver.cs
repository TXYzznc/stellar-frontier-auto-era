using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Authorable limits and bind-safe poses for the five-degree-of-freedom arm presentation.</summary>
    public struct ArmPresentationConfiguration
    {
        public float UpperArmLength;
        public float ForearmLength;
        public float KeepOutRadius;
        public float ShoulderMinimumDegrees;
        public float ShoulderMaximumDegrees;
        public float ElbowMinimumDegrees;
        public float ElbowMaximumDegrees;
        public float WristPitchMinimumDegrees;
        public float WristPitchMaximumDegrees;
        public float WristRollMinimumDegrees;
        public float WristRollMaximumDegrees;
        public float SafeShoulderDegrees;
        public float SafeElbowDegrees;
        public float SafeWristPitchDegrees;
        public float SafeWristRollDegrees;

        public static ArmPresentationConfiguration Default
        {
            get
            {
                return new ArmPresentationConfiguration
                {
                    UpperArmLength = 2.1f, ForearmLength = 1.8f, KeepOutRadius = 0.35f,
                    ShoulderMinimumDegrees = -70f, ShoulderMaximumDegrees = 80f,
                    ElbowMinimumDegrees = -120f, ElbowMaximumDegrees = 120f,
                    WristPitchMinimumDegrees = -100f, WristPitchMaximumDegrees = 100f,
                    WristRollMinimumDegrees = -170f, WristRollMaximumDegrees = 170f,
                    SafeShoulderDegrees = 20f, SafeElbowDegrees = -65f,
                    SafeWristPitchDegrees = 35f, SafeWristRollDegrees = 0f
                };
            }
        }
    }

    public struct ArmPresentationSolution
    {
        public bool Reachable;
        public bool RequiresReposition;
        public bool IsSafeRetract;
        public float BaseYawDegrees;
        public float ShoulderPitchDegrees;
        public float ElbowPitchDegrees;
        public float WristPitchDegrees;
        public float WristRollDegrees;
        public Pose TargetPose;
    }

    /// <summary>Deterministic two-link reach solve plus two wrist channels. The base rotates continuously; every arm joint is configuration-clamped.</summary>
    public static class ArmPresentationSolver
    {
        public static ArmPresentationSolution Solve(Pose targetPose, ArmPresentationConfiguration configuration)
        {
            Vector3 target = targetPose.position;
            float planarDistance = new Vector2(target.x, target.z).magnitude;
            float targetDistance = Mathf.Sqrt(planarDistance * planarDistance + target.y * target.y);
            float minimumReach = Mathf.Abs(configuration.UpperArmLength - configuration.ForearmLength);
            float maximumReach = configuration.UpperArmLength + configuration.ForearmLength;
            bool requiresReposition = targetDistance < Mathf.Max(configuration.KeepOutRadius, minimumReach) || targetDistance > maximumReach;
            if (requiresReposition)
            {
                return new ArmPresentationSolution { Reachable = false, RequiresReposition = true, TargetPose = targetPose };
            }

            float cosineElbow = Mathf.Clamp((targetDistance * targetDistance - configuration.UpperArmLength * configuration.UpperArmLength - configuration.ForearmLength * configuration.ForearmLength) / (2f * configuration.UpperArmLength * configuration.ForearmLength), -1f, 1f);
            float elbowRadians = Mathf.Acos(cosineElbow);
            float shoulderRadians = Mathf.Atan2(target.y, Mathf.Max(0.0001f, planarDistance)) - Mathf.Atan2(configuration.ForearmLength * Mathf.Sin(elbowRadians), configuration.UpperArmLength + configuration.ForearmLength * Mathf.Cos(elbowRadians));
            Vector3 targetEuler = NormalizeEuler(targetPose.rotation.eulerAngles);
            float shoulder = Mathf.Clamp(shoulderRadians * Mathf.Rad2Deg, configuration.ShoulderMinimumDegrees, configuration.ShoulderMaximumDegrees);
            float elbow = Mathf.Clamp(-elbowRadians * Mathf.Rad2Deg, configuration.ElbowMinimumDegrees, configuration.ElbowMaximumDegrees);

            return new ArmPresentationSolution
            {
                Reachable = true,
                BaseYawDegrees = Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg,
                ShoulderPitchDegrees = shoulder,
                ElbowPitchDegrees = elbow,
                WristPitchDegrees = Mathf.Clamp(targetEuler.x - shoulder - elbow, configuration.WristPitchMinimumDegrees, configuration.WristPitchMaximumDegrees),
                WristRollDegrees = Mathf.Clamp(targetEuler.z, configuration.WristRollMinimumDegrees, configuration.WristRollMaximumDegrees),
                TargetPose = targetPose
            };
        }

        public static ArmPresentationSolution CreateSafeRetract(float baseYawDegrees, ArmPresentationConfiguration configuration)
        {
            return new ArmPresentationSolution
            {
                Reachable = true, IsSafeRetract = true, BaseYawDegrees = baseYawDegrees,
                ShoulderPitchDegrees = Mathf.Clamp(configuration.SafeShoulderDegrees, configuration.ShoulderMinimumDegrees, configuration.ShoulderMaximumDegrees),
                ElbowPitchDegrees = Mathf.Clamp(configuration.SafeElbowDegrees, configuration.ElbowMinimumDegrees, configuration.ElbowMaximumDegrees),
                WristPitchDegrees = Mathf.Clamp(configuration.SafeWristPitchDegrees, configuration.WristPitchMinimumDegrees, configuration.WristPitchMaximumDegrees),
                WristRollDegrees = Mathf.Clamp(configuration.SafeWristRollDegrees, configuration.WristRollMinimumDegrees, configuration.WristRollMaximumDegrees),
                TargetPose = new Pose(Vector3.zero, Quaternion.identity)
            };
        }

        private static Vector3 NormalizeEuler(Vector3 eulerDegrees)
        {
            return new Vector3(NormalizeAngle(eulerDegrees.x), NormalizeAngle(eulerDegrees.y), NormalizeAngle(eulerDegrees.z));
        }

        private static float NormalizeAngle(float degrees)
        {
            return degrees > 180f ? degrees - 360f : degrees;
        }
    }
}
