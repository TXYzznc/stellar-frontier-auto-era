using AutoEra.Settings;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    /// <summary>
    /// 操作设置的状态契约（规格 02-系统与设置 · 操作 · 镜头）。
    ///
    /// 这一页有两个容易做错的地方，用例就守着这两条：
    ///
    /// <list type="bullet">
    /// <item><b>区间必须是同一个来源</b>。滑条区间、本机设置的夹取、镜头自己的取值如果各写一份，
    ///       表现就是「滑条能拖到 40，镜头在 15 处封顶」。所以这里的断言不写魔法数字，
    ///       而是对照 <see cref="RegionCameraParameters"/>——三处必须一起变。</item>
    /// <item><b>反转必须真的被镜头消费</b>。界面能勾选、存储里有值，但镜头不读它，就是界面在撒谎；
    ///       反转此前**根本没有实现**，是本页补的，所以必须有断言证明它落到了目标上。</item>
    /// </list>
    ///
    /// 另外守住一条**范围**：本页不含改键——设计原文「第一版不开放改键，只读显示 InputModule 当前绑定」。
    /// 「当前按键」栏只在读模型里以只读文本出现，没有可写入口，这里断言设置对象的公开面里没有改键 API。
    /// </summary>
    public sealed class ControlSettingsEditModeTests
    {
        [Test]
        public void Defaults_MatchTheCameraComponentsSerializedValues()
        {
            var settings = new AutoEraControlSettings(new MemorySettingsStore());

            Assert.That(settings.PanSpeed, Is.EqualTo(RegionCameraParameters.DefaultPanSpeed));
            Assert.That(settings.RotationSpeed, Is.EqualTo(RegionCameraParameters.DefaultRotationSpeed));
            Assert.That(settings.ZoomSpeed, Is.EqualTo(RegionCameraParameters.DefaultZoomSpeed));
            Assert.That(settings.InvertHorizontal, Is.False);
            Assert.That(settings.InvertVertical, Is.False);

            Assert.That(settings.DifferenceFromDefaults(), Is.Zero);
        }

        [Test]
        public void SliderRangesAndClampingShareOneSource()
        {
            // 滑条用的就是这四个常量；只要它们是同一套，界面与镜头就不会各说各话。
            Assert.That(RegionCameraParameters.MinPanSpeed, Is.LessThan(RegionCameraParameters.DefaultPanSpeed));
            Assert.That(RegionCameraParameters.MaxPanSpeed, Is.GreaterThan(RegionCameraParameters.DefaultPanSpeed));
            Assert.That(RegionCameraParameters.MinRotationSpeed, Is.LessThan(RegionCameraParameters.DefaultRotationSpeed));
            Assert.That(RegionCameraParameters.MaxRotationSpeed, Is.GreaterThan(RegionCameraParameters.DefaultRotationSpeed));
            Assert.That(RegionCameraParameters.MinZoomSpeed, Is.LessThan(RegionCameraParameters.DefaultZoomSpeed));
            Assert.That(RegionCameraParameters.MaxZoomSpeed, Is.GreaterThan(RegionCameraParameters.DefaultZoomSpeed));

            var settings = new AutoEraControlSettings(new MemorySettingsStore());
            Assert.That(settings.SetPanSpeed(RegionCameraParameters.MaxPanSpeed, out _), Is.True);
            Assert.That(settings.PanSpeed, Is.EqualTo(RegionCameraParameters.MaxPanSpeed),
                "区间上界必须真的能设进去——否则就是「滑条能拖到、值却被砍掉」。");
        }

        [Test]
        public void OutOfRangeWritesAreRefusedWithAReason()
        {
            var settings = new AutoEraControlSettings(new MemorySettingsStore());

            Assert.That(settings.SetPanSpeed(RegionCameraParameters.MaxPanSpeed + 10f, out string reason), Is.False);
            StringAssert.Contains("平移速度", reason);
            Assert.That(settings.PanSpeed, Is.EqualTo(RegionCameraParameters.DefaultPanSpeed),
                "被拒绝的写入不能留下半份状态。");

            Assert.That(settings.SetRotationSpeed(float.NaN, out reason), Is.False);
            Assert.That(settings.RotationSpeed, Is.EqualTo(RegionCameraParameters.DefaultRotationSpeed));

            Assert.That(settings.SetZoomSpeed(RegionCameraParameters.MinZoomSpeed - 1f, out reason), Is.False);
            StringAssert.Contains("缩放速度", reason);
        }

        [Test]
        public void ReadingBackOnASecondInstanceSeesTheStoredValues()
        {
            var store = new MemorySettingsStore();
            var first = new AutoEraControlSettings(store);
            first.SetPanSpeed(22f, out _);
            first.SetInvertHorizontal(true, out _);

            // 模拟重新打开页面／重启：同一份存储上再建一个设置对象。
            var second = new AutoEraControlSettings(store);

            Assert.That(second.PanSpeed, Is.EqualTo(22f));
            Assert.That(second.InvertHorizontal, Is.True);
            Assert.That(second.DifferenceFromDefaults(), Is.EqualTo(2));
        }

        [Test]
        public void ApplyTo_PushesEveryParameterOntoTheCamera()
        {
            var settings = new AutoEraControlSettings(new MemorySettingsStore());
            settings.SetPanSpeed(20f, out _);
            settings.SetRotationSpeed(6f, out _);
            settings.SetZoomSpeed(11f, out _);
            settings.SetInvertHorizontal(true, out _);
            settings.SetInvertVertical(true, out _);

            var camera = new FakeCameraTarget();
            settings.ApplyTo(camera);

            Assert.That(camera.PanSpeed, Is.EqualTo(20f));
            Assert.That(camera.RotationSpeed, Is.EqualTo(6f));
            Assert.That(camera.ZoomSpeed, Is.EqualTo(11f));
            Assert.That(camera.InvertHorizontal, Is.True,
                "反转此前没有实现——勾了不生效是这一页最容易留下的假接线。");
            Assert.That(camera.InvertVertical, Is.True);
        }

        [Test]
        public void ApplyToANullTargetIsNormal_NotAnError()
        {
            var settings = new AutoEraControlSettings(new MemorySettingsStore());
            settings.SetPanSpeed(18f, out _);

            // 世界外打开设置时没有镜头：参数已经存好了，进区域时再取用。
            Assert.DoesNotThrow(() => settings.ApplyTo(null));
            Assert.That(settings.PanSpeed, Is.EqualTo(18f));
        }

        [Test]
        public void ResetToDefaults_OnlyTouchesThisPage()
        {
            var store = new MemorySettingsStore();
            var control = new AutoEraControlSettings(store);
            var display = new AutoEraDisplaySettings(store);
            var audio = new AutoEraAudioSettings(store);

            display.SetFrameLimit((int)FrameLimitOption.Thirty, out _);
            audio.SetBus(AudioBus.Music, 0.3f, out _);
            control.SetPanSpeed(9f, out _);
            control.SetInvertVertical(true, out _);
            Assert.That(control.DifferenceFromDefaults(), Is.EqualTo(2));

            Assert.That(control.ResetToDefaults(out string reason), Is.True, reason);

            Assert.That(control.DifferenceFromDefaults(), Is.Zero);
            Assert.That(display.FrameLimit, Is.EqualTo((int)FrameLimitOption.Thirty),
                "「恢复本页默认」只恢复操作分页。");
            Assert.That(audio.GetBus(AudioBus.Music), Is.EqualTo(0.3f),
                "声音分页也必须保留。");
        }

        [Test]
        public void WriteFailure_KeepsTheValueAndReportsTheReason()
        {
            var store = new MemorySettingsStore { FailWrites = true };
            var settings = new AutoEraControlSettings(store);

            Assert.That(settings.SetZoomSpeed(7f, out string reason), Is.False);
            StringAssert.Contains("写入失败", reason);
            Assert.That(settings.ZoomSpeed, Is.EqualTo(7f),
                "写入失败保留内存值，不回退到旧值。");
        }

        [Test]
        public void OneWriteSavesOnce()
        {
            var store = new MemorySettingsStore();
            var settings = new AutoEraControlSettings(store);

            settings.SetInvertHorizontal(true, out _);

            Assert.That(store.SaveCount, Is.EqualTo(1),
                "落盘是整份设置写文件，不是每个键一次。");
        }

        [Test]
        public void ThePageHasNoRebindingApi_ByDesign()
        {
            // 设计原文：第一版不开放改键，只读显示 InputModule 当前绑定。
            // 这条用例是**范围守卫**：一旦有人给这一页加上写绑定表的能力，它就失败，
            // 逼着改动回去对设计（要改的是设计，不是偷偷加 API）。
            var methods = typeof(AutoEraControlSettings).GetMethods();
            for (int i = 0; i < methods.Length; i++)
            {
                string name = methods[i].Name;
                Assert.That(name, Does.Not.Contain("Binding"),
                    "操作页不含改键：" + name);
                Assert.That(name, Does.Not.Contain("Rebind"),
                    "操作页不含改键：" + name);
            }
        }

        private sealed class FakeCameraTarget : IRegionCameraTarget
        {
            public float PanSpeed { get; set; } = RegionCameraParameters.DefaultPanSpeed;
            public float RotationSpeed { get; set; } = RegionCameraParameters.DefaultRotationSpeed;
            public float ZoomSpeed { get; set; } = RegionCameraParameters.DefaultZoomSpeed;
            public bool InvertHorizontal { get; set; }
            public bool InvertVertical { get; set; }
        }
    }
}
