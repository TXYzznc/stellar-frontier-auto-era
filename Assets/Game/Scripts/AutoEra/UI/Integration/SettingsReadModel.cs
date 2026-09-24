using System;
using System.Collections.Generic;
using AutoEra.Input;
using AutoEra.Settings;
using AutoEra.World.Region;
using UnityGameFramework.Runtime;

namespace AutoEra.UI
{
    /// <summary>设置域的只读快照。三个分页各自一态：接线的分页给真实行，未接线的分页给可辨原因。</summary>
    public readonly struct SettingsDomainSnapshot
    {
        public SettingsDomainSnapshot(UiDataState displayState, string displayReason,
            IReadOnlyList<UiDetailField> displayRows, IReadOnlyList<UiDetailField> qualityRows,
            UiDataState audioState, string audioReason,
            IReadOnlyList<UiDetailField> audioRows, IReadOnlyList<UiDetailField> audioPersistenceRows,
            UiDataState controlState, string controlReason,
            IReadOnlyList<UiDetailField> controlRows, IReadOnlyList<UiDetailField> controlBindingRows,
            int initialPage)
        {
            DisplayState = displayState;
            DisplayReason = displayReason;
            DisplayRows = displayRows;
            QualityRows = qualityRows;
            AudioState = audioState;
            AudioReason = audioReason;
            AudioRows = audioRows;
            AudioPersistenceRows = audioPersistenceRows;
            ControlState = controlState;
            ControlReason = controlReason;
            ControlRows = controlRows;
            ControlBindingRows = controlBindingRows;
            InitialPage = initialPage;
        }

        /// <summary>「显示模式」栏的状态（整页级：设置组件不可用时为 Unavailable）。</summary>
        public UiDataState DisplayState { get; }
        public string DisplayReason { get; }

        /// <summary>显示模式栏的行（模式、垂直同步、帧率上限）。</summary>
        public IReadOnlyList<UiDetailField> DisplayRows { get; }

        /// <summary>质量设置栏的行（画质档位、阴影、抗锯齿、持久化状态）。</summary>
        public IReadOnlyList<UiDetailField> QualityRows { get; }

        public UiDataState AudioState { get; }
        public string AudioReason { get; }

        /// <summary>声音页·音量栏的行（五路：原始值 ＋ 最终生效值）。</summary>
        public IReadOnlyList<UiDetailField> AudioRows { get; }

        /// <summary>声音页·本机配置状态栏的行（立即生效、保存状态、与默认值的区别）。</summary>
        public IReadOnlyList<UiDetailField> AudioPersistenceRows { get; }

        public UiDataState ControlState { get; }
        public string ControlReason { get; }

        /// <summary>操作页·镜头参数栏的行（平移／旋转／缩放速度、水平反转、垂直反转）。</summary>
        public IReadOnlyList<UiDetailField> ControlRows { get; }

        /// <summary>操作页·当前按键栏的行（InputModule 的实际映射，只读）。</summary>
        public IReadOnlyList<UiDetailField> ControlBindingRows { get; }

        /// <summary>打开时应落在哪一页：第一个已接线的分页（三个都不可用时仍是规格首页）。</summary>
        public int InitialPage { get; }

        public static SettingsDomainSnapshot Unavailable(string reason, string audioReason, string controlReason, int initialPage) =>
            new SettingsDomainSnapshot(UiDataState.Unavailable, reason, null, null,
                UiDataState.Unavailable, audioReason, null, null,
                UiDataState.Unavailable, controlReason, null, null, initialPage);
    }

    /// <summary>
    /// 设置域读取模型。
    ///
    /// **这一域不需要新建持久化载体**——生产里早就有：`GF.Setting`（`SettingComponent`）
    /// 在世界启动时就被 `PreloadProcedure` 用来读写语言与声音分组音量，并已落盘。
    /// 之前它显示「未接入」的原因是**界面没有可注入的存储边界**：
    /// 界面只能直接摸 `GF.Setting` 这个全局组件，那既不可测，也会把「界面依赖框架全局状态」种回来。
    /// 本域做的是把那条边界显式化（`ISettingsStore`）并把三页分成「已接线」与「未接线」两态。
    ///
    /// 分工：显示与性能分页、声音分页已接线（前者走 `Screen` / `QualitySettings` / `Application`，
    /// 后者走 `GF.Sound` 的分组音量）；操作分页尚未接线，原因是键位重绑缺一套可持久化的绑定表。
    /// </summary>
    public interface ISettingsReadModel : IDisposable
    {
        SettingsDomainSnapshot Snapshot { get; }

        event Action Changed;

        /// <summary>重新读取本机设置（设置没有领域事件，改动都由本页发起，因此是显式刷新）。</summary>
        void Refresh();

        /// <summary>当前显示与性能设置；不可用时为 null。</summary>
        AutoEraDisplaySettings Display { get; }

