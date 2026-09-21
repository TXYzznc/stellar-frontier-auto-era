using System;
using AutoEra.World.Identity;

namespace AutoEra.Energy
{
    /// <summary>
    /// 第一版能源设施的**设计数值**（规格 06「第一版能源设施基准」与 13-统一数值模型）。
    ///
    /// 单独放一处、且只有这一份：设施参数一旦在两个地方各写一份，
    /// 就会走成「文档说 15、代码说 20」这种没人能一眼发现的偏差。
    /// 数值允许小数（能源不受经济面额必须为整数的约束），界面功率最多显示一位小数。
    /// </summary>
    public static class FirstVersionEnergy
    {
        /// <summary>初始太阳能发电器：白天 15 功率，夜间 0。不可建造、拆除、出售或损坏。</summary>
        public const float SolarDaylightPower = 15f;

        /// <summary>基础生物质发电机额定功率。</summary>
        public const float BiomassGeneratorRatedPower = 60f;

        /// <summary>基础蓄电池容量。</summary>
        public const float BatteryCapacity = 240f;

        /// <summary>1 生物质 ＝ 60 电量。</summary>
        public const float EnergyPerBiomass = 60f;

        /// <summary>新存档初始生物质（对应 600 电量，保证基础引导不被燃料短缺打断）。</summary>
        public const int StartupBiomass = 10;

        /// <summary>把生物质数量折算成电量。</summary>
        public static float BiomassToEnergy(float biomass) => biomass * EnergyPerBiomass;

        /// <summary>把电量折算成生物质数量（发电机保存已投入未消耗的燃料，所以这个换算是双向的）。</summary>
        public static float EnergyToBiomass(float energy) => energy / EnergyPerBiomass;
    }

    /// <summary>
    /// 免费环境能源（第一版的初始太阳能发电器）。
    ///
    /// 它没有燃料、也没有输出功率设置：开启时按环境提供全部可用功率，盈余优先充电、
    /// 储满后舍弃。定位是**能源死锁恢复保障**——玩家没有金币和生物质时仍能等白天恢复生产，
    /// 所以它默认开启，且第一版不能拆除或损坏。
    /// </summary>
    public sealed class EnvironmentGenerator : IEnergyGenerator
    {
        private readonly float _ratedPower;

        public EnvironmentGenerator(PersistentId id, float ratedPower)
        {
            if (ratedPower < 0f) throw new ArgumentOutOfRangeException(nameof(ratedPower));
            Id = id;
            _ratedPower = ratedPower;
        }

        public PersistentId Id { get; }

        public GeneratorKind Kind => GeneratorKind.Environment;

        public float RatedPower => _ratedPower;

        public bool IsOn { get; set; } = true;

        /// <summary>
        /// 当前可用出力。白天等于额定功率，夜间为 0——
        /// 由 <see cref="DaylightCycle"/> 决定，设施自己不看时钟。
        /// </summary>
        public float EnvironmentPower { get; set; }

        public float FuelEnergyAvailable
        {
            get => 0f;
            set { }
        }

        public bool AllowsCharging
        {
            get => false;
            set { }
        }

        public float ChargeTargetRatio
        {
            get => 0f;
            set { }
        }

        public float ActualOutputPower { get; set; }

        public float FuelEnergyConsumed { get; set; }

        /// <summary>按世界时间更新可用出力；返回本次是否处于日照。</summary>
        public bool UpdateEnvironment(long worldMilliseconds)
        {
            bool daylight = DaylightCycle.IsDaylight(worldMilliseconds);
            EnvironmentPower = daylight ? _ratedPower : 0f;
            return daylight;
        }
    }

    /// <summary>
    /// 燃料发电站（第一版的基础生物质发电机）。
    ///
    /// 三条与「电池」不同的地方，都来自规格：
    /// <list type="bullet">
    /// <item>**只补足免费能源未满足的需求**，实际输出 ＝ min（额定功率，电网分配的剩余需求）；</item>
    /// <item>**默认不主动为储能充电**；玩家开启许可并设置目标比例后，未达到目标的储能空间
    ///       才会形成额外电网需求，达到目标后自动回到只满足实时负载；</item>
    /// <item>**保存已投入但尚未消耗完的燃料电量**，启停不会损失剩余燃料（所以燃料以电量记账）。</item>
    /// </list>
    /// </summary>
    public sealed class FuelGenerator : IEnergyGenerator
    {
        public FuelGenerator(PersistentId id, float ratedPower, float fuelEnergy)
        {
            if (ratedPower < 0f) throw new ArgumentOutOfRangeException(nameof(ratedPower));
            if (fuelEnergy < 0f) throw new ArgumentOutOfRangeException(nameof(fuelEnergy));
            Id = id;
            RatedPower = ratedPower;
            FuelEnergyAvailable = fuelEnergy;
        }

