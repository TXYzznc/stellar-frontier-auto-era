using System;
using System.Collections.Generic;
using AutoEra.DataTable;

namespace AutoEra.Machines.Sensors
{
    public sealed class SensorCatalog
    {
        private readonly Dictionary<int, SensorProfile> _profiles = new Dictionary<int, SensorProfile>();
        public int Count => _profiles.Count;
        public SensorCatalog(IEnumerable<SensorDefinitions> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));
            foreach (var row in rows)
            {
                if (row == null || (long)row.ComponentModelId * 10 + row.Level != row.Id ||
                    !Enum.TryParse(row.Kind, out SensorKind kind) || !Enum.IsDefined(typeof(SensorKind), kind))
                    throw new FormatException("Invalid sensor identity or kind.");
                var profile = new SensorProfile(row.ComponentModelId, row.Level, kind);
                if (row.IntervalMilliseconds != profile.IntervalMilliseconds || row.Range != profile.Range || row.ComputeCost != profile.ComputeCost)
                    throw new FormatException("Sensor configuration differs from DEC-197: " + row.Id);
                _profiles.Add(row.Id, profile);
            }
        }
        public bool TryGet(int componentRowId, out SensorProfile profile) => _profiles.TryGetValue(componentRowId, out profile);
        public static SensorCatalog FromLoadedGameData()
        {
            var table = GF.DataTable.GetDataTable<SensorDefinitions>();
            if (table == null || table.Count == 0) throw new InvalidOperationException("Sensor table has not loaded.");
            return new SensorCatalog(table.GetAllDataRows());
        }
    }
}
