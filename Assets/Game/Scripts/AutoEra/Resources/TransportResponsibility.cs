using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    public enum TransportPhase { Loading, Carrying, Unloading, Waiting, Cancelled, Delivered }
    public readonly struct TransportResponsibilitySnapshot
    {
        public PersistentId Machine { get; }
        public PersistentId Task { get; }
        public PersistentId Source { get; }
        public PersistentId Destination { get; }
        public string Item { get; }
        public int Loaded { get; }
        public int Delivered { get; }
        public int RequestedUnits { get; }
        public int ReservedUnits { get; }
        public int Pending => Loaded - Delivered;
        public TransportPhase Phase { get; }
        public string Reason { get; }
        internal TransportResponsibilitySnapshot(TransportResponsibility value)
        { Machine = value.Machine; Task = value.Task; Source = value.Source; Destination = value.Destination; Item = value.Item;
            Loaded = value.Loaded; Delivered = value.Delivered; RequestedUnits = value.RequestedUnits; ReservedUnits = value.ReservedUnits; Phase = value.Phase; Reason = value.Reason; }
    }
    internal sealed class TransportResponsibility
    {
        internal PersistentId Machine, Task, Source, Destination;
        internal string Item, Reason;
        internal int Loaded, Delivered, RequestedUnits, ReservedUnits;
        internal TransportPhase Phase;
        internal bool Active;
    }
    /// <summary>Delivery responsibility only. Cargo quantities and ownership remain in CargoOwnershipAuthority.</summary>
    public sealed partial class TransportResponsibilityLedger
    {
        private readonly Dictionary<(PersistentId, string), TransportResponsibility> _records = new Dictionary<(PersistentId, string), TransportResponsibility>();
        private readonly Dictionary<PersistentId, (PersistentId machine, string item, bool unloading)> _transactions = new Dictionary<PersistentId, (PersistentId, string, bool)>();
        public long Revision { get; private set; }
        public event Action Changed;
        internal bool MatchesActive(PersistentId machine, PersistentId task, PersistentId source, PersistentId destination, string item, int requested)
            => _records.TryGetValue((machine,item),out var value) && value.Active && value.Task==task &&
                value.Source==source && value.Destination==destination && value.RequestedUnits==requested;
        internal bool TryBegin(PersistentId machine, PersistentId task, PersistentId source, PersistentId destination, string item,
            int existingCargo, bool unloading, int requestedUnits, out string reason)
        {
            reason = null; var key = (machine, item);
            if (!machine.IsValid || !task.IsValid || !source.IsValid || !destination.IsValid || source == destination || string.IsNullOrWhiteSpace(item) || existingCargo < 0 || requestedUnits <= 0)
            { reason = "InvalidDeliveryRoute"; return false; }
            _records.TryGetValue(key, out var value);
            if (value != null && value.Active)
            { reason = "ActiveTransferExists"; return false; }
            if (value != null && value.Loaded > value.Delivered)
            {
                if (value.Source != source || value.Destination != destination) { reason = "ExistingDeliveryResponsibility"; return false; }
                if (existingCargo != value.Loaded - value.Delivered) { reason = "CargoResponsibilityMismatch"; return false; }
                if (!unloading) { reason = "DeliverExistingCargoFirst"; return false; }
            }
            else
            { value = new TransportResponsibility { Machine = machine, Source = source, Destination = destination, Item = item, Loaded = existingCargo }; _records[key] = value; }
            value.Task = task; value.Active = true; value.RequestedUnits = requestedUnits; value.ReservedUnits = 0; value.Phase = unloading ? TransportPhase.Unloading : TransportPhase.Loading; value.Reason = null; Publish(); return true;
        }
        internal void Bind(ResourceReservation reservation, PersistentId machine, string item, bool unloading)
        { _transactions.Add(reservation.TransactionId, (machine, item, unloading)); _records[(machine,item)].ReservedUnits = reservation.CommittedLimit; Publish(); }
        // Authority invokes this before publishing either cargo projection or inventory changes.
        internal void StageCommit(ResourceTransferCommitted fact)
        {
            if (!_transactions.TryGetValue(fact.Result.TransactionId, out var binding)) return;
            int units = fact.Result.ActualUnits; var machine = binding.machine; var item = binding.item; bool unloading = binding.unloading;
            if (units <= 0) return;
            var value = _records[(machine,item)];
            if (unloading) { if (units > value.Loaded - value.Delivered) throw new InvalidOperationException("Delivery exceeds responsibility."); value.Delivered += units; }
            else value.Loaded = checked(value.Loaded + units);
            value.ReservedUnits = fact.Result.RemainingReservedUnits;
            value.Phase = value.Loaded == value.Delivered ? TransportPhase.Delivered : value.Active ? (unloading ? TransportPhase.Unloading : TransportPhase.Loading) : TransportPhase.Carrying; value.Reason = null; Revision++;
        }
        internal void NotifyCommit(ResourceTransferCommitted fact)
        { if (_transactions.ContainsKey(fact.Result.TransactionId)) Changed?.Invoke(); }
        internal void Stop(PersistentId machine, string item, bool cancelled, string reason)
        {
            if (!_records.TryGetValue((machine,item), out var value)) return;
            value.Active = false;
            value.ReservedUnits = 0;
            value.Phase = value.Loaded == value.Delivered ? TransportPhase.Delivered : cancelled ? TransportPhase.Cancelled : reason == null ? TransportPhase.Carrying : TransportPhase.Waiting;
            value.Reason = reason; Publish();
        }
        public void ReportNavigationFailure(PersistentId task, bool cancelled, string reason)
        {
            if (!task.IsValid || string.IsNullOrWhiteSpace(reason)) return;
            bool changed = false;
            foreach (var value in _records.Values)
            {
                if (value.Task != task || value.Loaded == value.Delivered) continue;
                value.Active = false; value.ReservedUnits = 0;
                value.Phase = cancelled ? TransportPhase.Cancelled : TransportPhase.Waiting;
                value.Reason = reason; changed = true;
            }
            if (changed) Publish();
        }
        public bool TryRead(PersistentId machine, string item, out TransportResponsibilitySnapshot snapshot)
        { snapshot = default; if (!_records.TryGetValue((machine,item), out var value)) return false; snapshot = new TransportResponsibilitySnapshot(value); return true; }
        public IReadOnlyList<TransportResponsibilitySnapshot> Capture()
        {
            var values = new List<TransportResponsibilitySnapshot>(_records.Count); foreach (var value in _records.Values) values.Add(new TransportResponsibilitySnapshot(value));
            values.Sort((a,b) => { int machine = a.Machine.CompareTo(b.Machine); return machine != 0 ? machine : string.CompareOrdinal(a.Item,b.Item); }); return values.AsReadOnly();
        }
        private void Publish() { Revision++; Changed?.Invoke(); }
        internal void Release() { _records.Clear(); _transactions.Clear(); Changed = null; }
    }
}
