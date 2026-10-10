using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Time;
using GameFramework;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AutoEra.Tests.Editor
{
    /// <summary>Actual GF UI lifecycle with explicit fixture worlds. Editor evidence is not a Player performance budget.</summary>
    public sealed class UiLifetimeStressPlayModeTests
    {
        private const string Evidence = "openspec/changes/b50-p8007-p8011-p8012-ui-performance-closure/evidence";
        private static UnityGameFramework.Runtime.UIComponent Ui => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
        private static UnityGameFramework.Runtime.DebuggerComponent Debugger => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.DebuggerComponent>();
        private static readonly UIViews[] Views = { UIViews.FieldHudDetailForm, UIViews.OperationDialogForm,
            UIViews.ProgressReportForm, UIViews.BaseCommandHubForm, UIViews.SettingsForm };

        [UnityTest, Timeout(600000)]
        public IEnumerator FiveRealForms_ColdCancelAnd100CyclesEach_ReleaseFocusInputParamsAndSubscriptions()
        {
            const string launch = "Assets/Game/Scene/Launch.unity";
            if (!SceneManager.GetSceneByPath(launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch, OpenSceneMode.Additive));
            yield return new EnterPlayMode();
            yield return Verify();
        }

        [UnityTearDown]
        public IEnumerator StopPlayMode()
        {
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
        }

        private static IEnumerator Verify()
        {
            yield return Wait(() => MachineCatalog.IsGameDataLoaded && Ui != null && Ui.HasUIGroup("Default") && Find<MainMenuForm>() != null);
            foreach (var form in Ui.GetAllLoadedUIForms()) if (form.Logic is MainMenuForm) Ui.CloseUIForm(form.SerialId);
            yield return null;
            Assert.That(Screen.width, Is.EqualTo(1920));
            Assert.That(Screen.height, Is.EqualTo(1080));
            Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.False);
            string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "AutoEraB50Ui-" + Guid.NewGuid().ToString("N")));
            var focus = new GameObject("B50FocusFixture", typeof(RectTransform), typeof(Button));
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory(), null, new SaveSlotService(root));
            var secondContext = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory(), null, new SaveSlotService(Path.Combine(root, "B")));
            var serials = new List<int>();
            bool debugger = Debugger != null && Debugger.ActiveWindow;
            if (Debugger != null) Debugger.ActiveWindow = false;
            try
            {
                Assert.That(context.TryCreateWorldSession(0, out var world), Is.True);
                world.Machines.Create(new MachineDefinition(1001, "会话A测试机", 1, 0, 1, 1, 30, true, true, 100));
                var session = AutoEraUiSession.ForWorld(context, world);
                Assert.That(secondContext.TryCreateWorldSession(0, out var secondWorld), Is.True);
                secondWorld.Machines.Create(new MachineDefinition(1001, "会话B测试机", 1, 0, 1, 1, 30, true, true, 100));
                int paramsBefore = References("UIParams"), loadsBefore = References("OpenUIFormInfo");
                int cold = AutoEraUiNavigator.Open(UIViews.SettingsForm, session);
                serials.Add(cold);
                Assert.That(cold, Is.GreaterThan(0));
                Assert.That(Ui.IsLoadingUIForm(cold), Is.True, "Cold form must actually traverse the asynchronous loader.");
                AutoEraUiNavigator.Close(cold);
                Assert.That(Ui.IsLoadingUIForm(cold), Is.False, "Navigation close must cancel the pending request.");
                yield return Wait(() => References("OpenUIFormInfo") <= loadsBefore);
                Assert.That(Ui.HasUIForm(cold), Is.False, "Cancelled request must not appear after its asset arrives.");
                Assert.That(References("UIParams"), Is.EqualTo(paramsBefore), "Cancelled request must return its original parameters.");
                AutoEraUiNavigator.Close(cold);
                int invalid = AutoEraUiNavigator.Open((UIViews)int.MaxValue, session);
                Assert.That(invalid, Is.EqualTo(AutoEraUiNavigator.InvalidSerialId));
                Assert.That(References("UIParams"), Is.EqualTo(paramsBefore), "Rejected configuration must return its original parameters.");
                foreach (var parentView in new[] { UIViews.BaseCommandHubForm, UIViews.FieldHudDetailForm })
                {
                    int parentId = AutoEraUiNavigator.Open(parentView, session);
                    serials.Add(parentId);
                    yield return Wait(() => Ui.HasUIForm(parentId));
                    yield return null;
                    int beforeChildLoads = References("OpenUIFormInfo");
                    var parent = Ui.GetUIForm(parentId).Logic;
                    bool opened = parent is BaseCommandHubForm hub ? hub.ShowHubPage(BaseCommandHubForm.PageEnergy)
                        : ((FieldHudDetailForm)parent).ShowSelectionPage(5);
                    Assert.That(opened, Is.True);
                    int[] children = Ui.GetAllLoadingUIFormSerialIds();
                    Assert.That(children.Length, Is.EqualTo(1), "First child must really be loading asynchronously.");
                    serials.Add(children[0]);
                    AutoEraUiNavigator.Close(parentId);
                    yield return Wait(() => References("OpenUIFormInfo") <= beforeChildLoads);
                    yield return null;
                    Assert.That(Ui.HasUIForm(children[0]) || Ui.IsLoadingUIForm(children[0]), Is.False);
                    Assert.That(References("UIParams"), Is.EqualTo(paramsBefore), "Parent cancellation must release the child's original parameters.");
                }
                yield return Stress(session, AutoEraUiSession.ForWorld(secondContext, secondWorld), focus, serials);
            }
            finally
            {
                if (Debugger != null) Debugger.ActiveWindow = debugger;
                foreach (int serial in serials)
                    if (Ui != null && (Ui.HasUIForm(serial) || Ui.IsLoadingUIForm(serial))) Ui.CloseUIForm(serial);
                context.Dispose();
                secondContext.Dispose();
                Object.Destroy(focus);
                string parent = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                Assert.That(Path.GetDirectoryName(root), Is.EqualTo(parent));
                Assert.That(Path.GetFileName(root), Does.StartWith("AutoEraB50Ui-"));
                if (Directory.Exists(root)) Directory.Delete(root, true);
            }
        }

        [Serializable]
        public sealed class Measurement
        {
            public string Environment = "Unity2022.3.62f3c1 WindowsEditor 1920x1080; fixture worlds; not a Development Player budget";
            public string Sampling = "Initial sample is after cancellation scenarios; parent forms may already be cached. Not cold launch timings.";
            public int CyclesPerForm, WarmFormObjects, FinalFormObjects, WarmUiNodes, FinalUiNodes, ParamsBefore, ParamsAfter;
            public int OldWorldListeners, NewWorldListeners, LiveFormsBefore, LiveFormsAfter;
            public FormSample[] Forms;
        }
        [Serializable]
        public sealed class FormSample
        {
            public string Form;
            public double InitialSampleMs, P50Ms, P95Ms, P99Ms;
            public double[] RepeatedOpenMs;
        }
        private static int Listeners(AutoEraUiSession session)
        {
            var field = session.World.Machines.GetType().GetField("Changed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (field.GetValue(session.World.Machines) as Delegate)?.GetInvocationList().Length ?? 0;
        }
        private static AutoEraUiSession Session(AutoEraUiFormBase form)
        {
            Assert.That(form.Params.TryGet(AutoEraUiParamKeys.Session, out UnityGameFramework.Runtime.VarObject value), Is.True);
            Assert.That(value.Value, Is.TypeOf<AutoEraUiSession>());
            return (AutoEraUiSession)value.Value;
        }
        private static int UiNodes()
        {
            int count = 0;
            foreach (var form in Object.FindObjectsOfType<AutoEraUiFormBase>(true))
                count += form.GetComponentsInChildren<Transform>(true).Length;
            return count;
        }
        private static IEnumerator Stress(AutoEraUiSession first, AutoEraUiSession second, GameObject focus, List<int> serials)
        {
            var measurement = new Measurement { CyclesPerForm = 100, Forms = new FormSample[Views.Length] };
            Assert.That(Listeners(first), Is.Zero);
            Assert.That(Listeners(second), Is.Zero);
            measurement.LiveFormsBefore = Ui.GetAllLoadedUIForms().Length;
            for (int i = 0; i < Views.Length; i++)
            {
                var sample = new FormSample { Form = Views[i].ToString(), RepeatedOpenMs = new double[100] };
                measurement.Forms[i] = sample;
                EventSystem.current.SetSelectedGameObject(focus);
                long started = System.Diagnostics.Stopwatch.GetTimestamp();
                int id = AutoEraUiNavigator.Open(Views[i], first);
                serials.Add(id);
                yield return Wait(() => Ui.HasUIForm(id));
                yield return null;
                sample.InitialSampleMs = (System.Diagnostics.Stopwatch.GetTimestamp() - started) * 1000d / System.Diagnostics.Stopwatch.Frequency;
                Assert.That(Session((AutoEraUiFormBase)Ui.GetUIForm(id).Logic).World, Is.SameAs(first.World));
                if (Ui.GetUIForm(id).Logic is SettingsForm settings)
                {
                    for (int page = 0; page < 3; page++)
                    {
                        Assert.That(settings.ShowSettingsPage(page), Is.True);
                        yield return null;
                        foreach (var node in settings.GetComponentsInChildren<Transform>(true))
                            if (node.name.StartsWith("Grp_") && node.name.EndsWith("State"))
                                Assert.That(node.gameObject.activeInHierarchy, Is.False, "Ready settings must show its content without a state card: " + node.name);
                        foreach (var slider in settings.GetComponentsInChildren<Slider>(true))
                        {
                            Assert.That(slider.fillRect, Is.Not.Null, slider.name);
                            Assert.That(slider.handleRect, Is.Not.Null, slider.name);
                            Assert.That(slider.targetGraphic, Is.Not.Null, slider.name);
                            Assert.That(slider.fillRect.rect.height, Is.EqualTo(8f).Within(0.1f), slider.name + " fill height");
                            Assert.That(slider.handleRect.rect.height, Is.EqualTo(24f).Within(0.1f), slider.name + " handle height");
                            if (slider.gameObject.activeInHierarchy)
                            {
                                Assert.That(slider.fillRect.rect.width,
                                    Is.EqualTo(((RectTransform)slider.fillRect.parent).rect.width * slider.normalizedValue).Within(0.1f), slider.name + " fill value");
                                AssertControlHit(slider);
                            }
                            Assert.That(slider.GetComponentInChildren<TMPro.TMP_Text>(true).text, Does.Not.Contain("—"), slider.name + " value label");
                        }
                        foreach (var toggle in settings.GetComponentsInChildren<Toggle>(true))
                        {
                            Assert.That(toggle.graphic, Is.Not.Null, toggle.name);
                            Assert.That(toggle.targetGraphic, Is.Not.Null, toggle.name);
                            if (!toggle.gameObject.activeInHierarchy) continue;
                            AssertControlHit(toggle);
                            bool original = toggle.isOn;
                            toggle.SetIsOnWithoutNotify(true);
                            yield return new WaitForSecondsRealtime(0.2f);
                            Assert.That(toggle.graphic.canvasRenderer.GetAlpha(), Is.EqualTo(1f), toggle.name + " checked");
                            toggle.SetIsOnWithoutNotify(false);
                            yield return new WaitForSecondsRealtime(0.2f);
                            Assert.That(toggle.graphic.canvasRenderer.GetAlpha(), Is.Zero, toggle.name + " unchecked");
                            toggle.SetIsOnWithoutNotify(original);
                        }
                        yield return Screenshot("settings-page-" + page + "-1920x1080.png");
                    }
                    settings.ShowSettingsPage(0);
                    yield return null;
                }
                yield return Screenshot("warm-" + Views[i] + "-1920x1080.png");
                AutoEraUiNavigator.Close(id);
                yield return null;
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.SameAs(focus));
            }
            measurement.WarmFormObjects = Object.FindObjectsOfType<AutoEraUiFormBase>(true).Length;
            measurement.WarmUiNodes = UiNodes();
            measurement.ParamsBefore = References("UIParams");
            for (int cycle = 0; cycle < 100; cycle++)
            {
                AutoEraUiSession session = cycle < 50 ? first : second;
                for (int i = 0; i < Views.Length; i++)
                {
                    EventSystem.current.SetSelectedGameObject(focus);
                    long started = System.Diagnostics.Stopwatch.GetTimestamp();
                    int id = AutoEraUiNavigator.Open(Views[i], session);
                    serials.Add(id);
                    yield return Wait(() => Ui.HasUIForm(id));
                    yield return null;
                    measurement.Forms[i].RepeatedOpenMs[cycle] = (System.Diagnostics.Stopwatch.GetTimestamp() - started) * 1000d / System.Diagnostics.Stopwatch.Frequency;
                    var form = (AutoEraUiFormBase)Ui.GetUIForm(id).Logic;
                    AutoEraUiNavigator.Close(serials[0]);
                    Assert.That(Ui.HasUIForm(id), Is.True, "Old cancelled serial must not close its replacement.");
                    Assert.That(Session(form).World, Is.SameAs(session.World), "Pooled form retained an old session.");
                    Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.EqualTo(form.BlocksWorldInput));
                    AutoEraUiNavigator.Close(id);
                    yield return null;
                    Assert.That(Ui.HasUIForm(id), Is.False);
                    Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.False);
                    Assert.That(EventSystem.current.currentSelectedGameObject, Is.SameAs(focus));
                    Assert.That(Listeners(first), Is.Zero, "Closed page retained old world subscription.");
                    Assert.That(Listeners(second), Is.Zero, "Closed page retained new world subscription.");
                    Assert.That(first.World.IsActive && second.World.IsActive, Is.True, "Closing UI must not dispose a world.");
                }
            }
            measurement.FinalFormObjects = Object.FindObjectsOfType<AutoEraUiFormBase>(true).Length;
            measurement.FinalUiNodes = UiNodes();
            measurement.ParamsAfter = References("UIParams");
            measurement.OldWorldListeners = Listeners(first);
            measurement.NewWorldListeners = Listeners(second);
            measurement.LiveFormsAfter = Ui.GetAllLoadedUIForms().Length;
            Assert.That(measurement.FinalFormObjects, Is.LessThanOrEqualTo(measurement.WarmFormObjects));
            Assert.That(measurement.FinalUiNodes, Is.LessThanOrEqualTo(measurement.WarmUiNodes), "Repeated opening leaked child GameObjects.");
            Assert.That(measurement.ParamsAfter, Is.EqualTo(measurement.ParamsBefore));
            Assert.That(measurement.LiveFormsAfter, Is.EqualTo(measurement.LiveFormsBefore));
            foreach (var sample in measurement.Forms)
            {
                var sorted = (double[])sample.RepeatedOpenMs.Clone();
                Array.Sort(sorted);
                sample.P50Ms = sorted[49]; sample.P95Ms = sorted[94]; sample.P99Ms = sorted[98];
            }
            Directory.CreateDirectory(Evidence);
            File.WriteAllText(Path.Combine(Evidence, "ui-lifetime-editor-measurement.json"), JsonUtility.ToJson(measurement, true));
        }

        private static void AssertControlHit(Selectable control)
        {
            Assert.That(control.GetComponent<Image>().raycastTarget, Is.True, control.name);
            var canvas = control.GetComponentInParent<Canvas>();
            var rect = (RectTransform)control.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Exists(hit => hit.gameObject == control.gameObject), Is.True, control.name + " root hit area");
        }
        private static int References(string typeName)
        {
            foreach (var info in ReferencePool.GetAllReferencePoolInfos()) if (info.Type.Name == typeName) return info.UsingReferenceCount;
            return 0;
        }
        private static IEnumerator Screenshot(string name)
        {
            yield return null;
            yield return null;
            string path = Path.GetFullPath(Path.Combine(Evidence, name));
            Directory.CreateDirectory(Evidence);
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return Wait(() => File.Exists(path));
            byte[] png = File.ReadAllBytes(path);
            int Size(int offset) => (png[offset] << 24) | (png[offset + 1] << 16) | (png[offset + 2] << 8) | png[offset + 3];
            Assert.That(Size(16), Is.EqualTo(1920));
            Assert.That(Size(20), Is.EqualTo(1080));
        }
        private static T Find<T>() where T : class
        {
            foreach (var form in Ui.GetAllLoadedUIForms()) if (form.Logic is T value) return value;
            return null;
        }
        private static IEnumerator Wait(Func<bool> condition)
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!condition() && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(condition(), Is.True, "Expected actual UI state within 30 seconds.");
        }
    }
}
