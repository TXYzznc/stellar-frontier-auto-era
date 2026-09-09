using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Independent acceptance-scene presenter. It only animates assigned prototype RigRoots and restores their bind poses when disabled.</summary>
    public sealed class FunctionalRigAcceptanceDemoDirector : MonoBehaviour
    {
        [SerializeField] private Transform _fourWheelRig;
        [SerializeField] private Transform _carrierRig;
        [SerializeField] private Transform _armRig;
        [SerializeField] private Transform _doorRig;
        [SerializeField] private Transform _conveyorRig;
        [SerializeField] private Transform _waterRig;
        [SerializeField] private Transform _sawRig;
        [SerializeField] private Transform _drillRig;
        [SerializeField] private Transform _cargoRig;
        [SerializeField] private Transform _fixedRotaryRig;

        private Quaternion _fourWheelBindRotation;
        private Quaternion _armBindRotation;
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
        private Transform _armBaseYaw;
        private Transform _armShoulder;
        private Transform _armElbow;
        private Transform _armWristPitch;
        private Transform _armWristRoll;
        private Transform _leftDoorLeaf;
        private Transform _rightDoorLeaf;
        private Transform _driveRoller;
        private Transform _tailRoller;
        private Transform _upperIdlerFront;
        private Transform _upperIdlerRear;
        private Transform _lowerIdlerFront;
        private Transform _lowerIdlerRear;
        private EffectorWorkRigPreview _waterPreview;
        private EffectorWorkRigPreview _sawPreview;
        private EffectorWorkRigPreview _drillPreview;
        private CargoBayPreview _cargoPreview;
        private Transform _cargoLeftDoor;
        private Transform _cargoRightDoor;
        private Transform _fixedYaw;

        public void Configure(
            Transform fourWheelRig, Transform carrierRig, Transform armRig, Transform doorRig, Transform conveyorRig,
            Transform waterRig, Transform sawRig, Transform drillRig, Transform cargoRig, Transform fixedRotaryRig)
        {
            _fourWheelRig = fourWheelRig; _carrierRig = carrierRig; _armRig = armRig; _doorRig = doorRig; _conveyorRig = conveyorRig;
            _waterRig = waterRig; _sawRig = sawRig; _drillRig = drillRig; _cargoRig = cargoRig; _fixedRotaryRig = fixedRotaryRig;
        }

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
            _armBaseYaw = FindJoint(_armRig, "base_yaw");
            _armShoulder = FindJoint(_armRig, "shoulder_pitch");
            _armElbow = FindJoint(_armRig, "elbow_pitch");
            _armWristPitch = FindJoint(_armRig, "wrist_pitch");
            _armWristRoll = FindJoint(_armRig, "wrist_roll");
            _leftDoorLeaf = FindJoint(_doorRig, "left_leaf");
            _rightDoorLeaf = FindJoint(_doorRig, "right_leaf");
            _driveRoller = FindJoint(_conveyorRig, "drive_roller");
            _tailRoller = FindJoint(_conveyorRig, "tail_roller");
            _upperIdlerFront = FindJoint(_conveyorRig, "upper_idler_front");
            _upperIdlerRear = FindJoint(_conveyorRig, "upper_idler_rear");
            _lowerIdlerFront = FindJoint(_conveyorRig, "lower_idler_front");
            _lowerIdlerRear = FindJoint(_conveyorRig, "lower_idler_rear");
            _waterPreview = _waterRig == null ? null : _waterRig.GetComponent<EffectorWorkRigPreview>();
            _sawPreview = _sawRig == null ? null : _sawRig.GetComponent<EffectorWorkRigPreview>();
            _drillPreview = _drillRig == null ? null : _drillRig.GetComponent<EffectorWorkRigPreview>();
            _cargoPreview = _cargoRig == null ? null : _cargoRig.GetComponent<CargoBayPreview>();
            _cargoLeftDoor = FindJoint(_cargoRig, "left_door");
            _cargoRightDoor = FindJoint(_cargoRig, "right_door");
            _fixedYaw = FindJoint(_fixedRotaryRig, "yaw_pivot");
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

            ArmPresentationSolution arm = ArmPresentationSolver.Solve(new Pose(new Vector3(Mathf.Sin(_elapsed) * 1.5f, 0.5f, 2f), Quaternion.Euler(0f, 0f, Mathf.Sin(_elapsed) * 35f)), ArmPresentationConfiguration.Default);
            if (_armBaseYaw != null) _armBaseYaw.localRotation = Quaternion.Euler(0f, arm.BaseYawDegrees, 0f);
            if (_armShoulder != null) _armShoulder.localRotation = Quaternion.Euler(arm.ShoulderPitchDegrees, 0f, 0f);
            if (_armElbow != null) _armElbow.localRotation = Quaternion.Euler(arm.ElbowPitchDegrees, 0f, 0f);
            if (_armWristPitch != null) _armWristPitch.localRotation = Quaternion.Euler(arm.WristPitchDegrees, 0f, 0f);
            if (_armWristRoll != null) _armWristRoll.localRotation = Quaternion.Euler(0f, 0f, arm.WristRollDegrees);

            SlidingDoorPresentationState door = SlidingDoorPresentation.Evaluate(SlidingDoorPresentationMode.DoublePanel, cycle < 0.5f, false, cycle < 0.5f ? cycle * 2f : (cycle - 0.5f) * 2f);
            if (_leftDoorLeaf != null) _leftDoorLeaf.localPosition = new Vector3(-1.15f - door.FirstPanelOpen * 1.2f, 0f, 0f);
            if (_rightDoorLeaf != null) _rightDoorLeaf.localPosition = new Vector3(1.15f + door.SecondPanelOpen * 1.2f, 0f, 0f);

            float driveRollerDegrees = ConveyorLoopPresentation.EvaluateRollerDegrees(_elapsed, ConveyorLoopPresentation.DriveRollerRadius);
            float supportRollerDegrees = ConveyorLoopPresentation.EvaluateRollerDegrees(_elapsed, ConveyorLoopPresentation.SupportRollerRadius);

            if (_driveRoller != null) _driveRoller.localRotation = Quaternion.Euler(driveRollerDegrees, 0f, 0f);
            if (_tailRoller != null) _tailRoller.localRotation = Quaternion.Euler(driveRollerDegrees, 0f, 0f);
            if (_upperIdlerFront != null) _upperIdlerFront.localRotation = Quaternion.Euler(supportRollerDegrees, 0f, 0f);
            if (_upperIdlerRear != null) _upperIdlerRear.localRotation = Quaternion.Euler(supportRollerDegrees, 0f, 0f);
            if (_lowerIdlerFront != null) _lowerIdlerFront.localRotation = Quaternion.Euler(supportRollerDegrees, 0f, 0f);
            if (_lowerIdlerRear != null) _lowerIdlerRear.localRotation = Quaternion.Euler(supportRollerDegrees, 0f, 0f);

            if (_waterPreview != null) _waterPreview.Apply(cycle < 0.7f ? EffectorWorkPhase.Working : EffectorWorkPhase.ValveClosing, cycle, _elapsed);
            if (_sawPreview != null) _sawPreview.Apply(cycle < 0.75f ? EffectorWorkPhase.Working : EffectorWorkPhase.Retracting, cycle, _elapsed);
            if (_drillPreview != null) _drillPreview.Apply(cycle < 0.75f ? EffectorWorkPhase.Pressing : EffectorWorkPhase.Retracting, cycle, _elapsed);
            float cargoTransfer = cycle < 0.25f ? cycle * 4f : (cycle < 0.75f ? 1f : 4f - cycle * 4f);
            if (_cargoPreview != null) _cargoPreview.ApplyTransfer(Mathf.PingPong(_elapsed * 0.22f, 1f), cargoTransfer);
            else
            {
                if (_cargoLeftDoor != null) _cargoLeftDoor.localPosition = new Vector3(-0.72f - cycle * 0.75f, 0f, 1.45f);
                if (_cargoRightDoor != null) _cargoRightDoor.localPosition = new Vector3(0.72f + cycle * 0.75f, 0f, 1.45f);
            }
            if (_fixedYaw != null) _fixedYaw.localRotation = Quaternion.Euler(0f, _elapsed * 45f, 0f);
        }

        private void OnDisable()
        {
            RestoreBindPose();
            for (int index = 0; index < _animatedJointCount; index++)
            {
                Transform joint = _animatedJoints[index];
                if (joint == null) continue;
                joint.localPosition = _jointBindPositions[index];
                joint.localRotation = _jointBindRotations[index];
            }
        }

        private void CaptureBindPose()
        {
            if (_fourWheelRig != null) _fourWheelBindRotation = _fourWheelRig.localRotation;
            if (_armRig != null) _armBindRotation = _armRig.localRotation;
            if (_doorRig != null) _doorBindPosition = _doorRig.localPosition;
            if (_conveyorRig != null) _conveyorBindPosition = _conveyorRig.localPosition;
        }

        private void RestoreBindPose()
        {
            if (_fourWheelRig != null) _fourWheelRig.localRotation = _fourWheelBindRotation;
            if (_armRig != null) _armRig.localRotation = _armBindRotation;
            if (_doorRig != null) _doorRig.localPosition = _doorBindPosition;
            if (_conveyorRig != null) _conveyorRig.localPosition = _conveyorBindPosition;
        }

        private Transform FindJoint(Transform rigRoot, string stableId)
        {
            if (rigRoot == null) return null;
            Transform joint = rigRoot.Find("RigRoot/VisualRoot/Joint_" + stableId);
            if (joint == null)
            {
                foreach (Transform candidate in rigRoot.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate.name == stableId || candidate.name == "Joint_" + stableId)
                    {
                        joint = candidate;
                        break;
                    }
                }
            }
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
