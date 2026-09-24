using System;
using System.Collections.Generic;

namespace AutoEra.Algorithms
{
    public sealed class AlgorithmTrigger
    {
        public ulong NodeId, RootNodeId, Revision, Generation, Sequence, TaskId, SourceId, BindingGeneration, SourceTargetId;
        public string Port = "event";
        public long Time;
        public bool Continuous;
        public readonly Dictionary<string, AlgorithmValue> Inputs = new Dictionary<string, AlgorithmValue>(StringComparer.Ordinal);
        public AlgorithmTrigger Copy()
        {
            var copy = new AlgorithmTrigger { NodeId = NodeId, Revision = Revision, Generation = Generation, Sequence = Sequence, TaskId = TaskId, Port = Port, Time = Time,
                Continuous = Continuous, RootNodeId = RootNodeId, SourceId = SourceId, BindingGeneration = BindingGeneration, SourceTargetId = SourceTargetId };
            foreach (var pair in Inputs) copy.Inputs.Add(pair.Key, pair.Value.Copy());
            return copy;
        }
    }
    public sealed class AlgorithmIntent
    {
        public ulong NodeId;
        public AlgorithmNodeKind Kind;
        public AlgorithmValue Value;
        /// <summary>效应器动作类型（仅 Effector 意图使用，其余为空）。</summary>
        public AlgorithmEffectorAction? Action;
        /// <summary>效应器/任务控制意图的多参数快照（含 target 对象），仅这些意图填充。</summary>
        public Dictionary<string, AlgorithmValue> Parameters;
        /// <summary>效应器行为节点的组件绑定键（绑定效应器组件，与 Input 节点绑定传感器同源）。</summary>
        public string BindingKey;
        /// <summary>任务控制节点的稳定任务名（SubmitTask/QueryTask 对齐防重，取节点 Field）。</summary>
        public string Field;
    }
    public sealed class AlgorithmBatch
    {
        public string Error { get; internal set; }
        public ulong FailedNode { get; internal set; }
        public int Cost { get; internal set; }
        internal readonly List<ulong> Path = new List<ulong>();
        internal readonly Dictionary<string, AlgorithmValue> Writes = new Dictionary<string, AlgorithmValue>(StringComparer.Ordinal);
        internal readonly List<AlgorithmIntent> Intents = new List<AlgorithmIntent>();
        public IReadOnlyList<ulong> ExecutedPath => Path.AsReadOnly();
    }

    /// <summary>Pure speculative evaluation. No shared writes or world calls before batch commit.</summary>
    public sealed class AlgorithmEvaluation
    {
        private readonly Dictionary<ulong, AlgorithmNode> _nodes = new Dictionary<ulong, AlgorithmNode>();
        private readonly Dictionary<(ulong, string), AlgorithmEdge> _inputs = new Dictionary<(ulong, string), AlgorithmEdge>();
        private readonly Dictionary<(ulong, string), List<AlgorithmEdge>> _outputs = new Dictionary<(ulong, string), List<AlgorithmEdge>>();
        private readonly Dictionary<(ulong, string), AlgorithmValue> _cache = new Dictionary<(ulong, string), AlgorithmValue>();
        private readonly HashSet<ulong> _visited = new HashSet<ulong>();
        private AlgorithmTrigger _trigger;
        private IReadOnlyDictionary<string, AlgorithmValue> _state;
        private AlgorithmBatch _batch;
        private readonly Func<string, string, AlgorithmValue> _cargoReader;
        private readonly Func<string, AlgorithmValue> _taskQuerier;

