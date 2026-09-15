#if UNITY_EDITOR
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;

namespace UGF.EditorTools
{
    [Serializable]
    public sealed class AILanguageManifest
    {
        public const int CurrentSchemaVersion = 1;
        public const string Kind = "GF_X.Language.AI";

        public int schemaVersion = CurrentSchemaVersion;
        public string kind = Kind;
        public string relativePath;
        public string sourceFingerprint;
        public List<AILanguageEntry> entries = new List<AILanguageEntry>();
    }

    [Serializable]
    public sealed class AILanguageEntry
    {
        public string key;
        public string value;
    }

    public static class AILanguageAdapter
    {
        public static bool TryReverseJsonToExcel(string jsonFile, out List<string> errors)
        {
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(jsonFile) || !File.Exists(jsonFile))
            {
                errors.Add($"Language JSON does not exist: {jsonFile}");
                return false;
            }

            string jsonBaseline = AIDataSyncPipeline.ComputeFileFingerprint(jsonFile);
            string json = File.ReadAllText(jsonFile);
            if (!TryBuildExcelRows(json, out var manifest, out var rows, out errors))
            {
                return false;
            }

            if (!AIDataSyncPipeline.TryResolveGameDataPath(AIDataKind.Language, manifest.relativePath, ".xlsx", out string excelFile, out string pathError))
            {
                errors.Add($"Language xlsx output path is invalid: {pathError}");
                return false;
            }

            string excelBaseline = AIDataSyncPipeline.ComputeFileFingerprint(excelFile);
            if (File.Exists(excelFile) && !TryValidateExistingFingerprint(excelFile, manifest, errors))
            {
                return false;
            }

            if (!AIDataSyncPipeline.TryBuildTemporaryExcel(rows, out string temporaryFile, out string buildError))
            {
                errors.Add($"Could not build temporary language xlsx: {buildError}");
                return false;
            }

            string temporaryJson = temporaryFile + ".json";
            string output = GameDataGenerator.GetGameDataExcelOutputFile(GameDataType.Language, excelFile);
            string stagedOutput = temporaryFile + ".output" + Path.GetExtension(output);
            string outputBaseline = AIDataSyncPipeline.ComputeFileFingerprint(output);
            string bytesOutput = Path.ChangeExtension(output, ".bytes");
            string bytesBaseline = AIDataSyncPipeline.ComputeFileFingerprint(bytesOutput);
            bool useBytes = AppConfigs.GetInstanceEditor()?.LoadFromBytes == true;
            try
            {
                if (!GameDataGenerator.ExportLanguageExcel(temporaryFile, stagedOutput, useBytes))
                    throw new InvalidOperationException("Language generation failed.");
                // The generated workbook is still staged. Commit its logical fingerprint
                // together with it, never in a separate post-commit JSON write.
                manifest.sourceFingerprint = AIDataSyncPipeline.ComputeWorksheetFingerprint(rows);
                File.WriteAllText(temporaryJson, JsonConvert.SerializeObject(manifest, Formatting.Indented), new System.Text.UTF8Encoding(false));
                var report = new AIDataSyncReportItem();
                var replacements = new List<AIDataFileReplacement>
                {
                    new AIDataFileReplacement { sourceFile = temporaryFile, destinationFile = excelFile, expectedDestinationFingerprint = excelBaseline },
                    new AIDataFileReplacement { sourceFile = temporaryJson, destinationFile = jsonFile, expectedDestinationFingerprint = jsonBaseline },
                    new AIDataFileReplacement { sourceFile = stagedOutput, destinationFile = output, expectedDestinationFingerprint = outputBaseline },
                };
                if (useBytes) replacements.Add(new AIDataFileReplacement { sourceFile = Path.ChangeExtension(stagedOutput, ".bytes"), destinationFile = bytesOutput, expectedDestinationFingerprint = bytesBaseline });
                bool replaced = AIDataSyncPipeline.ReplaceFilesTransactionally(replacements, report);
                errors.AddRange(report.errors);
                if (!replaced)
                {
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                errors.Add(exception.Message);
                return false;
            }
            finally
            {
                if (File.Exists(stagedOutput)) File.Delete(stagedOutput);
                if (File.Exists(Path.ChangeExtension(stagedOutput, ".bytes"))) File.Delete(Path.ChangeExtension(stagedOutput, ".bytes"));
                if (File.Exists(temporaryJson)) File.Delete(temporaryJson);
                if (File.Exists(temporaryFile))
                {
                    File.Delete(temporaryFile);
                }
            }
        }

