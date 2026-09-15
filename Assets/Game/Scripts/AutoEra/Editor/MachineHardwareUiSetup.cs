using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TMPro;
using AutoEra.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Editor
{
    /// <summary>Imports only the approved panel and sprites; never imports the authoring Canvas or shared fonts.</summary>
    public static class MachineHardwareUiSetup
    {
        public const string PanelPath = "Assets/Game/Prefabs/UI/Operations/MachineHardwarePanel.prefab";
        private const string SpriteRoot = "Assets/Game/Sprites/UI/Operations/MachineHardware/";
        private const string FontPath = "Assets/Game/Fonts/UI/SIMHEI SDF.asset";
        [Serializable] private sealed class Manifest { public string root; public Entry[] assets; }
        [Serializable] private sealed class Entry { public string path, guid, sha256, metaSha256; }

        [MenuItem("Tools/Auto Era UI/Import Approved Machine Hardware")]
        public static void ImportApproved()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new InvalidOperationException("Import requires an idle EditMode editor.");
            string project = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
            string manifestPath = Path.GetFullPath(Path.Combine(project,
                "../ArtResource/Docs/ArtPipeline/UIRequirements/B11_MachineHardware/CandidateManifest_V02.json"));
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null) throw new InvalidDataException("Existing Operations font is required; no font import is allowed.");
            var imports = new List<KeyValuePair<Entry, string>>();
            foreach (Entry entry in manifest.assets)
            {
                string target;
                const string marker = "/Sprites/";
                int markerIndex = entry.path.IndexOf(marker, StringComparison.Ordinal);
                if (markerIndex >= 0)
                    target = SpriteRoot + Regex.Replace(entry.path.Substring(markerIndex + marker.Length), @"(?:_LocalAlpha)?_V\d+(?=\.png$)", "");
                else if (entry.path.EndsWith("/B11_MachineHardwarePanel_V02.prefab", StringComparison.Ordinal)) target = PanelPath;
                else continue;
                string source = Path.Combine(manifest.root, entry.path);
                if (!Matches(source, entry.sha256) || !Matches(source + ".meta", entry.metaSha256))
                    throw new InvalidDataException("Approved source changed: " + entry.path);
                string existing = AssetDatabase.GUIDToAssetPath(entry.guid);
                if (!string.IsNullOrEmpty(existing) && existing != target)
                    throw new InvalidDataException("GUID collision: " + entry.guid + " at " + existing);
                if (File.Exists(target) || File.Exists(target + ".meta"))
                    throw new InvalidDataException("Destination already exists; refusing overwrite: " + target);
                imports.Add(new KeyValuePair<Entry, string>(entry, target));
            }
            if (imports.Count != 27) throw new InvalidDataException("Expected approved panel and 26 sprites.");
            foreach (var item in imports) EnsureFolder(Path.GetDirectoryName(item.Value).Replace('\\', '/'));
            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (var item in imports)
                {
                    // New external import only. No existing asset is moved, deleted, or overwritten.
                    File.Copy(Path.Combine(manifest.root, item.Key.path), item.Value, false);
                    File.Copy(Path.Combine(manifest.root, item.Key.path) + ".meta", item.Value + ".meta", false);
                    AssetDatabase.ImportAsset(item.Value, ImportAssetOptions.ForceSynchronousImport);
                }
            }
            finally { AssetDatabase.StopAssetEditing(); }
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var item in imports)
                if (AssetDatabase.AssetPathToGUID(item.Value) != item.Key.guid)
                    throw new InvalidDataException("Imported GUID mismatch: " + item.Value);
            GameObject root = PrefabUtility.LoadPrefabContents(PanelPath);
            try
            {
                root.name = "MachineHardwarePanel";
                foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    text.font = font;
                    text.fontSharedMaterial = font.material;
                    text.raycastTarget = false;
                }
                PrefabUtility.SaveAsPrefabAsset(root, PanelPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            Debug.Log("B11_IMPORT_OK|assets=27|sourceGuidPreserved=true|fontsModified=0");
        }

        private static bool Matches(string path, string expected)
        {
            if (!File.Exists(path)) return false;
            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return string.Equals(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", ""), expected, StringComparison.OrdinalIgnoreCase);
        }

        [MenuItem("Tools/Auto Era UI/Bind Machine Hardware")]
        public static void BindPanel()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                throw new InvalidOperationException("Binding requires idle EditMode.");
            GameObject root = PrefabUtility.LoadPrefabContents(PanelPath);
            try
            {
                var nodes = new Dictionary<string, Transform>();
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) nodes.Add(t.name, t);
                // Static visual fixtures are not runtime inventory; keep only the inactive reusable item template.
                foreach (string name in new[] { "B11_PickerRowArm", "B11_PickerRowCargo", "B11_PickerRowWater", "B11_PickerRowTransmitter" })
                    if (nodes.TryGetValue(name, out var fixture)) UnityEngine.Object.DestroyImmediate(fixture.gameObject);
                nodes["B11_PickerRowItemTemplate"].gameObject.SetActive(false);
                var rows = nodes["B11_PickerRows"].gameObject;
                var layout = rows.GetComponent<VerticalLayoutGroup>() ?? rows.AddComponent<VerticalLayoutGroup>();
                layout.childControlWidth = true; layout.childControlHeight = false;
                layout.childForceExpandWidth = true; layout.childForceExpandHeight = false; layout.spacing = 12;
                var fitter = rows.GetComponent<ContentSizeFitter>() ?? rows.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                CloneButton(nodes, "B11_Sensor1_Enable", "B11_Sensor2_Enable", "B11_Sensor2", new Vector2(348, -110), "启用");
                CloneButton(nodes, "B11_Effector1_Enable", "B11_Effector2_Enable", "B11_Effector2", new Vector2(348, -110), "启用");
                CloneButton(nodes, "B11_Rename", "B11_Close", "MachineHardwarePanel", new Vector2(1540, -213), "关闭");
                // Source enable controls are transparent hit areas. Give both occupied slots
                // the already-approved Choose button treatment rather than default black text.
                var choose = nodes["B11_Sensor2_Choose"].GetComponent<Button>();
                var chooseLabel = choose.GetComponentInChildren<TMP_Text>(true);
                foreach (string slotName in new[] { "B11_Sensor2", "B11_Effector2" })
                {
                    var slotObject = nodes[slotName].gameObject;
                    var selector = slotObject.GetComponent<Button>() ?? slotObject.AddComponent<Button>();
                    selector.targetGraphic = slotObject.GetComponent<Image>();
                    selector.targetGraphic.raycastTarget = true;
                    selector.transition = Selectable.Transition.None;
                }
                foreach (var button in root.GetComponentsInChildren<Button>(true))
                {
                    if (!button.name.EndsWith("_Enable", StringComparison.Ordinal)) continue;
                    var rect = (RectTransform)button.transform;
                    rect.anchoredPosition = new Vector2(568, -65);
                    rect.sizeDelta = new Vector2(205, 59);
                    var image = button.GetComponent<Image>();
                    image.sprite = null; image.color = Color.clear;
                    var art = button.transform.Find(button.name + "_Art");
                    if (art == null)
                    {
                        art = UnityEngine.Object.Instantiate(nodes["B11_Sensor2_Choose_Art"], button.transform, false);
                        art.name = button.name + "_Art"; art.SetAsFirstSibling();
                    }
                    button.targetGraphic = art.GetComponent<Image>(); button.colors = choose.colors;
                    var label = button.GetComponentInChildren<TMP_Text>(true);
                    label.font = chooseLabel.font; label.fontSharedMaterial = chooseLabel.fontSharedMaterial;
                    label.fontSize = chooseLabel.fontSize; label.color = chooseLabel.color;
                    label.alignment = TextAlignmentOptions.Center;
                    var labelRect = label.rectTransform;
                    labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
                    labelRect.offsetMin = new Vector2(6, 2); labelRect.offsetMax = new Vector2(-6, -2);
                }
                var panel = root.GetComponent<MachineHardwarePanel>() ?? root.AddComponent<MachineHardwarePanel>();
                if (root.GetComponent<CanvasGroup>() == null) root.AddComponent<CanvasGroup>();
                TMP_InputField input = root.GetComponentInChildren<TMP_InputField>(true);
                if (input == null)
                {
                    var go = new GameObject("B11_RenameInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
                    go.transform.SetParent(root.transform, false);
                    var rect = (RectTransform)go.transform; rect.sizeDelta = new Vector2(500, 52); rect.anchoredPosition = new Vector2(0, 420);
                    var text = UnityEngine.Object.Instantiate(nodes["B11_Identity"].gameObject, go.transform, false);
                    text.name = "B11_RenameInputText";
                    var textRect = (RectTransform)text.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = new Vector2(10, 2); textRect.offsetMax = new Vector2(-10, -2);
                    var label = text.GetComponent<TextMeshProUGUI>(); label.fontSize = 22; label.color = Color.black;
                    input = go.GetComponent<TMP_InputField>(); input.textViewport = rect; input.textComponent = label; input.characterLimit = 64;
                    input.lineType = TMP_InputField.LineType.SingleLine; go.SetActive(false);
                }
                var serialized = new SerializedObject(panel);
                var all = root.GetComponentsInChildren<Transform>(true);
                var array = serialized.FindProperty("_nodes"); array.arraySize = all.Length;
                for (int i = 0; i < all.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = all[i];
                serialized.FindProperty("_tabNormal").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + "Common/TabNormal.png");
                serialized.FindProperty("_tabSelected").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + "Common/TabSelected.png");
                serialized.FindProperty("_tabNormalText").colorValue = nodes["B11_TabOverview_Label"].GetComponent<TMP_Text>().color;
                serialized.FindProperty("_tabSelectedText").colorValue = nodes["B11_TabHardware_Label"].GetComponent<TMP_Text>().color;
                serialized.FindProperty("_rowNormal").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + "Picker/ListItemNormal.png");
                serialized.FindProperty("_rowSelected").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + "Picker/ListItemSelected.png");
                serialized.FindProperty("_rowDisabled").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + "Picker/ListItemDisabled.png");
                serialized.FindProperty("_renameInput").objectReferenceValue = input;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                root.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, PanelPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
            const string hudPath = "Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab";
            GameObject hud = PrefabUtility.LoadPrefabContents(hudPath);
            try
            {
                var existing = hud.GetComponentInChildren<MachineHardwarePanel>(true);
                if (existing == null)
                {
                    var instance = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PanelPath), hud.transform);
                    existing = instance.GetComponent<MachineHardwarePanel>();
                }
                var rect = (RectTransform)existing.transform;
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
                var form = new SerializedObject(hud.GetComponent<FieldHudForm>());
                form.FindProperty("_machinePanel").objectReferenceValue = existing;
                form.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(hud, hudPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(hud); }
            const string hubPath = "Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab";
            GameObject hub = PrefabUtility.LoadPrefabContents(hubPath);
            try
            {
                var data = new SerializedObject(hub.GetComponent<BaseCommandHubForm>());
                TMP_Text summary = null;
                foreach (var text in hub.GetComponentsInChildren<TMP_Text>(true)) if (text.name == "Txt_ObjectsState") summary = text;
                if (summary == null) throw new InvalidDataException("Missing approved object state label.");
                data.FindProperty("_machineRosterSummary").objectReferenceValue = summary;
                data.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SaveAsPrefabAsset(hub, hubPath);
            }
            finally { PrefabUtility.UnloadPrefabContents(hub); }
            Debug.Log("B11_BINDING_OK|formalPanel=true|runtimeInventoryFixtures=0");
        }
        private static void CloneButton(Dictionary<string, Transform> nodes, string source, string name, string parent, Vector2 position, string caption)
        {
            if (nodes.TryGetValue(name,out var existing)) { ((RectTransform)existing).anchoredPosition = position; return; }
            var clone = UnityEngine.Object.Instantiate(nodes[source].gameObject, nodes[parent], false);
            foreach (Transform t in clone.GetComponentsInChildren<Transform>(true)) t.name = t.name.Replace(source, name).Replace("(Clone)", "");
            clone.name = name; ((RectTransform)clone.transform).anchoredPosition = position;
            clone.GetComponentInChildren<TMP_Text>(true).text = caption;
        }
        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
