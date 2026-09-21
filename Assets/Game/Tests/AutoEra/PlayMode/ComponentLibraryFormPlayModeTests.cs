using System.Collections;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 组件库在真实 GF 运行时下的验收（组件域只读接入）。
    ///
    /// 只断言「界面能打开」不足以证明这一域接上了——之前它就是这样：界面打得开、状态是 Disabled、
    /// 一句「域未接入」。所以这里断言的是**两半数据真的合起来了**：
    /// 花名册里的一件散件在「散件」页渲染成一行，点行按稳定 Id 选中，
    /// 装上机器之后它出现在「已安装」页并带上机器名与槽位。
    /// </summary>
    public sealed class ComponentLibraryFormPlayModeTests
    {
        private const string LaunchSceneName = "Launch";

        [UnityTest]
        public IEnumerator ComponentLibrary_RendersRealRosterRows_AndFollowsTheComponentAcrossPages()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True, "机器与组件数据表必须在运行期就绪。");

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int formId = 0;
                bool opened = false;
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetComponentRow(20011, out ComponentDisplayRow coreRow), Is.True,
                        "数据表里必须有 20011 这一行（核心 1 级）。");

                    MachineInstance machine = session.Machines.Create(
                        new MachineDefinition(1, "组件库夹具机", 1, 2, 1, 1, 20, false, false, 100, 2d, 2d));
                    // 两件**完全相同**的散件：规格要求它们合并显示数量，展开后仍保留各实例。
                    ComponentInstance first = CreateCore(session, coreRow);
                    ComponentInstance second = CreateCore(session, coreRow);
                    Assert.That(second.Id, Is.Not.EqualTo(first.Id));

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(context, session);
                    UIParams parameters = uiSession.WriteTo(UIParams.Create());
                    parameters.Set(AutoEraUiParamKeys.Request,
                        new AutoEraUiPageRequest(ComponentLibraryForm.PageLooseComponents));
                    formId = GF.UI.OpenUIForm(UIViews.ComponentLibraryForm, parameters);
                    opened = true;
                    Assert.That(formId, Is.GreaterThanOrEqualTo(0), "ComponentLibraryForm 必须能从 UITable 打开。");
                    yield return WaitForForm(formId, expectedLoaded: true);

                    var form = GF.UI.GetUIForm(formId).Logic as ComponentLibraryForm;
                    Assert.That(form, Is.Not.Null, "必须挂上 ComponentLibraryForm 的 GF UIForm 桥。");

                    // ① 域已接线：状态是 Ready，不再是「未接入」的 Disabled。
                    Assert.That(form.ComponentDataState, Is.EqualTo(UiDataState.Ready),
                        "花名册里有组件时状态必须是 Ready——这正是这一项要修的旧行为。");

                    // ② 两件相同的散件合并成一行，但个体数仍是 2。
                    Assert.That(form.LooseComponentCount, Is.EqualTo(2));
                    Assert.That(form.LooseGroupCount, Is.EqualTo(1),
                        "完全相同的散件必须合并成一行（规格：按类型／型号／等级合并显示数量）。");

                    // ③ 列表真的渲染出那一行（走对象池，不是预制体里的静态占位）。
                    Button row = FirstRowButton(form.LooseComponentsCatalogContent);
                    Assert.That(row, Is.Not.Null, "散件页必须渲染出至少一行。");
                    row.onClick.Invoke();
                    yield return null;
                    Assert.That(form.HasGroupSelection, Is.True, "点合并行选中的是「一整组散件」。");

                    // ④ 装上机器：它离开散件、出现在已安装页，并带上机器名与槽位。
                    Assert.That(session.Machines.Install(machine.Id, ManagementOrigin.Library, first.Id, 0),
                        Is.EqualTo(MachineManagementResult.Completed));
                    yield return null;
                    Assert.That(form.LooseComponentCount, Is.EqualTo(1), "装上机器的那件不再是散件。");
                    Assert.That(form.LooseGroupCount, Is.EqualTo(1), "剩下的那一件仍占一行。");
                    Button installedRow = FirstRowButton(form.InstalledComponentsCatalogContent);
                    Assert.That(installedRow, Is.Not.Null, "已安装页必须渲染出这件组件。");
                    installedRow.onClick.Invoke();
                    yield return null;
                    Assert.That(form.SelectedComponentId, Is.EqualTo(first.Id), "点行必须按稳定 Id 选中那件实例。");

                    // ④ 取消能关闭，且关闭后不再持有读模型（读模型 Dispose 时会退订花名册）。
                    Assert.That(form.TryHandleIntent(AutoEraUiIntent.Cancel), Is.True);
                    yield return WaitForForm(formId, expectedLoaded: false);
                    opened = false;
                }
                finally
                {
                    if (opened && GF.UI != null && GF.UI.HasUIForm(formId)) GF.UI.CloseUIForm(formId);
                    context.ReleaseActiveWorldSession();
                }
            }
        }

        /// <summary>按目录行造一件真实规格的组件实例（规格来自数据表，而不是夹具里另写一份）。</summary>
        private static ComponentInstance CreateCore(AutoEraWorldSession session, ComponentDisplayRow row) =>
            session.Machines.CreateComponent(new ComponentDefinition(row.ModelId, row.Kind, row.Level,
                row.AddedCapacity, row.ComputeCapacity, row.LogicCapacity, row.HasBehavior));

        /// <summary>
        /// 内容节点下第一个**已激活**的行按钮。
        ///
        /// 不按 `UiListRowItem` 找：它不是 Component，而是 GF 对象池对象
        /// （`UIItemObject : ObjectBase`），`GetComponentsInChildren&lt;UiListRowItem&gt;` 会直接抛异常。
        /// </summary>
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
