using System;
using System.Collections.Generic;
using System.Globalization;
using AutoEra.Buildings;
using AutoEra.DataTable;

namespace AutoEra.Logistics
{
    public sealed class ResourceItemDefinition
    {
        public string Id { get; }
        public CargoItemClass Class { get; }
        public int ModelId { get; }
        public ResourceItemDefinition(string id, CargoItemClass kind, int modelId = 0)
        {
            if (string.IsNullOrWhiteSpace(id) || kind == CargoItemClass.Unknown || !Enum.IsDefined(typeof(CargoItemClass), kind) ||
                ((kind == CargoItemClass.Component || kind == CargoItemClass.MachineCarrier) && modelId <= 0))
                throw new ArgumentException("Invalid resource item definition.");
            Id = id; Class = kind; ModelId = modelId;
        }
    }

    /// <summary>Immutable item identities and routing classes. Art availability never grants gameplay readiness.</summary>
    public sealed class ResourceItemCatalog
    {
        public const string Gold = "30001", Wood = "30002", Ore = "30003", Water = "30004", Biomass = "30005";
        private readonly Dictionary<string, ResourceItemDefinition> _items = new Dictionary<string, ResourceItemDefinition>(StringComparer.Ordinal);
        public int Count => _items.Count;
        public IReadOnlyList<ResourceItemDefinition> Definitions { get; }
        public ResourceItemCatalog(IEnumerable<ResourceItemDefinition> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            foreach (var item in items)
            {
                if (item == null || _items.ContainsKey(item.Id)) throw new ArgumentException("Duplicate or null resource item.");
                _items.Add(item.Id, item);
            }
            var definitions = new List<ResourceItemDefinition>(_items.Values);
            definitions.Sort((a,b) => string.CompareOrdinal(a.Id,b.Id)); Definitions = definitions.AsReadOnly();
        }
        public bool TryGet(string id, out ResourceItemDefinition item)
        { item = null; return id != null && _items.TryGetValue(id, out item); }
        public static ResourceItemCatalog FromLoadedGameData()
        {
            var table = GF.DataTable.GetDataTable<FirstVersionObjects>();
            if (table == null) throw new InvalidOperationException("First-version item identities have not loaded.");
            var items = new List<ResourceItemDefinition>();
            foreach (var row in table.GetAllDataRows())
            {
                CargoItemClass kind;
                switch (row.Category)
                {
                    case "Resource": kind = row.Id == 30001 ? CargoItemClass.Gold : CargoItemClass.CommonResource; break;
                    case "Seed": kind = CargoItemClass.Seed; break;
                    case "Crop": kind = CargoItemClass.FarmProduct; break;
                    case "Carrier": kind = CargoItemClass.MachineCarrier; break;
                    case "Core": case "Sensor": case "Effector": kind = CargoItemClass.Component; break;
                    default: continue; // Energy, bundles and structures do not imply a supported cargo item.
                }
                items.Add(new ResourceItemDefinition(row.Id.ToString(CultureInfo.InvariantCulture), kind, row.DefinitionModelId));
            }
            return new ResourceItemCatalog(items);
        }
    }
}
