using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    public sealed class RegionExecutionSnapshot
    {
        public RegionPublicProviderSnapshot[] PublicProviders;
        public RegionMachineRuntimeSnapshot[] Machines;
    }
    public sealed partial class RegionMachineRuntimeRegistry
    {
        internal bool MatchesPersistentWorld(AutoEraWorldSession world,InitialRegion region)
            => !_disposed && ReferenceEquals(_session,world) && ReferenceEquals(_region,region);
        public bool TryCapturePersistent(long now,double navigationSeconds,out RegionExecutionSnapshot snapshot)
        {
            snapshot=null;
            if(_disposed || _advancing || _pendingDetach.Count!=0) return false;
            var rows=new List<RegionMachineRuntimeSnapshot>(_runtimes.Count);
            foreach(var runtime in _runtimes.Values)
            { if(!runtime.TryCapturePersistent(now,navigationSeconds,out var row)) return false;rows.Add(row); }
            rows.Sort((a,b)=>a.Machine.CompareTo(b.Machine));
            snapshot=new RegionExecutionSnapshot { Machines=rows.ToArray(),PublicProviders=SensorEnvironment.CapturePublicPersistent() };return true;
        }

        /// <summary>Restore all fresh candidate hosts after region identities, work queues and physical authorities. Discard the candidate on any failure.</summary>
        public bool RestorePersistent(RegionExecutionSnapshot snapshot,long now,double navigationSeconds,
            Func<MachineNavigationTargetSnapshot,MachineNavigationTarget> resolveNavigation,out string reason)
        {
            reason="InvalidRegionRuntimeSnapshot";
            if(_disposed || _advancing || snapshot?.Machines==null || snapshot.PublicProviders==null || snapshot.Machines.Length!=_runtimes.Count) return false;
            var seen=new HashSet<PersistentId>();
            foreach(var row in snapshot.Machines) if(row==null || !seen.Add(row.Machine) || !_runtimes.ContainsKey(row.Machine)) return false;
            if(!SensorEnvironment.RestorePublicPersistent(snapshot.PublicProviders)) return false;
            foreach(var row in snapshot.Machines)
                if(!_runtimes[row.Machine].RestorePersistent(row,now,navigationSeconds,resolveNavigation,out reason)) return false;
            reason=null;return true;
        }
    }
}
