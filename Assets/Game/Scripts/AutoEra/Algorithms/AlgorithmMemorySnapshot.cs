using System.Collections.Generic;

namespace AutoEra.Algorithms
{
    /// <summary>In-memory checkpoint only. Capturing physical commands in flight is forbidden.</summary>
    public sealed class AlgorithmMemorySnapshot
    {
        internal ulong Instance, Generation;
        internal AlgorithmDocument Applied;
        internal Dictionary<string, AlgorithmValue> State;
        internal readonly List<AlgorithmTrigger> Events = new List<AlgorithmTrigger>();
        internal readonly List<AlgorithmTrigger> Timers = new List<AlgorithmTrigger>();
        internal readonly List<long> Remaining = new List<long>();
        internal bool Invalid;
        internal AlgorithmPauseReason PauseReasons;
        internal string Reason;
        internal AlgorithmRunRecord[] History;
        public ulong InstanceId => Instance;
        public ulong AppliedRevision => Applied.Revision;
    }
}
