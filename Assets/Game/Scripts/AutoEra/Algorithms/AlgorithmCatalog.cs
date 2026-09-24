using System;

namespace AutoEra.Algorithms
{
    public sealed class AlgorithmPort
    {
        public string Key { get; }
        public AlgorithmType Type { get; }
        public bool Required { get; }
        public AlgorithmPort(string key, AlgorithmType type, bool required = true) { Key = key; Type = type; Required = required; }
    }

    public static class AlgorithmCatalog
    {
        public static int Cost(AlgorithmNodeKind kind)
        {
            if (kind == AlgorithmNodeKind.Variable || kind == AlgorithmNodeKind.SetVariable || kind == AlgorithmNodeKind.Delay || kind == AlgorithmNodeKind.Hysteresis) return 2;
            if (kind == AlgorithmNodeKind.SubmitTask || kind == AlgorithmNodeKind.QueryTask || kind == AlgorithmNodeKind.CancelTask) return 4;
            return 1;
        }
        public static AlgorithmPort[] Inputs(AlgorithmNode node)
        {
            var value = node.ValueType;
            var signal = AlgorithmType.Of(AlgorithmValueKind.Event);
            var boolean = AlgorithmType.Of(AlgorithmValueKind.Boolean);
            switch (node.Kind)
            {
                case AlgorithmNodeKind.Arithmetic:
                case AlgorithmNodeKind.Compare: return new[] { new AlgorithmPort("a", value), new AlgorithmPort("b", value) };
                case AlgorithmNodeKind.Boolean: return node.Operator == AlgorithmOperator.Not ? new[] { new AlgorithmPort("a", boolean) } : new[] { new AlgorithmPort("a", boolean), new AlgorithmPort("b", boolean) };
                case AlgorithmNodeKind.Branch: return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("condition", boolean) };
                case AlgorithmNodeKind.Merge: return new[] { new AlgorithmPort("a", signal, false), new AlgorithmPort("b", signal, false) };
                case AlgorithmNodeKind.SetVariable: return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("value", value) };
                case AlgorithmNodeKind.Delay: return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("seconds", AlgorithmType.Of(AlgorithmValueKind.Number, "s")) };
                case AlgorithmNodeKind.Navigate: return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("target", AlgorithmType.Of(AlgorithmValueKind.Position)) };
                case AlgorithmNodeKind.Effector: return EffectorInputs(node);
                case AlgorithmNodeKind.SubmitTask: return new[] { new AlgorithmPort("event", signal) };
                case AlgorithmNodeKind.QueryTask: return Array.Empty<AlgorithmPort>();
                case AlgorithmNodeKind.CancelTask: return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("task", AlgorithmType.Of(AlgorithmValueKind.Object)) };
                case AlgorithmNodeKind.Hysteresis: return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("value", AlgorithmType.Of(AlgorithmValueKind.Number)), new AlgorithmPort("on", AlgorithmType.Of(AlgorithmValueKind.Number)), new AlgorithmPort("off", AlgorithmType.Of(AlgorithmValueKind.Number)) };
                case AlgorithmNodeKind.Log: return new[] { new AlgorithmPort("event", signal) };
                case AlgorithmNodeKind.Cargo: return new[] { new AlgorithmPort("event", signal, false) };
                default: return Array.Empty<AlgorithmPort>();
            }
        }
        private static AlgorithmPort[] EffectorInputs(AlgorithmNode node)
        {
            var signal = AlgorithmType.Of(AlgorithmValueKind.Event);
            switch (node.Action)
            {
                case AlgorithmEffectorAction.Spray:
                    return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("on", AlgorithmType.Of(AlgorithmValueKind.Boolean)), new AlgorithmPort("flow", AlgorithmType.Of(AlgorithmValueKind.Number), false) };
                case AlgorithmEffectorAction.ModifySpray:
                    return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("flow", AlgorithmType.Of(AlgorithmValueKind.Number)) };
                case AlgorithmEffectorAction.StopSpray:
                    return new[] { new AlgorithmPort("event", signal) };
                case AlgorithmEffectorAction.Sow:
                    return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("count", AlgorithmType.Of(AlgorithmValueKind.Number)), new AlgorithmPort("crop", AlgorithmType.Of(AlgorithmValueKind.Enumeration), false) };
                case AlgorithmEffectorAction.Harvest:
                case AlgorithmEffectorAction.Cut:
                case AlgorithmEffectorAction.Drill:
                    return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("count", AlgorithmType.Of(AlgorithmValueKind.Number)) };
                case AlgorithmEffectorAction.Clean:
                    return new[] { new AlgorithmPort("event", signal) };
                case AlgorithmEffectorAction.Transfer:
                    return new[] { new AlgorithmPort("event", signal), new AlgorithmPort("count", AlgorithmType.Of(AlgorithmValueKind.Number)), new AlgorithmPort("item", AlgorithmType.Of(AlgorithmValueKind.Enumeration)) };
                default:
                    return new[] { new AlgorithmPort("event", signal) };
            }
        }
        public static AlgorithmPort[] Outputs(AlgorithmNode node)
        {
            var signal = AlgorithmType.Of(AlgorithmValueKind.Event);
            switch (node.Kind)
            {
                case AlgorithmNodeKind.Startup:
                case AlgorithmNodeKind.Merge:
                case AlgorithmNodeKind.Delay: return new[] { new AlgorithmPort("event", signal) };
                case AlgorithmNodeKind.Branch: return new[] { new AlgorithmPort("true", signal), new AlgorithmPort("false", signal) };
                case AlgorithmNodeKind.Navigate: return new[] { new AlgorithmPort("accepted", signal), new AlgorithmPort("started", signal), new AlgorithmPort("completed", signal), new AlgorithmPort("failed", signal), new AlgorithmPort("cancelled", signal), new AlgorithmPort("rejected", signal), new AlgorithmPort("partial",signal), new AlgorithmPort("preempted",signal), new AlgorithmPort("targetInvalid",signal) };
                case AlgorithmNodeKind.Effector: return EffectorOutputs();
                case AlgorithmNodeKind.SubmitTask: return new[] { new AlgorithmPort("accepted", signal), new AlgorithmPort("task", AlgorithmType.Of(AlgorithmValueKind.Object)) };
                case AlgorithmNodeKind.QueryTask: return new[] { new AlgorithmPort("found", AlgorithmType.Of(AlgorithmValueKind.Boolean)), new AlgorithmPort("task", AlgorithmType.Of(AlgorithmValueKind.Object)) };
                case AlgorithmNodeKind.CancelTask: return new[] { new AlgorithmPort("accepted", signal) };
                case AlgorithmNodeKind.Hysteresis: return new[] { new AlgorithmPort("value", AlgorithmType.Of(AlgorithmValueKind.Boolean)), new AlgorithmPort("event", signal) };
                case AlgorithmNodeKind.Input: return new[] { new AlgorithmPort("value", node.ValueType), new AlgorithmPort("sampled", signal), new AlgorithmPort("valid", AlgorithmType.Of(AlgorithmValueKind.Boolean)) };
                case AlgorithmNodeKind.Compare:
                case AlgorithmNodeKind.Boolean: return new[] { new AlgorithmPort("value", AlgorithmType.Of(AlgorithmValueKind.Boolean)) };
                case AlgorithmNodeKind.Log:
                case AlgorithmNodeKind.SetVariable: return Array.Empty<AlgorithmPort>();
                case AlgorithmNodeKind.Cargo: return new[] { new AlgorithmPort("capacity", AlgorithmType.Of(AlgorithmValueKind.Number)), new AlgorithmPort("remaining", AlgorithmType.Of(AlgorithmValueKind.Number)), new AlgorithmPort("amount", AlgorithmType.Of(AlgorithmValueKind.Number)), new AlgorithmPort("has_item", AlgorithmType.Of(AlgorithmValueKind.Boolean)), new AlgorithmPort("changed", signal) };
                default: return new[] { new AlgorithmPort("value", node.ValueType) };
            }
        }
        private static AlgorithmPort[] EffectorOutputs()
        {
            var signal = AlgorithmType.Of(AlgorithmValueKind.Event);
            return new[] { new AlgorithmPort("accepted", signal), new AlgorithmPort("started", signal), new AlgorithmPort("completed", signal),
                new AlgorithmPort("failed", signal), new AlgorithmPort("cancelled", signal), new AlgorithmPort("preempted", signal),
                new AlgorithmPort("targetInvalid", signal), new AlgorithmPort("rejected", signal), new AlgorithmPort("partial", signal) };
        }
        public static bool Compatible(AlgorithmType source, AlgorithmType target)
        {
            if (source == null || target == null || source.Kind != target.Kind) return false;
            if (source.Kind == AlgorithmValueKind.Number && source.Unit != target.Unit) return false;
            if (source.Kind == AlgorithmValueKind.Enumeration && source.EnumFamily != target.EnumFamily) return false;
            if (source.Kind == AlgorithmValueKind.Object)
            {
                if (!string.IsNullOrEmpty(target.ObjectCategory) && source.ObjectCategory != target.ObjectCategory) return false;
                if (target.Capabilities == null || source.Capabilities == null) return false;
                foreach (var capability in target.Capabilities) if (Array.IndexOf(source.Capabilities, capability) < 0) return false;
            }
            return true;
        }
    }
}
