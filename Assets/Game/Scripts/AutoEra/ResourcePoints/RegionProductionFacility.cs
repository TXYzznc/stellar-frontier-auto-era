using System;
using AutoEra.Machines.Sensors;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.ResourcePoints
{
    public enum ProductionFacilityKind { Forest, Mineral }
    /// <summary>Explicit content binding. Domain state is retained by the world when this view is hidden or released.</summary>
    public sealed partial class RegionProductionFacility : MonoBehaviour, ISensorReadProvider
    {
        [SerializeField] private ProductionFacilityKind _kind;
        [SerializeField] private int _size;
        [SerializeField] private ForestMineralProductionConfig _config;
        [SerializeField] private Transform[] _treeRoots = Array.Empty<Transform>();
        [SerializeField] private Transform[] _rocks = Array.Empty<Transform>();
        [SerializeField] private Transform _groundPileAnchor;
        private AutoEraWorldSession _world;
        private InitialRegion _region;
        private RegionObject _object;
        private SensorSnapshot _snapshot;
        private long _version, _previousDomainRevision = -1, _previousCargoRevision = -1;
        private ProductionTreePresentation[] _presentations;
        public long NavigationRevision { get; private set; }
        public ForestProduction Forest { get; private set; }
        public MineralProduction Mineral { get; private set; }
        public ProductionRules Rules { get; private set; }
        internal ResourceProductionWorldService Production => _world.Production;
        public PersistentObjectReference Target => _object == null ? default : new PersistentObjectReference(_object.Id, _object.Kind);
        public bool IsAvailable => _world != null && _world.IsActive && _region != null && _region.IsActive && _object != null && _object.IsRegistered;
        public Vector3 GroundPilePosition => _groundPileAnchor != null ? _groundPileAnchor.position : transform.position;
        public void BindFalls(TreeFallSimulation simulation)
        { if (_presentations != null) foreach (var presentation in _presentations) presentation.Bind(Forest, simulation); }
        public void CollectObstacles(System.Collections.Generic.List<Bounds> destination)
        { if (IsAvailable && _presentations != null) foreach (var presentation in _presentations) if (presentation.HasObstacle) destination.Add(presentation.ObstacleBounds); }
        public void CollectOtherTreeObstacles(PersistentId except, System.Collections.Generic.List<Bounds> destination)
        { if (IsAvailable && _presentations != null) foreach (var presentation in _presentations) if (presentation.TreeId != except && presentation.HasObstacle) destination.Add(presentation.ObstacleBounds); }
        public void ConfigureForEditor(ProductionFacilityKind kind, int size, ForestMineralProductionConfig config, Transform[] trees, Transform[] rocks, Transform pile)
        { _kind = kind; _size = size; _config = config; _treeRoots = trees ?? Array.Empty<Transform>(); _rocks = rocks ?? Array.Empty<Transform>(); _groundPileAnchor = pile; }
        public void Initialize(AutoEraWorldSession world, InitialRegion region, RegionObject value)
            => InitializeCore(world,region,value,false);
        private void InitializeCore(AutoEraWorldSession world,InitialRegion region,RegionObject value,bool persistent)
        {
            if (_world != null || world == null || region == null || value == null || value.Kind != PersistentObjectKind.ResourcePoint || _config == null)
                throw new InvalidOperationException("Production content binding missing or already initialized.");
            _world = world; _region = region; _object = value; Rules = _config.Read();
            world.Production.BindGroundPosition(value.Id, GroundPilePosition);
            if (_kind == ProductionFacilityKind.Forest)
            {
                if (_treeRoots.Length != Rules.TreeCount(_size)) throw new InvalidOperationException("Tree content count does not match configuration.");
                var positions = new Vector3[_treeRoots.Length];
                for (int i = 0; i < positions.Length; i++)
                { if (_treeRoots[i] == null) throw new InvalidOperationException("Tree content reference missing."); positions[i] = _treeRoots[i].position; }
                Forest = world.Production.RegisterForest(value.Id, Rules, positions);
                if(persistent)
                {
                    if(Forest.Count!=positions.Length)throw new InvalidOperationException("Saved original tree slot count changed.");
                    for(int i=0;i<positions.Length;i++)if(!Forest.ReadAt(i).Position.Equals(positions[i]))throw new InvalidOperationException("Saved original tree placement changed without relocation.");
                }
                _presentations = new ProductionTreePresentation[_treeRoots.Length];
                for (int i = 0; i < _presentations.Length; i++)
                { _presentations[i] = _treeRoots[i].GetComponent<ProductionTreePresentation>(); if (_presentations[i] == null) throw new InvalidOperationException("Tree presentation binding missing."); _presentations[i].Bind(Forest, null); }
            }
            else
            {
                Mineral = world.Production.RegisterMineral(value.Id, _size, value.Id.Value, Rules);
                if (_rocks.Length != Mineral.FullRockCount) throw new InvalidOperationException("Rock content count does not match deposit tier.");
            }
            RefreshCore(world.Clock.WorldMilliseconds,!persistent);
        }

        public void InitializePersistent(AutoEraWorldSession world,InitialRegion region,RegionObject value)
        {
            if(world==null || value==null || (_kind==ProductionFacilityKind.Forest ? !world.Production.TryGetForest(value.Id,out _) : !world.Production.TryGetMineral(value.Id,out _)))
                throw new InvalidOperationException("Saved production authority is missing.");
            if(_config==null)throw new InvalidOperationException("Saved production configuration is missing.");
            var authored=_config.Read();
            var saved=_kind==ProductionFacilityKind.Forest ? (world.Production.TryGetForest(value.Id,out var forest) ? forest.CapturePersistent().Rules : null) :
                (world.Production.TryGetMineral(value.Id,out var mineral) ? mineral.CapturePersistent().Rules : null);
            if(saved==null || !saved.Matches(authored))throw new InvalidOperationException("Production content rules changed without a supported version migration.");
            InitializeCore(world,region,value,true);
        }
        public bool Supports(SensorKind kind) => kind == SensorKind.ObjectState;
        public void Release()
        {
            if (_presentations != null) foreach (var presentation in _presentations) if (presentation != null) presentation.ReleaseRepresentation();
            _world = null; _region = null; _object = null; _snapshot = null; Forest = null; Mineral = null; Rules = null;
            _version = 0; _previousDomainRevision = _previousCargoRevision = -1; _presentations = null; NavigationRevision++;
        }
        public Vector3 ClosestPoint(Vector3 anchor) => _object != null ? RegionPlacement.ClosestPoint(_object, anchor) : transform.position;
        public bool TryRead(SensorKind kind, out SensorSnapshot snapshot)
        {
            snapshot = null; if (!IsAvailable || !Supports(kind)) return false; Refresh(_world.Clock.WorldMilliseconds);
            if (_snapshot == null)
                _snapshot = Forest != null ? new SensorSnapshot(++_version, _object.PublicStatus, infinite: true, cachedAmount: Forest.CachedUnits, trees: Forest.CaptureTrees()) :
                    new SensorSnapshot(++_version, _object.PublicStatus, Mineral.RemainingUnits, cachedAmount: Mineral.CachedUnits);
            snapshot = _snapshot; return true;
        }
        public void Refresh(long now)
            => RefreshCore(now,true);
        private void RefreshCore(long now,bool settle)
        {
            if (!IsAvailable) return;
            if (settle && Mineral != null && Mineral.TryCleanup(now))
            { _region.RemoveResourcePoint(_object.Id); gameObject.SetActive(false); return; }
            long revision = Forest != null ? Forest.Revision : Mineral.Revision;
            long cargoRevision = _world.Resources.Authority.Revision;
            if (_previousDomainRevision == revision && _previousCargoRevision == cargoRevision)
            {
                if (Forest == null) return;
                // Only falling trees need a physics boundary check without a domain revision.
                for (int i = 0; i < Forest.Count; i++)
                { var tree = Forest.ReadAt(i); if (tree.Stage == TreeStage.Falling) _presentations[i].Apply(tree, now,settle); }
                if (Forest.Revision == revision && _world.Resources.Authority.Revision == cargoRevision) return;
                revision = Forest.Revision; cargoRevision = _world.Resources.Authority.Revision;
            }
            _previousDomainRevision = revision; _previousCargoRevision = cargoRevision;
            _snapshot = null;
            if (Forest != null)
            {
                for (int i = 0; i < Forest.Count && i < _treeRoots.Length; i++)
                {
                    var root = _treeRoots[i]; if (root == null) continue;
                    var tree = Forest.ReadAt(i); bool visible = tree.Stage != TreeStage.Removed;
                    if (root.gameObject.activeSelf != visible) root.gameObject.SetActive(visible);
                    var presentation = _presentations[i]; bool before = presentation.HasObstacle;
                    presentation.Apply(tree, now,settle); if (before != presentation.HasObstacle) NavigationRevision++;
                }
                string status = Forest.MatureCount > 0 ? "可采集" : "生长中";
                if (_object.PublicStatus != status || _object.PublicCachedAmount != Forest.CachedUnits)
                    _object.SetPublicState(status, infinite: true, cachedAmount: Forest.CachedUnits);
            }
            else
            {
                for (int i = 0; i < _rocks.Length; i++) if (_rocks[i] != null && _rocks[i].gameObject.activeSelf != Mineral.IsRockVisible(i)) _rocks[i].gameObject.SetActive(Mineral.IsRockVisible(i));
                string status = Mineral.RemainingExact > 0 ? "可采集" : "耗尽";
                if (_object.PublicStatus != status || _object.PublicResourceAmount != Mineral.RemainingUnits || _object.PublicCachedAmount != Mineral.CachedUnits)
                    _object.SetPublicState(status, Mineral.RemainingUnits, cachedAmount: Mineral.CachedUnits);
            }
        }
    }
}
