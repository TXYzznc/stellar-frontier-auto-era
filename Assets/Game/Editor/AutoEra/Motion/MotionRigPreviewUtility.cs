using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    internal static class MotionRigPreviewUtility
    {
        [MenuItem("AutoEra/Functional Prototypes/Restore Selected MotionRig Bind Pose")]
        private static void RestoreSelectedRig()
        {
            MotionRig rig = Selection.activeGameObject == null ? null : Selection.activeGameObject.GetComponent<MotionRig>();
            if (rig == null) { Debug.LogWarning("[AutoEra.Motion] Select a MotionRig root first."); return; }
            foreach (MotionJointBinding binding in rig.JointBindings)
            {
                Undo.RecordObject(binding.JointTransform, "Restore MotionRig Bind Pose");
                binding.JointTransform.localPosition = binding.BindLocalPosition;
                binding.JointTransform.localRotation = binding.BindLocalRotation;
            }
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        private static void DrawJointAxes(MotionRig rig, GizmoType gizmoType)
        {
            Gizmos.color = Color.cyan;
            foreach (MotionJointBinding binding in rig.JointBindings)
            {
                if (binding.JointTransform == null) continue;
                Gizmos.DrawLine(binding.JointTransform.position, binding.JointTransform.position + binding.JointTransform.TransformDirection(binding.LocalAxis.normalized) * 0.5f);
            }
        }
    }
}
