using System;
using AutoEra.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AutoEra.Editor
{
    public static class AutoEraStartupAssetBuilder
    {
        private const string PrefabPath = "Assets/Game/Prefabs/UI/Startup/MainMenuForm.prefab";
        private const string ScenePath = "Assets/Game/Scene/MainMenu.unity";

        [MenuItem("Game Framework/AutoEra/Fix Minimal Menu Contrast")]
        public static void FixContrast()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            GameObject root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var panel = root.transform.Find("MenuPanel");
                var button = panel.Find("EnterButton").GetComponent<Button>();
                // Reuse the approved dark surface instead of the navigation button's transparent white graphic.
                button.GetComponent<Image>().color = panel.GetComponent<Image>().color;
                PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [MenuItem("Game Framework/AutoEra/Register Startup Scenes")]
        public static void RegisterScenes()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (string path in new[] { ScenePath, "Assets/Game/Scene/InitialRegion.unity" })
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) throw new InvalidOperationException("Missing scene: " + path);
                int index = scenes.FindIndex(s => s.path == path);
                if (index >= 0)
                {
                    if (!scenes[index].enabled) throw new InvalidOperationException("Existing scene is disabled; preserve existing settings: " + path);
                    continue;
                }
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoEra][Startup] Registered enabled scenes without reordering existing entries.");
        }

        [MenuItem("Game Framework/AutoEra/Build Minimal Startup Assets")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Exit Play Mode.");
            var source = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab");
            if (source == null) throw new InvalidOperationException("Operations style source missing.");
            Button style = source.GetComponentInChildren<Button>(true);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Game/Fonts/UI/SIMHEI SDF.asset");
            if (style == null || font == null) throw new InvalidOperationException("Existing button style/font missing.");
            if (!AssetDatabase.IsValidFolder("Assets/Game/Prefabs/UI/Startup")) AssetDatabase.CreateFolder("Assets/Game/Prefabs/UI", "Startup");
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                var root = new GameObject("MainMenuForm", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(MainMenuForm));
                try
                {
                    root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                    var scaler = root.GetComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    var panel = new GameObject("MenuPanel", typeof(RectTransform), typeof(Image));
                    panel.transform.SetParent(root.transform, false);
                    var rect = (RectTransform)panel.transform;
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                    rect.offsetMin = rect.offsetMax = Vector2.zero;
                    Image background = panel.GetComponent<Image>();
                    Image sourceBackground = source.GetComponentInChildren<Image>(true);
                    background.color = sourceBackground != null ? sourceBackground.color : Color.black;
                    Text(panel.transform, "Title", "星际拓荒：自动纪元", font, 40, 140);
                    TMP_Text status = Text(panel.transform, "Status", "进入初始区域", font, 24, 35);
                    var buttonObject = new GameObject("EnterButton", typeof(RectTransform), typeof(Image), typeof(Button));
                    buttonObject.transform.SetParent(panel.transform, false);
                    var buttonRect = (RectTransform)buttonObject.transform;
                    buttonRect.sizeDelta = new Vector2(360, 72);
                    buttonRect.anchoredPosition = new Vector2(0, -80);
                    var button = buttonObject.GetComponent<Button>();
                    button.colors = style.colors; button.transition = Selectable.Transition.ColorTint;
                    var image = buttonObject.GetComponent<Image>();
                    var sourceImage = style.targetGraphic as Image;
                    if (sourceImage != null) { image.sprite = sourceImage.sprite; image.color = sourceImage.color; image.type = sourceImage.type; }
                    image.color = background.color;
                    button.targetGraphic = image;
                    Text(button.transform, "Label", "进入 / 重试", font, 24, 0);
                    var data = new SerializedObject(root.GetComponent<MainMenuForm>());
                    data.FindProperty("_enterButton").objectReferenceValue = button;
                    data.FindProperty("_status").objectReferenceValue = status;
                    data.ApplyModifiedPropertiesWithoutUndo();
                    PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                }
                finally { UnityEngine.Object.DestroyImmediate(root); }
            }
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                Scene previous = SceneManager.GetActiveScene();
                Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                try { EditorSceneManager.SaveScene(scene, ScenePath); }
                finally { EditorSceneManager.CloseScene(scene, true); if (previous.IsValid()) SceneManager.SetActiveScene(previous); }
            }
            var settings = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("Assets/Game/ScriptableAssets/Core/AppConfigs.asset"));
            SerializedProperty procedures = settings.FindProperty("mProcedures");
            foreach (string name in new[] { "AutoEra.Procedures.AutoEraStartupProcedure", "AutoEra.Procedures.AutoEraMainMenuProcedure", "AutoEra.Procedures.AutoEraWorldProcedure" })
            {
                bool exists = false;
                for (int i = 0; i < procedures.arraySize; i++) exists |= procedures.GetArrayElementAtIndex(i).stringValue == name;
                if (exists) continue;
                int index = procedures.arraySize;
                procedures.InsertArrayElementAtIndex(index);
                procedures.GetArrayElementAtIndex(index).stringValue = name;
            }
            settings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[AutoEra][Startup] Minimal scene, prefab and procedure registration ready.");
        }

        private static TMP_Text Text(Transform parent, string name, string value, TMP_FontAsset font, int size, float y)
        {
            var node = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            node.transform.SetParent(parent, false);
            var rect = (RectTransform)node.transform;
            rect.sizeDelta = new Vector2(name == "Label" ? 350 : 1100, 90);
            rect.anchoredPosition = new Vector2(0, y);
            var text = node.GetComponent<TextMeshProUGUI>();
            text.font = font; text.fontSize = size; text.text = value;
            text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
            return text;
        }
    }
}
