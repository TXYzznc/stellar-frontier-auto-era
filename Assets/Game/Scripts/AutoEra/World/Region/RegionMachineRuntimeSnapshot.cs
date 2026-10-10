using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    public sealed class RegionMachineRuntimeSnapshot
    {
        public PersistentId Machine;
        public ulong HardwareRevision;
        public MachineTaskQueueSnapshot Tasks;
        public MachineComputeSnapshot Compute;
        public RegionHardwareSnapshot Hardware;
        public AlgorithmInstanceSnapshot[] Algorithms;
        public AlgorithmMachineAdapterSnapshot Adapter;
        public MachineNavigationSnapshot Navigation;
    }
    public sealed partial class RegionMachineRuntime
    {
        public bool TryCapturePersistent(long now,double navigationSeconds,out RegionMachineRuntimeSnapshot snapshot)
        {
            snapshot=null;
            if(_disposed || _advancing || _persistentRestoreIncomplete || _pendingRemoval.Count!=0 || now<0 || now<_worldMilliseconds ||
                Instances==null || Hardware==null || !Context.Compute.TryCapturePersistent(out var compute) ||
                !Hardware.TryCapturePersistent(now,out var hardware) || !Instances.TryCapturePersistent(now,out var algorithms) ||
                !Adapter.TryCapturePersistent(now,out var adapter)) return false;
            MachineNavigationSnapshot navigation=null;
            if(Navigation!=null && !Navigation.TryCapturePersistent(navigationSeconds,out navigation)) return false;
            snapshot=new RegionMachineRuntimeSnapshot { Machine=MachineId,HardwareRevision=_hardware.Read(),Tasks=Context.Tasks.Capture(),
                Compute=compute,Hardware=hardware,Algorithms=algorithms,Adapter=adapter,Navigation=navigation };
            return ValidateLeaseOwnership(snapshot);
        }

        /// <summary>Unpublished candidate only. On failure discard the entire candidate; never resume a partially restored runtime.</summary>
        public bool RestorePersistent(RegionMachineRuntimeSnapshot snapshot,long now,double navigationSeconds,
            Func<MachineNavigationTargetSnapshot,MachineNavigationTarget> resolveNavigation,out string reason)
        {
            reason="InvalidMachineRuntimeSnapshot";
            if(_disposed || _advancing || _persistentRestoreIncomplete || now<0 || snapshot==null || snapshot.Machine!=MachineId ||
                snapshot.Tasks==null || snapshot.Compute==null || snapshot.Hardware==null || snapshot.Algorithms==null || snapshot.Adapter==null ||
                Instances==null || Hardware==null || Adapter.HasRuntime || Context.Compute.Used!=0 || Context.Compute.WaitingCount!=0 ||
                !ValidateLeaseOwnership(snapshot) || snapshot.Navigation!=null && (Navigation==null || resolveNavigation==null)) return false;
            _persistentRestoreIncomplete=true;Context.BeginPersistentRestore();
            try
            {
                _worldMilliseconds=now;Adapter.SetWorldTime(now);Hardware.SetWorldTime(now);
                Context.Tasks.Restore(snapshot.Tasks);
                if(!Context.Compute.RestorePersistent(snapshot.Compute,now) || !_hardware.RestorePersistentRevision(snapshot.HardwareRevision)) return false;
                if(!Hardware.RestorePersistent(snapshot.Hardware,now,out reason)) return false;
                var restored=new List<AlgorithmRuntime>();
                if(!Instances.RestorePersistent(snapshot.Algorithms,now,(id,plan)=>
                {
                    var runtime=new AlgorithmRuntime(new PersistentId(id),plan,Context.Compute,Adapter.CreateInstanceSink(id));
                    restored.Add(runtime);return runtime;
                },out reason) || Context.Compute.AppliedLogicCost!=snapshot.Compute.AppliedLogicCost) return false;
                foreach(var runtime in restored) Adapter.Attach(runtime);
                if(snapshot.Navigation!=null && !Navigation.RestorePersistent(snapshot.Navigation,navigationSeconds,resolveNavigation)) return false;
                if(!Adapter.RestorePersistent(snapshot.Adapter,now,PersistentTransport)) return false;
                Context.CompletePersistentRestore();_persistentRestoreIncomplete=false;reason=null;return true;
            }
            catch(ArgumentException error) { reason=error.Message;return false; }
            catch(InvalidOperationException error) { reason=error.Message;return false; }
            catch(OverflowException error) { reason=error.Message;return false; }
            finally { if(_persistentRestoreIncomplete && string.IsNullOrEmpty(reason)) reason="InvalidMachineRuntimeSnapshot"; }
        }

        internal AutoEra.Logistics.TransportResponsibilityLedger PersistentTransport { private get; set; }

        private static bool ValidateLeaseOwnership(RegionMachineRuntimeSnapshot snapshot)
        {
            if(snapshot.Compute?.Running==null || snapshot.Compute.Waiting==null || snapshot.Algorithms==null || snapshot.Hardware?.Sensors==null) return false;
            var leases=new HashSet<PersistentId>();var claims=new HashSet<PersistentId>();
            foreach(var lease in snapshot.Compute.Running) if(lease==null || !leases.Add(lease.Id)) return false;
            foreach(var lease in snapshot.Compute.Waiting) if(lease==null || !leases.Add(lease.Id)) return false;
            bool Claim(PersistentId id) => !id.IsValid || leases.Contains(id) && claims.Add(id);
            foreach(var row in snapshot.Algorithms)
            {
                if(row==null) return false;
                var runtime=row.Runtime;if(runtime==null) continue;
                if(runtime.StateLeases==null || runtime.Delays==null) return false;
                foreach(var lease in runtime.StateLeases.Values) if(!Claim(lease)) return false;
                foreach(var delay in runtime.Delays) if(delay==null || !Claim(delay.ComputeLeaseId)) return false;
                if(runtime.Prepared!=null && !Claim(runtime.Prepared.ComputeLeaseId)) return false;
            }
            foreach(var sensor in snapshot.Hardware.Sensors) if(sensor==null || !Claim(sensor.ComputeLeaseId)) return false;
            if(snapshot.Navigation!=null && (!Claim(snapshot.Navigation.PlanningLeaseId)||!Claim(snapshot.Navigation.MovingLeaseId))) return false;
            return claims.SetEquals(leases);
        }
    }
}
