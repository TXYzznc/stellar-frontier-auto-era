#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using OfficeOpenXml;
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
            return TryResolvePath(GetExcelRoot(kind), relativePath, extension, out fullPath, out error);
        }

        public static bool TryResolveAIJsonPath(AIDataKind kind, string relativePath, out string fullPath, out string error)
        {
            return TryResolvePath(GetJsonRoot(kind), relativePath, ".json", out fullPath, out error);
        }

        private static bool TryResolvePath(string root, string relativePath, string extension, out string fullPath, out string error)
        {
            fullPath = null;
            if (!TryNormalizeRelativePath(relativePath, out string normalized, out error))
            {
                return false;
            }

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

        public static bool ReplaceFilesTransactionally(IList<AIDataFileReplacement> replacements, AIDataSyncReportItem report,
            IDictionary<string, string> readDependencies = null, Action<string, bool> writeCheckpoint = null)
        {
            if (replacements == null || replacements.Count == 0)
            {
                report.errors.Add("No file replacements were provided.");
                return false;
            }

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string backupRoot = Path.Combine(projectRoot, "Temp", "AIDataSyncBackups", Guid.NewGuid().ToString("N"));
            var completed = new List<AIDataFileReplacement>();
            var lockedTargets = new Dictionary<AIDataFileReplacement, FileStream>();
            var existedTargets = new HashSet<AIDataFileReplacement>();
            var readLocks = new List<FileStream>();
            bool preserveBackups = false;
            try
            {
                if (readDependencies != null)
                {
                    foreach (var dependency in readDependencies)
                    {
                        if (dependency.Value == string.Empty)
                        {
                            if (File.Exists(dependency.Key)) throw new IOException("Read dependency appeared after staging: " + dependency.Key);
                            continue;
                        }
                        var input = new FileStream(dependency.Key, FileMode.Open, FileAccess.Read, FileShare.Read);
                        readLocks.Add(input);
                        using (var hash = SHA256.Create())
                            if (BitConverter.ToString(hash.ComputeHash(input)).Replace("-", string.Empty) != dependency.Value)
                                throw new IOException("Read dependency changed after staging: " + dependency.Key);
                    }
                }
                var destinations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (AIDataFileReplacement replacement in replacements)
                {
                    if (replacement == null || string.IsNullOrWhiteSpace(replacement.sourceFile) ||
                        string.IsNullOrWhiteSpace(replacement.destinationFile) || !File.Exists(replacement.sourceFile))
                        throw new InvalidOperationException("Every staged replacement must exist before committing.");
                    if (!destinations.Add(Path.GetFullPath(replacement.destinationFile)))
                        throw new InvalidOperationException("Duplicate transaction destination.");
                    if (string.Equals(Path.GetFullPath(replacement.sourceFile), Path.GetFullPath(replacement.destinationFile), StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Staged source must differ from destination.");
                    replacement.backupFile = null;
                    if (replacement.expectedDestinationFingerprint != null &&
                        replacement.expectedDestinationFingerprint != ComputeFileFingerprint(replacement.destinationFile))
                        throw new IOException("Destination changed after staging: " + replacement.destinationFile);
                }
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
                    bool existed = File.Exists(replacement.destinationFile);
                    var target = new FileStream(replacement.destinationFile, existed ? FileMode.Open : FileMode.CreateNew,
                        FileAccess.ReadWrite, FileShare.None);
                    lockedTargets.Add(replacement, target);
                    if (existed) existedTargets.Add(replacement);
                    if (replacement.expectedDestinationFingerprint != null)
                    {
                        string actual;
                        using (var hash = SHA256.Create())
                            actual = existed ? BitConverter.ToString(hash.ComputeHash(target)).Replace("-", string.Empty) : string.Empty;
                        target.Position = 0;
                        if (actual != replacement.expectedDestinationFingerprint)
                            throw new IOException("Destination changed before exclusive commit: " + replacement.destinationFile);
                    }
                    if (existed)
                    {
                        string backupFile = Path.Combine(backupRoot, completed.Count.ToString("D4") + ".bak");
                        Directory.CreateDirectory(backupRoot);
                        using (var backup = File.Create(backupFile)) target.CopyTo(backup);
                        target.Position = 0;
                        replacement.backupFile = backupFile;
                    }

                    completed.Add(replacement);
                    using (var source = File.OpenRead(replacement.sourceFile))
                    {
                        target.SetLength(0);
                        // Scoped fault-injection seam for transaction tests; no global mutable hook.
                        writeCheckpoint?.Invoke(replacement.destinationFile, false);
                        source.CopyTo(target);
                        target.Flush(true);
                    }
                }

                report.rollbackSucceeded = true;
                foreach (var replacement in replacements)
                    DataTableUpdater.RecordCommittedVersion(replacement.destinationFile, ComputeFileFingerprint(replacement.sourceFile));
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
                        writeCheckpoint?.Invoke(replacement.destinationFile, true);
                        if (string.IsNullOrWhiteSpace(replacement.backupFile))
                        {
                            lockedTargets[replacement].Dispose();
                            File.Delete(replacement.destinationFile);
                        }
                        else
                        {
                            var target = lockedTargets[replacement];
                            target.Position = 0;
                            target.SetLength(0);
                            using (var backup = File.OpenRead(replacement.backupFile)) backup.CopyTo(target);
                            target.Flush(true);
                        }
                    }
                    catch (Exception rollbackException)
                    {
                        rollbackSucceeded = false;
                        preserveBackups = true;
                        report.errors.Add($"Rollback failed: {rollbackException.Message}");
                    }
                }

                report.rollbackSucceeded = rollbackSucceeded;
                if (preserveBackups)
                    report.errors.Add("Recovery backups retained at: " + backupRoot);
                return false;
            }
            finally
            {
                foreach (var input in readLocks) input.Dispose();
                foreach (var pair in lockedTargets)
                {
                    pair.Value.Dispose();
                    if (!completed.Contains(pair.Key) && !existedTargets.Contains(pair.Key))
                    {
                        try { File.Delete(pair.Key.destinationFile); }
                        catch (Exception cleanupException) { report.warnings.Add(cleanupException.Message); }
                    }
                }
                if (!preserveBackups && Directory.Exists(backupRoot))
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

        // Empty string represents a missing file, not an empty existing file.
        public static string ComputeFileFingerprint(string path)
        {
            if (!File.Exists(path)) return string.Empty;
            using (var hash = SHA256.Create())
            using (var stream = File.OpenRead(path))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }

        public static bool TryBuildTemporaryExcel(IList<string[]> rows, out string temporaryFile, out string error)
        {
            temporaryFile = null;
            error = null;
            try
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName;
                string stagingDirectory = Path.Combine(projectRoot, "Temp", "AIDataSyncStaging");
                Directory.CreateDirectory(stagingDirectory);
                temporaryFile = Path.Combine(stagingDirectory, Guid.NewGuid().ToString("N") + ".xlsx");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var package = new ExcelPackage(new FileInfo(temporaryFile)))
                {
                    var sheet = package.Workbook.Worksheets.Add("Sheet 1");
                    for (int row = 0; rows != null && row < rows.Count; row++)
                    {
                        string[] cells = rows[row] ?? Array.Empty<string>();
                        for (int column = 0; column < cells.Length; column++)
                        {
                            sheet.SetValue(row + 1, column + 1, cells[column] ?? string.Empty);
                        }
                    }

                    package.Save();
                }

                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                if (!string.IsNullOrWhiteSpace(temporaryFile) && File.Exists(temporaryFile))
                {
                    File.Delete(temporaryFile);
                }

                temporaryFile = null;
                return false;
            }
        }

        public static string ComputeWorksheetFingerprint(IList<string[]> rows)
        {
            int width = 0;
            foreach (var row in rows) width = Math.Max(width, row?.Length ?? 0);
            var rectangular = new List<string[]>();
            foreach (var row in rows)
            {
                var cells = new string[width];
                if (row != null) Array.Copy(row, cells, row.Length);
                rectangular.Add(cells);
            }
            return ComputeLogicalFingerprint(rectangular);
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
        public string expectedDestinationFingerprint;
        internal string backupFile;
    }
}
#endif
