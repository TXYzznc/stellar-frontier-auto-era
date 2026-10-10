using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    public sealed class TransportLedgerSnapshot
    {
        public long Revision;
        public Record[] Records;
        public Binding[] Bindings;
        public sealed class Record
        {
            public ulong Machine,Task,Source,Destination;
            public string Item,Reason;
            public int Loaded,Delivered,Requested,Reserved;
            public TransportPhase Phase;
            public bool Active;
        }
        public sealed class Binding {public ulong Transaction,Machine;public string Item;public bool Unloading;}
    }
    public sealed partial class TransportResponsibilityLedger
    {
        public TransportLedgerSnapshot CapturePersistent()
        {
            var records=new List<TransportLedgerSnapshot.Record>();var bindings=new List<TransportLedgerSnapshot.Binding>();
            foreach(var r in _records.Values)records.Add(new TransportLedgerSnapshot.Record {Machine=r.Machine.Value,Task=r.Task.Value,
                Source=r.Source.Value,Destination=r.Destination.Value,Item=r.Item,Reason=r.Reason,Loaded=r.Loaded,Delivered=r.Delivered,
                Requested=r.RequestedUnits,Reserved=r.ReservedUnits,Phase=r.Phase,Active=r.Active});
            records.Sort((a,b)=> {int id=a.Machine.CompareTo(b.Machine);return id!=0 ? id : string.CompareOrdinal(a.Item,b.Item);});
            foreach(var b in _transactions)bindings.Add(new TransportLedgerSnapshot.Binding {Transaction=b.Key.Value,Machine=b.Value.machine.Value,Item=b.Value.item,Unloading=b.Value.unloading});
            bindings.Sort((a,b)=>a.Transaction.CompareTo(b.Transaction));
            return new TransportLedgerSnapshot {Revision=Revision,Records=records.ToArray(),Bindings=bindings.ToArray()};
        }
        public bool TryRestorePersistent(TransportLedgerSnapshot saved,CargoOwnershipAuthority authority,out string reason)
        {
            reason="Invalid transport responsibility checkpoint";
            if(_records.Count!=0 || _transactions.Count!=0 || saved?.Records==null || saved.Bindings==null || saved.Revision<0 ||
                authority==null || !authority.TryCapturePersistent(out var cargo))return false;
            var records=new Dictionary<(PersistentId,string),TransportResponsibility>();
            var bindings=new Dictionary<PersistentId,(PersistentId machine,string item,bool unloading)>();
            var transactions=new Dictionary<ulong,CargoOwnershipSnapshot.TransactionRecord>();var lots=new Dictionary<ulong,CargoOwnershipSnapshot.LotRecord>();
            foreach(var row in cargo.Transactions)transactions.Add(row.Id,row);foreach(var row in cargo.Lots)lots.Add(row.Id,row);
            try
            {
                foreach(var row in saved.Records)
                {
                    if(row==null || row.Machine==0 || row.Machine>cargo.AllocatedThrough || row.Task==0 || row.Task>cargo.AllocatedThrough ||
                        row.Source==0 || row.Destination==0 || row.Source==row.Destination || row.Source>cargo.AllocatedThrough || row.Destination>cargo.AllocatedThrough ||
                        string.IsNullOrWhiteSpace(row.Item) || row.Loaded<0 || row.Delivered<0 || row.Delivered>row.Loaded || row.Requested<=0 || row.Reserved<0 || row.Reserved>row.Requested ||
                        !Enum.IsDefined(typeof(TransportPhase),row.Phase) || row.Active && row.Phase!=TransportPhase.Loading && row.Phase!=TransportPhase.Unloading || !row.Active && row.Reserved!=0)
                        throw new ArgumentException("Invalid original delivery responsibility.");
                    var machine=new PersistentId(row.Machine);
                    if(!authority.TryReadContainer(new CargoOwner(CargoOwnerKind.Receiver,machine),out var container) || container.Kind!=CargoContainerKind.MachineCargo ||
                        container.Count(row.Item)!=row.Loaded-row.Delivered)throw new ArgumentException("Delivery responsibility disagrees with actual cargo.");
                    var value=new TransportResponsibility {Machine=machine,Task=new PersistentId(row.Task),Source=new PersistentId(row.Source),Destination=new PersistentId(row.Destination),
                        Item=row.Item,Reason=row.Reason,Loaded=row.Loaded,Delivered=row.Delivered,RequestedUnits=row.Requested,ReservedUnits=row.Reserved,Phase=row.Phase,Active=row.Active};
                    if(!records.TryAdd((machine,row.Item),value))throw new ArgumentException("Duplicate machine/item delivery responsibility.");
                }
                var remaining=new Dictionary<(PersistentId,string),int>();
                foreach(var row in saved.Bindings)
                {
                    if(row==null || !records.TryGetValue((new PersistentId(row.Machine),row.Item),out var record) || !transactions.TryGetValue(row.Transaction,out var tx) ||
                        lots[tx.Lot].Item!=row.Item || (row.Unloading ? tx.Source!=row.Machine || tx.SourceKind!=CargoOwnerKind.Receiver : tx.Destination!=row.Machine || tx.DestinationKind!=CargoOwnerKind.Receiver))
                        throw new ArgumentException("Original delivery binding is missing.");
                    if(tx.Remaining>0 && (!record.Active || tx.Task!=record.Task.Value ||
                        (row.Unloading ? tx.Destination!=record.Destination.Value : tx.Source!=record.Source.Value)))throw new ArgumentException("Active delivery binding changed route or task.");
                    var key=(record.Machine,record.Item);remaining.TryGetValue(key,out int units);remaining[key]=checked(units+tx.Remaining);
                    if(!bindings.TryAdd(new PersistentId(row.Transaction),(record.Machine,record.Item,row.Unloading)))throw new ArgumentException("Duplicate delivery transaction binding.");
                }
                foreach(var pair in records)
                {remaining.TryGetValue(pair.Key,out int units);if(units!=pair.Value.ReservedUnits)throw new ArgumentException("Delivery reservation was lost.");}
                foreach(var pair in records)_records.Add(pair.Key,pair.Value);
                foreach(var pair in bindings)_transactions.Add(pair.Key,pair.Value);
                Revision=saved.Revision;reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            {reason=error.Message;return false;}
        }
    }
}
