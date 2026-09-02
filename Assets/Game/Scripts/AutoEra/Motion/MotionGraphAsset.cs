using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Motion
{
    public enum MotionParameterType { Boolean, Integer, Float, Vector3, Quaternion }
    public enum MotionNodeKind { Rotate, Translate, Aim, OpenClose, ContinuousRotate, Oscillate, Wait, Sequence, Parallel, Loop, ConditionalWait, Branch }

    [CreateAssetMenu(menuName = "AutoEra/Motion/Motion Graph", fileName = "MotionGraph")]
    public sealed class MotionGraphAsset : ScriptableObject
    {
        [SerializeField] private int _schemaVersion = 1;
        [SerializeField] private string _graphId;
        [SerializeField] private string _graphVersion = "1.0.0";
        [SerializeField] private MotionParameterDefinition[] _parameters = Array.Empty<MotionParameterDefinition>();
        [SerializeField] private MotionNodeDefinition[] _nodes = Array.Empty<MotionNodeDefinition>();
        [SerializeField] private MotionConnectionDefinition[] _connections = Array.Empty<MotionConnectionDefinition>();

        public int SchemaVersion => _schemaVersion;
        public string GraphId => _graphId;
        public string GraphVersion => _graphVersion;
        public IReadOnlyList<MotionParameterDefinition> Parameters => _parameters;
        public IReadOnlyList<MotionNodeDefinition> Nodes => _nodes;
        public IReadOnlyList<MotionConnectionDefinition> Connections => _connections;

        public void Configure(int schemaVersion, string graphId, string graphVersion, MotionParameterDefinition[] parameters, MotionNodeDefinition[] nodes, MotionConnectionDefinition[] connections)
        {
            _schemaVersion = schemaVersion;
            _graphId = graphId;
            _graphVersion = graphVersion;
            _parameters = parameters ?? Array.Empty<MotionParameterDefinition>();
            _nodes = nodes ?? Array.Empty<MotionNodeDefinition>();
            _connections = connections ?? Array.Empty<MotionConnectionDefinition>();
        }

        public bool TryValidate(out string error)
        {
            if (_schemaVersion != 1 || string.IsNullOrWhiteSpace(_graphId) || string.IsNullOrWhiteSpace(_graphVersion))
            { error = "MotionGraphAsset requires schema version 1, graph ID, and graph version."; return false; }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (MotionParameterDefinition parameter in _parameters)
            {
                if (parameter == null || string.IsNullOrWhiteSpace(parameter.StableId) || !ids.Add(parameter.StableId))
                { error = "MotionGraphAsset has an invalid or duplicate parameter ID."; return false; }
            }
            ids.Clear();
            foreach (MotionNodeDefinition node in _nodes)
            {
                if (node == null || string.IsNullOrWhiteSpace(node.StableId) || !ids.Add(node.StableId))
                { error = "MotionGraphAsset has an invalid or duplicate node ID."; return false; }
            }
            foreach (MotionConnectionDefinition connection in _connections)
            {
                if (connection == null || !ids.Contains(connection.FromNodeId) || !ids.Contains(connection.ToNodeId) || connection.FromNodeId == connection.ToNodeId)
                { error = "MotionGraphAsset has an invalid connection."; return false; }
            }
            error = null;
            return true;
        }
    }

    [Serializable]
    public sealed class MotionParameterDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private MotionParameterType _type;
        public string StableId => _stableId;
        public MotionParameterType Type => _type;
        public MotionParameterDefinition(string stableId, MotionParameterType type) { _stableId = stableId; _type = type; }
    }

    [Serializable]
    public sealed class MotionNodeDefinition
    {
        [SerializeField] private string _stableId;
        [SerializeField] private MotionNodeKind _kind;
        [SerializeField] private string _targetJointId;
        [SerializeField] private string _parameterId;
        public string StableId => _stableId;
        public MotionNodeKind Kind => _kind;
        public string TargetJointId => _targetJointId;
        public string ParameterId => _parameterId;
        public MotionNodeDefinition(string stableId, MotionNodeKind kind, string targetJointId, string parameterId) { _stableId = stableId; _kind = kind; _targetJointId = targetJointId; _parameterId = parameterId; }
    }

    [Serializable]
    public sealed class MotionConnectionDefinition
    {
        [SerializeField] private string _fromNodeId;
        [SerializeField] private string _toNodeId;
        public string FromNodeId => _fromNodeId;
        public string ToNodeId => _toNodeId;
        public MotionConnectionDefinition(string fromNodeId, string toNodeId) { _fromNodeId = fromNodeId; _toNodeId = toNodeId; }
    }
}
