using System;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    /// <summary>Scene binding only. Serialized fields describe content, not persistent runtime state.</summary>
    public sealed class RegionObjectView : MonoBehaviour
    {
        [SerializeField] private string _displayName;
        [SerializeField] private PersistentObjectKind _kind;
        [SerializeField] private Vector2 _footprint = Vector2.one;
        [SerializeField] private bool _blocksNavigation = true;
        [SerializeField] private bool _infiniteResource;
        [SerializeField] private Transform _focusAnchor;
        [SerializeField] private string[] _workChannels = Array.Empty<string>();
        [SerializeField] private bool _tintProxy;
        [SerializeField] private Color _proxyTint = Color.white;
        private readonly System.Collections.Generic.List<RegionWorkQueue> _workQueues = new System.Collections.Generic.List<RegionWorkQueue>();
        private InitialRegion _region;
        private LineRenderer _outline;
        private Material _outlineMaterial;
        public bool IsHighlighted { get; private set; }

        public RegionObject Model { get; private set; }
        public Vector3 FocusPosition => _focusAnchor != null ? _focusAnchor.position : transform.position;

        public void Initialize(InitialRegion region)
        {
            if (_region != null) throw new InvalidOperationException("View is already bound.");
            if (region == null) throw new ArgumentNullException(nameof(region));
            Vector3 p = transform.position;
            RegionObject model = region.Register(_kind, _displayName, new Vector2(p.x, p.z),
                _footprint, transform.eulerAngles.y, _blocksNavigation);
            _region = region;
            Model = model;
            if (_tintProxy)
            {
                var block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", _proxyTint);
                Transform visual = transform.Find("Visual");
                if (visual != null) foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>()) renderer.SetPropertyBlock(block);
            }
            Model.SetPublicState(_kind == PersistentObjectKind.ResourcePoint ? "有效" : "待机", infinite: _infiniteResource);
            foreach (string channel in _workChannels)
                _workQueues.Add(new RegionWorkQueue(region, Model.Id,
                    new Rect(Model.Position - _footprint * .5f, _footprint), channel));
        }

        public RegionWorkQueue GetWorkChannel(int index) => index >= 0 && index < _workQueues.Count ? _workQueues[index] : null;

        public void SetHighlight(bool hovered, bool selected)
        {
            IsHighlighted = Model != null && Model.IsRegistered && (hovered || selected);
            if (!UnityEngine.Application.isPlaying) return;
            if (_outline == null && IsHighlighted)
            {
                var node = new GameObject("SelectionOutline");
                node.transform.SetParent(transform, false);
                _outline = node.AddComponent<LineRenderer>();
                _outline.useWorldSpace = false; _outline.loop = true; _outline.positionCount = 4;
                Vector2 half = _footprint * .5f;
                _outline.SetPosition(0, new Vector3(-half.x,.035f,-half.y));
                _outline.SetPosition(1, new Vector3(-half.x,.035f,half.y));
                _outline.SetPosition(2, new Vector3(half.x,.035f,half.y));
                _outline.SetPosition(3, new Vector3(half.x,.035f,-half.y));
                _outlineMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                _outline.sharedMaterial = _outlineMaterial;
                _outline.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                _outline.receiveShadows = false;
            }
            if (_outline == null) return;
            _outline.enabled = IsHighlighted;
            _outline.widthMultiplier = selected ? .075f : .035f;
            _outlineMaterial.SetColor("_BaseColor", selected ? new Color(1f,.58f,.16f) : Color.white);
        }

        public void Release()
        {
            SetHighlight(false, false);
            foreach (RegionWorkQueue queue in _workQueues) queue.Dispose();
            _workQueues.Clear();
            if (_region != null && Model != null) _region.Remove(Model.Id);
            _region = null;
            Model = null;
        }

        private void OnDestroy()
        {
            Release();
            if (_outlineMaterial != null) Destroy(_outlineMaterial);
        }
    }
}