        public AlgorithmEvaluation(AlgorithmPlan plan, Func<string, string, AlgorithmValue> cargoReader = null, Func<string, AlgorithmValue> taskQuerier = null)
        {
            _cargoReader = cargoReader; _taskQuerier = taskQuerier;
            var document = plan.CopyDocument();
            foreach (var node in document.Nodes) if (!node.Deleted) _nodes.Add(node.Id, node);
            foreach (var edge in document.Edges)
            {
                _inputs.Add(Key(edge.To, edge.Input), edge);
                var output = Key(edge.From, edge.Output);
                if (!_outputs.TryGetValue(output, out var list)) _outputs.Add(output, list = new List<AlgorithmEdge>());
                list.Add(edge);
            }
            foreach (var list in _outputs.Values) list.Sort((a, b) => a.To.CompareTo(b.To));
        }
        public AlgorithmBatch Evaluate(AlgorithmTrigger trigger, IReadOnlyDictionary<string, AlgorithmValue> state)
        {
            _trigger = trigger; _state = state; _batch = new AlgorithmBatch(); _cache.Clear(); _visited.Clear();
            try
            {
                Touch(trigger.NodeId);
                bool eventPort = false;
                foreach (var port in AlgorithmCatalog.Outputs(_nodes[trigger.NodeId]))
                    if (port.Key == trigger.Port && port.Type.Kind == AlgorithmValueKind.Event) eventPort = true;
                if (!eventPort) throw new EvaluationFailure(trigger.NodeId, "InvalidEventRoot");
                Emit(trigger.NodeId, trigger.Port, 0);
            }
            catch (EvaluationFailure error) { _batch.Error = error.Message; _batch.FailedNode = error.Node; _batch.Writes.Clear(); _batch.Intents.Clear(); }
            return _batch;
        }
        private static (ulong, string) Key(ulong id, string port) => (id, port);
        private void Touch(ulong id)
        {
            if (!_nodes.TryGetValue(id, out var node)) throw new EvaluationFailure(id, "UnknownTriggerNode");
            if (_visited.Add(id)) { _batch.Path.Add(id); _batch.Cost += AlgorithmCatalog.Cost(node.Kind); }
        }
        private void Emit(ulong id, string port, int depth)
        {
            if (depth > _nodes.Count) throw new EvaluationFailure(id, "SynchronousCycle");
            if (!_outputs.TryGetValue(Key(id, port), out var edges)) return;
            foreach (var edge in edges)
            {
                var node = _nodes[edge.To]; Touch(node.Id);
                switch (node.Kind)
                {
                    case AlgorithmNodeKind.Branch: Emit(node.Id, Read(node, "condition").Boolean ? "true" : "false", depth + 1); break;
                    case AlgorithmNodeKind.Merge: Emit(node.Id, "event", depth + 1); break;
                    case AlgorithmNodeKind.SetVariable:
                        _batch.Writes[node.StateKey] = Read(node, "value").Copy(); _cache.Clear(); break;
                    case AlgorithmNodeKind.Navigate:
                        _batch.Intents.Add(new AlgorithmIntent { NodeId = node.Id, Kind = node.Kind, Value = Read(node, "target").Copy() }); break;
                    case AlgorithmNodeKind.Delay:
                        var seconds = Read(node, "seconds");
                        if (seconds.Number <= 0) throw new EvaluationFailure(node.Id, "DelayMustBePositive");
                        _batch.Intents.Add(new AlgorithmIntent { NodeId = node.Id, Kind = node.Kind, Value = seconds.Copy() }); break;
                    case AlgorithmNodeKind.Log: _batch.Intents.Add(new AlgorithmIntent { NodeId = node.Id, Kind = node.Kind }); break;
                    case AlgorithmNodeKind.Effector:
                        _batch.Intents.Add(new AlgorithmIntent { NodeId = node.Id, Kind = node.Kind, Action = node.Action, Parameters = ReadParameters(node), BindingKey = node.BindingKey }); break;
                    case AlgorithmNodeKind.SubmitTask:
                        _batch.Intents.Add(new AlgorithmIntent { NodeId = node.Id, Kind = node.Kind, Field = node.Field }); break;
                    case AlgorithmNodeKind.CancelTask:
                        _batch.Intents.Add(new AlgorithmIntent { NodeId = node.Id, Kind = node.Kind, Parameters = ReadParameters(node) }); break;
                    case AlgorithmNodeKind.Hysteresis:
                        double hv = Read(node, "value").Number;
                        double onThreshold = Read(node, "on").Number;
                        double offThreshold = Read(node, "off").Number;
                        bool locked = _batch.Writes.TryGetValue(node.StateKey, out var hw) ? hw.Boolean : (_state.TryGetValue(node.StateKey, out var hs) && hs.Boolean);
                        if ((!locked && hv < onThreshold) || (locked && hv > offThreshold))
                        { _batch.Writes[node.StateKey] = AlgorithmValue.Bool(!locked); _cache.Clear(); Emit(node.Id, "event", depth + 1); }
                        break;
                    default: throw new EvaluationFailure(node.Id, "InvalidEventDestination");
                }
            }
        }
        private AlgorithmValue Read(AlgorithmNode node, string port)
        {
            if (!_inputs.TryGetValue(Key(node.Id, port), out var edge)) throw new EvaluationFailure(node.Id, "MissingInput");
            var result = Value(edge.From, edge.Output);
            if (!result.IsValid) throw new EvaluationFailure(node.Id, "InputUnavailable");
            return result;
        }
        private Dictionary<string, AlgorithmValue> ReadParameters(AlgorithmNode node)
        {
            var parameters = new Dictionary<string, AlgorithmValue>(StringComparer.Ordinal);
            foreach (var port in AlgorithmCatalog.Inputs(node))
            {
                if (port.Key == "event" || !_inputs.ContainsKey(Key(node.Id, port.Key))) continue;
                parameters.Add(port.Key, Read(node, port.Key).Copy());
            }
            return parameters;
        }
        private AlgorithmValue Value(ulong id, string port)
        {
            var key = Key(id, port);
            if (_cache.TryGetValue(key, out var result)) return result;
            var node = _nodes[id]; Touch(id);
            switch (node.Kind)
            {
                case AlgorithmNodeKind.Constant:
                case AlgorithmNodeKind.Parameter: result = node.Default.Copy(); break;
                case AlgorithmNodeKind.Variable:
                    if (!_batch.Writes.TryGetValue(node.StateKey, out result) && !_state.TryGetValue(node.StateKey, out result)) result = node.Default;
                    result = result.Copy(); break;
                case AlgorithmNodeKind.Hysteresis:
                    if (!_batch.Writes.TryGetValue(node.StateKey, out result) && !_state.TryGetValue(node.StateKey, out result)) result = AlgorithmValue.Bool(false);
                    result = result.Copy(); break;
                case AlgorithmNodeKind.Input:
                    bool exists = _trigger.Inputs.TryGetValue(node.BindingKey, out result);
                    if (port == "valid") result = AlgorithmValue.Bool(exists && result.IsValid);
                    else if (!exists) result = new AlgorithmValue { Type = node.ValueType.Copy(), IsValid = false };
                    else if (!AlgorithmCatalog.Compatible(result.Type, node.ValueType)) throw new EvaluationFailure(id, "InputTypeChanged");
                    break;
                case AlgorithmNodeKind.Arithmetic:
                    double a = Read(node, "a").Number, b = Read(node, "b").Number;
                    if (node.Operator == AlgorithmOperator.Divide && b == 0) throw new EvaluationFailure(id, "DivisionByZero");
                    double value;
                    switch (node.Operator)
                    {
                        case AlgorithmOperator.Add: value = a + b; break;
                        case AlgorithmOperator.Subtract: value = a - b; break;
                        case AlgorithmOperator.Multiply: value = a * b; break;
                        case AlgorithmOperator.Divide: value = a / b; break;
                        case AlgorithmOperator.Minimum: value = Math.Min(a, b); break;
                        default: value = Math.Max(a, b); break;
                    }
                    result = AlgorithmValue.Numeric(value, node.ValueType.Unit); break;
                case AlgorithmNodeKind.Compare:
                    var left = Read(node, "a"); var right = Read(node, "b");
                    int comparison = left.Type.Kind == AlgorithmValueKind.Object ? left.ObjectId.CompareTo(right.ObjectId) :
                        left.Type.Kind == AlgorithmValueKind.Enumeration ? left.EnumValue.CompareTo(right.EnumValue) : left.Number.CompareTo(right.Number);
                    result = AlgorithmValue.Bool(node.Operator == AlgorithmOperator.Equal ? comparison == 0 : node.Operator == AlgorithmOperator.NotEqual ? comparison != 0 :
                        node.Operator == AlgorithmOperator.Less ? comparison < 0 : node.Operator == AlgorithmOperator.Greater ? comparison > 0 :
                        node.Operator == AlgorithmOperator.LessOrEqual ? comparison <= 0 : comparison >= 0); break;
                case AlgorithmNodeKind.Boolean:
                    bool first = Read(node, "a").Boolean;
                    result = AlgorithmValue.Bool(node.Operator == AlgorithmOperator.Not ? !first : node.Operator == AlgorithmOperator.And ? first & Read(node, "b").Boolean : first | Read(node, "b").Boolean); break;
                case AlgorithmNodeKind.Cargo:
                    result = _cargoReader?.Invoke(port, node.Field);
                    if (result == null) result = new AlgorithmValue { Type = port == "has_item" ? AlgorithmType.Of(AlgorithmValueKind.Boolean) : AlgorithmType.Of(AlgorithmValueKind.Number), IsValid = false };
                    break;
                case AlgorithmNodeKind.QueryTask:
                    var queried = _taskQuerier?.Invoke(node.Field);
                    if (queried == null) result = new AlgorithmValue { Type = port == "found" ? AlgorithmType.Of(AlgorithmValueKind.Boolean) : AlgorithmType.Of(AlgorithmValueKind.Object), IsValid = false };
                    else if (port == "found") result = AlgorithmValue.Bool(queried.ObjectId != 0);
                    else result = queried.Copy();
                    break;
                default: throw new EvaluationFailure(id, "UnsupportedValueNode");
            }
            if (!AlgorithmValidator.Finite(result)) throw new EvaluationFailure(id, "NonFiniteValue");
            _cache[key] = result; return result;
        }
        private sealed class EvaluationFailure : Exception
        {
            internal readonly ulong Node;
            internal EvaluationFailure(ulong node, string reason) : base(reason) { Node = node; }
        }
    }
}