        public PersistentId Id { get; }

        public GeneratorKind Kind => GeneratorKind.Fuel;

        public float RatedPower { get; }

        public bool IsOn { get; set; } = true;

        public float EnvironmentPower => 0f;

        public float FuelEnergyAvailable { get; set; }

        public bool AllowsCharging { get; set; }

        public float ChargeTargetRatio { get; set; }

        public float ActualOutputPower { get; set; }

        public float FuelEnergyConsumed { get; set; }

        /// <summary>剩余燃料折算成的生物质数量（界面与蓄电任务用得到）。</summary>
        public float RemainingBiomass => FirstVersionEnergy.EnergyToBiomass(FuelEnergyAvailable);

        /// <summary>
        /// 是否处于「待命」：开着、但没有需求，实际输出与燃料消耗都是 0
        /// （规格：基础生物质发电机没有空转燃料消耗）。
        /// </summary>
        public bool IsStandby => IsOn && ActualOutputPower <= 0f;

        /// <summary>投入燃料（按电量记账，内部按 1 生物质 ＝ 60 电量换算）。</summary>
        public void AddBiomass(float biomass)
        {
            if (biomass < 0f) throw new ArgumentOutOfRangeException(nameof(biomass));
            FuelEnergyAvailable += FirstVersionEnergy.BiomassToEnergy(biomass);
        }
    }

    /// <summary>
    /// 蓄电设施（第一版的基础蓄电池）。
    ///
    /// 第一版**不设置充放电功率上限、不设转换损耗**，也不实现最低储能保护——
    /// 所以这个类型除了容量与当前电量之外没有别的可调项；
    /// 出现「最小保留电量」这类字段就说明有人在实现设计以外的东西。
    /// </summary>
    public sealed class BatteryStorage : IEnergyStorage
    {
        public BatteryStorage(PersistentId id, float capacity, float charge)
        {
            if (capacity <= 0f) throw new ArgumentOutOfRangeException(nameof(capacity));
            Id = id;
            Capacity = capacity;
            Charge = Clamp(charge, 0f, capacity);
        }

        public PersistentId Id { get; }

        public float Capacity { get; }

        public float Charge { get; set; }

        public StorageState State { get; set; }

        public float ActualPower { get; set; }

        /// <summary>储量比例（界面显示电量／上限）。</summary>
        public float Ratio => Capacity <= 0f ? 0f : Charge / Capacity;

        private static float Clamp(float value, float min, float max) =>
            value < min ? min : value > max ? max : value;
    }

    /// <summary>
    /// 参与区域电网结算的用电对象。
    ///
    /// 规格把停机的最小单位定成「整台机器或整座建筑」，所以这里的粒度就是对象级：
    /// 停机只改本对象的供电结论，不碰它内部的组件设置。
    ///
    /// 它只覆盖「用待机／稳定两个点就能描述」的对象（建筑等）。
    /// **机器不走这一条**——它的耗电必须按部件逐项求和，见 <c>MachineEnergyConsumer</c>。
    /// </summary>
    public sealed class EnergyConsumer : IEnergyConsumer
    {
        public EnergyConsumer(PersistentId id, PowerPriority priority, float standbyPower, float workingPower)
        {
            if (standbyPower < 0f) throw new ArgumentOutOfRangeException(nameof(standbyPower));
            if (workingPower < 0f) throw new ArgumentOutOfRangeException(nameof(workingPower));
            Id = id;
            Priority = priority;
            StandbyPower = standbyPower;
            WorkingPower = workingPower;
        }

        public PersistentId Id { get; }

        public PowerPriority Priority { get; set; }

        public float StandbyPower { get; }

        public float WorkingPower { get; }

        public bool IsWorking { get; set; }

        /// <summary>
        /// 是否请求用电。玩家自己关掉的对象为 false，于是它既不耗电也不参与停机判定——
        /// 这与「因缺电被停机」是两件不同的事，界面要区分显示。
        /// </summary>
        public bool IsDemandActive { get; set; } = true;

        public long QueueOrder { get; set; }

        public bool IsPowered { get; set; }

        public float ActualPower { get; set; }

        public bool IsStoppedByShortage { get; set; }

        /// <summary>当前请求功率（请求用电时按工作状态选待机或稳定功率）。</summary>
        public float RequestedPower => IsDemandActive ? (IsWorking ? WorkingPower : StandbyPower) : 0f;
    }
}
