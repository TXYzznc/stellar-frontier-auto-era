using System.Collections.Generic;
using AutoEra.World.Identity;
// 燃料只存在于场景声明的设施实例里（结算快照没有它），所以「燃料耗尽」这一类事件必须读设施
// 当前状态。同一程序集内的跨命名空间引用，与 MachineEnergyConsumer 读机器是同一情形。
using AutoEra.World.Region;

namespace AutoEra.Energy
{
    /// <summary>
    /// 能源历史上的一个**离散事件**种类。
    ///
    /// 来源：规格 06「第一版能源界面」——「第一版不制作连续功率曲线。能源历史只记录
    /// 电量过低／耗尽、缺电停机／恢复、燃料耗尽和负载首次超过供给等离散事件。」
    ///
    /// 其中**「电量过低」本批不实现**：文档没有给出「过低」的判据（没有百分比阈值，
    /// 也没有「低于多少分钟续航」的口径），凭空定一个数字会让这条记录变成实现侧的自造规则。
    /// 「电量耗尽」（＝ 0）是无歧义的，所以先做它；「过低」等设计给出判据后再补。
    /// </summary>
    public enum EnergyEventKind
    {
        /// <summary>缺电停机：电网从低优先级开始停止供电。</summary>
        ShortageStopped = 0,

        /// <summary>缺电恢复：之前被停机的对象重新拿到供电。</summary>
        ShortageRecovered = 1,

        /// <summary>电量耗尽：储能从「有电」变成 0。</summary>
        StorageDepleted = 2,

        /// <summary>电量恢复：储能从 0 重新充上电。</summary>
        StorageRecovered = 3,

        /// <summary>燃料耗尽：燃料发电设施的可燃生物质归零。</summary>
        FuelExhausted = 4,

        /// <summary>负载首次超过供给：本次供电缺口没能被任何可停机对象吸收。</summary>
        SupplyExceeded = 5,
    }

    /// <summary>一次结算里新产生的一个离散事件。带足够的现场数值，正文可以自己拼。</summary>
    public readonly struct EnergyDiscreteEvent
    {
        public EnergyDiscreteEvent(EnergyEventKind kind, PersistentId subject, float demand, float supply)
        {
            Kind = kind;
            Subject = subject;
            Demand = demand;
            Supply = supply;
        }

        public EnergyEventKind Kind { get; }

        /// <summary>事件主体：机器身份，或（储能／燃料类事件）设施身份。</summary>
        public PersistentId Subject { get; }

        /// <summary>本时刻的真实负载（功率）；非功率类事件为 0。</summary>
        public float Demand { get; }

        /// <summary>本时刻的可用供给（功率）；非功率类事件为 0。</summary>
        public float Supply { get; }
    }

    /// <summary>
    /// 能源事件的**词汇表**：日志里写的动作名、界面上的标签、以及「原因／影响」两句话。
    ///
    /// 放在一处、并且生产侧与读取侧共用同一批常量，是因为日志记录里没有「事件种类」这个字段
    /// （<see cref="AutoEra.Events.EventJournalRecord"/> 只有 kind／domain／correlation／source／action／时间／序号／终态／结果）。
    /// 读取侧要靠动作名认出这是哪一类能源事件，才能算出「活跃或已恢复」和「持续时长」；
    /// 两边各写一份字面量，改一处就会让配对悄悄失效——那正是这一页最容易出现的假数据。
    /// </summary>
    public static class EnergyEventText
    {
        public const string ShortageStopped = "缺电停机";
        public const string ShortageRecovered = "缺电恢复";
        public const string StorageDepleted = "电量耗尽";
        public const string StorageRecovered = "电量恢复";
        public const string FuelExhausted = "燃料耗尽";
        public const string SupplyExceeded = "负载首次超过供给";

        public static string Label(EnergyEventKind kind)
        {
            switch (kind)
            {
                case EnergyEventKind.ShortageStopped: return ShortageStopped;
                case EnergyEventKind.ShortageRecovered: return ShortageRecovered;
                case EnergyEventKind.StorageDepleted: return StorageDepleted;
                case EnergyEventKind.StorageRecovered: return StorageRecovered;
                case EnergyEventKind.FuelExhausted: return FuelExhausted;
                case EnergyEventKind.SupplyExceeded: return SupplyExceeded;
                default: return kind.ToString();
            }
        }

