using AutoEra.Events;
using AutoEra.Logistics;
using AutoEra.World.Identity;

namespace AutoEra.ResourcePoints
{
    public sealed class ResourceProductionFactEventArgs : AutoEraFactEventArgs
    {
        public static readonly int EventId = typeof(ResourceProductionFactEventArgs).GetHashCode();
        public override int Id => EventId;
        public PersistentId MachineId { get; private set; }
        public PersistentId ComponentId { get; private set; }
        public PersistentId TaskId { get; private set; }
        public PersistentId BehaviorId { get; private set; }
        public ResourceProductionReceipt Receipt { get; private set; }
        internal ResourceProductionFactEventArgs Initialize(CorrelationId taskCorrelation, PersistentId machine, PersistentId component,
            PersistentId task, PersistentId behavior, ResourceProductionReceipt receipt)
        {
            Initialize(EventDomain.Resource, taskCorrelation, receipt.Owner.Id,
                "ProductionCommitted task=" + task.Value + " behavior=" + behavior.Value + " units=" + receipt.Units, false, EventOutcome.None);
            MachineId = machine; ComponentId = component; TaskId = task; BehaviorId = behavior; Receipt = receipt; return this;
        }
        public override void Clear() { base.Clear(); MachineId = ComponentId = TaskId = BehaviorId = default; Receipt = default; }
    }
}
