using System;
using System.Collections.Generic;
using System.Globalization;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    /// <summary>World-owned machine/component identity and ownership. No Unity object or static state.</summary>
    public sealed partial class MachineRoster : IDisposable
    {
        private readonly PersistentIdAllocator _ids;
        private readonly PersistentObjectRegistry _registry;
        private readonly Dictionary<PersistentId, MachineInstance> _machines = new Dictionary<PersistentId, MachineInstance>();
        private readonly Dictionary<PersistentId, ComponentInstance> _components = new Dictionary<PersistentId, ComponentInstance>();
        private readonly Dictionary<int, ulong> _serials = new Dictionary<int, ulong>();
        private readonly HashSet<string> _names = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<PersistentId, MachineHardwareOperation> _hardwareOperations = new Dictionary<PersistentId, MachineHardwareOperation>();
        public MachineHardwareOperation GetHardwareOperation(PersistentId id)
        {
            EnsureActive();
            if (!_hardwareOperations.TryGetValue(id, out var operation))
            { operation = new MachineHardwareOperation(this); _hardwareOperations.Add(id, operation); }
            return operation;
        }
        public IEnumerable<MachineInstance> Machines => _machines.Values;
        public IEnumerable<ComponentInstance> Components => _components.Values;
        private bool _disposed;
        public bool IsActive => !_disposed;
        public event Action<MachineRoster> Disposed;
        public event Action Changed;
        private void OnMachineChanged(MachineInstance machine) => Changed?.Invoke();
        private void EnsureActive() { if (_disposed) throw new ObjectDisposedException(nameof(MachineRoster)); }
        public bool TryGet(PersistentId id, out MachineInstance machine) => _machines.TryGetValue(id, out machine);

        public MachineRoster(PersistentIdAllocator ids, PersistentObjectRegistry registry)
        { _ids = ids ?? throw new ArgumentNullException(nameof(ids)); _registry = registry ?? throw new ArgumentNullException(nameof(registry)); }

        public MachineInstance Create(MachineDefinition definition)
        {
            EnsureActive();
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            _serials.TryGetValue(definition.Id, out ulong previous);
            ulong serial = checked(previous + 1);
            string name = definition.Name + "-" + serial.ToString("D3", CultureInfo.InvariantCulture);
            // A player may already have chosen a future default name. Never overwrite that identity.
            if (_names.Contains(name))
            {
                string prefix = name;
                int suffix = 1;
                do { name = prefix + " (" + (suffix++).ToString(CultureInfo.InvariantCulture) + ")"; } while (_names.Contains(name));
            }
            if (!_ids.TryAllocate(out var id)) throw new InvalidOperationException("Identity space exhausted.");
            var machine = new MachineInstance(id, definition, serial, name);
            if (_registry.TryRegister(id, PersistentObjectKind.Machine, machine) != PersistentRegistryResult.Success)
                throw new InvalidOperationException("Machine registration failed.");
            _machines.Add(id, machine); _names.Add(name); _serials[definition.Id] = serial;
            machine.Changed += OnMachineChanged; Changed?.Invoke();
            return machine;
        }

        public ComponentInstance CreateComponent(ComponentDefinition definition)
        {
            EnsureActive();
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (!_ids.TryAllocate(out var id)) throw new InvalidOperationException("Identity space exhausted.");
            var component = new ComponentInstance(id, definition);
            _components.Add(id, component);
            Changed?.Invoke();
            return component;
        }

        public MachineManagementResult Rename(PersistentId id, string name)
        {
            if (!_machines.TryGetValue(id, out var machine)) return MachineManagementResult.InvalidState;
            if (string.IsNullOrWhiteSpace(name)) return MachineManagementResult.InvalidName;
            name = name.Trim();
            foreach (char character in name) if (char.IsControl(character)) return MachineManagementResult.InvalidName;
            if (machine.Name == name) return MachineManagementResult.Completed;
            if (!_names.Add(name)) return MachineManagementResult.DuplicateName;
            _names.Remove(machine.Name); machine.Name = name; machine.NotifyChanged();
            return MachineManagementResult.Completed;
        }

        public MachineManagementResult Deploy(PersistentId id)
        {
            if (!_machines.TryGetValue(id, out var machine) || machine.Deployed) return MachineManagementResult.InvalidState;
            if (machine.Integrity <= 0) return MachineManagementResult.Destroyed;
            machine.Deployed = true; machine.NotifyChanged(); return MachineManagementResult.Completed;
        }

        public MachineManagementResult Install(PersistentId machineId, ManagementOrigin origin, PersistentId componentId, int index)
        {
            if (!_machines.TryGetValue(machineId, out var machine)) return MachineManagementResult.InvalidState;
            return _components.TryGetValue(componentId, out var component)
                ? machine.Install(origin, component, index) : MachineManagementResult.MissingComponent;
        }
        public MachineManagementResult Remove(PersistentId machineId, ManagementOrigin origin, HardwareKind kind, int index)
            => _machines.TryGetValue(machineId, out var machine) ? machine.Remove(origin, kind, index) : MachineManagementResult.InvalidState;

        public ulong GetHistoricalCount(int definitionId) => _serials.TryGetValue(definitionId, out var count) ? count : 0;

        /// <summary>Restores historical high water marks before creating new machines, including lost machines.</summary>
        public void RestoreHistoricalCounts(IEnumerable<KeyValuePair<int, ulong>> counts)
        {
            EnsureActive();
            if (counts == null) throw new ArgumentNullException(nameof(counts));
            var staged = new Dictionary<int, ulong>();
            foreach (var pair in counts)
            {
                if (pair.Key <= 0 || pair.Value < GetHistoricalCount(pair.Key) || staged.ContainsKey(pair.Key))
                    throw new ArgumentException("Invalid historical model count.");
                staged.Add(pair.Key, pair.Value);
            }
            foreach (var pair in staged) _serials[pair.Key] = pair.Value;
        }

        // Library recovery keeps ownership/identity. Selling is a separate economy responsibility.
        public MachineManagementResult RecoverToLibrary(PersistentId id, ManagementOrigin origin)
        {
            if (!_machines.TryGetValue(id, out var machine)) return MachineManagementResult.InvalidState;
            if (origin != ManagementOrigin.Field || !machine.Deployed) return MachineManagementResult.InvalidOrigin;
            if (machine.Integrity < machine.Definition.MaximumIntegrity) return MachineManagementResult.RepairRequired;
            if (machine.RequestedRunState != MachineRunState.Stopped) return MachineManagementResult.MustStop;
            if (machine.HasActiveBehavior) return MachineManagementResult.WaitingForSafeStop;
            machine.Deployed = false;
            machine.UpdateEnvironment(false, false);
            machine.NotifyChanged();
            return MachineManagementResult.Completed;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var operation in _hardwareOperations.Values) operation.Dispose();
            _hardwareOperations.Clear();
            foreach (var machine in _machines.Values)
            {
                machine.Changed -= OnMachineChanged;
                _registry.TryUnregister(machine.Id, PersistentObjectKind.Machine, machine);
            }
            _machines.Clear(); _components.Clear(); _names.Clear();
            Changed = null;
            var disposed = Disposed;
            Disposed = null;
            disposed?.Invoke(this);
        }
    }
}
