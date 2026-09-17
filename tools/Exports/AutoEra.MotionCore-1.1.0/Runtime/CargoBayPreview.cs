using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Presentation-only capacity indicator for a machine's single aggregate container.</summary>
    public sealed class CargoBayPreview : MonoBehaviour
    {
        [SerializeField] private Transform _leftDoor;
        [SerializeField] private Transform _rightDoor;
        [SerializeField] private Transform _transferTray;
        [SerializeField] private Transform _loadVisual;
        [SerializeField] private float _doorTravel = 0.45f;
        [SerializeField] private float _trayTravel = 0.75f;
        private Vector3 _leftBind;
        private Vector3 _rightBind;
        private Vector3 _trayBind;
        private MaterialPropertyBlock _propertyBlock;

        public void Configure(Transform leftDoor, Transform rightDoor, Transform transferTray, Transform loadVisual, float doorTravel = 0.45f, float trayTravel = 0.75f)
        {
            _leftDoor = leftDoor; _rightDoor = rightDoor; _transferTray = transferTray; _loadVisual = loadVisual;
            _doorTravel = Mathf.Max(0f, doorTravel);
            _trayTravel = Mathf.Max(0f, trayTravel);
            _leftBind = leftDoor == null ? Vector3.zero : leftDoor.localPosition;
            _rightBind = rightDoor == null ? Vector3.zero : rightDoor.localPosition;
            _trayBind = transferTray == null ? Vector3.zero : transferTray.localPosition;
        }

        public void ApplyFill(float normalizedFill, bool transferActive)
        {
            ApplyTransfer(normalizedFill, transferActive ? 1f : 0f);
        }

        /// <summary>Shows the passive machine-level transfer sequence without owning inventory or transfer authority.</summary>
        public void ApplyTransfer(float normalizedFill, float transferProgress)
        {
            float fill = Mathf.Clamp01(normalizedFill);
            float progress = Mathf.Clamp01(transferProgress);
            if (_leftDoor != null) _leftDoor.localPosition = _leftBind + Vector3.left * (progress * _doorTravel);
            if (_rightDoor != null) _rightDoor.localPosition = _rightBind + Vector3.right * (progress * _doorTravel);
            if (_transferTray != null) _transferTray.localPosition = _trayBind + Vector3.forward * (progress * _trayTravel);
            if (_loadVisual == null) return;
            _loadVisual.localScale = new Vector3(2.15f, Mathf.Max(0.03f, fill * 1.45f), 2.15f);
            _loadVisual.localPosition = new Vector3(0f, -0.62f + _loadVisual.localScale.y * 0.5f, 0f);
            MeshRenderer renderer = _loadVisual.GetComponent<MeshRenderer>();
            if (renderer == null) return;
            if (_propertyBlock == null) _propertyBlock = new MaterialPropertyBlock();
            _propertyBlock.SetColor("_BaseColor", fill >= 0.95f ? Color.red : (fill <= 0.02f ? new Color(0.15f, 0.45f, 0.3f) : Color.yellow));
            _propertyBlock.SetColor("_Color", fill >= 0.95f ? Color.red : (fill <= 0.02f ? new Color(0.15f, 0.45f, 0.3f) : Color.yellow));
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
