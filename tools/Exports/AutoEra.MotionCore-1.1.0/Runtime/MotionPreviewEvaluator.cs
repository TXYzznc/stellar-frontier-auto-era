using UnityEngine;

namespace AutoEra.Motion
{
    /// <summary>Optional presentation-only participant driven by the editor motion-preview timeline.</summary>
    public interface IMotionPreviewPresentation
    {
        void ApplyPreviewTime(float elapsedSeconds);
        void RestorePreviewBindPose();
    }

    /// <summary>Editor and test preview evaluator. It drives only declared rig bindings and owns no gameplay state.</summary>
    public static class MotionPreviewEvaluator
    {
        public static bool Apply(MotionGraphAsset graph, MotionRig rig, float progress, float elapsedSeconds)
        {
            return Apply(graph, rig, progress, elapsedSeconds, new System.Collections.Generic.HashSet<MotionGraphAsset>());
        }

        private static bool Apply(MotionGraphAsset graph, MotionRig rig, float progress, float elapsedSeconds, System.Collections.Generic.HashSet<MotionGraphAsset> visited)
        {
            if (graph == null || rig == null || !graph.IsCompatibleWith(rig) || !visited.Add(graph)) return false;
            foreach (MotionNodeDefinition node in graph.Nodes)
            {
                if (!MotionGraphComposition.IsPrimitiveNode(node.Kind) || string.IsNullOrWhiteSpace(node.TargetJointId)) continue;
                if (!rig.TryGetBinding(node.TargetJointId, out MotionJointBinding binding)) return false;
                if (node.Kind == MotionNodeKind.Translate || node.Kind == MotionNodeKind.OpenClose)
                    binding.JointTransform.localPosition = MotionPrimitives.Translate(binding.BindLocalPosition, binding.LocalAxis, binding.MinimumValue, binding.MaximumValue, progress);
                else if (node.Kind == MotionNodeKind.ContinuousRotate)
                    binding.JointTransform.localRotation = MotionPrimitives.ContinuousRotate(binding.BindLocalRotation, binding.LocalAxis, binding.MaximumValue, elapsedSeconds);
                else if (node.Kind == MotionNodeKind.Rotate || node.Kind == MotionNodeKind.Aim || node.Kind == MotionNodeKind.Oscillate)
                    binding.JointTransform.localRotation = MotionPrimitives.Rotate(binding.BindLocalRotation, binding.LocalAxis, binding.MinimumValue, binding.MaximumValue, progress);
            }

            int actionCount = graph.ReferencedActions.Count;
            if (actionCount > 0)
            {
                int actionIndex = Mathf.Min((int)(Mathf.Clamp01(progress) * actionCount), actionCount - 1);
                if (!Apply(graph.ReferencedActions[actionIndex], rig, progress * actionCount - actionIndex, elapsedSeconds, visited)) return false;
            }

            ApplyPresentationTime(rig, elapsedSeconds);
            visited.Remove(graph);
            return true;
        }

        public static void RestoreBindPose(MotionRig rig)
        {
            if (rig == null) return;
            foreach (MotionJointBinding binding in rig.JointBindings)
            {
                if (binding == null || binding.JointTransform == null) continue;
                binding.JointTransform.localPosition = binding.BindLocalPosition;
                binding.JointTransform.localRotation = binding.BindLocalRotation;
            }

            foreach (MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is IMotionPreviewPresentation presentation) presentation.RestorePreviewBindPose();
            }
        }

        private static void ApplyPresentationTime(MotionRig rig, float elapsedSeconds)
        {
            foreach (MonoBehaviour behaviour in rig.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour is IMotionPreviewPresentation presentation) presentation.ApplyPreviewTime(elapsedSeconds);
            }
        }
    }
}
