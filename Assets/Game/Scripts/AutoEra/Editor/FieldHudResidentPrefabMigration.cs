#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Editor
{
    /// <summary>一次性把现场 HUD 常驻层拆成独立 UIForm Prefab。</summary>
    public static class FieldHudResidentPrefabMigration
    {
        private const string Source = "Assets/Game/Prefabs/UI/Hud/FieldHudForm.prefab";
        private const string Target = "Assets/Game/Prefabs/UI/Hud/FieldHudResidentForm.prefab";
        private static readonly string[] ResidentNames =
        {
            "Panel_PageHudStatus", "Panel_PageHudTracker", "Panel_PageHudAlerts",
            "Panel_PageHudNavigation", "Panel_PageHudSave"
        };

        [MenuItem("AutoEra/UI/拆分 FieldHud 常驻层")]
        public static void SplitResidentLayer()
        {
            GameObject source = PrefabUtility.LoadPrefabContents(Source);
            try
            {
                Transform host = source.transform.Find("Grp_PageHost");
                if (host == null) throw new System.InvalidOperationException("找不到 FieldHudForm/Grp_PageHost");

                GameObject residentRoot = Object.Instantiate(source);
                residentRoot.name = "FieldHudResidentForm";
                residentRoot.transform.SetParent(null);
                Object.DestroyImmediate(residentRoot.GetComponent<AutoEra.UI.FieldHudForm>());
                residentRoot.AddComponent<AutoEra.UI.FieldHudResidentForm>();
                Transform residentHost = residentRoot.transform.Find("Grp_PageHost");
                for (int i = residentHost.childCount - 1; i >= 0; i--)
                {
                    Transform child = residentHost.GetChild(i);
                    if (!IsResident(child.name)) Object.DestroyImmediate(child.gameObject);
                }
                PrefabUtility.SaveAsPrefabAsset(residentRoot, Target);
                Object.DestroyImmediate(residentRoot);

                for (int i = host.childCount - 1; i >= 0; i--)
                {
                    if (IsResident(host.GetChild(i).name)) Object.DestroyImmediate(host.GetChild(i).gameObject);
                }
                PrefabUtility.SaveAsPrefabAsset(source, Source);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("FieldHud 常驻层已拆分为 " + Target);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        [MenuItem("AutoEra/UI/拆分 FieldHud 现场详情层")]
        public static void SplitDetailLayer()
        {
            GameObject source = PrefabUtility.LoadPrefabContents(Source);
            GameObject detail = null;
            try
            {
                detail = Object.Instantiate(source);
                detail.name = "FieldHudDetailForm";
                detail.transform.SetParent(null);
                Object.DestroyImmediate(detail.GetComponent<AutoEra.UI.FieldHudForm>());
                detail.AddComponent<AutoEra.UI.FieldHudDetailForm>();
                EnsureDetailExitButton(detail.transform, "Btn_FormBack", new Vector2(-180f, -32f), "返回");
                EnsureDetailExitButton(detail.transform, "Btn_FormClose", new Vector2(-72f, -32f), "关闭");
                PrefabUtility.SaveAsPrefabAsset(detail, "Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab");
                Object.DestroyImmediate(detail);

                Transform host = source.transform.Find("Grp_PageHost");
                if (host != null)
                {
                    for (int i = host.childCount - 1; i >= 0; i--)
                        Object.DestroyImmediate(host.GetChild(i).gameObject);
                }
                PrefabUtility.SaveAsPrefabAsset(source, Source);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("FieldHud 现场详情层已拆分为 Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab");
            }
            finally
            {
                if (detail != null) Object.DestroyImmediate(detail);
                PrefabUtility.UnloadPrefabContents(source);
            }
        }

        [MenuItem("AutoEra/UI/补齐 FieldHud 详情出口")]
        [MenuItem("AutoEra/UI/Ensure FieldHud Detail Exit Buttons")]
        public static void EnsureDetailExitButtonsInAsset()
        {
            GameObject detail = PrefabUtility.LoadPrefabContents("Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab");
            try
            {
                EnsureDetailExitButton(detail.transform, "Btn_FormBack", new Vector2(-180f, -32f), "返回");
                EnsureDetailExitButton(detail.transform, "Btn_FormClose", new Vector2(-72f, -32f), "关闭");
                PrefabUtility.SaveAsPrefabAsset(detail, "Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            finally { PrefabUtility.UnloadPrefabContents(detail); }
        }

        private static void EnsureDetailExitButton(Transform root, string name, Vector2 position, string labelText)
        {
            Transform existing = root.Find(name);
            GameObject go = existing == null
                ? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button))
                : existing.gameObject;
            if (existing == null) go.transform.SetParent(root, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(96f, 36f);
            rect.anchoredPosition = position;
            Image image = go.GetComponent<Image>();
            image.color = new Color(0.22f, 0.42f, 0.58f, 1f);
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;

            TextMeshProUGUI label = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
            {
                GameObject labelObject = new GameObject("Txt_" + name + "Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(go.transform, false);
                label = labelObject.GetComponent<TextMeshProUGUI>();
            }

            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(4f, 2f);
            labelRect.offsetMax = new Vector2(-4f, -2f);
            label.text = labelText;
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }

        private static bool IsResident(string name)
        {
            for (int i = 0; i < ResidentNames.Length; i++)
                if (ResidentNames[i] == name) return true;
            return false;
        }
    }
}
#endif
