namespace AutoEra.Save
{
    /// <summary>Outcome of reading a save slot.</summary>
    public enum SaveSlotReadStatus
    {
        /// <summary>A valid record was read.</summary>
        Success = 0,

        /// <summary>The slot has no save file.</summary>
        Empty = 1,

        /// <summary>The slot file (and its backup) is unreadable or malformed.</summary>
        Corrupt = 2,

        /// <summary>The slot was written by a newer format version this build cannot read.</summary>
        NewerVersion = 3,
    }

    /// <summary>Result of a <see cref="SaveSlotService.Read"/> call.</summary>
    public readonly struct SaveSlotReadResult
    {
        public SaveSlotReadResult(SaveSlotReadStatus status, SaveSlotRecord record)
        {
            Status = status;
            Record = record;
        }

        public SaveSlotReadStatus Status { get; }

        public SaveSlotRecord Record { get; }

        public bool IsSuccess => Status == SaveSlotReadStatus.Success;
    }
}
