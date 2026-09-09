using System;
using System.Collections.Generic;
using AutoEra.Motion;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    /// <summary>Editor-facing wrapper for the pure preview timeline rules.</summary>
    public static class MotionPreviewTimeline
    {
        public const float CycleSeconds = MotionPreviewTimelineCore.CycleSeconds;
        public const float MinimumSpeed = MotionPreviewTimelineCore.MinimumSpeed;
        public const float MaximumSpeed = MotionPreviewTimelineCore.MaximumSpeed;

        public static float ClampSpeed(float speed) => MotionPreviewTimelineCore.ClampSpeed(speed);
        public static float AdvanceProgress(float progress, float deltaSeconds, float speed) => MotionPreviewTimelineCore.AdvanceProgress(progress, deltaSeconds, speed);
        public static float ElapsedForProgress(float progress) => MotionPreviewTimelineCore.ElapsedForProgress(progress);
    }

    /// <summary>Editor-only action browser and preview surface. It never submits gameplay instructions.</summary>
    internal sealed class MotionPreviewWindow : EditorWindow
    {
        private MotionRig _rig;
        private MotionGraphAsset _selected;
        private float _progress;
        private float _elapsed;
        private float _speed = 1f;
        private bool _playing;
        private readonly List<MotionGraphAsset> _compatible = new List<MotionGraphAsset>();
        private readonly List<MotionGraphAsset> _composition = new List<MotionGraphAsset>();

        [MenuItem("AutoEra/Motion/动作预览")]
        private static void Open() => GetWindow<MotionPreviewWindow>("动作预览");

        private void OnGUI()
        {
            _rig = (MotionRig)EditorGUILayout.ObjectField("动作对象", _rig, typeof(MotionRig), true);
            if (GUILayout.Button("刷新可用动作")) RefreshActions();
            using (new EditorGUI.DisabledScope(_rig == null))
            {
                EditorGUI.BeginChangeCheck();
                float scrubbedProgress = EditorGUILayout.Slider("预览进度", _progress, 0f, 1f);
                if (EditorGUI.EndChangeCheck()) ScrubTo(scrubbedProgress);
                _speed = EditorGUILayout.Slider("播放速度", _speed, MotionPreviewTimeline.MinimumSpeed, MotionPreviewTimeline.MaximumSpeed);
                EditorGUILayout.LabelField("速度倍率", _speed.ToString("0.0") + "×");
                foreach (MotionGraphAsset action in _compatible)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("播放", GUILayout.Width(54f))) { _selected = action; _playing = true; ScrubTo(0f); }
                    if (GUILayout.Button("加入组合", GUILayout.Width(72f))) _composition.Add(action);
                    EditorGUILayout.ObjectField(action, typeof(MotionGraphAsset), false);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(_playing ? "暂停" : "播放当前")) _playing = !_playing;
                if (GUILayout.Button("速度重置")) _speed = 1f;
                if (GUILayout.Button("安全中断")) { _playing = false; MotionPreviewEvaluator.RestoreBindPose(_rig); }
                if (GUILayout.Button("重置")) { _playing = false; _progress = 0f; MotionPreviewEvaluator.RestoreBindPose(_rig); }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("组合动作", _composition.Count.ToString());
                if (GUILayout.Button("保存组合动作图")) SaveComposition();
            }
        }

        private void Update()
        {
            if (!_playing || _rig == null || _selected == null) return;
            _speed = MotionPreviewTimeline.ClampSpeed(_speed);
            _elapsed += Time.deltaTime * _speed;
            _progress = MotionPreviewTimeline.AdvanceProgress(_progress, Time.deltaTime, _speed);
            MotionPreviewEvaluator.Apply(_selected, _rig, _progress, _elapsed);
            Repaint();
        }

        private void ScrubTo(float progress)
        {
            _progress = Mathf.Clamp01(progress);
            _elapsed = MotionPreviewTimeline.ElapsedForProgress(_progress);
            if (_rig != null && _selected != null)
            {
                MotionPreviewEvaluator.RestoreBindPose(_rig);
                MotionPreviewEvaluator.Apply(_selected, _rig, _progress, _elapsed);
            }

            Repaint();
        }

        private void RefreshActions()
        {
            _compatible.Clear();
            if (_rig == null) return;
            foreach (string guid in AssetDatabase.FindAssets("t:MotionGraphAsset"))
            {
                MotionGraphAsset graph = AssetDatabase.LoadAssetAtPath<MotionGraphAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (graph != null && graph.IsCompatibleWith(_rig)) _compatible.Add(graph);
            }
        }

        private void SaveComposition()
        {
            if (_composition.Count == 0) return;
            string path = EditorUtility.SaveFilePanelInProject("保存组合动作图", "MotionComposition", "asset", "选择保存位置");
            if (string.IsNullOrEmpty(path)) return;
            MotionGraphAsset graph = CreateInstance<MotionGraphAsset>();
            graph.Configure(1, Guid.NewGuid().ToString("N"), "1.0.0", Array.Empty<MotionParameterDefinition>(), new[] { new MotionNodeDefinition("sequence", MotionNodeKind.Sequence, string.Empty, string.Empty) }, Array.Empty<MotionConnectionDefinition>(), _rig == null ? null : _rig.ContractId);
            graph.ConfigureReferences(_composition.ToArray());
            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.SaveAssets();
            _selected = graph;
            RefreshActions();
        }
    }
}