        public static bool TryExportExcelToJson(string excelFile, out string jsonFile, out List<string> errors)
        {
            jsonFile = null;
            if (!TryCreateManifestFromExcel(excelFile, out var manifest, out errors))
            {
                return false;
            }

            if (!AIDataSyncPipeline.TryResolveAIJsonPath(AIDataKind.Language, manifest.relativePath, out jsonFile, out string pathError))
            {
                errors.Add($"Language JSON output path is invalid: {pathError}");
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(jsonFile));
            File.WriteAllText(jsonFile, JsonConvert.SerializeObject(manifest, Formatting.Indented), new System.Text.UTF8Encoding(false));
            return true;
        }

        public static bool TryCreateManifestFromExcel(string excelFile, out AILanguageManifest manifest, out List<string> errors)
        {
            manifest = null;
            errors = new List<string>();
            if (string.IsNullOrWhiteSpace(excelFile) || !File.Exists(excelFile))
            {
                errors.Add($"Language xlsx does not exist: {excelFile}");
                return false;
            }

            string relative = Path.ChangeExtension(Path.GetRelativePath(ConstEditor.LanguageExcelPath, excelFile), null)?.Replace('\\', '/');
            if (!AIDataSyncPipeline.TryNormalizeRelativePath(relative, out relative, out string pathError))
            {
                errors.Add($"Language relative path is invalid: {pathError}");
                return false;
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(excelFile)))
            {
                var sheet = package.Workbook.Worksheets.Count > 0 ? package.Workbook.Worksheets[0] : null;
                if (sheet?.Dimension == null)
                {
                    errors.Add($"Language xlsx has no worksheet data: {excelFile}");
                    return false;
                }

                manifest = new AILanguageManifest
                {
                    relativePath = relative,
                    entries = new List<AILanguageEntry>(),
                };
                for (int row = 1; row <= sheet.Dimension.End.Row; row++)
                {
                    string key = GetCellText(sheet, row, 2);
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        continue;
                    }

                    manifest.entries.Add(new AILanguageEntry
                    {
                        key = key,
                        value = GetCellText(sheet, row, 3),
                    });
                }

                manifest.sourceFingerprint = AIDataSyncPipeline.ComputeLogicalFingerprint(ReadLogicalRows(sheet));
            }

            return TryParseManifest(JsonConvert.SerializeObject(manifest), out manifest, out errors);
        }

        public static bool TryBuildExcelRows(string json, out AILanguageManifest manifest, out List<string[]> rows, out List<string> errors)
        {
            rows = null;
            if (!TryParseManifest(json, out manifest, out errors))
            {
                return false;
            }

            rows = new List<string[]>();
            foreach (AILanguageEntry entry in manifest.entries)
            {
                rows.Add(new[] { string.Empty, entry.key, entry.value ?? string.Empty });
            }

            return true;
        }

        public static bool TryParseManifest(string json, out AILanguageManifest manifest, out List<string> errors)
        {
            manifest = null;
            errors = new List<string>();
            try
            {
                manifest = JsonConvert.DeserializeObject<AILanguageManifest>(json);
            }
            catch (JsonException exception)
            {
                errors.Add($"Language JSON is invalid: {exception.Message}");
                return false;
            }

            if (manifest == null)
            {
                errors.Add("Language JSON has no manifest.");
                return false;
            }

            if (manifest.schemaVersion != AILanguageManifest.CurrentSchemaVersion)
            {
                errors.Add($"Unsupported language schema version: {manifest.schemaVersion}.");
            }

            if (!string.Equals(manifest.kind, AILanguageManifest.Kind, StringComparison.Ordinal))
            {
                errors.Add($"Unexpected language manifest kind: {manifest.kind}.");
            }

            if (!AIDataSyncPipeline.TryNormalizeRelativePath(manifest.relativePath, out manifest.relativePath, out string pathError))
            {
                errors.Add($"Language relative path is invalid: {pathError}");
            }

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (AILanguageEntry entry in manifest.entries ?? new List<AILanguageEntry>())
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.key))
                {
                    errors.Add("Language entries require a non-empty key.");
                    continue;
                }

                if (!keys.Add(entry.key))
                {
                    errors.Add($"Duplicate language key: {entry.key}.");
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

        private static bool TryValidateExistingFingerprint(string excelFile, AILanguageManifest manifest, List<string> errors)
        {
            if (!TryCreateManifestFromExcel(excelFile, out var current, out var readErrors))
            {
                errors.AddRange(readErrors);
                return false;
            }

            var report = new AIDataSyncReportItem();
            bool valid = AIDataSyncPipeline.ValidateBaseline(
                new AIDataSyncManifest { sourceFingerprint = manifest.sourceFingerprint }, current.sourceFingerprint, report);
            errors.AddRange(report.errors);
            return valid;
        }
    }
}
#endif
