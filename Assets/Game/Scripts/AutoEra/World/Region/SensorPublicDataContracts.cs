using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public readonly struct SoilCellReadout
    {
        public long Id { get; }
        public Vector3 Position { get; }
        public float Area { get; }
        public float Moisture { get; }
        public bool IsValid { get; }
        public SoilCellReadout(long id, Vector3 position, float area, float moisture, bool valid)
        {
            if (id <= 0 || !Finite(position) || !Finite(area) || area <= 0 || !Finite(moisture) || moisture < 0 || moisture > 100)
                throw new ArgumentException("Invalid soil cell.");
            Id = id; Position = position; Area = area; Moisture = moisture; IsValid = valid;
        }
        internal static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
        internal static bool Finite(Vector3 v) => Finite(v.x) && Finite(v.y) && Finite(v.z);
    }
    public readonly struct CropCellReadout
    {
        public long Id { get; }
        public float Suitability { get; }
        public float GrowthSpeed { get; }
        public bool IsValid { get; }
        public CropCellReadout(long id, float suitability, float growthSpeed, bool valid)
        {
            if (id <= 0 || !SoilCellReadout.Finite(suitability) || suitability < 0 || suitability > 1 ||
                !SoilCellReadout.Finite(growthSpeed) || growthSpeed < 0) throw new ArgumentException("Invalid crop cell.");
            Id = id; Suitability = suitability; GrowthSpeed = growthSpeed; IsValid = valid;
        }
    }
    public sealed class SensorSnapshot
    {
        public long Version { get; }
        public string PublicStatus { get; }
        public long? ResourceAmount { get; }
        public bool Infinite { get; }
        public IReadOnlyList<SoilCellReadout> Soil { get; }
        public IReadOnlyList<CropCellReadout> Crops { get; }
        public SensorSnapshot(long version, string publicStatus = null, long? resourceAmount = null, bool infinite = false,
            SoilCellReadout[] soil = null, CropCellReadout[] crops = null)
        {
            if (version < 0 || resourceAmount < 0 || (infinite && resourceAmount.HasValue)) throw new ArgumentException("Invalid snapshot.");
            Version = version; PublicStatus = publicStatus; ResourceAmount = resourceAmount; Infinite = infinite;
            if (soil != null)
            {
                var ids = new HashSet<long>(); foreach (var cell in soil) if (cell.Id <= 0 || !ids.Add(cell.Id)) throw new ArgumentException("Duplicate soil ID.");
                Soil = new ReadOnlyCollection<SoilCellReadout>((SoilCellReadout[])soil.Clone());
            }
            if (crops != null)
            {
                var ids = new HashSet<long>(); foreach (var cell in crops) if (cell.Id <= 0 || !ids.Add(cell.Id)) throw new ArgumentException("Duplicate crop ID.");
                Crops = new ReadOnlyCollection<CropCellReadout>((CropCellReadout[])crops.Clone());
            }
        }
        public bool TryGetWeightedMoisture(out double value)
        {
            value = 0; double area = 0;
            if (Soil == null || Soil.Count == 0) return false;
            for (int i = 0; i < Soil.Count; i++) { var cell = Soil[i]; if (!cell.IsValid) return false; area += cell.Area; value += cell.Area * cell.Moisture; }
            value /= area; return true;
        }
    }
    public interface ISensorReadProvider
    {
        PersistentObjectReference Target { get; }
        bool IsAvailable { get; }
        bool Supports(SensorKind kind);
        Vector3 ClosestPoint(Vector3 anchor);
        bool TryRead(SensorKind kind, out SensorSnapshot snapshot);
    }
    public interface ISensorAnchor { bool TryGetPosition(out Vector3 position); }
    public interface ISensorEnvironment
    {
        bool IsActive { get; }
        bool Contains(PersistentObjectReference target);
        bool TryGetProvider(PersistentObjectReference target, out ISensorReadProvider provider);
    }
}
