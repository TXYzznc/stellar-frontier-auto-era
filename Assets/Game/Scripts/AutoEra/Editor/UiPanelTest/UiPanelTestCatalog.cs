#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using AutoEra.UI.Testing;

using UnityEditor;
using UnityEngine;

namespace AutoEra.Editor.UiPanelTest
{
    /// <summary>单个可测界面的合并视图：UIViews 枚举 + UITable 配置 + 已登记准备钩子。</summary>
    public sealed class UiPanelTestEntry
    {
        public int ViewId;
        public string ViewName = string.Empty;
        public bool HasEnumEntry;
        public UIViews View;
        public bool HasUiTableRow;
        public string TableNote = string.Empty;
        public int SortOrder;
        public string PrefabAssetPath = string.Empty;
        public bool PauseCoveredUI;
        public bool EscapeClose;
        public string GroupName = string.Empty;
        public int GroupDepth;
        public Type SetupType;
        public string SetupNote = string.Empty;

        /// <summary>枚举与 UITable 行同时存在时才能经 GF.UI 直开。</summary>
        public bool CanOpenDirectly => HasEnumEntry && HasUiTableRow;

        public string SetupLabel => SetupType == null ? "未登记" : SetupType.Name;
    }

    /// <summary>UITable 源文本的原始行（列序对齐 UITable.ParseDataRow）。</summary>
    public sealed class UiTableRawRow
    {
        public int Id;
        public string Note = string.Empty;
        public int SortOrder;
        public string UIPrefab = string.Empty;
        public bool PauseCoveredUI;
        public int UIGroupId;
        public bool EscapeClose;
    }

    /// <summary>UIGroupTable 源文本的原始行。</summary>
    public sealed class UiGroupRawRow
    {
        public int Id;
        public string Name = string.Empty;
        public int Depth;
    }

    /// <summary>
    /// UI 面板测试目录：Editor 下离线解析 UITable/UIGroupTable 源文本、反射 UIViews 枚举、
    /// 扫描已登记的 <see cref="UiPanelTestSetupAttribute"/> 准备钩子，合并为可测界面清单。
    /// 新界面完成 UITable 配置后无需登记即自动出现在清单中。
    /// </summary>
    public static class UiPanelTestCatalog
    {
        public const string UiTableAssetPath = "Assets/Game/DataTable/Core/UITable.txt";
        public const string UiGroupTableAssetPath = "Assets/Game/DataTable/Core/UIGroupTable.txt";

        public static List<UiPanelTestEntry> Build()
        {
            var groups = ParseUiGroups(ReadAssetText(UiGroupTableAssetPath));
            var rows = ParseUiTable(ReadAssetText(UiTableAssetPath));
            var setups = ScanRegisteredSetups();

            var rowById = new Dictionary<int, UiTableRawRow>();
            foreach (UiTableRawRow row in rows)
            {
                rowById[row.Id] = row;
            }

            var entries = new List<UiPanelTestEntry>();
            foreach (UIViews view in Enum.GetValues(typeof(UIViews)))
            {
                int id = (int)view;
                UiPanelTestEntry entry = CreateEntry(id, view.ToString(), hasEnum: true);
                entry.View = view;
                if (rowById.TryGetValue(id, out UiTableRawRow row))
                {
                    ApplyRow(entry, row, groups);
                    rowById.Remove(id);
                }

                ApplySetup(entry, setups);
                entries.Add(entry);
            }

            foreach (UiTableRawRow leftover in rowById.Values)
            {
                UiPanelTestEntry entry = CreateEntry(leftover.Id, leftover.Id.ToString(CultureInfo.InvariantCulture), hasEnum: false);
                ApplyRow(entry, leftover, groups);
                ApplySetup(entry, setups);
                entries.Add(entry);
            }

            entries.Sort((a, b) => a.ViewId.CompareTo(b.ViewId));
            return entries;
        }

