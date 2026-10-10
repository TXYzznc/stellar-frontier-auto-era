using System;
using AutoEra.World.Identity;
using AutoEra.Buildings;

namespace AutoEra.Logistics
{
    // Shared contracts: quantity and ownership come from the single b07 authority.
    public enum CargoOwnerKind { WorldFree, Effector, ConveyorSegment, OutputPort, Receiver }
    public readonly struct CargoOwner : IEquatable<CargoOwner>
    {
        public CargoOwnerKind Kind { get; }
        public PersistentId Id { get; }
        public CargoOwner(CargoOwnerKind kind, PersistentId id)
        {
            if (!Enum.IsDefined(typeof(CargoOwnerKind), kind) || !id.IsValid) throw new ArgumentException("A cargo owner requires a defined role and permanent identity.");
            Kind = kind; Id = id;
        }
        public bool Equals(CargoOwner other) => Kind == other.Kind && Id == other.Id;
        public override bool Equals(object obj) => obj is CargoOwner other && Equals(other);
        public override int GetHashCode() => ((int)Kind * 397) ^ Id.GetHashCode();
        public bool IsValid => Id.IsValid;
    }
    public readonly struct CargoLotSnapshot
    {
        public PersistentId Id { get; }
        public string ItemType { get; }
        public int Units { get; }
        public CargoOwner Owner { get; }
        public ulong Version { get; }
        public CargoLotSnapshot(PersistentId id, string itemType, int units, CargoOwner owner, ulong version)
        {
            if (!id.IsValid || string.IsNullOrWhiteSpace(itemType) || units <= 0 || !owner.IsValid || version == 0) throw new ArgumentException("Invalid cargo lot snapshot.");
            Id = id; ItemType = itemType; Units = units; Owner = owner; Version = version;
        }
    }
    public readonly struct ResourceReservation
    {
        public PersistentId TransactionId { get; }
        public PersistentId TaskId { get; }
        public PersistentId LotId { get; }
        public CargoOwner Source { get; }
        public CargoOwner Destination { get; }
        public ulong SourceVersion { get; }
        public ulong DestinationGeneration { get; }
        public int CommittedLimit { get; }
        public ResourceReservation(PersistentId transaction, PersistentId task, CargoLotSnapshot source, CargoOwner destination, ulong destinationGeneration, int limit)
        {
            if (!transaction.IsValid || !task.IsValid || !source.Id.IsValid || !destination.IsValid || source.Owner.Equals(destination) || destinationGeneration == 0 || limit <= 0 || limit > source.Units)
                throw new ArgumentException("Reservation identities, generations and limit must be valid.");
            TransactionId = transaction; TaskId = task; LotId = source.Id; Source = source.Owner; Destination = destination;
            SourceVersion = source.Version; DestinationGeneration = destinationGeneration; CommittedLimit = limit;
        }
    }
    public enum ResourceTransferState { Reserved, Partial, Completed, Cancelled, Rejected, Waiting }
    public readonly struct ResourceTransferResult
    {
        public PersistentId TransactionId { get; }
        public PersistentId TaskId { get; }
        public ulong CompletionSequence { get; }
        public int ActualUnits { get; }
        public int TotalCommittedUnits { get; }
        public int RemainingReservedUnits { get; }
        public ResourceTransferState State { get; }
        public string Reason { get; }
        public PersistentId ReceivedLotId { get; }
        public CargoOwner Source { get; }
        public CargoOwner Destination { get; }
        public WarehouseDestination Route { get; }
        public ResourceTransferResult(ResourceReservation reservation, ulong sequence, int actual, int total, int remaining, ResourceTransferState state, string reason = null,
            PersistentId receivedLotId = default, WarehouseDestination route = WarehouseDestination.LocalInventory)
        {
            if (!reservation.TransactionId.IsValid || sequence == 0 || actual < 0 || total < actual || remaining < 0 || total > reservation.CommittedLimit || remaining > reservation.CommittedLimit - total || !Enum.IsDefined(typeof(ResourceTransferState), state))
                throw new ArgumentException("Transfer result cannot exceed the reservation or erase committed units.");
            if ((state == ResourceTransferState.Completed || state == ResourceTransferState.Cancelled || state == ResourceTransferState.Rejected) && remaining != 0)
                throw new ArgumentException("A terminal result must release all uncommitted reservations.");
            TransactionId = reservation.TransactionId; TaskId = reservation.TaskId; CompletionSequence = sequence;
            ActualUnits = actual; TotalCommittedUnits = total; RemainingReservedUnits = remaining; State = state; Reason = reason;
            ReceivedLotId = receivedLotId; Source = reservation.Source; Destination = reservation.Destination; Route = route;
        }
    }
    /// <summary>One authority owns quantities, owner versions, capacity reservations and idempotent receipts. UI and physical proxies only read it.</summary>
    public interface IResourceTransferAuthority
    {
        bool TryReadLot(PersistentId lotId, out CargoLotSnapshot lot);
        bool TryReserve(PersistentId transactionId, PersistentId taskId, PersistentId sourceLotId, ulong expectedSourceVersion,
            CargoOwner destination, ulong expectedDestinationGeneration, int requestedUnits, out ResourceReservation reservation, out string reason);
        // A repeated (transaction, completionSequence) must return the exact original result without publishing a second fact.
        ResourceTransferResult Commit(ResourceReservation reservation, ulong completionSequence, int requestedUnits);
        ResourceTransferResult Cancel(ResourceReservation reservation, ulong completionSequence);
        bool TryReadResult(PersistentId transactionId, ulong completionSequence, out ResourceTransferResult result);
        bool IsAtCommitBoundary { get; }
    }
    public static class ResourceTransferBudget
    {
        /// <summary>Pure planning bound, not an inventory or ownership ledger.</summary>
        public static bool TryPlan(int requested, int sourceAvailable, int destinationRemaining, out int promised)
        {
            promised = 0;
            if (requested <= 0 || sourceAvailable < 0 || destinationRemaining < 0) return false;
            promised = Math.Min(requested, Math.Min(sourceAvailable, destinationRemaining)); return promised > 0;
        }
    }
}
