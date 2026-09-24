namespace AutoEra.Events
{
    /// <summary>Accountable project domains. Producers attach facts to exactly one domain.</summary>
    public enum EventDomain
    {
        Task = 0,
        Algorithm = 1,
        Effector = 2,
        Resource = 3,
        Alert = 4,

        /// <summary>
        /// 能源域：电网自己产生的**离散**事件（缺电停机／恢复、电量耗尽／恢复、燃料耗尽、
        /// 负载首次超过供给）。规格 06「第一版能源界面」写明第一版不做连续功率曲线、
        /// 能源历史只记离散事件，所以这里记的是「状态跨越」，不是功率采样。
        ///
        /// 追加在末尾（= 5）而不是插进中间：这些取值会写进日志记录并可能落盘，
        /// 改动既有取值会让旧存档里的记录改变含义。
        /// </summary>
        Energy = 5,
    }

    /// <summary>Journal record kind. The event bus only ever receives Fact records.</summary>
    public enum EventKind
    {
        Command = 0,
        Fact = 1,
    }

    /// <summary>Final result of a trigger. Only terminal facts set a value other than None.</summary>
    public enum EventOutcome
    {
        None = 0,
        Succeeded = 1,
        Failed = 2,
        Cancelled = 3,
    }
}