using AutoEra.World.Identity;

namespace AutoEra.Logistics
{
    /// <summary>Domain state changes are prevalidated and non-throwing; authority publishes afterwards.</summary>
    internal interface IResourceProductionContribution
    {
        bool Validate(out string reason);
        void Commit();
    }

    public readonly struct ResourceProductionReceipt
    {
        public PersistentId ProducerId { get; }
        public ulong Sequence { get; }
        public CargoOwner Owner { get; }
        public string Item { get; }
        public int Units { get; }
        public PersistentId LotId { get; }
        internal ResourceProductionReceipt(PersistentId producer, ulong sequence, CargoOwner owner, string item, int units, PersistentId lot)
        { ProducerId = producer; Sequence = sequence; Owner = owner; Item = item; Units = units; LotId = lot; }
    }
}
