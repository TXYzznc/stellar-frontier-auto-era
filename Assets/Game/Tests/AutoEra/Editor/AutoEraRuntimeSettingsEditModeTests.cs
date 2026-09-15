using System;
using System.Collections.Generic;
using AutoEra.Application;
using AutoEra.World.Time;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraRuntimeSettingsEditModeTests
    {
        [Test]
        public void StartupMessageReferences_RejectMissingKeysDuplicateIdsAndMissingPhases()
        {
            var valid = new[] {new KeyValuePair<int,string>(1,"menu"),new KeyValuePair<int,string>(2,"world")};
            Assert.DoesNotThrow(()=>AutoEraRuntimeSettings.ValidateStartupMessageReferences(valid,key=>true));
            Assert.Throws<FormatException>(()=>AutoEraRuntimeSettings.ValidateStartupMessageReferences(valid,key=>key=="menu"));
            Assert.Throws<FormatException>(()=>AutoEraRuntimeSettings.ValidateStartupMessageReferences(new[] {valid[0],valid[0]},key=>true));
            Assert.Throws<FormatException>(()=>AutoEraRuntimeSettings.ValidateStartupMessageReferences(new[] {valid[0]},key=>true));
        }
        private static Dictionary<string, string> Values() => new Dictionary<string, string>
        {
            ["AutoEra.Scene.MainMenu"] = "MainMenu",
            ["AutoEra.Scene.World"] = "InitialRegion",
            ["AutoEra.Time.InitialMilliseconds"] = "0",
            ["AutoEra.Time.CycleMilliseconds"] = "1440000",
            ["AutoEra.Time.SunlitMilliseconds"] = "960000"
        };

        [Test]
        public void Settings_UseConfiguredTimeAndSceneNames()
        {
            var values = Values();
            var settings = AutoEraRuntimeSettings.Load(k => values[k]);
            Assert.That(settings.WorldScene, Is.EqualTo("InitialRegion"));
            Assert.That(settings.InitialMilliseconds, Is.Zero);
            Assert.That(settings.DayNight.GetPhase(959999), Is.EqualTo(WorldDayNightPhase.Sunlit));
            Assert.That(settings.DayNight.GetPhase(960000), Is.EqualTo(WorldDayNightPhase.Dark));
            Assert.That(settings.DayNight.GetPhase(1440000), Is.EqualTo(WorldDayNightPhase.Sunlit));
        }

        [Test]
        public void Settings_RejectMissingTimeTraversalAndDarkStart()
        {
            var values = Values();
            values["AutoEra.Time.InitialMilliseconds"] = null;
            Assert.Throws<FormatException>(() => AutoEraRuntimeSettings.Load(k => values[k]));
            values["AutoEra.Time.InitialMilliseconds"] = "960000";
            Assert.Throws<FormatException>(() => AutoEraRuntimeSettings.Load(k => values[k]));
            values["AutoEra.Time.InitialMilliseconds"] = "0";
            values["AutoEra.Scene.World"] = "../Outside";
            Assert.Throws<FormatException>(() => AutoEraRuntimeSettings.Load(k => values[k]));
        }

        [Test]
        public void Settings_RejectInvalidCyclesNumbersAndIdenticalScenes()
        {
            foreach (string invalid in new[] { "-1", "NaN", "9223372036854775808", "1.5", "" })
            {
                var values = Values();
                values["AutoEra.Time.InitialMilliseconds"] = invalid;
                Assert.Throws<FormatException>(() => AutoEraRuntimeSettings.Load(k => values[k]));
            }
            var data = Values();
            data["AutoEra.Time.CycleMilliseconds"] = "0";
            Assert.Throws<ArgumentOutOfRangeException>(() => AutoEraRuntimeSettings.Load(k => data[k]));
            data = Values(); data["AutoEra.Time.SunlitMilliseconds"] = "1440001";
            Assert.Throws<ArgumentOutOfRangeException>(() => AutoEraRuntimeSettings.Load(k => data[k]));
            data = Values(); data["AutoEra.Scene.World"] = data["AutoEra.Scene.MainMenu"];
            Assert.Throws<FormatException>(() => AutoEraRuntimeSettings.Load(k => data[k]));
        }
    }
}
