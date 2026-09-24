using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace AutoEra.Save
{
    /// <summary>
    /// Three-slot save service. Owns create / overwrite / read / delete for exactly three isolated
    /// slots and persists metadata (version, slot, save time, length, checksum) alongside the content.
    ///
    /// **写入顺序就是规格写的那条**（10-存档与状态持久化「安全快照与异步写入」）：
    /// 序列化 → 写临时文件 → **校验临时文件** → **轮转备份** → 原子替换正式存档。
    /// 校验发生在替换之前，所以一个写坏的临时文件永远不会变成正式存档。
    ///
    /// 每个槽位保存 **1 份正式存档 ＋ 最近 3 份滚动备份**（同文档「第一版存档槽位」）。
    /// 备份只用于损坏恢复、不作为普通读取项显示，删除槽位时三份一起删。
    /// 读取依次尝试正式存档与三份由新到旧的备份，并且**如实报告用的是哪一份**——
    /// 静默回退会让玩家以为自己玩的还是最新进度。
    ///
    /// World time is captured as a value snapshot only — the service never advances or blocks the
    /// deterministic world clock. The root directory is injectable so EditMode tests can target a
    /// temporary folder.
    /// </summary>
    public sealed class SaveSlotService
    {
        public const int SlotCount = 3;

        /// <summary>每个槽位保留的滚动备份份数（规格：最近 3 份）。</summary>
        public const int BackupCount = 3;

        private readonly string _rootDirectory;
        private readonly Func<DateTime> _utcNow;

        public SaveSlotService(string rootDirectory)
            : this(rootDirectory, null)
        {
        }

        /// <summary>
        /// 注入「现在」的时钟，便于测试断言保存UTC时间；
        /// 传 null 时用 <see cref="DateTime.UtcNow"/>。
        /// </summary>
        public SaveSlotService(string rootDirectory, Func<DateTime> utcNow)
        {
            if (string.IsNullOrWhiteSpace(rootDirectory))
            {
                throw new ArgumentException("Save root directory must not be empty.", nameof(rootDirectory));
            }

            _rootDirectory = rootDirectory;
            _utcNow = utcNow ?? (() => DateTime.UtcNow);
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
            return Create(slotIndex, summary, worldTimeMilliseconds, contentJson, false);
        }

        public bool Create(int slotIndex, string summary, long worldTimeMilliseconds, string contentJson,
            bool offlineSettlementPending)
        {
            ValidateSlotIndex(slotIndex);
            if (File.Exists(GetSlotPath(slotIndex)))
            {
                return false;
            }

            return Write(slotIndex, summary, worldTimeMilliseconds, contentJson, offlineSettlementPending);
        }

        /// <summary>Writes a save regardless of the slot's current occupancy, replacing any existing content.</summary>
        public bool Overwrite(int slotIndex, string summary, long worldTimeMilliseconds, string contentJson)
        {
            return Overwrite(slotIndex, summary, worldTimeMilliseconds, contentJson, false);
        }

        public bool Overwrite(int slotIndex, string summary, long worldTimeMilliseconds, string contentJson,
            bool offlineSettlementPending)
        {
            ValidateSlotIndex(slotIndex);
            return Write(slotIndex, summary, worldTimeMilliseconds, contentJson, offlineSettlementPending);
        }

        /// <summary>Reads a slot. See <see cref="SaveSlotReadStatus"/> for the possible outcomes.</summary>
        public SaveSlotReadResult Read(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            string path = GetSlotPath(slotIndex);

            bool mainExists = File.Exists(path);
            if (mainExists)
            {
                SaveSlotReadStatus mainStatus = TryReadRecord(path, slotIndex, out SaveSlotRecord main);
                if (mainStatus == SaveSlotReadStatus.NewerVersion)
                {
                    // 版本过新是**确定的结论**，不该被备份悄悄盖过去：玩家需要换新版本，而不是回退进度。
                    return new SaveSlotReadResult(SaveSlotReadStatus.NewerVersion, null);
                }

                if (mainStatus == SaveSlotReadStatus.Success)
                {
                    return new SaveSlotReadResult(SaveSlotReadStatus.Success, main);
                }
            }

            // 正式存档不存在或不可用：依次试三份备份（由新到旧），并报告用的是哪一份。
            for (int index = 1; index <= BackupCount; index++)
            {
                string backupPath = GetBackupPath(slotIndex, index);
                if (!File.Exists(backupPath))
                {
                    continue;
                }

                SaveSlotReadStatus status = TryReadRecord(backupPath, slotIndex, out SaveSlotRecord backup);
                if (status == SaveSlotReadStatus.NewerVersion)
                {
                    return new SaveSlotReadResult(SaveSlotReadStatus.NewerVersion, null);
                }

                if (status == SaveSlotReadStatus.Success)
                {
                    return new SaveSlotReadResult(SaveSlotReadStatus.RecoveredFromBackup, backup, index);
                }
            }

            if (!mainExists && !HasBackup(slotIndex))
            {
                return new SaveSlotReadResult(SaveSlotReadStatus.Empty, null);
            }

            return new SaveSlotReadResult(SaveSlotReadStatus.Corrupt, null);
        }

        /// <summary>
        /// Deletes a slot and **all** of its auxiliary files：临时文件与三份备份。
        /// 规格要求删除时明确告知「正式存档与三份内部备份都会被删除」，所以这里不能只删主文件——
        /// 留下备份会让「已删除的进度」还能被恢复出来。
        /// </summary>
        public bool Delete(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            string path = GetSlotPath(slotIndex);
            bool deleted = TryDeleteFile(path);
            deleted |= TryDeleteFile(path + ".tmp");
            for (int index = 1; index <= BackupCount; index++)
            {
                deleted |= TryDeleteFile(GetBackupPath(slotIndex, index));
            }

            return deleted;
        }

        /// <summary>是否存在任何一份备份文件（不判断能不能读）。</summary>
        public bool HasBackup(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            for (int index = 1; index <= BackupCount; index++)
            {
                if (File.Exists(GetBackupPath(slotIndex, index)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在**可用**的备份（可读且通过校验）。界面用它决定「恢复」能不能点：
        /// 存在一个读不出来的备份文件却把按钮点亮，只会让玩家点下去才发现救不回来。
        /// </summary>
        public bool HasUsableBackup(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return FindNewestUsableBackup(slotIndex) > 0;
        }

        /// <summary>三份备份的可展示信息，序号由新到旧。</summary>
        public IReadOnlyList<SaveSlotBackupInfo> ListBackups(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            var list = new List<SaveSlotBackupInfo>(BackupCount);
            for (int index = 1; index <= BackupCount; index++)
            {
                string path = GetBackupPath(slotIndex, index);
                bool exists = File.Exists(path);
                SaveSlotRecord record = null;
                bool usable = false;
                if (exists && TryReadRecord(path, slotIndex, out record) == SaveSlotReadStatus.Success)
                {
                    usable = true;
                }

                list.Add(new SaveSlotBackupInfo(index, exists, usable, usable ? record : null));
            }

            return list;
        }

        /// <summary>
        /// 把**最新一份可用备份**提升为正式存档，用于正式存档损坏后的恢复。
        ///
        /// 返回 false 表示没有可用备份，此时正式存档保持原样——恢复失败不得让玩家丢掉更多东西。
        /// 提升之后备份链按序号前移（被提升的那份从链上移除，比它旧的依次前移），
        /// 这样「序号 1 最新」这条不变式在任何一份被提升之后都仍然成立。
        /// </summary>
        public bool RestoreFromBackup(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            int index = FindNewestUsableBackup(slotIndex);
            if (index <= 0)
            {
                return false;
            }

            string backupPath = GetBackupPath(slotIndex, index);
            if (TryReadRecord(backupPath, slotIndex, out SaveSlotRecord record) != SaveSlotReadStatus.Success)
            {
                return false;
            }

            try
            {
                EnsureDirectory();
                string json = JsonConvert.SerializeObject(record, Formatting.Indented);
                // 只替换正式存档、不产生新备份：备份链的轮转由下面的移位负责，
                // 否则「提升」会把正要被提升的那份又写成一个新备份。
                ReplacePrimary(GetSlotPath(slotIndex), json);
                ShiftBackupsAfterPromotion(slotIndex, index);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>正式存档的完整路径（测试与工具用）。</summary>
        public string GetSlotPath(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return Path.Combine(_rootDirectory, $"slot_{slotIndex}.json");
        }

        /// <summary>第 index 份备份的路径（1 最新，测试与工具用）。</summary>
        public string GetBackupPath(int slotIndex, int backupIndex)
        {
            ValidateSlotIndex(slotIndex);
            if (backupIndex < 1 || backupIndex > BackupCount)
            {
                throw new ArgumentOutOfRangeException(nameof(backupIndex), backupIndex,
                    $"Backup index must be within [1, {BackupCount}].");
            }

            return GetSlotPath(slotIndex) + ".bak" + backupIndex;
        }

        public bool Exists(int slotIndex)
        {
            ValidateSlotIndex(slotIndex);
            return File.Exists(GetSlotPath(slotIndex));
        }

        // ---------------------------------------------------------------- 读

        /// <summary>
        /// 读取一个文件并判定它是否可用。返回 Success 之外的状态时 <paramref name="record"/> 为 null。
        /// </summary>
        private SaveSlotReadStatus TryReadRecord(string path, int expectedSlot, out SaveSlotRecord record)
        {
            record = TryDeserialize(path);
            if (record == null)
            {
                return SaveSlotReadStatus.Corrupt;
            }

            if (record.Version > SaveSlotRecord.CurrentVersion)
            {
                record = null;
                return SaveSlotReadStatus.NewerVersion;
            }

            // 槽位ID必须对得上：把别的槽位的文件复制过来是**明确的损坏**，
            // 而不是「一份能读的存档」——否则复制文件就能把进度搬到另一个世界上。
            if (record.SlotIndex != expectedSlot)
            {
                record = null;
                return SaveSlotReadStatus.Corrupt;
            }

            // 格式 1 没有校验值，按迁移入口接受：一次格式升级不该把旧存档判成损坏。
            if (record.Version >= SaveSlotRecord.FirstChecksumVersion)
            {
                if (record.ContentLength != (record.ContentJson ?? string.Empty).Length ||
                    !SaveChecksum.Matches(record.Checksum, SaveChecksum.Compute(record)))
                {
                    record = null;
                    return SaveSlotReadStatus.Corrupt;
                }
            }

            return SaveSlotReadStatus.Success;
        }

        private static SaveSlotRecord TryDeserialize(string path)
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

        private int FindNewestUsableBackup(int slotIndex)
        {
            for (int index = 1; index <= BackupCount; index++)
            {
                string path = GetBackupPath(slotIndex, index);
                if (File.Exists(path) && TryReadRecord(path, slotIndex, out _) == SaveSlotReadStatus.Success)
                {
                    return index;
                }
            }

            return 0;
        }

        // ---------------------------------------------------------------- 写

        private bool Write(int slotIndex, string summary, long worldTimeMilliseconds, string contentJson,
            bool offlineSettlementPending)
        {
            string content = contentJson ?? string.Empty;
            var record = new SaveSlotRecord
            {
                Version = SaveSlotRecord.CurrentVersion,
                SlotIndex = slotIndex,
                SavedUtcTicks = _utcNow().ToUniversalTime().Ticks,
                WorldTimeMilliseconds = worldTimeMilliseconds,
                Summary = summary ?? string.Empty,
                ContentLength = content.Length,
                OfflineSettlementPending = offlineSettlementPending,
                ContentJson = content,
            };
            record.Checksum = SaveChecksum.Compute(record);

            try
            {
                EnsureDirectory();
                string json = JsonConvert.SerializeObject(record, Formatting.Indented);
                string path = GetSlotPath(slotIndex);
                string tempPath = path + ".tmp";

                // ① 写临时文件
                File.WriteAllText(tempPath, json, Encoding.UTF8);

                // ② 校验临时文件——在替换之前。写坏的内容永远不该变成正式存档。
                if (TryReadRecord(tempPath, slotIndex, out _) != SaveSlotReadStatus.Success)
                {
                    TryDeleteFile(tempPath);
                    return false;
                }

                // ③ 轮转备份（bak2→bak3、bak1→bak2，最旧的丢弃）
                RotateBackups(slotIndex);

                // ④ 原子替换：被替换下来的正式存档成为新的 bak1
                ReplacePrimary(path, null, GetBackupPath(slotIndex, 1));
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>把备份链整体后退一位，为新产生的 bak1 腾位置。</summary>
        private void RotateBackups(int slotIndex)
        {
            TryDeleteFile(GetBackupPath(slotIndex, BackupCount));
            for (int index = BackupCount - 1; index >= 1; index--)
            {
                string from = GetBackupPath(slotIndex, index);
                if (!File.Exists(from))
                {
                    continue;
                }

                string to = GetBackupPath(slotIndex, index + 1);
                TryDeleteFile(to);
                TryMoveFile(from, to);
            }
        }

        /// <summary>
        /// 提升一份备份之后整理链：**比被提升的那份更新的备份按定义都不可用**（否则提升的就是它们），
        /// 所以直接删掉；比它旧的依次前移一位。这样「序号 1 最新」与「链上都是可用内容」两条
        /// 同时成立。
        /// </summary>
        private void ShiftBackupsAfterPromotion(int slotIndex, int promotedIndex)
        {
            for (int index = 1; index < promotedIndex; index++)
            {
                TryDeleteFile(GetBackupPath(slotIndex, index));
            }

            for (int index = promotedIndex; index <= BackupCount; index++)
            {
                string to = GetBackupPath(slotIndex, index);
                string from = index < BackupCount ? GetBackupPath(slotIndex, index + 1) : null;
                if (from != null && File.Exists(from))
                {
                    TryDeleteFile(to);
                    TryMoveFile(from, to);
                }
                else
                {
                    TryDeleteFile(to);
                }
            }
        }

        private void EnsureDirectory()
        {
            if (!Directory.Exists(_rootDirectory))
            {
                Directory.CreateDirectory(_rootDirectory);
            }
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

        /// <summary>
        /// 用临时文件原子替换某个文件。
        ///
        /// <paramref name="content"/> 为 null 时表示「临时文件已经写好，直接换上」——
        /// 这是写入路径的第四步；非 null 时先把内容写进临时文件，这是恢复提升路径。
        /// </summary>
        private static void ReplacePrimary(string path, string content, string backupPath = null)
        {
            string tempPath = path + ".tmp";
            if (content != null)
            {
                File.WriteAllText(tempPath, content, Encoding.UTF8);
            }

            if (!File.Exists(path))
            {
                File.Move(tempPath, path);
                return;
            }

            if (backupPath == null)
            {
                // 无备份的替换：File.Replace 传 null 就是「原子替换、不留备份」。
                try
                {
                    File.Replace(tempPath, path, null);
                    return;
                }
                catch (PlatformNotSupportedException)
                {
                    File.Delete(path);
                    File.Move(tempPath, path);
                    return;
                }
            }

            TryDeleteFile(backupPath);
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

        private static bool TryMoveFile(string from, string to)
        {
            try
            {
                File.Move(from, to);
                return true;
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                return false;
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
