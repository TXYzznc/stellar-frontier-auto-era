using System;
using System.Collections.Generic;
using AutoEra.ResourcePoints;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Machines.Sensors
{
    public sealed class SoilReadoutSnapshot
    {
        public long Id;
        public Vector3 Position;
        public float Area, Moisture;
        public bool Valid;
    }
    public sealed class CropReadoutSnapshot
    {
        public long Id;
        public float Suitability, GrowthSpeed;
        public bool Valid;
    }
    public sealed class SensorReadoutSnapshot
    {
        public long Version;
        public string PublicStatus;
        public long? ResourceAmount, CachedAmount, CacheCapacity;
        public bool Infinite;
        public SoilReadoutSnapshot[] Soil;
        public CropReadoutSnapshot[] Crops;
        public TreeReadout[] Trees;
        internal static SensorReadoutSnapshot Capture(SensorSnapshot source)
        {
            if (source == null) return null;
            var result = new SensorReadoutSnapshot { Version = source.Version, PublicStatus = source.PublicStatus, ResourceAmount = source.ResourceAmount,
                CachedAmount = source.CachedAmount, CacheCapacity = source.CacheCapacity, Infinite = source.Infinite };
            if (source.Soil != null)
            {
                var cells = new List<SoilReadoutSnapshot>(source.Soil.Count);
                foreach (var cell in source.Soil) cells.Add(new SoilReadoutSnapshot { Id = cell.Id, Position = cell.Position, Area = cell.Area, Moisture = cell.Moisture, Valid = cell.IsValid });
                result.Soil = cells.ToArray();
            }
            if (source.Crops != null)
            {
                var cells = new List<CropReadoutSnapshot>(source.Crops.Count);
                foreach (var cell in source.Crops) cells.Add(new CropReadoutSnapshot { Id = cell.Id, Suitability = cell.Suitability, GrowthSpeed = cell.GrowthSpeed, Valid = cell.IsValid });
                result.Crops = cells.ToArray();
            }
            if (source.Trees != null) { result.Trees = new TreeReadout[source.Trees.Count]; for (int i = 0; i < result.Trees.Length; i++) result.Trees[i] = source.Trees[i]; }
            return result;
        }
        internal SensorSnapshot Build()
        {
            SoilCellReadout[] soil = null; CropCellReadout[] crops = null;
            if (Soil != null) { soil = new SoilCellReadout[Soil.Length]; for (int i = 0; i < soil.Length; i++)
                { var cell = Soil[i] ?? throw new ArgumentException("Null soil readout."); soil[i] = new SoilCellReadout(cell.Id, cell.Position, cell.Area, cell.Moisture, cell.Valid); } }
            if (Crops != null) { crops = new CropCellReadout[Crops.Length]; for (int i = 0; i < crops.Length; i++)
                { var cell = Crops[i] ?? throw new ArgumentException("Null crop readout."); crops[i] = new CropCellReadout(cell.Id, cell.Suitability, cell.GrowthSpeed, cell.Valid); } }
            return new SensorSnapshot(Version, PublicStatus, ResourceAmount, Infinite, soil, crops, CachedAmount, CacheCapacity, Trees);
        }
    }
    public sealed class MachineSensorSnapshot
    {
        public PersistentId ComponentId;
        public PersistentId ComputeLeaseId;
        public PersistentObjectReference Target;
        public ulong Generation, Sequence, RequestVersion;
        public long RemainingMilliseconds;
        public SensorReadReason Reason;
        public SensorReadoutSnapshot Sample;
    }
    public sealed partial class MachineSensor
    {
        public bool TryCapturePersistent(long now, out MachineSensorSnapshot snapshot)
        {
            snapshot = null;
            if (_disposed || now < _now || now < 0) return false;
            snapshot = new MachineSensorSnapshot { ComponentId = Id, Target = Target, Generation = Generation, Sequence = _sequence,
                ComputeLeaseId = _lease?.Id ?? PersistentId.Invalid,
                RequestVersion = _requestVersion, RemainingMilliseconds = Math.Max(0, _next - now), Reason = Reason, Sample = SensorReadoutSnapshot.Capture(LastSample) };
            return true;
        }
        public bool RestorePersistent(MachineSensorSnapshot snapshot, long now)
        {
            if (_disposed || _lease != null || Generation != 0 || _sequence != 0 || LastSample != null || snapshot == null || now < 0 ||
                snapshot.ComponentId != Id || snapshot.RemainingMilliseconds < 0 || snapshot.RemainingMilliseconds > Profile.IntervalMilliseconds ||
                snapshot.RemainingMilliseconds > long.MaxValue - now || snapshot.Target.IsValid && snapshot.Generation == 0 ||
                !Enum.IsDefined(typeof(SensorReadReason), snapshot.Reason) || snapshot.Reason == SensorReadReason.Disposed ||
                snapshot.Reason == SensorReadReason.None && snapshot.Sample == null) return false;
            SensorSnapshot sample;
            try { sample = snapshot.Sample?.Build(); } catch (ArgumentException) { return false; }
            if (snapshot.Reason == SensorReadReason.None && Profile.Kind == SensorKind.Soil && !sample.TryGetWeightedMoisture(out _)) return false;
            ComputeRequest lease = null;
            if (snapshot.ComputeLeaseId.IsValid && (!_context.Compute.TryFindPersistentRequest(snapshot.ComputeLeaseId, out lease) ||
                lease.Source != Id || lease.Cost != Profile.ComputeCost || lease.Class != ComputeClass.Sampling ||
                lease.MergeKind != ComputeMergeKind.SensorSample || !lease.CanYieldAtBoundary || lease.Version != snapshot.RequestVersion)) return false;
            Target = snapshot.Target; Generation = snapshot.Generation; _sequence = snapshot.Sequence; _requestVersion = snapshot.RequestVersion;
            _lease = lease;
            _now = now; _next = now + snapshot.RemainingMilliseconds; LastSample = sample; Reason = snapshot.Reason;
            return true;
        }
    }
}
