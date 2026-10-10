using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.ResourcePoints;

namespace AutoEra.World.Region
{
    public enum RegionEffectorCheckpointKind { ResourceTransfer, ResourceProduction }

    /// <summary>Closed domain payload; never serializes an executor, service, view or delegate.</summary>
    public sealed class RegionEffectorOperationSnapshot
    {
        public int Version = 1;
        public RegionEffectorCheckpointKind Kind;
        public ResourceTransferOperationSnapshot Transfer;
        public ProductionEffectorOperationSnapshot Production;
    }
    public sealed class ResourceTransferOperationSnapshot
    {
        public ulong Transaction, Sequence;
        public long LastWorldMilliseconds;
        public int Committed;
        public double Progress, Prepared;
        public bool Reserved, Paused, Completed;
        public BehaviorOutcome Outcome;
        public string WaitingReason;
    }
    public sealed class ProductionEffectorOperationSnapshot
    {
        public ulong Tree, Sequence;
        public long LastWorldMilliseconds;
        public int Produced;
        public double Elapsed;
        public bool Fell, Paused, Completed;
        public BehaviorOutcome Outcome;
        public string WaitingReason;
    }
    public interface IRegionPersistentEffectorOperation
    {
        bool TryCapturePersistent(long now, out RegionEffectorOperationSnapshot snapshot);
    }
    public interface IRegionPersistentEffectorExecutor : IRegionEffectorExecutor
    {
        /// <summary>Bind already restored authorities; must not start a new transaction or production responsibility.</summary>
        bool TryRestorePersistent(MachineExecutionContext context, ComponentInstance component,
            BehaviorRequest<EffectorBehaviorParameters> request, RegionEffectorOperationSnapshot snapshot,
            long now, out IRegionEffectorOperation operation, out string reason);
    }
}
