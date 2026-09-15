using System;
using System.Collections.Generic;
using AutoEra.DataTable;

namespace AutoEra.Machines
{
    public enum CatalogResourceState { DataOnly, Ready, DevelopmentProxy, PendingResource }

    /// <summary>Resource coverage index only; never grants machine instantiation or gameplay readiness.</summary>
    public sealed class FirstVersionCatalog
    {
        private readonly Dictionary<int, FirstVersionObjects> _entries = new Dictionary<int, FirstVersionObjects>();
        private static readonly HashSet<string> Categories = new HashSet<string>(StringComparer.Ordinal)
            { "Resource", "Energy", "Seed", "Crop", "Bundle", "Building", "ResourcePoint", "Carrier", "Core", "Sensor", "Effector" };
        public int Count => _entries.Count;
        public FirstVersionCatalog(IEnumerable<FirstVersionObjects> rows, MachineCatalog machines)
        {
            if (rows == null || machines == null) throw new ArgumentNullException();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                if (row == null || row.Id <= 0 || _entries.ContainsKey(row.Id) || string.IsNullOrWhiteSpace(row.Name) ||
                    !Categories.Contains(row.Category) || !names.Add(row.Category + ":" + row.Name) ||
                    !Enum.TryParse(row.ResourceState, out CatalogResourceState state) || !Enum.IsDefined(typeof(CatalogResourceState), state))
                    throw new FormatException("Invalid catalog entry.");
                bool requiresModel = row.Category == "Carrier" || row.Category == "Core" || row.Category == "Sensor" || row.Category == "Effector";
                if ((requiresModel && (row.DefinitionModelId <= 0 || row.DefinitionModelId > (int.MaxValue - 2) / 10 ||
                    !machines.Availability.ContainsKey(row.DefinitionModelId * 10 + 1))) || (!requiresModel && row.DefinitionModelId != 0))
                    throw new FormatException("Invalid catalog model reference: " + row.Id);
                bool visual = state == CatalogResourceState.Ready || state == CatalogResourceState.DevelopmentProxy;
                if (visual == string.IsNullOrWhiteSpace(row.Prefab)) throw new FormatException("Catalog resource state/path mismatch: " + row.Id);
                if (!string.IsNullOrEmpty(row.Prefab) && (row.Prefab.StartsWith("/", StringComparison.Ordinal) ||
                    row.Prefab.Contains("..") || row.Prefab.Contains(":") || row.Prefab.Contains("\\") || row.Prefab.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)))
                    throw new FormatException("Invalid catalog relative resource path: " + row.Id);
                _entries.Add(row.Id, row);
            }
        }
        public bool TryGet(int id, out FirstVersionObjects entry) => _entries.TryGetValue(id, out entry);
        public static FirstVersionCatalog FromLoadedGameData()
        {
            var table = GF.DataTable.GetDataTable<FirstVersionObjects>();
            if (table == null) throw new InvalidOperationException("First-version catalog has not loaded.");
            return new FirstVersionCatalog(table.GetAllDataRows(), MachineCatalog.FromLoadedGameData());
        }
    }
}