        /// <summary>当前声音设置；不可用时为 null。</summary>
        AutoEraAudioSettings Audio { get; }

        /// <summary>当前操作设置；不可用时为 null。</summary>
        AutoEraControlSettings Control { get; }
    }

    /// <summary>设置域不可用时的诚实空实现。</summary>
    internal sealed class UnavailableSettingsReadModel : ISettingsReadModel
    {
        public UnavailableSettingsReadModel(SettingsDomainSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public SettingsDomainSnapshot Snapshot { get; }

        public event Action Changed
        {
            add { }
            remove { }
        }

        public void Refresh() { }

        public AutoEraDisplaySettings Display => null;

        public AutoEraAudioSettings Audio => null;

        public AutoEraControlSettings Control => null;

        public void Dispose() { }
    }

    /// <summary>真实实现：显示与性能、声音、操作三页走各自的设置类。</summary>
    internal sealed class DisplaySettingsReadModel : ISettingsReadModel
    {
        private readonly AutoEraDisplaySettings _display = null;
        private readonly AutoEraAudioSettings _audio = null;
        private readonly AutoEraControlSettings _control = null;

        /// <summary>
        /// 现场输入模块交出的**按键清单**（只读）。世界外打开设置时为 null——
        /// 那时确实没有「实际映射」可读，页面据此说明，而不是拿默认映射冒充实际映射。
        /// </summary>
        private readonly AutoEra.Input.RegionInputBindingSet _bindings;
        private readonly List<UiDetailField> _displayRows = new List<UiDetailField>(6);
        private readonly List<UiDetailField> _qualityRows = new List<UiDetailField>(6);
        private readonly List<UiDetailField> _audioRows = new List<UiDetailField>(8);
        private readonly List<UiDetailField> _audioPersistenceRows = new List<UiDetailField>(4);
        private readonly List<UiDetailField> _controlRows = new List<UiDetailField>(6);
        private readonly List<UiDetailField> _controlBindingRows = new List<UiDetailField>(8);
        private SettingsDomainSnapshot _snapshot;
        private bool _disposed;

        public DisplaySettingsReadModel(ISettingsStore store, AutoEra.Input.RegionInputBindingSet bindings = null)
        {
            _display = new AutoEraDisplaySettings(store);
            _audio = new AutoEraAudioSettings(store);
            _control = new AutoEraControlSettings(store);
            _bindings = bindings;
            Publish();
        }

        public SettingsDomainSnapshot Snapshot => _snapshot;

        public event Action Changed;

        public AutoEraDisplaySettings Display => _display;

        public AutoEraAudioSettings Audio => _audio;

        public AutoEraControlSettings Control => _control;

        public void Refresh() => Publish();

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Changed = null;
        }

        /// <summary>
        /// 重建快照后再发事件。设置没有领域事件源（改动只可能由本页发起），
        /// 所以每次写入之后由页面显式调用 `Refresh()`——这与存档域「文件系统没有推送」同理，
        /// 不去伪造一条并不存在的事件流。
        /// </summary>
        private void Publish()
        {
            // 五个列表一个都不能漏：漏掉的那个会在每次 Refresh 后**累积重复行**，
            // 表现是「改一次滑条，那一栏就多出一条」。
            _displayRows.Clear();
            _qualityRows.Clear();
            _audioRows.Clear();
            _audioPersistenceRows.Clear();
            _controlRows.Clear();
            _controlBindingRows.Clear();

            _displayRows.Add(new UiDetailField("显示模式", WindowModeLabel(_display.WindowMode)));
            _displayRows.Add(new UiDetailField("垂直同步", _display.VSyncEnabled ? "开启" : "关闭"));
            _displayRows.Add(new UiDetailField("帧率上限",
                _display.FrameLimit == (int)FrameLimitOption.Unlimited ? "不限" : _display.FrameLimit + " 帧／秒"));
            // 规格明确要求：垂直同步控制帧率时要说清上限的实际作用，不得显示互相矛盾的状态。
            _displayRows.Add(new UiDetailField("上限是否生效", _display.FrameLimitExplanation));

            _qualityRows.Add(new UiDetailField("画质档位",
                _display.QualityLevelName(_display.QualityLevel) + "（第 " + (_display.QualityLevel + 1) + " / " + _display.QualityLevelCount + " 档）"));
            _qualityRows.Add(new UiDetailField("阴影", _display.EffectiveShadowDescription));
            _qualityRows.Add(new UiDetailField("抗锯齿", _display.EffectiveAntialiasingDescription));
            _qualityRows.Add(new UiDetailField("与默认值差异",
                _display.DifferenceFromDefaults() == 0 ? "与默认值一致" : _display.DifferenceFromDefaults() + " 项不同于默认值"));

            BuildAudioRows();
            BuildControlRows();
            BuildBindingRows();

            _snapshot = new SettingsDomainSnapshot(
                UiDataState.Ready, null, _displayRows.ToArray(), _qualityRows.ToArray(),
                UiDataState.Ready, null, _audioRows.ToArray(), _audioPersistenceRows.ToArray(),
                UiDataState.Ready, null, _controlRows.ToArray(), _controlBindingRows.ToArray(),
                SettingsForm.PageAudioSettings);

            Changed?.Invoke();
        }

