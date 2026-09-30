using System;
using System.Collections.Generic;

namespace AutoEra.Algorithms
{
    public enum AlgorithmValueKind { Boolean, Number, Enumeration, Object, Objects, Position, Event, Communication, SoilGrid, CropGrid, TreeGrid }
    public enum AlgorithmNodeKind { Constant, Parameter, Input, Startup, Arithmetic, Compare, Boolean, Branch, Merge, Variable, SetVariable, Delay, Navigate, Effector, SubmitTask, QueryTask, CancelTask, Hysteresis, Log, Cargo }
    public enum AlgorithmOperator { Add, Subtract, Multiply, Divide, Minimum, Maximum, Equal, NotEqual, Less, Greater, LessOrEqual, GreaterOrEqual, And, Or, Not }
    public enum AlgorithmEffectorAction { Spray, ModifySpray, StopSpray, Sow, Harvest, Clean, Transfer, Cut, Drill }

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
        public string Field = "resource";
        public AlgorithmEffectorAction Action;
        public bool Deleted;
        public float LayoutX, LayoutY;
        public AlgorithmNode Copy() => new AlgorithmNode { Id = Id, Kind = Kind, Operator = Operator, ValueType = ValueType?.Copy(),
            Default = Default?.Copy(), BindingKey = BindingKey, StateKey = StateKey, Field = Field, Action = Action, Deleted = Deleted, LayoutX = LayoutX, LayoutY = LayoutY };
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

        /// <summary>
        /// 删除草稿节点并**级联移除所有引用该节点的边**（图编辑器语义：不留悬空边）。
        /// 节点与其关联边在同一次修订变更中移除；节点不存在或已删除时返回 false 且文档不变。
        /// </summary>
        public bool RemoveNode(ulong id)
        {
            if (Nodes == null || Edges == null)
            {
                return false;
            }

            bool removed = false;
            foreach (var node in Nodes)
            {
                if (node != null && node.Id == id && !node.Deleted)
                {
                    node.Deleted = true;
                    removed = true;
                    break;
                }
            }

            if (!removed)
            {
                return false;
            }

            for (int i = Edges.Count - 1; i >= 0; i--)
            {
                AlgorithmEdge edge = Edges[i];
                if (edge != null && (edge.From == id || edge.To == id))
                {
                    Edges.RemoveAt(i);
                }
            }

            Revision++;
            return true;
        }

        /// <summary>移动节点画布坐标（画布自由布局）。未找到或已删除的节点返回 false。</summary>
        public bool MoveNode(ulong id, float x, float y)
        {
            foreach (var node in Nodes) if (node.Id == id && !node.Deleted) { node.LayoutX = x; node.LayoutY = y; Revision++; return true; }
            return false;
        }

        /// <summary>向草稿添加带稳定身份的节点；身份重复或节点结构无效时不修改文档。</summary>
        public bool CreateNode(AlgorithmNode node)
        {
            if (node == null || node.Id == 0 || node.ValueType == null || Nodes == null || !Enum.IsDefined(typeof(AlgorithmNodeKind), node.Kind))
            {
                return false;
            }

            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes[i] != null && Nodes[i].Id == node.Id)
                {
                    return false;
                }
            }

            Nodes.Add(node.Copy());
            Revision++;
            return true;
        }

        /// <summary>连接一对存在且强类型兼容的端口。一个目标输入最多接受一条边。</summary>
        public bool Connect(ulong from, string output, ulong to, string input)
        {
            if (Nodes == null || Edges == null || string.IsNullOrEmpty(output) || string.IsNullOrEmpty(input))
            {
                return false;
            }

            AlgorithmNode source = FindNode(from);
            AlgorithmNode target = FindNode(to);
            if (source == null || target == null)
            {
                return false;
            }

            AlgorithmPort sourcePort = FindPort(AlgorithmCatalog.Outputs(source), output);
            AlgorithmPort targetPort = FindPort(AlgorithmCatalog.Inputs(target), input);
            if (sourcePort == null || targetPort == null || !AlgorithmCatalog.Compatible(sourcePort.Type, targetPort.Type))
            {
                return false;
            }

            for (int i = 0; i < Edges.Count; i++)
            {
                AlgorithmEdge edge = Edges[i];
                if (edge != null && edge.To == to && edge.Input == input)
                {
                    return false;
                }
            }

            Edges.Add(new AlgorithmEdge { From = from, Output = output, To = to, Input = input });
            Revision++;
            return true;
        }

        /// <summary>按完整边身份只断开一条边；不存在时不修改文档。</summary>
        public bool Disconnect(ulong from, string output, ulong to, string input)
        {
            if (Edges == null)
            {
                return false;
            }

            for (int i = 0; i < Edges.Count; i++)
            {
                AlgorithmEdge edge = Edges[i];
                if (edge != null && edge.From == from && edge.Output == output && edge.To == to && edge.Input == input)
                {
                    Edges.RemoveAt(i);
                    Revision++;
                    return true;
                }
            }

            return false;
        }

        private AlgorithmNode FindNode(ulong id)
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                AlgorithmNode node = Nodes[i];
                if (node != null && node.Id == id && !node.Deleted)
                {
                    return node;
                }
            }

            return null;
        }

        private static AlgorithmPort FindPort(AlgorithmPort[] ports, string key)
        {
            if (ports == null)
            {
                return null;
            }

            for (int i = 0; i < ports.Length; i++)
            {
                if (ports[i] != null && ports[i].Key == key)
                {
                    return ports[i];
                }
            }

            return null;
        }
    }
}
