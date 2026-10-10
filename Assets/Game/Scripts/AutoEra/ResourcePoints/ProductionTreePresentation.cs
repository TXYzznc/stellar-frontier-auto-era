using AutoEra.World.Identity;
using UnityEngine;
using System.Collections.Generic;
using AutoEra.World.Region;

namespace AutoEra.ResourcePoints
{
    /// <summary>Approved geometry split at the actual fracture; isolated physics never owns wood quantity.</summary>
    public sealed class ProductionTreePresentation : MonoBehaviour
    {
        [SerializeField] private Transform _visual;
        [SerializeField] private float _authoredHeight = 3;
        [SerializeField] private float _trunkRadius;
        private Vector3 _scale;
        private MeshFilter[] _filters;
        private Mesh[] _originals;
        private readonly List<Mesh> _slices = new List<Mesh>();
        private ForestProduction _forest;
        private TreeFallSimulation _simulation;
        private GameObject _upper;
        private Rigidbody _body;
        private bool _split;
        private LODGroup _lod;
        public PersistentId TreeId { get; private set; }
        public ulong FellingSequence { get; private set; }
        public bool HasObstacle { get; private set; }
        public Bounds ObstacleBounds { get; private set; }
        public GameObject FallingUpper => _upper;
        public void ConfigureForEditor(Transform visual, float authoredHeight, float trunkRadius)
        { _visual = visual; _authoredHeight = authoredHeight; _trunkRadius = trunkRadius; }
        public void Bind(ForestProduction forest, TreeFallSimulation simulation) { _forest = forest; _simulation = simulation; }
        private void Initialize()
        {
            if (_filters != null) return;
            _scale = _visual.localScale; _filters = _visual.GetComponentsInChildren<MeshFilter>(true); _originals = new Mesh[_filters.Length];
            _lod = _visual.GetComponentInChildren<LODGroup>(true);
            for (int i = 0; i < _filters.Length; i++) _originals[i] = _filters[i].sharedMesh;
        }
        public void Apply(TreeReadout tree, long now,bool allowSettlement=true)
        {
            if (_visual == null || _authoredHeight <= 0) return;
            Initialize(); TreeId = tree.Id;
            if (tree.Stage == TreeStage.Growing || tree.Stage == TreeStage.Mature)
            {
                if (_split) Restore();
                _visual.localScale = _scale * (float)(tree.Height / _authoredHeight);
            }
            else if (tree.Stage == TreeStage.Falling || tree.Stage == TreeStage.Stump)
            {
                if (!_split || FellingSequence != tree.FellingSequence) Split(tree);
                if (allowSettlement && tree.Stage == TreeStage.Falling && _body != null && now > tree.FallingStartedAt + 250 && _body.IsSleeping())
                    _forest?.TrySettle(tree.Id, tree.FellingSequence, now, out _);
                if (tree.Stage == TreeStage.Stump) ClearUpper();
            }
            FellingSequence = tree.FellingSequence;
            HasObstacle = tree.Stage != TreeStage.Removed && (tree.Stage != TreeStage.Stump || tree.Height > .001);
            float maximumHeight = _forest != null ? (float)_forest.MaximumHeight : _authoredHeight;
            float radius = _trunkRadius * maximumHeight / _authoredHeight;
            ObstacleBounds = new Bounds(tree.Position + Vector3.up * maximumHeight * .5f, new Vector3(radius * 2, maximumHeight, radius * 2));
        }
        private void Split(TreeReadout tree)
        {
            Restore(); _visual.localScale = _scale * (float)((tree.FractureHeight + tree.UpperLength) / _authoredHeight);
            if (_lod != null && _lod.enabled && _lod.gameObject.activeInHierarchy) _lod.ForceLOD(0); // Distant impostors do not contain a true trunk fracture mesh.
            var pivot = new GameObject("FallenUpper_" + tree.Id.Value); pivot.transform.position = tree.Position + Vector3.up * (float)tree.FractureHeight;
            var upperVisual = Instantiate(_visual.gameObject, pivot.transform); upperVisual.transform.position = _visual.position; upperVisual.transform.rotation = _visual.rotation;
            upperVisual.GetComponentInChildren<LODGroup>(true)?.ForceLOD(0);
            var upperFilters = upperVisual.GetComponentsInChildren<MeshFilter>(true);
            for (int i = 0; i < _filters.Length; i++)
            {
                var matrix = transform.worldToLocalMatrix * _filters[i].transform.localToWorldMatrix;
                var lower = TreeMeshSlice.Clip(_originals[i], matrix, (float)tree.FractureHeight, false);
                var upper = TreeMeshSlice.Clip(_originals[i], matrix, (float)tree.FractureHeight, true);
                _filters[i].sharedMesh = lower; upperFilters[i].sharedMesh = upper; _slices.Add(lower); _slices.Add(upper);
            }
            _split = true;
            if (tree.Stage != TreeStage.Falling || _simulation == null) { RegionNavigation.DestroySafely(pivot); return; }
            var renderers = pivot.GetComponentsInChildren<Renderer>(true); Bounds bounds = new Bounds(pivot.transform.position, Vector3.zero); bool first = true;
            foreach (var renderer in renderers) if (renderer.GetComponent<MeshFilter>()?.sharedMesh.vertexCount > 0)
            { if (first) { bounds = renderer.bounds; first = false; } else bounds.Encapsulate(renderer.bounds); }
            _upper = pivot; _body = _simulation.Attach(pivot, bounds, tree.FallDirection);
        }
        private void ClearUpper() { if (_upper != null) RegionNavigation.DestroySafely(_upper); _upper = null; _body = null; }
        private void Restore()
        {
            ClearUpper();
            if (_filters != null) for (int i = 0; i < _filters.Length; i++) if (_filters[i] != null) _filters[i].sharedMesh = _originals[i];
            foreach (var mesh in _slices) if (mesh != null) RegionNavigation.DestroySafely(mesh);
            _slices.Clear(); _split = false;
            if (_lod != null && _lod.enabled && _lod.gameObject.activeInHierarchy) _lod.ForceLOD(-1);
        }
        public void ReleaseRepresentation()
        { Restore(); if (_visual != null && _filters != null) _visual.localScale = _scale;
            _forest = null; _simulation = null; TreeId = default; FellingSequence = 0; HasObstacle = false; }
        private void OnDestroy() => Restore();
    }
}
