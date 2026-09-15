using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed class RegionCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector3 _focus;
        [SerializeField] private float _distance = 32f;
        [SerializeField] private float _yaw;
        [SerializeField] private float _pitch = 55f;
        [SerializeField] private float _panSpeed = 15f;
        [SerializeField] private float _rotationSpeed = 3f;
        [SerializeField] private float _zoomSpeed = 5f;
        public Camera ViewCamera => _camera;
        public Vector3 FocusPosition => _focus;

        public void Apply(Vector2 pan, Vector2 orbit, float zoom, float seconds, Rect bounds)
        {
            _yaw += orbit.x * _rotationSpeed;
            _pitch = Mathf.Clamp(_pitch - orbit.y * _rotationSpeed, 25, 80);
            _distance = Mathf.Clamp(_distance - zoom * _zoomSpeed, 8, 65);
            Vector3 motion = Quaternion.Euler(0, _yaw, 0) * new Vector3(pan.x, 0, pan.y);
            _focus += Vector3.ClampMagnitude(motion, 1f) * (_panSpeed * Mathf.Max(0, seconds));
            _focus.x = Mathf.Clamp(_focus.x, bounds.xMin, bounds.xMax);
            _focus.z = Mathf.Clamp(_focus.z, bounds.yMin, bounds.yMax);
            RefreshPose();
        }

        public void Focus(Vector3 position) { _focus = position; RefreshPose(); }

        private void RefreshPose()
        {
            if (_camera == null) return;
            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0);
            _camera.transform.SetPositionAndRotation(_focus - rotation * Vector3.forward * _distance, rotation);
        }
    }
}
