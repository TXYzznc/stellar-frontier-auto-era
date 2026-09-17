using System.Collections;
using System.Linq;
using AutoEra.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AutoEra.Tests.Editor
{
    public sealed class FrameworkGroupingDegradationEditModeTests
    {
        [UnityTest]
        public IEnumerator Startup_AppliesConfiguredGroups_AndUnconfiguredUiDoesNotBlockStartup()
        {
            if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene(
                    "Assets/Game/Scene/Launch.unity",
                    OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }

            yield return new EnterPlayMode();
            yield return VerifyConfiguredGroupsAndUiDegradationAfterDomainReload();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifyConfiguredGroupsAndUiDegradationAfterDomainReload()
        {
            float until = Time.realtimeSinceStartup + 30f;
            while (Time.realtimeSinceStartup < until && !UnityEngine.Object
                       .FindObjectsOfType<MainMenuForm>()
                       .Any(menu => menu.GetComponentInChildren<Button>().interactable))
            {
                yield return null;
            }

            var menuForm = UnityEngine.Object.FindObjectsOfType<MainMenuForm>()
                .SingleOrDefault(menu => menu.GetComponentInChildren<Button>().interactable);
            Assert.That(menuForm, Is.Not.Null, "Configured startup UI should be available.");

            var entity = GameEntry.GetComponent<EntityComponent>();
            var sound = GameEntry.GetComponent<SoundComponent>();
            var ui = GameEntry.GetComponent<UIComponent>();

            Assert.That(entity, Is.Not.Null);
            Assert.That(sound, Is.Not.Null);
            Assert.That(ui, Is.Not.Null);
            Assert.That(entity.HasEntityGroup("Default"), Is.True);
            Assert.That(entity.HasEntityGroup("Effect"), Is.True);
            Assert.That(entity.HasEntityGroup("Persistent"), Is.True);
            Assert.That(sound.HasSoundGroup("Music"), Is.True);
            Assert.That(sound.HasSoundGroup("Sound"), Is.True);
            Assert.That(ui.HasUIGroup("Default"), Is.True);
            Assert.That(ui.HasUIGroup("Dialog"), Is.True);
            Assert.That(ui.HasUIGroup("Overlay"), Is.True);

            int serialId = -2;
            Assert.DoesNotThrow(() => serialId = UIExtension.OpenUIForm(ui, (UIViews)6999));
            Assert.That(serialId, Is.EqualTo(-1));
            Assert.That(UnityEngine.Object.FindObjectsOfType<MainMenuForm>()
                .Any(menu => menu.GetComponentInChildren<Button>().interactable), Is.True);
        }
    }
}