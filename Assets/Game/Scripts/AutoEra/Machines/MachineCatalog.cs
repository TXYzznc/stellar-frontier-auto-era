using System;
using System.Collections.Generic;
using AutoEra.DataTable;

namespace AutoEra.Machines
{
    /// <summary>Product validation over tool-generated rows. Missing art is not a fake valid prefab.</summary>
    public sealed class MachineCatalog
    {
        private readonly Dictionary<int, MachineDefinition> _machines = new Dictionary<int, MachineDefinition>();
        private readonly Dictionary<int, ComponentDefinition> _components = new Dictionary<int, ComponentDefinition>();
        private readonly Dictionary<int, DefinitionAvailability> _availability = new Dictionary<int, DefinitionAvailability>();
        private readonly Dictionary<int, string> _prefabs = new Dictionary<int, string>();
        public IReadOnlyDictionary<int, DefinitionAvailability> Availability => _availability;

        public MachineCatalog(IEnumerable<MachineDefinitions> machines, IEnumerable<ComponentDefinitions> components,
            Func<string, bool> hasLanguageKey, Func<string, string> localize)
        {
            if (machines == null || components == null || hasLanguageKey == null || localize == null) throw new ArgumentNullException();
            foreach (var row in machines)
            {
                var state = ValidateCommon(row.Id, row.ModelId, row.Level, row.NameKey, row.Availability, row.Prefab,
                    row.PurchasePrice, row.RecyclePrice, row.IdlePower, row.WorkingPower, hasLanguageKey);
                if (row.SensorSlots < 0 || row.CoreSlots < 0 || row.EffectorSlots < 0 || row.BaseCapacity < 0) throw new FormatException("Invalid slots/capacity: " + row.Id);
                if (state != DefinitionAvailability.PendingConfiguration)
                    _machines.Add(row.Id, new MachineDefinition(row.ModelId, localize(row.NameKey), row.Level,
                        row.SensorSlots, row.CoreSlots, row.EffectorSlots, row.BaseCapacity, row.CanMove, row.CanRotate, row.MaximumIntegrity));
            }
            foreach (var row in components)
            {
                var state = ValidateCommon(row.Id, row.ModelId, row.Level, row.NameKey, row.Availability, row.Prefab,
                    row.PurchasePrice, row.RecyclePrice, row.IdlePower, row.WorkingPower, hasLanguageKey);
                if (!Enum.TryParse(row.Kind, out HardwareKind kind) || !Enum.IsDefined(typeof(HardwareKind), kind)) throw new FormatException("Invalid hardware kind: " + row.Id);
                if (state != DefinitionAvailability.PendingConfiguration)
                    _components.Add(row.Id, new ComponentDefinition(row.ModelId, kind, row.Level, row.AddedCapacity, row.ComputeCapacity, row.LogicCapacity, row.HasBehavior));
            }
        }

        private DefinitionAvailability ValidateCommon(int id, int model, int level, string key, string availability,
            string prefab, int price, int recycle, double idle, double working, Func<string, bool> hasKey)
        {
            if (id <= 0 || model <= 0 || level < 1 || level > 2 || (long)model * 10 + level != id || _availability.ContainsKey(id))
                throw new FormatException("Duplicate/invalid definition identity: " + id);
            if (string.IsNullOrEmpty(key) || !hasKey(key)) throw new FormatException("Missing name key: " + key);
            if (!Enum.TryParse(availability, out DefinitionAvailability state) || !Enum.IsDefined(typeof(DefinitionAvailability), state))
                throw new FormatException("Invalid availability: " + id);
            if (price < 0 || recycle < 0 || recycle > price || idle < 0 || working < idle ||
                double.IsNaN(idle) || double.IsInfinity(idle) || double.IsNaN(working) || double.IsInfinity(working))
                throw new FormatException("Invalid economic/power values: " + id);
            if ((!string.IsNullOrEmpty(prefab) && (!prefab.StartsWith("Machines/", StringComparison.Ordinal) || prefab.Contains("..") || prefab.Contains(":") || prefab.Contains("\\"))) ||
                (state == DefinitionAvailability.Ready && string.IsNullOrEmpty(prefab))) throw new FormatException("Invalid prefab reference: " + id);
            _availability.Add(id, state); _prefabs.Add(id, prefab);
            return state;
        }

        public bool TryGetMachine(int rowId, out MachineDefinition definition) => _machines.TryGetValue(rowId, out definition);
        public bool TryGetComponent(int rowId, out ComponentDefinition definition) => _components.TryGetValue(rowId, out definition);
        public bool TryGetReadyPrefab(int rowId, out string prefab)
        {
            prefab = null;
            if (!_availability.TryGetValue(rowId, out var state) || state != DefinitionAvailability.Ready) return false;
            prefab = _prefabs[rowId]; return true;
        }
        public static MachineCatalog FromLoadedGameData()
        {
            var machines = GF.DataTable.GetDataTable<MachineDefinitions>();
            var components = GF.DataTable.GetDataTable<ComponentDefinitions>();
            if (machines == null || components == null) throw new InvalidOperationException("Machine tables have not loaded.");
            return new MachineCatalog(machines.GetAllDataRows(), components.GetAllDataRows(), GF.Localization.HasRawString, key => GF.Localization.GetString(key));
        }
        public static bool IsGameDataLoaded => GF.DataTable != null && GF.Localization != null &&
            GF.DataTable.GetDataTable<MachineDefinitions>() != null && GF.DataTable.GetDataTable<MachineDefinitions>().Count > 0 &&
            GF.DataTable.GetDataTable<ComponentDefinitions>() != null && GF.DataTable.GetDataTable<ComponentDefinitions>().Count > 0 &&
            GF.Localization.HasRawString("Component.Core");
    }
}
