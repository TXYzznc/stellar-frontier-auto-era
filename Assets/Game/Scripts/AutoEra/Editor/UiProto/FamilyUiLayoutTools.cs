using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Editor.UiProto
{
    /// <summary>B52 in-place layout rollout and isolated 1080p structure review.</summary>
    public static class FamilyUiLayoutTools
    {
        public const string Change = "openspec/changes/b52-ui-all-pages-visual-optimization";
        private const string Evidence = Change + "/evidence";
        private const string Manifest = Change + "/art/rollout-manifest.json";

        [MenuItem("Game Framework/AutoEra/UI/B52/导出缺失结构合同")]
        public static void ExportAuthored()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请停止 Play Mode。");
            Directory.CreateDirectory(Evidence + "/authored");
            foreach (string path in new[] {
                "Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab",
                "Assets/Game/Prefabs/UI/Item/AlgorithmNodeItem.prefab",
                "Assets/Game/Prefabs/UI/Item/AlgorithmEdgeItem.prefab" })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                string output = Evidence + "/authored/" + prefab.name + ".json";
                if (File.Exists(output)) throw new InvalidOperationException("基线已存在，禁止覆盖：" + output);
                var doc = new JObject { ["prefabPath"] = path, ["root"] = Capture(prefab.transform) };
                File.WriteAllText(output, doc.ToString());
            }
        }

        private static JArray V(Vector2 v) => new JArray(v.x, v.y);
        private static JObject Capture(Transform t)
        {
            var r = (RectTransform)t;
            var c = new JArray();
            var img = t.GetComponent<Image>();
            if (img != null) c.Add(new JObject { ["type"] = "Image", ["color"] = new JArray(img.color.r,img.color.g,img.color.b,img.color.a), ["raycastTarget"] = img.raycastTarget });
            var text = t.GetComponent<TMP_Text>();
            if (text != null) c.Add(new JObject { ["type"] = "TextMeshProUGUI", ["text"] = text.text, ["fontSize"] = text.fontSize, ["raycastTarget"] = text.raycastTarget, ["wordWrap"] = text.enableWordWrapping });
            if (t.GetComponent<Button>() != null) c.Add(new JObject { ["type"] = "Button" });
            var lg = t.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (lg != null) c.Add(new JObject { ["type"] = lg.GetType().Name, ["spacing"] = lg.spacing, ["childControlWidth"] = lg.childControlWidth, ["childControlHeight"] = lg.childControlHeight, ["childForceExpandWidth"] = lg.childForceExpandWidth, ["childForceExpandHeight"] = lg.childForceExpandHeight });
            var le = t.GetComponent<LayoutElement>();
            if (le != null) c.Add(new JObject { ["type"] = "LayoutElement", ["minWidth"] = le.minWidth, ["minHeight"] = le.minHeight, ["preferredWidth"] = le.preferredWidth, ["preferredHeight"] = le.preferredHeight, ["flexibleWidth"] = le.flexibleWidth, ["flexibleHeight"] = le.flexibleHeight, ["ignoreLayout"] = le.ignoreLayout });
            var fitter = t.GetComponent<ContentSizeFitter>();
            if (fitter != null) c.Add(new JObject { ["type"] = "ContentSizeFitter", ["horizontalFit"] = (int)fitter.horizontalFit, ["verticalFit"] = (int)fitter.verticalFit });
            if (t.GetComponent<ScrollRect>() != null) c.Add(new JObject { ["type"] = "ScrollRect" });
            if (t.GetComponent<RectMask2D>() != null) c.Add(new JObject { ["type"] = "RectMask2D" });
            var children = new JArray();
            foreach (Transform child in t) children.Add(Capture(child));
            return new JObject { ["name"] = t.name, ["active"] = t.gameObject.activeSelf,
                ["anchorMin"] = V(r.anchorMin), ["anchorMax"] = V(r.anchorMax), ["pivot"] = V(r.pivot),
                ["sizeDelta"] = V(r.sizeDelta), ["anchoredPosition"] = V(r.anchoredPosition), ["components"] = c, ["children"] = children };
        }

        [MenuItem("Game Framework/AutoEra/UI/B52/应用2系统流程")]
        public static void Apply2() => Apply(2);
        [MenuItem("Game Framework/AutoEra/UI/B52/应用3目录选择")]
        public static void Apply3() => Apply(3);
        [MenuItem("Game Framework/AutoEra/UI/B52/应用4生产现场")]
        public static void Apply4() => Apply(4);
        [MenuItem("Game Framework/AutoEra/UI/B52/应用5算法")]
        public static void Apply5() => Apply(5);
        [MenuItem("Game Framework/AutoEra/UI/B52/应用6中枢阅读")]
        public static void Apply6() => Apply(6);

        public static void Apply(int batch)
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请停止 Play Mode。");
            var manifest = JObject.Parse(File.ReadAllText(Manifest));
            foreach (JObject entry in (JArray)manifest["forms"])
            {
                if ((int)entry["batch"] != batch || entry["retained"]?.Value<bool>() == true) continue;
                AutoEraUiPrefabGenerator.ApplyExistingLayout((string)entry["contract"]);
            }
            if (batch == 5)
                foreach (string file in Directory.GetFiles(Change + "/art/items", "*.json")) ApplyItem(file);
            AssetDatabase.SaveAssets();
            File.WriteAllLines(Evidence + "/gate1-batch" + batch + ".txt", AutoEraContractGate1Checker.CheckAll());
            Debug.Log("[B52] 已应用结构批次 " + batch);
        }

        private static void ApplyItem(string file)
        {
            var doc = JObject.Parse(File.ReadAllText(file));
            string path = (string)doc["prefabPath"];
            var root = PrefabUtility.LoadPrefabContents(path);
            try { ApplyItemRect(root.transform, (JObject)doc["root"]); PrefabUtility.SaveAsPrefabAsset(root, path); }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        private static Vector2 ReadV(JToken v) => new Vector2((float)v[0], (float)v[1]);
        private static void ApplyItemRect(Transform t, JObject n)
        {
            var r = (RectTransform)t;
            r.anchorMin = ReadV(n["anchorMin"]); r.anchorMax = ReadV(n["anchorMax"]);
            r.pivot = ReadV(n["pivot"]); r.sizeDelta = ReadV(n["sizeDelta"]); r.anchoredPosition = ReadV(n["anchoredPosition"]);
            foreach (JObject child in (JArray)n["children"])
            {
                Transform target = t.Find((string)child["name"]);
                if (target == null) throw new InvalidOperationException("Item 路径缺失：" + child["name"]);
                ApplyItemRect(target, child);
            }
        }

        [MenuItem("Game Framework/AutoEra/UI/B52/导出全部结构预览")]
        public static void CaptureAll()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("请停止 Play Mode。");
            Directory.CreateDirectory(Evidence + "/previews");
            var manifest = JObject.Parse(File.ReadAllText(Manifest));
            var records = new JArray();
            foreach (JObject entry in (JArray)manifest["forms"])
            {
                var doc = JObject.Parse(File.ReadAllText((string)entry["contract"]));
                foreach (string page in entry["pages"].Values<string>())
                {
                    string output = Evidence + "/previews/" + entry["form"] + "-" + page + ".png";
                    Preview((string)doc["prefabPath"], page, output);
                    records.Add(new JObject { ["form"] = entry["form"], ["page"] = page, ["file"] = output, ["kind"] = "isolated-prefab-structure", ["resolution"] = "1920x1080" });
                }
            }
            File.WriteAllText(Evidence + "/preview-index.json", records.ToString());
            Debug.Log("[B52] 结构截图 " + records.Count + " 张。");
        }

        private static void Preview(string prefabPath, string page, string output)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            RenderTexture target = null; Texture2D pixels = null; var previous = RenderTexture.active;
            try
            {
                var root = new GameObject("B52Preview", typeof(RectTransform), typeof(Canvas));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
                var cameraGo = new GameObject("B52Camera", typeof(Camera));
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraGo, scene);
                var camera = cameraGo.GetComponent<Camera>(); camera.scene = scene;
                camera.orthographic = true; camera.orthographicSize = 540; camera.transform.position = new Vector3(0,0,-1000);
                camera.nearClipPlane = .1f; camera.farClipPlane = 2000; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.065f,.075f,.085f);
                var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 100;
                target = new RenderTexture(1920,1080,24); camera.targetTexture = target;
                var go = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath), root.transform);
                go.SetActive(true);
                foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.StartsWith("Panel_Page", StringComparison.Ordinal)) t.gameObject.SetActive(t.name == page);
                    if (t.name.StartsWith("Grp_", StringComparison.Ordinal) && t.name.EndsWith("State", StringComparison.Ordinal)) t.gameObject.SetActive(false);
                }
                Canvas.ForceUpdateCanvases();
                foreach (var text in go.GetComponentsInChildren<TMP_Text>(true)) text.ForceMeshUpdate();
                Canvas.ForceUpdateCanvases(); camera.Render(); RenderTexture.active = target;
                pixels = new Texture2D(1920,1080,TextureFormat.RGB24,false); pixels.ReadPixels(new Rect(0,0,1920,1080),0,0); pixels.Apply();
                File.WriteAllBytes(output,pixels.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                if (pixels != null) UnityEngine.Object.DestroyImmediate(pixels);
                // Unity logs an Error when a RenderTexture is released while it is
                // still assigned to Camera.targetTexture. Clear the binding before
                // releasing the capture target so batch preview export leaves the
                // editor console clean for the project checks.
                if (target != null)
                {
                    foreach (GameObject previewRoot in scene.GetRootGameObjects())
                    {
                        Camera previewCamera = previewRoot.GetComponent<Camera>();
                        if (previewCamera != null && previewCamera.targetTexture == target)
                            previewCamera.targetTexture = null;
                    }
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                }
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}
