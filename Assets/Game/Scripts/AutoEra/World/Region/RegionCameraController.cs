using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed class RegionCameraController : MonoBehaviour, IRegionCameraTarget
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector3 _focus;
        [SerializeField] private float _distance = 32f;
        [SerializeField] private float _yaw;
        [SerializeField] private float _pitch = 55f;
        [SerializeField] private float _panSpeed = RegionCameraParameters.DefaultPanSpeed;
        [SerializeField] private float _rotationSpeed = RegionCameraParameters.DefaultRotationSpeed;
        [SerializeField] private float _zoomSpeed = RegionCameraParameters.DefaultZoomSpeed;
        [SerializeField] private bool _invertHorizontal;
        [SerializeField] private bool _invertVertical;
        public Camera ViewCamera => _camera;
        public Vector3 FocusPosition => _focus;

        /// <summary>
        /// 镜头参数（设置页的写入目标）。赋值一律进区间：
        /// 参数是玩家可调的，越界值会让镜头表现与界面显示不一致。
        /// </summary>
        public float PanSpeed
        {
            get => _panSpeed;
            set => _panSpeed = RegionCameraParameters.ClampPan(value);
        }

        public float RotationSpeed
        {
            get => _rotationSpeed;
            set => _rotationSpeed = RegionCameraParameters.ClampRotation(value);
        }

        public float ZoomSpeed
        {
            get => _zoomSpeed;
            set => _zoomSpeed = RegionCameraParameters.ClampZoom(value);
        }

        /// <summary>水平反转：轨道旋转的水平方向与平移的水平方向一起取反。</summary>
        public bool InvertHorizontal
        {
            get => _invertHorizontal;
            set => _invertHorizontal = value;
        }

        /// <summary>垂直反转：轨道旋转的垂直方向与滚轮缩放方向一起取反。</summary>
        public bool InvertVertical
        {
            get => _invertVertical;
            set => _invertVertical = value;
        }

        public void Apply(Vector2 pan, Vector2 orbit, float zoom, float seconds, Rect bounds)
        {
            float horizontal = _invertHorizontal ? -1f : 1f;
            float vertical = _invertVertical ? -1f : 1f;
            _yaw += orbit.x * _rotationSpeed * horizontal;
            _pitch = Mathf.Clamp(_pitch - orbit.y * _rotationSpeed * vertical, 25, 80);
            _distance = Mathf.Clamp(_distance - zoom * _zoomSpeed * vertical, 8, 65);
            Vector3 motion = Quaternion.Euler(0, _yaw, 0) * new Vector3(pan.x * horizontal, 0, pan.y);
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
