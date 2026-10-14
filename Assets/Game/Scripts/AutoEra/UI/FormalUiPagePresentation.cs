using UnityEngine;

using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>Applies the authored shared shell placement when an existing page opens.</summary>
    [DisallowMultipleComponent]
    public sealed class FormalUiPagePresentation : MonoBehaviour
    {
        [SerializeField] private RectTransform[] _targets;
        [SerializeField] private Vector4[] _rects;
        [SerializeField] private int _family;
        [SerializeField] private Image _stateTarget;
        [SerializeField] private Sprite _activeStateSprite;
        [SerializeField] private Sprite _normalStateSprite;

        public void ConfigureState(Image target, Sprite active, Sprite normal)
        {
            _stateTarget = target;
            _activeStateSprite = active;
            _normalStateSprite = normal;
            if (_stateTarget != null)
                _stateTarget.sprite = gameObject.activeSelf ? _activeStateSprite : _normalStateSprite;
        }

        public void Configure(RectTransform[] targets, Vector4[] rects, int family)
        {
            _targets = targets;
            _rects = rects;
            _family = family;
            Apply();
        }

        private void OnEnable() => Apply();
        private void OnDisable()
        {
            if (_stateTarget != null) _stateTarget.sprite = _normalStateSprite;
        }
        public void ApplyForPreview() => Apply();

        private void Apply()
        {
            if (_stateTarget != null && _activeStateSprite != null)
                _stateTarget.sprite = _activeStateSprite;
            if (_targets == null || _rects == null) return;
            int count = Mathf.Min(_targets.Length, _rects.Length);
            for (int i = 0; i < count; i++)
            {
                RectTransform target = _targets[i];
                if (target == null) continue;
                Vector4 rect = _rects[i];
                target.anchorMin = target.anchorMax = new Vector2(0, 1);
                target.pivot = new Vector2(0, 1);
                target.anchoredPosition = new Vector2(rect.x, -rect.y);
                target.sizeDelta = new Vector2(rect.z, rect.w);
                var grid = target.GetComponent<GridLayoutGroup>();
                if (grid == null) continue;
                bool compact = _family >= 5 && _family <= 7;
                bool vertical = !compact && rect.z <= 252 && rect.w > 120;
                grid.constraint = compact || vertical ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
                grid.constraintCount = compact ? 3 : 1;
                grid.cellSize = compact ? new Vector2((rect.z - 20) / 3, 32) : vertical ? new Vector2(rect.z, 60) : new Vector2(166, 42);
                grid.spacing = new Vector2(10, 8);
                if (compact && grid.transform.childCount > 8)
                {
                    for (int childIndex = 0; childIndex < grid.transform.childCount; childIndex++)
                    {
                        GameObject child = grid.transform.GetChild(childIndex).gameObject;
                        string id = child.name;
                        bool machine = id.Contains("MachineHardware") || id.Contains("MachineAlgorithm") || id.Contains("MachineDiagnostics");
                        bool resource = id.Contains("Farm") || id.Contains("Forest") || id.Contains("Mineral") || id.Contains("Water") && !id.Contains("Warehouse") || id.Contains("SensorRecords");
                        child.SetActive(_family == 5 ? machine : _family == 6 ? resource : !machine && !resource);
                    }
                }
            }
        }
    }
}
