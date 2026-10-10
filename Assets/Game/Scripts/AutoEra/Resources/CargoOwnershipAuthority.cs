using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoEra.Buildings;
using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    public sealed class ResourceTransferCommitted
    {
        public ResourceTransferResult Result { get; }
        internal ResourceTransferCommitted(ResourceTransferResult result) { Result = result; }
    }
    public sealed class ResourceReservationCreated
    {
        public ResourceReservation Reservation { get; }
        internal ResourceReservationCreated(ResourceReservation reservation) { Reservation = reservation; }
    }
    internal sealed class CargoProjectionChange
    {
        public CargoContainer Source { get; }
        public CargoContainer Destination { get; }
        public CargoProjectionChange(CargoContainer source, CargoContainer destination) { Source = source; Destination = destination; }
    }

    /// <summary>World-local b07 ownership authority. Main-thread transactions publish only after a complete commit.</summary>
    public sealed partial class CargoOwnershipAuthority : IResourceTransferAuthority, IDisposable
    {
        private sealed class Lot
        {
            public PersistentId Id, Payload;
            public string Item;
            public int Units, Reserved;
            public CargoOwner Owner;
            public ulong Version;
            public CargoLotSnapshot Snapshot => new CargoLotSnapshot(Id, Item, Units, Owner, Version);
        }
        private sealed class Transaction
        {
            public ResourceReservation Token;
            public WarehouseDestination Route;
            public int Requested, Total, Remaining;
            public ulong LastSequence;
            public ResourceTransferState State;
            public string Reason;
            public bool TerminalReported;
        }
        private readonly PersistentIdAllocator _ids;
        private readonly PersistentObjectRegistry _registry;
        private ResourceItemCatalog _catalog;
        private readonly Dictionary<CargoOwner, CargoContainer> _containers = new Dictionary<CargoOwner, CargoContainer>();
        private readonly Dictionary<PersistentId, Lot> _lots = new Dictionary<PersistentId, Lot>();
        private readonly Dictionary<PersistentId, Transaction> _transactions = new Dictionary<PersistentId, Transaction>();
        private readonly Dictionary<(PersistentId, ulong), ResourceTransferResult> _receipts = new Dictionary<(PersistentId, ulong), ResourceTransferResult>();
        private readonly Dictionary<string, long> _balances = new Dictionary<string, long>(StringComparer.Ordinal);
        private readonly Dictionary<(PersistentId, ulong), ResourceProductionReceipt> _production = new Dictionary<(PersistentId, ulong), ResourceProductionReceipt>();
        private readonly Action<Exception> _observerError;
        private ICargoLibrarySettlement _libraries;
        private bool _busy, _disposed;
        public event Action<CargoContainer> ContainerChanged;
        internal event Action<CargoProjectionChange> Projecting;
        internal event Action<ResourceTransferCommitted> Transferring;
        public event Action<ResourceTransferCommitted> Committed;
        public event Action<ResourceTransferCommitted> Settled;
        public event Action<ResourceReservationCreated> Reserved;
        public event Action<ResourceProductionReceipt> Produced;
        public IReadOnlyDictionary<string, long> Balances { get; }
        public long Revision { get; private set; }
        public bool IsAtCommitBoundary => !_busy && !_disposed;

        internal bool TryGetPersistentReservation(PersistentId transactionId, out ResourceReservation token, out int remaining, out ulong sequence)
        {
            token=default; remaining=0; sequence=0;
            if (!IsAtCommitBoundary || !_transactions.TryGetValue(transactionId,out var transaction)) return false;
            token=transaction.Token; remaining=transaction.Remaining; sequence=transaction.LastSequence; return true;
        }
        internal bool TryGetPersistentProducedUnits(PersistentId producer,out int units)
        {
            units=0;
            if(!IsAtCommitBoundary || !producer.IsValid) return false;
            long total=0;
            foreach(var receipt in _production.Values) if(receipt.ProducerId==producer) total+=receipt.Units;
            if(total>int.MaxValue) return false; units=(int)total; return true;
        }

        public CargoOwnershipAuthority(PersistentIdAllocator ids, ResourceItemCatalog catalog,
            PersistentObjectRegistry registry = null, ICargoLibrarySettlement libraries = null, Action<Exception> observerError = null)
        {
            _ids = ids ?? throw new ArgumentNullException(nameof(ids));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _registry = registry; _libraries = libraries; _observerError = observerError;
            Balances = new ReadOnlyDictionary<string, long>(_balances);
        }
        public void Configure(ResourceItemCatalog catalog, ICargoLibrarySettlement libraries = null)
        {
            RequireBoundary();
            if (_lots.Count != 0 || _transactions.Count != 0) throw new InvalidOperationException("Resource catalog cannot change after cargo creation.");
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog)); _libraries = libraries;
        }
        public CargoContainer RegisterContainer(CargoOwner owner, CargoContainerKind kind, long capacity, ulong generation = 1)
        {
            RequireBoundary();
            if (_containers.ContainsKey(owner)) throw new ArgumentException("Cargo owner already has a container.");
            if (_lots.ContainsKey(owner.Id) || _transactions.ContainsKey(owner.Id)) throw new ArgumentException("Container identity belongs to another persistent object.");
            var container = new CargoContainer(owner, kind, capacity, generation);
            if (!_ids.TryRestore(owner.Id)) throw new InvalidOperationException("Invalid owner identity.");
            _containers.Add(owner, container); Revision++; return container;
        }
        public bool TryReadContainer(CargoOwner owner, out CargoContainer container) => _containers.TryGetValue(owner, out container);
        public bool TryReadLot(PersistentId id, out CargoLotSnapshot snapshot)
        {
            snapshot = default;
            if (_disposed || !_lots.TryGetValue(id, out var lot) || lot.Units == 0) return false;
            snapshot = lot.Snapshot; return true;
        }
        public bool TryReadPayload(PersistentId id, out PersistentId payload)
        {
            payload = default;
            if (_disposed || !_lots.TryGetValue(id, out var lot) || lot.Units == 0) return false;
            payload = lot.Payload; return true;
        }
        public bool TryReadProduction(PersistentId producer, ulong sequence, out ResourceProductionReceipt receipt)
            => _production.TryGetValue((producer, sequence), out receipt);

        /// <summary>Production and resource debit share one boundary and retry receipt. World-free piles merge without changing ownership.</summary>
        internal bool TryProduce(PersistentId producer, ulong sequence, CargoOwner owner, string item, int units,
            IResourceProductionContribution contribution, out ResourceProductionReceipt receipt, out string reason)
        {
            receipt = default; reason = null;
            if (_production.TryGetValue((producer, sequence), out var prior))
            {
                if (!prior.Owner.Equals(owner) || prior.Item != item || prior.Units != units) { reason = "ProductionIdentityConflict"; return false; }
                receipt = prior; return true;
            }
            if (!CanWrite(out reason)) return false;
            if (!producer.IsValid || sequence == 0 || units < 0 || contribution == null ||
                owner.Kind != CargoOwnerKind.WorldFree || !_catalog.TryGet(item, out var definition) ||
                (definition.Class != CargoItemClass.LocalPhysicalItem && definition.Class != CargoItemClass.CommonResource))
            { reason = "InvalidProduction"; return false; }
            if (!_containers.TryGetValue(owner, out var container) || !container.IsAvailable || units > container.Remaining)
            { reason = "ProductionDestinationUnavailable"; return false; }
            if (!contribution.Validate(out reason)) return false;
            Lot lot = null;
            if (units > 0)
            {
                for (int i = 0; i < container.LotIds.Count; i++)
                    if (_lots.TryGetValue(container.LotIds[i], out var candidate) && candidate.Item == item &&
                        !candidate.Payload.IsValid && candidate.Units <= int.MaxValue - units) { lot = candidate; break; }
                if (lot == null)
                {
                    if (!_ids.TryAllocate(out var id)) { reason = "IdentityExhausted"; return false; }
                    lot = new Lot { Id = id, Item = item, Owner = owner, Version = 1 };
                    if (!Register(id, PersistentObjectKind.CargoLot, lot)) { reason = "DuplicateIdentity"; return false; }
                }
            }
            _busy = true;
            try
            {
                // Neither user callbacks nor presentation run inside this prevalidated state mutation.
                contribution.Commit();
                if (lot != null)
                {
                    if (!_lots.ContainsKey(lot.Id)) { _lots.Add(lot.Id, lot); container.LotIds.Add(lot.Id); }
                    lot.Units += units; container.Add(item, units);
                }
                receipt = new ResourceProductionReceipt(producer, sequence, owner, item, units, lot?.Id ?? default);
                _production.Add((producer, sequence), receipt); Revision++;
                List<Exception> errors = null;
                Invoke(Projecting, new CargoProjectionChange(container, null), ref errors);
                Invoke(ContainerChanged, container, ref errors);
                Invoke(Produced, receipt, ref errors);
                FinishErrors(errors);
            }
            finally { _busy = false; }
            return true;
        }
        public bool TryFindAvailableLot(CargoOwner owner, string item, out CargoLotSnapshot snapshot)
        {
            snapshot = default;
            if (_disposed || !_containers.TryGetValue(owner, out var container)) return false;
            for (int i = 0; i < container.LotIds.Count; i++)
                if (_lots.TryGetValue(container.LotIds[i], out var lot) && lot.Units > lot.Reserved &&
                    (item == null || StringComparer.Ordinal.Equals(item, lot.Item)))
                { snapshot = lot.Snapshot; return true; }
            return false;
        }
        /// <summary>Domain production/initialization entry. Presentation and UI never mint cargo.</summary>
        public bool TryMint(CargoOwner owner, string item, int units, out CargoLotSnapshot snapshot, out string reason, PersistentId payload = default)
        {
            snapshot = default; reason = null;
            if (!CanWrite(out reason)) return false;
            if (units <= 0 || !_catalog.TryGet(item, out var definition)) { reason = "UnknownItemOrQuantity"; return false; }
            if (!_containers.TryGetValue(owner, out var container) || !container.IsAvailable) { reason = "ContainerUnavailable"; return false; }
            if (container.Route(definition) != WarehouseDestination.LocalInventory || units > container.Remaining) { reason = "DestinationRejectedOrFull"; return false; }
            bool needsPayload = definition.Class == CargoItemClass.Component || definition.Class == CargoItemClass.MachineCarrier;
            if (needsPayload != payload.IsValid || (needsPayload && units != 1)) { reason = "InvalidLibraryPayload"; return false; }
            if (needsPayload && (_libraries == null || !_libraries.ValidateEntering(definition, payload, out reason)))
            { reason = reason ?? "LibraryUnavailable"; return false; }
            if (payload.IsValid)
                foreach (var existing in _lots.Values)
                    if (existing.Units > 0 && existing.Payload == payload) { reason = "PayloadAlreadyInCargo"; return false; }
            if (!_ids.TryAllocate(out var id)) { reason = "IdentityExhausted"; return false; }
            var lot = new Lot { Id = id, Item = item, Units = units, Owner = owner, Version = 1, Payload = payload };
            if (!Register(id, PersistentObjectKind.CargoLot, lot)) { reason = "DuplicateIdentity"; return false; }
            _busy = true;
            try
            {
                _lots.Add(id, lot); container.LotIds.Add(id); container.Add(item, units); Revision++;
                if (needsPayload) _libraries.Enter(lot.Snapshot, payload);
                snapshot = lot.Snapshot; Publish(container, null, null);
            }
            finally { _busy = false; }
            return true;
        }
        public bool TryReserve(PersistentId transactionId, PersistentId taskId, PersistentId sourceLotId, ulong expectedSourceVersion,
            CargoOwner destination, ulong expectedDestinationGeneration, int requestedUnits, out ResourceReservation reservation, out string reason)
        {
            reservation = default; reason = null;
            if (!CanWrite(out reason)) return false;
            if (!transactionId.IsValid || !taskId.IsValid || requestedUnits <= 0) { reason = "InvalidReservation"; return false; }
            if (transactionId == taskId || _lots.ContainsKey(taskId) || _transactions.ContainsKey(taskId)) { reason = "InvalidTaskIdentity"; return false; }
            if (_registry != null && _registry.TryGetKind(taskId, out var taskKind) && taskKind != PersistentObjectKind.Task)
            { reason = "InvalidTaskIdentity"; return false; }
            foreach (var containerOwner in _containers.Keys)
                if (transactionId == containerOwner.Id || taskId == containerOwner.Id) { reason = "DuplicateIdentity"; return false; }
            if (_transactions.TryGetValue(transactionId, out var repeated))
            {
                var token = repeated.Token;
                if (token.TaskId != taskId || token.LotId != sourceLotId || token.SourceVersion != expectedSourceVersion ||
                    !token.Destination.Equals(destination) || token.DestinationGeneration != expectedDestinationGeneration || repeated.Requested != requestedUnits)
                { reason = "TransactionIdentityConflict"; return false; }
                reservation = token; return true;
            }
            if (!_lots.TryGetValue(sourceLotId, out var lot) || lot.Units == 0 || lot.Version != expectedSourceVersion)
            { reason = "SourceMissingOrStale"; return false; }
            if (!_containers.TryGetValue(lot.Owner, out var source) || !source.IsAvailable ||
                !_containers.TryGetValue(destination, out var target) || !target.IsAvailable || target.Generation != expectedDestinationGeneration)
            { reason = "ContainerMissingOrStale"; return false; }
            if (lot.Owner.Equals(destination)) { reason = "SameOwner"; return false; }
            var route = target.Route(GetDefinition(lot.Item));
            if (route == WarehouseDestination.Rejected) { reason = "DestinationRejected"; return false; }
            if ((route == WarehouseDestination.ComponentLibrary || route == WarehouseDestination.MachineLibrary) &&
                (_libraries == null || !_libraries.Validate(lot.Snapshot, lot.Payload, route, out reason)))
            { reason = reason ?? "LibraryUnavailable"; return false; }
            int limit = Math.Min(requestedUnits, lot.Units - lot.Reserved);
            if (route == WarehouseDestination.LocalInventory) limit = (int)Math.Min(limit, target.Remaining);
            if (limit <= 0) { reason = "SourceReservedOrDestinationFull"; return false; }
            reservation = new ResourceReservation(transactionId, taskId, lot.Snapshot, destination, target.Generation, limit);
            var tx = new Transaction { Token = reservation, Route = route, Requested = requestedUnits, Remaining = limit, State = ResourceTransferState.Reserved };
            if (_lots.ContainsKey(transactionId) || !Register(transactionId, PersistentObjectKind.ResourceTransaction, tx))
            { reservation = default; reason = "DuplicateIdentity"; return false; }
            _ids.TryRestore(transactionId); _ids.TryRestore(taskId);
            _busy = true;
            try
            {
                _transactions.Add(transactionId, tx); lot.Reserved += limit;
                if (route == WarehouseDestination.LocalInventory) target.ReservedCapacity += limit;
                source.Revision++; target.Revision++; Revision++; Publish(source, target, null, new ResourceReservationCreated(reservation));
            }
            finally { _busy = false; }
            return true;
        }
        public ResourceTransferResult Commit(ResourceReservation reservation, ulong completionSequence, int requestedUnits)
        {
            if (completionSequence == 0 || requestedUnits <= 0) throw new ArgumentOutOfRangeException(nameof(completionSequence));
            var tx = Authenticate(reservation);
            if (_receipts.TryGetValue((reservation.TransactionId, completionSequence), out var cached)) return cached;
            if (_busy) return Result(tx, completionSequence, 0, ResourceTransferState.Waiting, "CommitInProgress");
            if (completionSequence < tx.LastSequence) return Result(tx, completionSequence, 0, ResourceTransferState.Waiting, "StaleCompletion");
            var source = _containers[reservation.Source]; var target = _containers[reservation.Destination];
            _busy = true;
            try
            {
                if (tx.Remaining == 0)
                {
                    var terminal = Store(tx, Result(tx, completionSequence, 0, tx.State, tx.Reason));
                    Publish(null, null, new ResourceTransferCommitted(terminal)); return terminal;
                }
                var lot = _lots[reservation.LotId];
                if (!source.IsAvailable || !target.IsAvailable || lot.Version != reservation.SourceVersion ||
                    !lot.Owner.Equals(reservation.Source) || target.Generation != reservation.DestinationGeneration)
                    return Reject(tx, lot, source, target, completionSequence, "ContainerOrOwnerChanged");
                int actual = Math.Min(requestedUnits, tx.Remaining);
                if (actual > lot.Units) throw new InvalidOperationException("Reserved source quantity was lost.");
                if (lot.Version == ulong.MaxValue) return Reject(tx, lot, source, target, completionSequence, "OwnerVersionExhausted");
                if (tx.Route == WarehouseDestination.GlobalBalance && Balance(lot.Item) > long.MaxValue - actual)
                    return Reject(tx, lot, source, target, completionSequence, "BalanceOverflow");
                if ((tx.Route == WarehouseDestination.ComponentLibrary || tx.Route == WarehouseDestination.MachineLibrary) &&
                    (_libraries == null || !_libraries.Validate(lot.Snapshot, lot.Payload, tx.Route, out var libraryReason)))
                    return Reject(tx, lot, source, target, completionSequence, "LibraryPayloadChanged");
                bool local = tx.Route == WarehouseDestination.LocalInventory;
                Lot received = null;
                if (local && actual < lot.Units)
                {
                    if (!_ids.TryAllocate(out var childId)) return Reject(tx, lot, source, target, completionSequence, "IdentityExhausted");
                    received = new Lot { Id = childId, Item = lot.Item, Units = actual, Owner = target.Owner, Version = lot.Version + 1, Payload = lot.Payload };
                    if (!Register(childId, PersistentObjectKind.CargoLot, received)) return Reject(tx, lot, source, target, completionSequence, "DuplicateIdentity");
                }
                // Library Commit is a prevalidated, non-throwing custody update, before any observer runs.
                if (tx.Route == WarehouseDestination.ComponentLibrary || tx.Route == WarehouseDestination.MachineLibrary)
                    _libraries.Commit(lot.Snapshot, lot.Payload, tx.Route);
                lot.Reserved -= actual; source.Remove(lot.Item, actual);
                if (local) target.ReservedCapacity -= actual;
                if (local && received == null)
                {
                    source.LotIds.Remove(lot.Id); lot.Owner = target.Owner; lot.Version++;
                    target.LotIds.Add(lot.Id); received = lot;
                }
                else
                {
                    lot.Units -= actual;
                    if (lot.Units == 0) source.LotIds.Remove(lot.Id);
                    if (received != null) { _lots.Add(received.Id, received); target.LotIds.Add(received.Id); }
                }
                if (local) target.Add(lot.Item, actual);
                else if (tx.Route == WarehouseDestination.GlobalBalance) _balances[lot.Item] = Balance(lot.Item) + actual;
                tx.Total += actual; tx.Remaining -= actual;
                tx.State = tx.Remaining == 0 ? ResourceTransferState.Completed : ResourceTransferState.Partial;
                tx.Reason = null; target.Revision++; Revision++;
                var result = Store(tx, Result(tx, completionSequence, actual, tx.State, null, received?.Id ?? default));
                Publish(source, target, new ResourceTransferCommitted(result)); return result;
            }
            finally { _busy = false; }
        }
        public ResourceTransferResult Cancel(ResourceReservation reservation, ulong completionSequence)
        {
            if (completionSequence == 0) throw new ArgumentOutOfRangeException(nameof(completionSequence));
            var tx = Authenticate(reservation);
            if (_receipts.TryGetValue((reservation.TransactionId, completionSequence), out var cached)) return cached;
            if (_busy) return Result(tx, completionSequence, 0, ResourceTransferState.Waiting, "CommitInProgress");
            if (completionSequence < tx.LastSequence) return Result(tx, completionSequence, 0, ResourceTransferState.Waiting, "StaleCompletion");
            _busy = true;
            try
            {
                if (tx.Remaining == 0)
                {
                    var terminal = Store(tx, Result(tx, completionSequence, 0, tx.State, tx.Reason));
                    Publish(null, null, new ResourceTransferCommitted(terminal)); return terminal;
                }
                var source = _containers[reservation.Source]; var target = _containers[reservation.Destination];
                Release(tx); tx.State = ResourceTransferState.Cancelled; tx.Reason = "Cancelled"; Revision++;
                var result = Store(tx, Result(tx, completionSequence, 0, tx.State, tx.Reason));
                Publish(source, target, new ResourceTransferCommitted(result)); return result;
            }
            finally { _busy = false; }
        }
        public bool TryReadResult(PersistentId transactionId, ulong sequence, out ResourceTransferResult result) => _receipts.TryGetValue((transactionId, sequence), out result);
        public long Balance(string item) => _balances.TryGetValue(item, out var value) ? value : 0;
        public bool TryCapture(out ResourceInventorySnapshot snapshot)
        {
            snapshot = null; if (!IsAtCommitBoundary) return false;
            var containers = new CargoContainerSnapshot[_containers.Count]; int index = 0;
            foreach (var container in _containers.Values) containers[index++] = new CargoContainerSnapshot(container);
            Array.Sort(containers, (a, b) => { int id = a.Owner.Id.CompareTo(b.Owner.Id); return id != 0 ? id : a.Owner.Kind.CompareTo(b.Owner.Kind); });
            var live = new List<CargoLotState>();
            foreach (var lot in _lots.Values) if (lot.Units > 0) live.Add(new CargoLotState(lot.Snapshot, lot.Payload, lot.Reserved));
            live.Sort((a, b) => a.Lot.Id.CompareTo(b.Lot.Id));
            var transactions = new ResourceTransactionSnapshot[_transactions.Count]; index = 0;
            foreach (var tx in _transactions.Values) transactions[index++] = new ResourceTransactionSnapshot(tx.Token, tx.Requested, tx.Route,
                tx.Total, tx.Remaining, tx.State, tx.Reason, tx.LastSequence, tx.TerminalReported);
            Array.Sort(transactions, (a, b) => a.Reservation.TransactionId.CompareTo(b.Reservation.TransactionId));
            var receipts = new ResourceTransferResult[_receipts.Count]; _receipts.Values.CopyTo(receipts, 0);
            Array.Sort(receipts, (a, b) => { int id = a.TransactionId.CompareTo(b.TransactionId); return id != 0 ? id : a.CompletionSequence.CompareTo(b.CompletionSequence); });
            var balances = new KeyValuePair<string, long>[_balances.Count]; index = 0;
            foreach (var pair in _balances) balances[index++] = pair;
            Array.Sort(balances, (a, b) => StringComparer.Ordinal.Compare(a.Key, b.Key));
            var production = new ResourceProductionReceipt[_production.Count]; _production.Values.CopyTo(production, 0);
            Array.Sort(production, (a, b) => { int id = a.ProducerId.CompareTo(b.ProducerId); return id != 0 ? id : a.Sequence.CompareTo(b.Sequence); });
            snapshot = new ResourceInventorySnapshot(Revision, containers, live.ToArray(), transactions, receipts, balances, production); return true;
        }
        public bool TryResize(CargoOwner owner, long capacity, bool available = true)
        {
            if (!CanWrite(out _) || !_containers.TryGetValue(owner, out var container) || capacity < container.Used || capacity < 0 || container.Generation == ulong.MaxValue) return false;
            if (container.Capacity == capacity && container.IsAvailable == available) return true;
            _busy = true;
            try
            {
                var affected = new HashSet<CargoContainer> { container };
                foreach (var tx in _transactions.Values)
                    if (tx.Remaining > 0 && (tx.Token.Destination.Equals(owner) || (!available && tx.Token.Source.Equals(owner))))
                    {
                        affected.Add(_containers[tx.Token.Source]); affected.Add(_containers[tx.Token.Destination]);
                        Release(tx); tx.State = ResourceTransferState.Cancelled; tx.Reason = "ContainerGenerationChanged";
                    }
                container.Capacity = capacity; container.IsAvailable = available; container.Generation++; container.Revision++; Revision++;
                List<Exception> errors = null;
                Invoke(Projecting, new CargoProjectionChange(container, null), ref errors);
                foreach (var changed in affected) Invoke(ContainerChanged, changed, ref errors);
                FinishErrors(errors); return true;
            }
            finally { _busy = false; }
        }
        private ResourceTransferResult Reject(Transaction tx, Lot lot, CargoContainer source, CargoContainer target, ulong sequence, string reason)
        {
            Release(tx); tx.State = ResourceTransferState.Rejected; tx.Reason = reason; Revision++;
            var result = Store(tx, Result(tx, sequence, 0, tx.State, reason)); Publish(source, target, new ResourceTransferCommitted(result)); return result;
        }
        private void Release(Transaction tx)
        {
            _lots[tx.Token.LotId].Reserved -= tx.Remaining;
            var source = _containers[tx.Token.Source]; var target = _containers[tx.Token.Destination];
            if (tx.Route == WarehouseDestination.LocalInventory) target.ReservedCapacity -= tx.Remaining;
            tx.Remaining = 0; source.Revision++; target.Revision++;
        }
        private Transaction Authenticate(ResourceReservation token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(CargoOwnershipAuthority));
            if (!_transactions.TryGetValue(token.TransactionId, out var tx) || !Same(tx.Token, token)) throw new ArgumentException("Reservation was not issued by this authority.");
            return tx;
        }
        private static bool Same(ResourceReservation a, ResourceReservation b) => a.TransactionId == b.TransactionId && a.TaskId == b.TaskId && a.LotId == b.LotId &&
            a.Source.Equals(b.Source) && a.Destination.Equals(b.Destination) && a.SourceVersion == b.SourceVersion && a.DestinationGeneration == b.DestinationGeneration && a.CommittedLimit == b.CommittedLimit;
        private ResourceTransferResult Result(Transaction tx, ulong sequence, int actual, ResourceTransferState state, string reason, PersistentId received = default) =>
            new ResourceTransferResult(tx.Token, sequence, actual, tx.Total, tx.Remaining, state, reason, received, tx.Route);
        private ResourceTransferResult Store(Transaction tx, ResourceTransferResult result)
        { _receipts.Add((tx.Token.TransactionId, result.CompletionSequence), result); tx.LastSequence = result.CompletionSequence; return result; }
        private bool Register(PersistentId id, PersistentObjectKind kind, object value) =>
            _registry == null || _registry.TryRegister(id, kind, value) == PersistentRegistryResult.Success;
        private ResourceItemDefinition GetDefinition(string id)
        { if (!_catalog.TryGet(id, out var item)) throw new InvalidOperationException("Cargo item definition was lost."); return item; }
        private bool CanWrite(out string reason)
        { reason = _disposed ? "WorldDisposed" : _busy ? "CommitInProgress" : null; return reason == null; }
        private void RequireBoundary()
        { if (!CanWrite(out var reason)) throw new InvalidOperationException(reason); }
        private void Publish(CargoContainer source, CargoContainer target, ResourceTransferCommitted fact, ResourceReservationCreated reserved = null)
        {
            List<Exception> errors = null;
            if (fact != null && fact.Result.ActualUnits > 0) Invoke(Transferring, fact, ref errors);
            if (source != null || target != null) Invoke(Projecting, new CargoProjectionChange(source, target), ref errors);
            if (source != null) Invoke(ContainerChanged, source, ref errors);
            if (target != null && !ReferenceEquals(source, target)) Invoke(ContainerChanged, target, ref errors);
            if (reserved != null) Invoke(Reserved, reserved, ref errors);
            if (fact != null)
            {
                var tx = _transactions[fact.Result.TransactionId];
                bool terminal = fact.Result.State == ResourceTransferState.Completed || fact.Result.State == ResourceTransferState.Cancelled || fact.Result.State == ResourceTransferState.Rejected;
                bool notify = fact.Result.ActualUnits > 0 || (terminal && !tx.TerminalReported);
                if (terminal) tx.TerminalReported = true;
                if (fact.Result.ActualUnits > 0) Invoke(Committed, fact, ref errors);
                if (notify) Invoke(Settled, fact, ref errors);
            }
            FinishErrors(errors);
        }
        private void Invoke<T>(Action<T> listeners, T value, ref List<Exception> errors)
        {
            if (listeners == null) return;
            foreach (Action<T> listener in listeners.GetInvocationList())
                try { listener(value); }
                catch (Exception error) { if (errors == null) errors = new List<Exception>(); errors.Add(error); }
        }
        private void FinishErrors(List<Exception> errors)
        {
            if (errors == null) return;
            if (_observerError != null) { foreach (var error in errors) _observerError(error); }
            else throw new AggregateException("Transaction committed; observer notification failed.", errors);
        }
        public void Dispose()
        {
            if (_disposed) return; RequireBoundary(); _disposed = true;
            if (_registry != null)
            {
                foreach (var lot in _lots.Values) _registry.TryUnregister(lot.Id, PersistentObjectKind.CargoLot, lot);
                foreach (var tx in _transactions.Values) _registry.TryUnregister(tx.Token.TransactionId, PersistentObjectKind.ResourceTransaction, tx);
            }
            ContainerChanged = null; Projecting = null; Transferring = null; Committed = null; Settled = null; Reserved = null; Produced = null;
            _lots.Clear(); _transactions.Clear(); _receipts.Clear(); _containers.Clear(); _balances.Clear(); _production.Clear();
        }
    }
}
