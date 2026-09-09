using System;
using TMPro;
using AutoEra.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Editor
{
    /// <summary>Builds stable Operations UI runtime bindings from the approved visual prefab hierarchy.</summary>
    public static class AutoEraOperationsPrefabBinder
    {
        private const string HubPrefabPath = "Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab";
        private const string HudPrefabPath = "Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab";
        private const string OperationsFontAssetPath = "Assets/Game/Fonts/UI/Operations/AlibabaPuHuiTi-3-85-Bold SDF.asset";
        private static readonly AutoEraHubPage[] Pages =
        {
            AutoEraHubPage.Overview,
            AutoEraHubPage.Tasks,
            AutoEraHubPage.Objects,
            AutoEraHubPage.Rules,
            AutoEraHubPage.Statistics
        };

        private static readonly string[] PageRoots =
        {
            "Panel_PageOverview",
            "Panel_PageTasks",
            "Panel_PageObjects",
            "Panel_PageRules",
            "Panel_PageStatistics"
        };

        private static readonly string[] NavigationLabels =
        {
            "Txt_TabOverview",
            "Txt_TabTasks",
            "Txt_TabObjects",
            "Txt_TabRules",
            "Txt_TabStatistics"
        };

        private static readonly Color ActiveNavigationColor = new Color(1f, 0.64f, 0.22f, 1f);
        private static readonly Color InactiveNavigationColor = new Color(0.9647059f, 0.94509804f, 0.9098039f, 1f);

        [MenuItem("Game Framework/AutoEra/Bind Operations UI Prefabs")]
        public static void BindOperationsPrefabs()
        {
            BindHubPrefab();
            BindHudPrefab();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Game Framework/AutoEra/Build Independent Operations UI Prefabs")]
        public static void BuildIndependentOperationsPrefabs()
        {
            BindOperationsPrefabs();
            MakePrefabIndependent(HubPrefabPath);
            MakePrefabIndependent(HudPrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void MakePrefabIndependent(string prefabPath)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                if (PrefabUtility.IsPartOfPrefabInstance(root))
                {
                    PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                }

                if (PrefabUtility.IsPartOfPrefabInstance(root))
                {
                    throw new InvalidOperationException($"Operations entry prefab remains connected after unpacking: {prefabPath}");
                }

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BindHubPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HubPrefabPath);
            try
            {
                BaseCommandHubForm form = root.GetComponent<BaseCommandHubForm>();
                if (form == null)
                {
                    form = root.AddComponent<BaseCommandHubForm>();
                }

                SerializedObject serializedForm = new SerializedObject(form);
                BindOperationsFont(root.transform);
                ReplaceUnsupportedDecorativeGlyphs(root.transform);
                SerializedProperty bindings = serializedForm.FindProperty("_pageBindings");
                bindings.arraySize = Pages.Length;

                Button[] buttons = new Button[Pages.Length];
                for (int index = 0; index < Pages.Length; index++)
                {
                    Transform pageRoot = FindRequired(root.transform, PageRoots[index]);
                    TMP_Text navigationLabel = FindRequired(root.transform, NavigationLabels[index]).GetComponent<TMP_Text>();
                    if (navigationLabel == null)
                    {
                        throw new InvalidOperationException($"{NavigationLabels[index]} requires TMP_Text.");
                    }

                    SerializedProperty binding = bindings.GetArrayElementAtIndex(index);
                    binding.FindPropertyRelative("_page").enumValueIndex = (int)Pages[index];
                    binding.FindPropertyRelative("_pageRoot").objectReferenceValue = pageRoot.gameObject;
                    binding.FindPropertyRelative("_activeNavigationVisual").objectReferenceValue = null;
                    binding.FindPropertyRelative("_navigationLabel").objectReferenceValue = navigationLabel;
                    binding.FindPropertyRelative("_activeLabelColor").colorValue = ActiveNavigationColor;
                    binding.FindPropertyRelative("_inactiveLabelColor").colorValue = InactiveNavigationColor;

                    Button button = navigationLabel.GetComponent<Button>();
                    if (button == null)
                    {
                        button = navigationLabel.gameObject.AddComponent<Button>();
                    }

                    button.targetGraphic = navigationLabel;
                    button.onClick = new Button.ButtonClickedEvent();
                    UnityEventTools.AddIntPersistentListener(button.onClick, form.SelectPage, (int)Pages[index]);
                    buttons[index] = button;
                }

                for (int index = 0; index < buttons.Length; index++)
                {
                    Navigation navigation = new Navigation
                    {
                        mode = Navigation.Mode.Explicit,
                        selectOnLeft = buttons[(index + buttons.Length - 1) % buttons.Length],
                        selectOnRight = buttons[(index + 1) % buttons.Length]
                    };
                    buttons[index].navigation = navigation;
                }

                serializedForm.FindProperty("_defaultFocus").objectReferenceValue = buttons[0];
                serializedForm.FindProperty("_rulesImpactOverlay").objectReferenceValue = FindRequired(root.transform, "Overlay_RulesImpact").gameObject;
                BindDangerConfirmation(root.transform, serializedForm);
                BindRuleOperationPresentation(root.transform, serializedForm);
                BindRuleHoldToConfirm(root.transform);
                BindReduceMotionEntry(root.transform);
                BindCancelIntentProxies(root.transform);
                serializedForm.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, HubPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BindHudPrefab()
        {
            GameObject root = PrefabUtility.LoadPrefabContents(HudPrefabPath);
            try
            {
                BindOperationsFont(root.transform);
                ReplaceUnsupportedDecorativeGlyphs(root.transform);
                BindHudTextCompatibility(root.transform);
                BindCancelIntentProxies(root.transform);
                if (root.GetComponent<FieldHudForm>() == null)
                {
                    root.AddComponent<FieldHudForm>();
                }

                PrefabUtility.SaveAsPrefabAsset(root, HudPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void BindOperationsFont(Transform root)
        {
            TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OperationsFontAssetPath);
            if (fontAsset == null)
            {
                throw new InvalidOperationException($"Operations UI font is missing: {OperationsFontAssetPath}");
            }

            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < texts.Length; index++)
            {
                texts[index].font = fontAsset;
            }
        }

        private static void BindHudTextCompatibility(Transform root)
        {
            Transform resourcesVisual = FindOptional(root, "Art_Txt_HudResourcesVisual");
            TMP_Text text = resourcesVisual == null ? null : resourcesVisual.GetComponent<TMP_Text>();
            if (text != null)
            {
                // Keep the approved numeric layout while avoiding unsupported decorative glyphs in the shared TMP asset.
                text.text = "电力 1280 / 1500\\n水 860 / 1200\\n食物 420 / 800\\n人员 42 / 60";
            }
        }

        private static void ReplaceUnsupportedDecorativeGlyphs(Transform root)
        {
            TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < texts.Length; index++)
            {
                texts[index].text = texts[index].text
                    .Replace("⚡", "电力")
                    .Replace("♟", "人员");
            }
        }

        private static void BindCancelIntentProxies(Transform root)
        {
            Selectable[] selectables = root.GetComponentsInChildren<Selectable>(true);
            for (int index = 0; index < selectables.Length; index++)
            {
                if (selectables[index].GetComponent<AutoEraUiCancelIntentProxy>() == null)
                {
                    selectables[index].gameObject.AddComponent<AutoEraUiCancelIntentProxy>();
                }
            }
        }

        private static void BindDangerConfirmation(Transform root, SerializedObject serializedForm)
        {
            Transform overlay = FindRequired(root, "Overlay_RulesImpact");
            AutoEraDangerConfirmationView view = overlay.GetComponent<AutoEraDangerConfirmationView>();
            if (view == null)
            {
                view = overlay.gameObject.AddComponent<AutoEraDangerConfirmationView>();
            }

            TMP_Text cancelText = FindRequired(root, "Txt_ImpactModalCancel").GetComponent<TMP_Text>();
            TMP_Text dangerText = FindRequired(root, "Txt_ImpactModalConfirm").GetComponent<TMP_Text>();
            Button cancelButton = GetOrAddButton(cancelText);
            Button dangerButton = GetOrAddButton(dangerText);

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("_overlayRoot").objectReferenceValue = overlay.gameObject;
            serializedView.FindProperty("_bodyText").objectReferenceValue = FindRequired(root, "Txt_ImpactModalBody").GetComponent<TMP_Text>();
            serializedView.FindProperty("_errorBox").objectReferenceValue = FindRequired(root, "Art_ImpactConfigError").gameObject;
            serializedView.FindProperty("_errorText").objectReferenceValue = FindRequired(root, "Art_Txt_ImpactError").GetComponent<TMP_Text>();
            serializedView.FindProperty("_cancelButton").objectReferenceValue = cancelButton;
            serializedView.FindProperty("_dangerButton").objectReferenceValue = dangerButton;
            serializedView.ApplyModifiedPropertiesWithoutUndo();

            serializedForm.FindProperty("_dangerConfirmationView").objectReferenceValue = view;
        }

        private static void BindRuleOperationPresentation(Transform root, SerializedObject serializedForm)
        {
            Transform stateRoot = FindRequired(root, "Art_RuleAsyncState");
            TMP_Text template = FindRequired(root, "Txt_RuleSummarySave").GetComponent<TMP_Text>();
            if (template == null)
            {
                throw new InvalidOperationException("Rule operation controls require a TMP text style template.");
            }

            TMP_Text cancelText = GetOrCreateRuleText(root, stateRoot.parent, template, "Btn_RuleOperationCancel", "取消执行", new Vector2(-120f, -76f), new Vector2(112f, 38f), false);
            TMP_Text retryText = GetOrCreateRuleText(root, stateRoot.parent, template, "Btn_RuleOperationRetry", "重试", new Vector2(0f, -76f), new Vector2(112f, 38f), false);
            TMP_Text detailsText = GetOrCreateRuleText(root, stateRoot.parent, template, "Btn_RuleOperationDetails", "查看详情", new Vector2(120f, -76f), new Vector2(112f, 38f), false);
            TMP_Text longWaitText = GetOrCreateRuleText(root, stateRoot.parent, template, "Txt_RuleOperationLongWait", "仍在处理中／查看详情", new Vector2(0f, -24f), new Vector2(360f, 32f), false);
            TMP_Text cancelledText = GetOrCreateRuleText(root, stateRoot.parent, template, "Txt_RuleOperationCancelled", "操作已取消", new Vector2(0f, -24f), new Vector2(180f, 32f), false);
            Image progressFill = GetOrCreateProgressFill(root, stateRoot.parent, stateRoot, new Vector2(0f, -48f));
            BindAccessibilityDescription(longWaitText.gameObject, "操作仍在处理中，可查看详情");
            BindAccessibilityDescription(detailsText.gameObject, "查看当前对象的操作详情");
            Button cancelButton = ConfigureOperationsActionButton(cancelText, "Assets/Game/Sprites/UI/Operations/Common/C02_C05_OperationsAndSafety/c02-action-secondary.png", false);
            Button retryButton = ConfigureOperationsActionButton(retryText, "Assets/Game/Sprites/UI/Operations/Common/C02_C05_OperationsAndSafety/c02-action-secondary.png", false);
            Button detailsButton = ConfigureOperationsActionButton(detailsText, "Assets/Game/Sprites/UI/Operations/Common/C02_C05_OperationsAndSafety/c02-action-secondary.png", false);

            SerializedProperty bindings = serializedForm.FindProperty("_operationBindings");
            bindings.arraySize = 1;
            SerializedProperty binding = bindings.GetArrayElementAtIndex(0);
            binding.FindPropertyRelative("_sourceId").stringValue = "rules";
            binding.FindPropertyRelative("_spinner").objectReferenceValue = FindRequired(root, "Art_RuleAsyncState").gameObject;
            binding.FindPropertyRelative("_progress").objectReferenceValue = progressFill.gameObject;
            binding.FindPropertyRelative("_longWaitHint").objectReferenceValue = longWaitText.gameObject;
            binding.FindPropertyRelative("_cancelAction").objectReferenceValue = cancelButton.gameObject;
            binding.FindPropertyRelative("_retryAction").objectReferenceValue = retryButton.gameObject;
            binding.FindPropertyRelative("_detailsAction").objectReferenceValue = detailsButton.gameObject;
            binding.FindPropertyRelative("_phaseText").objectReferenceValue = FindRequired(root, "Art_Txt_RuleAsync").GetComponent<TMP_Text>();
            binding.FindPropertyRelative("_progressFill").objectReferenceValue = progressFill;
            binding.FindPropertyRelative("_successState").objectReferenceValue = FindRequired(root, "Art_RuleSuccessState").gameObject;
            binding.FindPropertyRelative("_failureState").objectReferenceValue = FindRequired(root, "Art_RuleConfigError").gameObject;
            binding.FindPropertyRelative("_cancelledState").objectReferenceValue = cancelledText.gameObject;
            binding.FindPropertyRelative("_cancelButton").objectReferenceValue = cancelButton;
            binding.FindPropertyRelative("_retryButton").objectReferenceValue = retryButton;
            binding.FindPropertyRelative("_detailsButton").objectReferenceValue = detailsButton;
        }

        private static void BindReduceMotionEntry(Transform root)
        {
            Transform stateRoot = FindRequired(root, "Art_RuleAsyncState");
            TMP_Text template = FindRequired(root, "Txt_RuleSummarySave").GetComponent<TMP_Text>();
            TMP_Text label = GetOrCreateRuleText(root, stateRoot.parent, template, "Btn_ReduceMotion", "减弱动态：关", new Vector2(0f, -122f), new Vector2(160f, 38f), true);
            Button button = ConfigureOperationsActionButton(label, "Assets/Game/Sprites/UI/Operations/Common/C02_C05_OperationsAndSafety/c02-action-secondary.png", true);
            AutoEraReduceMotionEntry entry = label.GetComponent<AutoEraReduceMotionEntry>();
            if (entry == null)
            {
                entry = label.gameObject.AddComponent<AutoEraReduceMotionEntry>();
            }

            SerializedObject serializedEntry = new SerializedObject(entry);
            serializedEntry.FindProperty("_label").objectReferenceValue = label;
            serializedEntry.FindProperty("_button").objectReferenceValue = button;
            serializedEntry.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BindAccessibilityDescription(GameObject target, string description)
        {
            AutoEraUiAccessibilityDescription accessibility = target.GetComponent<AutoEraUiAccessibilityDescription>();
            if (accessibility == null)
            {
                accessibility = target.AddComponent<AutoEraUiAccessibilityDescription>();
            }

            SerializedObject serializedAccessibility = new SerializedObject(accessibility);
            serializedAccessibility.FindProperty("_description").stringValue = description;
            serializedAccessibility.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TMP_Text GetOrCreateRuleText(Transform root, Transform parent, TMP_Text template, string name, string value, Vector2 anchoredPosition, Vector2 size, bool activeByDefault)
        {
            Transform existing = FindOptional(root, name);
            TMP_Text text;
            if (existing == null)
            {
                text = UnityEngine.Object.Instantiate(template, parent, false);
                text.gameObject.name = name;
            }
            else
            {
                text = existing.GetComponent<TMP_Text>();
            }

            if (text == null)
            {
                throw new InvalidOperationException($"{name} requires TMP_Text.");
            }

            text.text = value;
            text.enableWordWrapping = false;
            text.overflowMode = TextOverflowModes.Ellipsis;
            text.gameObject.SetActive(activeByDefault);
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return text;
        }

        private static Button ConfigureOperationsActionButton(TMP_Text label, string spritePath, bool activeByDefault)
        {
            Transform parent = label.transform.parent;
            if (parent == null)
            {
                throw new InvalidOperationException($"{label.name} requires a parent container.");
            }

            string backgroundName = label.name + "_Background";
            bool labelAlreadyWrapped = parent.name == backgroundName && parent.GetComponent<Image>() != null;
            Transform backgroundTransform = labelAlreadyWrapped ? parent : parent.Find(backgroundName);
            if (!labelAlreadyWrapped && backgroundTransform == null)
            {
                backgroundTransform = label.transform.Find("Background");
            }

            GameObject backgroundObject;
            if (backgroundTransform == null)
            {
                backgroundObject = new GameObject(backgroundName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            }
            else
            {
                backgroundObject = backgroundTransform.gameObject;
                backgroundObject.name = backgroundName;
            }

            RectTransform labelRect = label.GetComponent<RectTransform>();
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            if (!labelAlreadyWrapped)
            {
                backgroundObject.transform.SetParent(parent, false);
                backgroundObject.transform.SetSiblingIndex(label.transform.GetSiblingIndex());
                backgroundRect.anchorMin = labelRect.anchorMin;
                backgroundRect.anchorMax = labelRect.anchorMax;
                backgroundRect.pivot = labelRect.pivot;
                backgroundRect.anchoredPosition = labelRect.anchoredPosition;
                backgroundRect.sizeDelta = labelRect.sizeDelta;

                label.transform.SetParent(backgroundObject.transform, false);
            }

            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
            backgroundObject.SetActive(activeByDefault);
            label.gameObject.SetActive(true);

            Image background = backgroundObject.GetComponent<Image>();

            if (background == null)
            {
                throw new InvalidOperationException($"{label.name} background requires Image.");
            }

            Button oldLabelButton = label.GetComponent<Button>();
            if (oldLabelButton != null)
            {
                UnityEngine.Object.DestroyImmediate(oldLabelButton);
            }

            Button button = backgroundObject.GetComponent<Button>();
            if (button == null)
            {
                button = backgroundObject.AddComponent<Button>();
            }

            background.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            background.type = Image.Type.Sliced;
            background.raycastTarget = true;
            label.raycastTarget = false;
            button.targetGraphic = background;
            button.transition = Selectable.Transition.ColorTint;
            return button;
        }

        private static Image GetOrCreateProgressFill(Transform root, Transform parent, Transform styleSource, Vector2 anchoredPosition)
        {
            Transform existing = FindOptional(root, "Bar_RuleOperationProgress");
            Image progressFill;
            if (existing == null)
            {
                var progressObject = new GameObject("Bar_RuleOperationProgress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                progressObject.transform.SetParent(parent, false);
                progressFill = progressObject.GetComponent<Image>();
                Image style = styleSource.GetComponent<Image>();
                if (style != null)
                {
                    progressFill.sprite = style.sprite;
                    progressFill.color = style.color;
                }
            }
            else
            {
                progressFill = existing.GetComponent<Image>();
            }

            if (progressFill == null)
            {
                throw new InvalidOperationException("Bar_RuleOperationProgress requires Image.");
            }

            RectTransform rect = progressFill.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(260f, 6f);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0f;
            return progressFill;
        }

        private static void BindRuleHoldToConfirm(Transform root)
        {
            TMP_Text hint = FindRequired(root, "Art_Txt_RuleDangerHold").GetComponent<TMP_Text>();
            if (hint == null)
            {
                throw new InvalidOperationException("Rule dangerous hold hint requires TMP_Text.");
            }

            Transform progressTransform = hint.transform.Find("RuleHoldProgress");
            Image progressRing;
            if (progressTransform == null)
            {
                var progressObject = new GameObject("RuleHoldProgress", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                progressObject.transform.SetParent(hint.transform, false);
                RectTransform rectTransform = progressObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0f, 0.5f);
                rectTransform.anchorMax = new Vector2(0f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(-24f, 0f);
                rectTransform.sizeDelta = new Vector2(18f, 18f);
                progressRing = progressObject.GetComponent<Image>();
                progressRing.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                    "Assets/Game/Sprites/UI/Operations/Common/C02_C05_OperationsAndSafety/c05-hold-danger-holding.png");
                progressRing.type = Image.Type.Filled;
                progressRing.fillMethod = Image.FillMethod.Radial360;
                progressRing.fillAmount = 0f;
                progressObject.SetActive(false);
            }
            else
            {
                progressRing = progressTransform.GetComponent<Image>();
            }

            AutoEraHoldToConfirmView view = hint.GetComponent<AutoEraHoldToConfirmView>();
            if (view == null)
            {
                view = hint.gameObject.AddComponent<AutoEraHoldToConfirmView>();
            }

            SerializedObject serializedView = new SerializedObject(view);
            serializedView.FindProperty("_progressRing").objectReferenceValue = progressRing;
            serializedView.FindProperty("_hintText").objectReferenceValue = hint;
            serializedView.FindProperty("_progressRoot").objectReferenceValue = progressRing.gameObject;
            serializedView.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Button GetOrAddButton(TMP_Text label)
        {
            if (label == null)
            {
                throw new InvalidOperationException("Confirmation label requires TMP_Text.");
            }

            Button button = label.GetComponent<Button>();
            if (button == null)
            {
                button = label.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = label;
            return button;
        }

        private static Transform FindRequired(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (string.Equals(transforms[index].name, name, StringComparison.Ordinal))
                {
                    return transforms[index];
                }
            }

            throw new InvalidOperationException($"Required Operations UI object is missing: {name}");
        }

        private static Transform FindOptional(Transform root, string name)
        {
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int index = 0; index < transforms.Length; index++)
            {
                if (string.Equals(transforms[index].name, name, StringComparison.Ordinal))
                {
                    return transforms[index];
                }
            }

            return null;
        }
    }
}
