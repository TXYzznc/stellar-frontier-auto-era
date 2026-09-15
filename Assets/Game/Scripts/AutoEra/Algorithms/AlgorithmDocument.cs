using System;
using System.Collections.Generic;

namespace AutoEra.Algorithms
{
    public enum AlgorithmValueKind { Boolean, Number, Enumeration, Object, Objects, Position, Event, Communication, SoilGrid, CropGrid, TreeGrid }
    public enum AlgorithmNodeKind { Constant, Parameter, Input, Startup, Arithmetic, Compare, Boolean, Branch, Merge, Variable, SetVariable, Delay, Navigate, Log }
    public enum AlgorithmOperator { Add, Subtract, Multiply, Divide, Minimum, Maximum, Equal, NotEqual, Less, Greater, And, Or, Not }

    [Serializable]
    public sealed class AlgorithmType
    {
        public AlgorithmValueKind Kind;
        public string Unit = "";
        public string EnumFamily = "";
        public string ObjectCategory = "";
        public string[] Capabilities = Array.Empty<string>();
        public AlgorithmType Copy() => new AlgorithmType { Kind = Kind, Unit = Unit, EnumFamily = EnumFamily,
            ObjectCategory = ObjectCategory, Capabilities = Capabilities == null ? null : (string[])Capabilities.Clone() };
        public static AlgorithmType Of(AlgorithmValueKind kind, string unit = "") => new AlgorithmType { Kind = kind, Unit = unit };
    }

    /// <summary>Closed typed payload. No user supplied object/dictionary or executable expression.</summary>
    [Serializable]
    public sealed class AlgorithmValue
    {
        public AlgorithmType Type = AlgorithmType.Of(AlgorithmValueKind.Number);
        public double Number;
        public bool Boolean;
        public int EnumValue;
        public ulong ObjectId;
        public double X, Y, Z;
        public bool IsValid = true;
        public AlgorithmValue Copy() => new AlgorithmValue { Type = Type?.Copy(), Number = Number, Boolean = Boolean,
            EnumValue = EnumValue, ObjectId = ObjectId, X = X, Y = Y, Z = Z, IsValid = IsValid };
        public static AlgorithmValue Numeric(double value, string unit = "") => new AlgorithmValue { Number = value, Type = AlgorithmType.Of(AlgorithmValueKind.Number, unit) };
        public static AlgorithmValue Bool(bool value) => new AlgorithmValue { Boolean = value, Type = AlgorithmType.Of(AlgorithmValueKind.Boolean) };
    }

    [Serializable]
    public sealed class AlgorithmNode
    {
        public ulong Id;
        public AlgorithmNodeKind Kind;
        public AlgorithmOperator Operator;
        public AlgorithmType ValueType = AlgorithmType.Of(AlgorithmValueKind.Number);
        public AlgorithmValue Default;
        public string BindingKey = "";
        public string StateKey = "";
        public bool Deleted;
        public float LayoutX, LayoutY;
        public AlgorithmNode Copy() => new AlgorithmNode { Id = Id, Kind = Kind, Operator = Operator, ValueType = ValueType?.Copy(),
            Default = Default?.Copy(), BindingKey = BindingKey, StateKey = StateKey, Deleted = Deleted, LayoutX = LayoutX, LayoutY = LayoutY };
    }

    [Serializable]
    public sealed class AlgorithmEdge
    {
        public ulong From, To;
        public string Output = "value", Input = "value";
        public AlgorithmEdge Copy() => new AlgorithmEdge { From = From, To = To, Output = Output, Input = Input };
    }

    [Serializable]
    public sealed class AlgorithmBinding
    {
        public string Key;
        public ulong ComponentId, TargetId;
        public ulong Generation;
        public AlgorithmType Type;
        public bool Available = true;
        public AlgorithmBinding Copy() => new AlgorithmBinding { Key = Key, ComponentId = ComponentId, TargetId = TargetId,
            Generation = Generation, Type = Type?.Copy(), Available = Available };
    }

    [Serializable]
    public sealed class AlgorithmDocument
    {
        public int SchemaVersion = 1;
        public string LanguageVersion = "1";
        public ulong DocumentId;
        public ulong Revision = 1;
        public List<AlgorithmNode> Nodes = new List<AlgorithmNode>();
        public List<AlgorithmEdge> Edges = new List<AlgorithmEdge>();
        public List<AlgorithmBinding> Bindings = new List<AlgorithmBinding>();
        public AlgorithmDocument Copy()
        {
            var result = new AlgorithmDocument { SchemaVersion = SchemaVersion, LanguageVersion = LanguageVersion, DocumentId = DocumentId, Revision = Revision };
            foreach (var n in Nodes) result.Nodes.Add(n?.Copy());
            foreach (var e in Edges) result.Edges.Add(e?.Copy());
            foreach (var b in Bindings) result.Bindings.Add(b?.Copy());
            return result;
        }
        public bool DeleteNode(ulong id)
        {
            foreach (var node in Nodes) if (node.Id == id && !node.Deleted) { node.Deleted = true; Revision++; return true; }
            return false;
        }
    }
}
