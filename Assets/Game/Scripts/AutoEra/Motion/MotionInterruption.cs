using UnityEngine;

namespace AutoEra.Motion
{
    public enum MotionInterruptionPolicy { Hold, Retract, Reset, ImmediateStop }

    public readonly struct MotionLocalPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public MotionLocalPose(Vector3 position, Quaternion rotation) { Position = position; Rotation = rotation; }
    }

    public static class MotionInterruption
    {
        public static MotionLocalPose Resolve(MotionInterruptionPolicy policy, MotionJointBinding binding, MotionLocalPose measuredPose, float normalizedRecovery)
        {
            if (policy == MotionInterruptionPolicy.Hold || policy == MotionInterruptionPolicy.ImmediateStop)
            {
                return measuredPose;
            }
            MotionLocalPose target = policy == MotionInterruptionPolicy.Retract
                ? new MotionLocalPose(binding.SafeLocalPosition, binding.SafeLocalRotation)
                : new MotionLocalPose(binding.BindLocalPosition, binding.BindLocalRotation);
            float progress = Mathf.Clamp01(normalizedRecovery);
            return new MotionLocalPose(Vector3.Lerp(measuredPose.Position, target.Position, progress), Quaternion.Slerp(measuredPose.Rotation, target.Rotation, progress));
        }
    }
}
