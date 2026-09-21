using System.Collections.Generic;
using System.Globalization;

namespace AutoEra.Save
{
    /// <summary>
    /// 存档状态的**统一说法**。
    ///
    /// 为什么把它放在存档域而不是某个 Form 里：规格对同一件事有硬要求——
    /// 「读取依次尝试正式存档和三份由新到旧的备份。**使用备份时告知玩家恢复时间点，不静默回退**」
    /// 与「恢复界面必须说明正式存档无法读取、**有效备份时间**和预计损失范围」。
    /// 这些句子会同时出现在存档列表、槽位详情与恢复页三处；分散写就会三处各说一套，
    /// 而其中任意一处说成「正常」都会构成静默回退。所以说法只有这一份，Form 只负责渲染。
    /// </summary>
    public static class SaveSlotNarrative
    {
        public static string DescribeStatus(SaveSlotReadStatus status)
        {
            switch (status)
            {
                case SaveSlotReadStatus.Success: return "正常";
                case SaveSlotReadStatus.Empty: return "无存档";
                case SaveSlotReadStatus.Corrupt: return "损坏";
                case SaveSlotReadStatus.NewerVersion: return "版本过新";
                case SaveSlotReadStatus.RecoveredFromBackup: return "正式存档不可读，已从备份读取";
                default: return "—";
            }
        }

        /// <summary>存档列表里的一行摘要：读的是备份时必须说出来，不能显示成普通占用。</summary>
        public static string DescribeRow(SaveSlotReadResult result)
        {
            switch (result.Status)
            {
                case SaveSlotReadStatus.Success:
                    return result.Record != null ? result.Record.Summary : string.Empty;
                case SaveSlotReadStatus.RecoveredFromBackup:
                    return "可读，但来自备份 " + result.BackupIndex + "（" + DescribeBackupTime(result.Record) + "）";
                case SaveSlotReadStatus.Corrupt:
                    return "存档损坏，可尝试恢复";
                case SaveSlotReadStatus.NewerVersion:
                    return "存档版本高于当前版本";
                default:
                    return "空槽位";
            }
        }

        /// <summary>本次读到的是哪一份。</summary>
        public static string DescribeOrigin(SaveSlotReadResult result)
        {
            if (result.IsFromBackup)
            {
                return "备份 " + result.BackupIndex + "（正式存档不可读）";
            }

            return result.Status == SaveSlotReadStatus.Success ? "正式存档" : "没有可读内容";
        }

        public static string DescribeBackupTime(SaveSlotRecord record)
        {
            if (record == null || record.SavedUtcTicks <= 0)
            {
                return "时间未知";
            }

            return record.SavedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>一份备份的一句话状态（恢复页的备份栏）。</summary>
        public static string DescribeBackup(SaveSlotBackupInfo backup)
        {
            if (!backup.Exists)
            {
                return "没有这一份";
            }

            if (!backup.Usable)
            {
                return "存在但不可读（不能用来恢复）";
            }

            return "可用，保存于 " + DescribeBackupTime(backup.Record)
                   + "（世界时间 " + FormatWorldTime(backup.Record) + "）";
        }

        /// <summary>
        /// 恢复页的正文。**必须说清「恢复到哪个时间点」与「预计损失范围」**：
        /// 只说「可以从备份恢复」会让玩家在不知道会丢多少的情况下按下按钮。
        /// 「预计损失范围」只能诚实到能给的程度——正式的进度在损坏文件里读不出来，
        /// 所以能说的是「从这份备份的保存时刻到上次成功保存之间」。
        /// </summary>
        public static string DescribeRecovery(SaveSlotReadResult result, IReadOnlyList<SaveSlotBackupInfo> backups)
        {
            if (result.Status == SaveSlotReadStatus.Empty)
            {
                return "这个槽位没有存档。";
            }

            SaveSlotBackupInfo? newest = NewestUsable(backups);
            if (newest == null)
            {
                return "正式存档无法读取，三份备份里也没有一份可用，无法恢复。";
            }

            SaveSlotBackupInfo usable = newest.Value;
            string time = DescribeBackupTime(usable.Record);
            if (result.Status == SaveSlotReadStatus.Success)
            {
                return "正式存档可以正常读取；当前也保留着可用的备份（最近一份保存于 " + time
                       + "）。恢复会用该备份覆盖正式存档，因此会退回那个时间点。";
            }

            if (result.Status == SaveSlotReadStatus.NewerVersion)
            {
                return "这份存档由更新的游戏版本写入，本版本无法读取，也不能用备份替代——"
                       + "请使用与存档版本匹配的游戏版本。";
            }

            return "正式存档无法读取。可恢复到备份 " + usable.Index + "，时间点 " + time
                   + "；预计损失范围是**这份备份保存之后**到上次成功保存之间的进度（正式存档已损坏，"
                   + "无法给出精确到分钟的范围）。恢复会把该备份提升为正式存档。";
        }

        /// <summary>最新的可用备份；没有可用备份时返回 null。</summary>
        public static SaveSlotBackupInfo? NewestUsable(IReadOnlyList<SaveSlotBackupInfo> backups)
        {
            if (backups == null)
            {
                return null;
            }

            // 列表本来就由新到旧，但这里不依赖调用方的排序：序号小的才是更新的。
            SaveSlotBackupInfo? best = null;
            for (int i = 0; i < backups.Count; i++)
            {
                if (!backups[i].Usable)
                {
                    continue;
                }

                if (best == null || backups[i].Index < best.Value.Index)
                {
                    best = backups[i];
                }
            }

            return best;
        }

        private static string FormatWorldTime(SaveSlotRecord record)
        {
            if (record == null)
            {
                return "—";
            }

            long totalSeconds = record.WorldTimeMilliseconds / 1000L;
            long days = totalSeconds / 86400L;
            long hours = totalSeconds % 86400L / 3600L;
            long minutes = totalSeconds % 3600L / 60L;
            return "第 " + days + " 天 " + hours.ToString("D2", CultureInfo.InvariantCulture) + ":"
                   + minutes.ToString("D2", CultureInfo.InvariantCulture);
        }
    }
}
