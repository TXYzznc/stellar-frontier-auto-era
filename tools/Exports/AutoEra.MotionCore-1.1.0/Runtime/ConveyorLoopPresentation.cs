using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Pure geometric contract shared by the conveyor prototype builder and its acceptance presenter.</summary>
    public static class ConveyorLoopPresentation
    {
        public const float HalfWidth = 1.2f;
        public const float HalfStraightLength = 2.5f;
        public const float ReturnRadius = 0.3f;
        public const float TreadSpeedMetersPerSecond = 0.9f;
        public const float DriveRollerRadius = 0.25f;
        public const float SupportRollerRadius = 0.14f;

        public static float LoopLength => HalfStraightLength * 4f + Mathf.PI * ReturnRadius * 2f;

        public static void EvaluateLoopPose(float normalizedDistance, out Vector3 position, out Quaternion rotation)
        {
            float distance = Mathf.Repeat(normalizedDistance, 1f) * LoopLength;
            float upperLength = HalfStraightLength * 2f;
            float arcLength = Mathf.PI * ReturnRadius;

            if (distance < upperLength)
            {
                position = new Vector3(0f, ReturnRadius, -HalfStraightLength + distance);
                rotation = Quaternion.identity;
                return;
            }

            distance -= upperLength;
            if (distance < arcLength)
            {
                float angle = distance / ReturnRadius;
                position = new Vector3(0f, Mathf.Cos(angle) * ReturnRadius, HalfStraightLength + Mathf.Sin(angle) * ReturnRadius);
                rotation = Quaternion.AngleAxis(angle, Vector3.right);
                return;
            }

            distance -= arcLength;
            if (distance < upperLength)
            {
                position = new Vector3(0f, -ReturnRadius, HalfStraightLength - distance);
                rotation = Quaternion.Euler(180f, 0f, 0f);
                return;
            }

            distance -= upperLength;
            float rearAngle = Mathf.PI + distance / ReturnRadius;
            position = new Vector3(0f, Mathf.Cos(rearAngle) * ReturnRadius, -HalfStraightLength + Mathf.Sin(rearAngle) * ReturnRadius);
            rotation = Quaternion.AngleAxis(rearAngle, Vector3.right);
        }

        public static Vector3 EvaluateCargoPosition(float normalizedProgress)
        {
            float progress = Mathf.Repeat(normalizedProgress, 1f);
            return new Vector3(0f, ReturnRadius + 0.22f, Mathf.Lerp(-HalfStraightLength + 0.3f, HalfStraightLength - 0.3f, progress));
        }

        public static float EvaluateRollerDegrees(float elapsedSeconds)
        {
            return EvaluateRollerDegrees(elapsedSeconds, DriveRollerRadius);
        }

        public static float EvaluateRollerDegrees(float elapsedSeconds, float rollerRadiusMeters)
        {
            return elapsedSeconds * TreadSpeedMetersPerSecond / rollerRadiusMeters * Mathf.Rad2Deg;
        }
    }
}
