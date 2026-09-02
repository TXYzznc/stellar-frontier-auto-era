using System.Collections.Generic;
using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
    public enum MotionValidationCapability
    {
        FourWheel,
        Arm,
        Effector,
        SlidingDoor,
        Conveyor,
        PresentationLifecycle
    }

    public enum MotionValidationControl
    {
        Play,
        Pause,
        Reset,
        Interrupt,
        Recover,
        UsePositiveCase,
        UseNegativeCase
    }

    public sealed class MotionValidationControlState
    {
        public MotionValidationCapability Capability { get; internal set; }
        public bool IsPlaying { get; internal set; }
        public bool IsPaused { get; internal set; }
        public bool IsInterrupted { get; internal set; }
        public bool UsesNegativeCase { get; internal set; }
        public float TestProgress { get; internal set; }
    }

    /// <summary>
    /// Fixed Editor entry point for inspecting a FunctionalRigContract and its prototype hierarchy.
    /// The panel is diagnostic-only: it never writes source contracts, scenes, or gameplay state.
    /// </summary>
    internal sealed class FunctionalRigPrototypeValidationPanel : EditorWindow
    {
        private TextAsset _contractJson;
        private FunctionalRigPrototypeHierarchy _hierarchy;
        private Vector2 _scrollPosition;
        private string[] _messages = System.Array.Empty<string>();
        private bool _lastValidationPassed;
        private MotionValidationControlState _controlState = CreateControlState(MotionValidationCapability.FourWheel);

        [MenuItem("AutoEra/Functional Prototypes/Validation Panel")]
        private static void Open()
        {
            GetWindow<FunctionalRigPrototypeValidationPanel>("Functional Rig Validation");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Functional Rig Contract Validation", EditorStyles.boldLabel);
            _contractJson = (TextAsset)EditorGUILayout.ObjectField("Contract JSON", _contractJson, typeof(TextAsset), false);
            _hierarchy = (FunctionalRigPrototypeHierarchy)EditorGUILayout.ObjectField("Prototype Hierarchy", _hierarchy, typeof(FunctionalRigPrototypeHierarchy), true);

            using (new EditorGUI.DisabledScope(_contractJson == null))
            {
                if (GUILayout.Button("验证合同"))
                {
                    RunContractValidation();
                }
            }

            using (new EditorGUI.DisabledScope(_contractJson == null || _hierarchy == null))
            {
                if (GUILayout.Button("验证合同与原型结构"))
                {
                    RunStructureValidation();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("固定动作验收控制", EditorStyles.boldLabel);
            MotionValidationCapability capability = (MotionValidationCapability)EditorGUILayout.EnumPopup("能力", _controlState.Capability);
            if (capability != _controlState.Capability)
            {
                _controlState = CreateControlState(capability);
            }

            _controlState.TestProgress = EditorGUILayout.Slider("测试进度", _controlState.TestProgress, 0f, 1f);
            _controlState.UsesNegativeCase = EditorGUILayout.Toggle("负例参数", _controlState.UsesNegativeCase);
            EditorGUILayout.BeginHorizontal();
            DrawControlButton("播放", MotionValidationControl.Play);
            DrawControlButton("暂停", MotionValidationControl.Pause);
            DrawControlButton("重置", MotionValidationControl.Reset);
            DrawControlButton("中断", MotionValidationControl.Interrupt);
            DrawControlButton("恢复", MotionValidationControl.Recover);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(_controlState.IsPaused ? "预览：已暂停" : (_controlState.IsPlaying ? "预览：播放中" : "预览：绑定基线"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(_lastValidationPassed ? "结果：通过" : "结果：未验证或失败", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MinHeight(120f));
            if (_messages.Length == 0)
            {
                EditorGUILayout.LabelField("尚无诊断输出。");
            }
            else
            {
                foreach (string message in _messages)
                {
                    EditorGUILayout.HelpBox(message, _lastValidationPassed ? MessageType.Info : MessageType.Error);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void RunContractValidation()
        {
            _messages = ValidateContractJson(_contractJson.text, out _);
            _lastValidationPassed = _messages.Length == 0;
        }

        private void DrawControlButton(string label, MotionValidationControl control)
        {
            if (GUILayout.Button(label))
            {
                _controlState = ApplyControl(_controlState, control);
            }
        }

        public static MotionValidationControlState CreateControlState(MotionValidationCapability capability)
        {
            return new MotionValidationControlState { Capability = capability };
        }

        public static MotionValidationControlState ApplyControl(MotionValidationControlState state, MotionValidationControl control)
        {
            if (state == null)
            {
                return null;
            }

            switch (control)
            {
                case MotionValidationControl.Play:
                    state.IsPlaying = true;
                    state.IsPaused = false;
                    state.IsInterrupted = false;
                    break;
                case MotionValidationControl.Pause:
                    state.IsPaused = state.IsPlaying;
                    break;
                case MotionValidationControl.Reset:
                    state.IsPlaying = false;
                    state.IsPaused = false;
                    state.IsInterrupted = false;
                    state.TestProgress = 0f;
                    break;
                case MotionValidationControl.Interrupt:
                    state.IsInterrupted = state.IsPlaying;
                    state.IsPlaying = false;
                    state.IsPaused = false;
                    break;
                case MotionValidationControl.Recover:
                    state.IsInterrupted = false;
                    state.IsPlaying = true;
                    state.IsPaused = false;
                    break;
                case MotionValidationControl.UsePositiveCase:
                    state.UsesNegativeCase = false;
                    break;
                case MotionValidationControl.UseNegativeCase:
                    state.UsesNegativeCase = true;
                    break;
            }

            return state;
        }

        private void RunStructureValidation()
        {
            _messages = ValidateContractJson(_contractJson.text, out FunctionalRigContract contract);
            if (_messages.Length == 0)
            {
                _messages = FunctionalRigPrototypeStructureValidator.Validate(contract, _hierarchy);
            }

            _lastValidationPassed = _messages.Length == 0;
        }

        public static string[] ValidateContractJson(string json, out FunctionalRigContract contract)
        {
            if (FunctionalRigContractJson.TryDeserialize(json, out contract, out string error))
            {
                return System.Array.Empty<string>();
            }

            contract = null;
            return new[] { error };
        }
    }
}
