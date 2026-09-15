using System;
using AutoEra.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor
{
    public static class InitialRegionHudBinder
    {
        [MenuItem("Game Framework/AutoEra/Bind Initial Region HUD")]
        public static void Bind()
        {
            const string path = "Assets/Game/Prefabs/UI/Operations/FieldHudForm.prefab";
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                FieldHudForm form = root.GetComponent<FieldHudForm>();
                if (form == null) throw new InvalidOperationException("FieldHudForm missing.");
                var serialized = new SerializedObject(form);
                Set(serialized, "_regionObjectPanel", Find(root, "Panel_ObjectQuick").gameObject);
                BindText(serialized, root, "_regionObjectSummary", "Art_Txt_HudObjectVisual");
                BindText(serialized, root, "_regionTimeSummary", "Art_Txt_HudTimeVisual");
                BindText(serialized, root, "_regionResourceSummary", "Art_Txt_HudResourcesVisual");
                BindText(serialized, root, "_regionAlertSummary", "Art_Txt_HudAlertVisual");
                string[] states = { "Art_HudEmptyState", "Art_HudErrorState", "Art_HudAsyncState" };
                SerializedProperty array = serialized.FindProperty("_regionAlternativeStates");
                array.arraySize = states.Length;
                for (int i = 0; i < states.Length; i++)
                    array.GetArrayElementAtIndex(i).objectReferenceValue = Find(root, states[i]).gameObject;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        private static void BindText(SerializedObject owner, GameObject root, string property, string name)
        {
            TMP_Text text = Find(root, name).GetComponent<TMP_Text>();
            if (text == null) throw new InvalidOperationException("TMP missing: " + name);
            Set(owner, property, text);
        }

        private static void Set(SerializedObject owner, string property, UnityEngine.Object value)
            => owner.FindProperty(property).objectReferenceValue = value;

        private static Transform Find(GameObject root, string name)
        {
            Transform result = null;
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
            {
                if (node.name != name) continue;
                if (result != null) throw new InvalidOperationException("Ambiguous UI node: " + name);
                result = node;
            }
            return result != null ? result : throw new InvalidOperationException("UI node missing: " + name);
        }
    }
}