        /// <summary>把日志里的动作名认回事件种类；不是能源事件时返回 null。</summary>
        public static EnergyEventKind? Parse(string action)
        {
            if (string.IsNullOrEmpty(action)) return null;

            switch (action)
            {
                case ShortageStopped: return EnergyEventKind.ShortageStopped;
                case ShortageRecovered: return EnergyEventKind.ShortageRecovered;
                case StorageDepleted: return EnergyEventKind.StorageDepleted;
                case StorageRecovered: return EnergyEventKind.StorageRecovered;
                case FuelExhausted: return EnergyEventKind.FuelExhausted;
                case SupplyExceeded: return EnergyEventKind.SupplyExceeded;
                default: return null;
            }
        }

        /// <summary>这一条事件是否把某个对象**置于停供状态**（用于配对出「活跃／已恢复」与持续时长）。</summary>
        public static bool IsStop(EnergyEventKind kind) =>
            kind == EnergyEventKind.ShortageStopped || kind == EnergyEventKind.StorageDepleted
            || kind == EnergyEventKind.FuelExhausted || kind == EnergyEventKind.SupplyExceeded;

        /// <summary>这一条事件是否表示**该主体已经恢复**。</summary>
        public static bool IsRecovery(EnergyEventKind kind) =>
            kind == EnergyEventKind.ShortageRecovered || kind == EnergyEventKind.StorageRecovered;

        /// <summary>
        /// 这条停供事件由哪一种恢复事件结案；没有恢复事件（燃料耗尽、供电缺口）时返回 null。
        ///
        /// 配对必须**按种类**而不是只按主体：储能耗尽与供电缺口都没有具体设施主体
        /// （取 Invalid），若只按主体配对，「电量恢复」会把「负载超过供给」也一起结案。
        /// </summary>
        public static EnergyEventKind? RecoveryOf(EnergyEventKind kind)
        {
            switch (kind)
            {
                case EnergyEventKind.ShortageStopped: return EnergyEventKind.ShortageRecovered;
                case EnergyEventKind.StorageDepleted: return EnergyEventKind.StorageRecovered;
                default: return null;
            }
        }

        /// <summary>恢复事件对应的停供事件（<see cref="RecoveryOf"/> 的逆）；不是恢复事件时返回 null。</summary>
        public static EnergyEventKind? StopOf(EnergyEventKind kind)
        {
            switch (kind)
            {
                case EnergyEventKind.ShortageRecovered: return EnergyEventKind.ShortageStopped;
                case EnergyEventKind.StorageRecovered: return EnergyEventKind.StorageDepleted;
                default: return null;
            }
        }

        /// <summary>事件原因：为什么发生。措辞描述领域事实，不是猜玩家的操作。</summary>
        public static string Reason(EnergyEventKind kind)
        {
            switch (kind)
            {
                case EnergyEventKind.ShortageStopped:
                    return "发电与储能加起来覆盖不了当时的负载，电网按供电优先级从最低一档开始停止供电。";
                case EnergyEventKind.ShortageRecovered:
                    return "发电或储能重新足够，之前被停下的对象按供电顺序恢复。";
                case EnergyEventKind.StorageDepleted:
                    return "储能电量降到 0，此后只能靠当时的发电量供电。";
                case EnergyEventKind.StorageRecovered:
                    return "发电出现盈余，储能重新开始充电。";
                case EnergyEventKind.FuelExhausted:
                    return "燃料发电设施的可燃生物质用尽，它不再提供功率。";
                case EnergyEventKind.SupplyExceeded:
                    return "负载超过当时的供给，且没有可继续停机的对象来吸收缺口。";
                default:
                    return "—";
            }
        }