        /// <summary>解析 UITable 源文本；列序：占位/Id/备注/SortOrder/UIPrefab/PauseCoveredUI/UIGroupId/EscapeClose。</summary>
        public static List<UiTableRawRow> ParseUiTable(string content)
        {
            var rows = new List<UiTableRawRow>();
            if (string.IsNullOrEmpty(content))
            {
                return rows;
            }

            foreach (string rawLine in content.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                string[] columns = line.Split('\t');
                if (columns.Length < 8 ||
                    !int.TryParse(columns[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                {
                    continue;
                }

                rows.Add(new UiTableRawRow
                {
                    Id = id,
                    Note = columns[2].Trim(),
                    SortOrder = ParseInt(columns[3]),
                    UIPrefab = columns[4].Trim(),
                    PauseCoveredUI = ParseBool(columns[5]),
                    UIGroupId = ParseInt(columns[6]),
                    EscapeClose = ParseBool(columns[7]),
                });
            }

            return rows;
        }

        /// <summary>解析 UIGroupTable 源文本；列序：占位/Id/备注/Name/Depth。</summary>
        public static List<UiGroupRawRow> ParseUiGroups(string content)
        {
            var rows = new List<UiGroupRawRow>();
            if (string.IsNullOrEmpty(content))
            {
                return rows;
            }

            foreach (string rawLine in content.Split('\n'))
            {
                string line = rawLine.TrimEnd('\r');
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                string[] columns = line.Split('\t');
                if (columns.Length < 5 ||
                    !int.TryParse(columns[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
                {
                    continue;
                }

                rows.Add(new UiGroupRawRow
                {
                    Id = id,
                    Name = columns[3].Trim(),
                    Depth = ParseInt(columns[4]),
                });
            }

            return rows;
        }

        /// <summary>扫描全部已登记的测试准备钩子（TypeCache 跨程序集生效）。</summary>
        public static Dictionary<int, UiPanelTestSetupInfo> ScanRegisteredSetups()
        {
            var result = new Dictionary<int, UiPanelTestSetupInfo>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<IUiPanelTestSetup>())
            {
                if (type == null || type.IsAbstract)
                {
                    continue;
                }

                object[] attributes = type.GetCustomAttributes(typeof(UiPanelTestSetupAttribute), false);
                if (attributes == null || attributes.Length == 0)
                {
                    continue;
                }

                var attribute = (UiPanelTestSetupAttribute)attributes[0];
                result[(int)attribute.View] = new UiPanelTestSetupInfo(type, attribute.Note);
            }

            return result;
        }

        private static UiPanelTestEntry CreateEntry(int id, string name, bool hasEnum)
        {
            return new UiPanelTestEntry { ViewId = id, ViewName = name, HasEnumEntry = hasEnum };
        }

        private static void ApplyRow(UiPanelTestEntry entry, UiTableRawRow row, List<UiGroupRawRow> groups)
        {
            entry.HasUiTableRow = true;
            entry.TableNote = row.Note;
            entry.SortOrder = row.SortOrder;
            entry.PrefabAssetPath = UtilityBuiltin.AssetsPath.GetUIFormPath(row.UIPrefab);
            entry.PauseCoveredUI = row.PauseCoveredUI;
            entry.EscapeClose = row.EscapeClose;
            foreach (UiGroupRawRow group in groups)
            {
                if (group.Id == row.UIGroupId)
                {
                    entry.GroupName = group.Name;
                    entry.GroupDepth = group.Depth;
                    break;
                }
            }
        }

        private static void ApplySetup(UiPanelTestEntry entry, Dictionary<int, UiPanelTestSetupInfo> setups)
        {
            if (setups.TryGetValue(entry.ViewId, out UiPanelTestSetupInfo setup))
            {
                entry.SetupType = setup.Type;
                entry.SetupNote = setup.Note;
            }
        }

        private static string ReadAssetText(string assetRelativePath)
        {
            string dataPath = UnityEngine.Application.dataPath;
            string projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);
            string fullPath = Path.Combine(projectRoot, assetRelativePath.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : string.Empty;
        }

        private static int ParseInt(string value)
        {
            return int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : 0;
        }

        private static bool ParseBool(string value)
        {
            return bool.TryParse(value.Trim(), out bool result) && result;
        }
    }

    /// <summary>已登记准备钩子的类型与说明。</summary>
    public sealed class UiPanelTestSetupInfo
    {
        public UiPanelTestSetupInfo(Type type, string note)
        {
            Type = type;
            Note = note ?? string.Empty;
        }

        public Type Type { get; }

        public string Note { get; }
    }
}
#endif
