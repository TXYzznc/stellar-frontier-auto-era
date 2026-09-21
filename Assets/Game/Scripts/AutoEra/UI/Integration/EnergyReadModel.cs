using System;
using System.Collections.Generic;
using System.Globalization;
using AutoEra.Energy;
using AutoEra.Machines;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.UI
{
    /// <summary>能源页里的一台设施（发电或蓄电）。</summary>
    public readonly struct UiEnergyFacilityRow
    {
        public UiEnergyFacilityRow(int index, PersistentId id, string name, string kind, string state, string detail,
            bool supportsChargingPolicy, bool chargingAllowed, float chargeTargetRatio)
        {
            Index = index;
            Id = id;
            Name = name;
            Kind = kind;
            State = state;
            Detail = detail;
            SupportsChargingPolicy = supportsChargingPolicy;
            ChargingAllowed = chargingAllowed;
            ChargeTargetRatio = chargeTargetRatio;
        }

        public int Index { get; }
        public PersistentId Id { get; }

        /// <summary>设施名（场景对象名，拿不到时退化成设施类别，不编造）。</summary>
        public string Name { get; }

        /// <summary>设施类别：环境能源／燃料发电／蓄电。</summary>
        public string Kind { get; }

        /// <summary>本时刻状态：运行／待命／充电／放电／待机。</summary>
        public string State { get; }

        /// <summary>一行明细：额定／实际功率、燃料与维持时间、电量与容量。</summary>
        public string Detail { get; }

        /// <summary>是否开放充电许可与目标储电比例（规格：仅燃料设施开放）。</summary>
        public bool SupportsChargingPolicy { get; }

        public bool ChargingAllowed { get; }
        public float ChargeTargetRatio { get; }
    }

    /// <summary>能源页里的一个用电对象（第一版只可能是机器）。</summary>
    public readonly struct UiEnergyConsumerRow
    {
        public UiEnergyConsumerRow(PersistentId id, string name, string group, string state, float power,
            string priority, bool stoppedByShortage)
        {
            Id = id;
            Name = name;
            Group = group;
            State = state;
            Power = power;
            Priority = priority;
            StoppedByShortage = stoppedByShortage;
        }

        public PersistentId Id { get; }
        public string Name { get; }

        /// <summary>分组：机器／建筑（第一版只有机器有运行时）。</summary>
        public string Group { get; }

        public string State { get; }
        public float Power { get; }
        public string Priority { get; }
        public bool StoppedByShortage { get; }
    }

    /// <summary>能源分页的只读快照。</summary>
    public readonly struct EnergyDomainSnapshot
    {
        public EnergyDomainSnapshot(UiDataState state, string reason,
            IReadOnlyList<UiDetailField> summary, IReadOnlyList<UiEnergyFacilityRow> facilities,
            IReadOnlyList<UiEnergyConsumerRow> consumers, int selectedFacility)
        {
            State = state;
            Reason = reason;
            Summary = summary ?? Array.Empty<UiDetailField>();
            Facilities = facilities ?? Array.Empty<UiEnergyFacilityRow>();
            Consumers = consumers ?? Array.Empty<UiEnergyConsumerRow>();
            SelectedFacility = selectedFacility;
        }

        public UiDataState State { get; }
        public string Reason { get; }
        public IReadOnlyList<UiDetailField> Summary { get; }
        public IReadOnlyList<UiEnergyFacilityRow> Facilities { get; }
        public IReadOnlyList<UiEnergyConsumerRow> Consumers { get; }

        /// <summary>当前选中的设施在列表里的序号；没有选中时为 -1。</summary>
        public int SelectedFacility { get; }

        public bool HasSelection => SelectedFacility >= 0 && SelectedFacility < Facilities.Count;

        /// <summary>选中的设施；没有选中时是一个默认行（调用方先用 <see cref="HasSelection"/> 判）。</summary>
        public UiEnergyFacilityRow Selected =>
            HasSelection ? Facilities[SelectedFacility] : default;

        public static EnergyDomainSnapshot Unavailable(string reason) =>
            new EnergyDomainSnapshot(UiDataState.Unavailable, reason, null, null, null, -1);
    }

    /// <summary>
    /// 能源分页的读取与写入边界。
    ///
    /// 写入只有一件事——**燃料设施的充电许可与目标储电比例**（规格 06「第一版能源界面」：
    /// 「燃料发电设施可以设置是否允许为蓄电池充电及目标储电比例；免费环境能源不显示燃料设置」）。
    /// 发电站开关属于现场操作，本页不提供。
    /// </summary>
    public interface IEnergyReadModel : IDisposable
    {
        EnergyDomainSnapshot Snapshot { get; }

        /// <summary>按区域通知变化；能源没有领域推送（结算是按节拍发生的），由界面按需刷新。</summary>
        event Action Changed;

        void Refresh();

        /// <summary>选中一台设施；越界返回 false。</summary>
        bool Select(int facilityIndex);

        void ClearSelection();

        /// <summary>设置选中燃料设施的充电许可；不可配置或没有选中时返回 false。</summary>
        bool SetChargingAllowed(bool allowed, out string reason);

        /// <summary>设置选中燃料设施的目标储电比例（0..1）；不可配置或没有选中时返回 false。</summary>
        bool SetChargeTargetRatio(float ratio, out string reason);
    }

    /// <summary>没有电网时的诚实空实现：只报 Unavailable，不伪造供需数据。</summary>
    internal sealed class UnavailableEnergyReadModel : IEnergyReadModel
    {
        public UnavailableEnergyReadModel(string reason)
        {
            Snapshot = EnergyDomainSnapshot.Unavailable(reason);
        }

        public EnergyDomainSnapshot Snapshot { get; }

        public event Action Changed
        {
            add { }
            remove { }
        }

        public void Refresh() { }
        public bool Select(int facilityIndex) => false;
        public void ClearSelection() { }
        public bool SetChargingAllowed(bool allowed, out string reason) { reason = Snapshot.Reason; return false; }
        public bool SetChargeTargetRatio(float ratio, out string reason) { reason = Snapshot.Reason; return false; }
        public void Dispose() { }
    }

    /// <summary>基于区域电网与机器花名册的只读实现（写入口只有燃料充电策略）。</summary>
    internal sealed class RegionEnergyReadModel : IEnergyReadModel
    {
        private readonly RegionEnergyService _energy;
        private readonly MachineRoster _machines;
        private readonly List<UiDetailField> _summary = new List<UiDetailField>(8);
        private readonly List<UiEnergyFacilityRow> _facilities = new List<UiEnergyFacilityRow>(4);
        private readonly List<UiEnergyConsumerRow> _consumers = new List<UiEnergyConsumerRow>(16);
        private EnergyDomainSnapshot _snapshot;
        private int _selected = -1;
        private bool _disposed;

        public RegionEnergyReadModel(RegionEnergyService energy, MachineRoster machines)
        {
            _energy = energy ?? throw new ArgumentNullException(nameof(energy));
            _machines = machines;
            Refresh();
        }

        public EnergyDomainSnapshot Snapshot => _snapshot;

        public event Action Changed;

        public void Refresh()
        {
            if (_disposed) return;
            if (!_energy.HasSupply)
            {
                // 区域里没有声明任何能源设施：这是可展示的事实，不是错误。
                _snapshot = EnergyDomainSnapshot.Unavailable(NoFacilitiesReason);
                Changed?.Invoke();
                return;
            }

            BuildSummary();
            BuildFacilities();
            BuildConsumers();
            if (_selected >= _facilities.Count) _selected = -1;

            _snapshot = new EnergyDomainSnapshot(UiDataState.Ready, null,
                _summary.ToArray(), _facilities.ToArray(), _consumers.ToArray(), _selected);
            Changed?.Invoke();
        }

        public bool Select(int facilityIndex)
        {
            if (_disposed || facilityIndex < 0 || facilityIndex >= _facilities.Count) return false;
            if (_selected == facilityIndex) return true;
            _selected = facilityIndex;
            Refresh();
            return true;
        }

        public void ClearSelection()
        {
            if (_disposed || _selected < 0) return;
            _selected = -1;
            Refresh();
        }

        public bool SetChargingAllowed(bool allowed, out string reason)
        {
            reason = null;
            RegionEnergyFacility facility = SelectedConfigurable(out reason);
            if (facility == null) return false;
            facility.Generator.AllowsCharging = allowed;
            Refresh();
            return true;
        }

        public bool SetChargeTargetRatio(float ratio, out string reason)
        {
            reason = null;
            RegionEnergyFacility facility = SelectedConfigurable(out reason);
            if (facility == null) return false;
            if (float.IsNaN(ratio) || float.IsInfinity(ratio))
            {
                reason = "目标储电比例必须是一个有效数值。";
                return false;
            }

            facility.Generator.ChargeTargetRatio = ratio < 0f ? 0f : ratio > 1f ? 1f : ratio;
            Refresh();
            return true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Changed = null;
            _summary.Clear();
            _facilities.Clear();
            _consumers.Clear();
        }

        public const string NoFacilitiesReason =
            "本区域没有声明任何能源设施：因此这里没有可展示的发电、储能与供电状态。"
            + "设施的建造与部署属于尚未接入的内容。";

        /// <summary>取选中的、且开放充电策略的燃料设施；不满足时给出可展示的原因。</summary>
        private RegionEnergyFacility SelectedConfigurable(out string reason)
        {
            reason = null;
            if (!_snapshot.HasSelection && _selected < 0)
            {
                reason = "先在设施列表里选中一台发电设施。";
                return null;
            }

            if (_selected < 0 || _selected >= _energy.Facilities.Count)
            {
                reason = "选中的设施已经不在列表里了。";
                return null;
            }

            RegionEnergyFacility facility = _energy.Facilities[_selected];
            if (!facility.SupportsChargingPolicy)
            {
                reason = "只有燃料发电设施开放充电许可与目标储电比例；环境能源与储能设施没有这项设置。";
                return null;
            }

            return facility;
        }

        /// <summary>
        /// 供需概要（规格 06「第一版能源界面」顶部那一排）。
        /// 估算时间**必须标注**会随负载、昼夜和设施状态变化——规格原文如此。
        /// </summary>
        private void BuildSummary()
        {
            _summary.Clear();
            EnergyGridSnapshot snapshot = _energy.Snapshot;

            _summary.Add(new UiDetailField("当前发电", Power(snapshot.GeneratedPower)));
            _summary.Add(new UiDetailField("当前用电", Power(snapshot.ConsumedPower)));
            _summary.Add(new UiDetailField("净功率", SignedPower(snapshot.NetPower)));
            _summary.Add(new UiDetailField("当前储电",
                snapshot.StorageCapacity > 0f
                    ? Charge(snapshot.StoredCharge) + " ／ " + Charge(snapshot.StorageCapacity)
                    : "本区域没有蓄电设施"));
            _summary.Add(new UiDetailField("充满／耗尽估算", Estimate(snapshot)));
            _summary.Add(new UiDetailField("缺电停机设备", snapshot.StoppedByShortage.Count.ToString(CultureInfo.InvariantCulture) + " 台"));
            if (snapshot.DiscardedPower > 0f)
            {
                _summary.Add(new UiDetailField("舍弃的盈余", Power(snapshot.DiscardedPower) + "（储能已满，环境能源盈余直接舍弃）"));
            }

            _summary.Add(new UiDetailField("日照",
                _energy.LastDaylight
                    ? "有日照（环境能源按可用出力发电）"
                    : "无日照（环境能源出力为 0，缺口由燃料发电或储能补足）"));
        }

        private static string Estimate(EnergyGridSnapshot snapshot)
        {
            float? runtime = snapshot.EstimatedRuntimeSeconds;
            if (runtime.HasValue)
            {
                return "约 " + Duration(runtime.Value) + "后耗尽（按当前净功率估算，会随负载、昼夜和设施状态变化）";
            }

            float? full = snapshot.EstimatedFullSeconds;
            if (full.HasValue)
            {
                return "约 " + Duration(full.Value) + "后充满（按当前净功率估算，会随负载、昼夜和设施状态变化）";
            }

            return snapshot.StorageCapacity > 0f
                ? "净功率为 0，储能既不满也不空，没有可估算的充满或耗尽时间。"
                : "本区域没有蓄电设施，因此在净功率为负时会直接按优先级停机。";
        }

        /// <summary>发电与蓄电设施（规格：名称、额定／实际功率、燃料与维持时间、充放电状态）。</summary>
        private void BuildFacilities()
        {
            _facilities.Clear();
            IReadOnlyList<RegionEnergyFacility> facilities = _energy.Facilities;
            for (int i = 0; i < facilities.Count; i++)
            {
                RegionEnergyFacility facility = facilities[i];
                if (facility.Storage != null)
                {
                    IEnergyStorage storage = facility.Storage;
                    _facilities.Add(new UiEnergyFacilityRow(i, facility.ObjectId,
                        Name(facility, "蓄电设施"), "蓄电", StorageStateLabel(storage.State),
                        Charge(storage.Charge) + " ／ " + Charge(storage.Capacity)
                        + "（" + Percent(storage.Capacity <= 0f ? 0f : storage.Charge / storage.Capacity) + "）"
                        + "　当前功率 " + SignedPower(storage.ActualPower),
                        supportsChargingPolicy: false, chargingAllowed: false, chargeTargetRatio: 0f));
                    continue;
                }

                IEnergyGenerator generator = facility.Generator;
                if (generator == null) continue;
                bool isFuel = generator.Kind == GeneratorKind.Fuel;
                _facilities.Add(new UiEnergyFacilityRow(i, facility.ObjectId,
                    Name(facility, isFuel ? "燃料发电设施" : "环境能源设施"),
                    isFuel ? "燃料发电" : "环境能源",
                    isFuel ? (generator.IsOn && generator.ActualOutputPower > 0f ? "运行" : "待命") : (generator.EnvironmentPower > 0f ? "发电" : "无日照"),
                    "额定 " + Power(generator.RatedPower) + "　实际 " + Power(generator.ActualOutputPower)
                    + (isFuel
                        ? "　燃料 " + Biomass(facility.RemainingBiomass) + "（约 " + FuelRuntime(generator, facility) + "）"
                        : string.Empty),
                    generator.Kind == GeneratorKind.Fuel,
                    generator.AllowsCharging,
                    generator.ChargeTargetRatio));
            }
        }

        /// <summary>
        /// 用电对象：按机器与建筑分组（规格）。
        /// 第一版只有机器有运行时与耗电模型——建筑还没有，所以这里**只列机器**，
        /// 并由正文说明分组情况，而不是编一组空建筑出来。
        /// </summary>
        private void BuildConsumers()
        {
            _consumers.Clear();
            if (_machines == null) return;

            foreach (MachineInstance machine in _machines.Machines)
            {
                if (!machine.Deployed) continue;
                bool tracked = _energy.TryGetConsumer(machine.Id, out MachineEnergyConsumer consumer);
                _consumers.Add(new UiEnergyConsumerRow(machine.Id, machine.Name, "机器",
                    State(machine, consumer, tracked),
                    tracked ? consumer.ActualPower : 0f,
                    PriorityLabel(RegionEnergyService.DefaultMachinePriority),
                    tracked && consumer.IsStoppedByShortage));
            }
        }

        private static string State(MachineInstance machine, MachineEnergyConsumer consumer, bool tracked)
        {
            if (tracked && consumer.IsStoppedByShortage) return "因缺电停机";
            if (!machine.SupplyAvailable) return "未供电";
            return machine.IsMoving ? "移动中" : "已供电";
        }

        private static string Name(RegionEnergyFacility facility, string fallback) =>
            string.IsNullOrWhiteSpace(facility.DisplayName) ? fallback : facility.DisplayName;

        private static string FuelRuntime(IEnergyGenerator generator, RegionEnergyFacility facility)
        {
            float output = generator.ActualOutputPower;
            if (output <= 0f)
            {
                return facility.RemainingBiomass > 0f ? "当前无消耗" : "燃料已耗尽";
            }

            float minutes = FirstVersionEnergy.EnergyToBiomass(facility.RemainingBiomass * 60f / output);
            return "可维持约 " + minutes.ToString("0.#", CultureInfo.InvariantCulture) + " 分钟";
        }

        private static string StorageStateLabel(StorageState state) => state switch
        {
            StorageState.Charging => "充电",
            StorageState.Discharging => "放电",
            _ => "待机",
        };

        private static string PriorityLabel(PowerPriority priority) => priority switch
        {
            PowerPriority.Critical => "关键设备",
            PowerPriority.Production => "普通生产",
            PowerPriority.Secondary => "次要生产",
            _ => "可暂停设备",
        };

        private static string Power(float value) =>
            value.ToString("0.#", CultureInfo.InvariantCulture) + " 功率";

        private static string SignedPower(float value) =>
            (value >= 0f ? "+" : string.Empty) + value.ToString("0.#", CultureInfo.InvariantCulture) + " 功率";

        private static string Charge(float value) =>
            value.ToString("0.#", CultureInfo.InvariantCulture) + " 电量";

        private static string Biomass(float value) =>
            value.ToString("0.##", CultureInfo.InvariantCulture) + " 生物质";

        private static string Percent(float ratio) =>
            (ratio * 100f).ToString("0", CultureInfo.InvariantCulture) + "%";

        private static string Duration(float seconds)
        {
            if (seconds < 60f) return seconds.ToString("0", CultureInfo.InvariantCulture) + " 秒";
            return (seconds / 60f).ToString("0.#", CultureInfo.InvariantCulture) + " 分钟";
        }
    }

    /// <summary>能源页的入口：从打开参数里的会话取区域电网。</summary>
    public static class EnergyReadModels
    {
        /// <summary>建立读模型。会话缺失、没有世界或区域没有电网时返回诚实空实现。</summary>
        public static IEnergyReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableEnergyReadModel("没有界面会话：本页无法读取区域电网。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableEnergyReadModel("当前不在世界里：能源供需属于区域，世界外没有可读的电网。");
            }

            if (session.RegionEnergy == null)
            {
                return new UnavailableEnergyReadModel(RegionEnergyReadModel.NoFacilitiesReason);
            }

            return new RegionEnergyReadModel(session.RegionEnergy, session.World.Machines);
        }
    }
}
