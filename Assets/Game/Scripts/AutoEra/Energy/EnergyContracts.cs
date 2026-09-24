using AutoEra.World.Identity;

namespace AutoEra.Energy
{
    /// <summary>
    /// 供电优先级（规格 06-能源储存与物流「供电优先级」）：
    /// 关键设备 → 普通生产 → 次要生产 → 可暂停设备；缺电时**从低到高**停机。
    /// 数值越大越重要，停机顺序就是这个枚举的升序。
    /// </summary>
    public enum PowerPriority
    {
        Pausable = 0,
        Secondary = 1,
        Production = 2,
        Critical = 3,
    }

    /// <summary>
    /// 发电设施的运行特征（规格：消耗型／环境型只是特征，不强制互斥）。
    /// 第一版只有两类参与调度：免费环境能源与已开启的燃料发电站。
    /// </summary>
    public enum GeneratorKind
    {
        /// <summary>太阳能等免费环境能源：优先直接满足负载，盈余自动充电，储满后舍弃。</summary>
        Environment = 0,

        /// <summary>消耗燃料的发电站：只补足免费能源未满足的需求，实际输出不超过额定。</summary>
        Fuel = 1,
    }

    /// <summary>储能设施在当前结算时刻的状态。</summary>
    public enum StorageState
    {
        Idle = 0,
        Charging = 1,
        Discharging = 2,
    }

    /// <summary>
    /// 参与区域电网结算的发电设施。
    ///
    /// 刻意只暴露结算需要的量：额定功率、开关、燃料余量与充电策略。
    /// 「实际输出」是结算的**结果**，由 <see cref="EnergyGrid"/> 回写，不是设施的输入——
    /// 否则就会出现「设施自己说自己发了多少」这种无法对账的状态。
    /// </summary>
    public interface IEnergyGenerator
    {
        PersistentId Id { get; }

        GeneratorKind Kind { get; }

        /// <summary>额定功率（功率单位）。第一版是固有属性，玩家只能开关。</summary>
        float RatedPower { get; }

        /// <summary>设施开关。关闭的设施不参与结算。</summary>
        bool IsOn { get; }

        /// <summary>当前环境（有日照／无日照）。只对 <see cref="GeneratorKind.Environment"/> 有意义。</summary>
        float EnvironmentPower { get; }

        /// <summary>剩余可用燃料折算成的电量（1 生物质 ＝ 60 电量）。环境能源恒为 0。</summary>
        float FuelEnergyAvailable { get; set; }

        /// <summary>玩家是否允许燃料发电为蓄电池充电（规格：默认不允许）。</summary>
        bool AllowsCharging { get; set; }

        /// <summary>燃料充电的目标储能比例（0..1）。</summary>
        float ChargeTargetRatio { get; set; }

        /// <summary>结算回写：本时刻的实际输出功率。</summary>
        float ActualOutputPower { get; set; }

        /// <summary>结算回写：本时刻消耗掉的燃料（折算成电量）。</summary>
        float FuelEnergyConsumed { get; set; }
    }

    /// <summary>参与区域电网结算的储能设施。</summary>
    public interface IEnergyStorage
    {
        PersistentId Id { get; }

        /// <summary>电量上限。</summary>
        float Capacity { get; }

        /// <summary>当前电量。</summary>
        float Charge { get; set; }

        /// <summary>结算回写：本时刻状态（充电／放电／待机）。</summary>
        StorageState State { get; set; }

        /// <summary>结算回写：本时刻实际充放电功率（充电为正、放电为负）。</summary>
        float ActualPower { get; set; }
    }

    /// <summary>
    /// 参与区域电网结算的用电对象（机器或建筑，规格里最小停机单位就是「整台机器或整座建筑」）。
    ///
    /// **它只暴露一个 <see cref="RequestedPower"/>，而不是「待机功率 ＋ 稳定功率 ＋ 是否在工作」**：
    /// 后者会把实现推向「整机一个倍率」，而规格明确禁止这种做法——
    /// 「整台机器是否处于工作负载必须按照各部件当前活动分别求和」。
    /// 一台机器完全可能是「核心在跑算法、效应器待机、传感器持续采样」这种混合状态，
    /// 两个点表示不了它。
    /// </summary>
    public interface IEnergyConsumer
    {
        PersistentId Id { get; }

        /// <summary>是否请求用电（玩家关掉的对象不请求）。</summary>
        bool IsDemandActive { get; }

        /// <summary>
        /// 本时刻请求的功率：由实现按**自己的部件状态**逐项求和得出，
        /// 电网不再替它决定「待机还是稳定」。不请求用电时必须是 0。
        /// </summary>
        float RequestedPower { get; }

        /// <summary>供电优先级。</summary>
        PowerPriority Priority { get; }

        /// <summary>进入供电队列的顺序（越小越早）。由 <see cref="EnergyGrid.AddConsumer"/> 分配。</summary>
        long QueueOrder { get; set; }

        /// <summary>结算回写：本时刻是否获得供电。缺电被停机的对象为 false。</summary>
        bool IsPowered { get; set; }

        /// <summary>结算回写：本时刻的实际耗电功率（未供电时为 0）。</summary>
        float ActualPower { get; set; }

        /// <summary>结算回写：本时刻是否因为缺电被停机（与「玩家自己关掉」区分开）。</summary>
        bool IsStoppedByShortage { get; set; }
    }
}
