using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor
{
    public sealed class UiStructureContractEditModeTests
    {
        [TestCase("Assets/Game/Prefabs/UI/Operations/FieldHudDetailForm.prefab")]
        [TestCase("Assets/Game/Prefabs/UI/Operations/OperationDialogForm.prefab")]
        [TestCase("Assets/Game/Prefabs/UI/Operations/ProgressReportForm.prefab")]
        [TestCase("Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab")]
        [TestCase("Assets/Game/Prefabs/UI/System/SettingsForm.prefab")]
        public void AuthoredRootStretchesWithoutOwningAnotherScaler(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null, path);
            var rect = prefab.GetComponent<RectTransform>();
            Assert.That(rect, Is.Not.Null);
            Assert.That(rect.anchorMin, Is.EqualTo(Vector2.zero), path + " root anchorMin");
            Assert.That(rect.anchorMax, Is.EqualTo(Vector2.one), path + " root anchorMax");
            Assert.That(rect.offsetMin, Is.EqualTo(Vector2.zero), path + " root offsetMin");
            Assert.That(rect.offsetMax, Is.EqualTo(Vector2.zero), path + " root offsetMax");
            Assert.That(rect.pivot, Is.EqualTo(new Vector2(0.5f, 0.5f)), path + " root pivot");
            Assert.That(prefab.GetComponent<Canvas>(), Is.Null, "GF creates the runtime canvas.");
            Assert.That(prefab.GetComponentsInChildren<CanvasScaler>(true), Is.Empty, "GF root owns the shared scaler.");
            foreach (var transform in prefab.GetComponentsInChildren<Transform>(true))
                if (transform.name.StartsWith("Item_") && transform.name.EndsWith("Template"))
                    Assert.That(transform.gameObject.activeSelf, Is.False, path + ": " + transform.name);
        }
    }
}
