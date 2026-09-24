using System;
using System.Collections.Generic;
using AutoEra.Settings;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 用户设置（规格 02-系统与设置：声音、操作、显示与性能三页）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// **持久化载体早就存在**：`GF.Setting`（`SettingComponent`）在世界启动时就被
    /// `PreloadProcedure` 用来读写语言与声音分组音量，并且会落盘。之前本页显示「未接入」的原因是
    /// **界面没有可注入的存储边界**——界面只能直接摸那个全局组件。现在把边界显式化成
    /// `ISettingsStore`：生产传 `GF.Setting` 的适配器，测试传内存实现，两边走同一条读写路径。
    ///
    /// 三页的接线程度刻意不同，并且**各自给出原因**：
    /// <list type="bullet">
    /// <item>显示与性能：已接线。`Screen` / `QualitySettings` / `Application` 都是真实后端；</item>
    /// <item>声音：已接线。五路音量走 `GF.Sound` 的音频分组，分组本身来自 `SoundGroupTable`；</item>
    /// <item>操作：未接线——镜头速度与键位重绑都需要一张可持久化的输入绑定表，现场输入目前没有。</item>
    /// </list>
    ///
    /// **初始页落在规格首页（声音）**：三页里前两页都已接线，所以直接按规格页序打开，
    /// 不再需要「跳到第一个可用的分页」那种绕行。
    /// </summary>
    public sealed partial class SettingsForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 声音、1 操作、2 显示与性能。</summary>
        public const int PageAudioSettings = 0;
        public const int PageControlSettings = 1;
        public const int PageDisplaySettings = 2;

        private static readonly AudioBus[] AudioSliderBuses =
        {
            AudioBus.Main, AudioBus.Music, AudioBus.Ambient, AudioBus.Machine, AudioBus.Ui,
        };

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        /// <summary>导航按钮 → 规格页索引（来源：契约的 `navigationPageIndex`）。</summary>
        private static readonly int[] NavigationPageIndex = { PageAudioSettings, PageControlSettings, PageDisplaySettings };

        private ISettingsReadModel _settings;

        /// <summary>本次打开的服务会话：操作页要靠它把镜头参数落到现场镜头上。</summary>
        private AutoEraUiSession _session;

        /// <summary>渲染期间置位：滑条赋值会触发 `onValueChanged`，不挡住就会「渲染 → 写回 → 再渲染」。</summary>
        private bool _rendering;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_navButtons != null)
            {
                for (int i = 0; i < _navButtons.Length; i++)
                {
                    Button button = _navButtons[i];
                    if (button == null || i >= NavigationPageIndex.Length)
                    {
                        continue;
                    }

                    int page = NavigationPageIndex[i];
                    button.onClick.AddListener(() => ShowSettingsPage(page));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);

            WireAudioPage();
            WireControlPage();
            WireDisplayPage();
        }

        private void WireAudioPage()
        {
            Slider[] sliders = AudioSliders;
            for (int i = 0; i < sliders.Length && i < AudioSliderBuses.Length; i++)
            {
                Slider slider = sliders[i];
                if (slider == null)
                {
                    continue;
                }

                AudioBus bus = AudioSliderBuses[i];
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.wholeNumbers = false;
                slider.onValueChanged.AddListener(value =>
                {
                    if (!_rendering)
                    {
                        SetBus(bus, value);
                    }
                });
            }

            if (_audioSettingsResetButton != null) _audioSettingsResetButton.onClick.AddListener(ResetAudioDefaults);
        }

        private void WireControlPage()
        {
            Slider[] sliders = ControlSliders;
            for (int i = 0; i < sliders.Length; i++)
            {
                Slider slider = sliders[i];
                if (slider == null)
                {
                    continue;
                }

                (float min, float max) = ControlSliderRanges[i];
                slider.minValue = min;
                slider.maxValue = max;
                slider.wholeNumbers = false;
                int which = i;
                slider.onValueChanged.AddListener(value =>
                {
                    if (_rendering)
                    {
                        return;
                    }

                    switch (which)
                    {
                        case 0: SetPanSpeed(value); break;
                        case 1: SetRotationSpeed(value); break;
                        default: SetZoomSpeed(value); break;
                    }
                });
            }

            if (_controlSettingsInvertHorizontalToggle != null)
            {
                _controlSettingsInvertHorizontalToggle.onValueChanged.AddListener(value =>
                {
                    if (!_rendering) SetInvertHorizontal(value);
                });
            }

            if (_controlSettingsInvertVerticalToggle != null)
            {
                _controlSettingsInvertVerticalToggle.onValueChanged.AddListener(value =>
                {
                    if (!_rendering) SetInvertVertical(value);
                });
            }

            if (_controlSettingsResetButton != null) _controlSettingsResetButton.onClick.AddListener(ResetControlDefaults);
        }

        /// <summary>三个镜头滑条，顺序与规格一致（平移／旋转／缩放）。</summary>
        private Slider[] ControlSliders => new[]
        {
            _controlSettingsPanSpeedSlider,
            _controlSettingsRotateSpeedSlider,
            _controlSettingsZoomSpeedSlider,
        };

        /// <summary>各滑条的区间。**取自镜头参数的单一来源**，滑条与镜头不会各有一套范围。</summary>
        private static readonly (float Min, float Max)[] ControlSliderRanges =
        {
            (AutoEra.World.Region.RegionCameraParameters.MinPanSpeed,
             AutoEra.World.Region.RegionCameraParameters.MaxPanSpeed),
            (AutoEra.World.Region.RegionCameraParameters.MinRotationSpeed,
             AutoEra.World.Region.RegionCameraParameters.MaxRotationSpeed),
            (AutoEra.World.Region.RegionCameraParameters.MinZoomSpeed,
             AutoEra.World.Region.RegionCameraParameters.MaxZoomSpeed),
        };

        private void WireDisplayPage()
        {
            if (_windowModeFullscreenButton != null)
                _windowModeFullscreenButton.onClick.AddListener(() => SetWindowMode(DisplayWindowMode.ExclusiveFullScreen));
            if (_windowModeBorderlessButton != null)
                _windowModeBorderlessButton.onClick.AddListener(() => SetWindowMode(DisplayWindowMode.BorderlessFullScreen));
            if (_vSyncEnabledButton != null) _vSyncEnabledButton.onClick.AddListener(() => SetVSync(true));
            if (_vSyncDisabledButton != null) _vSyncDisabledButton.onClick.AddListener(() => SetVSync(false));
            if (_frameLimitThirtyButton != null)
                _frameLimitThirtyButton.onClick.AddListener(() => SetFrameLimit((int)FrameLimitOption.Thirty));
            if (_frameLimitSixtyButton != null)
                _frameLimitSixtyButton.onClick.AddListener(() => SetFrameLimit((int)FrameLimitOption.Sixty));
            if (_frameLimitUnlimitedButton != null)
                _frameLimitUnlimitedButton.onClick.AddListener(() => SetFrameLimit((int)FrameLimitOption.Unlimited));
            if (_qualityPresetLowButton != null) _qualityPresetLowButton.onClick.AddListener(() => SetQualityLevel(0));
            if (_qualityPresetMediumButton != null) _qualityPresetMediumButton.onClick.AddListener(() => SetQualityLevel(1));
            if (_qualityPresetHighButton != null) _qualityPresetHighButton.onClick.AddListener(() => SetQualityLevel(2));
            if (_displaySettingsResetButton != null) _displaySettingsResetButton.onClick.AddListener(ResetDisplayDefaults);
        }

        /// <summary>五路滑条，顺序与规格一致（主／音乐／环境／机器与生产／UI与警报）。</summary>
        private Slider[] AudioSliders => new[]
        {
            _audioSettingsMainVolumeSlider,
            _audioSettingsMusicVolumeSlider,
            _audioSettingsAmbientVolumeSlider,
            _audioSettingsMachineVolumeSlider,
            _audioSettingsUiVolumeSlider,
        };

        protected override void OnAutoEraOpen()
        {
            AutoEraUiPageRequest pageRequest = null;
            TryGetRequest(out pageRequest);
            int initialPage = pageRequest != null ? pageRequest.Page : PageAudioSettings;
            // 未指定页时落在第一个已接线的分页上（见类注释）。
            TryGetSession(out AutoEraUiSession session);
            _session = session;
            _settings = SettingsReadModels.Create(session);
            if (pageRequest == null)
            {
                initialPage = _settings.Snapshot.InitialPage;
            }

            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null ? _navButtons[0].gameObject : null);

            _settings.Changed += OnSettingsChanged;
            Render(_settings.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseSettings();

        protected override void OnAutoEraRecycle()
        {
            ReleaseSettings();
            base.OnAutoEraRecycle();
        }

        private void ReleaseSettings()
        {
            if (_settings == null)
            {
                return;
            }

            _settings.Changed -= OnSettingsChanged;
            _settings.Dispose();
            _settings = null;
            _session = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowSettingsPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>显示与性能分页当前的数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? DisplayDataState => _settings?.Snapshot.DisplayState;

        /// <summary>声音分页当前的数据状态。测试与调试用。</summary>
        public UiDataState? AudioDataState => _settings?.Snapshot.AudioState;

        /// <summary>操作分页当前的数据状态。测试与调试用。</summary>
        public UiDataState? ControlDataState => _settings?.Snapshot.ControlState;

        /// <summary>某一路音量的原始值（滑条上的值）；读模型未建立时为 1。测试与调试用。</summary>
        public float AudioBusValue(AudioBus bus) => _settings?.Audio?.GetBus(bus) ?? 1f;

        /// <summary>某一路音量最终生效的值（主音量已乘进去）。测试与调试用。</summary>
        public float AudioBusEffectiveValue(AudioBus bus) => _settings?.Audio?.EffectiveVolume(bus) ?? 1f;

        /// <summary>滑条上的值是否与读模型一致（用于验证渲染没有把值写丢）。测试与调试用。</summary>
        public float AudioSliderValue(AudioBus bus)
        {
            int index = System.Array.IndexOf(AudioSliderBuses, bus);
            Slider[] sliders = AudioSliders;
            return index >= 0 && index < sliders.Length && sliders[index] != null
                ? sliders[index].value
                : 0f;
        }

        /// <summary>最近一次写入的失败原因；成功时为 null。界面直接展示，不各自拼字符串。测试与调试用。</summary>
        public string LastWriteReason { get; private set; }

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnSettingsChanged() => Render(_settings.Snapshot);

        // ------------------------------------------------------------ 声音的写入

        /// <summary>设置某一路音量。规格要求立即生效（拖动过程中就能听到），因此不等编辑结束。</summary>
        public void SetBus(AudioBus bus, float value)
        {
            AutoEraAudioSettings audio = _settings?.Audio;
            if (audio == null)
            {
                return;
            }

            LastWriteReason = audio.SetBus(bus, value, out string reason) ? null : reason;
            _settings.Refresh();
        }

        private void ResetAudioDefaults()
        {
            AutoEraAudioSettings audio = _settings?.Audio;
            if (audio == null)
            {
                return;
            }

            LastWriteReason = audio.ResetToDefaults(out string reason) ? null : reason;
            _settings.Refresh();
        }

        // ------------------------------------------------------------ 操作的写入

        /// <summary>
        /// 一次写入之后统一收尾：把参数推到镜头（世界外没有镜头时是 null，那是正常情况），
        /// 再刷新页面。**不改值的合法性由设置域判定**，界面只负责把失败原因显示出来。
        /// </summary>
        private void ApplyControl(Func<AutoEraControlSettings, string> write)
        {
            AutoEraControlSettings control = _settings?.Control;
            if (control == null)
            {
                return;
            }

            LastWriteReason = write(control);
            control.ApplyTo(_session?.RegionInput?.CameraTarget);
            _settings.Refresh();
        }

        public void SetPanSpeed(float value) => ApplyControl(c => c.SetPanSpeed(value, out string r) ? null : r);

        public void SetRotationSpeed(float value) => ApplyControl(c => c.SetRotationSpeed(value, out string r) ? null : r);

        public void SetZoomSpeed(float value) => ApplyControl(c => c.SetZoomSpeed(value, out string r) ? null : r);

        public void SetInvertHorizontal(bool value) => ApplyControl(c => c.SetInvertHorizontal(value, out string r) ? null : r);

        public void SetInvertVertical(bool value) => ApplyControl(c => c.SetInvertVertical(value, out string r) ? null : r);

        private void ResetControlDefaults() => ApplyControl(c => c.ResetToDefaults(out string r) ? null : r);

        // ------------------------------------------------------------ 显示与性能的写入

        private void SetWindowMode(DisplayWindowMode mode) => Apply(d => d.SetWindowMode(mode, out string r) ? null : r);

        private void SetVSync(bool enabled) => Apply(d => d.SetVSync(enabled, out string r) ? null : r);

        private void SetFrameLimit(int limit) => Apply(d => d.SetFrameLimit(limit, out string r) ? null : r);

        private void SetQualityLevel(int level) => Apply(d => d.SetQualityLevel(level, out string r) ? null : r);

        private void ResetDisplayDefaults() => Apply(d => d.ResetToDefaults(out string r) ? null : r);

        /// <summary>
        /// 统一处理写入结果：**成功与失败都刷新**，失败时把原因留在 <see cref="LastWriteReason"/> 上。
        ///
        /// 为什么失败也要刷新：规格要求「写入失败保留内存值并说明重启可能丢失」。值确实改了
        /// （写在内存与设置组件里），只是没落盘——界面必须显示新值 + 失败原因，而不是回到旧值装没事。
        /// 传入的委托返回失败原因（成功返回 null），因此每次写入只落盘一次。
        /// </summary>
        private void Apply(System.Func<AutoEraDisplaySettings, string> write)
        {
            AutoEraDisplaySettings display = _settings?.Display;
            if (display == null)
            {
                return;
            }

            LastWriteReason = write(display);
            _settings.Refresh();
        }

        // ------------------------------------------------------------ 渲染

        private void Render(SettingsDomainSnapshot snapshot)
        {
            RenderAudioPage(snapshot);
            RenderControlPage(snapshot);
            RenderDisplayPage(snapshot);
        }

        private void RenderAudioPage(SettingsDomainSnapshot snapshot)
        {
            bool unavailable = snapshot.AudioState == UiDataState.Unavailable;
            RenderUnavailablePage(unavailable, snapshot.AudioReason,
                _audioSettingsLoadingState, _audioSettingsEmptyState, _audioSettingsErrorState,
                _audioSettingsSuccessState, _audioSettingsDisabledState,
                _audioSettingsVolumesBody, _audioSettingsPersistenceBody);

            if (!unavailable)
            {
                // 已接线的两栏各有各的正文：音量栏说清换算关系，本机配置状态栏说清落盘语义。
                // （整页不可用时两者才共用同一句原因——那时确实只有一句话可说。）
                SetText(_audioSettingsVolumesBody, VolumesHint);
                SetText(_audioSettingsPersistenceBody, LastWriteReason ?? AudioWriteHint);
            }

            RenderDetailRows(_audioSettingsVolumesTemplate, _audioSettingsVolumesContent,
                unavailable ? NoFields : snapshot.AudioRows);
            RenderDetailRows(_audioSettingsPersistenceTemplate, _audioSettingsPersistenceContent,
                unavailable ? NoFields : snapshot.AudioPersistenceRows);

            SyncAudioSliders(unavailable);
            SetAudioOptionsInteractable(!unavailable);
        }

        /// <summary>
        /// 音量栏的正文。规格要求不得显示互相矛盾的状态，而这里最容易矛盾的就是
        /// 「滑条 50%」与「实际听到 25%」——所以把换算关系直接写出来，而不是让玩家自己推。
        /// </summary>
        private const string VolumesHint =
            "拖动立即生效。主音量是所有分路的乘数：实际音量 ＝ 主音量 × 该路音量"
            + "（例如主音量 50% ＋ 音乐 50% ＝ 实际 25%）。「音效」分组没有独立滑条，只随主音量。";

        /// <summary>
        /// 把滑条位置同步成读模型里的值。
        ///
        /// 用 `SetValueWithoutNotify`：`value` 的赋值会触发 `onValueChanged`，
        /// 那会在打开页面时把「渲染」变成一次「写入」（并落盘一次），
        /// 甚至在滑条与读模型不一致时来回抖动。另外 `_rendering` 兜一层——
        /// 有些 Unity 版本/自定义控件的路径仍可能回调。
        /// </summary>
        private void SyncAudioSliders(bool unavailable)
        {
            if (unavailable || _settings?.Audio == null)
            {
                return;
            }

            _rendering = true;
            try
            {
                Slider[] sliders = AudioSliders;
                for (int i = 0; i < sliders.Length && i < AudioSliderBuses.Length; i++)
                {
                    if (sliders[i] != null)
                    {
                        sliders[i].SetValueWithoutNotify(_settings.Audio.GetBus(AudioSliderBuses[i]));
                    }
                }
            }
            finally
            {
                _rendering = false;
            }
        }

        private const string AudioWriteHint =
            "拖动即生效并写入本机设置；写入失败会保留内存中的值并说明重启可能丢失。";

        private void RenderControlPage(SettingsDomainSnapshot snapshot)
        {
            bool unavailable = snapshot.ControlState == UiDataState.Unavailable;
            RenderUnavailablePage(unavailable, snapshot.ControlReason,
                _controlSettingsLoadingState, _controlSettingsEmptyState, _controlSettingsErrorState,
                _controlSettingsSuccessState, _controlSettingsDisabledState,
                _controlSettingsCameraBody, _controlSettingsBindingsBody);

            if (!unavailable)
            {
                SetText(_controlSettingsCameraBody, LastWriteReason ?? ControlCameraHint);
                // 「当前按键」是**只读**的：第一版不开放改键（设计原文），因此没有按键时说明
                // 为什么读不到，而不是显示一份默认映射冒充现场实际映射。
                SetText(_controlSettingsBindingsBody,
                    (snapshot.ControlBindingRows == null || snapshot.ControlBindingRows.Count == 0)
                        ? ControlBindingsUnavailableHint
                        : ControlBindingsHint);
            }

            RenderDetailRows(_controlSettingsCameraTemplate, _controlSettingsCameraContent,
                unavailable ? NoFields : snapshot.ControlRows);
            RenderDetailRows(_controlSettingsBindingsTemplate, _controlSettingsBindingsContent,
                unavailable ? NoFields : snapshot.ControlBindingRows);

            SyncControlControls(unavailable);
            SetControlOptionsInteractable(!unavailable);
        }

        private const string ControlCameraHint =
            "拖动即时生效并写入本机设置；区间与镜头自身使用同一套范围，因此滑条两端就是镜头的极限。";

        private const string ControlBindingsHint =
            "以下为现场输入模块当前的实际映射，**只读**：第一版不开放改键。";

        private const string ControlBindingsUnavailableHint =
            "当前不在现场，读不到输入模块的实际映射。进入区域后这一栏显示移动、旋转、缩放、聚焦、返回与放置旋转的绑定。"
            + "镜头参数不受影响：它们属于本机设置，进入区域后自动生效。";

        /// <summary>把滑条与开关同步成读模型里的值（与声音页同一条防回环做法）。</summary>
        private void SyncControlControls(bool unavailable)
        {
            if (unavailable || _settings?.Control == null)
            {
                return;
            }

            _rendering = true;
            try
            {
                AutoEraControlSettings control = _settings.Control;
                Slider[] sliders = ControlSliders;
                float[] values = { control.PanSpeed, control.RotationSpeed, control.ZoomSpeed };
                for (int i = 0; i < sliders.Length && i < values.Length; i++)
                {
                    if (sliders[i] != null)
                    {
                        sliders[i].SetValueWithoutNotify(values[i]);
                    }
                }

                if (_controlSettingsInvertHorizontalToggle != null)
                {
                    _controlSettingsInvertHorizontalToggle.SetIsOnWithoutNotify(control.InvertHorizontal);
                }

                if (_controlSettingsInvertVerticalToggle != null)
                {
                    _controlSettingsInvertVerticalToggle.SetIsOnWithoutNotify(control.InvertVertical);
                }
            }
            finally
            {
                _rendering = false;
            }
        }

        private void RenderDisplayPage(SettingsDomainSnapshot snapshot)
        {
            bool unavailable = snapshot.DisplayState == UiDataState.Unavailable;
            string reason = unavailable ? snapshot.DisplayReason : (LastWriteReason ?? WriteHint);
            RenderUnavailablePage(unavailable, reason,
                _displaySettingsLoadingState, _displaySettingsEmptyState, _displaySettingsErrorState,
                _displaySettingsSuccessState, _displaySettingsDisabledState,
                _displaySettingsDisplayBody, _displaySettingsQualityBody);

            RenderDetailRows(_displaySettingsDisplayTemplate, _displaySettingsDisplayContent,
                unavailable ? NoFields : snapshot.DisplayRows);
            RenderDetailRows(_displaySettingsQualityTemplate, _displaySettingsQualityContent,
                unavailable ? NoFields : snapshot.QualityRows);

            // 三页都已接线：三个分页的动作都可用。
            SetDisplayOptionsInteractable(!unavailable);
        }

        private const string WriteHint =
            "修改后立即生效并写入本机设置；若写入失败会保留内存中的值并说明重启可能丢失。";

        /// <summary>
        /// 未接线分页的整页呈现：Disabled + 原因；已接线分页则是 Success。
        /// 这里刻意**不复用** `DisableDomainActions()`——它按结构名禁用页面上除出口以外的所有按钮，
        /// 而显示与性能分页的按钮正是本页现在唯一真实可用的动作。
        /// </summary>
        private void RenderUnavailablePage(bool unavailable, string reason,
            GameObject loadingState, GameObject emptyState, GameObject errorState,
            GameObject successState, GameObject disabledState,
            params TMPro.TMP_Text[] bodies)
        {
            SetState(loadingState, false);
            SetState(emptyState, false);
            SetState(errorState, false);
            SetState(disabledState, unavailable);
            SetState(successState, !unavailable);

            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] != null)
                {
                    bodies[i].SetText(reason ?? string.Empty);
                }
            }
        }

        private void SetDisplayOptionsInteractable(bool value)
        {
            SetInteractable(_windowModeFullscreenButton, value);
            SetInteractable(_windowModeBorderlessButton, value);
            SetInteractable(_vSyncEnabledButton, value);
            SetInteractable(_vSyncDisabledButton, value);
            SetInteractable(_frameLimitThirtyButton, value);
            SetInteractable(_frameLimitSixtyButton, value);
            SetInteractable(_frameLimitUnlimitedButton, value);
            SetInteractable(_qualityPresetLowButton, value);
            SetInteractable(_qualityPresetMediumButton, value);
            SetInteractable(_qualityPresetHighButton, value);
            SetInteractable(_displaySettingsResetButton, value);
        }

        private void SetAudioOptionsInteractable(bool value)
        {
            SetInteractable(_audioSettingsMainVolumeSlider, value);
            SetInteractable(_audioSettingsMusicVolumeSlider, value);
            SetInteractable(_audioSettingsAmbientVolumeSlider, value);
            SetInteractable(_audioSettingsMachineVolumeSlider, value);
            SetInteractable(_audioSettingsUiVolumeSlider, value);
            SetInteractable(_audioSettingsResetButton, value);
        }

        private void SetControlOptionsInteractable(bool value)
        {
            SetInteractable(_controlSettingsPanSpeedSlider, value);
            SetInteractable(_controlSettingsRotateSpeedSlider, value);
            SetInteractable(_controlSettingsZoomSpeedSlider, value);
            SetInteractable(_controlSettingsInvertHorizontalToggle, value);
            SetInteractable(_controlSettingsInvertVerticalToggle, value);
            SetInteractable(_controlSettingsResetButton, value);
        }

        private static void SetInteractable(Selectable selectable, bool value)
        {
            if (selectable != null)
            {
                selectable.interactable = value;
            }
        }

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null)
            {
                text.SetText(value ?? string.Empty);
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
