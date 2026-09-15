using System;
using System.Collections.Generic;

namespace AutoEra.Algorithms
{
    public enum AlgorithmIssueSeverity { Error, Warning }
    public sealed class AlgorithmIssue
    {
        public string Code { get; }
        public ulong NodeId { get; }
        public string PortId { get; }
        public AlgorithmIssueSeverity Severity { get; }
        private readonly ulong[] _path;
        public ulong[] CopyPath() => (ulong[])_path.Clone();
        public AlgorithmIssue(string code, ulong node = 0, string port = null, AlgorithmIssueSeverity severity = AlgorithmIssueSeverity.Error, ulong[] path = null)
        { Code = code; NodeId = node; PortId = port; Severity = severity; _path = path == null ? Array.Empty<ulong>() : (ulong[])path.Clone(); }
    }

    public sealed class AlgorithmPlan
    {
        private readonly AlgorithmDocument _document;
        public ulong Revision => _document.Revision;
        public int LogicCost { get; }
        internal AlgorithmPlan(AlgorithmDocument document, int cost) { _document = document.Copy(); LogicCost = cost; }
        public AlgorithmDocument CopyDocument() => _document.Copy();
    }

    public static class AlgorithmValidator
    {
        public static bool TryCompile(AlgorithmDocument document, int capacity, out AlgorithmPlan plan, out List<AlgorithmIssue> issues, bool template = false)
        {
            plan = null; issues = new List<AlgorithmIssue>();
            if (document == null || document.SchemaVersion != 1 || document.LanguageVersion != "1" || document.DocumentId == 0 || document.Revision == 0 ||
                document.Nodes == null || document.Edges == null || document.Bindings == null)
            { issues.Add(new AlgorithmIssue("DocumentVersionOrStructure")); return false; }
            document = document.Copy();
            // Constants inherit a single required unit on the compiled copy, never mutate the draft.
            foreach (var constant in document.Nodes)
            {
                if (constant == null || constant.Deleted || constant.Kind != AlgorithmNodeKind.Constant || constant.ValueType?.Kind != AlgorithmValueKind.Number || !string.IsNullOrEmpty(constant.ValueType.Unit)) continue;
                string inferred = null;
                foreach (var edge in document.Edges)
                {
                    if (edge == null || edge.From != constant.Id) continue;
                    var target = document.Nodes.Find(n => n != null && n.Id == edge.To);
                    if (target?.ValueType == null) continue;
                    var port = Find(AlgorithmCatalog.Inputs(target), edge.Input);
                    if (port?.Type.Kind != AlgorithmValueKind.Number || string.IsNullOrEmpty(port.Type.Unit)) continue;
                    if (inferred != null && inferred != port.Type.Unit) issues.Add(new AlgorithmIssue("ConstantUnitConflict", constant.Id));
                    inferred = port.Type.Unit;
                }
                if (inferred != null) { constant.ValueType.Unit = inferred; if (constant.Default?.Type != null) constant.Default.Type.Unit = inferred; }
            }
            var nodes = new Dictionary<ulong, AlgorithmNode>();
            var stateTypes = new Dictionary<string, AlgorithmType>(StringComparer.Ordinal);
            var bindings = new Dictionary<string, AlgorithmBinding>(StringComparer.Ordinal);
            foreach (var binding in document.Bindings)
            {
                if (binding == null || string.IsNullOrEmpty(binding.Key) || bindings.ContainsKey(binding.Key)) { issues.Add(new AlgorithmIssue("BindingIdentity")); continue; }
                bindings.Add(binding.Key, binding);
            }
            int cost = 0;
            foreach (var node in document.Nodes)
            {
                if (node == null || node.Id == 0 || nodes.ContainsKey(node.Id)) { issues.Add(new AlgorithmIssue("NodeIdentity", node?.Id ?? 0)); continue; }
                nodes.Add(node.Id, node);
                if (node.Deleted) continue;
                if (!Enum.IsDefined(typeof(AlgorithmNodeKind), node.Kind) || node.ValueType == null || !Enum.IsDefined(typeof(AlgorithmValueKind), node.ValueType.Kind))
                { issues.Add(new AlgorithmIssue("UnsupportedNodeOrType", node.Id)); continue; }
                cost += AlgorithmCatalog.Cost(node.Kind);
                if (!Enum.IsDefined(typeof(AlgorithmOperator),node.Operator)) issues.Add(new AlgorithmIssue("UnknownOperator",node.Id));
                if (node.ValueType.Kind == AlgorithmValueKind.Event && (node.Kind == AlgorithmNodeKind.Constant || node.Kind == AlgorithmNodeKind.Parameter || node.Kind == AlgorithmNodeKind.Variable))
                    issues.Add(new AlgorithmIssue("EventIsNotAValue",node.Id));
                if (node.ValueType.Kind == AlgorithmValueKind.Objects || node.ValueType.Kind >= AlgorithmValueKind.Communication)
                    issues.Add(new AlgorithmIssue("StructuredEndpointNotImplemented", node.Id));
                if (node.Kind == AlgorithmNodeKind.Arithmetic && (node.ValueType.Kind != AlgorithmValueKind.Number || node.Operator > AlgorithmOperator.Maximum))
                    issues.Add(new AlgorithmIssue("ArithmeticOperator", node.Id));
                if (node.Kind == AlgorithmNodeKind.Arithmetic && (node.Operator == AlgorithmOperator.Multiply || node.Operator == AlgorithmOperator.Divide) && !string.IsNullOrEmpty(node.ValueType.Unit))
                    issues.Add(new AlgorithmIssue("UnsupportedCompoundDimension", node.Id));
                if (node.Kind == AlgorithmNodeKind.Compare && (node.Operator < AlgorithmOperator.Equal || node.Operator > AlgorithmOperator.Greater ||
                    (node.ValueType.Kind != AlgorithmValueKind.Number && node.ValueType.Kind != AlgorithmValueKind.Enumeration && node.ValueType.Kind != AlgorithmValueKind.Object)))
                    issues.Add(new AlgorithmIssue("ComparisonOperator", node.Id));
                if (node.Kind == AlgorithmNodeKind.Compare && node.ValueType.Kind != AlgorithmValueKind.Number && node.Operator != AlgorithmOperator.Equal && node.Operator != AlgorithmOperator.NotEqual)
                    issues.Add(new AlgorithmIssue("ComparisonRequiresNumber", node.Id));
                if (node.Kind == AlgorithmNodeKind.Boolean && (node.Operator < AlgorithmOperator.And || node.Operator > AlgorithmOperator.Not)) issues.Add(new AlgorithmIssue("BooleanOperator", node.Id));
                if (node.Kind == AlgorithmNodeKind.Variable || node.Kind == AlgorithmNodeKind.SetVariable)
                {
                    if (string.IsNullOrEmpty(node.StateKey)) issues.Add(new AlgorithmIssue("StateKeyMissing", node.Id));
                    else if (stateTypes.TryGetValue(node.StateKey, out var stateType) && (!AlgorithmCatalog.Compatible(stateType,node.ValueType)||!AlgorithmCatalog.Compatible(node.ValueType,stateType))) issues.Add(new AlgorithmIssue("StateTypeMismatch",node.Id));
                    else stateTypes[node.StateKey] = node.ValueType;
                }
                if (node.Kind == AlgorithmNodeKind.Constant || node.Kind == AlgorithmNodeKind.Parameter || node.Kind == AlgorithmNodeKind.Variable)
                    if (node.Default == null || !AlgorithmCatalog.Compatible(node.Default.Type, node.ValueType) || !Finite(node.Default)) issues.Add(new AlgorithmIssue("DefaultInvalid", node.Id));
                if (node.Kind == AlgorithmNodeKind.Input && !template)
                {
                    if (string.IsNullOrEmpty(node.BindingKey) || !bindings.TryGetValue(node.BindingKey, out var binding) || binding.ComponentId == 0 || binding.TargetId == 0 ||
                        !AlgorithmCatalog.Compatible(binding.Type, node.ValueType)) issues.Add(new AlgorithmIssue("RequiredBinding", node.Id));
                    else if (!binding.Available) issues.Add(new AlgorithmIssue("BindingTemporarilyUnavailable", node.Id, null, AlgorithmIssueSeverity.Warning));
                }
            }
            if (capacity < 0 || cost > capacity) issues.Add(new AlgorithmIssue("LogicCapacity"));
            var occupied = new HashSet<string>();
            var dataArcs = new Dictionary<ulong, List<ulong>>();
            var eventArcs = new Dictionary<ulong, List<ulong>>();
            foreach (var edge in document.Edges)
            {
                if (edge == null || !nodes.TryGetValue(edge.From, out var from) || !nodes.TryGetValue(edge.To, out var to) || from.Deleted || to.Deleted || from.ValueType == null || to.ValueType == null)
                { issues.Add(new AlgorithmIssue("DanglingEdge", edge?.To ?? 0)); continue; }
                var output = Find(AlgorithmCatalog.Outputs(from), edge.Output);
                var input = Find(AlgorithmCatalog.Inputs(to), edge.Input);
                if (output == null || input == null) { issues.Add(new AlgorithmIssue("PortMissing", to.Id, edge.Input)); continue; }
                if (!AlgorithmCatalog.Compatible(output.Type, input.Type)) issues.Add(new AlgorithmIssue("TypeUnitCapabilityMismatch", to.Id, edge.Input));
                if (!occupied.Add(to.Id + ":" + edge.Input)) issues.Add(new AlgorithmIssue("InputAlreadyConnected", to.Id, edge.Input));
                var arcs = output.Type.Kind == AlgorithmValueKind.Event ? eventArcs : dataArcs;
                if (output.Type.Kind == AlgorithmValueKind.Event && (from.Kind == AlgorithmNodeKind.Delay || from.Kind == AlgorithmNodeKind.Navigate || from.Kind == AlgorithmNodeKind.Input)) continue;
                if (!arcs.TryGetValue(from.Id, out var targets)) arcs.Add(from.Id, targets = new List<ulong>());
                targets.Add(to.Id);
            }
            foreach (var node in nodes.Values)
            {
                if (node.Deleted || node.ValueType == null) continue;
                foreach (var port in AlgorithmCatalog.Inputs(node)) if (port.Required && !occupied.Contains(node.Id + ":" + port.Key)) issues.Add(new AlgorithmIssue("InputRequired", node.Id, port.Key));
            }
            DetectCycles(nodes, dataArcs, issues); DetectCycles(nodes, eventArcs, issues);
            if (issues.Exists(issue => issue.Severity == AlgorithmIssueSeverity.Error)) return false;
            plan = new AlgorithmPlan(document, cost); return true;
        }
        internal static bool Finite(AlgorithmValue value) => value != null && value.Type != null &&
            !double.IsNaN(value.Number) && !double.IsInfinity(value.Number) &&
            !double.IsNaN(value.X + value.Y + value.Z) && !double.IsInfinity(value.X) && !double.IsInfinity(value.Y) && !double.IsInfinity(value.Z);
        private static AlgorithmPort Find(AlgorithmPort[] ports, string key) { foreach (var port in ports) if (port.Key == key) return port; return null; }
        private static void DetectCycles(Dictionary<ulong, AlgorithmNode> nodes, Dictionary<ulong, List<ulong>> arcs, List<AlgorithmIssue> issues)
        {
            var marks = new Dictionary<ulong, int>();
            var path = new List<ulong>();
            foreach (var id in nodes.Keys) Visit(id, arcs, marks, issues, path);
        }
        private static void Visit(ulong id, Dictionary<ulong, List<ulong>> arcs, Dictionary<ulong, int> marks, List<AlgorithmIssue> issues, List<ulong> path)
        {
            if (marks.TryGetValue(id, out int mark))
            {
                if (mark == 1) { var cycle = path.GetRange(path.IndexOf(id),path.Count-path.IndexOf(id));cycle.Add(id);issues.Add(new AlgorithmIssue("SynchronousCycle",id,null,AlgorithmIssueSeverity.Error,cycle.ToArray())); }
                return;
            }
            marks[id] = 1;path.Add(id);
            if (arcs.TryGetValue(id, out var targets)) foreach (var target in targets) Visit(target, arcs, marks, issues,path);
            path.RemoveAt(path.Count-1);
            marks[id] = 2;
        }
    }
}
