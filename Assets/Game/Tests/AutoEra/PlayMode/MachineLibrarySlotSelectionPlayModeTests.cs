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
    /// 整备页的槽位选择 → 硬件修改确认（规格 05-机器整备 → 17-HardwareConfirm）。
    ///
    /// 这一段是「界面只收集意图、领域负责执行」最容易被做错的地方：很容易在整备页上
    /// 直接调 `MachineRoster.Remove` 图省事。所以断言分两截：
    ///   ① 没选槽位时「安装或拆卸」必须不可点——按钮点了没反应等于界面在骗人；
    ///   ② 选中已占用槽位后点它，打开的必须是 17 的确认页，而且**此时机器还没有被改动**
    ///      （改动只能发生在确认页提交之后）。
    ///
    /// 一个 PlayMode 类只放一条用例：第二条用例会重新引导框架，而上一轮的世界对象已被销毁
    /// （实测报 `MissingReferenceException: UIComponent has been destroyed`）。
    /// </summary>
    public sealed class MachineLibrarySlotSelectionPlayModeTests
    {
        private const string LaunchSceneName = "Launch";

        [UnityTest]
        public IEnumerator SelectingAnOccupiedSlot_EnablesUninstall_AndOpensTheHardwareConfirmPage()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int libraryId = 0;
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
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, core.Id, 0),
                        Is.EqualTo(MachineManagementResult.Completed));

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

                    // ① 还没选槽位：「安装或拆卸」不可点，且页面说清了要先选一格。
                    Assert.That(library.HasSlotSelection, Is.False);
                    Assert.That(library.PreparationInstallButton.interactable, Is.False,
                        "没有选中槽位时不能给出可点的安装／拆卸入口——它无处可去。");
                    StringAssert.Contains("槽位", library.MachinePreparationAssemblyBody.text);

                    // ② 选中装着核心的那一格：入口变可用，且指向的槽位就是那一格。
                    Button slotRow = RowButtonByText(library.MachinePreparationAssemblyContent, "核心槽 0");
                    Assert.That(slotRow, Is.Not.Null, "整备页必须逐个列出槽位，且槽位行可选中。");
                    Assert.That(slotRow.interactable, Is.True);
                    slotRow.onClick.Invoke();
                    yield return null;

                    Assert.That(library.HasSlotSelection, Is.True);
                    Assert.That(library.SelectedSlotLabel, Is.EqualTo("核心槽 0"));
                    Assert.That(library.SelectedSlotComponent(), Is.Not.Null,
                        "选中的槽位里装着刚才装上的核心。");
                    Assert.That(library.PreparationInstallButton.interactable, Is.True,
                        "选中已占用槽位后，拆卸方向必须可用（它经 17-硬件确认走）。");
                    StringAssert.Contains("核心槽 0", library.MachinePreparationAssemblyBody.text);

                    // ③ 点它 → 打开 17 的硬件修改确认页；此时机器**还没被改动**。
                    library.PreparationInstallButton.onClick.Invoke();
                    var found = new DialogHandle();
                    yield return WaitForDialog(found);
                    Assert.That(found.Form, Is.Not.Null, "整备页的安装／拆卸入口必须落到 17-硬件修改确认。");
                    OperationDialogForm dialog = found.Form;
                    dialogId = found.SerialId;

                    Assert.That(dialog.HasHardwareRequest, Is.True);
                    Assert.That(dialog.HardwareMachine, Is.SameAs(machine));
                    Assert.That(dialog.HardwareState, Is.EqualTo(HardwareOperationState.Idle),
                        "确认页在提交之前不得动过硬件。");
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                        "确认之前槽位里还是原来那颗核心。");
                    Assert.That(dialog.HardwareConfirmCommitButton.interactable, Is.True);

                    string change = TextsOf(dialog.HardwareConfirmChangeContent);
                    StringAssert.Contains("核心槽 0", change);
                    StringAssert.Contains(coreRow.Name, change, "确认页要说清拆下来的到底是哪一颗。");

                    // ④ 不提交就关掉：确认之前机器必须原封不动。
                    GF.UI.CloseUIForm(dialogId);
                    yield return WaitForForm(dialogId, expectedLoaded: false);
                    dialogId = 0;
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                        "关掉确认页不该撤销任何东西——因为本来就什么都没做。");

                    // ⑤ 整备页的「一键卸下」：不需要先选槽位，但机器真的装着东西时才可点。
                    Assert.That(library.CanUnloadAll, Is.True);
                    Assert.That(library.PreparationUnloadButton.interactable, Is.True,
                        "装着组件的未部署机器必须给出可点的一键卸下入口。");
                    library.PreparationUnloadButton.onClick.Invoke();

                    var unload = new DialogHandle();
                    yield return WaitForDialog(unload);
                    Assert.That(unload.Form, Is.Not.Null, "一键卸下必须落到同一个 17-硬件修改确认。");
                    dialog = unload.Form;
                    dialogId = unload.SerialId;

                    Assert.That(dialog.HardwareRequestIsUnloadAll, Is.True,
                        "确认页必须知道这次是「整台清空」而不是某一格。");
                    Assert.That(dialog.HardwareMachine, Is.SameAs(machine));
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                        "确认之前槽位里还是原来那颗核心。");

                    string unloadChange = TextsOf(dialog.HardwareConfirmChangeContent);
                    StringAssert.Contains("核心槽 0", unloadChange, "变更清单要逐槽位列清楚。");
                    StringAssert.Contains(coreRow.Name, unloadChange);
                    StringAssert.Contains("组件库", unloadChange, "卸下的组件去哪里必须说清。");

                    // ⑥ 提交 → 全部卸下、归属清空（于是它们回到组件库成为散件）。
                    dialog.HardwareConfirmCommitButton.onClick.Invoke();
                    yield return null;

                    Assert.That(dialog.HardwareState, Is.EqualTo(HardwareOperationState.Completed),
                        "整备环境里的机器没有进行中的行为，提交即完成。");
                    Assert.That(machine.InstalledComponentCount, Is.Zero);
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.Null);
                    Assert.That(core.OwnerId.IsValid, Is.False,
                        "归属被清空，组件才真的回到组件库；留着 OwnerId 会让它既不在机器上也不在库里。");
                }
                finally
                {
                    if (dialogId > 0 && GF.UI != null && GF.UI.HasUIForm(dialogId)) GF.UI.CloseUIForm(dialogId);
                    if (libraryId > 0 && GF.UI != null && GF.UI.HasUIForm(libraryId)) GF.UI.CloseUIForm(libraryId);
                    context.ReleaseActiveWorldSession();
                }
            }
        }

        /// <summary>导航是异步的（要加载预制体），而且打开它的序列号只有 GF 知道，所以这里回读一次。</summary>
        private sealed class DialogHandle
        {
            public OperationDialogForm Form;
            public int SerialId;
        }

        private static IEnumerator WaitForDialog(DialogHandle handle)
        {
            const int maxFrames = 300;
            for (int frame = 0; frame < maxFrames; frame++)
            {
                foreach (UIForm form in GF.UI.GetAllLoadedUIForms())
                {
                    if (form.Logic is OperationDialogForm dialog && dialog.HasHardwareRequest)
                    {
                        handle.Form = dialog;
                        handle.SerialId = form.SerialId;
                        yield break;
                    }
                }

                yield return null;
            }

            Assert.Fail("硬件修改确认页在 300 帧内没有打开。");
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
