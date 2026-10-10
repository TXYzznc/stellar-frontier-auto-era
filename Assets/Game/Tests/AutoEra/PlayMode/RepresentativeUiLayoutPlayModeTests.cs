using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.Save;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Region;
using AutoEra.World.Time;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>Actual GF forms, fixture-owned domain state, and screenshots; not a formal world-flow acceptance.</summary>
    public sealed class RepresentativeUiLayoutPlayModeTests
    {
        private const string Evidence = "openspec/changes/b51-ui-representative-layout-prototypes/evidence";
        private static UnityGameFramework.Runtime.UIComponent Ui => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();

        [UnityTest, Timeout(180000)]
        public IEnumerator ThreeWorkspaces_PreserveSelectionNavigationAndWorldInput()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();
            yield return WaitForMachineCatalog();
            yield return Verify();
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            if (SceneManager.GetActiveScene().name == "Launch") yield break;
            AsyncOperation operation = SceneManager.LoadSceneAsync("Launch", LoadSceneMode.Single);
            Assert.IsNotNull(operation, "Launch 必须启用并加入 Build Settings。");
            yield return operation;
            yield return null;
        }

        private static IEnumerator WaitForRuntimeReady()
        {
            for (int frame = 0; frame < 600; frame++)
            {
                if (Ui != null && Ui.HasUIGroup("Default") && Find<MainMenuForm>() != null) yield break;
                yield return null;
            }
            Assert.Fail("GF UI 运行时在 600 帧内没有就绪。");
        }

        private static IEnumerator WaitForMachineCatalog()
        {
            double until = Time.realtimeSinceStartupAsDouble + 60d;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True, "机器数据表必须在运行期就绪。");
        }

        private static IEnumerator Verify()
        {
            yield return Wait(() => Ui != null && Ui.HasUIGroup("Default") && Find<MainMenuForm>() != null, "initial UI");
            foreach (var form in Ui.GetAllLoadedUIForms()) Ui.CloseUIForm(form.SerialId);
            var debugger = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.DebuggerComponent>();
            if (debugger != null) debugger.ActiveWindow = false;
            Assert.That(Screen.width, Is.EqualTo(1920)); Assert.That(Screen.height, Is.EqualTo(1080));
            string save = Path.Combine(Path.GetTempPath(), "AutoEraB51-" + Guid.NewGuid().ToString("N"));
            var context = new AutoEraApplicationContext(new SystemUtcTimeProvider(), new AutoEraWorldSessionFactory(), null, new SaveSlotService(save));
            InitialRegion region = null;
            try
            {
                Assert.That(context.TryCreateWorldSession(0, out var world), Is.True);
                var machine = world.Machines.Create(new MachineDefinition(1001, "结构验收机", 1, 2, 1, 2, 30, true, true, 100));
                region = new InitialRegion(world, new Rect(-50,-50,100,100));
                var session = AutoEraUiSession.ForWorld(context, world, region);
                int machineId = AutoEraUiNavigator.Open(UIViews.MachineLibraryForm, session);
                yield return Wait(() => Ui.GetUIForm(machineId) != null && Find<MachineLibraryForm>()?.MachineDataState == UiDataState.Ready, "machine library");
                var library = Find<MachineLibraryForm>();
                Button row = null;
                foreach (var button in library.GetComponentsInChildren<Button>(true))
                    if (button.name == "Btn_UndeployedMachinesCatalogRow" && button.gameObject.activeInHierarchy) { row = button; break; }
                Assert.That(row, Is.Not.Null); row.onClick.Invoke();
                Assert.That(library.SelectedMachineName, Is.EqualTo(machine.Name));
                library.ShowLibraryPage(MachineLibraryForm.PagePreparation);
                yield return null;
                Assert.That(Node(library,"Grp_MachinePreparationSuccessState").gameObject.activeSelf, Is.False);
                Assert.That(Node(library,"Panel_MachinePreparationDisplay").gameObject.activeInHierarchy, Is.True);
                var assembly = Node(library,"Content_MachinePreparationAssembly");
                Button slot = null;
                foreach (var button in assembly.GetComponentsInChildren<Button>()) if (button.interactable) { slot=button;break; }
                Assert.That(slot, Is.Not.Null); slot.onClick.Invoke();
                Assert.That(Node(library,"Btn_MachinePreparationInstall").GetComponent<Button>().interactable, Is.True);
                yield return Capture("machine-preparation-runtime-fixture.png");
                Ui.CloseUIForm(machineId); yield return null;

                int hubId = AutoEraUiNavigator.Open(UIViews.BaseCommandHubForm, session);
                yield return Wait(() => Find<BaseCommandHubForm>()?.CurrentPage == BaseCommandHubForm.PageOverview, "base hub");
                var hub = Find<BaseCommandHubForm>();
                Node(hub,"Btn_HubOverviewObjects").GetComponent<Button>().onClick.Invoke();
                Assert.That(hub.CurrentPage, Is.EqualTo(BaseCommandHubForm.PageObjects));
                hub.ShowHubPage(BaseCommandHubForm.PageOverview);
                Node(hub,"Btn_HubOverviewStats").GetComponent<Button>().onClick.Invoke();
                Assert.That(hub.CurrentPage, Is.EqualTo(4), "Logical statistics maps to physical page after energy extraction.");
                hub.ShowHubPage(BaseCommandHubForm.PageOverview);
                Assert.That(Node(hub,"Btn_HubOverviewSupply").GetComponent<Button>().interactable, Is.False);
                yield return Capture("hub-overview-runtime-fixture.png");
                Ui.CloseUIForm(hubId); yield return null;

                Assert.That(region.DeployMachine(machine.Id,new Vector2(5,5),new Vector2(2,2),out var regionObject), Is.EqualTo(RegionMachineDeploymentResult.Bound));
                Assert.That(region.Select(regionObject.Id, false), Is.True);
                int hudId = AutoEraUiNavigator.Open(UIViews.FieldHudForm, session);
                yield return Wait(() => Find<FieldHudResidentForm>() != null, "world HUD resident");
                yield return Wait(() => Find<FieldHudMachineOverviewForm>() != null, "machine overview");
                yield return null;
                var hud = Find<FieldHudResidentForm>();
                Assert.That(hud.ShowFormPage(0), Is.True);
                foreach (string module in new[]{"Status","Tracker","Alerts","Navigation","Save"})
                    Assert.That(Node(hud,"Panel_PageHud"+module).gameObject.activeSelf, Is.True, module);
                Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.False);
                var pointer = new PointerEventData(EventSystem.current) { position = new Vector2(960,540) };
                var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer,hits);
                Assert.That(hits.Exists(h=>h.gameObject.GetComponentInParent<AutoEraUiFormBase>() != null), Is.False, "Central world must remain unobstructed.");
                Assert.That(Node(Find<FieldHudMachineOverviewForm>(),"Grp_MachineOverviewSuccessState").gameObject.activeSelf, Is.False);
                yield return Capture("world-hud-runtime-fixture.png");
                Ui.CloseUIForm(hudId); yield return null;
                Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.False);
            }
            finally
            {
                foreach (var form in Ui.GetAllLoadedUIForms()) Ui.CloseUIForm(form.SerialId);
                region?.Dispose(); context.Dispose();
            }
        }

        private static Transform Node(Component root,string name)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true)) if(t.name==name)return t;
            Assert.Fail("Missing node: "+name); return null;
        }
        private static T Find<T>() where T : Component
        {
            foreach(var f in Ui.GetAllLoadedUIForms()) if(f.Logic is T t) return t;
            T[] components = UnityEngine.Object.FindObjectsOfType<T>(true);
            for (int i = 0; i < components.Length; i++)
                if (components[i].gameObject.activeInHierarchy) return components[i];
            return null;
        }
        private static IEnumerator Wait(Func<bool> condition, string label = "runtime UI")
        {
            double end=Time.realtimeSinceStartupAsDouble+30;
            while(!condition() && Time.realtimeSinceStartupAsDouble<end)yield return null;
            Assert.That(condition(),Is.True,$"{label} did not reach the expected state.");
        }
        private static IEnumerator Capture(string name)
        {
            yield return null; yield return null;
            Directory.CreateDirectory(Evidence);
            string path=Path.GetFullPath(Path.Combine(Evidence,name));
            if(File.Exists(path))File.Delete(path);
            ScreenCapture.CaptureScreenshot(path);
            yield return Wait(()=>File.Exists(path));
        }
    }
}
