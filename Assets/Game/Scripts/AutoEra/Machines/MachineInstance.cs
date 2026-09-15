using System;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    public enum ManagementOrigin { Field, Hub, Library }
    public enum MachineRunState { Stopped, Running, Sleeping }
    public enum MachineManagementResult
    {
        Completed, WaitingForSafeStop, InvalidOrigin, NotDeployed, MustStop, NotActivated,
        Destroyed, Disconnected, InvalidSlot, Occupied, AlreadyInstalled, MissingComponent,
        CapacityInUse, CoreHasNoSwitch, InvalidName, DuplicateName, InvalidState, ComputeInUse, LogicCapacityInUse, RepairRequired
    }

    public sealed class ComponentInstance
    {
        public PersistentId Id { get; }
        public ComponentDefinition Definition { get; }
        public PersistentId OwnerId { get; internal set; }
        public bool Enabled { get; internal set; } = true;
        internal ComponentInstance(PersistentId id, ComponentDefinition definition) { Id = id; Definition = definition; }
    }

    public sealed class MachineInstance
    {
        private readonly ComponentInstance[][] _slots;
        public PersistentId Id { get; }
        public MachineDefinition Definition { get; }
        public ulong ModelSerial { get; }
        public string Name { get; internal set; }
        public bool Deployed { get; internal set; }
        // Transient region ownership; neither a second identity nor a saved deployment flag.
        internal object RegionBindingOwner { get; set; }
        public bool Activated { get; private set; }
        public bool PowerSwitchOn { get; private set; } = true;
        public bool SupplyAvailable { get; private set; }
        public bool SignalAvailable { get; private set; }
        public double Integrity { get; private set; }
        public MachineRunState RequestedRunState { get; private set; } = MachineRunState.Stopped;
        public bool HasActiveBehavior { get; private set; }
        public int UsedCapacity { get; private set; }
        public int ReservedCompute { get; private set; }
        public int AppliedLogicCost { get; private set; }
        public int ComputeWaitingCount { get; private set; }
        public long Revision { get; private set; }
        public event Action<MachineInstance> Changed;
        internal void NotifyChanged() { Revision++; Changed?.Invoke(this); }
        public bool Powered => Deployed && PowerSwitchOn && SupplyAvailable && Integrity > 0;
        public bool Connected => Activated && Powered && SignalAvailable;
        public bool CanRun => Activated && Powered && RequestedRunState == MachineRunState.Running;
        public int TotalCapacity => checked(Definition.BaseCapacity + Sum(d => d.AddedCapacity));
        public int ComputeCapacity => Sum(d => d.ComputeCapacity);
        public int LogicCapacity => Sum(d => d.LogicCapacity);

        internal MachineInstance(PersistentId id, MachineDefinition definition, ulong serial, string name)
        {
            Id = id; Definition = definition; ModelSerial = serial; Name = name; Integrity = definition.MaximumIntegrity;
            _slots = new[] { new ComponentInstance[definition.SensorSlots], new ComponentInstance[definition.CoreSlots], new ComponentInstance[definition.EffectorSlots] };
        }

        internal void RestoreConfiguration(bool deployed, bool activated, bool powerSwitch, MachineRunState state, double integrity, int used)
        {
            if (!Enum.IsDefined(typeof(MachineRunState), state) || used < 0 || used > TotalCapacity ||
                (!activated && state != MachineRunState.Stopped) || (!deployed && state != MachineRunState.Stopped) ||
                (integrity == 0 && (activated || powerSwitch || state != MachineRunState.Stopped)))
                throw new ArgumentException("Invalid recovered machine state.");
            UpdateIntegrity(integrity);
            Deployed = deployed; Activated = activated; PowerSwitchOn = powerSwitch;
            RequestedRunState = state; UsedCapacity = used;
            // Environment and live execution are acquired afresh; never deserialize stale availability.
            SupplyAvailable = false; SignalAvailable = false; HasActiveBehavior = false;
        }

        private int Sum(Func<ComponentDefinition, int> selector)
        {
            int value = 0;
            foreach (var group in _slots) foreach (var item in group) if (item != null) value = checked(value + selector(item.Definition));
            return value;
        }

        public ComponentInstance GetComponent(HardwareKind kind, int index)
            => ValidSlot(kind, index) ? _slots[(int)kind][index] : null;
        private bool ValidSlot(HardwareKind kind, int index)
            => (int)kind >= 0 && (int)kind < _slots.Length && index >= 0 && index < _slots[(int)kind].Length;

        public MachineManagementResult Activate(ManagementOrigin origin)
        {
            if (origin != ManagementOrigin.Field) return MachineManagementResult.InvalidOrigin;
            if (!Deployed) return MachineManagementResult.NotDeployed;
            if (Integrity <= 0) return MachineManagementResult.Destroyed;
            Activated = true;
            NotifyChanged();
            return MachineManagementResult.Completed;
        }

        public MachineManagementResult SetRunState(ManagementOrigin origin, MachineRunState state)
        {
            if (!Enum.IsDefined(typeof(MachineRunState), state)) return MachineManagementResult.InvalidState;
            var access = CheckManagementAccess(origin);
            if (access != MachineManagementResult.Completed) return access;
            if (state != MachineRunState.Stopped && !Activated) return MachineManagementResult.NotActivated;
            if (Integrity <= 0) return MachineManagementResult.Destroyed;
            RequestedRunState = state;
            NotifyChanged();
            return state == MachineRunState.Stopped && HasActiveBehavior ? MachineManagementResult.WaitingForSafeStop : MachineManagementResult.Completed;
        }

        public MachineManagementResult SetPowerSwitch(ManagementOrigin origin, bool on)
        {
            if (origin != ManagementOrigin.Field || !Deployed) return MachineManagementResult.InvalidOrigin;
            if (on && Integrity <= 0) return MachineManagementResult.Destroyed;
            PowerSwitchOn = on; NotifyChanged(); return MachineManagementResult.Completed;
        }

        // External adapters supply facts; this is not an energy grid or signal simulator.
        public void UpdateEnvironment(bool supply, bool signal)
        {
            if (SupplyAvailable == supply && SignalAvailable == signal) return;
            SupplyAvailable = supply; SignalAvailable = signal; NotifyChanged();
        }
        public void UpdateBehaviorActivity(bool active)
        { if (HasActiveBehavior == active) return; HasActiveBehavior = active; NotifyChanged(); }
        public void UpdateComputeUsage(int reserved, int logicCost, int waiting = 0)
        {
            if (reserved < 0 || reserved > ComputeCapacity || logicCost < 0 || logicCost > LogicCapacity || waiting < 0)
                throw new ArgumentOutOfRangeException();
            if (ReservedCompute == reserved && AppliedLogicCost == logicCost && ComputeWaitingCount == waiting) return;
            ReservedCompute = reserved; AppliedLogicCost = logicCost; ComputeWaitingCount = waiting; NotifyChanged();
        }
        public void UpdateContainerUsage(int used)
        {
            if (used < 0 || used > TotalCapacity) throw new ArgumentOutOfRangeException(nameof(used));
            UsedCapacity = used; NotifyChanged();
        }
        public void UpdateIntegrity(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > Definition.MaximumIntegrity) throw new ArgumentOutOfRangeException(nameof(value));
            Integrity = value;
            if (value == 0) { Activated = false; PowerSwitchOn = false; RequestedRunState = MachineRunState.Stopped; }
            NotifyChanged();
        }
        public bool IsComponentWorking(HardwareKind kind, int index)
        {
            var item = GetComponent(kind, index);
            return item != null && CanRun && item.Enabled;
        }
        public MachineManagementResult SetComponentEnabled(ManagementOrigin origin, HardwareKind kind, int index, bool enabled)
        {
            var access = CheckManagementAccess(origin);
            if (access != MachineManagementResult.Completed) return access;
            if (kind == HardwareKind.Core) return MachineManagementResult.CoreHasNoSwitch;
            if (!ValidSlot(kind, index)) return MachineManagementResult.InvalidSlot;
            var item = GetComponent(kind, index);
            if (item == null) return MachineManagementResult.MissingComponent;
            item.Enabled = enabled; NotifyChanged(); return MachineManagementResult.Completed;
        }
        private MachineManagementResult CheckManagementAccess(ManagementOrigin origin)
        {
            if (origin == ManagementOrigin.Library) return Deployed ? MachineManagementResult.InvalidOrigin : MachineManagementResult.Completed;
            if (origin == ManagementOrigin.Field) return Deployed ? MachineManagementResult.Completed : MachineManagementResult.NotDeployed;
            if (origin == ManagementOrigin.Hub) return Connected ? MachineManagementResult.Completed : MachineManagementResult.Disconnected;
            return MachineManagementResult.InvalidOrigin;
        }
        private MachineManagementResult HardwareGate(ManagementOrigin origin)
        {
            if ((Deployed && origin != ManagementOrigin.Field) || (!Deployed && origin != ManagementOrigin.Library)) return MachineManagementResult.InvalidOrigin;
            if (Integrity <= 0) return MachineManagementResult.Destroyed;
            if (RequestedRunState != MachineRunState.Stopped) return MachineManagementResult.MustStop;
            return HasActiveBehavior ? MachineManagementResult.WaitingForSafeStop : MachineManagementResult.Completed;
        }
        internal MachineManagementResult Install(ManagementOrigin origin, ComponentInstance component, int index)
        {
            var gate = HardwareGate(origin);
            if (gate != MachineManagementResult.Completed) return gate;
            var kind = component.Definition.Kind;
            if (!ValidSlot(kind, index)) return MachineManagementResult.InvalidSlot;
            if (component.OwnerId.IsValid) return MachineManagementResult.AlreadyInstalled;
            if (_slots[(int)kind][index] != null) return MachineManagementResult.Occupied;
            if ((long)TotalCapacity + component.Definition.AddedCapacity > int.MaxValue ||
                (long)ComputeCapacity + component.Definition.ComputeCapacity > int.MaxValue ||
                (long)LogicCapacity + component.Definition.LogicCapacity > int.MaxValue)
                return MachineManagementResult.InvalidState;
            _slots[(int)kind][index] = component; component.OwnerId = Id;
            NotifyChanged();
            return MachineManagementResult.Completed;
        }
        internal MachineManagementResult Remove(ManagementOrigin origin, HardwareKind kind, int index)
        {
            var gate = HardwareGate(origin);
            if (gate != MachineManagementResult.Completed) return gate;
            if (!ValidSlot(kind, index)) return MachineManagementResult.InvalidSlot;
            var item = _slots[(int)kind][index];
            if (item == null) return MachineManagementResult.MissingComponent;
            if (TotalCapacity - item.Definition.AddedCapacity < UsedCapacity) return MachineManagementResult.CapacityInUse;
            if (ComputeCapacity - item.Definition.ComputeCapacity < ReservedCompute) return MachineManagementResult.ComputeInUse;
            if (item.Definition.Kind == HardwareKind.Core && ComputeWaitingCount > 0) return MachineManagementResult.ComputeInUse;
            if (LogicCapacity - item.Definition.LogicCapacity < AppliedLogicCost) return MachineManagementResult.LogicCapacityInUse;
            _slots[(int)kind][index] = null; item.OwnerId = PersistentId.Invalid;
            NotifyChanged();
            return MachineManagementResult.Completed;
        }
    }
}
