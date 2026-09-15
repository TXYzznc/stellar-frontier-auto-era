using System;

namespace AutoEra.Machines
{
    public enum HardwareKind { Sensor, Core, Effector }
    public enum DefinitionAvailability { Ready, PendingConfiguration, PendingResource }

    /// <summary>Validated immutable configuration, separate from machine state.</summary>
    public sealed class MachineDefinition
    {
        public int Id { get; }
        public string Name { get; }
        public int Level { get; }
        public int SensorSlots { get; }
        public int CoreSlots { get; }
        public int EffectorSlots { get; }
        public int BaseCapacity { get; }
        public bool CanMove { get; }
        public bool CanRotate { get; }
        public double MaximumIntegrity { get; }

        public MachineDefinition(int id, string name, int level, int sensors, int cores, int effectors,
            int capacity, bool canMove, bool canRotate, double maximumIntegrity)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(name) || level < 1 || level > 2 ||
                sensors < 0 || cores < 0 || effectors < 0 || capacity < 0 ||
                maximumIntegrity <= 0 || double.IsInfinity(maximumIntegrity) || double.IsNaN(maximumIntegrity))
                throw new ArgumentException("Invalid machine definition.");
            Id = id; Name = name; Level = level; SensorSlots = sensors; CoreSlots = cores;
            EffectorSlots = effectors; BaseCapacity = capacity; CanMove = canMove;
            CanRotate = canRotate; MaximumIntegrity = maximumIntegrity;
        }

        public int SlotCount(HardwareKind kind)
        {
            switch (kind)
            {
                case HardwareKind.Sensor: return SensorSlots;
                case HardwareKind.Core: return CoreSlots;
                case HardwareKind.Effector: return EffectorSlots;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }

    public sealed class ComponentDefinition
    {
        public int Id { get; }
        public HardwareKind Kind { get; }
        public int Level { get; }
        public int AddedCapacity { get; }
        public int ComputeCapacity { get; }
        public int LogicCapacity { get; }
        public bool HasBehavior { get; }

        public ComponentDefinition(int id, HardwareKind kind, int level, int addedCapacity,
            int computeCapacity, int logicCapacity, bool hasBehavior)
        {
            if (id <= 0 || !Enum.IsDefined(typeof(HardwareKind), kind) || level < 1 || level > 2 ||
                addedCapacity < 0 || computeCapacity < 0 || logicCapacity < 0 ||
                (kind != HardwareKind.Core && (computeCapacity != 0 || logicCapacity != 0)) ||
                (kind != HardwareKind.Effector && (addedCapacity != 0 || hasBehavior)) ||
                (addedCapacity > 0 && hasBehavior))
                throw new ArgumentException("Invalid component definition.");
            Id = id; Kind = kind; Level = level; AddedCapacity = addedCapacity;
            ComputeCapacity = computeCapacity; LogicCapacity = logicCapacity; HasBehavior = hasBehavior;
        }
    }
}
