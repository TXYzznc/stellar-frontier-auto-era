using System.Collections;
using AutoEra.Application;
using AutoEra.Settings;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.PlayMode
{
    /// <summary>
    /// 用户设置在真实 GF 运行时下的验收（三页都已接线后重写）。
    ///
    /// 这一项要修掉的旧行为是「一句话盖住整页」：设置页原本三页一律 Disabled 并写同一句
    /// 「还没有持久化载体」。后来显示与性能接线、声音接线、操作接线，三页现在**都**是 Ready，
    /// 因此用例不能再靠「某页不可用」来证明自己跑到过别的页——改成分页各自断言真实后端。
    ///
    /// 两页刻意走**整条真实链路**，而不是断言「页面上写着什么」：
    /// <list type="bullet">
    /// <item>声音：拖滑条 → 落盘到 `GF.Setting` → 换算成最终音量 → 写进真实音频分组，
    ///       断言落在 `GF.Sound.GetSoundGroup(...).Volume` 上；</item>
    /// <item>操作：拖滑条／勾反转 → 落盘 → **推到现场镜头上**，断言落在
    ///       `RegionCameraController` 的对应属性上。只断言页面上的数字会漏掉
    ///       「界面在自说自话」这种失败——操作页最容易留下的就是这种假接线。</item>
    /// </list>
    ///
    /// 结束时两页都恢复默认，避免把开发机上的持久化设置留在测试改过的值上。
    ///
    /// 刻意**不点显示模式那两个按钮**：那会真的切换编辑器的全屏模式（写型副作用）。
    /// </summary>
    public sealed class SettingsFormPlayModeTests
    {
        private const string LaunchSceneName = "Launch";
        private const string RegionSceneName = "InitialRegion";

        [UnityTest]
        public IEnumerator Settings_AllThreePagesAreWired_AndAudioAndCameraWritesReachTheirRealBackends()
        {
            yield return EnsureLaunchSceneLoaded();
            yield return WaitForRuntimeReady();

            int formId = 0;
            bool opened = false;
            SettingsForm form = null;
            try
            {
                formId = GF.UI.OpenUIForm(UIViews.SettingsForm, UIParams.Create());
                opened = true;
                Assert.That(formId, Is.GreaterThanOrEqualTo(0), "SettingsForm 必须能从 UITable 打开。");
                yield return WaitForForm(formId, expectedLoaded: true);

                form = GF.UI.GetUIForm(formId).Logic as SettingsForm;
                Assert.That(form, Is.Not.Null, "必须挂上 SettingsForm 的 GF UIForm 桥。");

                // ① 三页都已接线：状态各自 Ready，而且控件真的能点（不是「碰巧没绑定」）。
                Assert.That(form.AudioDataState, Is.EqualTo(UiDataState.Ready),
                    "声音分页已接线：音频分组来自 SoundGroupTable，音量走 GF.Sound。");
                Assert.That(form.DisplayDataState, Is.EqualTo(UiDataState.Ready));
                Assert.That(form.ControlDataState, Is.EqualTo(UiDataState.Ready),
                    "操作分页已接线：镜头速度是既有的 RegionCameraController，不是待建系统。");
                Assert.That(form.AudioSettingsMusicVolumeSlider.interactable, Is.True);
                Assert.That(form.QualityPresetMediumButton.interactable, Is.True);
                Assert.That(form.ControlSettingsPanSpeedSlider.interactable, Is.True);

                // ② 初始页按规格页序落在声音页（第一页）。
                Assert.That(form.CurrentPage, Is.EqualTo(SettingsForm.PageAudioSettings));

                // ③ 声音分页走真实后端：拖滑条 → 真实分组音量变化。
                foreach (string group in new[] { "Music", "Sound", "Ambient", "Machine", "Ui" })
                {
                    Assert.That(GF.Sound.HasSoundGroup(group), Is.True,
                        $"音频分组 {group} 必须由 SoundGroupTable 建成。");
                }

                form.AudioSettingsMainVolumeSlider.value = 1f;
                form.AudioSettingsMusicVolumeSlider.value = 0.5f;
                yield return null;

                Assert.That(form.AudioBusValue(AudioBus.Music), Is.EqualTo(0.5f).Within(0.0001f));
                Assert.That(GF.Sound.GetSoundGroup("Music").Volume, Is.EqualTo(0.5f).Within(0.0001f),
                    "音乐分组必须真的变成 50%——界面只写自己的快照就是自说自话。");

                // ④ 主音量是总控：它乘到每一路上，不改各路的原始值。
                form.AudioSettingsMainVolumeSlider.value = 0.5f;
                yield return null;

                Assert.That(form.AudioBusValue(AudioBus.Music), Is.EqualTo(0.5f).Within(0.0001f),
                    "主音量不该改写音乐滑条自己的值。");
                Assert.That(GF.Sound.GetSoundGroup("Music").Volume, Is.EqualTo(0.25f).Within(0.0001f),
                    "音乐最终音量 ＝ 主音量 × 音乐 ＝ 0.25。");
                Assert.That(GF.Sound.GetSoundGroup("Ambient").Volume, Is.EqualTo(0.5f).Within(0.0001f),
                    "没单独调过的路也要被主音量缩放。");

                // ⑤ 音量栏要显示百分比，而且**原始值与最终生效值都要给**。
                string volumes = TextsOf(form.AudioSettingsVolumesContent);
                StringAssert.Contains("50%", volumes, "音量栏必须显示百分比（规格：滑条旁显示百分比）。");
                StringAssert.Contains("25%", volumes,
                    "主音量已乘进去，最终生效值必须显示出来，否则页面会显示互相矛盾的状态。");

                // ⑥ 世界外打开设置：读不到现场输入模块的实际映射，页面必须**说明**
                //    而不是拿一份默认映射冒充——那会让玩家以为界面认识他的键位。
                Assert.That(form.ShowSettingsPage(SettingsForm.PageControlSettings), Is.True);
                yield return null;
                StringAssert.Contains("读不到", form.ControlSettingsBindingsBody.text,
                    "世界外打开设置时，按键栏必须如实说明读不到现场映射。");

                // ⑦ 关掉世界外的那个实例，换到**带现场会话**的实例上验收操作分页的真实落点。
                GF.UI.CloseUIForm(formId);
                opened = false;
                form = null;
                yield return WaitForForm(formId, expectedLoaded: false);

                yield return VerifyControlPageAgainstTheLiveCamera();
            }
            finally
            {
                // 把两页都恢复到默认：测试不该改开发机上的持久化设置。
                if (form != null)
                {
                    if (form.AudioSettingsResetButton != null) form.AudioSettingsResetButton.onClick.Invoke();
                    if (form.ControlSettingsResetButton != null) form.ControlSettingsResetButton.onClick.Invoke();
                }

                if (opened && GF.UI != null && GF.UI.HasUIForm(formId)) GF.UI.CloseUIForm(formId);
            }
        }

        /// <summary>
        /// 操作分页的端到端：现场场景建好镜头与会话，页面写入后**真镜头的属性**必须变。
        /// </summary>
        private static IEnumerator VerifyControlPageAgainstTheLiveCamera()
        {
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
            var input = entry.GetComponent<AutoEra.Input.RegionInputModule>();
            var camera = entry.GetComponent<RegionCameraController>();
            Assert.That(input, Is.Not.Null, "现场场景必须在区域入口同一个根节点上提供世界输入模块。");
            Assert.That(camera, Is.Not.Null, "现场场景必须提供镜头控制组件。");
            Assert.That(input.CameraTarget, Is.SameAs(camera),
                "会话交出的应用目标必须是现场这台镜头——换成别的实例，设置就落到没人看的地方。");

            using (AutoEraApplicationContext context = new AutoEraApplicationCompositionRoot().Create())
            {
                int formId = 0;
                bool opened = false;
                SettingsForm form = null;
                try
                {
                    Assert.That(context.TryCreateWorldSession(0, out AutoEraWorldSession session), Is.True);
                    bool ready = false;
                    string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    double until = Time.realtimeSinceStartupAsDouble + 25;
                    while (!ready && failure == null && Time.realtimeSinceStartupAsDouble < until) yield return null;
                    Assert.That(failure, Is.Null);
                    Assert.That(ready, Is.True);

                    AutoEraUiSession uiSession = AutoEraUiSession.ForWorld(context, session, entry.Region, input,
                        entry.MachineRuntimes);
                    Assert.That(uiSession.HasRegionInput, Is.True);

                    UIParams parameters = uiSession.WriteTo(UIParams.Create());
                    parameters.Set(AutoEraUiParamKeys.Request,
                        new AutoEraUiPageRequest(SettingsForm.PageControlSettings));
                    formId = GF.UI.OpenUIForm(UIViews.SettingsForm, parameters);
                    opened = true;
                    yield return WaitForForm(formId, expectedLoaded: true);

                    form = GF.UI.GetUIForm(formId).Logic as SettingsForm;
                    Assert.That(form, Is.Not.Null);
                    Assert.That(form.CurrentPage, Is.EqualTo(SettingsForm.PageControlSettings),
                        "打开参数指定的分页必须生效。");

                    // 现场会话下按键栏必须给出**实际映射**（与 ⑥ 的世界外情况相对）。
                    string bindings = TextsOf(form.ControlSettingsBindingsContent);
                    StringAssert.Contains("设备通道", bindings,
                        "「缩放」照实说它是设备通道；滚轮不是可改的按键。");

                    // ① 拖平移滑条 → 本机设置写进去了 **且现场镜头的属性真的变了**。
                    float defaultPan = RegionCameraParameters.DefaultPanSpeed;
                    form.ControlSettingsPanSpeedSlider.value = defaultPan + 7f;
                    yield return null;

                    Assert.That(GF.Setting.GetFloat(AutoEraControlSettings.KeyPanSpeed, -1f),
                        Is.EqualTo(defaultPan + 7f).Within(0.0001f),
                        "值必须真的写进本机设置（GF.Setting 是持久化载体）。");
                    Assert.That(camera.PanSpeed, Is.EqualTo(defaultPan + 7f).Within(0.0001f),
                        "现场镜头必须真的跟着变——只改界面快照就是假接线。");

                    // ② 勾反转 → 现场镜头的反转开关真的打开（反转是本批补的实现）。
                    form.ControlSettingsInvertHorizontalToggle.isOn = true;
                    yield return null;
                    Assert.That(camera.InvertHorizontal, Is.True,
                        "反转此前根本没有实现：勾了不生效是本页最容易留下的假接线。");

                    // ③ 恢复本页默认 → 现场镜头回到默认，且只动本页。
                    form.ControlSettingsResetButton.onClick.Invoke();
                    yield return null;
                    Assert.That(camera.PanSpeed, Is.EqualTo(RegionCameraParameters.DefaultPanSpeed).Within(0.0001f));
                    Assert.That(camera.InvertHorizontal, Is.False);
                    Assert.That(GF.Sound.GetSoundGroup("Music").Volume, Is.GreaterThan(0f),
                        "操作页的「恢复本页默认」不该动声音。");
                }
                finally
                {
                    if (form != null && form.ControlSettingsResetButton != null)
                    {
                        form.ControlSettingsResetButton.onClick.Invoke();
                    }

                    if (opened && GF.UI != null && GF.UI.HasUIForm(formId)) GF.UI.CloseUIForm(formId);
                }
            }
        }

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
