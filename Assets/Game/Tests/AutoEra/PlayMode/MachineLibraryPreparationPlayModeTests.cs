using System.Collections;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 机器整备页在真实 GF 运行时下的验收（规格 05-机器整备 三栏）。
    ///
    /// 这一页原本三栏一律显示「装配、载体与就绪数据尚未接入」。现在三栏都是真实数据，
    /// 所以断言必须落在**内容**上：槽位是不是逐个列出了、装着的那一格有没有型号名、
    /// 解锁那一条有没有明说缺口。只看状态组会漏掉「三栏都空着但状态是 Success」这种失败。
    ///
    /// 一个 PlayMode 类只放一条用例：第二条用例会重新引导框架，而上一轮的世界对象已被销毁
    /// （实测报 `MissingReferenceException: UIComponent has been destroyed`）。
    /// </summary>
    public sealed class MachineLibraryPreparationPlayModeTests
    {
        private const string LaunchSceneName = "Launch";

        [UnityTest]
        public IEnumerator PreparationEntry_OpensThePreparationPage_AndRendersRealSlotRows()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True);

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int libraryId = 0;
                bool opened = false;
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition wheeled), Is.True);

                    MachineInstance machine = session.Machines.Create(wheeled);
                    // 装一颗核心：整备页必须把「哪一个槽位装着什么」逐个列出来。
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
                    opened = true;
                    yield return WaitForForm(libraryId, expectedLoaded: true);

                    var library = GF.UI.GetUIForm(libraryId).Logic as MachineLibraryForm;
                    Assert.That(library, Is.Not.Null);

                    // 选中一台库中机器 → 「整备」可点；点它必须落到整备页。
                    Button row = FirstRowButton(library.UndeployedMachinesCatalogContent);
                    Assert.That(row, Is.Not.Null);
                    row.onClick.Invoke();
                    yield return null;
                    Assert.That(library.UndeployedPrepareButton.interactable, Is.True,
                        "库中机器应当可以整备（规格 05-机器整备 的入口就是 09-未部署机器→整备）。");

                    library.UndeployedPrepareButton.onClick.Invoke();
                    yield return null;
                    Assert.That(library.CurrentPage, Is.EqualTo(MachineLibraryForm.PagePreparation));

                    // 三栏必须渲染出真实行：槽位逐个列出，且装着的那一格带型号名与状态。
                    string assembly = TextsOf(library.MachinePreparationAssemblyContent);
                    StringAssert.Contains("核心槽 0", assembly, "槽位必须逐个列出。");
                    StringAssert.Contains(coreRow.Name, assembly, "已装槽位要显示型号名。");
                    StringAssert.Contains("已装组件", assembly);

                    string readiness = TextsOf(library.MachinePreparationReadinessContent);
                    StringAssert.Contains("出售资格", readiness);
                    StringAssert.Contains("成长解锁", readiness,
                        "解锁域没有创建者，这一行必须明说而不是编一个「已解锁」。");

                    string carrier = TextsOf(library.MachinePreparationCarrierContent);
                    StringAssert.Contains("兼容安装位", carrier);
                }
                finally
                {
                    if (opened && GF.UI != null && GF.UI.HasUIForm(libraryId)) GF.UI.CloseUIForm(libraryId);
                    context.ReleaseActiveWorldSession();
                }
            }
        }

        /// <summary>把一栏里已激活行条目的文本拼起来——用于断言「真的渲染了内容」而不是只看状态组。</summary>
        private static string TextsOf(RectTransform content)
        {
            if (content == null)
            {
                return string.Empty;
            }

            var text = new System.Text.StringBuilder(256);
            foreach (TMPro.TMP_Text item in content.GetComponentsInChildren<TMPro.TMP_Text>(false))
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