        /// <summary>
        /// 声音页的两栏。音量栏**同时给原始值与最终生效值**：
        /// 主音量乘下去之后，玩家调的那一格与真正听到的音量不是同一个数——
        /// 只显示其中一个，会让「主音量 50% ＋ 音乐 100%」看起来像 100% 在响。
        /// </summary>
        private void BuildAudioRows()
        {
            IReadOnlyList<AudioBus> buses = AutoEraAudioSettings.Buses;
            for (int i = 0; i < buses.Count; i++)
            {
                AudioBus bus = buses[i];
                float raw = _audio.GetBus(bus);
                string value = AutoEraAudioSettings.Percent(raw);
                if (bus != AudioBus.Main)
                {
                    value += "（主音量后 " + AutoEraAudioSettings.Percent(_audio.EffectiveVolume(bus)) + "）";
                }

                _audioRows.Add(new UiDetailField(AutoEraAudioSettings.BusLabel(bus), value));
            }

            // 「音效」分组在这版规格里没有独立滑条，它只随主音量走——
            // 明说这一条，比让玩家找不到它更诚实。
            _audioRows.Add(new UiDetailField("音效（随主音量）",
                AutoEraAudioSettings.Percent(_audio.SharedEffectVolume)));

            int difference = _audio.DifferenceFromDefaults();
            _audioPersistenceRows.Add(new UiDetailField("立即生效",
                "拖动滑条立即改变音量，不需要确认；「严重警报」也服从音量，但视觉反馈不受影响。"));
            _audioPersistenceRows.Add(new UiDetailField("保存状态",
                "音量写入本机设置；写入失败会保留内存中的值并说明重启可能丢失。"));
            _audioPersistenceRows.Add(new UiDetailField("与默认值差异",
                difference == 0 ? "与默认值一致" : difference + " 路不同于默认值"));
        }

        internal static string WindowModeLabel(DisplayWindowMode mode) => mode switch
        {
            DisplayWindowMode.ExclusiveFullScreen => "全屏（独占）",
            DisplayWindowMode.BorderlessFullScreen => "无边框全屏",
            _ => "未知",
        };

        /// <summary>
        /// 操作页·镜头参数栏。区间一并显示：玩家需要知道滑条两端代表什么，
        /// 而区间与镜头自身的夹取来自同一处（<see cref="RegionCameraParameters"/>）。
        /// </summary>
        private void BuildControlRows()
        {
            _controlRows.Add(new UiDetailField("平移速度",
                Format(_control.PanSpeed, RegionCameraParameters.MinPanSpeed, RegionCameraParameters.MaxPanSpeed)));
            _controlRows.Add(new UiDetailField("旋转速度／灵敏度",
                Format(_control.RotationSpeed, RegionCameraParameters.MinRotationSpeed, RegionCameraParameters.MaxRotationSpeed)));
            _controlRows.Add(new UiDetailField("缩放速度",
                Format(_control.ZoomSpeed, RegionCameraParameters.MinZoomSpeed, RegionCameraParameters.MaxZoomSpeed)));
            _controlRows.Add(new UiDetailField("水平反转", _control.InvertHorizontal ? "开启" : "关闭"));
            _controlRows.Add(new UiDetailField("垂直反转", _control.InvertVertical ? "开启" : "关闭"));
            _controlRows.Add(new UiDetailField("与默认值差异",
                _control.DifferenceFromDefaults() == 0
                    ? "与默认值一致"
                    : _control.DifferenceFromDefaults() + " 项不同于默认值"));
        }

        /// <summary>
        /// 操作页·当前按键栏。**只读**——设计明确第一版不开放改键
        /// （「只读显示 InputModule 当前绑定」），所以这里没有任何可写入口。
        ///
        /// 「缩放」没有按键：滚轮与指针在设备层是**通道**而不是按键，所以它照实说「设备通道」，
        /// 而不是编一个键名出来。
        /// </summary>
        private void BuildBindingRows()
        {
            if (_bindings == null)
            {
                // 世界外打开设置：确实读不到现场输入模块的实际映射，说明它，而不是拿默认映射冒充。
                return;
            }

            _controlBindingRows.Add(new UiDetailField("移动", KeyList(
                RegionInputAction.PanForward, RegionInputAction.PanBack,
                RegionInputAction.PanLeft, RegionInputAction.PanRight)));
            _controlBindingRows.Add(new UiDetailField("旋转", KeyOf(RegionInputAction.Orbit, "（按住拖动）")));
            _controlBindingRows.Add(new UiDetailField("缩放", "鼠标滚轮（设备通道，不可改键）"));
            _controlBindingRows.Add(new UiDetailField("聚焦", KeyOf(RegionInputAction.Focus, null)));
            _controlBindingRows.Add(new UiDetailField("返回", KeyOf(RegionInputAction.Cancel, null)));
            _controlBindingRows.Add(new UiDetailField("放置旋转", KeyOf(RegionInputAction.Rotate, null)));
        }

