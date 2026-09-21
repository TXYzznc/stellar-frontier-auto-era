using AutoEra.World.Identity;

namespace AutoEra.Alerts
{
    /// <summary>警报等级（规格 15-警报列表与详情：提醒／警告／严重）。数值可比大小，中枢要取「最高等级」。</summary>
    public enum AlertSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2,
    }

    /// <summary>警报状态：活跃或已恢复（恢复后转历史，规格原文）。</summary>
    public enum AlertState
    {
        Active = 0,
        Recovered = 1,
    }

    /// <summary>
    /// 第一版真正有判据的警报种类。
    ///
    /// 规格 01-机器自动化与算法 要求「红色算法异常、队列溢出和完全损坏等重要问题进入独立警报历史」，
    /// 04-第一版 要求能源侧「缺电停机与恢复、警报」。但**只有已经存在的领域事实才能报警**，
    /// 所以第一版只做四类能直接读出真值、并且能说出「真实恢复条件」的问题：
    /// 缺电停机、电量耗尽、燃料耗尽、机器完全损坏。
    ///
    /// **未做且理由**：队列溢出（需要先有队列容量口径）、算法异常（需要算法运行结果进日志的稳定判据）。
    /// 这两类等各自的判据落定后再加——凭一个猜测出来的阈值报警，等于让玩家为一条不存在的问题跑一趟。
    /// </summary>
    public enum AlertKind
    {
        /// <summary>缺电停机：来源是被停的机器。</summary>
        EnergyShortage = 0,

        /// <summary>电量耗尽：区域储能降到 0，来源是那台蓄电设施（多台时取区域口径）。</summary>
        StorageDepleted = 1,

        /// <summary>燃料耗尽：燃料发电设施可燃生物质用尽，来源是那台设施。</summary>
        FuelExhausted = 2,

        /// <summary>机器完全损坏：完整度为 0，来源是那台机器。</summary>
        MachineDestroyed = 3,
    }

    /// <summary>
    /// 警报的**文案与元数据唯一来源**。
    ///
    /// 「真实恢复条件」是规格明确要求的字段，也是这个系统唯一诚实的部分：警报不能有「清除」按钮，
    /// 玩家能做的是解决问题本身。所以每一类警报都必须写得出「什么条件成立它才会自己消失」，
    /// 写不出来的问题就不该被做成警报。
    /// </summary>
    public static class AlertCatalog
    {
        public static string Label(AlertKind kind)
        {
            switch (kind)
            {
                case AlertKind.EnergyShortage: return "缺电停机";
                case AlertKind.StorageDepleted: return "电量耗尽";
                case AlertKind.FuelExhausted: return "燃料耗尽";
                case AlertKind.MachineDestroyed: return "机器完全损坏";
                default: return kind.ToString();
            }
        }

        public static AlertSeverity Severity(AlertKind kind)
        {
            switch (kind)
            {
                case AlertKind.EnergyShortage: return AlertSeverity.Warning;
                case AlertKind.StorageDepleted: return AlertSeverity.Warning;
                case AlertKind.FuelExhausted: return AlertSeverity.Warning;
                case AlertKind.MachineDestroyed: return AlertSeverity.Critical;
                default: return AlertSeverity.Info;
            }
        }

        /// <summary>来源与目标：来源是出问题的对象，目标是它影响到的业务范围。</summary>
        public static string Target(AlertKind kind)
        {
            switch (kind)
            {
                case AlertKind.EnergyShortage: return "该机器 · 所在区域电网";
                case AlertKind.StorageDepleted: return "所在区域电网 · 夜间与负载高峰供电";
                case AlertKind.FuelExhausted: return "所在区域电网 · 依赖燃料发电的负载";
                case AlertKind.MachineDestroyed: return "该机器 · 它承担的任务与算法";
                default: return "—";
            }
        }

        public static string Reason(AlertKind kind)
        {
            switch (kind)
            {
                case AlertKind.EnergyShortage:
                    return "发电与储能加起来覆盖不了当时的负载，电网按供电优先级把该对象停下来了。";
                case AlertKind.StorageDepleted:
                    return "区域储能电量降到 0，此后只能靠当时的发电量供电。";
                case AlertKind.FuelExhausted:
                    return "燃料发电设施的可燃生物质用尽，它不再提供功率。";
                case AlertKind.MachineDestroyed:
                    return "机器完整度降到 0：它不再工作，也不再耗电。";
                default:
                    return "—";
            }
        }

        public static string Impact(AlertKind kind)
        {
            switch (kind)
            {
                case AlertKind.EnergyShortage:
                    return "该对象停止工作；已接受的事务不会因为它停机而被取消。";
                case AlertKind.StorageDepleted:
                    return "夜间或负载高峰没有缓冲，缺口会直接变成停机。";
                case AlertKind.FuelExhausted:
                    return "需要补充燃料才能恢复发电；太阳能与储能不受影响。";
                case AlertKind.MachineDestroyed:
                    return "它在区域里的任务、算法运行与搬运都会中断，直到修复。";
                default:
                    return "—";
            }
        }

        /// <summary>
        /// 真实恢复条件：**只有它成立，警报才会自己转成已恢复**。
        /// 这一段是给玩家看的行动指引，也是实现侧的状态机定义——两边必须说的是同一件事。
        /// </summary>
        public static string RecoveryCondition(AlertKind kind)
        {
            switch (kind)
            {
                case AlertKind.EnergyShortage:
                    return "发电或储能重新足够，该对象重新拿到供电。";
                case AlertKind.StorageDepleted:
                    return "发电出现盈余，区域储能重新充上电。";
                case AlertKind.FuelExhausted:
                    return "该设施重新有燃料（补充燃料流程接入后生效）。";
                case AlertKind.MachineDestroyed:
                    return "机器完整度恢复到 0 以上。";
                default:
                    return "—";
            }
        }
    }

    /// <summary>
    /// 一条警报的不可变视图。
    ///
    /// 账本内部按序号保存这些结构体，状态变化时**整体替换**那一条（而不是原地改字段）：
    /// 界面拿到的快照必须是自洽的一份，不能出现「状态已经恢复、但次数还是旧的」这种半截状态。
    /// </summary>
    public readonly struct AlertEntry
    {
        public AlertEntry(int id, AlertKind kind, PersistentId source, AlertSeverity severity, AlertState state,
            int count, long firstMilliseconds, long lastMilliseconds, long recoveredMilliseconds, bool read)
        {
            Id = id;
            Kind = kind;
            Source = source;
            Severity = severity;
            State = state;
            Count = count;
            FirstMilliseconds = firstMilliseconds;
            LastMilliseconds = lastMilliseconds;
            RecoveredMilliseconds = recoveredMilliseconds;
            Read = read;
        }

        /// <summary>稳定标识：用于选中、标记已读与界面上的行身份。</summary>
        public int Id { get; }

        public AlertKind Kind { get; }
        public PersistentId Source { get; }
        public AlertSeverity Severity { get; }
        public AlertState State { get; }

        /// <summary>发生次数：同类型同来源在**同一段活跃期内**合并计数（规格：同类型、来源与目标合并）。</summary>
        public int Count { get; }

        public long FirstMilliseconds { get; }
        public long LastMilliseconds { get; }

        /// <summary>恢复时间；仍活跃时为 0。</summary>
        public long RecoveredMilliseconds { get; }

        /// <summary>已读／未读。标记已读**只改变阅读状态，不解决问题**（规格原文）。</summary>
        public bool Read { get; }

        public bool IsActive => State == AlertState.Active;
    }
}
