using System;
using System.Collections;
using System.Text;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 整备页 → 12-组件选择器 → 17-硬件修改确认 的**装入**全链（规格 05-机器整备 →
    /// 12-选择与绑定 → 17-操作确认）。
    ///
    /// 这是本批的核心验收：三段界面各自只做一件事——整备页只交出「哪台机器的哪一格」，
    /// 选择器只交出「哪一件组件」，确认页只收集确认——**真正的装入由领域执行**。
    /// 因此断言必须穿过整条链去看领域的最终状态：槽位里出现了那件组件、它不再是散件。
    /// 只断言「确认页打开了」会漏掉「三段都开了但谁都没装」这种失败。
    ///
    /// 一个 PlayMode 类只放一条用例：第二条用例会重新引导框架，而上一轮的世界对象已被销毁
    /// （实测报 `MissingReferenceException: UIComponent has been destroyed`）。
    /// </summary>
    public sealed class ComponentPickerInstallPlayModeTests
    {
        private const string LaunchSceneName = "Launch";

        [UnityTest]
        public IEnumerator EmptySlot_OpensThePicker_ThenTheConfirm_ThenTheDomainReallyInstalls()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int libraryId = 0;
                int pickerId = 0;
                int dialogId = 0;
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition wheeled), Is.True);

                    MachineInstance machine = session.Machines.Create(wheeled);
                    Assert.That(catalog.TryGetComponentRow(20011, out ComponentDisplayRow coreRow), Is.True);
                    ComponentInstance core = session.Machines.CreateComponent(
                        new ComponentDefinition(coreRow.ModelId, coreRow.Kind, coreRow.Level,
                            coreRow.AddedCapacity, coreRow.ComputeCapacity, coreRow.LogicCapacity, coreRow.HasBehavior));

                    // 起点：库里有一件散件，槽位是空的。
                    Assert.That(core.OwnerId.IsValid, Is.False, "新组件是散件。");
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.Null, "槽位是空的。");

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(context, session);
                    UIParams parameters = uiSession.WriteTo(UIParams.Create());
                    parameters.Set(AutoEraUiParamKeys.Request, new AutoEraUiPageRequest(MachineLibraryForm.PageUndeployed));
                    libraryId = GF.UI.OpenUIForm(UIViews.MachineLibraryForm, parameters);
                    yield return WaitForForm(libraryId, expectedLoaded: true);

                    var library = GF.UI.GetUIForm(libraryId).Logic as MachineLibraryForm;
                    Assert.That(library, Is.Not.Null);

                    Button row = FirstRowButton(library.UndeployedMachinesCatalogContent);
                    Assert.That(row, Is.Not.Null);
                    row.onClick.Invoke();
                    yield return null;
                    library.UndeployedPrepareButton.onClick.Invoke();
                    yield return null;
                    Assert.That(library.CurrentPage, Is.EqualTo(MachineLibraryForm.PagePreparation));

                    // 选中一个**空**槽位：入口变可用，且点它是去选择器（不是去确认页）。
                    Button emptySlotRow = RowButtonByText(library.MachinePreparationAssemblyContent, "核心槽 0");
                    Assert.That(emptySlotRow, Is.Not.Null, "整备页必须逐个列出槽位。");
                    emptySlotRow.onClick.Invoke();
                    yield return null;
                    Assert.That(library.SelectedSlotLabel, Is.EqualTo("核心槽 0"));
                    Assert.That(library.SelectedSlotComponent(), Is.Null, "这一格是空的。");
                    Assert.That(library.PreparationInstallButton.interactable, Is.True,
                        "空槽位也要能点——它会去 12-组件选择器。");

                    library.PreparationInstallButton.onClick.Invoke();
                    var pickerHandle = new FormHandle<ComponentPickerForm>();
                    yield return WaitForForm(pickerHandle, form => form.HasRequest);
                    Assert.That(pickerHandle.Form, Is.Not.Null, "空槽位的安装入口必须落到 12-组件选择器。");
                    pickerId = pickerHandle.SerialId;
                    ComponentPickerForm picker = pickerHandle.Form;

                    // 选择器：候选是那件散件，且还没预选就不该能确认。
                    ComponentPickerSnapshot pickerSnapshot = picker.PickerSnapshot;
                    Assert.That(pickerSnapshot.CandidateCount, Is.EqualTo(1));
                    Assert.That(pickerSnapshot.Candidates[0].Id, Is.EqualTo(core.Id));
                    Assert.That(pickerSnapshot.CanConfirm, Is.False, "还没预选就不该能确认。");
                    StringAssert.Contains("核心槽 0", pickerSnapshot.SlotLabel);

                    Button candidateRow = FirstRowButton(picker.ComponentPickerCandidatesContent);
                    Assert.That(candidateRow, Is.Not.Null);
                    candidateRow.onClick.Invoke();
                    yield return null;
                    Assert.That(picker.PickerSnapshot.SelectedId, Is.EqualTo(core.Id));
                    Assert.That(picker.CanConfirmSelection, Is.True);
                    StringAssert.Contains("当前",
                        TextsOf(picker.ComponentPickerComparisonContent),
                        "比较栏要给「当前值 → 装有之后」，不是孤零零一个增量。");

                    // 确认：写回候选身份并打开 17-硬件修改确认；**此时还没有装入任何东西**。
                    picker.Confirm();
                    yield return null;
                    Assert.That(picker.PickedComponentId, Is.EqualTo(core.Id));
                    Assert.That(core.OwnerId.IsValid, Is.False, "确认之前组件还没有归属。");
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.Null, "确认之前槽位还是空的。");

                    var dialogHandle = new FormHandle<OperationDialogForm>();
                    yield return WaitForForm(dialogHandle, form => form.HasHardwareRequest);
                    Assert.That(dialogHandle.Form, Is.Not.Null, "选择器确认后必须进入 17-硬件修改确认。");
                    dialogId = dialogHandle.SerialId;
                    OperationDialogForm dialog = dialogHandle.Form;

                    Assert.That(dialog.HardwareMachine, Is.SameAs(machine));
                    Assert.That(dialog.HardwareState, Is.EqualTo(HardwareOperationState.Idle));
                    StringAssert.Contains(coreRow.Name, TextsOf(dialog.HardwareConfirmChangeContent),
                        "确认页要说清装进去的是哪一件。");

                    // 提交：领域真的把它装上了。
                    dialog.HardwareConfirmCommitButton.onClick.Invoke();
                    yield return null;
                    yield return null;

                    Assert.That(dialog.HardwareState, Is.EqualTo(HardwareOperationState.Completed),
                        "库中机器走 Library 来源、且没有正在执行的行为，提交后应当立即完成。");
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                        "提交之后槽位里必须真的装着那件组件。");
                    Assert.That(core.OwnerId, Is.EqualTo(machine.Id),
                        "装入成功后组件归属这台机器；只把状态组点亮成 Success 是假成功。");

                    // 回整备页看：它读的是同一个领域状态，所以那一格显示的必须是刚装上的组件。
                    StringAssert.Contains(coreRow.Name, TextsOf(library.MachinePreparationAssemblyContent),
                        "整备页必须看到刚装上的组件——它的数据来自同一个花名册。");
                    Assert.That(library.SelectedSlotComponent(), Is.SameAs(core));
                }
                finally
                {
                    if (dialogId > 0 && GF.UI != null && GF.UI.HasUIForm(dialogId)) GF.UI.CloseUIForm(dialogId);
                    if (pickerId > 0 && GF.UI != null && GF.UI.HasUIForm(pickerId)) GF.UI.CloseUIForm(pickerId);
                    if (libraryId > 0 && GF.UI != null && GF.UI.HasUIForm(libraryId)) GF.UI.CloseUIForm(libraryId);
                    context.ReleaseActiveWorldSession();
                }
            }
        }

        /// <summary>异步打开的界面：只有 GF 知道序列号，所以回读一次。</summary>
        private sealed class FormHandle<TForm> where TForm : UIFormLogic
        {
            public TForm Form;
            public int SerialId;
        }

        private static IEnumerator WaitForForm<TForm>(FormHandle<TForm> handle, Func<TForm, bool> ready)
            where TForm : UIFormLogic
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                foreach (UIForm form in GF.UI.GetAllLoadedUIForms())
                {
                    if (form.Logic is TForm typed && ready(typed))
                    {
                        handle.Form = typed;
                        handle.SerialId = form.SerialId;
                        yield break;
                    }
                }

                yield return null;
            }

            Assert.Fail($"界面 {typeof(TForm).Name} 在 300 帧内没有打开。");
        }

        private static string TextsOf(RectTransform content)
        {
            if (content == null)
            {
                return string.Empty;
            }

            var text = new StringBuilder(256);
            foreach (TMP_Text item in content.GetComponentsInChildren<TMP_Text>(false))
            {
                text.Append(item.text).Append('|');
            }

            return text.ToString();
        }

        private static Button FirstRowButton(RectTransform content)
        {
            return content == null ? null : content.GetComponentInChildren<Button>(false);
        }

        private static Button RowButtonByText(RectTransform content, string needle)
        {
            if (content == null)
            {
                return null;
            }

            foreach (Button button in content.GetComponentsInChildren<Button>(false))
            {
                foreach (TMP_Text text in button.GetComponentsInChildren<TMP_Text>(false))
                {
                    if (text.text != null && text.text.Contains(needle))
                    {
                        return button;
                    }
                }
            }

            return null;
        }

        private static IEnumerator EnsureLaunchSceneLoaded()
        {
            if (SceneManager.GetActiveScene().name == LaunchSceneName)
            {
                yield break;
            }

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(LaunchSceneName, LoadSceneMode.Single);
            Assert.That(loadOperation, Is.Not.Null, "Launch 必须在 Build Settings 里启用。");
            yield return loadOperation;
            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(LaunchSceneName));
            yield return null;
        }

        private static IEnumerator WaitForRuntimeReady()
        {
            const int maxFrames = 600;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI != null && GF.DataTable != null &&
                    GF.DataTable.HasDataTable<UITable>() &&
                    GF.DataTable.HasDataTable<UIGroupTable>() &&
                    GF.UI.HasUIGroup("Default"))
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("GF UI runtime or required UI data tables did not become ready within 600 frames.");
        }

        private static IEnumerator WaitForForm(int serialId, bool expectedLoaded)
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                if (GF.UI.HasUIForm(serialId) == expectedLoaded)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail($"UI form serial {serialId} did not reach loaded={expectedLoaded} within 300 frames.");
        }
    }
}
