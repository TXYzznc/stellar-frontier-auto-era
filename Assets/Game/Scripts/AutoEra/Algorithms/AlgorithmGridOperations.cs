using System;
using System.Collections.Generic;
using AutoEra.ResourcePoints;
using Newtonsoft.Json;

namespace AutoEra.Algorithms
{
    /// <summary>Closed immutable payload and fixed grid operations shared by all effectors.</summary>
    public sealed class AlgorithmTreeGrid
    {
        [JsonProperty(Required = Required.Always)] public IReadOnlyList<TreeReadout> Cells { get; }
        [JsonConstructor]
        internal AlgorithmTreeGrid(List<TreeReadout> cells) : this((IEnumerable<TreeReadout>)cells) { }
        public AlgorithmTreeGrid(IEnumerable<TreeReadout> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var rows = new List<TreeReadout>(values); rows.Sort((a, b) => a.Id.CompareTo(b.Id)); ulong previous = 0;
            foreach (var row in rows)
            {
                if (!row.Id.IsValid || row.Id.Value == previous || !ProductionRules.Finite(row.HP) || !ProductionRules.Finite(row.Height) || row.HP < 0 || row.HP > row.Height)
                    throw new ArgumentException("Invalid tree-grid cell.");
                previous = row.Id.Value;
            }
            Cells = rows.AsReadOnly();
        }
    }
    public static class AlgorithmGridOperations
    {
        public static bool IsFilter(string field) => field == "all" || field == "standing" || field == "mature" || field == "damaged";
        public static bool IsField(string field) => field == "height" || field == "hp" || field == "ratio" || field == "stage" || field == "position";
        public static AlgorithmType FieldType(string field)
        {
            if (field == "position") return AlgorithmType.Of(AlgorithmValueKind.Position);
            if (field == "stage") return new AlgorithmType { Kind = AlgorithmValueKind.Enumeration, EnumFamily = "TreeStage" };
            return AlgorithmType.Of(AlgorithmValueKind.Number, field == "height" || field == "hp" ? "m" : "");
        }
        public static AlgorithmValue Filter(AlgorithmValue grid, string field)
        {
            if (grid?.Trees == null || !IsFilter(field)) return Invalid(AlgorithmValueKind.TreeGrid);
            var rows = new List<TreeReadout>();
            foreach (var cell in grid.Trees.Cells)
            {
                bool standing = cell.Stage == TreeStage.Growing || cell.Stage == TreeStage.Mature;
                if (field == "all" || field == "standing" && standing || field == "mature" && cell.Stage == TreeStage.Mature || field == "damaged" && standing && cell.HP < cell.Height) rows.Add(cell);
            }
            return new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.TreeGrid), Trees = new AlgorithmTreeGrid(rows) };
        }
        public static AlgorithmValue Select(AlgorithmValue grid)
        {
            if (grid?.Trees == null || grid.Trees.Cells.Count == 0) return Invalid(AlgorithmValueKind.Object);
            return new AlgorithmValue { Type = new AlgorithmType { Kind = AlgorithmValueKind.Object, ObjectCategory = "Tree" }, ObjectId = grid.Trees.Cells[0].Id.Value };
        }
        public static AlgorithmValue Read(AlgorithmValue grid, AlgorithmValue selected, string field)
        {
            if (grid?.Trees == null || !grid.IsValid || selected == null || !selected.IsValid || selected.Type?.Kind != AlgorithmValueKind.Object ||
                selected.Type.ObjectCategory != "Tree" || !IsField(field)) return new AlgorithmValue { Type = FieldType(field), IsValid = false };
            foreach (var cell in grid.Trees.Cells)
            {
                if (cell.Id.Value != selected.ObjectId || cell.Stage == TreeStage.Removed) continue;
                switch (field)
                {
                    case "position": return new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Position), X = cell.Position.x, Y = cell.Position.y, Z = cell.Position.z };
                    case "stage": return new AlgorithmValue { Type = FieldType(field), EnumValue = (int)cell.Stage };
                    case "height": return AlgorithmValue.Numeric(cell.Height, "m");
                    case "hp": return AlgorithmValue.Numeric(cell.HP, "m");
                    default: return AlgorithmValue.Numeric(cell.HPRatio);
                }
            }
            return new AlgorithmValue { Type = FieldType(field), IsValid = false };
        }
        private static AlgorithmValue Invalid(AlgorithmValueKind kind) => new AlgorithmValue { Type = AlgorithmType.Of(kind), IsValid = false };
    }
}
