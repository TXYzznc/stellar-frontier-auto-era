using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Deterministic time mapping shared by the preview UI and its EditMode tests.</summary>
    public static class MotionPreviewTimelineCore
    {
        public const float CycleSeconds = 2f;
        public const float MinimumSpeed = 0.1f;
        public const float MaximumSpeed = 4f;

        public static float ClampSpeed(float speed) => Mathf.Clamp(speed, MinimumSpeed, MaximumSpeed);

        public static float AdvanceProgress(float progress, float deltaSeconds, float speed)
        {
            return Mathf.Repeat(Mathf.Clamp01(progress) + Mathf.Max(0f, deltaSeconds) * ClampSpeed(speed) / CycleSeconds, 1f);
        }

        public static float ElapsedForProgress(float progress) => Mathf.Clamp01(progress) * CycleSeconds;
    }
}
