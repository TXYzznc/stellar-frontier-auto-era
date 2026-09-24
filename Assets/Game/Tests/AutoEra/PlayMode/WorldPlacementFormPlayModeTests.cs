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
    /// 世界放置的**机器部署页**在真实 GF 运行时下的验收（变更 `region-machine-deployment-runtime` 5.1）。
    ///
    /// 这一页有一条与其它管理页**反向**的硬性质：它**不挡世界输入**。
    /// 规格写明「世界虚影可见，底部居中操作条，只拦截 UI 占用区域，不用全屏遮罩」——
    /// 如果它照默认挡输入，玩家就会一边看着页面一边点不到世界，落位预览也永远不动。
    /// 所以这里的关键断言之一就是 `AutoEraUiRuntime.BlocksWorldInput == false`，
    /// 而且它必须在**页面已经打开**的状态下成立（⑦ 用另外两页做反向对照）。
    ///
    /// 其余断言覆盖「选点 → 校验 → 提交」这条链，并且用的是页面自己的入口
    /// （`MovePlacementTo` / `ConfirmPlacement`），而不是绕过它直接调领域——
    /// 否则页面接线错了测试照样绿。
    /// </summary>
    public sealed class WorldPlacementFormPlayModeTests
    {
        private const string LaunchSceneName = "Launch";
        private const string RegionSceneName = "InitialRegion";

        [UnityTest]
        public IEnumerator MachineDeploymentPage_DoesNotBlockTheWorld_AndCommitsThroughItsOwnEntryPoints()
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

            var input = entry.GetComponent<AutoEra.Input.RegionInputModule>();
            Assert.That(input, Is.Not.Null, "现场场景必须在区域入口同一个根节点上提供世界输入模块。");

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int formId = 0;
                bool opened = false;
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

                    // 本用例**不经主菜单**进入世界（它自己准备区域会话），而主菜单默认是挡世界输入的
                    // 一类界面——生产里进入世界时它已经关闭。把它收掉并等到输入真正空闲，
                    // 否则「打开本页之后世界输入还可用吗」这条断言就测不到本页。
                    yield return CloseBlockingFormsOutsideWorld();

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(context, session, entry.Region, input);
                    Assert.That(uiSession.HasRegionInput, Is.True, "会话必须携带世界输入模块，否则本页无法驱动预览。");

                    UIParams parameters = uiSession.WriteTo(UIParams.Create());
                    parameters.Set(AutoEraUiParamKeys.Request,
                        new AutoEraUiPageRequest(WorldPlacementForm.PageMachineDeployment, machine.Id));
                    formId = GF.UI.OpenUIForm(UIViews.WorldPlacementForm, parameters);
                    opened = true;
                    Assert.That(formId, Is.GreaterThanOrEqualTo(0), "WorldPlacementForm 必须能从 UITable 打开。");
                    yield return WaitForForm(formId, expectedLoaded: true);

                    var form = GF.UI.GetUIForm(formId).Logic as WorldPlacementForm;
                    Assert.That(form, Is.Not.Null, "必须挂上 WorldPlacementForm 的 GF UIForm 桥。");

                    // ① 页面确实在机器部署页，而且**已经开始了落位**（预览已交给现场输入模块）。
                    Assert.That(form.CurrentPage, Is.EqualTo(WorldPlacementForm.PageMachineDeployment));
                    Assert.That(form.DeploymentTarget, Is.EqualTo(machine.Id), "页面必须认打开参数里那个稳定身份。");
                    Assert.That(form.IsDeploying, Is.True);
                    Assert.That(input.Placement, Is.Not.Null, "预览必须由现场输入模块持有——那是它在世界里跟着指针走的前提。");

                    // ② 本页不挡世界输入：这是它相对其它管理页反向的一条性质。
                    Assert.That(form.BlocksWorldInput, Is.False);
                    Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.False,
                        "机器部署页打开时世界输入必须继续可用，否则预览永远不动、玩家点不到世界。");

                    // ③ 还没选点：预览非法，也还没有结局。
                    Assert.That(form.PlacementValid, Is.False);
                    Assert.That(form.DeploymentOutcome, Is.Null, "还没提交过就不该有结局。");

                    // ④ 选点 → 合法 → 通过页面自己的入口提交。
                    Assert.That(form.MovePlacementTo(new Vector2(20, -25)), Is.True);
                    Assert.That(form.PlacementValid, Is.True);

                    int objectsBefore = entry.Region.Count;
                    form.ConfirmPlacement();
                    Assert.That(form.DeploymentOutcome, Is.EqualTo(MachineDeploymentOutcome.Deployed));
                    Assert.That(form.IsDeploying, Is.False, "提交成功后本次落位就结束了。");
                    Assert.That(machine.Deployed, Is.True, "花名册必须转入已部署。");
                    Assert.That(entry.Region.Count, Is.EqualTo(objectsBefore + 1), "正好多出这台机器的区域对象。");
                    Assert.That(input.Placement, Is.Null, "提交后输入模块不得继续握着一个已经结束的预览。");

                    // ⑤ 部署成功后旋转／确认失效，取消保留（全屏根唯一的出口）。
                    Assert.That(form.MachineDeploymentRotateButton.interactable, Is.False);
                    Assert.That(form.MachineDeploymentConfirmButton.interactable, Is.False);
                    Assert.That(form.MachineDeploymentCancelButton.interactable, Is.True);

                    // ⑥ 关闭页面：预览与读模型都要收干净。
                    Assert.That(form.TryHandleIntent(AutoEraUiIntent.Cancel), Is.True);
                    yield return WaitForForm(formId, expectedLoaded: false);
                    opened = false;
                    Assert.That(input.Placement, Is.Null, "关闭页面不得把预览留在世界里。");

                    // ⑦ 反向对照：另外两页（建造放置／世界绑定）当前整页不可用，
                    //    它们**照默认挡住世界输入**，业务按钮也被整域禁用——只有出口还活着。
                    UIParams second = uiSession.WriteTo(UIParams.Create());
                    second.Set(AutoEraUiParamKeys.Request, new AutoEraUiPageRequest(WorldPlacementForm.PageBuildPlacement));
                    formId = GF.UI.OpenUIForm(UIViews.WorldPlacementForm, second);
                    opened = true;
                    yield return WaitForForm(formId, expectedLoaded: true);
                    var buildPage = GF.UI.GetUIForm(formId).Logic as WorldPlacementForm;
                    Assert.That(buildPage, Is.Not.Null);
                    Assert.That(buildPage.CurrentPage, Is.EqualTo(WorldPlacementForm.PageBuildPlacement));
                    Assert.That(buildPage.BlocksWorldInput, Is.True, "不可用的页挡住世界输入才是诚实的。");
                    Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.True);
                    Assert.That(FindButton(buildPage.gameObject, "Btn_BuildPlacementRotate").interactable, Is.False,
                        "整页不可用必须禁用业务动作。");
                    Assert.That(FindButton(buildPage.gameObject, "Btn_BuildPlacementCancel"), Is.Not.Null,
                        "取消是唯一的出口，必须保留。");
                    Assert.That(buildPage.TryHandleIntent(AutoEraUiIntent.Cancel), Is.True);
                    yield return WaitForForm(formId, expectedLoaded: false);
                    opened = false;
                }
                finally
                {
                    if (opened && GF.UI != null && GF.UI.HasUIForm(formId)) GF.UI.CloseUIForm(formId);
                    entry.Release();
                    context.ReleaseActiveWorldSession();
                }
            }

            yield return SceneManager.UnloadSceneAsync(scene);
        }

        /// <summary>
        /// 收掉世界外的界面（主菜单等）并等到世界输入真正空闲。
        ///
        /// 主菜单在场景加载后是由流程异步打开的，所以这段必须放在**区域就绪之后**再跑一次，
        /// 早跑会扑空——实测第一版就是这样：开局收一次什么也没收到，随后主菜单自己打开了。
        /// </summary>
        private static IEnumerator CloseBlockingFormsOutsideWorld()
        {
            for (int pass = 0; pass < 5 && AutoEraUiRuntime.BlocksWorldInput; pass++)
            {
                foreach (MainMenuForm menu in Object.FindObjectsOfType<MainMenuForm>(true))
                {
                    if (menu != null && GF.UI.HasUIForm(menu.Id))
                    {
                        GF.UI.CloseUIForm(menu.Id);
                    }
                }

                for (int frame = 0; frame < 60 && AutoEraUiRuntime.BlocksWorldInput; frame++) yield return null;
            }

            Assert.That(AutoEraUiRuntime.BlocksWorldInput, Is.False,
                "前置：世界输入此刻应当是空闲的，否则后面的断言测不到本页。");
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

        private static Button FindButton(GameObject root, string name)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
                if (button != null && button.name == name) return button;
            Assert.Fail("找不到按钮：" + name);
            return null;
        }
    }
}
