using System;
using System.Collections.Generic;
using UnityEngine;

namespace AutoEra.Motion
{
    public enum MotionExecutionState { Prepared, Running, Completed, Cancelled, Recovering }

    /// <summary>One runtime owner per rig for lifecycle and exclusive joint-channel arbitration.</summary>
    public sealed class MotionExecutor : MonoBehaviour
    {
        [SerializeField] private MotionRig _rig;
        private readonly Dictionary<string, string> _jointOwners = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, MotionExecutionState> _states = new Dictionary<string, MotionExecutionState>(StringComparer.Ordinal);

        public MotionRig Rig => _rig;
        public void Configure(MotionRig rig) { _rig = rig; }

        public bool TryPrepare(string executionId, IReadOnlyList<string> jointIds)
        {
            if (_rig == null || string.IsNullOrWhiteSpace(executionId) || jointIds == null || _states.ContainsKey(executionId)) return false;
            for (int i = 0; i < jointIds.Count; i++)
            {
                if (!_rig.TryGetBinding(jointIds[i], out _) || (_jointOwners.TryGetValue(jointIds[i], out string owner) && owner != executionId)) return false;
            }
            for (int i = 0; i < jointIds.Count; i++) _jointOwners[jointIds[i]] = executionId;
            _states.Add(executionId, MotionExecutionState.Prepared);
            return true;
        }

        public bool TryTransition(string executionId, MotionExecutionState expected, MotionExecutionState next)
        {
            if (!_states.TryGetValue(executionId, out MotionExecutionState current) || current != expected) return false;
            _states[executionId] = next;
            return true;
        }

        public bool TryGetState(string executionId, out MotionExecutionState state) => _states.TryGetValue(executionId, out state);

        public bool TryRelease(string executionId)
        {
            if (!_states.ContainsKey(executionId)) return false;
            var releases = new List<string>();
            foreach (KeyValuePair<string, string> owner in _jointOwners) if (owner.Value == executionId) releases.Add(owner.Key);
            foreach (string jointId in releases) _jointOwners.Remove(jointId);
            _states.Remove(executionId);
            return true;
        }
    }
}
