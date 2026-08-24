#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace UGF.EditorTools
{
    /// <summary>
    /// Shared, product-neutral safety primitives for the AI JSON to GameData workflow.
    /// Adapters own their schemas; this class owns path boundaries, logical fingerprints,
    /// baseline conflict detection, and recoverable file replacement.
    /// </summary>
    public static class AIDataSyncPipeline
    {
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);

        public static string GetJsonRoot(AIDataKind kind)
        {
            switch (kind)
            {
                case AIDataKind.DataTable: return ConstEditor.AIDataTablePath;
                case AIDataKind.Config: return ConstEditor.AIDataConfigPath;
                case AIDataKind.Language: return ConstEditor.AIDataLanguagePath;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        public static string GetExcelRoot(AIDataKind kind)
        {
            switch (kind)
            {
                case AIDataKind.DataTable: return ConstEditor.DataTableExcelPath;
                case AIDataKind.Config: return ConstEditor.ConfigExcelPath;
                case AIDataKind.Language: return ConstEditor.LanguageExcelPath;
                default: throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
            }
        }

        public static bool TryNormalizeRelativePath(string candidate, out string relativePath, out string error)
        {
            relativePath = null;
            error = null;
            if (string.IsNullOrWhiteSpace(candidate) || Path.IsPathRooted(candidate))
            {
                error = "Path must be a non-empty relative path.";
                return false;
            }

            string normalized = candidate.Replace('\\', '/').Trim('/');
            string[] segments = normalized.Split('/');
            if (segments.Length == 0 || Array.Exists(segments, segment => string.IsNullOrWhiteSpace(segment) || segment == "." || segment == ".."))
            {
                error = "Path contains an invalid segment.";
                return false;
            }

            relativePath = normalized;
            return true;
        }

        public static bool TryResolveGameDataPath(AIDataKind kind, string relativePath, string extension, out string fullPath, out string error)
        {
            fullPath = null;
            if (!TryNormalizeRelativePath(relativePath, out string normalized, out error))
            {
                return false;
            }

            string root = GetExcelRoot(kind);
            string candidate = Path.GetFullPath(Path.Combine(root, normalized + extension));
            string normalizedRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!candidate.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase))
            {
                error = "Resolved path escapes its GameData root.";
                return false;
            }

            fullPath = candidate;
            return true;
        }

        public static string ComputeLogicalFingerprint(IList<string[]> rows)
        {
            using (var hash = SHA256.Create())
            using (var stream = new MemoryStream())
            {
                WriteInt32(stream, rows == null ? 0 : rows.Count);
                if (rows != null)
                {
                    foreach (string[] row in rows)
                    {
                        WriteInt32(stream, row == null ? -1 : row.Length);
                        if (row == null)
                        {
                            continue;
                        }

                        foreach (string cell in row)
                        {
                            byte[] bytes = Utf8NoBom.GetBytes((cell ?? string.Empty).Replace("\r\n", "\n").Replace("\r", "\n"));
                            WriteInt32(stream, bytes.Length);
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }

                return BitConverter.ToString(hash.ComputeHash(stream.ToArray())).Replace("-", string.Empty);
            }
        }

        public static bool ValidateBaseline(AIDataSyncManifest manifest, string currentFingerprint, AIDataSyncReportItem report)
        {
            if (manifest == null)
            {
                report.errors.Add("AI JSON manifest is missing.");
                return false;
            }

            report.baselineFingerprint = manifest.sourceFingerprint;
            report.currentFingerprint = currentFingerprint;
            if (string.IsNullOrWhiteSpace(manifest.sourceFingerprint))
            {
                report.errors.Add("AI JSON manifest has no source fingerprint.");
                return false;
            }

            if (!string.Equals(manifest.sourceFingerprint, currentFingerprint, StringComparison.OrdinalIgnoreCase))
            {
                report.errors.Add("Source xlsx fingerprint differs from the JSON export baseline. Re-export JSON before reversing.");
                return false;
            }

            return true;
        }

        public static bool ReplaceFilesTransactionally(IList<AIDataFileReplacement> replacements, AIDataSyncReportItem report)
        {
            if (replacements == null || replacements.Count == 0)
            {
                report.errors.Add("No file replacements were provided.");
                return false;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string backupRoot = Path.Combine(projectRoot, "Temp", "AIDataSyncBackups", Guid.NewGuid().ToString("N"));
            var completed = new List<AIDataFileReplacement>();
            try
            {
                foreach (AIDataFileReplacement replacement in replacements)
                {
                    if (replacement == null || string.IsNullOrWhiteSpace(replacement.sourceFile) || string.IsNullOrWhiteSpace(replacement.destinationFile))
                    {
                        throw new InvalidOperationException("Each replacement requires a source and destination file.");
                    }

                    if (!File.Exists(replacement.sourceFile))
                    {
                        throw new FileNotFoundException("Replacement source does not exist.", replacement.sourceFile);
                    }

                    string destinationDirectory = Path.GetDirectoryName(replacement.destinationFile);
                    Directory.CreateDirectory(destinationDirectory);
                    if (File.Exists(replacement.destinationFile))
                    {
                        string backupFile = Path.Combine(backupRoot, completed.Count.ToString("D4") + ".bak");
                        Directory.CreateDirectory(backupRoot);
                        File.Copy(replacement.destinationFile, backupFile, true);
                        replacement.backupFile = backupFile;
                    }

                    File.Copy(replacement.sourceFile, replacement.destinationFile, true);
                    completed.Add(replacement);
                }

                report.rollbackSucceeded = true;
                return true;
            }
            catch (Exception exception)
            {
                report.errors.Add($"Transactional replacement failed: {exception.Message}");
                bool rollbackSucceeded = true;
                for (int i = completed.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        AIDataFileReplacement replacement = completed[i];
                        if (string.IsNullOrWhiteSpace(replacement.backupFile))
                        {
                            File.Delete(replacement.destinationFile);
                        }
                        else
                        {
                            File.Copy(replacement.backupFile, replacement.destinationFile, true);
                        }
                    }
                    catch (Exception rollbackException)
                    {
                        rollbackSucceeded = false;
                        report.errors.Add($"Rollback failed: {rollbackException.Message}");
                    }
                }

                report.rollbackSucceeded = rollbackSucceeded;
                return false;
            }
            finally
            {
                if (Directory.Exists(backupRoot))
                {
                    try
                    {
                        Directory.Delete(backupRoot, true);
                    }
                    catch (Exception cleanupException)
                    {
                        report.warnings.Add($"Could not remove temporary backup directory: {cleanupException.Message}");
                    }
                }
            }
        }

        private static void WriteInt32(Stream stream, int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    public sealed class AIDataFileReplacement
    {
        public string sourceFile;
        public string destinationFile;
        internal string backupFile;
    }
}
#endif
