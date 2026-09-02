using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Motion
{
    public enum MotionJointChannel
    {
        Rotation,
        Translation
    }

    /// <summary>
    /// Static, explicit rig binding used by Motion Core. It contains no graph, gameplay state,
    /// or object lookup; the executor will later own mutable motion state separately.
    /// </summary>
    public sealed class MotionRig : MonoBehaviour
    {
        [SerializeField] private MotionJointBinding[] _jointBindings = Array.Empty<MotionJointBinding>();

        private readonly Dictionary<string, MotionJointBinding> _bindingsByStableId = new Dictionary<string, MotionJointBinding>(StringComparer.Ordinal);

        public IReadOnlyList<MotionJointBinding> JointBindings => _jointBindings;

        public void Configure(MotionJointBinding[] jointBindings)
        {
            _jointBindings = jointBindings ?? Array.Empty<MotionJointBinding>();
            RebuildLookup();
        }

        public bool TryGetBinding(string stableId, out MotionJointBinding binding)
        {
            EnsureLookup();
            return _bindingsByStableId.TryGetValue(stableId, out binding);
        }

        public bool TryValidate(out string error)
        {
            _bindingsByStableId.Clear();
            foreach (MotionJointBinding binding in _jointBindings)
            {
                if (binding == null)
                {
                    error = "MotionRig contains a null joint binding.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(binding.StableId))
                {
                    error = "MotionRig joint binding requires a stable ID.";
                    return false;
                }

                if (binding.JointTransform == null)
                {
                    error = "MotionRig joint binding is missing Transform: " + binding.StableId;
                    return false;
                }

                if (binding.LocalAxis.sqrMagnitude < 0.000001f)
                {
                    error = "MotionRig joint binding has a zero local axis: " + binding.StableId;
                    return false;
                }

                if (binding.MinimumValue > binding.MaximumValue)
                {
                    error = "MotionRig joint binding has an invalid range: " + binding.StableId;
                    return false;
                }

                if (_bindingsByStableId.ContainsKey(binding.StableId))
                {
                    error = "MotionRig contains a duplicate stable ID: " + binding.StableId;
                    return false;
                }

                _bindingsByStableId.Add(binding.StableId, binding);
            }

            error = null;
            return true;
        }

        private void OnValidate()
        {
            if (!TryValidate(out string error))
            {
                Debug.LogWarning("[AutoEra.Motion] MotionRig Validation Failed: " + error, this);
            }
        }

        private void EnsureLookup()
        {
            if (_bindingsByStableId.Count != _jointBindings.Length)
            {
                RebuildLookup();
            }
        }

        private void RebuildLookup()
        {
            TryValidate(out _);
        }
    }

    [Serializable]
    public sealed class MotionJointBinding
    {
        [SerializeField] private string _stableId;
        [SerializeField] private Transform _jointTransform;
        [SerializeField] private MotionJointChannel _channel;
        [SerializeField] private Vector3 _localAxis = Vector3.up;
        [SerializeField] private float _minimumValue;
        [SerializeField] private float _maximumValue;
        [SerializeField] private Vector3 _bindLocalPosition;
        [SerializeField] private Vector3 _bindLocalEulerDegrees;
        [SerializeField] private Vector3 _safeLocalPosition;
        [SerializeField] private Vector3 _safeLocalEulerDegrees;

        public string StableId => _stableId;
        public Transform JointTransform => _jointTransform;
        public MotionJointChannel Channel => _channel;
        public Vector3 LocalAxis => _localAxis;
        public float MinimumValue => _minimumValue;
        public float MaximumValue => _maximumValue;
        public Vector3 BindLocalPosition => _bindLocalPosition;
        public Quaternion BindLocalRotation => Quaternion.Euler(_bindLocalEulerDegrees);
        public Vector3 SafeLocalPosition => _safeLocalPosition;
        public Quaternion SafeLocalRotation => Quaternion.Euler(_safeLocalEulerDegrees);

        public MotionJointBinding(
            string stableId,
            Transform jointTransform,
            MotionJointChannel channel,
            Vector3 localAxis,
            float minimumValue,
            float maximumValue,
            Vector3 bindLocalPosition,
            Vector3 bindLocalEulerDegrees,
            Vector3 safeLocalPosition,
            Vector3 safeLocalEulerDegrees)
        {
            _stableId = stableId;
            _jointTransform = jointTransform;
            _channel = channel;
            _localAxis = localAxis;
            _minimumValue = minimumValue;
            _maximumValue = maximumValue;
            _bindLocalPosition = bindLocalPosition;
            _bindLocalEulerDegrees = bindLocalEulerDegrees;
            _safeLocalPosition = safeLocalPosition;
            _safeLocalEulerDegrees = safeLocalEulerDegrees;
        }
    }
}
