using System;

namespace AutoEra.Save
{
    /// <summary>Outcome of reading a save slot.</summary>
    public enum SaveSlotReadStatus
    {
        /// <summary>正式存档可读，内容通过校验。</summary>
        Success = 0,

        /// <summary>The slot has no save file.</summary>
        Empty = 1,

        /// <summary>正式存档与三份备份都不可读或未通过校验。</summary>
        Corrupt = 2,

        /// <summary>The slot was written by a newer format version this build cannot read.</summary>
        NewerVersion = 3,

        /// <summary>
        /// 正式存档不可用，但某一份备份可读。
        ///
        /// 这是一个**独立状态**而不是 Success 的附注：规格要求「使用备份时告知玩家恢复时间点，
        /// 不静默回退」，如果回退读备份仍然报 Success，所有消费方（列表、详情、存档状态灯）
        /// 都会把「正在读一份旧备份」显示成「一切正常」——而那正是静默回退。
        /// </summary>
        RecoveredFromBackup = 4,
    }

    /// <summary>一份备份的可展示信息（恢复界面要用「有效备份时间」）。</summary>
    public readonly struct SaveSlotBackupInfo
    {
        public SaveSlotBackupInfo(int index, bool exists, bool usable, SaveSlotRecord record)
        {
            Index = index;
            Exists = exists;
            Usable = usable;
            Record = record;
        }

        /// <summary>备份序号：1 是最新的一份，<see cref="SaveSlotService.BackupCount"/> 是最旧的。</summary>
        public int Index { get; }

        /// <summary>该序号的备份文件是否存在。</summary>
        public bool Exists { get; }

        /// <summary>是否可读**且**通过校验（可读但校验失败的备份不能用来恢复）。</summary>
        public bool Usable { get; }

        /// <summary>备份里的记录；不存在或不可读时为 null。</summary>
        public SaveSlotRecord Record { get; }
    }

    /// <summary>Result of a <see cref="SaveSlotService.Read"/> call.</summary>
    public readonly struct SaveSlotReadResult
    {
        public SaveSlotReadResult(SaveSlotReadStatus status, SaveSlotRecord record)
            : this(status, record, 0)
        {
        }

        public SaveSlotReadResult(SaveSlotReadStatus status, SaveSlotRecord record, int backupIndex)
        {
            Status = status;
            Record = record;
            BackupIndex = backupIndex;
        }

        public SaveSlotReadStatus Status { get; }

        public SaveSlotRecord Record { get; }

        /// <summary>0 ＝ 读的是正式存档；1..3 ＝ 读的是第几份备份（1 最新）。</summary>
        public int BackupIndex { get; }

        /// <summary>本次读取是否用到了备份。</summary>
        public bool IsFromBackup => BackupIndex > 0;

        /// <summary>
        /// 是否读到了可用内容。**包含 <see cref="SaveSlotReadStatus.RecoveredFromBackup"/>**：
        /// 调用方想知道「有没有内容可展示」时用这个，而不是拿 Status 去比 Success。
        /// </summary>
        public bool IsSuccess =>
            Status == SaveSlotReadStatus.Success || Status == SaveSlotReadStatus.RecoveredFromBackup;

        /// <summary>是否读到了正式存档本身（没有回退）。</summary>
        public bool IsPrimary => Status == SaveSlotReadStatus.Success;
    }
}
