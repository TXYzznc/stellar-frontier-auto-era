using System.Collections;
using AutoEra.Application;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 机器库的**部署入口**在真实 GF 运行时下的验收（变更 `region-machine-deployment-runtime` 5.2）。
    ///
    /// 这条链只有三段，但每一段都可能悄悄断掉，而且断掉之后界面看起来仍然「正常」：
    /// <list type="number">
    /// <item>没选机器时部署按钮必须**不可点**（而不是点了没反应）；</item>
    /// <item>选了库里的机器之后它必须变成可点；</item>
    /// <item>点下去必须打开**世界放置的机器部署页**，并带上那台机器的稳定身份。</item>
    /// </list>
    /// 第 3 段尤其重要：目标页是靠打开参数里的 <c>PersistentId</c> 认机器的，
    /// 少带一个参数就会变成「页面打开了，但说你没指定机器」——那种失败不会被编译器和门禁发现。
    ///
    /// 同时钉住「其余动作仍然禁用以待接入」：改名／安装／卸载／升级／出售都不该因为部署接线了
    /// 而被顺手放开。
    /// </summary>
    public sealed class MachineLibraryDeployEntryPlayModeTests
    {
        private const string LaunchSceneName = "Launch";
        private const string RegionSceneName = "InitialRegion";

        [UnityTest]
        public IEnumerator DeployEntry_IsDisabledWithoutSelection_AndOpensTheDeploymentPageWithTheStableId()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();
            yield return Verify();
        }

        private static IEnumerator Verify()
        {
            double until = Time.realtimeSinceStartupAsDouble + 30;
            while (!MachineCatalog.IsGameDataLoaded && Time.realtimeSinceStartupAsDouble < until) yield return null;
            Assert.That(MachineCatalog.IsGameDataLoaded, Is.True, "机器数据表必须在运行期就绪。");

            AsyncOperation loading = SceneManager.LoadSceneAsync(RegionSceneName, LoadSceneMode.Additive);
            Assert.That(loading, Is.Not.Null, "InitialRegion 必须在 Build Settings 里启用。");
            while (!loading.isDone) yield return null;
            Scene scene = SceneManager.GetSceneByName(RegionSceneName);
            Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
            InitialRegionScene entry = null;
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int libraryId = 0;
                int placementId = 0;
                bool libraryOpen = false;
                bool placementOpen = false;
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);

                    MachineCatalog catalog = MachineCatalog.FromLoadedGameData();
                    Assert.That(catalog.TryGetMachine(10011, out MachineDefinition wheeled), Is.True);
                    MachineInstance machine = session.Machines.Create(wheeled);
                    Assert.That(machine.Deployed, Is.False, "前置：这台机器必须还在库里。");

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(
                        context, session, entry.Region, entry.GetComponent<AutoEra.Input.RegionInputModule>(),
                        entry.MachineRuntimes);

                    UIParams parameters = uiSession.WriteTo(UIParams.Create());
                    parameters.Set(AutoEraUiParamKeys.Request,
                        new AutoEraUiPageRequest(MachineLibraryForm.PageUndeployed));
                    libraryId = GF.UI.OpenUIForm(UIViews.MachineLibraryForm, parameters);
                    libraryOpen = true;
                    Assert.That(libraryId, Is.GreaterThanOrEqualTo(0));
                    yield return WaitForForm(libraryId, expectedLoaded: true);

                    var library = GF.UI.GetUIForm(libraryId).Logic as MachineLibraryForm;
                    Assert.That(library, Is.Not.Null, "必须挂上 MachineLibraryForm 的 GF UIForm 桥。");
                    Assert.That(library.CurrentPage, Is.EqualTo(MachineLibraryForm.PageUndeployed));

                    // ① 没选机器：部署按钮不可点（而不是点了没反应）。
                    Assert.That(library.UndeployedDeployButton, Is.Not.Null);
                    Assert.That(library.UndeployedDeployButton.interactable, Is.False,
                        "没有选中机器时部署无处可去，按钮必须明确禁用。");

                    // ② 其余依赖改装／库存／经济域的动作继续禁用——部署接线不该顺手放开它们。
                    Assert.That(library.UndeployedRenameButton.interactable, Is.False);
                    Assert.That(library.UndeployedSellButton.interactable, Is.False);
                    Assert.That(library.UndeployedPrepareButton.interactable, Is.False);

                    // ③ 点第一行（走真实的行按钮，而不是绕过界面直接选）。
                    Button row = FirstRowButton(library.UndeployedMachinesCatalogContent);
                    Assert.That(row, Is.Not.Null, "库中机器列表必须渲染出至少一行。");
                    row.onClick.Invoke();
                    yield return null;

                    Assert.That(library.SelectedMachineName, Is.EqualTo(machine.Name),
                        "点行必须选中那台机器。");
                    Assert.That(library.UndeployedDeployButton.interactable, Is.True,
                        "选中一台未部署的机器之后，部署按钮必须变成可点。");

                    // ④ 点部署：必须打开世界放置的机器部署页，并带上同一个稳定身份。
                    library.UndeployedDeployButton.onClick.Invoke();
                    yield return null;

                    placementId = FindOpenForm<WorldPlacementForm>();
                    placementOpen = placementId > 0;
                    Assert.That(placementOpen, Is.True, "部署按钮必须打开 WorldPlacementForm。");
                    var placement = GF.UI.GetUIForm(placementId).Logic as WorldPlacementForm;
                    Assert.That(placement.CurrentPage, Is.EqualTo(WorldPlacementForm.PageMachineDeployment),
                        "必须直接落在机器部署页。");
                    Assert.That(placement.DeploymentTarget, Is.EqualTo(machine.Id),
                        "打开参数必须带上那台机器的稳定身份——少了它目标页只会说「没有指定机器」。");
                    Assert.That(placement.IsDeploying, Is.True, "目标页应当已经开始了这次落位。");
                }
                finally
                {
                    if (placementOpen && GF.UI != null && GF.UI.HasUIForm(placementId)) GF.UI.CloseUIForm(placementId);
                    if (libraryOpen && GF.UI != null && GF.UI.HasUIForm(libraryId)) GF.UI.CloseUIForm(libraryId);
                    entry.Release();
                    context.ReleaseActiveWorldSession();
                }
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>
        /// 列表内容节点下第一个**已激活**的行按钮。
        ///
        /// 为什么不按 <c>UiListRowItem</c> 找：它不是 Component，而是 GF 对象池的对象
        /// （`UIItemObject : ObjectBase`），`GetComponentsInChildren&lt;UiListRowItem&gt;` 会直接抛
        /// 「不是 MonoBehaviour／Component／interface」。池化行的组件是 `UIItemBase`，
        /// 而行的**动作**在 `Btn_...Row` 上——所以按 Button 找才是稳的。
        /// 传 includeInactive=false 顺带排除了未激活的模板自身。
        /// </summary>
        private static Button FirstRowButton(RectTransform content)
        {
            return content == null ? null : content.GetComponentInChildren<Button>(false);
        }

        private static int FindOpenForm<TForm>() where TForm : AutoEraUiFormBase
        {
            foreach (TForm form in Object.FindObjectsOfType<TForm>(true))
            {
                if (form != null && form.gameObject.activeInHierarchy)
                {
                    return form.Id;
                }
            }

            return 0;
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