        /// <summary>事件影响：对玩家意味着什么。</summary>
        public static string Impact(EnergyEventKind kind)
        {
            switch (kind)
            {
                case EnergyEventKind.ShortageStopped:
                    return "该对象停止工作，直到供电恢复；已接受的事务不会因为它停机而被取消。";
                case EnergyEventKind.ShortageRecovered:
                    return "该对象重新工作；记录恢复**不会**重新启动它，是否继续由它自己的状态决定。";
                case EnergyEventKind.StorageDepleted:
                    return "此后夜间或负载高峰没有缓冲，缺口会直接变成停机。";
                case EnergyEventKind.StorageRecovered:
                    return "缓冲重新建立，缺口会先由储能吸收。";
                case EnergyEventKind.FuelExhausted:
                    return "需要补充燃料才能恢复发电；太阳能与储能不受影响。";
                case EnergyEventKind.SupplyExceeded:
                    return "负载无法被完全满足：应当扩容发电、储能，或降低同时运行的设备数量。";
                default:
                    return "—";
            }
        }
    }

    /// <summary>
    /// 离散能源事件的识别：把「连续结算」变成「状态跨越」。
    ///
    /// 这一层刻意是**纯领域对象**（不依赖 Unity、不写日志）：结算每帧都在发生，而历史记录
    /// 只在状态跨越时出现一次。把它塞进电网里会让「结算」这个每帧路径带上记账职责；
    /// 放在外面，它就能在 EditMode 里被逐帧推演地验证——包括最要紧的那条不变式：
    /// **同一次停机只记一条，恢复之前不再重复记**。
    ///
    /// 用法：每次结算之后调用 <see cref="Capture"/>，把新事件追加到调用方给的列表里。
    /// 它内部只保留「上一次是什么状态」，因此内存恒定，也不会每帧分配。
    /// </summary>
    public sealed class EnergyEventRecorder
    {
        private readonly HashSet<PersistentId> _stopped = new HashSet<PersistentId>();
        private readonly HashSet<PersistentId> _stoppedNow = new HashSet<PersistentId>();
        private readonly HashSet<PersistentId> _dryFuel = new HashSet<PersistentId>();

        /// <summary>回收缓冲：每次 Capture 都要算「这一帧恢复了谁」，但不该每次都新建集合。</summary>
        private readonly HashSet<PersistentId> _recovered = new HashSet<PersistentId>();

        /// <summary>
        /// 储能的「上一次有没有电」。**必须单独用 <see cref="bool"/> 而不是从快照推断**：
        /// 第一次结算时没有「上一次」，若把初始状态当成「刚耗尽」，打开游戏的第一帧就会记一条
        /// 假的「电量耗尽」。
        /// </summary>
        private bool _storageHadCharge;
        private bool _storageKnown;

        private bool _wasShort;

        /// <summary>
        /// 识别本次结算新产生的离散事件，按发生顺序追加到 <paramref name="into"/>（不清空它）。
        ///
        /// <paramref name="facilities"/> 用于燃料耗尽：燃料只存在于设施实例里，快照里没有它。
        /// </summary>
        public void Capture(EnergyGridSnapshot snapshot, IReadOnlyList<RegionEnergyFacility> facilities,
            List<EnergyDiscreteEvent> into)
        {
            if (into == null) return;

            CaptureStops(snapshot, into);
            CaptureStorage(snapshot, facilities, into);
            CaptureFuel(facilities, into);
            CaptureShortfall(snapshot, into);
        }

        /// <summary>清空「上一次的状态」；区域重建后调用，避免跨场景沿用旧状态。</summary>
        public void Reset()
        {
            _stopped.Clear();
            _stoppedNow.Clear();
            _dryFuel.Clear();
            _storageHadCharge = false;
            _storageKnown = false;
            _wasShort = false;
        }

        /// <summary>缺电停机／恢复：以「被停机集合」的进出为判据，与停机顺序无关。</summary>
        private void CaptureStops(EnergyGridSnapshot snapshot, List<EnergyDiscreteEvent> into)
        {
            _stoppedNow.Clear();
            IReadOnlyList<PersistentId> stopped = snapshot.StoppedByShortage;
            for (int i = 0; i < stopped.Count; i++)
            {
                _stoppedNow.Add(stopped[i]);
                if (_stopped.Add(stopped[i]))
                {
                    into.Add(new EnergyDiscreteEvent(EnergyEventKind.ShortageStopped, stopped[i],
                        snapshot.ConsumedPower, snapshot.GeneratedPower));
                }
            }

            // 恢复同样按集合差集处理：这一帧不在停机集合里、上一帧在，就是恢复了。
            if (_stopped.Count > _stoppedNow.Count)
            {
                _recovered.Clear();
                foreach (PersistentId id in _stopped)
                {
                    if (!_stoppedNow.Contains(id)) _recovered.Add(id);
                }

                foreach (PersistentId id in _recovered)
                {
                    _stopped.Remove(id);
                    into.Add(new EnergyDiscreteEvent(EnergyEventKind.ShortageRecovered, id,
                        snapshot.ConsumedPower, snapshot.GeneratedPower));
                }
            }
        }

