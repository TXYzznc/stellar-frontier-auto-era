using System.Collections.Generic;
using AutoEra.Buildings;
using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    public readonly struct CargoContainerSnapshot
    {
        public CargoOwner Owner { get; }
        public CargoContainerKind Kind { get; }
        public long Capacity { get; }
        public ulong Generation { get; }
        public bool IsAvailable { get; }
        internal CargoContainerSnapshot(CargoContainer value)
        { Owner = value.Owner; Kind = value.Kind; Capacity = value.Capacity; Generation = value.Generation; IsAvailable = value.IsAvailable; }
    }
    public readonly struct CargoLotState
    {
        public CargoLotSnapshot Lot { get; }
        public PersistentId PayloadId { get; }
        public int ReservedUnits { get; }
        internal CargoLotState(CargoLotSnapshot lot, PersistentId payload, int reserved) { Lot = lot; PayloadId = payload; ReservedUnits = reserved; }
    }
    public readonly struct ResourceTransactionSnapshot
    {
        public ResourceReservation Reservation { get; }
        public int RequestedUnits { get; }
        public WarehouseDestination Route { get; }
        public int TotalCommittedUnits { get; }
        public int RemainingReservedUnits { get; }
        public ResourceTransferState State { get; }
        public string Reason { get; }
        public ulong LastCompletionSequence { get; }
        public bool TerminalReported { get; }
        internal ResourceTransactionSnapshot(ResourceReservation reservation, int requested, WarehouseDestination route, int total,
            int remaining, ResourceTransferState state, string reason, ulong sequence, bool terminalReported)
        { Reservation = reservation; RequestedUnits = requested; Route = route; TotalCommittedUnits = total; RemainingReservedUnits = remaining;
            State = state; Reason = reason; LastCompletionSequence = sequence; TerminalReported = terminalReported; }
    }
    /// <summary>Detached immutable inventory state, including unresolved reservations and exact idempotent receipts.</summary>
    public sealed class ResourceInventorySnapshot
    {
        public long Revision { get; }
        public IReadOnlyList<CargoContainerSnapshot> Containers { get; }
        public IReadOnlyList<CargoLotState> Lots { get; }
        public IReadOnlyList<ResourceTransactionSnapshot> Transactions { get; }
        public IReadOnlyList<ResourceTransferResult> Receipts { get; }
        public IReadOnlyList<KeyValuePair<string, long>> Balances { get; }
        public IReadOnlyList<ResourceProductionReceipt> ProductionReceipts { get; }
        internal ResourceInventorySnapshot(long revision, CargoContainerSnapshot[] containers, CargoLotState[] lots,
            ResourceTransactionSnapshot[] transactions, ResourceTransferResult[] receipts, KeyValuePair<string, long>[] balances,
            ResourceProductionReceipt[] production = null)
        {
            Revision = revision; Containers = System.Array.AsReadOnly(containers); Lots = System.Array.AsReadOnly(lots);
            Transactions = System.Array.AsReadOnly(transactions); Receipts = System.Array.AsReadOnly(receipts); Balances = System.Array.AsReadOnly(balances);
            ProductionReceipts = System.Array.AsReadOnly(production ?? System.Array.Empty<ResourceProductionReceipt>());
        }
    }
}
