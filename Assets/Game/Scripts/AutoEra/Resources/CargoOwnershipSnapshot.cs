using System;
using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    public sealed class CargoOwnershipSnapshot
    {
        public ulong AllocatedThrough;
        public long Revision;
        public ContainerRecord[] Containers;
        public LotRecord[] Lots;
        public TransactionRecord[] Transactions;
        public ReceiptRecord[] Receipts;
        public BalanceRecord[] Balances;
        public ProductionRecord[] Production;
        public sealed class ContainerRecord
        {
            public CargoOwnerKind OwnerKind;
            public ulong Owner,Generation;
            public CargoContainerKind Kind;
            public long Capacity,Used,ReservedCapacity,Revision;
            public bool Available;
            public ulong[] LotOrder;
        }
        public sealed class LotRecord
        {
            public ulong Id,Payload,Owner,Version;
            public CargoOwnerKind OwnerKind;
            public string Item;
            public int Units,Reserved;
        }
        public sealed class TransactionRecord
        {
            public ulong Id,Task,Lot,Source,Destination,SourceVersion,DestinationGeneration,LastSequence;
            public CargoOwnerKind SourceKind,DestinationKind;
            public int Limit,Requested,Total,Remaining;
            public WarehouseDestination Route;
            public ResourceTransferState State;
            public string Reason;
            public bool TerminalReported;
        }
        public sealed class ReceiptRecord
        {
            public ulong Transaction,Sequence,ReceivedLot;
            public int Actual,Total,Remaining;
            public ResourceTransferState State;
            public string Reason;
        }
        public sealed class BalanceRecord { public string Item;public long Units; }
        public sealed class ProductionRecord
        {
            public ulong Producer,Sequence,Owner,Lot;
            public CargoOwnerKind OwnerKind;
            public string Item;
            public int Units;
        }
    }

    public sealed partial class CargoOwnershipAuthority
    {
        public bool TryCapturePersistent(out CargoOwnershipSnapshot snapshot)
        {
            snapshot=null;if(!IsAtCommitBoundary)return false;
            var containers=new List<CargoOwnershipSnapshot.ContainerRecord>();
            foreach(var c in _containers.Values)
            {
                var order=new ulong[c.LotIds.Count];for(int i=0;i<order.Length;i++)order[i]=c.LotIds[i].Value;
                containers.Add(new CargoOwnershipSnapshot.ContainerRecord {Owner=c.Owner.Id.Value,OwnerKind=c.Owner.Kind,Kind=c.Kind,
                    Capacity=c.Capacity,Used=c.Used,ReservedCapacity=c.ReservedCapacity,Generation=c.Generation,Available=c.IsAvailable,Revision=c.Revision,LotOrder=order});
            }
            containers.Sort((a,b)=> {int id=a.Owner.CompareTo(b.Owner);return id!=0 ? id : a.OwnerKind.CompareTo(b.OwnerKind);});
            var lots=new List<CargoOwnershipSnapshot.LotRecord>();
            foreach(var l in _lots.Values)lots.Add(new CargoOwnershipSnapshot.LotRecord {Id=l.Id.Value,Payload=l.Payload.Value,Item=l.Item,
                Units=l.Units,Reserved=l.Reserved,Owner=l.Owner.Id.Value,OwnerKind=l.Owner.Kind,Version=l.Version});
            lots.Sort((a,b)=>a.Id.CompareTo(b.Id));
            var transactions=new List<CargoOwnershipSnapshot.TransactionRecord>();
            foreach(var t in _transactions.Values)
            {
                var r=t.Token;transactions.Add(new CargoOwnershipSnapshot.TransactionRecord {Id=r.TransactionId.Value,Task=r.TaskId.Value,Lot=r.LotId.Value,
                    Source=r.Source.Id.Value,SourceKind=r.Source.Kind,Destination=r.Destination.Id.Value,DestinationKind=r.Destination.Kind,
                    SourceVersion=r.SourceVersion,DestinationGeneration=r.DestinationGeneration,Limit=r.CommittedLimit,
                    Requested=t.Requested,Total=t.Total,Remaining=t.Remaining,LastSequence=t.LastSequence,State=t.State,Reason=t.Reason,
                    TerminalReported=t.TerminalReported,Route=t.Route});
            }
            transactions.Sort((a,b)=>a.Id.CompareTo(b.Id));
            var receipts=new List<CargoOwnershipSnapshot.ReceiptRecord>();
            foreach(var r in _receipts.Values)receipts.Add(new CargoOwnershipSnapshot.ReceiptRecord {Transaction=r.TransactionId.Value,Sequence=r.CompletionSequence,
                ReceivedLot=r.ReceivedLotId.Value,Actual=r.ActualUnits,Total=r.TotalCommittedUnits,Remaining=r.RemainingReservedUnits,State=r.State,Reason=r.Reason});
            receipts.Sort((a,b)=> {int id=a.Transaction.CompareTo(b.Transaction);return id!=0 ? id : a.Sequence.CompareTo(b.Sequence);});
            var balances=new List<CargoOwnershipSnapshot.BalanceRecord>();
            foreach(var b in _balances)balances.Add(new CargoOwnershipSnapshot.BalanceRecord {Item=b.Key,Units=b.Value});
            balances.Sort((a,b)=>string.CompareOrdinal(a.Item,b.Item));
            var production=new List<CargoOwnershipSnapshot.ProductionRecord>();
            foreach(var r in _production.Values)production.Add(new CargoOwnershipSnapshot.ProductionRecord {Producer=r.ProducerId.Value,Sequence=r.Sequence,
                Owner=r.Owner.Id.Value,OwnerKind=r.Owner.Kind,Item=r.Item,Units=r.Units,Lot=r.LotId.Value});
            production.Sort((a,b)=> {int id=a.Producer.CompareTo(b.Producer);return id!=0 ? id : a.Sequence.CompareTo(b.Sequence);});
            snapshot=new CargoOwnershipSnapshot {AllocatedThrough=_ids.IsExhausted ? ulong.MaxValue : _ids.NextId.Value-1,Revision=Revision,
                Containers=containers.ToArray(),Lots=lots.ToArray(),Transactions=transactions.ToArray(),Receipts=receipts.ToArray(),Balances=balances.ToArray(),Production=production.ToArray()};
            return true;
        }

        /// <summary>Validate detached facts, then publish to a fresh unpublished authority. Never mint, reserve, settle or notify.</summary>
        public bool TryRestorePersistent(CargoOwnershipSnapshot saved,out string reason)
        {
            reason="Invalid cargo checkpoint";
            if(!IsAtCommitBoundary || _containers.Count!=0 || _lots.Count!=0 || _transactions.Count!=0 || _production.Count!=0 || _balances.Count!=0 || _receipts.Count!=0)return false;
            var containers=new Dictionary<CargoOwner,CargoContainer>();var lots=new Dictionary<PersistentId,Lot>();
            var transactions=new Dictionary<PersistentId,Transaction>();var receipts=new Dictionary<(PersistentId,ulong),ResourceTransferResult>();
            var balances=new Dictionary<string,long>(StringComparer.Ordinal);var production=new Dictionary<(PersistentId,ulong),ResourceProductionReceipt>();
            var registered=new List<(PersistentId id,PersistentObjectKind kind,object value)>();
            bool committed=false;
            try
            {
                if(saved?.Containers==null || saved.Lots==null || saved.Transactions==null || saved.Receipts==null || saved.Balances==null || saved.Production==null || saved.Revision<0)
                    throw new ArgumentException(reason);
                bool Id(ulong id)=>id!=0 && id<=saved.AllocatedThrough;
                var owned=new HashSet<ulong>();var payloads=new HashSet<ulong>();
                foreach(var row in saved.Containers)
                {
                    if(row==null || !Id(row.Owner) || row.Revision<0 || row.Used<0 || row.ReservedCapacity<0 || row.LotOrder==null)throw new ArgumentException("Invalid container record.");
                    var owner=new CargoOwner(row.OwnerKind,new PersistentId(row.Owner));
                    var container=new CargoContainer(owner,row.Kind,row.Capacity,row.Generation) {IsAvailable=row.Available,Revision=row.Revision};
                    if(!containers.TryAdd(owner,container))throw new ArgumentException("Duplicate container owner.");
                }
                foreach(var row in saved.Lots)
                {
                    if(row==null || !Id(row.Id) || !owned.Add(row.Id) || row.Version==0 || row.Units<0 || row.Reserved<0 || row.Reserved>row.Units ||
                        !_catalog.TryGet(row.Item,out var definition))throw new ArgumentException("Invalid cargo lot.");
                    var owner=new CargoOwner(row.OwnerKind,new PersistentId(row.Owner));
                    if(!containers.TryGetValue(owner,out var container))throw new ArgumentException("Missing lot owner.");
                    bool library=definition.Class==CargoItemClass.Component || definition.Class==CargoItemClass.MachineCarrier;
                    if(library!= (row.Payload!=0) || row.Payload!=0 && !Id(row.Payload) || row.Payload!=0 && row.Units>1 ||
                        container.Route(definition)!=WarehouseDestination.LocalInventory)throw new ArgumentException("Invalid lot item route or payload.");
                    var lot=new Lot {Id=new PersistentId(row.Id),Payload=new PersistentId(row.Payload),Item=row.Item,Units=row.Units,Reserved=row.Reserved,Owner=owner,Version=row.Version};
                    lots.Add(lot.Id,lot);
                    if(row.Units>0)
                    {
                        container.Add(row.Item,row.Units);
                        if(library && (!payloads.Add(row.Payload) || _libraries==null || !_libraries.ValidateEntering(definition,lot.Payload,out var payloadReason)))
                            throw new ArgumentException("Invalid original cargo custody.");
                    }
                }
                foreach(var row in saved.Containers)
                {
                    var container=containers[new CargoOwner(row.OwnerKind,new PersistentId(row.Owner))];var ordered=new HashSet<ulong>();
                    foreach(ulong id in row.LotOrder)
                    {
                        if(!ordered.Add(id) || !lots.TryGetValue(new PersistentId(id),out var lot) || lot.Units==0 || !lot.Owner.Equals(container.Owner))
                            throw new ArgumentException("Invalid cargo lot order.");
                        container.LotIds.Add(lot.Id);
                    }
                    foreach(var lot in lots.Values)if(lot.Units>0 && lot.Owner.Equals(container.Owner) && !ordered.Contains(lot.Id.Value))throw new ArgumentException("Missing cargo lot order.");
                    if(container.Used!=row.Used || container.Used>container.Capacity)throw new ArgumentException("Cargo total disagrees with owner projection.");
                    container.Revision=row.Revision; // Rebuilding derived counts must not increment the saved revision.
                }
                var reservedLots=new Dictionary<PersistentId,long>();var expectedBalances=new Dictionary<string,long>(StringComparer.Ordinal);
                foreach(var row in saved.Transactions)
                {
                    if(row==null || !Id(row.Id) || !owned.Add(row.Id) || !Id(row.Task) || row.Id==row.Task || row.Requested<row.Limit || row.Limit<=0 ||
                        row.Total<0 || row.Remaining<0 || (long)row.Total+row.Remaining>row.Limit || !Enum.IsDefined(typeof(ResourceTransferState),row.State) || row.State==ResourceTransferState.Waiting ||
                        !lots.TryGetValue(new PersistentId(row.Lot),out var lot))throw new ArgumentException("Invalid resource transaction.");
                    var source=new CargoOwner(row.SourceKind,new PersistentId(row.Source));var destination=new CargoOwner(row.DestinationKind,new PersistentId(row.Destination));
                    if(!containers.TryGetValue(source,out _) || !containers.TryGetValue(destination,out var target) || source.Equals(destination) ||
                        target.Route(GetDefinition(lot.Item))!=row.Route || row.Route==WarehouseDestination.Rejected)throw new ArgumentException("Invalid saved transfer route.");
                    var descriptor=new CargoLotSnapshot(lot.Id,lot.Item,row.Limit,source,row.SourceVersion);
                    var token=new ResourceReservation(new PersistentId(row.Id),new PersistentId(row.Task),descriptor,destination,row.DestinationGeneration,row.Limit);
                    bool terminal=row.State==ResourceTransferState.Completed || row.State==ResourceTransferState.Cancelled || row.State==ResourceTransferState.Rejected;
                    if(terminal && row.Remaining!=0 || row.State==ResourceTransferState.Completed && row.Total!=row.Limit ||
                        row.State==ResourceTransferState.Reserved && (row.Total!=0 || row.Remaining!=row.Limit) ||
                        row.State==ResourceTransferState.Partial && (row.Total==0 || row.Remaining==0))throw new ArgumentException("Incoherent saved transfer state.");
                    if(row.Remaining>0 && (!lot.Owner.Equals(source) || lot.Version!=row.SourceVersion || target.Generation!=row.DestinationGeneration || !target.IsAvailable || !containers[source].IsAvailable))
                        throw new ArgumentException("Remaining reservation lost its original owner or destination generation.");
                    reservedLots.TryGetValue(lot.Id,out long reserved);reservedLots[lot.Id]=checked(reserved+row.Remaining);
                    if(row.Route==WarehouseDestination.LocalInventory)target.ReservedCapacity=checked(target.ReservedCapacity+row.Remaining);
                    if(row.Route==WarehouseDestination.GlobalBalance) {expectedBalances.TryGetValue(lot.Item,out long total);expectedBalances[lot.Item]=checked(total+row.Total);}
                    transactions.Add(token.TransactionId,new Transaction {Token=token,Route=row.Route,Requested=row.Requested,Total=row.Total,Remaining=row.Remaining,
                        State=row.State,Reason=row.Reason,LastSequence=row.LastSequence,TerminalReported=row.TerminalReported});
                }
                foreach(var lot in lots.Values)
                {reservedLots.TryGetValue(lot.Id,out long reserved);if(reserved!=lot.Reserved)throw new ArgumentException("Source reservation is not conserved.");}
                foreach(var row in saved.Containers)
                {
                    var container=containers[new CargoOwner(row.OwnerKind,new PersistentId(row.Owner))];
                    if(container.ReservedCapacity!=row.ReservedCapacity || container.ReservedCapacity>container.Capacity-container.Used)
                        throw new ArgumentException("Destination reservation is not conserved.");
                }
                var receiptTotals=new Dictionary<PersistentId,int>();var receiptSequences=new Dictionary<PersistentId,ulong>();
                foreach(var row in saved.Receipts)
                {
                    if(row==null || !transactions.TryGetValue(new PersistentId(row.Transaction),out var tx) || row.Sequence>tx.LastSequence ||
                        row.ReceivedLot!=0 && (!lots.TryGetValue(new PersistentId(row.ReceivedLot),out var received) || received.Item!=lots[tx.Token.LotId].Item))
                        throw new ArgumentException("Receipt lost its original transaction or cargo.");
                    var result=new ResourceTransferResult(tx.Token,row.Sequence,row.Actual,row.Total,row.Remaining,row.State,row.Reason,new PersistentId(row.ReceivedLot),tx.Route);
                    if(row.State==ResourceTransferState.Waiting || row.Actual>0 && (tx.Route==WarehouseDestination.LocalInventory)!=(row.ReceivedLot!=0) ||
                        !receipts.TryAdd((tx.Token.TransactionId,row.Sequence),result))throw new ArgumentException("Invalid or duplicate committed receipt.");
                    receiptTotals.TryGetValue(tx.Token.TransactionId,out int total);receiptTotals[tx.Token.TransactionId]=checked(total+row.Actual);
                    receiptSequences.TryGetValue(tx.Token.TransactionId,out ulong sequence);receiptSequences[tx.Token.TransactionId]=Math.Max(sequence,row.Sequence);
                }
                foreach(var tx in transactions.Values)
                {
                    receiptTotals.TryGetValue(tx.Token.TransactionId,out int total);receiptSequences.TryGetValue(tx.Token.TransactionId,out ulong sequence);
                    if(total!=tx.Total || sequence!=tx.LastSequence)throw new ArgumentException("Transfer totals disagree with original receipts.");
                    var ordered=new List<ResourceTransferResult>();
                    foreach(var receipt in receipts.Values)if(receipt.TransactionId==tx.Token.TransactionId)ordered.Add(receipt);
                    ordered.Sort((a,b)=>a.CompletionSequence.CompareTo(b.CompletionSequence));
                    int running=0;bool reported=false;
                    foreach(var receipt in ordered)
                    {
                        running=checked(running+receipt.ActualUnits);
                        if(receipt.TotalCommittedUnits!=running || receipt.TotalCommittedUnits>tx.Total)throw new ArgumentException("Receipt history is not conserved.");
                        if(receipt.State==ResourceTransferState.Completed || receipt.State==ResourceTransferState.Cancelled || receipt.State==ResourceTransferState.Rejected)reported=true;
                    }
                    if(tx.TerminalReported!=reported)throw new ArgumentException("Terminal settlement acknowledgement was lost.");
                }
                foreach(var row in saved.Balances)
                {
                    if(row==null || row.Units<0 || !_catalog.TryGet(row.Item,out _) || !balances.TryAdd(row.Item,row.Units) ||
                        !expectedBalances.TryGetValue(row.Item,out long expected) || expected!=row.Units)throw new ArgumentException("Balance settlement is not conserved.");
                }
                foreach(var pair in expectedBalances)if(pair.Value>0 && !balances.ContainsKey(pair.Key))throw new ArgumentException("Missing settled balance.");
                foreach(var row in saved.Production)
                {
                    if(row==null || !Id(row.Producer) || row.Sequence==0 || row.OwnerKind!=CargoOwnerKind.WorldFree || row.Units<0 || !_catalog.TryGet(row.Item,out _) ||
                        !containers.ContainsKey(new CargoOwner(row.OwnerKind,new PersistentId(row.Owner))) || row.Units==0 && row.Lot!=0 ||
                        row.Units>0 && (!lots.TryGetValue(new PersistentId(row.Lot),out var lot) || lot.Item!=row.Item))throw new ArgumentException("Invalid production receipt.");
                    var result=new ResourceProductionReceipt(new PersistentId(row.Producer),row.Sequence,new CargoOwner(row.OwnerKind,new PersistentId(row.Owner)),row.Item,row.Units,new PersistentId(row.Lot));
                    if(!production.TryAdd((result.ProducerId,result.Sequence),result))throw new ArgumentException("Duplicate production responsibility.");
                }
                foreach(var container in containers.Values)if(owned.Contains(container.Owner.Id.Value))throw new ArgumentException("Cargo identity also owns a container.");
                foreach(var tx in transactions.Values)if(owned.Contains(tx.Token.TaskId.Value))throw new ArgumentException("Cargo identity also owns a task.");
                foreach(var lot in lots.Values)if(lot.Payload.IsValid && owned.Contains(lot.Payload.Value))throw new ArgumentException("Cargo identity also owns its payload.");
                if(_registry!=null)
                {
                    foreach(var lot in lots.Values)
                    {if(!Register(lot.Id,PersistentObjectKind.CargoLot,lot))throw new InvalidOperationException("Recovered lot identity is occupied.");registered.Add((lot.Id,PersistentObjectKind.CargoLot,lot));}
                    foreach(var tx in transactions.Values)
                    {if(!Register(tx.Token.TransactionId,PersistentObjectKind.ResourceTransaction,tx))throw new InvalidOperationException("Recovered transaction identity is occupied.");registered.Add((tx.Token.TransactionId,PersistentObjectKind.ResourceTransaction,tx));}
                }
                foreach(var pair in containers)_containers.Add(pair.Key,pair.Value);
                foreach(var pair in lots)_lots.Add(pair.Key,pair.Value);
                foreach(var pair in transactions)_transactions.Add(pair.Key,pair.Value);
                foreach(var pair in receipts)_receipts.Add(pair.Key,pair.Value);
                foreach(var pair in balances)_balances.Add(pair.Key,pair.Value);
                foreach(var pair in production)_production.Add(pair.Key,pair.Value);
                if(saved.AllocatedThrough!=0)_ids.TryRestore(new PersistentId(saved.AllocatedThrough));
                foreach(var lot in lots.Values)if(lot.Units>0 && lot.Payload.IsValid)_libraries.Enter(lot.Snapshot,lot.Payload);
                Revision=saved.Revision;committed=true;reason=null;return true;
            }
            catch(Exception error) when(error is ArgumentException || error is InvalidOperationException || error is OverflowException)
            {reason=error.Message;return false;}
            finally
            {
                if(!committed && _registry!=null)foreach(var entry in registered)_registry.TryUnregister(entry.id,entry.kind,entry.value);
            }
        }
    }
}
