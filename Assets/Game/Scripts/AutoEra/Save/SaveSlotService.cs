using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace AutoEra.Save
{
    /// <summary>
    /// Three-slot save service. Owns create / overwrite / read / delete for exactly three isolated
    /// slots, and persists metadata (version, world time, summary) alongside the formal content.
    ///
    /// Writes use "temporary file + atomic replace + backup" so an interrupted write never corrupts
    /// the last good save; reads fall back to the backup when the main file is malformed. World time
    /// is captured as a value snapshot only — the service never advances or blocks the deterministic
    /// world clock. The root directory is injectable so EditMode tests can target a temporary folder.
    /// </summary>
    public sealed class SaveSlotService
    {
        public const int SlotCount = 3;

        private readonly string _rootDirectory;

        public SaveSlotService(string rootDirectory)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentException("Save root directory must not be empty.", nameof(rootDirectory));
            }

            _rootDirectory = rootDirectory;
        }

        public string RootDirectory => _rootDirectory;

        /// <summary>Creates the service rooted under the platform persistent data path.</summary>
        public static SaveSlotService CreateDefault()
        {
            return new SaveSlotService(Path.Combine(UnityEngine.Application.persistentDataPath, "AutoEra", "SaveSlots"));
        }

        public static bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < SlotCount;
        }

        /// <summary>Creates a new save in an empty slot. Returns false when the slot is already occupied.</summary>
        public bool Create(int slotIndex, string summary, long worldTimeMilliseconds, string contentJson)
        {
            ValidateSlotIndex(slotIndex);
            string path = GetSlotPath(slotIndex);
            if (File.Exists(path))
            {
                return false;
            }

            return Write(slotIndex, summary, worldTimeMilliseconds, contentJson);
        }

        /// <summary>Writes a save regardless of the slot's current occupancy, replacing any existing content.</summary>
        public bool Overwrite(int slotIndex, string summary, long worldTimeMilliseconds, string contentJson)
        {
            ValidateSlotIndex(slotIndex);
            return Write(slotIndex, summary, worldTimeMilliseconds, contentJson);
        }

        /// <summary>Reads a slot. See <see cref="SaveSlotReadStatus"/> for the possible outcomes.</summary>
        public SaveSlotReadResult Read(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            string path = GetSlotPath(slotIndex);
            if (!File.Exists(path))
            {
                return new SaveSlotReadResult(SaveSlotReadStatus.Empty, null);
            }

            SaveSlotRecord record = TryReadFile(path);
            if (record != null)
            {
                return ValidateVersion(record);
            }

            // Main file is malformed; roll back to the backup written by the previous save.
            string backupPath = path + ".bak";
            if (File.Exists(backupPath))
            {
                SaveSlotRecord backup = TryReadFile(backupPath);
                if (backup != null)
                {
                    return ValidateVersion(backup);
                }
            }

            return new SaveSlotReadResult(SaveSlotReadStatus.Corrupt, null);
        }

        /// <summary>Deletes a slot and all of its auxiliary files (backup and temporary).</summary>
        public bool Delete(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            string path = GetSlotPath(slotIndex);
            bool deleted = false;
            deleted |= TryDeleteFile(path);
            deleted |= TryDeleteFile(path + ".bak");
            deleted |= TryDeleteFile(path + ".tmp");
            return deleted;
        }

        /// <summary>是否存在可用的备份文件。界面用它预先禁用不可行的「恢复」，而不是点下去才失败。</summary>
        public bool HasBackup(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return File.Exists(GetSlotPath(slotIndex) + ".bak");
        }

        /// <summary>
        /// 把备份提升为主文件，用于主文件损坏后的恢复。
        ///
        /// 返回 false 表示没有可用备份，此时主文件保持原样——恢复失败不得让玩家丢掉更多东西。
        /// 注意 <see cref="Read"/> 本来就会在主文件损坏时回退读备份，所以「能不能读出来」
        /// 与「备份有没有被提升为主文件」是两件事：前者只影响显示，后者才真正修好存档。
        /// </summary>
        public bool RestoreFromBackup(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            string path = GetSlotPath(slotIndex);
            string backupPath = path + ".bak";
            if (!File.Exists(backupPath))
            {
                return false;
            }

            SaveSlotRecord backup = TryReadFile(backupPath);
            if (backup == null)
            {
                return false;
            }

            try
            {
                EnsureDirectory();
                string json = JsonConvert.SerializeObject(backup, Formatting.Indented);
                // 先把备份挪开，避免 WriteAtomic 用自己的副本覆盖掉即将被提升的那份内容。
                string promoted = backupPath + ".promote";
                File.Copy(backupPath, promoted, overwrite: true);
                TryDeleteFile(backupPath);
                try
                {
                    WriteAtomic(path, json);
                }
                finally
                {
                    TryDeleteFile(promoted);
                }

                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }

        public bool Exists(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return File.Exists(GetSlotPath(slotIndex));
        }

        private SaveSlotReadResult ValidateVersion(SaveSlotRecord record)
        {
            if (record.Version > SaveSlotRecord.CurrentVersion)
            {
                return new SaveSlotReadResult(SaveSlotReadStatus.NewerVersion, null);
            }

            // Migration hook: when a future format bumps CurrentVersion, older records are upgraded here.
            return new SaveSlotReadResult(SaveSlotReadStatus.Success, record);
        }

        private bool Write(int slotIndex, string summary, long worldTimeMilliseconds, string contentJson)
        {
            var record = new SaveSlotRecord
            {
                Version = SaveSlotRecord.CurrentVersion,
                WorldTimeMilliseconds = worldTimeMilliseconds,
                Summary = summary ?? string.Empty,
                ContentJson = contentJson ?? string.Empty,
            };

            try
            {
                EnsureDirectory();
                string json = JsonConvert.SerializeObject(record, Formatting.Indented);
                WriteAtomic(GetSlotPath(slotIndex), json);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }
        }

        private string GetSlotPath(int slotIndex)
        {
            return Path.Combine(_rootDirectory, $"slot_{slotIndex}.json");
        }

        private void ValidateSlotIndex(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotIndex),
                    slotIndex,
                    $"Save slot index must be within [0, {SlotCount - 1}].");
            }
        }

        private static void WriteAtomic(string path, string content)
        {
            string tempPath = path + ".tmp";
            string backupPath = path + ".bak";

            File.WriteAllText(tempPath, content, Encoding.UTF8);
            if (File.Exists(path))
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }

                try
                {
                    File.Replace(tempPath, path, backupPath);
                }
                catch (PlatformNotSupportedException)
                {
                    // Non-Windows fallback: best-effort backup, then swap.
                    File.Copy(path, backupPath, overwrite: true);
                    File.Delete(path);
                    File.Move(tempPath, path);
                }
            }
            else
            {
                File.Move(tempPath, path);
            }
        }

        private static SaveSlotRecord TryReadFile(string path)
        {
            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                return JsonConvert.DeserializeObject<SaveSlotRecord>(json);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
            {
                return null;
            }
        }

        private static bool TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return true;
                }

                return false;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
