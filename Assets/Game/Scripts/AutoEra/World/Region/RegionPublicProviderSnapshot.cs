using System;
using System.Collections.Generic;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    public sealed class RegionPublicProviderSnapshot
    {
        public PersistentObjectReference Target;
        public long Version;
        public bool Dirty;
        public SensorReadoutSnapshot Cached;
    }
    public sealed partial class RegionSensorReadProvider
    {
        internal RegionPublicProviderSnapshot CapturePersistent()
        {
            if(_disposed) throw new ObjectDisposedException(nameof(RegionSensorReadProvider));
            return new RegionPublicProviderSnapshot { Target=Target,Version=_version,Dirty=_dirty,Cached=SensorReadoutSnapshot.Capture(_snapshot) };
        }
        internal bool RestorePersistent(RegionPublicProviderSnapshot state)
        {
            if(_disposed || _version!=0 || _snapshot!=null || state==null || state.Target!=Target || state.Version<0 ||
                state.Cached==null && (state.Version!=0 || !state.Dirty) || state.Cached!=null && state.Cached.Version!=state.Version) return false;
            SensorSnapshot cached;
            try { cached=state.Cached?.Build(); } catch(ArgumentException) { return false; }
            if(!state.Dirty && cached!=null && (cached.PublicStatus!=_target.PublicStatus || cached.ResourceAmount!=_target.PublicResourceAmount ||
                cached.Infinite!=_target.ResourceIsInfinite || cached.CachedAmount!=_target.PublicCachedAmount || cached.CacheCapacity!=_target.PublicCacheCapacity)) return false;
            _version=state.Version;_dirty=state.Dirty;_snapshot=cached;return true;
        }
    }
    public sealed partial class RegionSensorEnvironment
    {
        internal RegionPublicProviderSnapshot[] CapturePublicPersistent()
        {
            if(_disposed) throw new ObjectDisposedException(nameof(RegionSensorEnvironment));
            var rows=new List<RegionPublicProviderSnapshot>(_owned.Count);
            foreach(var provider in _owned.Values) rows.Add(provider.CapturePersistent());
            rows.Sort((a,b)=>a.Target.Id.CompareTo(b.Target.Id));return rows.ToArray();
        }
        internal bool RestorePublicPersistent(RegionPublicProviderSnapshot[] state)
        {
            if(_disposed || state==null || state.Length!=_owned.Count) return false;
            var ids=new HashSet<PersistentId>();
            foreach(var row in state) if(row==null || !ids.Add(row.Target.Id) || !_owned.ContainsKey(row.Target.Id)) return false;
            foreach(var row in state) if(!_owned[row.Target.Id].RestorePersistent(row)) return false;
            return true;
        }
    }
}
