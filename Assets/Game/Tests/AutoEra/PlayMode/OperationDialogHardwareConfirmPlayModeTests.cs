using System.Collections;
using System.Text;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 17-硬件修改确认页在真实 GF 运行时下的端到端验收：从打开请求到领域真的改动硬件。
    ///
    /// 这条用例守的是本批最核心的分工——**界面只收集确认，执行交给领域**：
    ///   * 确认页在提交之前不得动过硬件（否则「确认」就是个摆设）；
    ///   * 提交之后改动必须真的发生（槽位空了、组件回到组件库成为散件），而不是只把状态组
    ///     点亮成 Success 的假成功；
    ///   * 来源由确认页按机器的部署状态推导——这里是一台**库中机器**，所以它必须走
    ///     Library 来源。若界面传错来源，领域会以 InvalidOrigin 拒绝，状态组就会是 Error，
    ///     这些断言都会失败。
    ///
    /// 一个 PlayMode 类只放一条用例：第二条用例会重新引导框架，而上一轮的世界对象已被销毁
    /// （实测报 `MissingReferenceException: UIComponent has been destroyed`）。
    /// </summary>
    public sealed class OperationDialogHardwareConfirmPlayModeTests
    {
        private const string LaunchSceneName = "Launch";

        [UnityTest]
        public IEnumerator ConfirmingAnUninstall_ModifiesNothingBeforeCommit_AndReallyRemovesAfterIt()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
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
                    parameters.Set(AutoEraUiParamKeys.Request,
                        new AutoEraHardwareRequest(machine.Id, HardwareKind.Core, 0, PersistentId.Invalid, remove: true));
                    dialogId = GF.UI.OpenUIForm(UIViews.OperationDialogForm, parameters);
                    yield return WaitForForm(dialogId, expectedLoaded: true);

                    var dialog = GF.UI.GetUIForm(dialogId).Logic as OperationDialogForm;
                    Assert.That(dialog, Is.Not.Null);
                    Assert.That(dialog.HasHardwareRequest, Is.True);
                    Assert.That(dialog.CurrentPage, Is.EqualTo(OperationDialogForm.PageHardwareConfirm),
                        "带硬件请求打开时必须直接落在硬件修改确认页，而不是别的确认页。");
                    Assert.That(dialog.HardwareMachine, Is.SameAs(machine));

                    // ① 提交之前：机器一点没动，且提交入口是可用的。
                    Assert.That(dialog.HardwareState, Is.EqualTo(HardwareOperationState.Idle));
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.SameAs(core),
                        "确认之前槽位里还是原来那颗核心。");
                    Assert.That(core.OwnerId, Is.EqualTo(machine.Id));
                    Assert.That(dialog.HardwareConfirmCommitButton.interactable, Is.True);

                    // ② 确认页必须说清这次改动是什么、卸下会影响什么、以及算法绑定不会自动重来。
                    string change = TextsOf(dialog.HardwareConfirmChangeContent);
                    StringAssert.Contains("核心槽 0", change);
                    StringAssert.Contains(coreRow.Name, change, "要说清拆下来的到底是哪一颗。");
                    string effects = TextsOf(dialog.HardwareConfirmEffectsContent);
                    StringAssert.Contains("算法", effects, "必须说明算法绑定不会自动重绑。");

                    // ③ 提交：领域真的把组件拆下来了。
                    dialog.HardwareConfirmCommitButton.onClick.Invoke();
                    yield return null;
                    yield return null;

                    Assert.That(dialog.HardwareState, Is.EqualTo(HardwareOperationState.Completed),
                        "库中机器走 Library 来源、且没有正在执行的行为，提交后应当立即完成；"
                        + "若这里是 Rejected，多半是来源传错了。");
                    Assert.That(dialog.HardwareResult, Is.EqualTo(MachineManagementResult.Completed));
                    Assert.That(machine.GetComponent(HardwareKind.Core, 0), Is.Null,
                        "提交之后槽位必须真的空了——状态组变绿但槽位没动就是假成功。");
                    Assert.That(core.OwnerId.IsValid, Is.False,
                        "拆下来的组件回到组件库，成为可再安装的散件。");

                    // ④ 同一次确认不得被提交两次。
                    Assert.That(dialog.HardwareConfirmCommitButton.interactable, Is.False,
                        "已完成的意图不再接受第二次提交。");
                    Assert.That(dialog.HardwareConfirmKeepButton.interactable, Is.True,
                        "「保留配置」（不提交并关闭）必须始终可用，它是这页的安全出口。");
                }
                finally
                {
                    if (dialogId > 0 && GF.UI != null && GF.UI.HasUIForm(dialogId)) GF.UI.CloseUIForm(dialogId);
                    context.ReleaseActiveWorldSession();
                }
            }
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
