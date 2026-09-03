using UnityEngine;

namespace AutoEra.Motion.Adapter
{
    public enum AutoEraMotionPresentationStatus { Running, Completed, Cancelled }
    public enum AutoEraMotionActionType { None, CarrierPosition, CarrierYaw, ArmManipulate, WaterSpray, SawCut, DrillMine, DoorMove, ConveyorMove }
    public enum AutoEraMotionActionPhase { Accepted, Executing, SafeClosure, Terminal }

    /// <summary>Read-only presentation projection of a task system instruction; it is not a queue entry or scheduler command.</summary>
    public readonly struct AutoEraMotionInstructionProjection
    {
        public AutoEraMotionInstructionProjection(
            string instructionId,
            string parentTaskId,
            AutoEraMotionActionType actionType,
            AutoEraMotionActionPhase phase,
            bool interrupted,
            float normalizedProgress,
            int efficiencyTier,
            Pose targetPose)
        {
            InstructionId = instructionId;
            ParentTaskId = parentTaskId;
            ActionType = actionType;
            Phase = phase;
            Interrupted = interrupted;
            NormalizedProgress = normalizedProgress;
            EfficiencyTier = efficiencyTier;
            TargetPose = targetPose;
        }

        public string InstructionId { get; }
        public string ParentTaskId { get; }
        public AutoEraMotionActionType ActionType { get; }
        public AutoEraMotionActionPhase Phase { get; }
        public bool Interrupted { get; }
        public float NormalizedProgress { get; }
        public int EfficiencyTier { get; }
        public Pose TargetPose { get; }
    }

    public readonly struct AutoEraMotionPresentationSnapshot
    {
        public AutoEraMotionPresentationSnapshot(
            bool interrupted,
            float normalizedProgress,
            int efficiencyTier,
            Pose targetPose,
            AutoEraMotionPresentationStatus status)
        {
            Interrupted = interrupted;
            NormalizedProgress = normalizedProgress;
            EfficiencyTier = efficiencyTier;
            TargetPose = targetPose;
            Status = status;
        }

        public bool Interrupted { get; }
        public float NormalizedProgress { get; }
        public int EfficiencyTier { get; }
        public Pose TargetPose { get; }
        public AutoEraMotionPresentationStatus Status { get; }
    }

    /// <summary>Product boundary: translates authoritative presentation inputs to typed parameters only.</summary>
    public sealed class AutoEraMotionParameterAdapter
    {
        private readonly MotionParameterContext _context;
        public AutoEraMotionParameterAdapter(MotionParameterContext context) { _context = context; }
        public void ApplyPresentationState(bool interrupted, float normalizedProgress, int efficiencyTier, Pose targetPose)
        {
            _context.SetBoolean("interrupted", interrupted);
            _context.SetFloat("normalizedProgress", Mathf.Clamp01(normalizedProgress));
            _context.SetInteger("efficiencyTier", efficiencyTier);
            _context.SetVector3("targetPosition", targetPose.position);
            _context.SetQuaternion("targetRotation", targetPose.rotation);
        }

        public void ApplyAuthoritativePresentationSnapshot(AutoEraMotionPresentationSnapshot snapshot)
        {
            ApplyPresentationState(snapshot.Interrupted, snapshot.NormalizedProgress, snapshot.EfficiencyTier, snapshot.TargetPose);
            _context.SetBoolean("presentationCompleted", snapshot.Status == AutoEraMotionPresentationStatus.Completed);
            _context.SetBoolean("presentationCancelled", snapshot.Status == AutoEraMotionPresentationStatus.Cancelled);
        }

        public void ApplyCurrentInstructionProjection(AutoEraMotionInstructionProjection projection)
        {
            ApplyPresentationState(
                projection.Interrupted,
                projection.NormalizedProgress,
                projection.EfficiencyTier,
                projection.TargetPose);
            _context.SetInteger("activeActionType", (int)projection.ActionType);
            _context.SetInteger("activeActionPhase", (int)projection.Phase);
            _context.SetBoolean("presentationCompleted", projection.Phase == AutoEraMotionActionPhase.Terminal && !projection.Interrupted);
            _context.SetBoolean("presentationCancelled", projection.Phase == AutoEraMotionActionPhase.Terminal && projection.Interrupted);
        }
    }
}
