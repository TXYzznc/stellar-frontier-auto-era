using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Motion
{
    public interface IMotionTargetPoseProvider { bool TryGetTargetPose(string targetId, out Pose pose); }
    public interface IMotionEnvironmentInputProvider { bool TryGetScalar(string inputId, out float value); }

    public sealed class MotionParameterContext
    {
        private readonly Dictionary<string, bool> _booleans = new Dictionary<string, bool>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> _integers = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, float> _floats = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly Dictionary<string, Vector3> _vectors = new Dictionary<string, Vector3>(StringComparer.Ordinal);
        private readonly Dictionary<string, Quaternion> _rotations = new Dictionary<string, Quaternion>(StringComparer.Ordinal);
        public void SetBoolean(string id, bool value) => _booleans[id] = value;
        public void SetInteger(string id, int value) => _integers[id] = value;
        public void SetFloat(string id, float value) => _floats[id] = value;
        public void SetVector3(string id, Vector3 value) => _vectors[id] = value;
        public void SetQuaternion(string id, Quaternion value) => _rotations[id] = value;
        public bool TryGetBoolean(string id, out bool value) => _booleans.TryGetValue(id, out value);
        public bool TryGetInteger(string id, out int value) => _integers.TryGetValue(id, out value);
        public bool TryGetFloat(string id, out float value) => _floats.TryGetValue(id, out value);
    }
}