        private string KeyOf(RegionInputAction action, string suffix)
        {
            return _bindings.TryGet(action, out RegionInputBinding binding)
                ? AutoEraUiFormat.KeyLabel(binding.Key) + (suffix ?? string.Empty)
                : "未绑定";
        }

        private string KeyList(params RegionInputAction[] actions)
        {
            var text = new System.Text.StringBuilder(16);
            for (int i = 0; i < actions.Length; i++)
            {
                if (!_bindings.TryGet(actions[i], out RegionInputBinding binding))
                {
                    continue;
                }

                if (text.Length > 0)
                {
                    text.Append(" ／ ");
                }

                text.Append(AutoEraUiFormat.KeyLabel(binding.Key));
            }

            return text.Length == 0 ? "未绑定" : text.ToString();
        }

        private static string Format(float value, float min, float max) =>
            value.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)
            + "（区间 " + min.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)
            + "–" + max.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "）";
    }

    /// <summary>设置域里尚未接线的分页各自的原因。单独放一处，避免两处各写一遍而走样。</summary>
    internal static class SettingsUnavailableReasons
    {
        /// <summary>没有存储载体时，声音页的缺口：它要读写的正是音频分组音量。</summary>
        public const string NoAudioStoreReason =
            "本机设置组件尚未就绪：五路音量没有可读写的存储载体，因此声音页暂不可用。";

        /// <summary>没有存储载体时，操作页的缺口：它要读写的是镜头参数。</summary>
        public const string NoControlStoreReason =
            "本机设置组件尚未就绪：镜头参数没有可读写的存储载体，因此操作页暂不可用。";
    }

    /// <summary>设置界面的统一入口。</summary>
    public static class SettingsReadModels
    {
        /// <summary>本机设置组件不可用（框架还没起来时会出现）。</summary>
        public const string NoStoreReason =
            "本机设置组件尚未就绪：设置没有可读写的存储载体，因此本页暂不可用。";

        /// <summary>
        /// 建立读模型。`Create(session)` 走生产路径：存储取 `GF.Setting`，
        /// 按键清单取现场输入模块（世界外为 null，那一栏会说明读不到实际映射）。
        /// </summary>
        public static ISettingsReadModel Create(AutoEraUiSession session)
        {
            return Create(GfStore(), session?.RegionInput?.Bindings);
        }

        /// <summary>
        /// 建立读模型；显式传入存储的入口供测试与「已经拿到设置组件」的调用方使用。
        ///
        /// 与组件域同一个理由：为了测试让所有人都走 `GF.Setting`，会把
        /// 「界面依赖框架全局状态」悄悄种回来。
        /// </summary>
        public static ISettingsReadModel Create(ISettingsStore store,
            AutoEra.Input.RegionInputBindingSet bindings = null)
        {
            if (store == null)
            {
                return new UnavailableSettingsReadModel(SettingsDomainSnapshot.Unavailable(
                    NoStoreReason, SettingsUnavailableReasons.NoAudioStoreReason,
                    SettingsUnavailableReasons.NoControlStoreReason, SettingsForm.PageAudioSettings));
            }

            return new DisplaySettingsReadModel(store, bindings);
        }

        /// <summary>
        /// 单独取操作分页的设置对象（存储不可用时为 null）。
        ///
        /// 存在的理由是**区域入口**：镜头参数属于本机设置，玩家可能在主菜单里就调好了，
        /// 那时现场还没有镜头。所以进入区域建立镜头之后必须有人把保存的值推上去，
        /// 否则「保存了但没生效」——玩家得先打开一次设置页才会生效，那是在骗人。
        /// 入口只需要这一个设置对象，不需要整套读模型。
        /// </summary>
        public static AutoEraControlSettings CreateControlSettings() =>
            GfStore() is { } store ? new AutoEraControlSettings(store) : null;

        private static ISettingsStore GfStore()
        {
            SettingComponent component = null;
            try
            {
                component = GF.Setting;
            }
            catch (Exception)
            {
                // 框架尚未就绪时 GF.Setting 可能抛异常（组件还没注册）；界面据此说明原因即可。
                component = null;
            }

            return component != null ? new GfSettingsStore(component) : null;
        }
    }
}
