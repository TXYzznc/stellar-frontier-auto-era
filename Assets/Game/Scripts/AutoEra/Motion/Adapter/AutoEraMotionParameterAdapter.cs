using UnityEngine;

namespace AutoEra.Motion.Adapter
{
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
    }
}
