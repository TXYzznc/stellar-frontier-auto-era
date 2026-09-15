using System;
using System.Globalization;
using System.Collections.Generic;
using AutoEra.World.Time;

namespace AutoEra.Application
{
    public sealed class AutoEraRuntimeSettings
    {
        private AutoEraRuntimeSettings(string mainMenu, string world, long initial, WorldDayNightRules rules)
        {
            MainMenuScene = mainMenu;
            WorldScene = world;
            InitialMilliseconds = initial;
            DayNight = rules;
        }
        public string MainMenuScene { get; }
        public string WorldScene { get; }
        public long InitialMilliseconds { get; }
        public WorldDayNightRules DayNight { get; }

        public static void ValidateLoadedStartupData()
        {
            var table = GF.DataTable.GetDataTable<AutoEra.DataTable.StartupMessages>();
            if (table == null) throw new FormatException("Foundation/StartupMessages is not loaded.");
            var rows = new List<KeyValuePair<int,string>>();
            foreach (var row in table.GetAllDataRows()) rows.Add(new KeyValuePair<int,string>(row.Id,row.MessageKey));
            ValidateStartupMessageReferences(rows, GF.Localization.HasRawString);
        }

        public static void ValidateStartupMessageReferences(IEnumerable<KeyValuePair<int,string>> rows, Func<string,bool> containsLanguageKey)
        {
            if (rows == null || containsLanguageKey == null) throw new ArgumentNullException();
            var ids = new HashSet<int>();
            foreach (var row in rows)
            {
                if (row.Key <= 0 || !ids.Add(row.Key)) throw new FormatException("Foundation/StartupMessages invalid or duplicate Id: " + row.Key);
                if (string.IsNullOrWhiteSpace(row.Value) || !containsLanguageKey(row.Value))
                    throw new FormatException("Foundation/StartupMessages missing localization reference, Id="+row.Key+", MessageKey="+row.Value);
            }
            if (!ids.Contains(1) || !ids.Contains(2)) throw new FormatException("Foundation/StartupMessages requires startup phases 1 and 2.");
        }

        public static AutoEraRuntimeSettings Load(Func<string, string> read)
        {
            if (read == null) throw new ArgumentNullException(nameof(read));
            string menu = SceneName(read("AutoEra.Scene.MainMenu"));
            string world = SceneName(read("AutoEra.Scene.World"));
            if (menu == world) throw new FormatException("Main menu and world scenes must differ.");
            long initial = Number(read, "AutoEra.Time.InitialMilliseconds");
            var rules = new WorldDayNightRules(Number(read, "AutoEra.Time.CycleMilliseconds"), Number(read, "AutoEra.Time.SunlitMilliseconds"));
            if (rules.GetPhase(initial) != WorldDayNightPhase.Sunlit) throw new FormatException("New world must start during sunlight.");
            return new AutoEraRuntimeSettings(menu, world, initial, rules);
        }

        private static long Number(Func<string, string> read, string key)
        {
            if (!long.TryParse(read(key), NumberStyles.None, CultureInfo.InvariantCulture, out long value))
                throw new FormatException("Missing or invalid non-negative world-time configuration: " + key);
            return value;
        }

        private static string SceneName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.StartsWith("/") || name.Contains("..") || name.Contains(":") || name.Contains("\\") || name.EndsWith(".unity"))
                throw new FormatException("Expected a relative scene resource name.");
            return name;
        }
    }
}
