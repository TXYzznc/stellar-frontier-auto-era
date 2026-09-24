using System;
using System.Collections.Generic;
using UnityGameFramework.Runtime;

namespace AutoEra.Settings
{
    /// <summary>
    /// 规格 02-系统与设置 · 声音页里的五路音量总线。
    ///
    /// <see cref="Main"/> 是**总控**：它不直接对应某个音频分组，而是乘到其余每一路上。
    /// 这个区别必须显式建模，否则「主音量 50% + 音乐 50%」会被实现成两个互相覆盖的值。
    /// </summary>
    public enum AudioBus
    {
        /// <summary>主音量（总控；乘到其余每一路上）。</summary>
        Main,

        /// <summary>音乐。</summary>
        Music,

        /// <summary>环境。</summary>
        Ambient,

        /// <summary>机器与生产。</summary>
        Machine,

        /// <summary>UI 与警报。</summary>
        Ui,
    }

    /// <summary>
    /// 本机声音设置：五路音量的**读取、写入、应用**集中在这里。
    ///
    /// 分工与显示设置一致：持久化走注入的 <see cref="ISettingsStore"/>（生产中就是 `GF.Setting`），
    /// 应用到音频系统走 `GF.Sound` 的分组音量。
    ///
    /// **两组键，各司其职**（这是本类最容易做错的地方）：
    /// <list type="bullet">
    /// <item><c>AutoEra.Audio.Bus.&lt;总线&gt;</c> ＝ 滑条上的**原始值**（0..1），只由玩家拖动改变；</item>
    /// <item><c>Sound.&lt;分组&gt;.Volume</c> ＝ **最终生效音量** ＝ 主音量 × 该路原始值，
    ///       沿用框架既有的键名。</item>
    /// </list>
    /// 为什么把乘好的值写进框架键：启动时 `PreloadProcedure` 只会「把每一行读回来」，
    /// 它不该、也不能知道主音量的存在（那是产品概念，在框架层里出现就是分层倒置）。
    /// 于是主音量的换算只在**写**的时候做一次，读的时候天然就是最终值。
    /// 反过来说：**绝不能**把 `Sound.&lt;分组&gt;.Volume` 读回来当原始值用——那会把乘过的值再乘一遍。
    ///
    /// 新增音频分组是**数据变更**（`GameData/AIData/DataTables/Core/SoundGroupTable.json` 加一行），
    /// 不是代码变更：本类只按名字访问分组，不引用具体的枚举值。
    /// </summary>
    public sealed class AutoEraAudioSettings
    {
        /// <summary>滑条原始值的键前缀（一路一个键）。</summary>
        public const string BusKeyPrefix = "AutoEra.Audio.Bus.";

        /// <summary>框架既有的分组音量键（`SettingExtension` 用的就是这一套）。</summary>
        public const string GroupVolumeKeyPrefix = "Sound.";

        /// <summary>没有独立滑条的音频分组：它只随主音量，不单独调。</summary>
        public const string SharedEffectGroup = "Sound";

        private static readonly AudioBus[] AllBuses =
        {
            AudioBus.Main, AudioBus.Music, AudioBus.Ambient, AudioBus.Machine, AudioBus.Ui,
        };

        private static readonly string[] AllGroups =
        {
            "Music", SharedEffectGroup, "Ambient", "Machine", "Ui",
        };

        private readonly ISettingsStore _store;

