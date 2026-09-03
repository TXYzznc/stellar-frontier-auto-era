using UnityEngine;

namespace AutoEra.Motion
{
    public enum FourWheelSteeringMode { Normal, CounterSteer, Crab }
    public struct FourWheelPresentationState { public float FrontLeft; public float FrontRight; public float RearLeft; public float RearRight; public float WheelRotationDegrees; }

    public struct FourWheelKinematicsInput
    {
        public float SteeringDegrees;
        public FourWheelSteeringMode SteeringMode;
        public float TravelDistanceMeters;
        public float WheelRadiusMeters;
        public float FrontLeftSuspensionMeters;
        public float FrontRightSuspensionMeters;
        public float RearLeftSuspensionMeters;
        public float RearRightSuspensionMeters;
    }

    public struct WheelPresentationState
    {
        public float SteeringDegrees;
        public float SuspensionMeters;
        public float RollDegrees;
    }

    public struct FourWheelKinematicsState
    {
        public WheelPresentationState FrontLeft;
        public WheelPresentationState FrontRight;
        public WheelPresentationState RearLeft;
        public WheelPresentationState RearRight;
    }

    public static class FourWheelPresentation
    {
        public static FourWheelPresentationState Evaluate(float steerDegrees, FourWheelSteeringMode mode, float speed, float elapsedSeconds)
        {
            float rear = mode == FourWheelSteeringMode.CounterSteer ? -steerDegrees : steerDegrees;
            if (mode == FourWheelSteeringMode.Normal) rear = 0f;
            float rotation = speed * elapsedSeconds * 360f;
            return new FourWheelPresentationState { FrontLeft = steerDegrees, FrontRight = steerDegrees, RearLeft = rear, RearRight = rear, WheelRotationDegrees = rotation };
        }

        public static FourWheelKinematicsState EvaluateKinematics(FourWheelKinematicsInput input)
        {
            float rearSteering = input.SteeringMode == FourWheelSteeringMode.CounterSteer
                ? -input.SteeringDegrees
                : input.SteeringDegrees;
            if (input.SteeringMode == FourWheelSteeringMode.Normal)
            {
                rearSteering = 0f;
            }

            float wheelCircumference = 2f * Mathf.PI * Mathf.Max(0.0001f, input.WheelRadiusMeters);
            float rollDegrees = input.TravelDistanceMeters / wheelCircumference * 360f;
            return new FourWheelKinematicsState
            {
                FrontLeft = CreateWheel(input.SteeringDegrees, input.FrontLeftSuspensionMeters, rollDegrees),
                FrontRight = CreateWheel(input.SteeringDegrees, input.FrontRightSuspensionMeters, rollDegrees),
                RearLeft = CreateWheel(rearSteering, input.RearLeftSuspensionMeters, rollDegrees),
                RearRight = CreateWheel(rearSteering, input.RearRightSuspensionMeters, rollDegrees)
            };
        }

        private static WheelPresentationState CreateWheel(float steeringDegrees, float suspensionMeters, float rollDegrees)
        {
            return new WheelPresentationState
            {
                SteeringDegrees = steeringDegrees,
                SuspensionMeters = suspensionMeters,
                RollDegrees = rollDegrees
            };
        }
    }
}
