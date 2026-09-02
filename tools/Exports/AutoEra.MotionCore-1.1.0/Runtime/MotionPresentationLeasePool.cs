using System.Collections.Generic;

namespace AutoEra.Motion
{
    public sealed class MotionPresentationLease
    {
        public string PrototypeId { get; private set; }
        public int PlaybackPass { get; private set; }
        public float NormalizedProgress { get; private set; }
        public MotionPresentationUpdateLevel UpdateLevel { get; private set; }
        public bool IsActive { get; private set; }

        internal void Begin(string prototypeId)
        {
            PrototypeId = prototypeId;
            PlaybackPass++;
            NormalizedProgress = 0f;
            UpdateLevel = MotionPresentationUpdateLevel.Near;
            IsActive = true;
        }

        public void SetPresentation(float normalizedProgress, MotionPresentationUpdateLevel updateLevel)
        {
            NormalizedProgress = UnityEngine.Mathf.Clamp01(normalizedProgress);
            UpdateLevel = updateLevel;
        }

        internal void ResetForPool()
        {
            PrototypeId = null;
            NormalizedProgress = 0f;
            UpdateLevel = MotionPresentationUpdateLevel.Near;
            IsActive = false;
        }
    }

    public sealed class MotionPresentationLeasePool
    {
        private readonly Stack<MotionPresentationLease> _available = new Stack<MotionPresentationLease>();

        public MotionPresentationLease Acquire(string prototypeId)
        {
            MotionPresentationLease lease = _available.Count > 0 ? _available.Pop() : new MotionPresentationLease();
            lease.Begin(prototypeId);
            return lease;
        }

        public void Release(MotionPresentationLease lease)
        {
            if (lease == null || !lease.IsActive)
                return;

            lease.ResetForPool();
            _available.Push(lease);
        }
    }
}
