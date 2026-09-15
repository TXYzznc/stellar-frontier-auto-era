using System.Collections;
using System.Linq;
using AutoEra.Procedures;
using AutoEra.UI;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraStartupFlowEditModeTests
    {
        [UnityTest]
        public IEnumerator MissingSound_ReportsOwnedFailureWithoutBlockingOtherGroups()
        {
            if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }
            yield return new EnterPlayMode();
            yield return VerifySoundFailureAfterDomainReload();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifySoundFailureAfterDomainReload()
        {
            float until = Time.realtimeSinceStartup+30;
            while (Time.realtimeSinceStartup < until && !Object.FindObjectsOfType<MainMenuForm>().Any(m=>m.GetComponentInChildren<Button>().interactable)) yield return null;
            var sound = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.SoundComponent>();
            var events = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.EventComponent>();
            const string missing = "Assets/Game/Audio/B10MissingSoundForValidation.wav";
            var marker = new object();
            int failures=0;
            string receivedAsset=null,receivedGroup=null,diagnostic=null;
            System.EventHandler<GameFramework.Event.GameEventArgs> onFailure = (sender,args) => {
                var e=(UnityGameFramework.Runtime.PlaySoundFailureEventArgs)args;
                if (!ReferenceEquals(e.UserData,marker)) return;
                failures++; receivedAsset=e.SoundAssetName;receivedGroup=e.SoundGroupName;diagnostic=e.ErrorMessage;
            };
            int before=events.Count(UnityGameFramework.Runtime.PlaySoundFailureEventArgs.EventId);
            events.Subscribe(UnityGameFramework.Runtime.PlaySoundFailureEventArgs.EventId,onFailure);
            try
            {
                Assert.That(sound.HasSoundGroup("Sound") && sound.HasSoundGroup("Music"),Is.True);
                sound.PlaySound(missing,"Sound",marker);
                until=Time.realtimeSinceStartup+15;
                while(Time.realtimeSinceStartup<until && failures==0) yield return null;
                Assert.That(failures,Is.EqualTo(1));
                Assert.That(receivedAsset,Is.EqualTo(missing));
                Assert.That(receivedGroup,Is.EqualTo("Sound"));
                Assert.That(diagnostic,Is.Not.Null.And.Not.Empty);
                Assert.That(sound.HasSoundGroup("Music"),Is.True);
                TestContext.WriteLine("Expected missing sound: "+receivedAsset+" / "+receivedGroup+" / "+diagnostic);
            }
            finally {events.Unsubscribe(UnityGameFramework.Runtime.PlaySoundFailureEventArgs.EventId,onFailure);}
            Assert.That(events.Count(UnityGameFramework.Runtime.PlaySoundFailureEventArgs.EventId),Is.EqualTo(before));
            Object.FindObjectsOfType<MainMenuForm>().Single().GetComponentInChildren<Button>().onClick.Invoke();
            until=Time.realtimeSinceStartup+30;
            while(Time.realtimeSinceStartup<until && Object.FindObjectsOfType<InitialRegionEntity>().Count(e=>e.Available)!=7) yield return null;
            Assert.That(Object.FindObjectsOfType<InitialRegionEntity>().Count(e=>e.Available),Is.EqualTo(7));
            Assert.That(Object.FindObjectsOfType<FieldHudForm>().Length,Is.EqualTo(1));
            Assert.DoesNotThrow(AutoEra.Application.AutoEraRuntimeSettings.ValidateLoadedStartupData);
        }

        [UnityTest]
        public IEnumerator FrameworkRestartDuringSceneLoad_ReleasesOldSessionAndAllowsFreshEntry()
        {
            if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }
            yield return new EnterPlayMode();
            yield return VerifyRestartAfterDomainReload();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifyRestartAfterDomainReload()
        {
            float until = Time.realtimeSinceStartup + 30;
            while (Time.realtimeSinceStartup < until && !Object.FindObjectsOfType<MainMenuForm>().Any(m => m.GetComponentInChildren<Button>().interactable)) yield return null;
            var menu = Object.FindObjectsOfType<MainMenuForm>().Single();
            menu.GetComponentInChildren<Button>().onClick.Invoke();
            var procedures = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.ProcedureComponent>();
            var world = procedures.GetProcedure<AutoEraWorldProcedure>();
            var field = typeof(AutoEraWorldProcedure).GetField("_context",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            AutoEra.Application.AutoEraApplicationContext old = null;
            until = Time.realtimeSinceStartup + 15;
            while (Time.realtimeSinceStartup < until)
            {
                old = (AutoEra.Application.AutoEraApplicationContext)field.GetValue(world);
                if (old != null && old.ActiveWorldSession != null) break;
                yield return null;
            }
            Assert.That(old,Is.Not.Null);
            var session = old.ActiveWorldSession;
            Assert.That(session,Is.Not.Null);
            string path = "Assets/Game/Scene/InitialRegion.unity";
            Assert.That(UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.SceneComponent>().SceneIsLoading(path),Is.True,
                "Restart must occur during the real in-flight load.");
            UnityGameFramework.Runtime.GameEntry.Shutdown(UnityGameFramework.Runtime.ShutdownType.Restart);
            until = Time.realtimeSinceStartup + 45;
            do { yield return null; }
            while (Time.realtimeSinceStartup < until && (!old.IsDisposed || !Object.FindObjectsOfType<MainMenuForm>().Any(m => m.GetComponentInChildren<Button>().interactable)));
            Assert.That(old.IsDisposed,Is.True);
            Assert.That(session.IsActive,Is.False);
            Assert.That(session.ObjectRegistry.Count,Is.Zero);
            Assert.That(Object.FindObjectsOfType<MainMenuForm>().Length,Is.EqualTo(1));
            Object.FindObjectsOfType<MainMenuForm>().Single().GetComponentInChildren<Button>().onClick.Invoke();
            until = Time.realtimeSinceStartup + 30;
            while (Time.realtimeSinceStartup < until && Object.FindObjectsOfType<InitialRegionEntity>().Count(e => e.Available) != 7) yield return null;
            Assert.That(Object.FindObjectsOfType<InitialRegionEntity>().Count(e => e.Available),Is.EqualTo(7));
            TestContext.WriteLine("Real framework restart while loading: old context/session released, fresh entry has seven entities.");
        }

        [UnityTest]
        public IEnumerator HalfInitializedRegionFailure_ReleasesRegistryAndSubscription()
        {
            if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }
            yield return new EnterPlayMode();
            yield return VerifyHalfInitializationAfterDomainReload();
            yield return new ExitPlayMode();
        }

        private static IEnumerator VerifyHalfInitializationAfterDomainReload()
        {
            float until = Time.realtimeSinceStartup + 30;
            while (Time.realtimeSinceStartup < until && !Object.FindObjectsOfType<MainMenuForm>().Any(m => m.GetComponentInChildren<Button>().interactable)) yield return null;
            var node = new GameObject("B10HalfInitializationFixture");
            var entry = node.AddComponent<InitialRegionScene>();
            var serialized = new SerializedObject(entry);
            serialized.FindProperty("_objects").arraySize = 1;
            serialized.FindProperty("_entityPrefabs").arraySize = 1;
            serialized.FindProperty("_entityPrefabs").GetArrayElementAtIndex(0).stringValue = "UnusedMissingSeed";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var events = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.EventComponent>();
            int before = events.Count(UnityGameFramework.Runtime.ShowEntityFailureEventArgs.EventId);
            using (var context = new AutoEra.Application.AutoEraApplicationCompositionRoot().Create())
            {
                Assert.That(context.TryCreateWorldSession(0,out var session),Is.True);
                Assert.Throws<System.InvalidOperationException>(() => entry.InitializeRuntime(session,()=>Assert.Fail("Must not become ready"),_=>{}));
                Assert.That(entry.Region,Is.Null);
                Assert.That(session.ObjectRegistry.Count,Is.Zero);
                Assert.That(events.Count(UnityGameFramework.Runtime.ShowEntityFailureEventArgs.EventId),Is.EqualTo(before));
                context.ReleaseActiveWorldSession();
                Assert.That(session.IsActive,Is.False);
                Assert.That(context.TryCreateWorldSession(0,out _),Is.True);
            }
            Object.Destroy(node);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CancelledSceneLoad_CompletesWithoutCallingOldOwner_ThenFreshLoadSucceeds()
        {
            if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }
            yield return new EnterPlayMode();
            yield return VerifyCancelledLoadAfterDomainReload();
            yield return new ExitPlayMode();
        }

        // Allocate callback closures after EnterPlayMode's domain reload, not in its outer iterator.
        private static IEnumerator VerifyCancelledLoadAfterDomainReload()
        {
            TestContext.WriteLine("B10 cancellation fixture v4: entered post-domain-reload helper.");
            float until = Time.realtimeSinceStartup + 30;
            while (Time.realtimeSinceStartup < until && !Object.FindObjectsOfType<MainMenuForm>().Any(m => m.GetComponentInChildren<Button>().interactable)) yield return null;
            Assert.That(Object.FindObjectsOfType<MainMenuForm>().Any(m => m.GetComponentInChildren<Button>().interactable), Is.True);
            var scenes = Object.FindObjectOfType<UnityGameFramework.Runtime.SceneComponent>();
            Assert.That(scenes, Is.Not.Null, "Live Launch Scene component is required.");
            const string relative = "InitialRegion";
            string path = EditorBuildSettings.scenes.Single(s => s.enabled && s.path.EndsWith("/" + relative + ".unity", System.StringComparison.Ordinal)).path;
            int oldCallbacks = 0, freshCallbacks = 0;
            using (var flow = new AutoEra.Application.AutoEraSceneFlow())
            {
                TestContext.WriteLine("B10 cancellation: live menu and scene component ready; invoking GF load.");
                flow.Load(relative, _ => oldCallbacks++, _ => oldCallbacks++);
                TestContext.WriteLine("B10 cancellation: GF load returned; checking in-flight state.");
                Assert.That(scenes.SceneIsLoading(path), Is.True, "Must cancel a real in-flight GF request.");
                flow.Cancel();
                until = Time.realtimeSinceStartup + 30;
                do { yield return null; }
                while (Time.realtimeSinceStartup < until && (scenes.SceneIsLoading(path) || scenes.SceneIsUnloading(path) || UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path).isLoaded));
                Assert.That(scenes.SceneIsLoading(path) || scenes.SceneIsUnloading(path), Is.False);
                Assert.That(UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path).isLoaded, Is.False);
                Assert.That(oldCallbacks, Is.Zero, "Cancelled completion must not activate the old owner.");
                string error = null;
                flow.Load(relative, _ => freshCallbacks++, message => error = message);
                until = Time.realtimeSinceStartup + 30;
                while (Time.realtimeSinceStartup < until && freshCallbacks == 0 && error == null) yield return null;
                Assert.That(error, Is.Null);
                Assert.That(freshCallbacks, Is.EqualTo(1));
                Assert.That(oldCallbacks, Is.Zero);
            }
            until = Time.realtimeSinceStartup + 30;
            while (Time.realtimeSinceStartup < until && scenes.SceneIsUnloading(path)) yield return null;
            Assert.That(UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path).isLoaded, Is.False);
            TestContext.WriteLine("B10 cancellation: old callbacks=0, fresh callbacks=1, owned scene released.");
        }

        [UnityTest]
        public IEnumerator MissingWorldScene_ReturnsToExistingMenu_AndRetryRecovers()
        {
            if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity",OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }
            yield return new EnterPlayMode();
            float until = Time.realtimeSinceStartup + 30;
            MainMenuForm menu = null;
            while (Time.realtimeSinceStartup < until)
            {
                menu = Object.FindObjectsOfType<MainMenuForm>().SingleOrDefault();
                if (menu != null && menu.GetComponentInChildren<Button>().interactable) break;
                yield return null;
            }
            Assert.That(menu, Is.Not.Null);
            var config = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.ConfigComponent>();
            string original = config.GetString("AutoEra.Scene.World");
            config.RemoveConfig("AutoEra.Scene.World");
            config.AddConfig("AutoEra.Scene.World",false,0,0,"B10MissingSceneForValidation");
            menu.GetComponentInChildren<Button>().onClick.Invoke();
            until = Time.realtimeSinceStartup + 30;
            bool sawError = false;
            while (Time.realtimeSinceStartup < until)
            {
                menu = Object.FindObjectsOfType<MainMenuForm>().SingleOrDefault();
                sawError = menu != null && menu.StatusText.Contains("进入区域失败");
                if (sawError && menu.GetComponentInChildren<Button>().interactable) break;
                yield return null;
            }
            Assert.That(sawError, Is.True, "Original failure must not be replaced by already-loaded menu error.");
            Assert.That(Object.FindObjectsOfType<InitialRegionEntity>().Count(e => e.Available), Is.Zero);
            config.RemoveConfig("AutoEra.Scene.World");
            config.AddConfig("AutoEra.Scene.World",false,0,0,original);
            menu.GetComponentInChildren<Button>().onClick.Invoke();
            until = Time.realtimeSinceStartup + 30;
            while (Time.realtimeSinceStartup < until && Object.FindObjectsOfType<InitialRegionEntity>().Count(e => e.Available) != 7) yield return null;
            Assert.That(Object.FindObjectsOfType<InitialRegionEntity>().Count(e => e.Available), Is.EqualTo(7));
            yield return new ExitPlayMode();
        }

        [UnityTest]
        public IEnumerator Launch_Menu_World_Menu_World_ReleasesEntities()
        {
            if (string.IsNullOrEmpty(EditorSceneManager.GetActiveScene().path))
            {
                // Test Runner supplies an empty isolation scene, not the editor's previously active Launch.
                var launch = EditorSceneManager.OpenScene("Assets/Game/Scene/Launch.unity", OpenSceneMode.Additive);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(launch);
            }
            Assert.That(EditorSceneManager.GetActiveScene().path, Is.EqualTo("Assets/Game/Scene/Launch.unity"));
            Assert.That(EditorSceneManager.GetActiveScene().isDirty, Is.False);
            yield return new EnterPlayMode();
            for (int cycle = 0; cycle < 2; cycle++)
            {
                float until = Time.realtimeSinceStartup + 30;
                MainMenuForm menu = null;
                while (Time.realtimeSinceStartup < until)
                {
                    menu = Object.FindObjectsOfType<MainMenuForm>().SingleOrDefault();
                    if (menu != null && menu.GetComponentInChildren<Button>().interactable) break;
                    yield return null;
                }
                Assert.That(menu, Is.Not.Null);
                int acceptedRequests = 0;
                menu.EnterRequested += () => acceptedRequests++;
                var enterButton = menu.GetComponentInChildren<Button>();
                enterButton.onClick.Invoke();
                enterButton.onClick.Invoke();
                Assert.That(acceptedRequests, Is.EqualTo(1), "Same-frame repeated clicks must submit once.");
                Assert.That(enterButton.interactable, Is.False);
                until = Time.realtimeSinceStartup + 30;
                InitialRegionScene region = null;
                while (Time.realtimeSinceStartup < until)
                {
                    region = Object.FindObjectsOfType<InitialRegionScene>().SingleOrDefault();
                    if (region != null && region.Region != null && region.Region.Count == 7 && Object.FindObjectsOfType<FieldHudForm>().Length == 1) break;
                    yield return null;
                }
                Assert.That(region, Is.Not.Null);
                Assert.That(region.Region.Count, Is.EqualTo(7));
                Assert.That(Object.FindObjectsOfType<InitialRegionEntity>().Count(e => e.Available), Is.EqualTo(7));
                Assert.That(Object.FindObjectsOfType<RegionObjectView>().All(v => v.Model != null), Is.True);
                if (cycle == 0)
                {
                    Assert.That(EditorApplication.ExecuteMenuItem("Game Framework/AutoEra/Runtime Evidence/Open Command Hub"), Is.True);
                    until = Time.realtimeSinceStartup + 15;
                    while (Time.realtimeSinceStartup < until && Object.FindObjectsOfType<BaseCommandHubForm>().Length == 0) yield return null;
                    Assert.That(Object.FindObjectsOfType<BaseCommandHubForm>().Length, Is.EqualTo(1));
                    Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.True);
                    long before = region.WorldMilliseconds;
                    float started = Time.realtimeSinceStartup;
                    yield return new WaitForSecondsRealtime(.6f);
                    double elapsedMilliseconds = (Time.realtimeSinceStartup - started) * 1000d;
                    Assert.That(region.WorldMilliseconds - before, Is.GreaterThan(400));
                    Assert.That(region.WorldMilliseconds - before, Is.EqualTo(elapsedMilliseconds).Within(200));
                    Assert.That(AutoEraUiRuntime.DispatchIntent(AutoEraUiIntent.Cancel), Is.True);
                    yield return null;
                    Assert.That(Object.FindObjectsOfType<BaseCommandHubForm>().Length, Is.Zero);
                }
                var procedures = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.ProcedureComponent>();
                ((AutoEraWorldProcedure)procedures.GetProcedure<AutoEraWorldProcedure>()).RequestReturnToMenu();
                until = Time.realtimeSinceStartup + 30;
                while (Time.realtimeSinceStartup < until && Object.FindObjectsOfType<InitialRegionEntity>().Any(e => e.Available)) yield return null;
                Assert.That(Object.FindObjectsOfType<InitialRegionEntity>().Count(e => e.Available), Is.Zero);
                Assert.That(Object.FindObjectsOfType<FieldHudForm>().Length, Is.Zero);
            }
            yield return new ExitPlayMode();
        }

        [UnityTearDown]
        public IEnumerator StopOnFailure()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }
    }
}
