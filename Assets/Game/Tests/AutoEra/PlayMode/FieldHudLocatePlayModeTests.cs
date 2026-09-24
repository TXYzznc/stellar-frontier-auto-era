using System.Collections;
using System.Linq;
using AutoEra.Application;
using AutoEra.Input;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityGameFramework.Runtime;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 现场定位：**F 键、双击与界面「聚焦」按钮必须是同一条意图通路**。
    ///
    /// 这一条不是顺手加的接线，而是规格明写的性质：14-WorldBinding 把按钮的语义直接写成
    /// 「有效对象双击或F聚焦」，00-通用合同则要求「输入、Button和快捷键汇入同一意图」。
    /// 如果界面自己实现一份（比如直接取包围盒中心），就会出现「双击聚焦到锚点、按钮聚焦到别处」
    /// 这种看得见的偏差——而两处代码都「没错」，只是各算各的。
    ///
    /// 所以用例断言两件事：
    ///   ① 按钮按下后镜头的焦点**就是**该对象的焦点锚点；
    ///   ② 把一帧「按了 F」喂进输入模块，得到的焦点与按钮**完全一致**。
    /// 第二条才是真正的证据：它证明两条路汇到了同一个实现，而不只是各自都动了镜头。
    /// </summary>
    public sealed class FieldHudLocatePlayModeTests
    {
        private const string LaunchSceneName = "Launch";
        private const string RegionSceneName = "InitialRegion";

        [UnityTest]
        public IEnumerator FieldLocate_ButtonAndFocusKeyDriveTheSameCameraTarget()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

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
            {
                if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            }

            Assert.That(entry, Is.Not.Null, "现场场景必须有 InitialRegionScene 入口。");
            RegionInputModule input = entry.GetComponent<RegionInputModule>();
            RegionCameraController camera = entry.GetComponent<RegionCameraController>();
            Assert.That(input, Is.Not.Null);
            Assert.That(camera, Is.Not.Null);

            // 把设备输入摘掉：否则测试期间真实的鼠标位置会驱动镜头平移，
            // 断言「焦点等于锚点」就会偶发失败。设备读取本来就只在输入边界里，换掉它不影响被测逻辑。
            input.SetSource(new SilentSource());

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int hudId = 0;
                bool hudOpen = false;
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

                    // 前置：主菜单默认挡世界输入，而 `Tick` 在世界输入被占用时会直接返回。
                    // 不收掉它，「喂一帧 F」根本走不到定位那一步——断言会失败在一个与本页无关的原因上。
                    yield return CloseBlockingForms();

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(context, session, entry.Region, input,
                        entry.MachineRuntimes);
                    hudId = GF.UI.OpenUIForm(UIViews.FieldHudForm, uiSession.WriteTo(UIParams.Create()));
                    hudOpen = true;
                    yield return WaitForForm(hudId, expectedLoaded: true);

                    FieldHudForm hud = GF.UI.GetUIForm(hudId).Logic as FieldHudForm;
                    Assert.That(hud, Is.Not.Null, "必须挂上 FieldHudForm 的 GF UIForm 桥。");

                    // 场景里那批 `_objects` 是**建造期模板**：InitializeRuntime 会把它们整批 SetActive(false)，
                    // 真正的对象是它按实体预制体实例化出来的（挂在自己的实体组下，可能不在同一个场景里）。
                    // 所以这里找的是全场景中**真正绑上模型**的视图，而不是初始场景的那些模板——
                    // 模板的 Model 永远是 null。
                    RegionObjectView view = null;
                    double viewUntil = Time.realtimeSinceStartupAsDouble + 30;
                    while (view == null && Time.realtimeSinceStartupAsDouble < viewUntil)
                    {
                        view = Object.FindObjectsOfType<RegionObjectView>(true).FirstOrDefault(v => v.Model != null);
                        if (view == null) yield return null;
                    }

                    Assert.That(view, Is.Not.Null, "现场场景必须有带模型的对象视图。");

                    // ① 没有选中任何东西时：定位按钮不可点——点下去没反应的入口等于界面在骗人。
                    Assert.That(input.CanFocusSelection, Is.False);
                    hud.ShowWorldTime(0L);
                    yield return null;
                    Assert.That(hud.MachineOverviewFocusButton.interactable, Is.False);
                    Assert.That(hud.FarmFocusButton.interactable, Is.False);
                    Assert.That(hud.BuildingOverviewFocusButton.interactable, Is.False);
                    Assert.That(hud.FocusSelection(), Is.False, "没有选中对象时定位必须如实返回 false。");

                    // ② 选中一个对象：入口变可用。
                    Assert.That(input.SelectTarget(view), Is.True);
                    hud.ShowWorldTime(0L);
                    yield return null;
                    Assert.That(input.CanFocusSelection, Is.True);
                    Assert.That(input.SelectedObjectId, Is.EqualTo(view.Model.Id));
                    Assert.That(hud.CanLocateSelection, Is.True);
                    Assert.That(hud.MachineOverviewFocusButton.interactable, Is.True);

                    // ③ 按钮路径：镜头焦点落到该对象的焦点锚点上。
                    camera.Focus(new Vector3(999f, 0f, 999f));
                    hud.MachineOverviewFocusButton.onClick.Invoke();
                    Vector3 byButton = camera.FocusPosition;
                    Assert.That(byButton, Is.EqualTo(view.FocusPosition),
                        "聚焦必须落在对象自己的锚点上，而不是包围盒中心或别处。");

                    // ④ 输入路径：喂一帧「按了 F」，结果必须与按钮**完全一致**。
                    camera.Focus(new Vector3(-999f, 0f, -999f));
                    input.SetSource(new FocusKeySource());
                    input.Tick(0.016f, false);
                    Vector3 byKey = camera.FocusPosition;

                    Assert.That(byKey, Is.EqualTo(byButton),
                        "F 键与按钮必须汇入同一条意图通路；各写一份就会出现看得见的偏差。");
                }
                finally
                {
                    if (hudOpen && GF.UI != null && GF.UI.HasUIForm(hudId)) GF.UI.CloseUIForm(hudId);
                    context.ReleaseActiveWorldSession();
                }
            }
        }

        /// <summary>什么都不读的输入源：测试不需要设备输入参与。</summary>
        private sealed class SilentSource : IRegionInputSource
        {
            public RegionInputFrame Read() => default;
        }

        /// <summary>只按下一帧 F 的输入源（其余全空），用来复现快捷键路径。</summary>
        private sealed class FocusKeySource : IRegionInputSource
        {
            public RegionInputFrame Read() => new RegionInputFrame { Focus = true };
        }

        /// <summary>
        /// 收掉挡世界输入的界面（主菜单是开局由流程异步打开的，所以要轮询几轮）。
        /// 世界输入被占用时 <c>RegionInputModule.Tick</c> 会直接返回，
        /// 那样「喂一帧 F」这条断言就测不到定位本身。
        /// </summary>
        private static IEnumerator CloseBlockingForms()
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
                "前置：世界输入此刻应当是空闲的，否则后面的断言测不到定位。");
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
