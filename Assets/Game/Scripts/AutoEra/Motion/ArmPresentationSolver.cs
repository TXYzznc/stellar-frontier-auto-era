using UnityEngine;

namespace AutoEra.Motion
{
    public struct ArmPresentationSolution
    {
        public bool Reachable;
        public bool RequiresReposition;
        public bool IsSafeRetract;
        public float YawDegrees;
        public float PitchDegrees;
        public float Extension;
        public Pose TargetPose;
        public Quaternion WristLocalRotation;
    }

    public static class ArmPresentationSolver
    {
        public static ArmPresentationSolution Solve(Vector3 targetLocal, float baseReach, float maxExtension)
        {
            return Solve(new Pose(targetLocal, Quaternion.identity), baseReach, maxExtension, 0f);
        }

        public static ArmPresentationSolution Solve(Pose targetPose, float baseReach, float maxExtension, float keepOutRadius)
        {
            Vector3 targetLocal = targetPose.position;
            float planar = new Vector2(targetLocal.x, targetLocal.z).magnitude;
            float distance = targetLocal.magnitude;
            float extension = Mathf.Max(0f, distance - baseReach);
            bool requiresReposition = distance < keepOutRadius || extension > maxExtension;
            if (requiresReposition)
            {
                return new ArmPresentationSolution
                {
                    Reachable = false,
                    RequiresReposition = true,
                    TargetPose = targetPose,
                    WristLocalRotation = targetPose.rotation
                };
            }

            return new ArmPresentationSolution
            {
                Reachable = true,
                RequiresReposition = false,
                YawDegrees = Mathf.Atan2(targetLocal.x, targetLocal.z) * Mathf.Rad2Deg,
                PitchDegrees = -Mathf.Atan2(targetLocal.y, Mathf.Max(0.0001f, planar)) * Mathf.Rad2Deg,
                Extension = extension,
                TargetPose = targetPose,
                WristLocalRotation = targetPose.rotation
            };
        }

        public static ArmPresentationSolution CreateSafeRetract(float yawDegrees)
        {
            return new ArmPresentationSolution
            {
                Reachable = true,
                IsSafeRetract = true,
                YawDegrees = yawDegrees,
                PitchDegrees = 0f,
                Extension = 0f,
                TargetPose = new Pose(Vector3.zero, Quaternion.identity),
                WristLocalRotation = Quaternion.identity
            };
        }
    }
}
