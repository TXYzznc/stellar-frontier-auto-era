using UnityEngine;

namespace AutoEra.Motion
{
    public enum FourWheelSteeringMode { Normal, CounterSteer, Crab }
    public struct FourWheelPresentationState { public float FrontLeft; public float FrontRight; public float RearLeft; public float RearRight; public float WheelRotationDegrees; }
    public static class FourWheelPresentation
    {
        public static FourWheelPresentationState Evaluate(float steerDegrees, FourWheelSteeringMode mode, float speed, float elapsedSeconds)
        {
            float rear = mode == FourWheelSteeringMode.CounterSteer ? -steerDegrees : steerDegrees;
            if (mode == FourWheelSteeringMode.Normal) rear = 0f;
            float rotation = speed * elapsedSeconds * 360f;
            return new FourWheelPresentationState { FrontLeft = steerDegrees, FrontRight = steerDegrees, RearLeft = rear, RearRight = rear, WheelRotationDegrees = rotation };
        }
    }
}
