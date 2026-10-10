using System;
using System.IO;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Editor.UiProto
{
    /// <summary>B51 authored-layout migration and isolated editor review captures.</summary>
    public static class RepresentativeUiLayoutTools
    {
        public const string Evidence = "openspec/changes/b51-ui-representative-layout-prototypes/evidence";
        public static readonly string[] Forms = { "MachineLibraryForm", "FieldHudResidentForm", "FieldHudMachineOverviewForm", "BaseCommandHubForm" };
        private static string Contract(string name) => AutoEraUiPrefabGenerator.ContractDirectory + "/" + name + ".contract.json";

        [MenuItem("Game Framework/AutoEra/UI/B51/导出HUD现有布局")]
        public static void ExportHud()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("停止 Play Mode 后执行。");
            Directory.CreateDirectory(Evidence);
            foreach (string name in new[] { Forms[1], Forms[2] })
            {
                JObject doc = JObject.Parse(File.ReadAllText(Contract(name)));
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>((string)doc["prefabPath"]);
                doc["root"] = Capture(prefab.transform);
                File.WriteAllText(Evidence + "/" + name + ".authored.json", doc.ToString());
            }
        }

        private static JArray V(Vector2 v) => new JArray(v.x, v.y);
        private static JArray C(Color c) => new JArray(c.r, c.g, c.b, c.a);
        private static JObject Capture(Transform t)
        {
            var r = (RectTransform)t;
            var components = new JArray();
            foreach (Component component in t.GetComponents<Component>())
            {
                if (component is Image im) components.Add(new JObject { ["type"] = "Image", ["color"] = C(im.color), ["imageType"] = (int)im.type, ["raycastTarget"] = im.raycastTarget, ["preserveAspect"] = im.preserveAspect });
                else if (component is TMP_Text text) components.Add(new JObject { ["type"] = "TextMeshProUGUI", ["text"] = text.text, ["fontSize"] = text.fontSize, ["fontPath"] = AssetDatabase.GetAssetPath(text.font), ["color"] = C(text.color), ["raycastTarget"] = text.raycastTarget, ["wordWrap"] = text.enableWordWrapping });
                else if (component is GridLayoutGroup grid) components.Add(new JObject { ["type"] = "GridLayoutGroup", ["cellSize"] = V(grid.cellSize), ["spacing"] = V(grid.spacing), ["constraint"] = (int)grid.constraint, ["constraintCount"] = grid.constraintCount });
                else if (component is Button || component is ScrollRect || component is RectMask2D || component is Mask || component is CanvasGroup || component is ContentSizeFitter || component is VerticalLayoutGroup || component is HorizontalLayoutGroup || component is LayoutElement)
                    components.Add(new JObject { ["type"] = component.GetType().Name });
            }
            var children = new JArray();
            foreach (Transform child in t) children.Add(Capture(child));
            return new JObject { ["name"] = t.name, ["active"] = t.gameObject.activeSelf, ["anchorMin"] = V(r.anchorMin), ["anchorMax"] = V(r.anchorMax), ["pivot"] = V(r.pivot), ["sizeDelta"] = V(r.sizeDelta), ["anchoredPosition"] = V(r.anchoredPosition), ["components"] = components, ["children"] = children };
        }

        [MenuItem("Game Framework/AutoEra/UI/B51/应用代表布局")]
        public static void Apply()
        {
            foreach (string name in Forms) AutoEraUiPrefabGenerator.ApplyExistingLayout(Contract(name));
            AssetDatabase.SaveAssets();
            var all = AutoEraContractGate1Checker.CheckAll();
            File.WriteAllLines(Evidence + "/gate1.txt", all);
            Debug.Log("[B51] 四个代表布局已应用。全仓库门禁问题数：" + all.Count);
        }

        [MenuItem("Game Framework/AutoEra/UI/B51/导出结构预览")]
        public static void CapturePreviews()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("停止 Play Mode 后执行。");
            Directory.CreateDirectory(Evidence);
            Preview("machine-preparation", Forms[0], "Panel_PageMachinePreparation", false);
            Preview("hub-overview", Forms[3], "Panel_PageHubOverview", false);
            Preview("world-hud", Forms[1], null, true);
        }

        private static void Preview(string file, string form, string pageName, bool includeOverview)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null;
            Texture2D pixels = null;
            RenderTexture previous = RenderTexture.active;
            try
            {
                var root = new GameObject("B51Preview", typeof(RectTransform), typeof(Canvas));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
                var cameraGo = new GameObject("B51Camera", typeof(Camera));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraGo, scene);
                var camera = cameraGo.GetComponent<Camera>();
                camera.scene = scene;
                camera.orthographic = true; camera.orthographicSize = 540;
                camera.transform.position = new Vector3(0, 0, -1000);
                camera.nearClipPlane = .1f; camera.farClipPlane = 2000;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(.065f, .075f, .085f);
                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 100;
                target = new RenderTexture(1920, 1080, 24); camera.targetTexture = target;
                AddForm(form, root.transform, pageName);
                if (includeOverview) AddForm(Forms[2], root.transform, "Panel_PageMachineOverview");
                Canvas.ForceUpdateCanvases();
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases(); camera.Render();
                RenderTexture.active = target;
                pixels = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0); pixels.Apply();
                File.WriteAllBytes(Evidence + "/" + file + "-structure-1920x1080.png", pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
                if (target != null) { target.Release(); UnityEngine.Object.DestroyImmediate(target); }
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void AddForm(string name, Transform parent, string pageName)
        {
            var doc = JObject.Parse(File.ReadAllText(Contract(name)));
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>((string)doc["prefabPath"]);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.SetActive(true);
            foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
            {
                if (t.name.StartsWith("Panel_Page", StringComparison.Ordinal)) t.gameObject.SetActive(pageName == null || t.name == pageName);
                if (t.name.StartsWith("Grp_", StringComparison.Ordinal) && t.name.EndsWith("State", StringComparison.Ordinal)) t.gameObject.SetActive(false);
                if (t.name == "Btn_FormClose") t.gameObject.SetActive(false);
            }
        }
    }
}
