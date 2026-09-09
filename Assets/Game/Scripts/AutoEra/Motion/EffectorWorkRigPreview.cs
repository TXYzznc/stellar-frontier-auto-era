using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Presentation binding for water, saw and drill mechanisms. Input phases are supplied by the product adapter.</summary>
    public sealed class EffectorWorkRigPreview : MonoBehaviour
    {
        [SerializeField] private EffectorWorkKind _kind;
        [SerializeField] private Transform _yawJoint;
        [SerializeField] private Transform _pitchJoint;
        [SerializeField] private Transform _liftJoint;
        [SerializeField] private Transform _feedJoint;
        [SerializeField] private Transform _rotorJoint;
        [SerializeField] private Transform _valveJoint;
        [Header("可调展示范围")]
        [SerializeField] private Vector2 _yawDegrees = new Vector2(-30f, 30f);
        [SerializeField] private Vector2 _pitchDegrees = new Vector2(-10f, 35f);
        [SerializeField] private float _liftTravel = 0.45f;
        [SerializeField] private Vector3 _liftAxis = Vector3.up;
        [SerializeField] private float _feedTravel = 0.25f;
        [SerializeField] private float _rotorDegreesPerSecond = 720f;
        [SerializeField] private Vector3 _rotorAxis = Vector3.right;
        [SerializeField] private float _valveDegrees = 90f;
        [SerializeField] private LineRenderer _waterArc;
        [SerializeField] private Transform _waterLandingMarker;
        [SerializeField] private ParticleSystem _workParticles;

        private Vector3 _liftBindPosition;
        private Vector3 _feedBindPosition;
        private bool _bindPoseCaptured;

        public void Configure(
            EffectorWorkKind kind,
            Transform yawJoint,
            Transform pitchJoint,
            Transform liftJoint,
            Transform feedJoint,
            Transform rotorJoint,
            Transform valveJoint)
        {
            _kind = kind;
            _yawJoint = yawJoint;
            _pitchJoint = pitchJoint;
            _liftJoint = liftJoint;
            _feedJoint = feedJoint;
            _rotorJoint = rotorJoint;
            _valveJoint = valveJoint;
            _rotorAxis = kind == EffectorWorkKind.DrillMine ? Vector3.up : Vector3.right;
            _liftTravel = kind == EffectorWorkKind.DrillMine ? 1.25f : 0.75f;
            _liftAxis = kind == EffectorWorkKind.DrillMine ? Vector3.down : Vector3.up;
            _feedTravel = kind == EffectorWorkKind.SawCut ? 0.5f : 0f;
            CaptureBindPose();
        }

        public void ConfigureVisualFeedback(LineRenderer waterArc, Transform waterLandingMarker, ParticleSystem workParticles)
        {
            _waterArc = waterArc;
            _waterLandingMarker = waterLandingMarker;
            _workParticles = workParticles;
        }

        public void Apply(EffectorWorkPhase phase, float normalizedProgress, float elapsedSeconds)
        {
            CaptureBindPose();
            float progress = Mathf.Clamp01(normalizedProgress);
            if (_yawJoint != null) _yawJoint.localRotation = Quaternion.Euler(0f, Mathf.Lerp(_yawDegrees.x, _yawDegrees.y, progress), 0f);
            if (_pitchJoint != null) _pitchJoint.localRotation = Quaternion.Euler(Mathf.Lerp(_pitchDegrees.x, _pitchDegrees.y, progress), 0f, 0f);
            if (_kind == EffectorWorkKind.WaterSpray)
            {
                if (_valveJoint != null) _valveJoint.localRotation = Quaternion.Euler(0f, 0f, phase == EffectorWorkPhase.Working ? _valveDegrees : 0f);
                UpdateWaterArc(phase == EffectorWorkPhase.Working);
                return;
            }

            bool elevated = phase == EffectorWorkPhase.Lifting || phase == EffectorWorkPhase.Working || phase == EffectorWorkPhase.Pressing;
            bool feeding = phase == EffectorWorkPhase.Feeding || phase == EffectorWorkPhase.Pressing || phase == EffectorWorkPhase.Working;
            if (_liftJoint != null) _liftJoint.localPosition = _liftBindPosition + _liftAxis * (elevated ? _liftTravel : 0f);
            if (_feedJoint != null) _feedJoint.localPosition = _feedBindPosition + Vector3.forward * (feeding ? _feedTravel : 0f);
            if (_rotorJoint != null && phase != EffectorWorkPhase.Idle && phase != EffectorWorkPhase.Completed)
            {
                _rotorJoint.localRotation = Quaternion.AngleAxis(elapsedSeconds * _rotorDegreesPerSecond, _rotorAxis);
            }
            UpdateParticles(phase == EffectorWorkPhase.Working || phase == EffectorWorkPhase.Pressing || phase == EffectorWorkPhase.Feeding);
        }

        private void UpdateWaterArc(bool active)
        {
            if (_waterArc == null) return;
            _waterArc.enabled = active;
            if (_waterLandingMarker != null) _waterLandingMarker.gameObject.SetActive(active);
            if (!active || _valveJoint == null) return;
            const int pointCount = 12;
            _waterArc.positionCount = pointCount;
            Vector3 origin = _valveJoint.position;
            Vector3 velocity = _valveJoint.forward * 4f + Vector3.up * 2f;
            for (int index = 0; index < pointCount; index++)
            {
                float time = index / (float)(pointCount - 1);
                _waterArc.SetPosition(index, origin + velocity * time + 0.5f * Physics.gravity * time * time);
            }
            if (_waterLandingMarker != null)
            {
                Vector3 landing = _waterArc.GetPosition(pointCount - 1);
                _waterLandingMarker.position = landing;
                float radius = Mathf.Clamp(0.25f + Vector3.Distance(landing, origin) * 0.06f, 0.25f, 0.75f);
                _waterLandingMarker.localScale = new Vector3(radius * 2f, 0.02f, radius * 2f);
            }
        }

        private void UpdateParticles(bool active)
        {
            if (_workParticles == null) return;
            if (active)
            {
                if (!_workParticles.isPlaying) _workParticles.Play(true);
            }
            else if (_workParticles.isPlaying)
            {
                _workParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void CaptureBindPose()
        {
            if (_bindPoseCaptured) return;
            if (_liftJoint != null) _liftBindPosition = _liftJoint.localPosition;
            if (_feedJoint != null) _feedBindPosition = _feedJoint.localPosition;
            _bindPoseCaptured = true;
        }
    }
}
