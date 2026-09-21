using System;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.Energy
{
    /// <summary>
    /// 把一台机器接成区域电网的**负载**，并把结算结论写回机器的供电状态。
    ///
    /// 它存在的理由是一条容易被绕过的设计约束：
    /// 「正常模式但当前没有工作的单个组件使用待机功率，**整台机器是否处于工作负载必须按照
    /// 各部件当前活动分别求和，不能用一个整机倍率替代**」（规格 06）。
    /// 所以这里的请求功率直接取自 <see cref="MachineInstance.CurrentPowerDraw"/>——
    /// 那是逐部件求和的结果，适配层只做转交，不重新解释。
    ///
    /// **回写只碰供电**：区域信号来自另一套来源（信号塔覆盖），
    /// 顺手把它一起覆盖掉是一个看不出来的越权，所以走 <see cref="MachineInstance.UpdateSupply"/>。
    ///
    /// 它不持有 Unity 对象、不订阅事件：结算顺序由持有电网的一方决定，
    /// 适配层只提供「问功率」与「告知结论」两个动作。
    /// </summary>
    public sealed class MachineEnergyConsumer : IEnergyConsumer
    {
        public MachineEnergyConsumer(MachineInstance machine, PowerPriority priority)
        {
            Machine = machine ?? throw new ArgumentNullException(nameof(machine));
            Priority = priority;
        }

        public MachineInstance Machine { get; }

        public PersistentId Id => Machine.Id;

        /// <summary>
        /// 是否请求用电。
        ///
        /// 判据是「玩家/系统没有把它关掉」：整机现场断电（<see cref="MachineInstance.PowerSwitchOn"/>
        /// 为假）时它不请求用电——那时它的能耗本来就该是 0，
        /// 而如果让它继续请求，电网会把它当成一个真实的缺口去停机别的设备。
        /// </summary>
        public bool IsDemandActive => Machine.Deployed && Machine.PowerSwitchOn;

        /// <summary>本时刻请求的功率：整机逐部件求和（未供电时为 0，所以停机不会自我维持缺口）。</summary>
        public float RequestedPower => IsDemandActive ? (float)Machine.CurrentPowerDraw : 0f;

        /// <summary>整机按待机功率统计的耗电，供界面显示「待机／工作」对比。</summary>
        public float IdlePower => (float)Machine.IdlePowerDraw;

        public PowerPriority Priority { get; set; }

        public long QueueOrder { get; set; }

        public bool IsPowered { get; set; }

        public float ActualPower { get; set; }

        public bool IsStoppedByShortage { get; set; }

        /// <summary>
        /// 把电网的结论写回机器。返回是否发生了变化（调用方据此决定要不要广播）。
        ///
        /// 只在**结论不同**时写：机器上挂着一堆订阅者，每次结算都无条件通知会让
        /// 「供电没变」变成一次全量刷新。
        /// </summary>
        public bool ApplySupply()
        {
            bool supply = IsPowered;
            if (Machine.SupplyAvailable == supply)
            {
                return false;
            }

            Machine.UpdateSupply(supply);
            return true;
        }
    }
}
