using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Algorithms
{
    public sealed class AlgorithmPreparedSnapshot
    {
        public AlgorithmTrigger Trigger;
        public PersistentId ComputeLeaseId;
        public int Cost;
        public ulong[] Path;
        public Dictionary<string, AlgorithmValue> Writes;
        public Dictionary<ulong, AlgorithmValue> NodeValues;
        public AlgorithmIntent[] Intents;
        internal static AlgorithmPreparedSnapshot Capture(AlgorithmTrigger trigger, AlgorithmBatch batch, PersistentId lease)
        {
            var writes = new Dictionary<string, AlgorithmValue>(StringComparer.Ordinal); foreach (var pair in batch.Writes) writes.Add(pair.Key, pair.Value.Copy());
            var values = new Dictionary<ulong, AlgorithmValue>(); foreach (var pair in batch.NodeValues) values.Add(pair.Key, pair.Value.Copy());
            var intents = new AlgorithmIntent[batch.Intents.Count]; for (int i = 0; i < intents.Length; i++) intents[i] = Copy(batch.Intents[i]);
            return new AlgorithmPreparedSnapshot { Trigger = trigger.Copy(), ComputeLeaseId = lease, Cost = batch.Cost,
                Path = batch.Path.ToArray(), Writes = writes, NodeValues = values, Intents = intents };
        }
        private static AlgorithmIntent Copy(AlgorithmIntent source)
        {
            var result = new AlgorithmIntent { NodeId = source.NodeId, Kind = source.Kind, Value = source.Value?.Copy(), Action = source.Action,
                BindingKey = source.BindingKey, Field = source.Field };
            if (source.Parameters != null) { result.Parameters = new Dictionary<string, AlgorithmValue>(); foreach (var pair in source.Parameters) result.Parameters.Add(pair.Key, pair.Value.Copy()); }
            return result;
        }
        internal bool TryBuild(AlgorithmDocument document, out AlgorithmBatch batch)
        {
            batch = null;
            if (!ComputeLeaseId.IsValid || Cost <= 0 || Path == null || Writes == null || NodeValues == null || Intents == null) return false;
            var nodes = new Dictionary<ulong, AlgorithmNode>(); foreach (var node in document.Nodes) if (!node.Deleted) nodes.Add(node.Id, node);
            var result = new AlgorithmBatch { Cost = Cost };
            var touched = new HashSet<ulong>(); var writable = new Dictionary<string, AlgorithmType>(StringComparer.Ordinal); long cost = 0;
            foreach (var id in Path)
            {
                if (!nodes.TryGetValue(id, out var node) || !touched.Add(id)) return false;
                result.Path.Add(id); cost += AlgorithmCatalog.Cost(node.Kind);
                if (node.Kind == AlgorithmNodeKind.SetVariable || node.Kind == AlgorithmNodeKind.Hysteresis)
                    writable[node.StateKey] = node.Kind == AlgorithmNodeKind.Hysteresis ? AlgorithmType.Of(AlgorithmValueKind.Boolean) : node.ValueType;
            }
            if (cost != Cost) return false;
            foreach (var pair in Writes)
            {
                if (string.IsNullOrEmpty(pair.Key) || !AlgorithmPersistentValidation.Value(pair.Value) ||
                    !writable.TryGetValue(pair.Key, out var type) || !AlgorithmCatalog.Compatible(pair.Value.Type, type)) return false;
                result.Writes.Add(pair.Key, pair.Value.Copy());
            }
            foreach (ulong id in Path) if (nodes[id].Kind == AlgorithmNodeKind.SetVariable && !Writes.ContainsKey(nodes[id].StateKey)) return false;
            foreach (var pair in NodeValues) { if (!nodes.ContainsKey(pair.Key) || !AlgorithmPersistentValidation.Value(pair.Value)) return false; result.NodeValues.Add(pair.Key, pair.Value.Copy()); }
            foreach (var intent in Intents)
            {
                if (intent == null || !touched.Contains(intent.NodeId) || !nodes.TryGetValue(intent.NodeId, out var node) || node.Kind != intent.Kind) return false;
                switch (intent.Kind)
                {
                    case AlgorithmNodeKind.Delay:
                        if (!AlgorithmPersistentValidation.Value(intent.Value) || intent.Value.Type.Kind != AlgorithmValueKind.Number || intent.Value.Type.Unit != "s" || intent.Value.Number <= 0) return false; break;
                    case AlgorithmNodeKind.Navigate:
                        if (!AlgorithmPersistentValidation.Value(intent.Value) || intent.Value.Type.Kind != AlgorithmValueKind.Position) return false; break;
                    case AlgorithmNodeKind.Effector:
                        if (!intent.Action.HasValue || !Enum.IsDefined(typeof(AlgorithmEffectorAction), intent.Action.Value) || intent.Action != node.Action || intent.BindingKey != node.BindingKey) return false; break;
                    case AlgorithmNodeKind.Log: case AlgorithmNodeKind.SubmitTask: case AlgorithmNodeKind.CancelTask: break;
                    default: return false;
                }
                if (intent.Parameters != null) foreach (var pair in intent.Parameters) if (string.IsNullOrEmpty(pair.Key) || !AlgorithmPersistentValidation.Value(pair.Value)) return false;
                result.Intents.Add(Copy(intent));
            }
            batch = result; return true;
        }
    }
}