        /// <summary>电量耗尽／恢复：0 与「非 0」之间的跨越。没有储能设施的区域不记这类事件。</summary>
        private void CaptureStorage(EnergyGridSnapshot snapshot, IReadOnlyList<RegionEnergyFacility> facilities,
            List<EnergyDiscreteEvent> into)
        {
            if (snapshot.StorageCapacity <= 0f)
            {
                // 没有储能设施：这一类事件不适用于本区域，也不该把「一直为 0」当成耗尽。
                _storageKnown = false;
                return;
            }

            bool hasCharge = snapshot.StoredCharge > 0f;
            if (_storageKnown && hasCharge != _storageHadCharge)
            {
                into.Add(new EnergyDiscreteEvent(
                    hasCharge ? EnergyEventKind.StorageRecovered : EnergyEventKind.StorageDepleted,
                    StorageSubject(facilities), snapshot.StoredCharge, snapshot.StorageCapacity));
            }

            _storageHadCharge = hasCharge;
            _storageKnown = true;
        }

        /// <summary>
        /// 电量事件的「对象」：第一版一个区域只有一台蓄电池，所以就取它；
        /// 多台时取不到单一主体，返回 Invalid 让界面如实说「本区域储能（合计）」。
        /// </summary>
        private static PersistentId StorageSubject(IReadOnlyList<RegionEnergyFacility> facilities)
        {
            if (facilities == null) return PersistentId.Invalid;

            PersistentId found = PersistentId.Invalid;
            for (int i = 0; i < facilities.Count; i++)
            {
                if (facilities[i] == null || facilities[i].Storage == null) continue;
                if (found.IsValid) return PersistentId.Invalid; // 不止一台：没有单一主体
                found = facilities[i].ObjectId;
            }

            return found;
        }

        /// <summary>燃料耗尽：每台燃料设施各自只在「从有到无」的那一次记一条。补上燃料后重新武装。</summary>
        private void CaptureFuel(IReadOnlyList<RegionEnergyFacility> facilities, List<EnergyDiscreteEvent> into)
        {
            if (facilities == null) return;

            for (int i = 0; i < facilities.Count; i++)
            {
                RegionEnergyFacility facility = facilities[i];
                if (facility == null || facility.Generator == null) continue;
                if (facility.Generator.Kind != GeneratorKind.Fuel) continue;

                if (facility.RemainingBiomass > 0f)
                {
                    _dryFuel.Remove(facility.ObjectId);
                    continue;
                }

                if (_dryFuel.Add(facility.ObjectId))
                {
                    into.Add(new EnergyDiscreteEvent(EnergyEventKind.FuelExhausted, facility.ObjectId, 0f, 0f));
                }
            }
        }

        /// <summary>
        /// 负载超过供给：以「缺口没能被任何可停机对象吸收」（<see cref="EnergyGridSnapshot.HasShortfall"/>）
        /// 为判据，只在由假变真的那一次记一条。
        ///
        /// 为什么不用「负载 &gt; 发电」：那在夜里只要储能补上就会天天发生，记成事件等于把
        /// 正常供电说成故障。规格要的是「负载首次超过供给」，即**供给真的不够**这一次。
        /// </summary>
        private void CaptureShortfall(EnergyGridSnapshot snapshot, List<EnergyDiscreteEvent> into)
        {
            if (snapshot.HasShortfall && !_wasShort)
            {
                into.Add(new EnergyDiscreteEvent(EnergyEventKind.SupplyExceeded, PersistentId.Invalid,
                    snapshot.ConsumedPower, snapshot.GeneratedPower));
            }

            _wasShort = snapshot.HasShortfall;
        }
    }
}
