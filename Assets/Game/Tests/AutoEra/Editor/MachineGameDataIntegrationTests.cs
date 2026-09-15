using System.Collections;
using AutoEra.DataTable;
using AutoEra.Machines;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;

namespace AutoEra.Tests.Editor
{
    public sealed class MachineGameDataIntegrationTests
    {
        [UnityTest]
        public IEnumerator Launch_LoadsGeneratedMachineCatalogThroughGameFramework()
        {
            if (!UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/Game/Scene/Launch.unity").isLoaded &&
                string.IsNullOrEmpty(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }
            Assert.That(UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/Game/Scene/Launch.unity").isLoaded,
                Is.True, "Run with the saved Launch scene loaded; never overwrite a user's dirty scene.");
            yield return new EnterPlayMode();
            yield return VerifyAfterReload();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifyAfterReload()
        {
            float until = Time.realtimeSinceStartup + 30;
            while (Time.realtimeSinceStartup < until && !MachineCatalog.IsGameDataLoaded) yield return null;
            var catalog = MachineCatalog.FromLoadedGameData();
            Assert.That(catalog.TryGetMachine(10011, out var wheel), Is.True);
            Assert.That(wheel.BaseCapacity, Is.EqualTo(30));
            Assert.That(catalog.TryGetComponent(20012, out var core), Is.True);
            Assert.That(core.ComputeCapacity, Is.EqualTo(75)); Assert.That(core.LogicCapacity, Is.EqualTo(50));
            Assert.That(catalog.TryGetReadyPrefab(20012, out _), Is.False, "Unproduced core art is not a runtime prefab.");
            Assert.That(catalog.TryGetReadyPrefab(10011, out string prefab), Is.True);
            Assert.That(prefab, Is.EqualTo("Machines/WheeledCarrier"));
            Assert.That(FirstVersionCatalog.FromLoadedGameData().Count, Is.EqualTo(40));
            for (int level = 1; level <= 2; level++)
            {
                Assert.That(catalog.TryGetMachine(10020 + level, out var fixedCarrier), Is.True);
                Assert.That(fixedCarrier.BaseCapacity, Is.EqualTo(level == 1 ? 10 : 12));
                Assert.That(fixedCarrier.MaximumIntegrity, Is.EqualTo(level == 1 ? 100 : 120));
                Assert.That(catalog.TryGetComponent(22060 + level, out var cargo), Is.True);
                Assert.That(cargo.AddedCapacity, Is.EqualTo(level == 1 ? 30 : 36));
                Assert.That(catalog.TryGetReadyPrefab(22060 + level, out _), Is.True);
            }
        }

        [UnityTearDown]
        public IEnumerator CleanupPlayMode()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }
    }
}
