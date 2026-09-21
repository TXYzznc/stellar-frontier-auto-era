using System.Collections.Generic;
using AutoEra.Settings;
using AutoEra.UI;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 设置域读取模型与写入路径的状态契约。
    ///
    /// 这一域的特点是**持久化载体早就在生产里**（`GF.Setting`），缺的是界面能注入的存储边界。
    /// 所以这里要钉的是一条边界性质：**读写只经过 <see cref="ISettingsStore"/>**，
    /// 因此测试可以完全脱离框架验证整条链路——包括「写入失败时值仍保留在内存里」这条规格要求。
    ///
    /// 另外两条容易写错的语义也在这里守着：
    /// ① 垂直同步开启时帧率上限**不生效**，界面不得显示互相矛盾的状态（规格原文）；
    /// ② 编辑器非播放模式**不得**把设置应用回引擎——`QualitySettings.SetQualityLevel` 会改写
    ///    `ProjectSettings/QualitySettings.asset`，那会变成一次意外提交。
    /// </summary>
    public sealed class SettingsReadModelEditModeTests
    {
        [Test]
        public void MissingStore_ReportsUnavailableAndKeepsTheThreeReasonsDistinct()
        {
            using (ISettingsReadModel model = SettingsReadModels.Create((ISettingsStore)null))
            {
                SettingsDomainSnapshot snapshot = model.Snapshot;
                Assert.That(snapshot.DisplayState, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(snapshot.DisplayReason, Does.Contain("设置组件"));
                Assert.That(snapshot.AudioState, Is.EqualTo(UiDataState.Unavailable));
                Assert.That(snapshot.ControlState, Is.EqualTo(UiDataState.Unavailable));

                // 三个原因必须互不相同：一句「设置不可用」会让排查的人无从下手。
                Assert.That(new[] { snapshot.DisplayReason, snapshot.AudioReason, snapshot.ControlReason }, Is.Unique);
                Assert.That(snapshot.AudioReason, Does.Contain("音量"), "声音页的原因要指到真正的缺口。");
                Assert.That(snapshot.ControlReason, Does.Contain("镜头参数"),
                    "操作页的原因要指到真正的缺口：它读写的是镜头参数。");
                Assert.That(snapshot.ControlReason, Does.Not.Contain("绑定"),
                    "「缺可持久化的绑定表」曾经被写成这一页的缺口，那是把设计明确排除的改键当成了待办。");
                Assert.That(model.Display, Is.Null);
                Assert.That(model.Audio, Is.Null);
                Assert.That(model.Control, Is.Null);
            }
        }

        [Test]
        public void RealStore_MakesTheControlPageReadyWithTheCameraRows()
        {
            var store = new MemorySettingsStore();
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                SettingsDomainSnapshot snapshot = model.Snapshot;
                Assert.That(snapshot.ControlState, Is.EqualTo(UiDataState.Ready),
                    "镜头速度是既有后端（RegionCameraController 的三个序列化字段），本页已接线。");
                Assert.That(snapshot.ControlReason, Is.Null);

                Assert.That(snapshot.ControlRows.Count, Is.EqualTo(6),
                    "三项速度 ＋ 两项反转 ＋ 与默认值差异。");
                Assert.That(Value(snapshot.ControlRows, "平移速度"), Does.Contain("区间"),
                    "区间要一并显示：滑条两端代表什么，玩家得看得见。");
                Assert.That(Value(snapshot.ControlRows, "水平反转"), Is.EqualTo("关闭"));
                Assert.That(Value(snapshot.ControlRows, "与默认值差异"), Is.EqualTo("与默认值一致"));
            }
        }

        [Test]
        public void ControlPage_WithoutTheLiveInputModule_SaysSoInsteadOfShowingDefaults()
        {
            // 世界外打开设置：确实读不到现场映射，页面必须说明，而不是拿默认映射冒充。
            using (ISettingsReadModel model = SettingsReadModels.Create(new MemorySettingsStore()))
            {
                Assert.That(model.Snapshot.ControlBindingRows, Is.Empty);
            }

            var bindings = AutoEra.Input.RegionInputBindingSet.CreateDefault();
            using (ISettingsReadModel model = SettingsReadModels.Create(new MemorySettingsStore(), bindings))
            {
                IReadOnlyList<UiDetailField> rows = model.Snapshot.ControlBindingRows;
                Assert.That(rows.Count, Is.EqualTo(6), "移动／旋转／缩放／聚焦／返回／放置旋转。");

                Assert.That(Labels(rows), Does.Contain("缩放"));
                Assert.That(Value(rows, "缩放"), Does.Contain("设备通道"),
                    "滚轮与指针在设备层是通道而不是按键——不编一个键名出来。");
            }
        }

        [Test]
        public void RefreshDoesNotAccumulateRows()
        {
            // 值类型快照那条规则的另一面：列表在每次 Publish 时都要清空重建，
            // 漏清一个列表的表现是「改一次值，那一栏就多出一条」。
            using (ISettingsReadModel model = SettingsReadModels.Create(new MemorySettingsStore()))
            {
                int control = model.Snapshot.ControlRows.Count;
                model.Refresh();
                model.Refresh();
                Assert.That(model.Snapshot.ControlRows.Count, Is.EqualTo(control));
                Assert.That(model.Snapshot.DisplayRows.Count, Is.EqualTo(4));
                Assert.That(model.Snapshot.AudioRows.Count, Is.EqualTo(AutoEraAudioSettings.Buses.Count + 1));
            }
        }

        [Test]
        public void RealStore_MakesTheAudioPageReadyWithFiveBuses()
        {
            using (ISettingsReadModel model = SettingsReadModels.Create(new MemorySettingsStore()))
            {
                SettingsDomainSnapshot snapshot = model.Snapshot;
                Assert.That(snapshot.AudioState, Is.EqualTo(UiDataState.Ready),
                    "音频分组来自 SoundGroupTable，五路音量走 GF.Sound——本页已接线。");
                Assert.That(snapshot.AudioReason, Is.Null);

                // 五路滑条 ＋ 一条「音效随主音量」的说明行：规格里没有音效滑条，
                // 但那个分组的音量确实存在，不说会让人找不到。
                Assert.That(snapshot.AudioRows.Count, Is.EqualTo(AutoEraAudioSettings.Buses.Count + 1));
                Assert.That(Value(snapshot.AudioRows, "主音量"), Is.EqualTo("100%"));
                Assert.That(Value(snapshot.AudioRows, "音效（随主音量）"), Is.EqualTo("100%"));
                Assert.That(snapshot.AudioPersistenceRows.Count, Is.EqualTo(3));

                // 初始页按规格页序落在声音页：三页里前两页都已接线，不再需要绕行。
                Assert.That(snapshot.InitialPage, Is.EqualTo(SettingsForm.PageAudioSettings));
            }
        }

        [Test]
        public void RealStore_MakesTheDisplayPageReadyWithRealRows()
        {
            var store = new MemorySettingsStore();
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                SettingsDomainSnapshot snapshot = model.Snapshot;
                Assert.That(snapshot.DisplayState, Is.EqualTo(UiDataState.Ready));
                Assert.That(snapshot.DisplayReason, Is.Null);
                Assert.That(snapshot.InitialPage, Is.EqualTo(SettingsForm.PageAudioSettings),
                    "初始页落在第一个已接线的分页上——按规格页序，声音页现在是第一页。");

                CollectionAssert.Contains(Labels(snapshot.DisplayRows), "显示模式");
                CollectionAssert.Contains(Labels(snapshot.DisplayRows), "垂直同步");
                CollectionAssert.Contains(Labels(snapshot.DisplayRows), "帧率上限");
                CollectionAssert.Contains(Labels(snapshot.DisplayRows), "上限是否生效");
                CollectionAssert.Contains(Labels(snapshot.QualityRows), "画质档位");
                CollectionAssert.Contains(Labels(snapshot.QualityRows), "阴影");
                CollectionAssert.Contains(Labels(snapshot.QualityRows), "抗锯齿");
                CollectionAssert.Contains(Labels(snapshot.QualityRows), "与默认值差异");
                Assert.That(model.Display, Is.Not.Null);
            }
        }

        [Test]
        public void WritingAValue_PersistsItAndRefreshesTheRows()
        {
            var store = new MemorySettingsStore();
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                Assert.That(model.Display.SetFrameLimit((int)FrameLimitOption.Thirty, out string reason), Is.True, reason);
                Assert.That(store.GetInt(AutoEraDisplaySettings.KeyFrameLimit, 0), Is.EqualTo(30), "值必须真的写进存储。");

                model.Refresh();
                Assert.That(Value(model.Snapshot.DisplayRows, "帧率上限"), Is.EqualTo("30 帧／秒"));

                Assert.That(model.Display.SetWindowMode(DisplayWindowMode.ExclusiveFullScreen, out reason), Is.True, reason);
                model.Refresh();
                Assert.That(Value(model.Snapshot.DisplayRows, "显示模式"), Does.Contain("独占"));
            }
        }

        [Test]
        public void VSyncSupressesTheFrameLimit_AndTheRowsSaySo()
        {
            var store = new MemorySettingsStore();
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                Assert.That(model.Display.SetFrameLimit((int)FrameLimitOption.Thirty, out _), Is.True);
                Assert.That(model.Display.SetVSync(true, out _), Is.True);

                // 规格：垂直同步控制帧率时必须说明帧率上限的实际作用，不得显示互相矛盾的状态。
                Assert.That(model.Display.FrameLimitEffective, Is.False);
                model.Refresh();
                Assert.That(Value(model.Snapshot.DisplayRows, "上限是否生效"), Does.Contain("不会被使用"));

                Assert.That(model.Display.SetVSync(false, out _), Is.True);
                Assert.That(model.Display.FrameLimitEffective, Is.True);
                Assert.That(model.Display.FrameLimit, Is.EqualTo(30), "关掉垂直同步后上限值必须还在。");
            }
        }

        [Test]
        public void InvalidValues_AreRejectedWithAReasonAndDoNotChangeTheStoredValue()
        {
            var store = new MemorySettingsStore();
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                Assert.That(model.Display.SetFrameLimit(45, out string limitReason), Is.False);
                Assert.That(limitReason, Is.Not.Null.And.Not.Empty);
                Assert.That(store.GetInt(AutoEraDisplaySettings.KeyFrameLimit, -999), Is.Not.EqualTo(45));

                Assert.That(model.Display.SetQualityLevel(99, out string qualityReason), Is.False);
                Assert.That(qualityReason, Is.Not.Null.And.Not.Empty);
            }
        }

        [Test]
        public void ResetToDefaults_OnlyTouchesTheDisplayAndQualityKeys()
        {
            var store = new MemorySettingsStore();
            store.SetString("SomeOther.Key", "untouched");
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                Assert.That(model.Display.SetFrameLimit((int)FrameLimitOption.Thirty, out _), Is.True);
                Assert.That(model.Display.SetQualityLevel(0, out _), Is.True);
                Assert.That(model.Display.DifferenceFromDefaults(), Is.GreaterThan(0));

                Assert.That(model.Display.ResetToDefaults(out string reason), Is.True, reason);
                Assert.That(model.Display.DifferenceFromDefaults(), Is.Zero, "恢复默认之后不应再有差异项。");
                Assert.That(store.GetString("SomeOther.Key", null), Is.EqualTo("untouched"),
                    "恢复本页默认只动显示与性能，不得碰其它分页的键。");
            }
        }

        [Test]
        public void WriteFailure_KeepsTheValueAndReportsTheReason()
        {
            var store = new MemorySettingsStore { FailWrites = true };
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                Assert.That(model.Display.SetFrameLimit((int)FrameLimitOption.Thirty, out string reason), Is.False);
                StringAssert.Contains("写入失败", reason);
                StringAssert.Contains("重启后可能丢失", reason, "失败说明必须讲清后果。");

                // 规格：写入失败保留内存值。所以读回来必须是新值，而不是回退到旧值。
                Assert.That(store.GetInt(AutoEraDisplaySettings.KeyFrameLimit, 0), Is.EqualTo(30));

                model.Refresh();
                Assert.That(model.Snapshot.DisplayState, Is.EqualTo(UiDataState.Ready),
                    "落盘失败不等于设置不可用——值是有的，只是可能不持久。");
                Assert.That(Value(model.Snapshot.DisplayRows, "帧率上限"), Is.EqualTo("30 帧／秒"));
            }
        }

        [Test]
        public void EditorPlayModeGuard_DoesNotWriteSettingsIntoTheProjectAsset()
        {
            Assume.That(UnityEngine.Application.isPlaying, Is.False, "本条只在编辑模式下有意义。");
            int before = QualitySettings.GetQualityLevel();
            var store = new MemorySettingsStore();
            using (ISettingsReadModel model = SettingsReadModels.Create(store))
            {
                int target = before == 0 ? 1 : 0;
                Assert.That(model.Display.SetQualityLevel(target, out string reason), Is.True, reason);
                Assert.That(store.GetInt(AutoEraDisplaySettings.KeyQualityLevel, -1), Is.EqualTo(target),
                    "设置值该落盘就落盘。");
                Assert.That(QualitySettings.GetQualityLevel(), Is.EqualTo(before),
                    "非播放模式不得把画质写回引擎——QualitySettings.SetQualityLevel 会改写 ProjectSettings 资产。");
            }
        }

        private static string[] Labels(IReadOnlyList<UiDetailField> fields)
        {
            var labels = new string[fields.Count];
            for (int i = 0; i < fields.Count; i++) labels[i] = fields[i].Label;
            return labels;
        }

        private static string Value(IReadOnlyList<UiDetailField> fields, string label)
        {
            for (int i = 0; i < fields.Count; i++) if (fields[i].Label == label) return fields[i].Value;
            return null;
        }
    }
}
