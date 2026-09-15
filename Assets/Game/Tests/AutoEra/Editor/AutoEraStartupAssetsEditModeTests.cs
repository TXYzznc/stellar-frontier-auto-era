using System.IO;
using AutoEra.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraStartupAssetsEditModeTests
    {
        [Test]
        public void StartupAssets_AreRegisteredAndHaveConcreteBindings()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/UI/Startup/MainMenuForm.prefab");
            Assert.That(prefab, Is.Not.Null);
            var form = prefab.GetComponent<MainMenuForm>();
            Assert.That(form, Is.Not.Null);
            var serialized = new SerializedObject(form);
            Assert.That(serialized.FindProperty("_enterButton").objectReferenceValue, Is.Not.Null);
            Assert.That(serialized.FindProperty("_status").objectReferenceValue, Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Game/Scene/MainMenu.unity"), Is.Not.Null);
            Assert.That((int)UIViews.MainMenuForm, Is.EqualTo(6000));
            var table = new UITable();
            bool found = false;
            foreach (string line in File.ReadAllLines("Assets/Game/DataTable/Core/UITable.txt"))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                Assert.That(table.ParseDataRow(line, null), Is.True);
                if (table.Id != 6000) continue;
                found = true;
                Assert.That(table.UIPrefab, Is.EqualTo("Startup/MainMenuForm"));
                Assert.That(table.EscapeClose, Is.False);
                Assert.That(table.UIGroupId, Is.EqualTo(1));
            }
            Assert.That(found, Is.True);
        }

        [Test]
        public void LanguageTable_TrailingEmptyIconRemainsParseable()
        {
            int count = 0;
            foreach (string line in File.ReadAllLines("Assets/Game/DataTable/Core/LanguagesTable.txt"))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                var row = new LanguagesTable();
                Assert.That(row.ParseDataRow(line, null), Is.True);
                Assert.That(row.LanguageKey, Is.Not.Empty);
                count++;
            }
            Assert.That(count, Is.GreaterThan(0));
        }
    }
}
