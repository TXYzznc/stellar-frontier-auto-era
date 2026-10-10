using System;
using System.Collections.Generic;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed class InitialRegionSnapshot
    {
        public Vector2 BoundsPosition, BoundsSize;
        public ulong AllocatedThrough;
        public RegionObjectSnapshot[] Objects;
    }

    public sealed class RegionObjectSnapshot
    {
        public ulong Id, AttachedResourcePoint;
        public PersistentObjectKind Kind;
        public string Name, PublicStatus;
        public Vector2 Position, Size;
        public float Yaw;
        public bool BlocksNavigation, ResourceIsInfinite;
        public long? PublicResourceAmount, PublicCachedAmount, PublicCacheCapacity;
    }

    public sealed partial class InitialRegion
    {
        public InitialRegionSnapshot CapturePersistentState()
        {
            if (!IsActive) throw new ObjectDisposedException(nameof(InitialRegion));
            var records = new List<RegionObjectSnapshot>(_objects.Count);
            foreach (var value in _objects.Values)
            {
                if (value.Kind == PersistentObjectKind.Machine && value.Machine == null)
                    throw new InvalidOperationException("A legacy machine proxy is not a persistent machine.");
                records.Add(new RegionObjectSnapshot { Id=value.Id.Value, Kind=value.Kind, Name=value.Name,
                    Position=value.Position, Size=value.Size, Yaw=value.Yaw, BlocksNavigation=value.BlocksNavigation,
                    AttachedResourcePoint=value.AttachedResourcePoint.Value, PublicStatus=value.PublicStatus,
                    PublicResourceAmount=value.PublicResourceAmount, ResourceIsInfinite=value.ResourceIsInfinite,
                    PublicCachedAmount=value.PublicCachedAmount, PublicCacheCapacity=value.PublicCacheCapacity });
            }
            records.Sort((a,b) => a.Id.CompareTo(b.Id));
            return new InitialRegionSnapshot { BoundsPosition=Bounds.position, BoundsSize=Bounds.size,
                AllocatedThrough=_session.IdAllocator.IsExhausted ? ulong.MaxValue : _session.IdAllocator.NextId.Value-1,
                Objects=records.ToArray() };
        }

        /// <summary>Restore on an empty candidate after the machine roster. Views and work queues bind afterwards.</summary>
        public void RestorePersistentState(InitialRegionSnapshot snapshot)
        {
            if (!IsActive || _objects.Count != 0) throw new InvalidOperationException("Region restoration requires an empty candidate.");
            if (snapshot?.Objects == null || snapshot.BoundsPosition != Bounds.position || snapshot.BoundsSize != Bounds.size)
                throw new ArgumentException("Region bounds or records do not match.");
            var built = new Dictionary<PersistentId,RegionObject>();
            foreach (var record in snapshot.Objects)
            {
                if (record == null || record.Id == 0 || record.Id > snapshot.AllocatedThrough ||
                    record.AttachedResourcePoint > snapshot.AllocatedThrough ||
                    (record.Kind != PersistentObjectKind.Machine && record.Kind != PersistentObjectKind.Building && record.Kind != PersistentObjectKind.ResourcePoint) ||
                    string.IsNullOrWhiteSpace(record.Name) || !RegionPlacement.IsFinite(record.Position) || !RegionPlacement.IsFinite(record.Size) ||
                    !RegionPlacement.IsFinite(record.Yaw) || record.Size.x <= 0 || record.Size.y <= 0 ||
                    !RegionPlacement.Inside(Bounds,record.Position,record.Size,record.Yaw)) throw new ArgumentException("Invalid region object.");
                var id = new PersistentId(record.Id);
                if (built.ContainsKey(id)) throw new ArgumentException("Duplicate region identity.");
                Machines.MachineInstance machine = null;
                if (record.Kind == PersistentObjectKind.Machine)
                {
                    if (!_session.Machines.TryGet(id,out machine) || machine.Name != record.Name || !machine.Deployed ||
                        machine.Integrity <= 0 || machine.RegionBindingOwner != null ||
                        _session.ObjectRegistry.TryResolve(id,PersistentObjectKind.Machine,out var registered) != PersistentRegistryResult.Success ||
                        !ReferenceEquals(machine,registered)) throw new ArgumentException("Region machine does not match the restored roster.");
                }
                else if (_session.ObjectRegistry.TryGetKind(id,out _)) throw new ArgumentException("Region identity already belongs to another object.");
                if (record.PublicCachedAmount.HasValue && record.PublicCacheCapacity.HasValue && record.PublicCachedAmount > record.PublicCacheCapacity)
                    throw new ArgumentException("Region cache exceeds capacity.");
                var obj = new RegionObject(id,record.Kind,record.Name,record.Position,record.Size,record.Yaw,record.BlocksNavigation,machine);
                obj.SetPublicState(record.PublicStatus,record.PublicResourceAmount,record.ResourceIsInfinite,record.PublicCachedAmount,record.PublicCacheCapacity);
                obj.AttachedResourcePoint = new PersistentId(record.AttachedResourcePoint);
                foreach (var other in built.Values)
                {
                    bool blocks = obj.Machine == null && other.Machine == null || obj.Machine != null && other.BlocksNavigation || other.Machine != null && obj.BlocksNavigation;
                    if (blocks && RegionPlacement.Overlaps(obj.Position,obj.Size,obj.Yaw,other.Position,other.Size,other.Yaw))
                        throw new ArgumentException("Restored region objects overlap.");
                }
                built.Add(id,obj);
            }
            foreach (var obj in built.Values)
                if (obj.AttachedResourcePoint.IsValid && (obj.Kind != PersistentObjectKind.Building ||
                    !built.TryGetValue(obj.AttachedResourcePoint,out var point) || point.Kind != PersistentObjectKind.ResourcePoint))
                    throw new ArgumentException("Invalid facility attachment.");

            // Validation above is complete. No object notifications or deployment commands are replayed.
            var registeredObjects = new List<RegionObject>();
            try
            {
                foreach (var obj in built.Values)
                    if (obj.Machine == null)
                    {
                        if (_session.ObjectRegistry.TryRegister(obj.Id,obj.Kind,obj) != PersistentRegistryResult.Success)
                            throw new InvalidOperationException("Registry changed during region restoration.");
                        registeredObjects.Add(obj);
                    }
            }
            catch
            {
                foreach (var obj in registeredObjects) _session.ObjectRegistry.TryUnregister(obj.Id,obj.Kind,obj);
                throw;
            }
            if (snapshot.AllocatedThrough != 0) _session.IdAllocator.TryRestore(new PersistentId(snapshot.AllocatedThrough));
            foreach (var obj in built.Values)
            {
                _objects.Add(obj.Id,obj); obj.IsRegistered = true;
                if (obj.Machine == null) continue;
                obj.Machine.RegionBindingOwner = this;
                obj.Machine.Changed += OnMachineChanged;
            }
        }
    }
}
