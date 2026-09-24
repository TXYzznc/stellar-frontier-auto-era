using System;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed class RegionSensorEnvironment : ISensorEnvironment, IDisposable
    {
        private readonly InitialRegion _region;
        private readonly System.Collections.Generic.Dictionary<PersistentObjectReference, ISensorReadProvider> _providers = new System.Collections.Generic.Dictionary<PersistentObjectReference, ISensorReadProvider>();
        private bool _disposed;
        public RegionSensorEnvironment(InitialRegion region) { _region = region ?? throw new ArgumentNullException(nameof(region)); }
        public bool IsActive => !_disposed && _region.IsActive;
        public bool Contains(PersistentObjectReference target) => IsActive && _region.TryGet(target.Id, out var item) && item.Kind == target.ExpectedKind;
        public void Register(ISensorReadProvider provider)
        {
            if (provider == null || !Contains(provider.Target) || _providers.ContainsKey(provider.Target)) throw new ArgumentException("Unique live public provider required.");
            _providers.Add(provider.Target, provider);
        }
        public bool TryGetProvider(PersistentObjectReference target, out ISensorReadProvider provider)
        { provider = null; return Contains(target) && _providers.TryGetValue(target, out provider); }
        public void Dispose() { _disposed = true; _providers.Clear(); }
    }
    public sealed class TransformSensorAnchor : ISensorAnchor
    {
        private readonly Transform _anchor;
        public TransformSensorAnchor(Transform anchor) { _anchor = anchor != null ? anchor : throw new ArgumentNullException(nameof(anchor)); }
        public bool TryGetPosition(out Vector3 position)
        { position = default; if (_anchor == null || !_anchor.gameObject.activeInHierarchy) return false; position = _anchor.position; return true; }
    }
    /// <summary>Only existing public state; never synthesizes farm/tree production data.</summary>
    public sealed class RegionSensorReadProvider : ISensorReadProvider, IDisposable
    {
        private readonly RegionObject _target;
        private readonly Func<Vector3, Vector3> _closest;
        private bool _disposed;
        private long _version;
        private SensorSnapshot _snapshot;
        private bool _dirty = true;
        public PersistentObjectReference Target { get; }
        public bool IsAvailable => !_disposed && _target.IsRegistered;
        public RegionSensorReadProvider(RegionObject target, Func<Vector3, Vector3> publicRegionClosestPoint)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _closest = publicRegionClosestPoint ?? throw new ArgumentNullException(nameof(publicRegionClosestPoint));
            Target = new PersistentObjectReference(target.Id, target.Kind);
            target.Changed += OnChanged;
        }
        private void OnChanged(RegionObject _) { _dirty = true; }
        public bool Supports(SensorKind kind) => kind == SensorKind.ObjectState;
        public Vector3 ClosestPoint(Vector3 anchor) => _closest(anchor);
        public bool TryRead(SensorKind kind, out SensorSnapshot snapshot)
        {
            snapshot = null; if (!IsAvailable || !Supports(kind)) return false;
            if (_dirty)
            {
                if (_snapshot == null || _snapshot.PublicStatus != _target.PublicStatus ||
                    _snapshot.ResourceAmount != _target.PublicResourceAmount || _snapshot.Infinite != _target.ResourceIsInfinite ||
                    _snapshot.CachedAmount != _target.PublicCachedAmount || _snapshot.CacheCapacity != _target.PublicCacheCapacity)
                    _snapshot = new SensorSnapshot(++_version, _target.PublicStatus, _target.PublicResourceAmount, _target.ResourceIsInfinite,
                        cachedAmount: _target.PublicCachedAmount, cacheCapacity: _target.PublicCacheCapacity);
                _dirty = false;
            }
            snapshot = _snapshot; return true;
        }
        public void Dispose() { if (_disposed) return; _disposed = true; _target.Changed -= OnChanged; _snapshot = null; }
    }
}
