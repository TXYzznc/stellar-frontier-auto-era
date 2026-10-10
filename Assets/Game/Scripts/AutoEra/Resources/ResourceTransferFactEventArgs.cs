using AutoEra.Events;

namespace AutoEra.Logistics
{
    public sealed class ResourceTransferFactEventArgs : AutoEraFactEventArgs
    {
        public static readonly int EventId = typeof(ResourceTransferFactEventArgs).GetHashCode();
        public override int Id => EventId;
        public ResourceTransferResult Result { get; private set; }
        internal ResourceTransferFactEventArgs Initialize(CorrelationId correlation, ResourceTransferResult result)
        {
            bool terminal = result.State == ResourceTransferState.Completed || result.State == ResourceTransferState.Cancelled || result.State == ResourceTransferState.Rejected;
            var outcome = result.State == ResourceTransferState.Completed ? EventOutcome.Succeeded :
                result.State == ResourceTransferState.Cancelled ? EventOutcome.Cancelled : EventOutcome.Failed;
            Initialize(EventDomain.Resource, correlation, result.Source.Id, "TransferSettled", terminal, outcome); Result = result; return this;
        }
        public override void Clear() { base.Clear(); Result = default; }
    }
}
