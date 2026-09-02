using System.Collections.Generic;
using AutoEra.Motion;
using AutoEra.Motion.Contracts;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.Motion
{
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
