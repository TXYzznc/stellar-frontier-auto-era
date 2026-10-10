using System;
using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.Machines.Sensors;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    public sealed class RegionHardwareEffectorSnapshot
    {
        public PersistentId Component, ActiveRequest;
        public ulong Generation;
        public EffectorBehaviorSnapshot<EffectorBehaviorParameters> Queue;
        public RegionEffectorOperationSnapshot Operation;
    }
    public sealed class RegionHardwareSnapshot
    {
        public PersistentId Machine;
        public ulong GenerationsAllocatedThrough;
        public MachineSensorSnapshot[] Sensors;
        public RegionHardwareEffectorSnapshot[] Effectors;
        public Dictionary<ulong,string> Unavailable;
    }
    public sealed partial class RegionHardwareRuntime
    {
        public bool TryCapturePersistent(long now,out RegionHardwareSnapshot snapshot)
        {
            snapshot=null;
            if(_disposed || _reconciling || _updatingPause || _advancing || now<0 || now<_now) return false;
            var sensors=new List<MachineSensorSnapshot>(_sensors.Count);
            foreach(var sensor in _sensors.Values)
            { if(!sensor.TryCapturePersistent(now,out var value)) return false;sensors.Add(value); }
            sensors.Sort((a,b)=>a.ComponentId.CompareTo(b.ComponentId));
            var effectors=new List<RegionHardwareEffectorSnapshot>(_ordered.Count);
            foreach(var item in _ordered)
            {
                if(!item.Queue.TryCapturePersistent(EffectorParameterSnapshot.Copy,out var queue)) return false;
                RegionEffectorOperationSnapshot operation=null;
                if(item.Operation!=null && (!(item.Operation is IRegionPersistentEffectorOperation source) || !source.TryCapturePersistent(now,out operation))) return false;
                if(item.Request!=null && (item.Operation==null || !ReferenceEquals(item.Request,item.Queue.Current))) return false;
                effectors.Add(new RegionHardwareEffectorSnapshot { Component=item.Component.Id,Generation=item.Generation,
                    Queue=queue,ActiveRequest=item.Request?.Id ?? PersistentId.Invalid,Operation=operation });
            }
            var unavailable=new Dictionary<ulong,string>();foreach(var pair in _unavailable)unavailable.Add(pair.Key.Value,pair.Value);
            snapshot=new RegionHardwareSnapshot { Machine=_context.Machine.Id,GenerationsAllocatedThrough=_generation,
                Sensors=sensors.ToArray(),Effectors=effectors.ToArray(),Unavailable=unavailable };
            return true;
        }

        /// <summary>Only on an unpublished fresh candidate, after compute/tasks and physical authorities. A failure requires disposing the candidate.</summary>
        internal bool RestorePersistent(RegionHardwareSnapshot snapshot,long now,out string reason)
        {
            reason="InvalidHardwareSnapshot";
            if(_disposed || _reconciling || _advancing || snapshot?.Sensors==null || snapshot.Effectors==null || snapshot.Unavailable==null ||
                snapshot.Machine!=_context.Machine.Id || now<0 || snapshot.Sensors.Length!=_sensors.Count || snapshot.Effectors.Length!=_effectors.Count) return false;
            var seen=new HashSet<PersistentId>();var generations=new HashSet<ulong>();
            foreach(var row in snapshot.Sensors)
                if(row==null || !_sensors.ContainsKey(row.ComponentId) || !seen.Add(row.ComponentId)) return false;
            foreach(var row in snapshot.Effectors)
            {
                if(row==null || !_effectors.TryGetValue(row.Component,out var item) || !seen.Add(row.Component) || row.Generation==0 ||
                    row.Generation>snapshot.GenerationsAllocatedThrough || !generations.Add(row.Generation) || row.Queue?.Waiting==null ||
                    item.Queue.Current!=null || item.Queue.WaitingCount!=0 || item.Operation!=null || item.Request!=null ||
                    row.ActiveRequest.IsValid!=(row.Operation!=null) || row.ActiveRequest.IsValid && row.Queue.Current?.Id!=row.ActiveRequest) return false;
            }
            foreach(var pair in snapshot.Unavailable)
                if(!Installed(new PersistentId(pair.Key),HardwareKind.Sensor) && !Installed(new PersistentId(pair.Key),HardwareKind.Effector) || string.IsNullOrWhiteSpace(pair.Value)) return false;
            foreach(var row in snapshot.Sensors) if(!_sensors[row.ComponentId].RestorePersistent(row,now)) return false;
            foreach(var row in snapshot.Effectors)
            {
                var item=_effectors[row.Component];
                if(!item.Queue.RestorePersistent(row.Queue,EffectorParameterSnapshot.Valid,EffectorParameterSnapshot.Copy)) return false;
                item.Generation=row.Generation;_adapter.RegisterEffector(item.Component,item.Queue,item.Generation);
                if(row.Operation!=null)
                {
                    var current=item.Queue.Current;
                    if(current==null || current.Id!=row.ActiveRequest || !Enum.TryParse(current.Parameters.Action,out AlgorithmEffectorAction action) ||
                        !_executors.TryGet(item.Component.Definition,action,out var executor) || !(executor is IRegionPersistentEffectorExecutor source) ||
                        !source.TryRestorePersistent(_context,item.Component,current,row.Operation,now,out var operation,out reason) || operation==null) return false;
                    item.Request=current;item.Operation=operation;
                }
            }
            _generation=snapshot.GenerationsAllocatedThrough;_now=now;
            _unavailable.Clear();foreach(var pair in snapshot.Unavailable)_unavailable.Add(new PersistentId(pair.Key),pair.Value);
            reason=null;return true;
        }
    }
}
