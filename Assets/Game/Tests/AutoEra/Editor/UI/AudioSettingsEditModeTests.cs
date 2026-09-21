using System.Collections.Generic;
using AutoEra.Settings;
using AutoEra.UI;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 声音设置的状态契约（规格 02-系统与设置 · AudioSettings）。
    ///
    /// 这一页最容易做错的是**主音量与分路音量的关系**：主音量不是第六路，而是乘到其余每一路上的总控。
    /// 如果实现成「六路并列」，主音量 50% ＋ 音乐 100% 就会听起来还是 100%；
    /// 如果实现成「读回来的值再乘一遍主音量」，则每打开一次页面音量就衰减一次。
    /// 所以这里守住的是这条换算链：
    ///
    /// <list type="bullet">
    /// <item>滑条上的原始值存在 <c>AutoEra.Audio.Bus.&lt;总线&gt;</c>，只由玩家拖动改变；</item>
    /// <item>框架键 <c>Sound.&lt;分组&gt;.Volume</c> 存的是**最终生效值**（主音量已乘进去），
    ///       这样启动时框架层只要「把每一行读回来」就能恢复，不需要知道主音量的存在；</item>
    /// <item>因此**绝不能**把框架键当原始值读回来——那会把乘过的值再乘一遍。</item>
    /// </list>
    /// </summary>
    public sealed class AudioSettingsEditModeTests
    {
        [Test]
        public void Defaults_AreAllFullVolume()
        {
            var settings = new AutoEraAudioSettings(new MemorySettingsStore());

            IReadOnlyList<AudioBus> buses = AutoEraAudioSettings.Buses;
            Assert.That(buses, Is.EqualTo(new[]
            {
                AudioBus.Main, AudioBus.Music, AudioBus.Ambient, AudioBus.Machine, AudioBus.Ui,
            }), "五路的顺序就是规格里滑条的排列顺序。");

            for (int i = 0; i < buses.Count; i++)
            {
                Assert.That(settings.GetBus(buses[i]), Is.EqualTo(AutoEraAudioSettings.DefaultBus));
                Assert.That(settings.EffectiveVolume(buses[i]), Is.EqualTo(1f));
            }

            Assert.That(settings.DifferenceFromDefaults(), Is.Zero);
        }

        [Test]
        public void MasterScalesEveryOtherBus_NotJustItsOwn()
        {
            var settings = new AutoEraAudioSettings(new MemorySettingsStore());

            Assert.That(settings.SetBus(AudioBus.Main, 0.5f, out string reason), Is.True, reason);
            Assert.That(settings.SetBus(AudioBus.Music, 0.5f, out reason), Is.True, reason);

            Assert.That(settings.GetBus(AudioBus.Music), Is.EqualTo(0.5f), "滑条上的原始值就是玩家拖到的位置。");
            Assert.That(settings.EffectiveVolume(AudioBus.Music), Is.EqualTo(0.25f).Within(0.0001f),
                "音乐最终音量 ＝ 主音量 × 音乐 —— 实现成并列就会听到 50%。");
            Assert.That(settings.EffectiveVolume(AudioBus.Ambient), Is.EqualTo(0.5f).Within(0.0001f),
                "没调过的路也要被主音量缩放。");
            Assert.That(settings.EffectiveVolume(AudioBus.Main), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void FrameworkKeysHoldTheEffectiveVolume_SoStartupNeedsNoKnowledgeOfTheMaster()
        {
            var store = new MemorySettingsStore();
            var settings = new AutoEraAudioSettings(store);

            settings.SetBus(AudioBus.Main, 0.4f, out _);
            settings.SetBus(AudioBus.Machine, 0.5f, out _);

            // 框架层（PreloadProcedure）只会读这些键并直接设进分组——所以它们必须是最终值。
            Assert.That(store.GetFloat("Sound.Machine.Volume", -1f), Is.EqualTo(0.2f).Within(0.0001f),
                "Sound.<分组>.Volume 是主音量乘完的最终音量。");
            Assert.That(store.GetFloat("Sound.Ambient.Volume", -1f), Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(store.GetFloat("Sound.Music.Volume", -1f), Is.EqualTo(0.4f).Within(0.0001f));
            // 「音效」分组在这版规格里没有独立滑条，只随主音量。
            Assert.That(store.GetFloat("Sound.Sound.Volume", -1f), Is.EqualTo(0.4f).Within(0.0001f));

            // 原始值单独一组键，两者不会互相污染。
            Assert.That(store.GetFloat("AutoEra.Audio.Bus.Machine", -1f), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void ReadingBack_DoesNotApplyTheMasterTwice()
        {
            var store = new MemorySettingsStore();
            var first = new AutoEraAudioSettings(store);
            first.SetBus(AudioBus.Main, 0.5f, out _);
            first.SetBus(AudioBus.Music, 0.6f, out _);

            // 模拟「重新打开页面 / 重启」：同一份存储上再建一个设置对象。
            var second = new AutoEraAudioSettings(store);

            Assert.That(second.GetBus(AudioBus.Music), Is.EqualTo(0.6f).Within(0.0001f),
                "原始值不该被主音量污染。");
            Assert.That(second.EffectiveVolume(AudioBus.Music), Is.EqualTo(0.3f).Within(0.0001f),
                "从框架键读回来当原始值就会变成 0.5×0.3＝0.15（再乘一遍主音量）。");
        }

        [Test]
        public void VolumeIsClampedToZeroOne()
        {
            var settings = new AutoEraAudioSettings(new MemorySettingsStore());

            settings.SetBus(AudioBus.Ui, 2.5f, out _);
            Assert.That(settings.GetBus(AudioBus.Ui), Is.EqualTo(1f));

            settings.SetBus(AudioBus.Ui, -3f, out _);
            Assert.That(settings.GetBus(AudioBus.Ui), Is.EqualTo(0f));

            settings.SetBus(AudioBus.Ui, float.NaN, out _);
            Assert.That(settings.GetBus(AudioBus.Ui), Is.EqualTo(AutoEraAudioSettings.DefaultBus),
                "NaN 不该把音量变成 0 或卡住滑条。");
        }

        [Test]
        public void PercentText_UsesWholePercent()
        {
            Assert.That(AutoEraAudioSettings.Percent(1f), Is.EqualTo("100%"));
            Assert.That(AutoEraAudioSettings.Percent(0f), Is.EqualTo("0%"));
            Assert.That(AutoEraAudioSettings.Percent(0.335f), Is.EqualTo("34%"),
                "四舍五入到整数百分比——滑条旁显示的就是这个。");
        }

        [Test]
        public void ResetToDefaults_OnlyTouchesTheFiveBuses()
        {
            var store = new MemorySettingsStore();
            var audio = new AutoEraAudioSettings(store);
            var display = new AutoEraDisplaySettings(store);

            // 先改一个显示设置：声音页的「恢复默认」不许碰它。
            display.SetFrameLimit((int)FrameLimitOption.Thirty, out _);
            audio.SetBus(AudioBus.Main, 0.2f, out _);
            audio.SetBus(AudioBus.Ui, 0.3f, out _);
            Assert.That(audio.DifferenceFromDefaults(), Is.EqualTo(2));

            Assert.That(audio.ResetToDefaults(out string reason), Is.True, reason);

            Assert.That(audio.DifferenceFromDefaults(), Is.Zero);
            for (int i = 0; i < AutoEraAudioSettings.Buses.Count; i++)
            {
                Assert.That(audio.GetBus(AutoEraAudioSettings.Buses[i]), Is.EqualTo(1f));
            }

            Assert.That(display.FrameLimit, Is.EqualTo((int)FrameLimitOption.Thirty),
                "「恢复本页默认」只恢复声音分页，保留其它分页设置（规格原文）。");
        }

        [Test]
        public void WriteFailure_KeepsTheValueAndReportsTheReason()
        {
            var store = new MemorySettingsStore { FailWrites = true };
            var settings = new AutoEraAudioSettings(store);

            Assert.That(settings.SetBus(AudioBus.Ambient, 0.25f, out string reason), Is.False);
            StringAssert.Contains("写入失败", reason);
            StringAssert.Contains("重启后可能丢失", reason, "失败说明必须讲清后果。");
            Assert.That(settings.GetBus(AudioBus.Ambient), Is.EqualTo(0.25f),
                "规格：写入失败保留内存值，不回退到旧值。");
        }

        [Test]
        public void OneWriteSavesOnce()
        {
            var store = new MemorySettingsStore();
            var settings = new AutoEraAudioSettings(store);

            settings.SetBus(AudioBus.Music, 0.5f, out _);

            Assert.That(store.SaveCount, Is.EqualTo(1),
                "一次拖动只该落盘一次——落盘是整份设置写文件，不是每个键一次。");
        }

        [Test]
        public void BusLabelsAndGroups_MatchTheSpecWording()
        {
            Assert.That(AutoEraAudioSettings.BusLabel(AudioBus.Main), Is.EqualTo("主音量"));
            Assert.That(AutoEraAudioSettings.BusLabel(AudioBus.Music), Is.EqualTo("音乐"));
            Assert.That(AutoEraAudioSettings.BusLabel(AudioBus.Ambient), Is.EqualTo("环境"));
            Assert.That(AutoEraAudioSettings.BusLabel(AudioBus.Machine), Is.EqualTo("机器与生产"));
            Assert.That(AutoEraAudioSettings.BusLabel(AudioBus.Ui), Is.EqualTo("UI 与警报"));

            Assert.That(AutoEraAudioSettings.GroupOf(AudioBus.Main), Is.Null,
                "主音量没有独立音频分组——它是总控。");
            Assert.That(AutoEraAudioSettings.GroupOf(AudioBus.Music), Is.EqualTo("Music"));
            Assert.That(AutoEraAudioSettings.GroupOf(AudioBus.Ambient), Is.EqualTo("Ambient"));
            Assert.That(AutoEraAudioSettings.GroupOf(AudioBus.Machine), Is.EqualTo("Machine"));
            Assert.That(AutoEraAudioSettings.GroupOf(AudioBus.Ui), Is.EqualTo("Ui"));
        }

        [Test]
        public void EveryGroupTheUsesActuallyExistsInTheSoundGroupTable()
        {
            // 五路总线里除主音量外都直接按名字访问音频分组；分组是**数据**（SoundGroupTable），
            // 所以"界面引用的分组确实在表里"必须有一条断言守着——否则改数据表时这里会静默失效
            // （SettingExtension 在分组不存在时是安静跳过的）。
            string text = System.IO.File.ReadAllText("Assets/Game/DataTable/Core/SoundGroupTable.txt");
            for (int i = 0; i < AutoEraAudioSettings.Buses.Count; i++)
            {
                string group = AutoEraAudioSettings.GroupOf(AutoEraAudioSettings.Buses[i]);
                if (group == null)
                {
                    continue;
                }

                StringAssert.Contains("\t" + group + "\t", text,
                    $"音频分组 {group} 必须存在于 SoundGroupTable：新增分组是数据变更，不是代码变更。");
            }

            StringAssert.Contains("\t" + AutoEraAudioSettings.SharedEffectGroup + "\t", text,
                "「音效」分组是 PlayEffect 的默认分组，不能被删掉。");
        }
    }
}
