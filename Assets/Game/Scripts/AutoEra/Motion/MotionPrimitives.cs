using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>
    /// Stateless, deterministic motion mathematics used by the centralized executor.
    /// Every method clamps its normalized input so callers never accumulate overshoot.
    /// </summary>
    public static class MotionPrimitives
    {
        public static Quaternion Rotate(Quaternion bindRotation, Vector3 localAxis, float minimumDegrees, float maximumDegrees, float normalizedProgress)
        {
            float degrees = Mathf.Lerp(minimumDegrees, maximumDegrees, Mathf.Clamp01(normalizedProgress));
            return bindRotation * Quaternion.AngleAxis(degrees, localAxis.normalized);
        }

        public static Vector3 Translate(Vector3 bindPosition, Vector3 localAxis, float minimumDistance, float maximumDistance, float normalizedProgress)
        {
            float distance = Mathf.Lerp(minimumDistance, maximumDistance, Mathf.Clamp01(normalizedProgress));
            return bindPosition + localAxis.normalized * distance;
        }

        public static Quaternion Aim(Quaternion currentRotation, Vector3 origin, Vector3 target, Vector3 localForward, Vector3 worldUp, float normalizedProgress)
        {
            Vector3 direction = target - origin;
            if (direction.sqrMagnitude < 0.000001f || localForward.sqrMagnitude < 0.000001f)
            {
                return currentRotation;
            }

            Quaternion desiredRotation = Quaternion.FromToRotation(currentRotation * localForward.normalized, direction.normalized) * currentRotation;
            return Quaternion.Slerp(currentRotation, desiredRotation, Mathf.Clamp01(normalizedProgress));
        }

        public static float OpenClose(float closedValue, float openValue, float normalizedProgress)
        {
            return Mathf.Lerp(closedValue, openValue, Mathf.Clamp01(normalizedProgress));
        }

        public static Quaternion ContinuousRotate(Quaternion bindRotation, Vector3 localAxis, float degreesPerSecond, float elapsedSeconds)
        {
            return bindRotation * Quaternion.AngleAxis(degreesPerSecond * Mathf.Max(0f, elapsedSeconds), localAxis.normalized);
        }

        public static float Oscillate(float minimumValue, float maximumValue, float frequencyHz, float elapsedSeconds)
        {
            float phase = Mathf.Sin(Mathf.Max(0f, elapsedSeconds) * frequencyHz * Mathf.PI * 2f) * 0.5f + 0.5f;
            return Mathf.Lerp(minimumValue, maximumValue, phase);
        }

        public static bool Wait(float elapsedSeconds, float durationSeconds)
        {
            return Mathf.Max(0f, elapsedSeconds) >= Mathf.Max(0f, durationSeconds);
        }
    }
}
