using System.Collections;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.PlayMode
{
    public sealed class MachineHardwareUiPlayModeTests
    {
        [UnityTest]
        public IEnumerator FormalHud_UsesAuthorityAndClosesWithoutCancellingAcceptedRequest()
        {
            if (SceneManager.GetActiveScene().name != "Launch") yield return SceneManager.LoadSceneAsync("Launch");
            double deadline = Time.realtimeSinceStartupAsDouble + 30;
            while ((!MachineCatalog.IsGameDataLoaded || GF.UI == null || !GF.UI.HasUIGroup("Default")) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded,Is.True);
            // Follow the real menu -> world transition. The owning procedure must close
            // its own form; closing it directly leaves a stale serial for shutdown.
            MainMenuForm menu = null;
            deadline = Time.realtimeSinceStartupAsDouble + 15;
            while (menu == null && Time.realtimeSinceStartupAsDouble < deadline)
            { menu = Object.FindObjectOfType<MainMenuForm>(); yield return null; }
            Assert.That(menu, Is.Not.Null);
            var enter = menu.GetComponentInChildren<Button>();
            while (!enter.interactable && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            Assert.That(enter.interactable,Is.True);
            enter.onClick.Invoke();
            int serial = -1;
            deadline = Time.realtimeSinceStartupAsDouble + 30;
            while (serial < 0 && Time.realtimeSinceStartupAsDouble < deadline)
            {
                foreach (var loaded in GF.UI.GetAllLoadedUIForms())
                    if (loaded.Logic is FieldHudForm) serial = loaded.SerialId;
                yield return null;
            }
            Assert.That(serial,Is.GreaterThanOrEqualTo(0));
            Assert.That(GF.UI.HasUIForm(serial),Is.True);
            var hud=(FieldHudForm)GF.UI.GetUIForm(serial).Logic;
            // This test supplies explicit access origin, not a physical mouse/camera selection.
            // Suspend only the device adapter so it cannot overwrite that fixture each frame.
            var input = Object.FindObjectOfType<AutoEra.Input.RegionInputModule>();
            bool inputWasEnabled = input != null && input.enabled;
            if (input != null) input.enabled = false;
            hud.SetFieldAccess(true, false);
            using(var session=new AutoEraWorldSessionFactory().Create(0))
            using(var region=new AutoEra.World.Region.InitialRegion(session,new Rect(-20,-20,40,40)))
            {
                try
                {
                    var catalog=MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011,out var definition),Is.True);
                    var machine=session.Machines.Create(definition);
                    Assert.That(region.DeployMachine(machine.Id,Vector2.zero,new Vector2(2,3),out _),
                        Is.EqualTo(AutoEra.World.Region.RegionMachineDeploymentResult.Bound));
                    // Explicit environmental/physical adapter input, not a claim that a power network exists.
                    machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true,true);
                    Assert.That(catalog.TryGetComponent(22061,out var cargoDefinition),Is.True);
                    var cargo=session.Machines.CreateComponent(cargoDefinition);
                    hud.BindRegion(region); hud.BindMachines(session.Machines);
                    Assert.That(region.Select(machine.Id,false),Is.True,"Same-ID selection must open the machine child through the region presenter.");
                    yield return null;
                    var panel=hud.GetComponentInChildren<MachineHardwarePanel>(true);
                    Assert.That(panel,Is.Not.Null); Assert.That(AutoEraUiRuntime.BlocksWorldInput,Is.True);
                    yield return Capture("overview");
                    Button(panel,"B11_TabHardware").onClick.Invoke();
                    Assert.That(panel.Presenter.Layer,Is.EqualTo(MachinePanelLayer.Hardware));
                    machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running); machine.UpdateBehaviorActivity(true);
                    Button(panel,"B11_Effector1_Select").onClick.Invoke();
                    Assert.That(panel.Presenter.SelectCandidate(cargo.Id),Is.True);
                    Assert.That(Label(panel,"B11_PickerTitle"),Is.EqualTo("效应器 1 · 选择组件"));
                    yield return Capture("picker");
                    Button(panel,"B11_PickerInstall").onClick.Invoke();
                    Assert.That(panel.Presenter.Layer,Is.EqualTo(MachinePanelLayer.Confirm));
                    Assert.That(Label(panel,"B11_FeedbackConfirmTitle"),Is.EqualTo("确认安装"));
                    Assert.That(EventSystem.current.currentSelectedGameObject.name,Is.EqualTo("B11_FeedbackConfirmCancel"));
                    yield return Capture("confirm");
                    Button(panel,"B11_FeedbackConfirmAccept").onClick.Invoke();
                    Assert.That(panel.Presenter.Layer,Is.EqualTo(MachinePanelLayer.Waiting));
                    panel.Close(); Assert.That(hud.BlocksWorldInput,Is.False,"Other Launch forms may still own their own input layer.");
                    machine.UpdateBehaviorActivity(false);
                    Assert.That(cargo.OwnerId,Is.EqualTo(machine.Id),"Closing only releases UI; accepted work remains authoritative.");
                    var secondCargo=session.Machines.CreateComponent(cargoDefinition);
                    Assert.That(session.Machines.Install(machine.Id,ManagementOrigin.Field,secondCargo.Id,1),Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(hud.OpenMachine(machine.Id,ManagementOrigin.Hub),Is.True);
                    panel.Presenter.ShowPage(true); yield return null;
                    Assert.That(Button(panel,"B11_Remove").interactable,Is.False);
                    Assert.That(Button(panel,"B11_Effector1_Enable").interactable,Is.False);
                    Assert.That(AutoEraUiRuntime.DispatchIntent(AutoEraUiIntent.Cancel),Is.True);
                    Assert.That(panel.IsOpen,Is.False); Assert.That(GF.UI.HasUIForm(serial),Is.True,"Child Cancel never closes the HUD.");
                    hud.OpenMachine(machine.Id,ManagementOrigin.Field); panel.Presenter.ShowPage(true);
                    Button(panel,"B11_Effector2").onClick.Invoke();
                    Assert.That(panel.Presenter.Slot,Is.EqualTo(1));
                    Button(panel,"B11_Effector2_Enable").onClick.Invoke();
                    Assert.That(secondCargo.Enabled,Is.False);
                    Assert.That(Button(panel,"B11_Effector2_Choose").gameObject.activeSelf,Is.False,"Occupied card uses its full-card selection, not overlapping buttons.");
                    yield return new WaitForSecondsRealtime(0.2f);
                    Assert.That(panel.gameObject.activeInHierarchy, Is.True);
                    Assert.That(panel.GetComponent<CanvasGroup>().interactable, Is.True);
                    System.IO.Directory.CreateDirectory("Temp/AutoEraTestResults");
                    ScreenCapture.CaptureScreenshot("Temp/AutoEraTestResults/b11-hardware-runtime.png");
                    yield return new WaitForEndOfFrame(); yield return null;
                    var procedure = UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.ProcedureComponent>();
                    var world = procedure.CurrentProcedure as AutoEra.Procedures.AutoEraWorldProcedure;
                    Assert.That(world,Is.Not.Null);
                    world.RequestReturnToMenu();
                    deadline = Time.realtimeSinceStartupAsDouble + 15;
                    while (GF.UI.HasUIForm(serial) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                    Assert.That(GF.UI.HasUIForm(serial),Is.False);
                    Assert.That(panel.Presenter,Is.Null,"Procedure-owned GF close releases child subscriptions.");
                }
                finally { if (hud != null) hud.BindMachines(null); if (input != null) input.enabled = inputWasEnabled; }
            }
        }
        private static IEnumerator Capture(string state)
        {
            yield return new WaitForEndOfFrame();
            System.IO.Directory.CreateDirectory("Temp/AutoEraTestResults");
            ScreenCapture.CaptureScreenshot("Temp/AutoEraTestResults/b11-" + state + "-runtime.png");
            yield return new WaitForEndOfFrame(); yield return null;
        }
        private static Button Button(MachineHardwarePanel panel,string name)
        {
            foreach(var button in panel.GetComponentsInChildren<Button>(true)) if(button.name==name) return button;
            Assert.Fail("Missing button: "+name); return null;
        }
        private static string Label(MachineHardwarePanel panel,string name)
        {
            foreach(var component in panel.GetComponentsInChildren<MonoBehaviour>(true))
                if(component != null && component.name == name && component.GetType().FullName == "TMPro.TextMeshProUGUI")
                    return (string)component.GetType().GetProperty("text").GetValue(component);
            Assert.Fail("Missing label: "+name); return null;
        }
    }
}
