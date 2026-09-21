using System;
using UnityGameFramework.Runtime;

namespace AutoEra.Settings
{
    /// <summary>本机设置里可以持久化的值类型。只列 <c>SettingComponent</c> 上确有对应方法的那些。</summary>
    public interface ISettingsStore
    {
        string GetString(string key, string defaultValue);
        int GetInt(string key, int defaultValue);
        bool GetBool(string key, bool defaultValue);
        float GetFloat(string key, float defaultValue);

        void SetString(string key, string value);
        void SetInt(string key, int value);
        void SetBool(string key, bool value);
        void SetFloat(string key, float value);

        /// <summary>把内存里的改动落盘；失败给出可展示的原因，不静默吞掉。</summary>
        bool Save(out string error);
    }

    /// <summary>
    /// `<see cref="SettingComponent"/>` 的适配器。
    ///
    /// 为什么要这一层：设置服务需要一个**可注入**的存储边界，否则测试只能依赖
    /// `GF.Setting` 这个全局组件在 EditMode 下也可用——那既不可靠，也会把
    /// 「界面依赖框架全局状态」这条悄悄种回来。
    /// </summary>
    public sealed class GfSettingsStore : ISettingsStore
    {
        private readonly SettingComponent _component;

        public GfSettingsStore(SettingComponent component)
        {
            _component = component ?? throw new ArgumentNullException(nameof(component));
        }

        public string GetString(string key, string defaultValue) => _component.GetString(key, defaultValue);
        public int GetInt(string key, int defaultValue) => _component.GetInt(key, defaultValue);
        public bool GetBool(string key, bool defaultValue) => _component.GetBool(key, defaultValue);
        public float GetFloat(string key, float defaultValue) => _component.GetFloat(key, defaultValue);

        public void SetString(string key, string value) => _component.SetString(key, value);
        public void SetInt(string key, int value) => _component.SetInt(key, value);
        public void SetBool(string key, bool value) => _component.SetBool(key, value);
        public void SetFloat(string key, float value) => _component.SetFloat(key, value);

        /// <summary>
        /// 落盘。
        ///
        /// **诚实说明**：框架的 `SettingComponent.Save()` 返回 `void`，它不告诉你写成功还是失败。
        /// 所以这里只能把「抛异常」当作失败信号（磁盘满、目录不可写这类真实故障都会抛），
        /// 而不是假装验证过落盘结果。静默写坏（写了一半且不抛）无法从这里发现——
        /// 这点必须让调用方知道，界面据此说的是「已写入本机设置」而不是「已验证保存成功」。
        /// </summary>
        public bool Save(out string error)
        {
            error = null;
            try
            {
                _component.Save();
                return true;
            }
            catch (Exception exception)
            {
                error = "本机设置写入失败：" + exception.Message + "。内存中的值已保留，但重启后可能丢失。";
                return false;
            }
        }
    }

    /// <summary>显示模式。刻意只有规格列出的两种：固定 1920×1080，不提供分辨率列表与窗口化。</summary>
    public enum DisplayWindowMode
    {
        /// <summary>独占全屏。</summary>
        ExclusiveFullScreen,

        /// <summary>无边框全屏。</summary>
        BorderlessFullScreen,
    }

    /// <summary>帧率上限。`Unlimited` 存 -1，与 Unity 的 `targetFrameRate` 语义一致。</summary>
    public enum FrameLimitOption
    {
        Thirty = 30,
        Sixty = 60,
        Unlimited = -1,
    }

    /// <summary>
    /// 本机显示与性能设置：**读取、写入、应用**三件事集中在这里。
    ///
    /// 分工是刻意的：
    /// <list type="bullet">
    /// <item>持久化走注入的 <see cref="ISettingsStore"/>（生产中就是 `GF.Setting`）；</item>
    /// <item>应用到引擎走 `Screen` / `QualitySettings` / `Application`。</item>
    /// </list>
    ///
    /// **编辑器里不把玩家设置写进工程配置**：`QualitySettings.SetQualityLevel` 在非播放模式下会改写
    /// `ProjectSettings/QualitySettings.asset` 并让工程变脏（那会变成一次意外提交）。
    /// 因此应用步骤只在 <c>Application.isPlaying</c> 时执行，非播放模式只落盘设置值——
    /// 这条由 `Application.isPlaying` 的判据保证，而不是靠调用方自觉。
    ///
    /// 关于垂直同步与帧率上限的关系（规格明确要求不得显示互相矛盾的状态）：
    /// Unity 在 `vSyncCount &gt; 0` 时**忽略** `targetFrameRate`。所以本类把「帧率上限是否真正生效」
    /// 作为一条独立的可展示事实暴露出去，而不是让界面各写一份判断。
    /// </summary>
    public sealed class AutoEraDisplaySettings
    {
        public const string KeyWindowMode = "AutoEra.Display.WindowMode";
        public const string KeyVSync = "AutoEra.Display.VSync";
        public const string KeyFrameLimit = "AutoEra.Display.FrameLimit";
        public const string KeyQualityLevel = "AutoEra.Display.QualityLevel";

