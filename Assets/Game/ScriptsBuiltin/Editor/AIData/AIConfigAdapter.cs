#if UNITY_EDITOR
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace UGF.EditorTools
{
    [Serializable]
    public sealed class AIConfigManifest
    {
        public const int CurrentSchemaVersion = 1;
        public const string Kind = "GF_X.Config.AI";

        public int schemaVersion = CurrentSchemaVersion;
        public string kind = Kind;
        public string relativePath;
        public string sourceFingerprint;
        public List<AIConfigEntry> entries = new List<AIConfigEntry>();
    }

    [Serializable]
    public sealed class AIConfigEntry
    {
        public string key;
        public string comment;
        public string value;
    }

    public static class AIConfigAdapter
    {
        public static bool TryExportExcelToJson(string excelFile, out string jsonFile, out List<string> errors)
        {
            jsonFile = null;
            if (!TryCreateManifestFromExcel(excelFile, out var manifest, out errors))
            {
                return false;
            }

            if (!AIDataSyncPipeline.TryResolveAIJsonPath(AIDataKind.Config, manifest.relativePath, out jsonFile, out string pathError))
            {
                errors.Add($"Config JSON output path is invalid: {pathError}");
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(jsonFile));
            File.WriteAllText(jsonFile, JsonConvert.SerializeObject(manifest, Formatting.Indented), new System.Text.UTF8Encoding(false));
            return true;
        }

        public static bool TryCreateManifestFromExcel(string excelFile, out AIConfigManifest manifest, out List<string> errors)
        {
            manifest = null;
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(excelFile) || !File.Exists(excelFile))
            {
                errors.Add($"Config xlsx does not exist: {excelFile}");
                return false;
            }

            string relative = Path.ChangeExtension(Path.GetRelativePath(ConstEditor.ConfigExcelPath, excelFile), null)?.Replace('\\', '/');
            if (!AIDataSyncPipeline.TryNormalizeRelativePath(relative, out relative, out string pathError))
            {
                errors.Add($"Config relative path is invalid: {pathError}");
                return false;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(excelFile)))
            {
                var sheet = package.Workbook.Worksheets.Count > 0 ? package.Workbook.Worksheets[0] : null;
                if (sheet?.Dimension == null)
                {
                    errors.Add($"Config xlsx has no worksheet data: {excelFile}");
                    return false;
                }

                manifest = new AIConfigManifest
                {
                    relativePath = relative,
                    entries = new List<AIConfigEntry>(),
                };
                for (int row = 3; row <= sheet.Dimension.End.Row; row++)
                {
                    string key = GetCellText(sheet, row, 2);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    manifest.entries.Add(new AIConfigEntry
                    {
                        key = key,
                        comment = GetCellText(sheet, row, 3),
                        value = GetCellText(sheet, row, 4),
                    });
                }

                manifest.sourceFingerprint = AIDataSyncPipeline.ComputeLogicalFingerprint(ReadLogicalRows(sheet));
            }

            return TryParseManifest(JsonConvert.SerializeObject(manifest), out manifest, out errors);
        }

        public static bool TryBuildExcelRows(string json, out AIConfigManifest manifest, out List<string[]> rows, out List<string> errors)
        {
            rows = null;
            if (!TryParseManifest(json, out manifest, out errors))
            {
                return false;
            }

            rows = new List<string[]>
            {
                new[] { "#", System.IO.Path.GetFileName(manifest.relativePath) },
                new[] { "#", "Key", "备注", "Value" },
            };
            foreach (AIConfigEntry entry in manifest.entries)
            {
                rows.Add(new[] { string.Empty, entry.key, entry.comment ?? string.Empty, entry.value ?? string.Empty });
            }

            return true;
        }

        public static bool TryParseManifest(string json, out AIConfigManifest manifest, out List<string> errors)
        {
            manifest = null;
            errors = new List<string>();
            try
            {
                manifest = JsonConvert.DeserializeObject<AIConfigManifest>(json);
            }
            catch (JsonException exception)
            {
                errors.Add($"Config JSON is invalid: {exception.Message}");
                return false;
            }

            if (manifest == null)
            {
                errors.Add("Config JSON has no manifest.");
                return false;
            }

            if (manifest.schemaVersion != AIConfigManifest.CurrentSchemaVersion)
            {
                errors.Add($"Unsupported config schema version: {manifest.schemaVersion}.");
            }

            if (!string.Equals(manifest.kind, AIConfigManifest.Kind, StringComparison.Ordinal))
            {
                errors.Add($"Unexpected config manifest kind: {manifest.kind}.");
            }

            if (!AIDataSyncPipeline.TryNormalizeRelativePath(manifest.relativePath, out manifest.relativePath, out string pathError))
            {
                errors.Add($"Config relative path is invalid: {pathError}");
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (AIConfigEntry entry in manifest.entries ?? new List<AIConfigEntry>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                {
                    errors.Add("Config entries require a non-empty key.");
                    continue;
                }

                if (!keys.Add(entry.key))
                {
                    errors.Add($"Duplicate config key: {entry.key}.");
                }
            }

            return errors.Count == 0;
        }

        private static List<string[]> ReadLogicalRows(ExcelWorksheet sheet)
        {
            var rows = new List<string[]>();
            for (int row = 1; row <= sheet.Dimension.End.Row; row++)
            {
                var cells = new string[sheet.Dimension.End.Column];
                for (int column = 1; column <= cells.Length; column++)
                {
                    cells[column - 1] = GetCellText(sheet, row, column);
                }

                rows.Add(cells);
            }

            return rows;
        }

        private static string GetCellText(ExcelWorksheet sheet, int row, int column)
        {
            return (sheet.GetValue(row, column)?.ToString() ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
#endif
