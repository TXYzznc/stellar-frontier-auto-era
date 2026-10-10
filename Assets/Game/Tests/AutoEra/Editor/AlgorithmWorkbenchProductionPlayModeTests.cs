using System;
using System.Collections;
using System.IO;
using AutoEra.Algorithms;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Region;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmWorkbenchProductionPlayModeTests
    {
        private static UnityGameFramework.Runtime.UIComponent Ui => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
        private static UnityGameFramework.Runtime.DebuggerComponent Debugger => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.DebuggerComponent>();
        private const string Evidence = "openspec/changes/b42-p3010-p3011-p3013-p3014-algorithm-workbench-closure/evidence";
        [UnityTest, Timeout(600000)]
        public IEnumerator TemplateBindingApplyClose_UsesFormalHostAndRealNavigationTask()
        {
            const string launch = "Assets/Game/Scene/Launch.unity";
            if (!SceneManager.GetSceneByPath(launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch, OpenSceneMode.Additive));
            yield return new EnterPlayMode(); yield return Verify(); yield return new ExitPlayMode();
        }
        private static IEnumerator Verify()
        {
            yield return Wait(() => MachineCatalog.IsGameDataLoaded && Ui != null && Ui.HasUIGroup("Default"));
            const string path = "Assets/Game/Scene/InitialRegion.unity";
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!loading.isDone) yield return null;
            var scene = SceneManager.GetSceneByPath(path); InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            bool debugger = Debugger != null && Debugger.ActiveWindow;
            if (Debugger != null) Debugger.ActiveWindow = false;
            using (var context = new AutoEraApplicationCompositionRoot().Create())
            {
                try
                {
                    context.TryCreateWorldSession(0, out var session);
                    bool ready = false; string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    yield return Wait(() => ready || failure != null); Assert.That(failure, Is.Null);
                    var catalog = MachineCatalog.FromLoadedGameData();
                    catalog.TryGetMachine(10011, out var definition); catalog.TryGetComponent(20011, out var coreDef);
                    catalog.TryGetComponent(21011, out var sensorDef);
                    var machine = session.Machines.Create(definition);
                    var core = session.Machines.CreateComponent(coreDef); var sensor = session.Machines.CreateComponent(sensorDef);
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, sensor.Id, 0), Is.EqualTo(MachineManagementResult.Completed));
                    using (var flow = new MachineDeploymentFlow(session, entry.Region))
                    {
                        Assert.That(flow.TryBegin(machine.Id, out var reason), Is.True, reason); flow.Preview.Move(new Vector2(20, -25));
                        Assert.That(flow.TryCommit(out _, out reason), Is.True, reason);
                    }
                    Assert.That(entry.TrySpawnMachine(machine.Id, out var spawn), Is.True, spawn);
                    yield return Wait(() => entry.FindMachineView(machine.Id) != null);
                    Assert.That(entry.MachineRuntimes.TryGet(machine.Id, out var runtime), Is.True);
                    entry.Region.Select(machine.Id, false);
                    machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true, true); machine.SetRunState(ManagementOrigin.Field, MachineRunState.Running);
                    var target = entry.Region.Register(AutoEra.World.Identity.PersistentObjectKind.Building, "工作台输入目标", new Vector2(24, -25), Vector2.one);
                    target.SetPublicState("可采集", 7);
                    // Test input preparation: a saved player template. No runtime, adapter, sensor, task or executor is injected.
                    ulong template = session.AlgorithmTemplates.Save("工作台导航验收", Template()); Assert.That(template, Is.Not.Zero);
                    var uiSession = AutoEraUiSession.ForWorld(context, session, entry.Region, entry.GetComponent<AutoEra.Input.RegionInputModule>(), entry.MachineRuntimes);
                    int emptyId = AutoEraUiNavigator.Open(UIViews.AlgorithmEditorForm, uiSession, new AutoEraUiPageRequest(AlgorithmEditorForm.PageEditor));
                    yield return Wait(() => Ui.HasUIForm(emptyId)); yield return Screenshot("workbench-empty-1920x1080.png");
                    Ui.CloseUIForm(emptyId);
                    int libraryId = AutoEraUiNavigator.Open(UIViews.AlgorithmLibraryForm, uiSession, new AutoEraUiPageRequest(AlgorithmLibraryForm.PagePlayerTemplates));
                    yield return Wait(() => Ui.HasUIForm(libraryId));
                    var library = (AlgorithmLibraryForm)Ui.GetUIForm(libraryId).Logic;
                    ClickRow(library.PlayerTemplatesCatalogContent, "工作台导航验收"); yield return null;
                    Assert.That(library.PlayerTemplatesCreateButton.interactable, Is.True); library.PlayerTemplatesCreateButton.onClick.Invoke();
                    yield return Wait(() => FindForm<AlgorithmEditorForm>() != null);
                    var editor = FindForm<AlgorithmEditorForm>();
                    Assert.That(runtime.Instances.ListInstances().Length, Is.EqualTo(1));
                    ulong instance = runtime.Instances.ListInstances()[0].Id;
                    editor.AlgorithmEditorBindButton.onClick.Invoke(); yield return Wait(() => FindForm<AlgorithmBindingForm>() != null);
                    var bindings = FindForm<AlgorithmBindingForm>(); ClickRow(bindings.PendingBindingsBindingsContent, "传感器");
                    yield return Wait(() => FindForm<NodeComponentPickerForm>() != null);
                    var picker = FindForm<NodeComponentPickerForm>(); ClickRow(picker.NodeComponentPickerCandidatesContent, null); yield return null;
                    ClickRow(picker.NodeComponentPickerCandidatesContent, "工作台输入目标");
                    yield return Wait(() => FindForm<NodeComponentPickerForm>() == null);
                    var bound = runtime.Instances.ReadDraft(instance).Bindings[0];
                    Assert.That(bound.ComponentId, Is.EqualTo(sensor.Id.Value)); Assert.That(bound.TargetId, Is.EqualTo(target.Id.Value)); Assert.That(bound.Generation, Is.GreaterThan(0));
                    bindings.BackButton.onClick.Invoke(); yield return Wait(() => FindForm<AlgorithmBindingForm>() == null);
                    Assert.That(editor.AlgorithmEditorApplyButton.interactable, Is.True); editor.AlgorithmEditorApplyButton.onClick.Invoke(); yield return null;
                    Assert.That(runtime.Instances.ListInstances()[0].AppliedRevision, Is.EqualTo(runtime.Instances.ReadDraft(instance).Revision));
                    AssertGraphVisible(editor); yield return Screenshot("workbench-complex-1920x1080.png");
                    editor.CloseButton.onClick.Invoke(); yield return Wait(() => FindForm<AlgorithmEditorForm>() == null);
                    entry.BindHud(null); entry.Advance(.02);
                    Assert.That(runtime.Navigation.IsActive, Is.True, "UI disposal must leave task ownership with the world host.");
                    double until = Time.realtimeSinceStartupAsDouble + 25;
                    while (runtime.Navigation.IsActive && Time.realtimeSinceStartupAsDouble < until) { entry.Advance(Time.unscaledDeltaTime); yield return null; }
                    Assert.That(runtime.Navigation.Outcome, Is.EqualTo(BehaviorOutcome.Completed));
                    for (int i = 0; i < 5; i++) { entry.Advance(.02); yield return null; }
                    int completed = 0; foreach (var task in runtime.Context.Tasks.History) if (task.State == MachineTaskState.Completed) completed++;
                    Assert.That(completed, Is.EqualTo(1));
                    using (var read = AlgorithmReadModels.Create(uiSession))
                    {
                        bool source = false, task = false;
                        foreach (var run in read.Snapshot.Runs)
                        { source |= run.SourceId == sensor.Id.Value && run.SourceTargetId == target.Id.Value && run.BindingGeneration == bound.Generation; task |= run.TaskId != 0 && run.ResultPort == "completed"; }
                        Assert.That(source && task, Is.True, "Diagnostics preserve real input identity and domain task outcome.");
                    }
                    int reopen = AutoEraUiNavigator.Open(UIViews.AlgorithmEditorForm, uiSession, new AutoEraUiPageRequest(AlgorithmEditorForm.PageEditor));
                    yield return Wait(() => Ui.HasUIForm(reopen)); editor = (AlgorithmEditorForm)Ui.GetUIForm(reopen).Logic;
                    var changed = runtime.Instances.ReadDraft(instance); changed.Nodes.Find(n => n.Kind == AlgorithmNodeKind.Constant).Default.X = 25;
                    Assert.That(runtime.Instances.Edit(instance, changed.Revision, changed), Is.True);
                    editor.AlgorithmEditorDiagnoseButton.onClick.Invoke(); yield return null;
                    for (int i = 0; i < 30 && !TaskDiagnosticVisible(editor); i++)
                    { editor.AlgorithmDiagnosisPreviousStepButton.onClick.Invoke(); yield return null; }
                    Assert.That(TaskDiagnosticVisible(editor), Is.True, "A recorded domain task can be selected through the UI history buttons.");
                    bool highlighted = false;
                    foreach (var node in editor.AlgorithmGraphContent.GetComponentsInChildren<AlgorithmNodeItem>())
                        highlighted |= node.Diagnostic == UiAlgorithmNodeDiagnostic.Executed;
                    Assert.That(highlighted, Is.True, "The captured execution path must reach the node visuals.");
                    AssertGraphVisible(editor); yield return Screenshot("workbench-diagnostic-1920x1080.png");
                    editor.AlgorithmDiagnosisReturnEditButton.onClick.Invoke(); yield return null;
                    editor.AlgorithmEditorApplyButton.onClick.Invoke(); yield return null;
                    Assert.That(runtime.Instances.ReadRequest(instance).State, Is.EqualTo(AlgorithmApplyState.WaitingSafePoint));
                    editor.CloseButton.onClick.Invoke(); yield return Wait(() => FindForm<AlgorithmEditorForm>() == null);
                    for (int i = 0; i < 10; i++) { entry.Advance(.02); yield return null; }
                    Assert.That(runtime.Instances.ReadRequest(instance).State, Is.EqualTo(AlgorithmApplyState.Succeeded));
                    Assert.That(runtime.Instances.ListInstances()[0].AppliedRevision, Is.EqualTo(runtime.Instances.ReadDraft(instance).Revision));
                }
                finally
                {
                    foreach (var form in Ui.GetAllLoadedUIForms()) if (form.Logic is AlgorithmEditorForm || form.Logic is AlgorithmBindingForm || form.Logic is NodeComponentPickerForm || form.Logic is AlgorithmLibraryForm) Ui.CloseUIForm(form.SerialId);
                    entry.Release(); context.ReleaseActiveWorldSession();
                    if (Debugger != null) Debugger.ActiveWindow = debugger;
                }
            }
            var unload = SceneManager.UnloadSceneAsync(scene); if (unload != null) while (!unload.isDone) yield return null;
        }
        private static AlgorithmDocument Template()
        {
            var g = new AlgorithmDocument { DocumentId = 100 };
            g.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.Startup, LayoutX = -400, LayoutY = 140 });
            g.Nodes.Add(new AlgorithmNode { Id = 2, Kind = AlgorithmNodeKind.Constant, LayoutX = -400, LayoutY = -40, ValueType = AlgorithmType.Of(AlgorithmValueKind.Position), Default = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.Position), X = 28, Y = 0, Z = -25, IsValid = true } });
            g.Nodes.Add(new AlgorithmNode { Id = 3, Kind = AlgorithmNodeKind.Navigate, LayoutX = 0, LayoutY = 20 });
            g.Nodes.Add(new AlgorithmNode { Id = 4, Kind = AlgorithmNodeKind.Input, Field = "resource", BindingKey = "read", LayoutX = -400, LayoutY = -200 });
            g.Nodes.Add(new AlgorithmNode { Id = 5, Kind = AlgorithmNodeKind.Log, LayoutX = 400, LayoutY = 140 });
            g.Nodes.Add(new AlgorithmNode { Id = 6, Kind = AlgorithmNodeKind.Log, LayoutX = 400, LayoutY = -200 });
            g.Edges.Add(new AlgorithmEdge { From = 1, Output = "event", To = 3, Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 2, Output = "value", To = 3, Input = "target" });
            g.Edges.Add(new AlgorithmEdge { From = 3, Output = "completed", To = 5, Input = "event" });
            g.Edges.Add(new AlgorithmEdge { From = 4, Output = "sampled", To = 6, Input = "event" }); return g;
        }
        private static T FindForm<T>() where T : AutoEraUiFormBase
        { foreach (var form in Ui.GetAllLoadedUIForms()) if (form.Logic is T found) return found; return null; }
        private static bool TaskDiagnosticVisible(AlgorithmEditorForm editor)
        { foreach (var text in editor.AlgorithmEditorInspectorContent.GetComponentsInChildren<TMP_Text>()) if (text.text.Contains(" · 完成")) return true; return false; }
        private static void AssertGraphVisible(AlgorithmEditorForm editor)
        {
            var viewport = (RectTransform)editor.AlgorithmGraphContent.parent; var corners = new Vector3[4]; int nodes = 0;
            foreach (Transform child in editor.AlgorithmGraphContent)
            {
                if (!child.gameObject.activeInHierarchy || child.GetComponent<AlgorithmNodeItem>() == null) continue;
                nodes++; ((RectTransform)child).GetWorldCorners(corners);
                foreach (var corner in corners) Assert.That(viewport.rect.Contains(viewport.InverseTransformPoint(corner)), Is.True, "Initial fit must show every node, including tall multi-port nodes.");
            }
            Assert.That(nodes, Is.EqualTo(6));
        }
        private static IEnumerator Wait(Func<bool> predicate)
        { double until = Time.realtimeSinceStartupAsDouble + 30; while (!predicate() && Time.realtimeSinceStartupAsDouble < until) yield return null; Assert.That(predicate(), Is.True, "Expected formal UI/runtime state within 30 seconds."); }
        private static void ClickRow(Transform content, string label)
        {
            foreach (Transform row in content)
            {
                if (!row.gameObject.activeInHierarchy) continue;
                bool match = label == null;
                foreach (var text in row.GetComponentsInChildren<TMP_Text>()) if (text.text.Contains(label ?? "\0")) match = true;
                var button = row.GetComponentInChildren<Button>();
                if (!match || button == null || !button.interactable) continue;
                button.onClick.Invoke(); return;
            }
            Assert.Fail("Visible selectable row not found: " + label);
        }
        private static IEnumerator Screenshot(string name)
        {
            yield return null; yield return null;
            Assert.That(Screen.width, Is.EqualTo(1920)); Assert.That(Screen.height, Is.EqualTo(1080));
            Directory.CreateDirectory(Evidence); string path = Path.GetFullPath(Path.Combine(Evidence, name));
            if (File.Exists(path)) File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return Wait(() => File.Exists(path));
            byte[] png = File.ReadAllBytes(path);
            int Width(int offset) => (png[offset] << 24) | (png[offset + 1] << 16) | (png[offset + 2] << 8) | png[offset + 3];
            Assert.That(Width(16), Is.EqualTo(1920)); Assert.That(Width(20), Is.EqualTo(1080));
        }
    }
}
