using System;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using UnityEngine;

namespace AutoEra.Machines
{
    public enum MachineNavigationState { Idle, WaitingWork, WaitingCompute, Paused, Planning, Moving, Aligning, Finished }
    public enum NavigationAdmission { Accepted, Busy, Immobile, Invalid }

    [Serializable]
    public sealed class MachineNavigationSettings
    {
        public float Speed = 3f;
        public float Acceleration = 12f;
        public float AngularSpeed = 180f;
        public float ArrivalDistance = 0.08f;
        public float StoppedSpeed = 0.02f;
        public float FacingTolerance = 1f;
        public float EffectiveDisplacement = 0.02f;
        public float ReplanSeconds = 3f;
        public float WarningSeconds = 10f;
        public float FailureSeconds = 30f;
        public float WorkWaitPromptSeconds = 30f;
        public int PlanningCompute = 10;
        public int MovingCompute = 5;

        public void Validate()
        {
            if (!Positive(Speed) || !Positive(Acceleration) || !Positive(AngularSpeed) ||
                !Positive(ArrivalDistance) || !Positive(StoppedSpeed) || !Positive(FacingTolerance) ||
                !Positive(EffectiveDisplacement) || !Positive(ReplanSeconds) || !Positive(WarningSeconds) ||
                !Positive(FailureSeconds) || !Positive(WorkWaitPromptSeconds) ||
                WarningSeconds < ReplanSeconds || FailureSeconds <= WarningSeconds || PlanningCompute <= 0 || MovingCompute <= 0)
                throw new ArgumentException("Invalid navigation configuration.");
        }
        internal static bool Positive(float value) => value > 0 && !float.IsInfinity(value);
        internal static bool Finite(Vector3 value) => !float.IsNaN(value.x + value.y + value.z) &&
            !float.IsInfinity(value.x) && !float.IsInfinity(value.y) && !float.IsInfinity(value.z);
    }

    /// <summary>Driver reports actual pose. TryPlan prepares a stopped route; only Resume starts movement.</summary>
    public interface IMachineNavigationDriver
    {
        Vector3 Position { get; }
        float Yaw { get; }
        float Speed { get; }
        bool PathValid { get; }
        bool TryPlan(MachineNavigationTarget target, MachineInstance machine, out Vector3 destination);
        void Resume();
        void Stop();
        void Align(float yaw, float degreesPerSecond, float deltaSeconds);
    }

    public sealed class MachineNavigationTarget
    {
        public InitialRegion Region { get; }
        public PersistentId ObjectId { get; }
        public RegionWorkQueue WorkQueue { get; }
        public Vector3[] Candidates { get; }
        public float? FacingYaw { get; }
        public Vector3? WaitingPosition { get; }
        private readonly Rect? _area;
        private readonly Func<MachineInstance, Vector3, bool> _envelope;
        private readonly Vector2 _maximumSize;

        public MachineNavigationTarget(InitialRegion region, Vector3 coordinate, float? facingYaw = null)
        {
            if (region == null || !MachineNavigationSettings.Finite(coordinate) ||
                (facingYaw.HasValue && (float.IsNaN(facingYaw.Value) || float.IsInfinity(facingYaw.Value))))
                throw new ArgumentException("Invalid navigation coordinate.");
            Region = region; Candidates = new[] { coordinate }; FacingYaw = facingYaw;
        }

        /// <param name="envelope">Public ability/installed-component compatibility, evaluated again after NavMesh sampling.</param>
        public MachineNavigationTarget(InitialRegion region, PersistentId target, RegionWorkQueue queue,
            Rect approachArea, float groundHeight, Vector2 maximumSize,
            Func<MachineInstance, Vector3, bool> envelope, float? facingYaw = null, Vector3? waitingPosition = null)
        {
            if (region == null || queue == null || !queue.BelongsTo(region, target) || envelope == null ||
                !MachineNavigationSettings.Positive(approachArea.width) || !MachineNavigationSettings.Positive(approachArea.height) ||
                !MachineNavigationSettings.Finite(new Vector3(approachArea.x, groundHeight, approachArea.y)) ||
                !MachineNavigationSettings.Positive(maximumSize.x) || !MachineNavigationSettings.Positive(maximumSize.y) ||
                (facingYaw.HasValue && (float.IsNaN(facingYaw.Value) || float.IsInfinity(facingYaw.Value))) ||
                (waitingPosition.HasValue && !MachineNavigationSettings.Finite(waitingPosition.Value)))
                throw new ArgumentException("Invalid public work approach.");
            Region = region; ObjectId = target; WorkQueue = queue; _area = approachArea;
            _envelope = envelope; _maximumSize = maximumSize; FacingYaw = facingYaw;
            WaitingPosition = waitingPosition;
            // Finite deterministic candidates, not a new player-authored route language.
            Candidates = new Vector3[9];
            int i = 0;
            for (int z = 0; z < 3; z++) for (int x = 0; x < 3; x++)
                Candidates[i++] = new Vector3(approachArea.x + approachArea.width * (x + 1) / 4f,
                    groundHeight, approachArea.y + approachArea.height * (z + 1) / 4f);
        }

        public bool IsValid => Region.IsActive && (!ObjectId.IsValid || Region.TryGet(ObjectId, out _));
        public bool Allows(MachineInstance machine, Vector3 position)
        {
            if (!IsValid || !MachineNavigationSettings.Finite(position) || !Region.TryGet(machine.Id, out var body)) return false;
            if (_area.HasValue && (!_area.Value.Contains(new Vector2(position.x, position.z)) ||
                body.Size.x > _maximumSize.x || body.Size.y > _maximumSize.y || !_envelope(machine, position))) return false;
            return Region.CanNavigate(machine.Id, new Vector2(position.x, position.z), FacingYaw ?? body.Yaw);
        }
    }
}
