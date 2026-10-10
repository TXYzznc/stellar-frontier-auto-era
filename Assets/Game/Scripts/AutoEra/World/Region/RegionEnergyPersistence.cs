using System;
using System.Collections.Generic;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.World.Region
{
    public sealed class RegionEnergyPersistenceSnapshot
    {
        public long CapturedAt,LastSettlementAt;
        public bool LastDaylight;
        public ulong[] FacilityOrder;
        public EnergyPersistenceSnapshot Grid;
    }
    public sealed partial class RegionEnergyService
    {
        private long _lastWorldMilliseconds=-1;
        private ulong[] _restoredFacilityOrder;
        public bool TryCapturePersistent(long worldMilliseconds,out RegionEnergyPersistenceSnapshot saved)
        {
            saved=null;
            if(_disposed || worldMilliseconds<0 || _lastWorldMilliseconds>worldMilliseconds || !_grid.TryCapturePersistent(out var grid))return false;
            var order=_restoredFacilityOrder==null ? new ulong[_facilities.Count] : (ulong[])_restoredFacilityOrder.Clone();
            if(_restoredFacilityOrder==null)for(int i=0;i<order.Length;i++)order[i]=_facilities[i].ObjectId.Value;
            saved=new RegionEnergyPersistenceSnapshot {CapturedAt=worldMilliseconds,LastSettlementAt=_lastWorldMilliseconds,LastDaylight=LastDaylight,FacilityOrder=order,Grid=grid};return true;
        }
        public static bool TryRestorePersistent(RegionEnergyPersistenceSnapshot saved,long worldMilliseconds,MachineRoster roster,out RegionEnergyService restored,out string reason)
        {
            restored=null;reason="区域能源检查点不完整";
            if(saved==null || saved.CapturedAt!=worldMilliseconds || saved.LastSettlementAt< -1 || saved.LastSettlementAt>worldMilliseconds || saved.FacilityOrder==null || roster==null ||
                saved.LastSettlementAt>=0 && saved.LastDaylight!=DaylightCycle.IsDaylight(saved.LastSettlementAt))return false;
            if(!EnergyGrid.TryRestorePersistent(saved.Grid,id=>roster.TryGet(id,out var machine) ? machine : null,out var grid,out reason))return false;
            var ids=new HashSet<ulong>();foreach(var g in grid.Generators)ids.Add(g.Id.Value);foreach(var s in grid.Storages)ids.Add(s.Id.Value);
            var order=new HashSet<ulong>();foreach(ulong id in saved.FacilityOrder)if(!ids.Contains(id) || !order.Add(id))return false;
            if(!ids.SetEquals(order))return false;
            var service=new RegionEnergyService {_grid=grid,_lastWorldMilliseconds=saved.LastSettlementAt,LastDaylight=saved.LastDaylight,_restoredFacilityOrder=(ulong[])saved.FacilityOrder.Clone()};
            foreach(var consumer in grid.Consumers)
            {
                if(!(consumer is MachineEnergyConsumer machine)) {reason="区域尚未接入该建筑负载的真实宿主";return false;}
                service._machines.Add(machine.Id,machine);
            }
            int deployed=0;foreach(var machine in roster.Machines)if(machine.Deployed)
            {deployed++;if(!service._machines.ContainsKey(machine.Id)) {reason="已部署机器缺少原供电队列";return false;} }
            if(deployed!=service._machines.Count)return false;
            service.Reconcile(roster);restored=service;reason=null;return true;
        }
        /// <summary>Bind every scene view only after validation; restored domain objects remain the authority.</summary>
        public bool TryBindPersistentFacilities(IEnumerable<RegionEnergyFacility> facilities,out string reason)
        {
            reason="能源资产与原检查点不匹配";if(_disposed || _restoredFacilityOrder==null || facilities==null || _facilities.Count!=0)return false;
            var views=new Dictionary<ulong,RegionEnergyFacility>();foreach(var view in facilities)
                if(view==null || !view.ObjectId.IsValid || !views.TryAdd(view.ObjectId.Value,view))return false;
            if(views.Count!=_restoredFacilityOrder.Length)return false;
            var generators=new Dictionary<ulong,IEnergyGenerator>();foreach(var g in _grid.Generators)generators.Add(g.Id.Value,g);
            var storages=new Dictionary<ulong,IEnergyStorage>();foreach(var s in _grid.Storages)storages.Add(s.Id.Value,s);
            foreach(ulong id in _restoredFacilityOrder)
                if(!views.TryGetValue(id,out var view) || !view.CanBindPersistent(new PersistentId(id),generators.TryGetValue(id,out var g) ? g : null,storages.TryGetValue(id,out var s) ? s : null))return false;
            foreach(ulong id in _restoredFacilityOrder)
            {
                var view=views[id];view.BindPersistent(new PersistentId(id),generators.TryGetValue(id,out var g) ? g : null,storages.TryGetValue(id,out var s) ? s : null);_facilities.Add(view);
            }
            _restoredFacilityOrder=null;
            reason=null;return true;
        }
    }
    public sealed partial class RegionEnergyFacility
    {
        internal bool CanBindPersistent(PersistentId id,IEnergyGenerator generator,IEnergyStorage storage)
        {
            if(!id.IsValid || ObjectId!=id)return false;
            if(_kind==RegionEnergyFacilityKind.Battery)return generator==null && storage is BatteryStorage && storage.Id==id && storage.Capacity==_capacity;
            return storage==null && generator!=null && generator.Id==id && generator.RatedPower==_ratedPower &&
                (_kind==RegionEnergyFacilityKind.EnvironmentGenerator ? generator is EnvironmentGenerator : _kind==RegionEnergyFacilityKind.FuelGenerator && generator is FuelGenerator);
        }
        internal void BindPersistent(PersistentId id,IEnergyGenerator generator,IEnergyStorage storage)
        {
            if(!CanBindPersistent(id,generator,storage))throw new ArgumentException("Invalid saved energy view binding.");
            _environment=generator as EnvironmentGenerator;_fuel=generator as FuelGenerator;_battery=storage as BatteryStorage;
        }
    }
}