        public AutoEraAudioSettings(ISettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        /// <summary>五路总线，顺序就是规格里滑条的排列顺序。</summary>
        public static IReadOnlyList<AudioBus> Buses => AllBuses;

        /// <summary>本机设置存储；为 null 表示设置服务不可用（界面据此说明原因）。</summary>
        public ISettingsStore Store => _store;

        // ------------------------------------------------------------ 读取

        /// <summary>某一路的原始值（0..1），与滑条上的数值一致。</summary>
        public float GetBus(AudioBus bus) => Clamp(_store.GetFloat(BusKey(bus), 1f));

        /// <summary>主音量。</summary>
        public float MasterVolume => GetBus(AudioBus.Main);

        /// <summary>
        /// 某一路**最终生效**的音量：主音量 × 该路原始值。
        /// 界面显示百分比用这个值，因为玩家听到的就是它。
        /// </summary>
        public float EffectiveVolume(AudioBus bus) => bus == AudioBus.Main
            ? MasterVolume
            : Clamp(MasterVolume * GetBus(bus));

        /// <summary>没有独立滑条、只随主音量走的分组的生效音量。</summary>
        public float SharedEffectVolume => MasterVolume;

        /// <summary>可展示的分组名（规格用词）。</summary>
        public static string BusLabel(AudioBus bus) => bus switch
        {
            AudioBus.Main => "主音量",
            AudioBus.Music => "音乐",
            AudioBus.Ambient => "环境",
            AudioBus.Machine => "机器与生产",
            AudioBus.Ui => "UI 与警报",
            _ => "未知",
        };

        /// <summary>该路对应的音频分组名；主音量没有独立分组，返回 null。</summary>
        public static string GroupOf(AudioBus bus) => bus switch
        {
            AudioBus.Music => "Music",
            AudioBus.Ambient => "Ambient",
            AudioBus.Machine => "Machine",
            AudioBus.Ui => "Ui",
            _ => null,
        };

        /// <summary>百分比文本（规格：滑条旁显示百分比）。</summary>
        public static string Percent(float value) =>
            UnityEngine.Mathf.RoundToInt(Clamp(value) * 100f) + "%";

        /// <summary>与默认值的差异条数，供「本机配置状态」栏展示。</summary>
        public int DifferenceFromDefaults()
        {
            int count = 0;
            for (int i = 0; i < AllBuses.Length; i++)
            {
                if (!UnityEngine.Mathf.Approximately(GetBus(AllBuses[i]), DefaultBus)) count++;
            }

            return count;
        }

        /// <summary>默认音量：全部 100%（数据表里的分组默认也是 1，两者一致）。</summary>
        public const float DefaultBus = 1f;

        // ------------------------------------------------------------ 写入

        /// <summary>
        /// 设置某一路的原始值并立即生效。
        ///
        /// 立即生效是规格要求的（「五个独立 Sld 控件修改对应值并立即预听」）：
        /// 拖动过程中就要能听到变化，不是等松手才写。
        /// </summary>
        public bool SetBus(AudioBus bus, float value, out string reason)
        {
            reason = null;
            if (!Enum.IsDefined(typeof(AudioBus), bus))
            {
                reason = "未知的音量路。";
                return false;
            }

            _store.SetFloat(BusKey(bus), Clamp(value));
            ApplyAll();
            return Settle(out reason);
        }

        /// <summary>恢复本页默认：只动声音分页的五路音量，不碰其它分页的设置。</summary>
        public bool ResetToDefaults(out string reason)
        {
            reason = null;
            for (int i = 0; i < AllBuses.Length; i++)
            {
                _store.SetFloat(BusKey(AllBuses[i]), DefaultBus);
            }

            ApplyAll();
            return Settle(out reason);
        }

        /// <summary>只落盘，不改任何值。</summary>
        public bool Save(out string reason) => Settle(out reason);

        // ------------------------------------------------------------ 应用

        /// <summary>
        /// 把五路值与主音量换算成**每个音频分组的最终音量**，写进框架既有的键并立即生效。
        ///
        /// 非播放模式不应用（与显示设置同一条理由：编辑态不该改运行时/工程状态），
        /// 但**仍然写键**——设置值本身必须落盘。
        /// </summary>
        public void ApplyAll()
        {
            for (int i = 0; i < AllGroups.Length; i++)
            {
                string group = AllGroups[i];
                float effective = group == SharedEffectGroup
                    ? SharedEffectVolume
                    : Clamp(MasterVolume * GetBus(BusOf(group)));
                _store.SetFloat(GroupVolumeKeyPrefix + group + ".Volume", effective);
                ApplyGroupVolume(group, effective);
            }
        }

        /// <summary>
        /// 把音量交给音频系统。分组还没建出来时**静默跳过**：
        /// 设置可以在框架起来之前被读写（比如启动早期的设置加载），那不是错误。
        /// </summary>
        private static void ApplyGroupVolume(string groupName, float volume)
        {
            if (groupName == null || !UnityEngine.Application.isPlaying)
            {
                return;
            }

            try
            {
                var soundGroup = GF.Sound != null ? GF.Sound.GetSoundGroup(groupName) : null;
                if (soundGroup != null)
                {
                    soundGroup.Volume = volume;
                }
            }
            catch (Exception)
            {
                // 框架尚未就绪时 GF.Sound 可能抛异常；设置值已经落盘，下次启动会恢复。
            }
        }

        private static AudioBus BusOf(string group) => group switch
        {
            "Music" => AudioBus.Music,
            "Ambient" => AudioBus.Ambient,
            "Machine" => AudioBus.Machine,
            "Ui" => AudioBus.Ui,
            _ => AudioBus.Main,
        };

        private static string BusKey(AudioBus bus) => BusKeyPrefix + bus;

        private static float Clamp(float value) =>
            float.IsNaN(value) ? DefaultBus : UnityEngine.Mathf.Clamp01(value);

        private bool Settle(out string reason) => _store.Save(out reason);
    }
}
