using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Independent acceptance-scene presenter. It only animates assigned prototype RigRoots and restores their bind poses when disabled.</summary>
    public sealed class FunctionalRigAcceptanceDemoDirector : MonoBehaviour
    {
        [SerializeField] private Transform _fourWheelRig;
        [SerializeField] private Transform _carrierRig;
        [SerializeField] private Transform _armRig;
        [SerializeField] private Transform _effectorRig;
        [SerializeField] private Transform _doorRig;
        [SerializeField] private Transform _conveyorRig;

        private Quaternion _fourWheelBindRotation;
        private Quaternion _armBindRotation;
        private Vector3 _effectorBindPosition;
        private Vector3 _doorBindPosition;
        private Vector3 _conveyorBindPosition;
        private float _elapsed;
        private readonly Transform[] _animatedJoints = new Transform[32];
        private readonly Vector3[] _jointBindPositions = new Vector3[32];
        private readonly Quaternion[] _jointBindRotations = new Quaternion[32];
        private int _animatedJointCount;
        private Transform _carrierChassis;
        private readonly Transform[] _carrierSteering = new Transform[4];
        private readonly Transform[] _carrierSuspension = new Transform[4];
        private readonly Transform[] _carrierRoll = new Transform[4];
        private Transform _fourWheelSteer;
        private Transform _fourWheelRoll;
        private Transform _armYaw;
        private Transform _armShoulder;
        private Transform _armExtend;
        private Transform _armWrist;
        private Transform _effectorSocket;
        private Transform _effectorLock;
        private Transform _effectorHold;
        private Transform _leftDoorLeaf;
        private Transform _rightDoorLeaf;
        private Transform _driveRoller;
        private Transform _tailRoller;
        private Transform _belt;

        private void Awake()
        {
            CaptureBindPose();
            _carrierChassis = FindJoint(_carrierRig, "chassis");
            BindCarrierWheel(0, "front_left");
            BindCarrierWheel(1, "front_right");
            BindCarrierWheel(2, "rear_left");
            BindCarrierWheel(3, "rear_right");
            _fourWheelSteer = FindJoint(_fourWheelRig, "steer");
            _fourWheelRoll = FindJoint(_fourWheelRig, "roll");
            _armYaw = FindJoint(_armRig, "yaw");
            _armShoulder = FindJoint(_armRig, "shoulder");
            _armExtend = FindJoint(_armRig, "extend");
            _armWrist = FindJoint(_armRig, "wrist");
            _effectorSocket = FindJoint(_effectorRig, "socket");
            _effectorLock = FindJoint(_effectorRig, "lock");
            _effectorHold = FindJoint(_effectorRig, "safety_hold");
            _leftDoorLeaf = FindJoint(_doorRig, "left_leaf");
            _rightDoorLeaf = FindJoint(_doorRig, "right_leaf");
            _driveRoller = FindJoint(_conveyorRig, "drive_roller");
            _tailRoller = FindJoint(_conveyorRig, "tail_roller");
            _belt = FindJoint(_conveyorRig, "belt");
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float cycle = Mathf.Repeat(_elapsed / 6f, 1f);

            FourWheelKinematicsState carrierWheels = FourWheelPresentation.EvaluateKinematics(new FourWheelKinematicsInput
            {
                SteeringDegrees = 25f,
                SteeringMode = FourWheelSteeringMode.CounterSteer,
                TravelDistanceMeters = _elapsed,
                WheelRadiusMeters = 0.7f,
                FrontLeftSuspensionMeters = Mathf.Sin(_elapsed * 1.2f) * -0.08f,
                FrontRightSuspensionMeters = Mathf.Sin(_elapsed * 1.2f + 0.7f) * -0.08f,
                RearLeftSuspensionMeters = Mathf.Sin(_elapsed * 1.2f + 1.4f) * -0.08f,
                RearRightSuspensionMeters = Mathf.Sin(_elapsed * 1.2f + 2.1f) * -0.08f
            });
            ApplyCarrierWheel(0, carrierWheels.FrontLeft);
            ApplyCarrierWheel(1, carrierWheels.FrontRight);
            ApplyCarrierWheel(2, carrierWheels.RearLeft);
            ApplyCarrierWheel(3, carrierWheels.RearRight);

            FourWheelPresentationState wheel = FourWheelPresentation.Evaluate(25f, FourWheelSteeringMode.CounterSteer, 1f, _elapsed);
            if (_fourWheelSteer != null) _fourWheelSteer.localRotation = Quaternion.Euler(0f, wheel.FrontLeft, 0f);
            if (_fourWheelRoll != null) _fourWheelRoll.localRotation = Quaternion.Euler(wheel.WheelRotationDegrees, 0f, 0f);

            ArmPresentationSolution arm = ArmPresentationSolver.Solve(new Vector3(Mathf.Sin(_elapsed) * 1.5f, 0.5f, 2f), 2f, 2f);
            if (_armYaw != null) _armYaw.localRotation = Quaternion.Euler(0f, arm.YawDegrees, 0f);
            if (_armShoulder != null) _armShoulder.localRotation = Quaternion.Euler(arm.PitchDegrees, 0f, 0f);
            if (_armExtend != null) _armExtend.localPosition = new Vector3(0f, 0f, 1.1f + arm.Extension);
            if (_armWrist != null) _armWrist.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_elapsed) * 35f);

            float effectorPhase = Mathf.Repeat(_elapsed / 4f, 1f);
            if (_effectorSocket != null) _effectorSocket.localPosition = new Vector3(0f, 0f, 0.45f + effectorPhase * 0.35f);
            if (_effectorLock != null) _effectorLock.localRotation = Quaternion.Euler(0f, 0f, effectorPhase * 90f);
            if (_effectorHold != null) _effectorHold.localPosition = new Vector3(0f, effectorPhase > 0.8f ? -0.2f : 0f, 0.3f);

            SlidingDoorPresentationState door = SlidingDoorPresentation.Evaluate(SlidingDoorPresentationMode.DoublePanel, cycle < 0.5f, false, cycle < 0.5f ? cycle * 2f : (cycle - 0.5f) * 2f);
            if (_leftDoorLeaf != null) _leftDoorLeaf.localPosition = new Vector3(-1.15f - door.FirstPanelOpen * 1.2f, 0f, 0f);
            if (_rightDoorLeaf != null) _rightDoorLeaf.localPosition = new Vector3(1.15f + door.SecondPanelOpen * 1.2f, 0f, 0f);

            ConveyorPresentationState conveyor = ConveyorPresentation.Advance(0f, 1f, 1f, false, _elapsed);
            if (_driveRoller != null) _driveRoller.localRotation = Quaternion.Euler(_elapsed * 240f, 0f, 0f);
            if (_tailRoller != null) _tailRoller.localRotation = Quaternion.Euler(_elapsed * 240f, 0f, 0f);
            if (_belt != null) _belt.localPosition = new Vector3(0f, 0.3f, conveyor.UvOffset * 0.35f);
        }

        private void OnDisable()
        {
            RestoreBindPose();
            for (int index = 0; index < _animatedJointCount; index++)
            {
                _animatedJoints[index].localPosition = _jointBindPositions[index];
                _animatedJoints[index].localRotation = _jointBindRotations[index];
            }
        }

        private void CaptureBindPose()
        {
            if (_fourWheelRig != null) _fourWheelBindRotation = _fourWheelRig.localRotation;
            if (_armRig != null) _armBindRotation = _armRig.localRotation;
            if (_effectorRig != null) _effectorBindPosition = _effectorRig.localPosition;
            if (_doorRig != null) _doorBindPosition = _doorRig.localPosition;
            if (_conveyorRig != null) _conveyorBindPosition = _conveyorRig.localPosition;
        }

        private void RestoreBindPose()
        {
            if (_fourWheelRig != null) _fourWheelRig.localRotation = _fourWheelBindRotation;
            if (_armRig != null) _armRig.localRotation = _armBindRotation;
            if (_effectorRig != null) _effectorRig.localPosition = _effectorBindPosition;
            if (_doorRig != null) _doorRig.localPosition = _doorBindPosition;
            if (_conveyorRig != null) _conveyorRig.localPosition = _conveyorBindPosition;
        }

        private Transform FindJoint(Transform rigRoot, string stableId)
        {
            if (rigRoot == null) return null;
            Transform joint = rigRoot.Find("VisualRoot/Joint_" + stableId);
            if (joint != null && _animatedJointCount < _animatedJoints.Length)
            {
                _animatedJoints[_animatedJointCount] = joint;
                _jointBindPositions[_animatedJointCount] = joint.localPosition;
                _jointBindRotations[_animatedJointCount] = joint.localRotation;
                _animatedJointCount++;
            }

            return joint;
        }

        private void BindCarrierWheel(int index, string prefix)
        {
            _carrierSteering[index] = FindJoint(_carrierRig, prefix + "_steer");
            _carrierSuspension[index] = FindJoint(_carrierRig, prefix + "_suspension");
            _carrierRoll[index] = FindJoint(_carrierRig, prefix + "_roll");
        }

        private void ApplyCarrierWheel(int index, WheelPresentationState wheel)
        {
            Transform steering = _carrierSteering[index];
            if (steering != null) steering.localRotation = Quaternion.Euler(0f, wheel.SteeringDegrees, 0f);

            Transform suspension = _carrierSuspension[index];
            if (suspension != null) suspension.localPosition = new Vector3(0f, wheel.SuspensionMeters, 0f);

            Transform roll = _carrierRoll[index];
            if (roll != null) roll.localRotation = Quaternion.Euler(wheel.RollDegrees, 0f, 0f);
        }
    }
}
