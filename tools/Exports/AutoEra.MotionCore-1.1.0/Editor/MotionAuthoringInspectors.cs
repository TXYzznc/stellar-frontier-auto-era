using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    [CustomEditor(typeof(MotionRig))]
    internal sealed class MotionRigInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var rig = (MotionRig)target;
            if (!rig.TryValidate(out string error)) EditorGUILayout.HelpBox(error, MessageType.Error);
            else EditorGUILayout.HelpBox("Rig binding is valid.", MessageType.Info);
        }
    }

    [CustomEditor(typeof(MotionGraphAsset))]
    internal sealed class MotionGraphAssetInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var graph = (MotionGraphAsset)target;
            if (!graph.TryValidate(out string error)) EditorGUILayout.HelpBox(error, MessageType.Error);
            else EditorGUILayout.HelpBox("Graph static configuration is valid.", MessageType.Info);
        }
    }
}
