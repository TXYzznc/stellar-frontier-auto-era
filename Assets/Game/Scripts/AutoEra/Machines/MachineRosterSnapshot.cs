using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    // In-memory recovery contract only. No disk format or running-behavior checkpoint is introduced.
    public sealed class MachineRosterSnapshot
    {
        public ulong AllocatedThrough;
        public KeyValuePair<int, ulong>[] HistoricalCounts;
        public MachineRecord[] Machines;
        public ComponentRecord[] Components;

        public sealed class MachineRecord
        {
            public ulong Id, Serial;
            public int ModelId, Level, UsedCapacity;
            public string Name;
            public bool Deployed, Activated, PowerSwitch;
            public MachineRunState RequestedState;
            public double Integrity;
        }
        public sealed class ComponentRecord
        {
            public ulong Id, Owner;
            public int ModelId, Level, Slot;
            public bool Enabled;
        }
    }

    public sealed partial class MachineRoster
    {
        public MachineRosterSnapshot CaptureConfiguration()
        {
            EnsureActive();
            var machines = new List<MachineRosterSnapshot.MachineRecord>();
            var components = new List<MachineRosterSnapshot.ComponentRecord>();
            var slots = new Dictionary<PersistentId, int>();
            foreach (var machine in _machines.Values)
            {
                if (machine.HasActiveBehavior) throw new InvalidOperationException("Capture requires a safe behavior checkpoint.");
                machines.Add(new MachineRosterSnapshot.MachineRecord { Id = machine.Id.Value, Serial = machine.ModelSerial,
                    ModelId = machine.Definition.Id, Level = machine.Definition.Level, Name = machine.Name,
                    Deployed = machine.Deployed, Activated = machine.Activated, PowerSwitch = machine.PowerSwitchOn,
                    RequestedState = machine.RequestedRunState, Integrity = machine.Integrity, UsedCapacity = machine.UsedCapacity });
                int[] counts = { machine.Definition.SensorSlots, machine.Definition.CoreSlots, machine.Definition.EffectorSlots };
                for (int kind = 0; kind < counts.Length; kind++)
                    for (int slot = 0; slot < counts[kind]; slot++)
                    {
                        var component = machine.GetComponent((HardwareKind)kind, slot);
                        if (component != null) slots.Add(component.Id, slot);
                    }
            }
            foreach (var item in _components.Values)
                components.Add(new MachineRosterSnapshot.ComponentRecord { Id = item.Id.Value, Owner = item.OwnerId.Value,
                    ModelId = item.Definition.Id, Level = item.Definition.Level, Enabled = item.Enabled,
                    Slot = slots.TryGetValue(item.Id, out int slot) ? slot : -1 });
            machines.Sort((a, b) => a.Id.CompareTo(b.Id));
            components.Sort((a, b) => a.Id.CompareTo(b.Id));
            var history = new List<KeyValuePair<int, ulong>>(_serials);
            history.Sort((a, b) => a.Key.CompareTo(b.Key));
            return new MachineRosterSnapshot { AllocatedThrough = _ids.IsExhausted ? ulong.MaxValue : _ids.NextId.Value - 1,
                HistoricalCounts = history.ToArray(), Machines = machines.ToArray(), Components = components.ToArray() };
        }

        public void RestoreConfiguration(MachineRosterSnapshot snapshot, Func<int, int, MachineDefinition> machineDefinition,
            Func<int, int, ComponentDefinition> componentDefinition)
        {
            EnsureActive();
            if (_machines.Count != 0 || _components.Count != 0 || _serials.Count != 0)
                throw new InvalidOperationException("Restore requires an empty roster.");
            if (snapshot?.Machines == null || snapshot.Components == null || snapshot.HistoricalCounts == null ||
                machineDefinition == null || componentDefinition == null) throw new ArgumentException("Incomplete snapshot.");
            var machines = new Dictionary<PersistentId, MachineInstance>();
            var components = new Dictionary<PersistentId, ComponentInstance>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            var identities = new HashSet<ulong>();
            var serials = new HashSet<string>(StringComparer.Ordinal);
            var history = new Dictionary<int, ulong>();
            foreach (var pair in snapshot.HistoricalCounts)
            {
                if (pair.Key <= 0 || history.ContainsKey(pair.Key)) throw new ArgumentException("Invalid history.");
                history.Add(pair.Key, pair.Value);
            }
            void ValidateId(ulong value)
            {
                if (value == 0 || value > snapshot.AllocatedThrough || !identities.Add(value) ||
                    _registry.TryResolve(new PersistentId(value), PersistentObjectKind.Machine, out _) != PersistentRegistryResult.Missing)
                    throw new ArgumentException("Invalid or occupied recovered identity.");
            }
            foreach (var item in snapshot.Machines)
            {
                if (item == null) throw new ArgumentException("Null machine.");
                ValidateId(item.Id);
                var definition = machineDefinition(item.ModelId, item.Level);
                if (definition == null || definition.Id != item.ModelId || definition.Level != item.Level ||
                    string.IsNullOrWhiteSpace(item.Name) || item.Name != item.Name.Trim() || !names.Add(item.Name) ||
                    item.Serial == 0 || !history.TryGetValue(item.ModelId, out ulong total) || item.Serial > total ||
                    !serials.Add(item.ModelId + ":" + item.Serial)) throw new ArgumentException("Invalid machine record.");
                foreach (char c in item.Name) if (char.IsControl(c)) throw new ArgumentException("Invalid name.");
                var id = new PersistentId(item.Id);
                machines.Add(id, new MachineInstance(id, definition, item.Serial, item.Name));
            }
            foreach (var item in snapshot.Components)
            {
                if (item == null) throw new ArgumentException("Null component.");
                ValidateId(item.Id);
                var definition = componentDefinition(item.ModelId, item.Level);
                if (definition == null || definition.Id != item.ModelId || definition.Level != item.Level ||
                    (definition.Kind == HardwareKind.Core && !item.Enabled)) throw new ArgumentException("Invalid component record.");
                var id = new PersistentId(item.Id);
                var component = new ComponentInstance(id, definition) { Enabled = item.Enabled };
                components.Add(id, component);
                if (item.Owner != 0)
                {
                    if (!machines.TryGetValue(new PersistentId(item.Owner), out var owner) ||
                        owner.Install(ManagementOrigin.Library, component, item.Slot) != MachineManagementResult.Completed)
                        throw new ArgumentException("Invalid component ownership/slot.");
                }
                else if (item.Slot != -1) throw new ArgumentException("Unowned component has slot.");
            }
            foreach (var item in snapshot.Machines)
                machines[new PersistentId(item.Id)].RestoreConfiguration(item.Deployed, item.Activated, item.PowerSwitch,
                    item.RequestedState, item.Integrity, item.UsedCapacity);
            // All validation occurred on isolated instances; publish only once the complete graph is valid.
            var registered = new List<MachineInstance>();
            try
            {
                foreach (var machine in machines.Values)
                {
                    if (_registry.TryRegister(machine.Id, PersistentObjectKind.Machine, machine) != PersistentRegistryResult.Success)
                        throw new InvalidOperationException("Registry changed during restore.");
                    registered.Add(machine);
                }
            }
            catch
            {
                foreach (var machine in registered) _registry.TryUnregister(machine.Id, PersistentObjectKind.Machine, machine);
                throw;
            }
            if (snapshot.AllocatedThrough != 0) _ids.TryRestore(new PersistentId(snapshot.AllocatedThrough));
            foreach (var pair in machines) { _machines.Add(pair.Key, pair.Value); pair.Value.Changed += OnMachineChanged; }
            foreach (var pair in components) _components.Add(pair.Key, pair.Value);
            foreach (var name in names) _names.Add(name);
            foreach (var pair in history) _serials.Add(pair.Key, pair.Value);
            Changed?.Invoke();
        }
    }
}
