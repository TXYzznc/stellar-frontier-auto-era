using AutoEra.UI;
using AutoEra.UI.Contracts;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraUiPrefabBindingEditModeTests
    {
        private const string HubPrefabPath = "Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab";
        private const string HudPrefabPath = "Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab";

        [TestCase(HubPrefabPath)]
        [TestCase(HudPrefabPath)]
        public void OperationsEntryPrefab_IsIndependentOfTheRetiredVisualCandidateChain(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                Assert.That(PrefabUtility.IsPartOfPrefabInstance(root), Is.False, prefabPath);
                Assert.That(root.GetComponentsInChildren<Transform>(true)
                    .Any(transform => PrefabUtility.IsPartOfPrefabInstance(transform.gameObject)), Is.False, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            foreach (string dependency in AssetDatabase.GetDependencies(prefabPath, true))
            {
                Assert.That(dependency.StartsWith("Assets/Game/Prefabs/UI/ART006_UI/"), Is.False, dependency);
                Assert.That(dependency.StartsWith("Assets/Game/Art/UI/ART006_UI/"), Is.False, dependency);
            }
        }

        [Test]
        public void CommandHub_BindsFivePagesAndNavigationButtons()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HubPrefabPath);
            try
            {
                BaseCommandHubForm form = root.GetComponent<BaseCommandHubForm>();
                Assert.IsNotNull(form);
                SerializedObject serializedForm = new SerializedObject(form);
                SerializedProperty pageBindings = serializedForm.FindProperty("_pageBindings");
                Assert.IsNotNull(pageBindings);
                Assert.AreEqual(5, pageBindings.arraySize);

                for (int index = 0; index < pageBindings.arraySize; index++)
                {
                    SerializedProperty binding = pageBindings.GetArrayElementAtIndex(index);
                    Assert.IsNotNull(binding.FindPropertyRelative("_pageRoot").objectReferenceValue);
                    var navigationLabel = binding.FindPropertyRelative("_navigationLabel").objectReferenceValue as Component;
                    Assert.IsNotNull(navigationLabel);
                    Assert.IsNotNull(navigationLabel.GetComponent<Button>());
                }

                Assert.IsNotNull(serializedForm.FindProperty("_rulesImpactOverlay").objectReferenceValue);
                Assert.IsNotNull(serializedForm.FindProperty("_dangerConfirmationView").objectReferenceValue);

                SerializedProperty operationBindings = serializedForm.FindProperty("_operationBindings");
                Assert.AreEqual(1, operationBindings.arraySize);
                SerializedProperty ruleOperation = operationBindings.GetArrayElementAtIndex(0);
                Assert.AreEqual("rules", ruleOperation.FindPropertyRelative("_sourceId").stringValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_spinner").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_phaseText").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_successState").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_failureState").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_progress").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_longWaitHint").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_cancelAction").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_retryAction").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_detailsAction").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_progressFill").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_cancelledState").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_cancelButton").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_retryButton").objectReferenceValue);
                Assert.IsNotNull(ruleOperation.FindPropertyRelative("_detailsButton").objectReferenceValue);

                Transform reduceMotionEntry = FindRequired(root.transform, "Btn_ReduceMotion");
                Assert.IsNotNull(reduceMotionEntry.GetComponent<AutoEraReduceMotionEntry>());
                Assert.IsTrue(reduceMotionEntry.gameObject.activeSelf);
                AutoEraUiAccessibilityDescription longWaitDescription = FindRequired(root.transform, "Txt_RuleOperationLongWait").GetComponent<AutoEraUiAccessibilityDescription>();
                Assert.IsNotNull(longWaitDescription);
                Assert.AreEqual("操作仍在处理中，可查看详情", longWaitDescription.Description);
                Assert.GreaterOrEqual(FindRequired(root.transform, "Txt_RuleOperationLongWait").GetComponent<RectTransform>().rect.width, 320f);
                Component longWaitText = FindRequired(root.transform, "Txt_RuleOperationLongWait").GetComponent("TextMeshProUGUI");
                Assert.IsNotNull(longWaitText);
                Assert.IsFalse(new SerializedObject(longWaitText).FindProperty("m_enableWordWrapping").boolValue);

                AssertHasInteractiveOperationsStyle(root.transform, "Btn_RuleOperationCancel");
                AssertHasInteractiveOperationsStyle(root.transform, "Btn_RuleOperationRetry");
                AssertHasInteractiveOperationsStyle(root.transform, "Btn_RuleOperationDetails");
                AssertHasInteractiveOperationsStyle(root.transform, "Btn_ReduceMotion");

                Transform holdLabel = FindRequired(root.transform, "Art_Txt_RuleDangerHold");
                AutoEraHoldToConfirmView holdView = holdLabel.GetComponent<AutoEraHoldToConfirmView>();
                Assert.IsNotNull(holdView);
                SerializedObject serializedHoldView = new SerializedObject(holdView);
                Assert.IsNotNull(serializedHoldView.FindProperty("_progressRing").objectReferenceValue);
                Assert.IsNotNull(serializedHoldView.FindProperty("_progressRoot").objectReferenceValue);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void FieldHud_HasStableRuntimeFormComponent()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HudPrefabPath);
            try
            {
                Assert.IsNotNull(root.GetComponent<FieldHudForm>());
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void RuleOperationActions_OnlyRaiseWhenTheirAuthoritativeStateAllowsIt()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HubPrefabPath);
            try
            {
                BaseCommandHubForm form = root.GetComponent<BaseCommandHubForm>();
                AutoEraUiOperationVisualBinding binding = GetRuleBinding(form);
                AutoEraUiOperationActionRequest? received = null;
                binding.SetActionHandler(request => received = request);

                var inProgress = new AutoEraUiOperationSnapshot("op-1", AutoEraUiOperationStatus.InProgress, "执行中", 0.5f, true, false, "detail-1", "rules");
                binding.Apply(inProgress, AutoEraUiOperationPresentation.Create(inProgress, false));
                GetButton(binding, "_cancelButton").onClick.Invoke();
                Assert.That(received.HasValue, Is.True);
                Assert.That(received.Value.Action, Is.EqualTo(AutoEraUiOperationAction.Cancel));

                received = null;
                GetButton(binding, "_retryButton").onClick.Invoke();
                Assert.That(received.HasValue, Is.False);

                var failed = new AutoEraUiOperationSnapshot("op-2", AutoEraUiOperationStatus.Failed, "失败", null, false, true, "detail-2", "rules");
                binding.Apply(failed, AutoEraUiOperationPresentation.Create(failed, false));
                GetButton(binding, "_retryButton").onClick.Invoke();
                Assert.That(received.HasValue, Is.True);
                Assert.That(received.Value.Action, Is.EqualTo(AutoEraUiOperationAction.Retry));
                Assert.That(received.Value.RetryRequiresConfirmation, Is.True);

                received = null;
                GetButton(binding, "_detailsButton").onClick.Invoke();
                Assert.That(received.HasValue, Is.True);
                Assert.That(received.Value.Action, Is.EqualTo(AutoEraUiOperationAction.ViewDetails));
                Assert.That(received.Value.DetailTargetId, Is.EqualTo("detail-2"));
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        [Test]
        public void RuleOperationVisuals_CoverIdleLoadingLongWaitFailureCancellationAndRequestLoss()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HubPrefabPath);
            try
            {
                AutoEraUiOperationVisualBinding binding = GetRuleBinding(root.GetComponent<BaseCommandHubForm>());
                FindRequired(root.transform, "Panel_PageRules").gameObject.SetActive(true);

                var idle = new AutoEraUiOperationSnapshot("idle", AutoEraUiOperationStatus.Idle, string.Empty, null, false, false, string.Empty, "rules");
                binding.Apply(idle, AutoEraUiOperationPresentation.Create(idle, false));
                Assert.IsFalse(GetAction(binding, "_cancelAction").activeSelf);
                Assert.IsFalse(FindRequired(root.transform, "Bar_RuleOperationProgress").gameObject.activeSelf);

                var loading = new AutoEraUiOperationSnapshot("loading", AutoEraUiOperationStatus.InProgress, "正在执行", 0.25f, true, false, "details", "rules");
                binding.Apply(loading, AutoEraUiOperationPresentation.Create(loading, false));
                Assert.IsTrue(FindRequired(root.transform, "Art_RuleAsyncState").gameObject.activeSelf);
                Assert.IsTrue(FindRequired(root.transform, "Bar_RuleOperationProgress").gameObject.activeSelf);
                Assert.IsTrue(GetAction(binding, "_cancelAction").activeSelf);
                Assert.IsTrue(FindRequired(root.transform, "Btn_RuleOperationCancel").gameObject.activeInHierarchy);

                var waiting = new AutoEraUiOperationSnapshot("waiting", AutoEraUiOperationStatus.InProgress, "同步中", null, false, false, "details", "rules");
                binding.Apply(waiting, AutoEraUiOperationPresentation.Create(waiting, true));
                Assert.IsTrue(FindRequired(root.transform, "Txt_RuleOperationLongWait").gameObject.activeSelf);
                Assert.IsFalse(FindRequired(root.transform, "Bar_RuleOperationProgress").gameObject.activeSelf);

                var failed = new AutoEraUiOperationSnapshot("failed", AutoEraUiOperationStatus.Failed, "失败", null, false, true, "details", "rules");
                binding.Apply(failed, AutoEraUiOperationPresentation.Create(failed, false));
                Assert.IsTrue(FindRequired(root.transform, "Art_RuleConfigError").gameObject.activeSelf);
                Assert.IsTrue(GetAction(binding, "_retryAction").activeSelf);
                Assert.IsTrue(GetAction(binding, "_detailsAction").activeSelf);

                var cancelled = new AutoEraUiOperationSnapshot("cancelled", AutoEraUiOperationStatus.Cancelled, "已取消", null, false, false, string.Empty, "rules");
                binding.Apply(cancelled, AutoEraUiOperationPresentation.Create(cancelled, false));
                Assert.IsTrue(FindRequired(root.transform, "Txt_RuleOperationCancelled").gameObject.activeSelf);

                var requestLost = new AutoEraUiOperationSnapshot("lost", AutoEraUiOperationStatus.RequestLost, "请求丢失", null, false, false, "details", "rules");
                binding.Apply(requestLost, AutoEraUiOperationPresentation.Create(requestLost, false));
                Assert.IsTrue(FindRequired(root.transform, "Art_RuleConfigError").gameObject.activeSelf);
                Assert.IsTrue(GetAction(binding, "_retryAction").activeSelf);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static AutoEraUiOperationVisualBinding GetRuleBinding(BaseCommandHubForm form)
        {
            FieldInfo field = typeof(BaseCommandHubForm).GetField("_operationBindings", BindingFlags.Instance | BindingFlags.NonPublic);
            var bindings = field == null ? null : field.GetValue(form) as AutoEraUiOperationVisualBinding[];
            Assert.IsNotNull(bindings);
            Assert.IsNotEmpty(bindings);
            return bindings[0];
        }

        private static Button GetButton(AutoEraUiOperationVisualBinding binding, string fieldName)
        {
            FieldInfo field = typeof(AutoEraUiOperationVisualBinding).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var button = field == null ? null : field.GetValue(binding) as Button;
            Assert.IsNotNull(button);
            return button;
        }

        private static GameObject GetAction(AutoEraUiOperationVisualBinding binding, string fieldName)
        {
            FieldInfo field = typeof(AutoEraUiOperationVisualBinding).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var action = field == null ? null : field.GetValue(binding) as GameObject;
            Assert.IsNotNull(action);
            return action;
        }

        private static void AssertHasInteractiveOperationsStyle(Transform root, string name)
        {
            Transform target = FindRequired(root, name);
            var text = target.GetComponent<Graphic>();
            var button = target.transform.parent == null ? null : target.transform.parent.GetComponent<Button>();
            Assert.IsNotNull(text);
            Assert.IsNotNull(button);
            Assert.IsFalse(text.raycastTarget);
            var background = button.targetGraphic as Image;
            Assert.IsNotNull(background);
            Assert.IsTrue(background.raycastTarget);
            Assert.IsNotNull(background.sprite);
            RectTransform labelRect = target.GetComponent<RectTransform>();
            Assert.AreEqual(Vector2.zero, labelRect.anchorMin);
            Assert.AreEqual(Vector2.one, labelRect.anchorMax);
            Assert.AreEqual(Vector2.zero, labelRect.anchoredPosition);
            Assert.AreEqual(Vector2.zero, labelRect.sizeDelta);
        }

        private static Transform FindRequired(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index].name == name)
                {
                    return transforms[index];
                }
            }

            Assert.Fail("Missing required UI transform: " + name);
            return null;
        }
    }
}
