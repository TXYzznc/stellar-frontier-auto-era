using System;
using UnityEngine;

namespace AutoEra.Motion.Adapter
{
    /// <summary>Projects measured carrier displacement; never moves the carrier or completes a task.</summary>
    public sealed class MachineNavigationMotionAdapter
    {
        private readonly MotionJointBinding[] _roll = new MotionJointBinding[4];
        private readonly MotionJointBinding[] _steer = new MotionJointBinding[4];
        private readonly float _wheelRadius, _wheelBase;
        private Vector3 _previous;
        private float _previousYaw, _travel;
        public float TravelDistance => _travel;
        public float RollDegrees { get; private set; }
        public MachineNavigationMotionAdapter(MotionRig rig, Vector3 initialPosition, float initialYaw, float wheelRadius, float wheelBase)
        {
            if (rig == null || wheelRadius <= 0 || wheelBase <= 0 || float.IsInfinity(wheelRadius + wheelBase) || float.IsNaN(wheelRadius + wheelBase))
                throw new ArgumentException("Invalid wheel motion binding.");
            string[] prefixes = { "front_left", "front_right", "rear_left", "rear_right" };
            for (int i = 0; i < prefixes.Length; i++)
                if (!rig.TryGetBinding(prefixes[i] + "_roll", out _roll[i]) || !rig.TryGetBinding(prefixes[i] + "_steer", out _steer[i]))
                    throw new ArgumentException("Formal carrier is missing wheel joints.");
            _wheelRadius = wheelRadius; _wheelBase = wheelBase; _previous = initialPosition; _previousYaw = initialYaw;
        }
        public void Sample(Vector3 position, float yaw)
        {
            Vector3 delta = position - _previous;
            float distance = delta.magnitude;
            float sign = Vector3.Dot(delta, Quaternion.Euler(0, yaw, 0) * Vector3.forward) < 0 ? -1 : 1;
            _travel += distance * sign;
            float steering = distance > 0.0001f ? Mathf.Atan(_wheelBase * Mathf.DeltaAngle(_previousYaw, yaw) * Mathf.Deg2Rad / distance) * Mathf.Rad2Deg : 0;
            var wheels = FourWheelPresentation.EvaluateKinematics(new FourWheelKinematicsInput
            { TravelDistanceMeters = _travel, WheelRadiusMeters = _wheelRadius, SteeringDegrees = steering, SteeringMode = FourWheelSteeringMode.Normal });
            RollDegrees = wheels.FrontLeft.RollDegrees;
            for (int i = 0; i < 4; i++)
            {
                _roll[i].JointTransform.localRotation = _roll[i].BindLocalRotation * Quaternion.AngleAxis(RollDegrees % 360f, _roll[i].LocalAxis);
                float value = Mathf.Clamp(i < 2 ? steering : 0, _steer[i].MinimumValue, _steer[i].MaximumValue);
                _steer[i].JointTransform.localRotation = _steer[i].BindLocalRotation * Quaternion.AngleAxis(value, _steer[i].LocalAxis);
            }
            _previous = position; _previousYaw = yaw;
        }
    }
}
