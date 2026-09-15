using AutoEra.UI;
using AutoEra.World.Region;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AutoEra.Input
{
    public struct RegionInputFrame
    {
        public Vector2 Pan, Orbit, Pointer;
        public float Zoom;
        public bool Select, Focus, Cancel, Rotate;
    }

    public interface IRegionInputSource { RegionInputFrame Read(); }

    /// <summary>PC device reads are confined to this replaceable input boundary.</summary>
    public sealed class RegionInputModule : MonoBehaviour
    {
        [SerializeField] private InitialRegionScene _scene;
        [SerializeField] private RegionCameraController _camera;
        [SerializeField] private LayerMask _selectionLayers;
        private IRegionInputSource _source;
        private RegionObjectView _selected;
        private RegionObjectView _hovered;
        private float _lastClickTime = -1f;
        [SerializeField] private RegionFieldAccess _fieldAccess = new RegionFieldAccess();
        private bool _accessible;
        private RegionPlacementPreview _placement;
        private LineRenderer _placementLine;
        private Material _placementMaterial;
        public RegionPlacementPreview Placement => _placement;

        public void BeginPlacement(Vector2 size, System.Action<Vector2,float> confirmed)
        {
            EndPlacement();
            _placement = new RegionPlacementPreview(_scene.Region, size, confirmed);
            if (!UnityEngine.Application.isPlaying) return;
            var node = new GameObject("PlacementOutline");
            node.transform.SetParent(transform, false);
            _placementLine = node.AddComponent<LineRenderer>();
            _placementLine.loop = true; _placementLine.positionCount = 4; _placementLine.useWorldSpace = false;
            _placementLine.widthMultiplier = .09f;
            _placementMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _placementLine.sharedMaterial = _placementMaterial;
            _placementLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            Vector2 half = size * .5f;
            _placementLine.SetPosition(0,new Vector3(-half.x,0,-half.y));
            _placementLine.SetPosition(1,new Vector3(-half.x,0,half.y));
            _placementLine.SetPosition(2,new Vector3(half.x,0,half.y));
            _placementLine.SetPosition(3,new Vector3(half.x,0,-half.y));
        }

        private void EndPlacement()
        {
            _placement?.Dispose(); _placement = null;
            if (_placementLine != null) Destroy(_placementLine.gameObject);
            if (_placementMaterial != null) Destroy(_placementMaterial);
            _placementLine = null; _placementMaterial = null;
        }
        private void OnDestroy() => EndPlacement();

        public void SetSource(IRegionInputSource source) => _source = source;
        public bool SelectTarget(RegionObjectView target)
        {
            if (_scene == null || !_scene.Select(target, false)) return false;
            if (_selected != null) _selected.SetHighlight(_selected == _hovered, false);
            if (_selected != target) _accessible = false;
            _selected = target;
            if (_selected != null) _selected.SetHighlight(false, true);
            UpdateFieldAccess(AutoEraUiRuntime.BlocksWorldInput);
            return true;
        }
        private void Awake() { if (_source == null) _source = new DesktopSource(); }

        private void Update()
        {
            bool blocked = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            Tick(Time.unscaledDeltaTime, blocked);
        }

        public void Tick(float elapsedSeconds, bool blocked)
        {
            if (_source == null || _scene == null || _scene.Region == null || !_scene.Region.IsActive || _camera == null) return;
            RegionInputFrame frame = _source.Read();
            bool managementOpen = AutoEraUiRuntime.BlocksWorldInput;
            UpdateFieldAccess(managementOpen);
            if (frame.Cancel)
            {
                if (!AutoEraUiRuntime.DispatchIntent(AutoEraUiIntent.Cancel))
                { if (_placement != null) EndPlacement(); else SelectTarget(null); }
                return;
            }
            if (blocked || managementOpen)
            {
                if (_hovered != null) _hovered.SetHighlight(false, _hovered == _selected);
                _hovered = null;
                return;
            }
            _camera.Apply(frame.Pan, frame.Orbit, frame.Zoom, elapsedSeconds, _scene.Region.Bounds);
            if (_placement != null && _camera.ViewCamera != null)
            {
                Ray pointer = _camera.ViewCamera.ScreenPointToRay(frame.Pointer);
                if (new Plane(Vector3.up, Vector3.zero).Raycast(pointer, out float distance))
                {
                    Vector3 p = pointer.GetPoint(distance);
                    _placement.Move(new Vector2(p.x,p.z));
                }
                if (frame.Rotate) _placement.Rotate();
                if (_placementLine != null)
                {
                    _placementLine.transform.SetPositionAndRotation(new Vector3(_placement.Position.x,.045f,_placement.Position.y), Quaternion.Euler(0,_placement.Yaw,0));
                    _placementMaterial.SetColor("_BaseColor", _placement.IsValid ? Color.white : Color.red);
                }
                if (frame.Select && _placement.Confirm()) EndPlacement();
                return;
            }
            if (_camera.ViewCamera != null)
            {
                RegionObjectView target = null;
                Ray ray = _camera.ViewCamera.ScreenPointToRay(frame.Pointer);
                if (Physics.Raycast(ray, out RaycastHit hit, 1000f, _selectionLayers, QueryTriggerInteraction.Collide))
                    target = hit.collider.GetComponentInParent<RegionObjectView>();
                if (_hovered != target && _hovered != null) _hovered.SetHighlight(false, _hovered == _selected);
                _hovered = target;
                if (target != null) target.SetHighlight(true, target == _selected);
                if (frame.Select)
                {
                    if (target != null && target == _selected && Time.unscaledTime - _lastClickTime < .3f)
                        _camera.Focus(target.FocusPosition);
                    SelectTarget(target);
                    _lastClickTime = Time.unscaledTime;
                }
            }
            if (frame.Focus && _selected != null && _selected.Model != null)
                _camera.Focus(_selected.FocusPosition);
            UpdateFieldAccess(false);
        }

        private void UpdateFieldAccess(bool managementOpen)
        {
            if (_selected == null || _selected.Model == null || _camera.ViewCamera == null) _accessible = false;
            else if (_selected.Model.Kind != AutoEra.World.Identity.PersistentObjectKind.Machine) _accessible = true;
            else
            {
                Vector3 target = _selected.FocusPosition;
                Vector3 delta = target - _camera.FocusPosition;
                float distance = new Vector2(delta.x, delta.z).magnitude;
                _accessible = _fieldAccess.Evaluate(_accessible, distance, _camera.ViewCamera.transform.position.y - target.y,
                    _camera.ViewCamera.WorldToViewportPoint(target));
            }
            _scene.ShowFieldAccess(_accessible, managementOpen);
        }

        private sealed class DesktopSource : IRegionInputSource
        {
            public RegionInputFrame Read()
            {
                return new RegionInputFrame
                {
                    Pan = new Vector2((UnityEngine.Input.GetKey(KeyCode.D) ? 1 : 0) - (UnityEngine.Input.GetKey(KeyCode.A) ? 1 : 0),
                        (UnityEngine.Input.GetKey(KeyCode.W) ? 1 : 0) - (UnityEngine.Input.GetKey(KeyCode.S) ? 1 : 0)),
                    Orbit = UnityEngine.Input.GetMouseButton(1) ? new Vector2(UnityEngine.Input.GetAxisRaw("Mouse X"), UnityEngine.Input.GetAxisRaw("Mouse Y")) : Vector2.zero,
                    Pointer = UnityEngine.Input.mousePosition,
                    Zoom = UnityEngine.Input.mouseScrollDelta.y,
                    Select = UnityEngine.Input.GetMouseButtonDown(0),
                    Focus = UnityEngine.Input.GetKeyDown(KeyCode.F),
                    Rotate = UnityEngine.Input.GetKeyDown(KeyCode.R),
                    Cancel = UnityEngine.Input.GetKeyDown(KeyCode.Escape)
                };
            }
        }
    }
}
