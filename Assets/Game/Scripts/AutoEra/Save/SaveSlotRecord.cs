using System;

namespace AutoEra.Save
{
    /// <summary>
    /// Persisted record of a single save slot. Metadata (version, slot, save time, length, checksum)
    /// is kept separate from the formal save content, which is carried as an opaque JSON blob until
    /// the world progression layer interprets it.
    ///
    /// **格式版本 2 加了四件事**，对应规格「每份存档包含格式版本、槽位ID、保存UTC时间、数据长度、
    /// 校验值和离线结算进度标记」：槽位ID、保存UTC时间、数据长度与校验值。
    /// 版本 1 的旧记录没有这些字段，读取时按迁移入口接受（见 <see cref="SaveSlotService"/>）——
    /// 有意不抛错，否则一次格式升级就会把玩家所有旧存档判成损坏。
    /// </summary>
    [Serializable]
    public sealed class SaveSlotRecord
    {
        /// <summary>Current save format version; increment on breaking format changes and add a migration.</summary>
        public const int CurrentVersion = 2;

        /// <summary>校验值开始成为格式一部分的版本。更早的记录没有校验值，只能按迁移入口接受。</summary>
        public const int FirstChecksumVersion = 2;

        /// <summary>Save format version (reserved migration slot).</summary>
        public int Version { get; set; }

        /// <summary>
        /// 槽位ID。校验它有两个理由：把文件复制到别的槽位时能被发现，
        /// 以及「这份备份到底属于哪个槽位」不必靠文件名猜。
        /// </summary>
        public int SlotIndex { get; set; }

        /// <summary>保存时刻的 UTC 时间（ticks）。恢复界面要说清「恢复到哪个时间点」，靠的就是它。</summary>
        public long SavedUtcTicks { get; set; }

        /// <summary>Snapshot of the deterministic world clock in milliseconds at save time. Reading it never advances or blocks the clock.</summary>
        public long WorldTimeMilliseconds { get; set; }

        /// <summary>Human-readable slot summary shown in the save list.</summary>
        public string Summary { get; set; }

        /// <summary>
        /// 正式内容的字节长度。它与校验值一起构成「内容是否被改动过」的判据：
        /// 只校验哈希会漏掉「内容被截断且哈希字段同时被清零」这类损坏。
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>正式内容的校验值（见 <see cref="SaveChecksum"/>）。格式 1 的记录为 null。</summary>
        public string Checksum { get; set; }

        /// <summary>离线结算是否尚未完成。未完成的槽位不允许进入世界操作（规格：结算未完成前不允许进入世界操作）。</summary>
        public bool OfflineSettlementPending { get; set; }

        /// <summary>Formal save content as an opaque JSON string.</summary>
        public string ContentJson { get; set; }

        /// <summary>保存时刻的 UTC 时间；没有记录时为 <see cref="DateTime.MinValue"/>。</summary>
        public DateTime SavedUtc =>
            SavedUtcTicks > 0 && SavedUtcTicks <= DateTime.MaxValue.Ticks
                ? new DateTime(SavedUtcTicks, DateTimeKind.Utc)
                : DateTime.MinValue;

        /// <summary>这份记录是否带有校验值（格式 2 起）。</summary>
        public bool HasChecksum => !string.IsNullOrEmpty(Checksum);
    }
}
