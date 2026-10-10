using System;
using AutoEra.Machines.Sensors;
using AutoEra.World.Region;

namespace AutoEra.ResourcePoints
{
    public sealed class ProductionFacilityPublicSnapshot
    {
        public ulong Point;
        public long Version,NavigationRevision,DomainRevision,CargoRevision;
        public SensorReadoutSnapshot Cached;
    }
    public sealed partial class RegionProductionFacility
    {
        public ProductionFacilityPublicSnapshot CapturePublicPersistent()
        {
            if(!IsAvailable)throw new InvalidOperationException("Production view is unavailable.");
            return new ProductionFacilityPublicSnapshot {Point=Target.Id.Value,Version=_version,NavigationRevision=NavigationRevision,
                DomainRevision=_previousDomainRevision,CargoRevision=_previousCargoRevision,Cached=SensorReadoutSnapshot.Capture(_snapshot)};
        }
        public bool RestorePublicPersistent(ProductionFacilityPublicSnapshot saved)
        {
            if(!IsAvailable || saved==null || _version!=0 || _snapshot!=null || saved.Point!=Target.Id.Value || saved.Version<0 || saved.NavigationRevision<0 ||
                saved.DomainRevision< -1 || saved.CargoRevision< -1 || saved.DomainRevision>(Forest?.Revision ?? Mineral.Revision) || saved.CargoRevision>_world.Resources.Authority.Revision ||
                saved.Cached!=null && saved.Cached.Version!=saved.Version)return false;
            if(saved.Cached?.Trees!=null)
            {
                if(Forest==null || saved.Cached.Trees.Length!=Forest.Count)return false;
                for(int i=0;i<Forest.Count;i++)if(saved.Cached.Trees[i].Id!=Forest.ReadAt(i).Id)return false;
            }
            SensorSnapshot cached;
            try {cached=saved.Cached?.Build();}catch(ArgumentException) {return false;}
            _version=saved.Version;NavigationRevision=saved.NavigationRevision;_previousDomainRevision=saved.DomainRevision;_previousCargoRevision=saved.CargoRevision;_snapshot=cached;return true;
        }
    }
}
