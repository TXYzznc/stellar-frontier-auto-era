using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Independent acceptance-scene presenter. It only animates assigned prototype RigRoots and restores their bind poses when disabled.</summary>
    public sealed class FunctionalRigAcceptanceDemoDirector : MonoBehaviour
    {
        [SerializeField] private Transform _fourWheelRig;
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

        private void Awake()
        {
            CaptureBindPose();
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float cycle = Mathf.Repeat(_elapsed / 6f, 1f);

            if (_fourWheelRig != null)
            {
                FourWheelPresentationState wheel = FourWheelPresentation.Evaluate(25f, FourWheelSteeringMode.CounterSteer, 1f, _elapsed);
                _fourWheelRig.localRotation = _fourWheelBindRotation * Quaternion.Euler(0f, wheel.FrontLeft * 0.2f, 0f);
            }

            if (_armRig != null)
            {
                ArmPresentationSolution arm = ArmPresentationSolver.Solve(new Vector3(Mathf.Sin(_elapsed) * 1.5f, 0.5f, 2f), 2f, 2f);
                _armRig.localRotation = _armBindRotation * Quaternion.Euler(arm.PitchDegrees * 0.15f, arm.YawDegrees * 0.15f, 0f);
            }

            if (_effectorRig != null)
            {
                float offset = Mathf.Sin(_elapsed * 2f) * 0.2f;
                _effectorRig.localPosition = _effectorBindPosition + new Vector3(0f, offset, 0f);
            }

            if (_doorRig != null)
            {
                SlidingDoorPresentationState door = SlidingDoorPresentation.Evaluate(
                    SlidingDoorPresentationMode.DoublePanel, cycle < 0.5f, false, cycle < 0.5f ? cycle * 2f : (cycle - 0.5f) * 2f);
                _doorRig.localPosition = _doorBindPosition + new Vector3(door.FirstPanelOpen * 0.25f, 0f, 0f);
            }

            if (_conveyorRig != null)
            {
                ConveyorPresentationState conveyor = ConveyorPresentation.Advance(0f, 1f, 1f, false, _elapsed);
                _conveyorRig.localPosition = _conveyorBindPosition + new Vector3(conveyor.UvOffset * 0.15f, 0f, 0f);
            }
        }

        private void OnDisable()
        {
            RestoreBindPose();
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
    }
}