        private readonly ISettingsStore _store;

        public AutoEraDisplaySettings(ISettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>本机设置存储；为 null 表示设置服务不可用（界面据此说明原因）。</summary>
        public ISettingsStore Store => _store;

        // ------------------------------------------------------------ 读取

        public DisplayWindowMode WindowMode
        {
            get
            {
                string stored = _store.GetString(KeyWindowMode, string.Empty);
                return Enum.TryParse(stored, out DisplayWindowMode mode) && Enum.IsDefined(typeof(DisplayWindowMode), mode)
                    ? mode
                    : DefaultWindowMode;
            }
        }

        public bool VSyncEnabled => _store.GetBool(KeyVSync, DefaultVSync);

        public int FrameLimit => _store.GetInt(KeyFrameLimit, DefaultFrameLimit);

        public int QualityLevel
        {
            get
            {
                int stored = _store.GetInt(KeyQualityLevel, DefaultQualityLevel);
                return stored >= 0 && stored < QualityLevelCount ? stored : DefaultQualityLevel;
            }
        }

        /// <summary>帧率上限此刻是否真的生效。垂直同步开启时 Unity 会忽略它——界面照这个字段说，不各写一份判断。</summary>
        public bool FrameLimitEffective => !VSyncEnabled;

        /// <summary>当前帧率上限的可展示说明。</summary>
        public string FrameLimitExplanation => VSyncEnabled
            ? "垂直同步已开启：帧率由显示器刷新率决定，这里的上限不会被使用。"
            : "上限已生效（未开启垂直同步）。";

        public int QualityLevelCount => UnityEngine.QualitySettings.names.Length;

        public string QualityLevelName(int level) =>
            level >= 0 && level < QualityLevelCount ? UnityEngine.QualitySettings.names[level] : "—";

        /// <summary>
        /// 当前**生效**的阴影配置。读的是引擎真实数值而不是设计文档里的期望值——
        /// 阴影与抗锯齿是画质档位的一部分（Unity 把按档位打包），所以只有「当前档位下实际生效的值」
        /// 是可以诚实回答的；逐档位分别列出需要换档才能读，那会让界面在打开时改引擎状态。
        /// </summary>
        public string EffectiveShadowDescription => UnityEngine.QualitySettings.shadows switch
        {
            UnityEngine.ShadowQuality.Disable => "关闭",
            UnityEngine.ShadowQuality.HardOnly => "仅硬阴影 · 距离 " + UnityEngine.QualitySettings.shadowDistance.ToString("0"),
            UnityEngine.ShadowQuality.All => "软阴影 · 距离 " + UnityEngine.QualitySettings.shadowDistance.ToString("0"),
            _ => "—",
        };

        /// <summary>当前生效的抗锯齿配置（引擎真实倍数）。</summary>
        public string EffectiveAntialiasingDescription
        {
            get
            {
                int samples = UnityEngine.QualitySettings.antiAliasing;
                return samples switch
                {
                    0 => "关闭",
                    2 => "2× 多重采样",
                    4 => "4× 多重采样",
                    8 => "8× 多重采样",
                    _ => samples + "× 多重采样",
                };
            }
        }

        // ------------------------------------------------------------ 默认值

        /// <summary>默认显示模式：无边框全屏（切窗口最不容易把玩家留在黑屏里）。</summary>
        public DisplayWindowMode DefaultWindowMode => DisplayWindowMode.BorderlessFullScreen;

        public bool DefaultVSync => true;

        public int DefaultFrameLimit => (int)FrameLimitOption.Sixty;

        /// <summary>默认画质取工程当前档位，而不是写死一个索引——工程改了默认档这里自动跟上。</summary>
        public int DefaultQualityLevel =>
            UnityEngine.QualitySettings.GetQualityLevel() is int current && current >= 0 && current < QualityLevelCount
                ? current
                : 1;

        // ------------------------------------------------------------ 写入

        public bool SetWindowMode(DisplayWindowMode mode, out string reason)
        {
            reason = null;
            if (!Enum.IsDefined(typeof(DisplayWindowMode), mode))
            {
                reason = "未知的显示模式。";
                return false;
            }

            _store.SetString(KeyWindowMode, mode.ToString());
            ApplyWindowMode(mode);
            return Settle(out reason);
        }

        public bool SetVSync(bool enabled, out string reason)
        {
            reason = null;
            _store.SetBool(KeyVSync, enabled);
            ApplyVSync(enabled);
            // 垂直同步会改变「帧率上限是否生效」，所以这里顺带把上限重新应用一次，
            // 否则会出现「界面说上限生效、引擎却不使用它」的偏差。
            ApplyFrameLimit(FrameLimit);
            return Settle(out reason);
        }

        public bool SetFrameLimit(int limit, out string reason)
        {
            reason = null;
            if (limit != (int)FrameLimitOption.Thirty && limit != (int)FrameLimitOption.Sixty
                && limit != (int)FrameLimitOption.Unlimited)
            {
                reason = "不支持的帧率上限。";
                return false;
            }

            _store.SetInt(KeyFrameLimit, limit);
            ApplyFrameLimit(limit);
            return Settle(out reason);
        }

        public bool SetQualityLevel(int level, out string reason)
        {
            reason = null;
            if (level < 0 || level >= QualityLevelCount)
            {
                reason = "不存在的画质档位。";
                return false;
            }

            _store.SetInt(KeyQualityLevel, level);
            if (CanApplyToEngine)
            {
                UnityEngine.QualitySettings.SetQualityLevel(level, true);
            }

            return Settle(out reason);
        }

        /// <summary>恢复本页默认：只动显示与性能，不碰其它分页。</summary>
        public bool ResetToDefaults(out string reason)
        {
            reason = null;
            _store.SetString(KeyWindowMode, DefaultWindowMode.ToString());
            _store.SetBool(KeyVSync, DefaultVSync);
            _store.SetInt(KeyFrameLimit, DefaultFrameLimit);
            _store.SetInt(KeyQualityLevel, DefaultQualityLevel);

            ApplyWindowMode(DefaultWindowMode);
            ApplyVSync(DefaultVSync);
            ApplyFrameLimit(DefaultFrameLimit);
            if (CanApplyToEngine)
            {
                UnityEngine.QualitySettings.SetQualityLevel(DefaultQualityLevel, true);
            }

            return Settle(out reason);
        }

        /// <summary>与默认值的差异条数，供「本机配置状态」栏展示。</summary>
        public int DifferenceFromDefaults()
        {
            int count = 0;
            if (WindowMode != DefaultWindowMode) count++;
            if (VSyncEnabled != DefaultVSync) count++;
            if (FrameLimit != DefaultFrameLimit) count++;
            if (QualityLevel != DefaultQualityLevel) count++;
            return count;
        }

        /// <summary>只落盘，不改任何值。用于「把当前设置写死」这类显式请求。</summary>
        public bool Save(out string reason) => Settle(out reason);

        // ------------------------------------------------------------ 应用

        /// <summary>
        /// 是否允许把设置应用到引擎。**非播放模式一律不应用**：
        /// `QualitySettings.SetQualityLevel` 在编辑器非播放模式下会改写
        /// `ProjectSettings/QualitySettings.asset`，让工程变脏并可能被误提交。
        /// </summary>
        internal static bool CanApplyToEngine => UnityEngine.Application.isPlaying;

        private static void ApplyWindowMode(DisplayWindowMode mode)
        {
            if (!CanApplyToEngine)
            {
                return;
            }

            UnityEngine.Screen.fullScreenMode = mode == DisplayWindowMode.ExclusiveFullScreen
                ? UnityEngine.FullScreenMode.ExclusiveFullScreen
                : UnityEngine.FullScreenMode.FullScreenWindow;
        }

        private static void ApplyVSync(bool enabled)
        {
            if (CanApplyToEngine)
            {
                UnityEngine.QualitySettings.vSyncCount = enabled ? 1 : 0;
            }
        }

        private static void ApplyFrameLimit(int limit)
        {
            if (CanApplyToEngine)
            {
                UnityEngine.Application.targetFrameRate = limit;
            }
        }

        private bool Settle(out string reason) => _store.Save(out reason);
    }
}
