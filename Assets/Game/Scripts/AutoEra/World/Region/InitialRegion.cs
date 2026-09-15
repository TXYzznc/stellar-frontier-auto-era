using System;
using System.Collections.Generic;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed partial class InitialRegion : IDisposable
    {
        private readonly AutoEraWorldSession _session;
        private readonly Dictionary<PersistentId, RegionObject> _objects = new Dictionary<PersistentId, RegionObject>();
        private bool _disposed;
        public InitialRegion(AutoEraWorldSession session, Rect bounds)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            if (!session.IsActive || !session.Machines.IsActive) throw new ObjectDisposedException(nameof(session));
            if (!RegionPlacement.IsFinite(bounds.position) || !RegionPlacement.IsFinite(bounds.size) || bounds.width <= 0 || bounds.height <= 0)
                throw new ArgumentOutOfRangeException(nameof(bounds));
            Bounds = bounds;
            _session.Machines.Disposed += OnRosterDisposed;
        }
        public Rect Bounds { get; }
        public string RegionId => "initial";
        private PersistentId _selectedId;
        public int Count => IsActive ? _objects.Count : 0;
        public bool IsActive => !_disposed && _session.IsActive;
        public PersistentId SelectedId { get => IsActive ? _selectedId : PersistentId.Invalid; private set => _selectedId = value; }
        public event Action SelectionChanged;
        public event Action ObjectsChanged;
        public event Action<PersistentId> ObjectRemoved;
        public IEnumerable<RegionObject> Objects => IsActive ? (IEnumerable<RegionObject>)_objects.Values : Array.Empty<RegionObject>();
        public bool TryGet(PersistentId id, out RegionObject obj)
        {
            obj = null;
            return !_disposed && _session.IsActive && _objects.TryGetValue(id, out obj);
        }

        public RegionObject Register(PersistentObjectKind kind, string name, Vector2 position, Vector2 size, float yaw = 0, bool blocksNavigation = true)
        {
            if (_disposed || !_session.IsActive) throw new ObjectDisposedException(nameof(InitialRegion));
            if (kind != PersistentObjectKind.Machine && kind != PersistentObjectKind.Building && kind != PersistentObjectKind.ResourcePoint)
                throw new ArgumentOutOfRangeException(nameof(kind));
            if (string.IsNullOrWhiteSpace(name) || !RegionPlacement.IsFinite(position) || !RegionPlacement.IsFinite(size) ||
                size.x <= 0 || size.y <= 0 || !RegionPlacement.IsFinite(yaw)) throw new ArgumentException("Invalid region object.");
            if (!CanPlace(position, size, yaw, PersistentId.Invalid, out string reason)) throw new InvalidOperationException(reason);
            if (!_session.IdAllocator.TryAllocate(out PersistentId id)) throw new InvalidOperationException("ID exhausted.");
            var obj = new RegionObject(id, kind, name, position, size, yaw, blocksNavigation);
            if (_session.ObjectRegistry.TryRegister(id, kind, obj) != PersistentRegistryResult.Success)
                throw new InvalidOperationException("Object registry rejected region object.");
            obj.IsRegistered = true;
            _objects.Add(id, obj);
            ObjectsChanged?.Invoke();
            return obj;
        }

        public bool Select(PersistentId id, bool inputBlocked)
        {
            if (_disposed || !_session.IsActive || inputBlocked) return false;
            if (id.IsValid && !TryGet(id, out _)) return false;
            if (SelectedId == id) return true;
            SelectedId = id; SelectionChanged?.Invoke(); return true;
        }

        public bool Remove(PersistentId id)
            => !_disposed && RemoveObject(id);

        private bool RemoveObject(PersistentId id)
        {
            // Cleanup must also work after the owning world session has ended.
            if (!_objects.TryGetValue(id, out RegionObject obj)) return false;
            if (obj.Machine == null) _session.ObjectRegistry.TryUnregister(id, obj.Kind, obj);
            else ReleaseMachineBinding(obj.Machine);
            _objects.Remove(id); obj.IsRegistered = false;
            if (SelectedId == id) { SelectedId = PersistentId.Invalid; SelectionChanged?.Invoke(); }
            ObjectRemoved?.Invoke(id);
            ObjectsChanged?.Invoke();
            return true;
        }

        public bool CanPlace(Vector2 position, Vector2 size, float yaw, PersistentId excluded, out string reason)
        {
            reason = null;
            if (_disposed || !_session.IsActive) { reason = "区域已失效"; return false; }
            if (!RegionPlacement.IsFinite(position) || !RegionPlacement.IsFinite(size) || !RegionPlacement.IsFinite(yaw) || size.x <= 0 || size.y <= 0)
                { reason = "参数无效"; return false; }
            if (!RegionPlacement.Inside(Bounds, position, size, yaw)) { reason = "超出区域边界"; return false; }
            foreach (RegionObject other in _objects.Values)
            {
                if (other.Id != excluded && RegionPlacement.Overlaps(position, size, yaw, other.Position, other.Size, other.Yaw))
                    { reason = "占地冲突：" + other.Name; return false; }
            }
            return true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _session.Machines.Disposed -= OnRosterDisposed;
            var ids = new List<PersistentId>(_objects.Keys);
            foreach (PersistentId id in ids) RemoveObject(id);
            _objects.Clear(); SelectedId = PersistentId.Invalid;
            SelectionChanged = null; ObjectRemoved = null; ObjectsChanged = null;
        }
    }
}
