using System;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed partial class RegionSensorEnvironment : ISensorEnvironment, IDisposable
    {
        private readonly InitialRegion _region;
        private readonly System.Collections.Generic.Dictionary<PersistentObjectReference, ISensorReadProvider> _providers = new System.Collections.Generic.Dictionary<PersistentObjectReference, ISensorReadProvider>();
        private readonly System.Collections.Generic.Dictionary<PersistentId, RegionSensorReadProvider> _owned = new System.Collections.Generic.Dictionary<PersistentId, RegionSensorReadProvider>();
        private readonly bool _autoRegister;
        private bool _disposed;
        public RegionSensorEnvironment(InitialRegion region, bool autoRegisterPublicState = false)
        {
            _region = region ?? throw new ArgumentNullException(nameof(region)); _autoRegister = autoRegisterPublicState;
            if (!_autoRegister) return;
            region.ObjectsChanged += Reconcile; region.ObjectRemoved += Remove;
            Reconcile();
        }
        public int Count => _providers.Count;
        private void Reconcile()
        {
            if (_disposed) return;
            foreach (var item in _region.Objects)
            {
                var target = new PersistentObjectReference(item.Id, item.Kind);
                if (_providers.ContainsKey(target)) continue;
                var provider = new RegionSensorReadProvider(item, p => RegionPlacement.ClosestPoint(item, p));
                _owned.Add(item.Id, provider); _providers.Add(target, provider);
            }
        }
        private void Remove(PersistentId id)
        {
            if (_owned.TryGetValue(id, out var owned)) { _providers.Remove(owned.Target); owned.Dispose(); _owned.Remove(id); }
            PersistentObjectReference removed = default;
            foreach (var key in _providers.Keys) if (key.Id == id) { removed = key; break; }
            if (removed.IsValid) _providers.Remove(removed);
        }
        public bool IsActive => !_disposed && _region.IsActive;
        public bool Contains(PersistentObjectReference target) => IsActive && _region.TryGet(target.Id, out var item) && item.Kind == target.ExpectedKind;
        public void Register(ISensorReadProvider provider)
        {
            if (provider == null || !Contains(provider.Target) || _providers.ContainsKey(provider.Target)) throw new ArgumentException("Unique live public provider required.");
            _providers.Add(provider.Target, provider);
        }
        /// <summary>生产域可替换通用公开状态；专用提供者的生命周期仍由生产域负责。</summary>
        public bool ReplacePublicProvider(ISensorReadProvider provider)
        {
            if (provider == null || !Contains(provider.Target)) return false;
            if (_owned.TryGetValue(provider.Target.Id, out var owned)) { owned.Dispose(); _owned.Remove(provider.Target.Id); }
            else if (_providers.TryGetValue(provider.Target, out var current) && !ReferenceEquals(current, provider)) return false;
            _providers[provider.Target] = provider; return true;
        }
        public bool Unregister(ISensorReadProvider provider)
        {
            if (provider == null || !_providers.TryGetValue(provider.Target, out var current) || !ReferenceEquals(current, provider)) return false;
            if (_owned.TryGetValue(provider.Target.Id, out var owned) && ReferenceEquals(owned, provider))
            { owned.Dispose(); _owned.Remove(provider.Target.Id); }
            _providers.Remove(provider.Target); if (_autoRegister) Reconcile(); return true;
        }
        public bool TryGetProvider(PersistentObjectReference target, out ISensorReadProvider provider)
        { provider = null; return Contains(target) && _providers.TryGetValue(target, out provider); }
        public void Dispose()
        {
            if (_disposed) return; _disposed = true;
            _region.ObjectsChanged -= Reconcile; _region.ObjectRemoved -= Remove;
            foreach (var provider in _owned.Values) provider.Dispose();
            _owned.Clear(); _providers.Clear();
        }
    }
    public sealed class TransformSensorAnchor : ISensorAnchor
    {
        private readonly Transform _anchor;
        public TransformSensorAnchor(Transform anchor) { _anchor = anchor != null ? anchor : throw new ArgumentNullException(nameof(anchor)); }
        public bool TryGetPosition(out Vector3 position)
        { position = default; if (_anchor == null || !_anchor.gameObject.activeInHierarchy) return false; position = _anchor.position; return true; }
    }
    /// <summary>Only existing public state; never synthesizes farm/tree production data.</summary>
    public sealed partial class RegionSensorReadProvider : ISensorReadProvider, IDisposable
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
                    _snapshot = new SensorSnapshot(checked(++_version), _target.PublicStatus, _target.PublicResourceAmount, _target.ResourceIsInfinite,
                        cachedAmount: _target.PublicCachedAmount, cacheCapacity: _target.PublicCacheCapacity);
                _dirty = false;
            }
            snapshot = _snapshot; return true;
        }
        public void Dispose() { if (_disposed) return; _disposed = true; _target.Changed -= OnChanged; _snapshot = null; }
    }
}
