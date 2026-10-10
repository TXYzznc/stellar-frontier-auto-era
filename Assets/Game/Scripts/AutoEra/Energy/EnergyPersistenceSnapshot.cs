using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.Energy
{
    public sealed class EnergyEventRecorderSnapshot
    {public ulong[] Stopped,DryFuel;public bool StorageHadCharge,StorageKnown,WasShort;}
    public sealed partial class EnergyEventRecorder
    {
        public EnergyEventRecorderSnapshot CapturePersistent()
        {
            var stopped=new List<ulong>();foreach(var id in _stopped)stopped.Add(id.Value);stopped.Sort();
            var dry=new List<ulong>();foreach(var id in _dryFuel)dry.Add(id.Value);dry.Sort();
            return new EnergyEventRecorderSnapshot {Stopped=stopped.ToArray(),DryFuel=dry.ToArray(),StorageHadCharge=_storageHadCharge,StorageKnown=_storageKnown,WasShort=_wasShort};
        }
        public bool RestorePersistent(EnergyEventRecorderSnapshot saved)
        {
            if(saved?.Stopped==null || saved.DryFuel==null || _stopped.Count!=0 || _dryFuel.Count!=0 || _storageKnown || _wasShort)return false;
            var stopped=new HashSet<PersistentId>();var dry=new HashSet<PersistentId>();
            foreach(ulong id in saved.Stopped)if(id==0 || !stopped.Add(new PersistentId(id)))return false;
            foreach(ulong id in saved.DryFuel)if(id==0 || !dry.Add(new PersistentId(id)))return false;
            foreach(var id in stopped)_stopped.Add(id);foreach(var id in dry)_dryFuel.Add(id);
            _storageHadCharge=saved.StorageHadCharge;_storageKnown=saved.StorageKnown;_wasShort=saved.WasShort;return true;
        }
    }
    public sealed class EnergyPersistenceSnapshot
    {
        public int Version=1;
        public long Revision,NextQueueOrder;
        public GeneratorRecord[] Generators;
        public StorageRecord[] Storages;
        public ConsumerRecord[] Consumers;
        public GridRecord LastSettlement;
        public sealed class GeneratorRecord
        { public ulong Id;public GeneratorKind Kind;public bool On,AllowsCharging;public float Rated,Environment,Fuel,Target,Output,Consumed; }
        public sealed class StorageRecord
        { public ulong Id;public float Capacity,Charge,Power;public StorageState State; }
        public sealed class ConsumerRecord
        {
            public ulong Id;public bool Machine,Demand,Working,Powered,Stopped;public PowerPriority Priority;
            public long QueueOrder;public float Standby,WorkingPower,Actual;
        }
        public sealed class GridRecord
        { public float Generated,Consumed,Stored,Capacity,Discarded,Charging,Discharging;public bool Shortfall;public ulong[] Stopped; }
    }
    public sealed partial class EnergyGrid
    {
        private bool _ticking;
        public bool TryCapturePersistent(out EnergyPersistenceSnapshot saved)
        {
            saved=null;if(_ticking)return false;
            var generators=new EnergyPersistenceSnapshot.GeneratorRecord[_generators.Count];
            for(int i=0;i<generators.Length;i++)
            {
                var g=_generators[i];if(!(g is EnvironmentGenerator) && !(g is FuelGenerator))return false;
                generators[i]=new EnergyPersistenceSnapshot.GeneratorRecord {Id=g.Id.Value,Kind=g.Kind,On=g.IsOn,Rated=g.RatedPower,
                    Environment=g.EnvironmentPower,Fuel=g.FuelEnergyAvailable,AllowsCharging=g.AllowsCharging,Target=g.ChargeTargetRatio,
                    Output=g.ActualOutputPower,Consumed=g.FuelEnergyConsumed};
            }
            var storages=new EnergyPersistenceSnapshot.StorageRecord[_storages.Count];
            for(int i=0;i<storages.Length;i++)
            {var s=_storages[i];if(!(s is BatteryStorage))return false;storages[i]=new EnergyPersistenceSnapshot.StorageRecord {Id=s.Id.Value,Capacity=s.Capacity,Charge=s.Charge,State=s.State,Power=s.ActualPower};}
            var consumers=new EnergyPersistenceSnapshot.ConsumerRecord[_consumers.Count];
            for(int i=0;i<consumers.Length;i++)
            {
                var c=_consumers[i];var building=c as EnergyConsumer;if(building==null && !(c is MachineEnergyConsumer))return false;
                consumers[i]=new EnergyPersistenceSnapshot.ConsumerRecord {Id=c.Id.Value,Machine=c is MachineEnergyConsumer,Priority=c.Priority,QueueOrder=c.QueueOrder,
                    Powered=c.IsPowered,Stopped=c.IsStoppedByShortage,Actual=c.ActualPower,Demand=building?.IsDemandActive ?? false,Working=building?.IsWorking ?? false,
                    Standby=building?.StandbyPower ?? 0,WorkingPower=building?.WorkingPower ?? 0};
            }
            var stops=new ulong[Snapshot.StoppedByShortage.Count];for(int i=0;i<stops.Length;i++)stops[i]=Snapshot.StoppedByShortage[i].Value;
            saved=new EnergyPersistenceSnapshot {Revision=Revision,NextQueueOrder=_queueCounter,Generators=generators,Storages=storages,Consumers=consumers,
                LastSettlement=new EnergyPersistenceSnapshot.GridRecord {Generated=Snapshot.GeneratedPower,Consumed=Snapshot.ConsumedPower,Stored=Snapshot.StoredCharge,
                    Capacity=Snapshot.StorageCapacity,Discarded=Snapshot.DiscardedPower,Charging=Snapshot.ChargingPower,Discharging=Snapshot.DischargingPower,Shortfall=Snapshot.HasShortfall,Stopped=stops}};
            return true;
        }
        /// <summary>Build an unpublished grid in saved registration order. Never tick or notify the borrowed machines.</summary>
        public static bool TryRestorePersistent(EnergyPersistenceSnapshot saved,Func<PersistentId,MachineInstance> resolveMachine,out EnergyGrid restored,out string reason)
        {
            restored=null;reason="能源检查点不完整";
            if(saved?.Version!=1 || saved.Revision<0 || saved.NextQueueOrder<0 || saved.Generators==null || saved.Storages==null || saved.Consumers==null || saved.LastSettlement?.Stopped==null)return false;
            var candidate=new EnergyGrid();var ids=new HashSet<ulong>();var orders=new HashSet<long>();var consumers=new Dictionary<ulong,IEnergyConsumer>();
            bool Own(ulong id)=>id!=0 && ids.Add(id);
            bool Nonnegative(float value)=>!float.IsNaN(value) && !float.IsInfinity(value) && value>=0;
            bool Finite(float value)=>!float.IsNaN(value) && !float.IsInfinity(value);
            try
            {
                foreach(var row in saved.Generators)
                {
                    if(row==null || !Own(row.Id) || !Enum.IsDefined(typeof(GeneratorKind),row.Kind) || !Nonnegative(row.Rated) || !Nonnegative(row.Environment) ||
                        !Nonnegative(row.Fuel) || !Nonnegative(row.Target) || row.Target>1 || !Nonnegative(row.Output) || !Nonnegative(row.Consumed))throw new ArgumentException("Invalid original generator.");
                    IEnergyGenerator g;
                    if(row.Kind==GeneratorKind.Environment)
                    {
                        if(row.Fuel!=0 || row.AllowsCharging || row.Target!=0 || row.Consumed!=0 || row.Environment>row.Rated)throw new ArgumentException("Invalid environment energy facts.");
                        g=new EnvironmentGenerator(new PersistentId(row.Id),row.Rated) {IsOn=row.On,EnvironmentPower=row.Environment,ActualOutputPower=row.Output};
                    }
                    else
                    {
                        if(row.Environment!=0)throw new ArgumentException("Fuel generation has environment output.");
                        g=new FuelGenerator(new PersistentId(row.Id),row.Rated,row.Fuel) {IsOn=row.On,AllowsCharging=row.AllowsCharging,ChargeTargetRatio=row.Target,
                            ActualOutputPower=row.Output,FuelEnergyConsumed=row.Consumed};
                    }
                    candidate.AddGenerator(g);
                }
                foreach(var row in saved.Storages)
                {
                    if(row==null || !Own(row.Id) || !Nonnegative(row.Capacity) || row.Capacity==0 || !Nonnegative(row.Charge) || row.Charge>row.Capacity ||
                        !Finite(row.Power) || !Enum.IsDefined(typeof(StorageState),row.State))throw new ArgumentException("Invalid original storage.");
                    candidate.AddStorage(new BatteryStorage(new PersistentId(row.Id),row.Capacity,row.Charge) {State=row.State,ActualPower=row.Power});
                }
                foreach(var row in saved.Consumers)
                {
                    if(row==null || !Own(row.Id) || !Enum.IsDefined(typeof(PowerPriority),row.Priority) || row.QueueOrder<0 || row.QueueOrder>=saved.NextQueueOrder ||
                        !orders.Add(row.QueueOrder) || !Nonnegative(row.Standby) || !Nonnegative(row.WorkingPower) || !Nonnegative(row.Actual) ||
                        row.Stopped && (row.Powered || row.Actual!=0))throw new ArgumentException("Invalid original power queue.");
                    IEnergyConsumer consumer;
                    if(row.Machine)
                    {
                        var machine=resolveMachine?.Invoke(new PersistentId(row.Id));
                        if(machine==null || machine.Id.Value!=row.Id || !machine.Deployed || row.Standby!=0 || row.WorkingPower!=0 || row.Demand || row.Working)
                            throw new ArgumentException("Missing original energy machine.");
                        consumer=new MachineEnergyConsumer(machine,row.Priority);
                    }
                    else consumer=new EnergyConsumer(new PersistentId(row.Id),row.Priority,row.Standby,row.WorkingPower) {IsDemandActive=row.Demand,IsWorking=row.Working};
                    candidate.AddConsumer(consumer);consumer.QueueOrder=row.QueueOrder;consumer.IsPowered=row.Powered;consumer.ActualPower=row.Actual;consumer.IsStoppedByShortage=row.Stopped;
                    consumers.Add(row.Id,consumer);
                }
                var last=saved.LastSettlement;
                if(!Nonnegative(last.Generated) || !Nonnegative(last.Consumed) || !Nonnegative(last.Stored) || !Nonnegative(last.Capacity) || last.Stored>last.Capacity ||
                    !Nonnegative(last.Discarded) || !Nonnegative(last.Charging) || !Nonnegative(last.Discharging))throw new ArgumentException("Invalid original energy read model.");
                var stops=new PersistentId[last.Stopped.Length];var seen=new HashSet<ulong>();
                for(int i=0;i<stops.Length;i++)
                {ulong id=last.Stopped[i];if(!seen.Add(id) || !consumers.TryGetValue(id,out var c) || !c.IsStoppedByShortage)throw new ArgumentException("Invalid original shortage order.");stops[i]=new PersistentId(id);}
                foreach(var pair in consumers)if(pair.Value.IsStoppedByShortage!=seen.Contains(pair.Key))throw new ArgumentException("Missing shortage read model.");
                candidate._queueCounter=saved.NextQueueOrder;candidate._publishedStops=Array.AsReadOnly(stops);
                candidate.Snapshot=new EnergyGridSnapshot(last.Generated,last.Consumed,last.Stored,last.Capacity,last.Discarded,last.Charging,last.Discharging,candidate._publishedStops,last.Shortfall);
                candidate.Revision=saved.Revision;restored=candidate;reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            {reason=error.Message;return false;}
        }
    }
}
