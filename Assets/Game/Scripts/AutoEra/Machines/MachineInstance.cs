using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Machines
{
    public enum ManagementOrigin { Field, Hub, Library }
    public enum MachineRunState { Stopped, Running, Sleeping }
    public enum MachineManagementResult
    {
        Completed, WaitingForSafeStop, InvalidOrigin, NotDeployed, MustStop, NotActivated,
        Destroyed, Disconnected, InvalidSlot, Occupied, AlreadyInstalled, MissingComponent,
        CapacityInUse, CoreHasNoSwitch, InvalidName, DuplicateName, InvalidState, ComputeInUse, LogicCapacityInUse, RepairRequired
    }

    public sealed class ComponentInstance
    {
        public PersistentId Id { get; }
        public ComponentDefinition Definition { get; }
        public PersistentId OwnerId { get; internal set; }
        public bool Enabled { get; internal set; } = true;
        internal ComponentInstance(PersistentId id, ComponentDefinition definition) { Id = id; Definition = definition; }
    }

    public sealed class MachineInstance
    {
        private readonly ComponentInstance[][] _slots;
        public PersistentId Id { get; }
        public MachineDefinition Definition { get; }
        public ulong ModelSerial { get; }
        public string Name { get; internal set; }
        public bool Deployed { get; internal set; }
        // Transient region ownership; neither a second identity nor a saved deployment flag.
        internal object RegionBindingOwner { get; set; }
        public bool Activated { get; private set; }
        public bool PowerSwitchOn { get; private set; } = true;
        public bool SupplyAvailable { get; private set; }
        public bool SignalAvailable { get; private set; }
        public double Integrity { get; private set; }
        public MachineRunState RequestedRunState { get; private set; } = MachineRunState.Stopped;
        public bool HasActiveBehavior { get; private set; }
        public int UsedCapacity { get; private set; }
        public int ReservedCompute { get; private set; }
        public int AppliedLogicCost { get; private set; }
        public int ComputeWaitingCount { get; private set; }
        public long Revision { get; private set; }
        public event Action<MachineInstance> Changed;
        internal void NotifyChanged() { Revision++; Changed?.Invoke(this); }
        public bool Powered => Deployed && PowerSwitchOn && SupplyAvailable && Integrity > 0;
        public bool Connected => Activated && Powered && SignalAvailable;
        public bool CanRun => Activated && Powered && RequestedRunState == MachineRunState.Running;
        public int TotalCapacity => checked(Definition.BaseCapacity + Sum(d => d.AddedCapacity));
        public int ComputeCapacity => Sum(d => d.ComputeCapacity);
        public int LogicCapacity => Sum(d => d.LogicCapacity);

        // ---------------------------------------------------------------- 能耗（区域电网的负载侧）

        /// <summary>
        /// 机器是否正在移动（导航进行中）。
        ///
        /// 由执行上下文推送，而不是让能耗模型自己去问导航——导航只属于现场运行时，
        /// 而「这台机器现在算不算在工作负载」在离线推进里也要答得出来。
        /// </summary>
        public bool IsMoving { get; private set; }

        /// <summary>正在执行动作的组件（效应器等），按组件身份记录。</summary>
        private readonly HashSet<PersistentId> _activeComponents = new HashSet<PersistentId>();

        /// <summary>
        /// 推送移动状态（执行上下文在导航开始／结束时调用）。
        ///
        /// **刻意不发 <see cref="Changed"/>**：这是能耗模型**按需读取**的事实，不是机器对外状态的
        /// 变化。广播它会让执行上下文自己（订阅了 Changed）被自己的推送重新唤醒，
        /// 现场运行时与读模型也会跟着白跑一遍——而收益只是「界面能立刻看到这个内部标志」，
        /// 没有任何一处需要它。
        /// </summary>
        public void UpdateNavigationActivity(bool moving)
        {
            IsMoving = moving;
        }

        /// <summary>
        /// 推送某个组件是否正在执行动作（执行上下文按效应器绑定推送）。
        /// 与 <see cref="UpdateNavigationActivity"/> 同理，不发变更事件。
        /// </summary>
        public void SetComponentActivity(PersistentId componentId, bool active)
        {
            if (!componentId.IsValid) return;
            if (active) _activeComponents.Add(componentId);
            else _activeComponents.Remove(componentId);
        }

        /// <summary>该组件此刻是否正在执行动作。</summary>
        public bool IsComponentActive(PersistentId componentId) =>
            componentId.IsValid && _activeComponents.Contains(componentId);

        /// <summary>
        /// 全部部件按待机功率统计的耗电（＝整机完全空闲时的负载）。
        ///
        /// 它与 <see cref="CurrentPowerDraw"/> 是**逐部件求和**，不是「整机一个倍率」——
        /// 规格明确要求「整台机器是否处于工作负载必须按照各部件当前活动分别求和」。
        /// </summary>
        public double IdlePowerDraw
        {
            get
            {
                double total = Definition.IdlePower;
                for (int k = 0; k < _slots.Length; k++)
                {
                    foreach (ComponentInstance item in _slots[k])
                    {
                        if (item != null) total += item.Definition.IdlePower;
                    }
                }

                return total;
            }
        }

        /// <summary>
        /// 按**各部件当前活动**逐项求和的耗电：这是「供给充足时这台机器会消耗多少」。
        ///
        /// 它**不看当前供电状态**（只看机器是否完好、是否休眠）：
        /// 区域电网要用它决定「该不该给这台机器供电」，如果用「现在有没有电」来算功率，
        /// 一台断电的机器就会请求 0 功率、于是永远拿不回供电——一个闭环死锁。
        /// 「断电机器能耗归零」说的是**实际消耗**，那由电网在结算时置 0。
        ///
        /// 规则（规格 06「供电状态、运行模式与当前活动」与 13-统一数值模型）：
        /// <list type="bullet">
        /// <item>载体：机器在移动时用稳定功率，否则待机；</item>
        /// <item>传感器：未休眠且启用时**持续采样**，用稳定功率；</item>
        /// <item>计算核心：正在运行算法（有算力占用）时用稳定功率；</item>
        /// <item>效应器：正在执行动作时用稳定功率，其余用待机。</item>
        /// </list>
        /// 机器休眠时全部按待机。整机现场断电或已损坏时请求为 0，
        /// 但那由 <c>IsDemandActive</c> 与完好度判断，不在活动模型里重复表达。
        /// </summary>
        public double CurrentPowerDraw
        {
            get
            {
                if (Integrity <= 0d) return 0d;

                bool awake = RequestedRunState != MachineRunState.Sleeping;
                double total = IsMoving && awake ? Definition.WorkingPower : Definition.IdlePower;
                for (int k = 0; k < _slots.Length; k++)
                {
                    HardwareKind kind = (HardwareKind)k;
                    for (int index = 0; index < _slots[k].Length; index++)
                    {
                        ComponentInstance item = _slots[k][index];
                        if (item == null) continue;
                        total += ItemPower(kind, index, item, awake);
                    }
                }

                return total;
            }
        }

        private double ItemPower(HardwareKind kind, int index, ComponentInstance item, bool awake)
        {
            if (!awake || !item.Enabled) return item.Definition.IdlePower;
            switch (kind)
            {
                // 传感器持续采样：只要供电、未休眠且启用就是稳定功率。
                case HardwareKind.Sensor:
                    return item.Definition.WorkingPower;
                // 计算核心运行算法时是稳定功率；有算力占用即视为在运行。
                case HardwareKind.Core:
                    return ReservedCompute > 0 ? item.Definition.WorkingPower : item.Definition.IdlePower;
                case HardwareKind.Effector:
                    return IsComponentActive(item.Id) ? item.Definition.WorkingPower : item.Definition.IdlePower;
                default:
                    return item.Definition.IdlePower;
            }
        }

        internal MachineInstance(PersistentId id, MachineDefinition definition, ulong serial, string name)
        {
            Id = id; Definition = definition; ModelSerial = serial; Name = name; Integrity = definition.MaximumIntegrity;
            _slots = new[] { new ComponentInstance[definition.SensorSlots], new ComponentInstance[definition.CoreSlots], new ComponentInstance[definition.EffectorSlots] };
        }

        internal void RestoreConfiguration(bool deployed, bool activated, bool powerSwitch, MachineRunState state, double integrity, int used)
        {
            if (!Enum.IsDefined(typeof(MachineRunState), state) || used < 0 || used > TotalCapacity ||
                (!activated && state != MachineRunState.Stopped) || (!deployed && state != MachineRunState.Stopped) ||
                (integrity == 0 && (activated || powerSwitch || state != MachineRunState.Stopped)))
                throw new ArgumentException("Invalid recovered machine state.");
            UpdateIntegrity(integrity);
            Deployed = deployed; Activated = activated; PowerSwitchOn = powerSwitch;
            RequestedRunState = state; UsedCapacity = used;
            // Environment and live execution are acquired afresh; never deserialize stale availability.
            SupplyAvailable = false; SignalAvailable = false; HasActiveBehavior = false;
        }

        private int Sum(Func<ComponentDefinition, int> selector)
        {
            int value = 0;
            foreach (var group in _slots) foreach (var item in group) if (item != null) value = checked(value + selector(item.Definition));
            return value;
        }

        public ComponentInstance GetComponent(HardwareKind kind, int index)
            => ValidSlot(kind, index) ? _slots[(int)kind][index] : null;
        private bool ValidSlot(HardwareKind kind, int index)
            => (int)kind >= 0 && (int)kind < _slots.Length && index >= 0 && index < _slots[(int)kind].Length;

        public MachineManagementResult Activate(ManagementOrigin origin)
        {
            if (origin != ManagementOrigin.Field) return MachineManagementResult.InvalidOrigin;
            if (!Deployed) return MachineManagementResult.NotDeployed;
            if (Integrity <= 0) return MachineManagementResult.Destroyed;
            Activated = true;
            NotifyChanged();
            return MachineManagementResult.Completed;
        }

        public MachineManagementResult SetRunState(ManagementOrigin origin, MachineRunState state)
        {
            if (!Enum.IsDefined(typeof(MachineRunState), state)) return MachineManagementResult.InvalidState;
            var access = CheckManagementAccess(origin);
            if (access != MachineManagementResult.Completed) return access;
            if (state != MachineRunState.Stopped && !Activated) return MachineManagementResult.NotActivated;
            if (Integrity <= 0) return MachineManagementResult.Destroyed;
            RequestedRunState = state;
            NotifyChanged();
            return state == MachineRunState.Stopped && HasActiveBehavior ? MachineManagementResult.WaitingForSafeStop : MachineManagementResult.Completed;
        }

        public MachineManagementResult SetPowerSwitch(ManagementOrigin origin, bool on)
        {
            if (origin != ManagementOrigin.Field || !Deployed) return MachineManagementResult.InvalidOrigin;
            if (on && Integrity <= 0) return MachineManagementResult.Destroyed;
            PowerSwitchOn = on; NotifyChanged(); return MachineManagementResult.Completed;
        }

        // External adapters supply facts; this is not an energy grid or signal simulator.
        public void UpdateEnvironment(bool supply, bool signal)
        {
            if (SupplyAvailable == supply && SignalAvailable == signal) return;
            SupplyAvailable = supply; SignalAvailable = signal; NotifyChanged();
        }

        /// <summary>
        /// 只更新**供电**事实（区域电网的结论）。
        ///
        /// 单列一个方法是因为供电与区域信号来自两个不同的来源（电网／信号塔覆盖），
        /// 用一个「把两个都写上」的方法会让电网顺手把信号也覆盖掉——那是一个看不出来的越权。
        /// </summary>
        public void UpdateSupply(bool supply)
        {
            if (SupplyAvailable == supply) return;
            SupplyAvailable = supply; NotifyChanged();
        }
        public void UpdateBehaviorActivity(bool active)
        { if (HasActiveBehavior == active) return; HasActiveBehavior = active; NotifyChanged(); }
        public void UpdateComputeUsage(int reserved, int logicCost, int waiting = 0)
        {
            if (reserved < 0 || reserved > ComputeCapacity || logicCost < 0 || logicCost > LogicCapacity || waiting < 0)
                throw new ArgumentOutOfRangeException();
            if (ReservedCompute == reserved && AppliedLogicCost == logicCost && ComputeWaitingCount == waiting) return;
            ReservedCompute = reserved; AppliedLogicCost = logicCost; ComputeWaitingCount = waiting; NotifyChanged();
        }
        public void UpdateContainerUsage(int used)
        {
            if (used < 0 || used > TotalCapacity) throw new ArgumentOutOfRangeException(nameof(used));
            UsedCapacity = used; NotifyChanged();
        }
        public void UpdateIntegrity(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < 0 || value > Definition.MaximumIntegrity) throw new ArgumentOutOfRangeException(nameof(value));
            Integrity = value;
            if (value == 0) { Activated = false; PowerSwitchOn = false; RequestedRunState = MachineRunState.Stopped; }
            NotifyChanged();
        }
        public bool IsComponentWorking(HardwareKind kind, int index)
        {
            var item = GetComponent(kind, index);
            return item != null && CanRun && item.Enabled;
        }
        public MachineManagementResult SetComponentEnabled(ManagementOrigin origin, HardwareKind kind, int index, bool enabled)
        {
            var access = CheckManagementAccess(origin);
            if (access != MachineManagementResult.Completed) return access;
            if (kind == HardwareKind.Core) return MachineManagementResult.CoreHasNoSwitch;
            if (!ValidSlot(kind, index)) return MachineManagementResult.InvalidSlot;
            var item = GetComponent(kind, index);
            if (item == null) return MachineManagementResult.MissingComponent;
            item.Enabled = enabled; NotifyChanged(); return MachineManagementResult.Completed;
        }
        private MachineManagementResult CheckManagementAccess(ManagementOrigin origin)
        {
            if (origin == ManagementOrigin.Library) return Deployed ? MachineManagementResult.InvalidOrigin : MachineManagementResult.Completed;
            if (origin == ManagementOrigin.Field) return Deployed ? MachineManagementResult.Completed : MachineManagementResult.NotDeployed;
            if (origin == ManagementOrigin.Hub) return Connected ? MachineManagementResult.Completed : MachineManagementResult.Disconnected;
            return MachineManagementResult.InvalidOrigin;
        }
        private MachineManagementResult HardwareGate(ManagementOrigin origin)
        {
            if ((Deployed && origin != ManagementOrigin.Field) || (!Deployed && origin != ManagementOrigin.Library)) return MachineManagementResult.InvalidOrigin;
            if (Integrity <= 0) return MachineManagementResult.Destroyed;
            if (RequestedRunState != MachineRunState.Stopped) return MachineManagementResult.MustStop;
            return HasActiveBehavior ? MachineManagementResult.WaitingForSafeStop : MachineManagementResult.Completed;
        }
        internal MachineManagementResult Install(ManagementOrigin origin, ComponentInstance component, int index)
        {
            var gate = HardwareGate(origin);
            if (gate != MachineManagementResult.Completed) return gate;
            var kind = component.Definition.Kind;
            if (!ValidSlot(kind, index)) return MachineManagementResult.InvalidSlot;
            if (component.OwnerId.IsValid) return MachineManagementResult.AlreadyInstalled;
            if (_slots[(int)kind][index] != null) return MachineManagementResult.Occupied;
            if ((long)TotalCapacity + component.Definition.AddedCapacity > int.MaxValue ||
                (long)ComputeCapacity + component.Definition.ComputeCapacity > int.MaxValue ||
                (long)LogicCapacity + component.Definition.LogicCapacity > int.MaxValue)
                return MachineManagementResult.InvalidState;
            _slots[(int)kind][index] = component; component.OwnerId = Id;
            NotifyChanged();
            return MachineManagementResult.Completed;
        }
        internal MachineManagementResult Remove(ManagementOrigin origin, HardwareKind kind, int index)
        {
            var gate = HardwareGate(origin);
            if (gate != MachineManagementResult.Completed) return gate;
            if (!ValidSlot(kind, index)) return MachineManagementResult.InvalidSlot;
            var item = _slots[(int)kind][index];
            if (item == null) return MachineManagementResult.MissingComponent;
            if (TotalCapacity - item.Definition.AddedCapacity < UsedCapacity) return MachineManagementResult.CapacityInUse;
            if (ComputeCapacity - item.Definition.ComputeCapacity < ReservedCompute) return MachineManagementResult.ComputeInUse;
            if (item.Definition.Kind == HardwareKind.Core && ComputeWaitingCount > 0) return MachineManagementResult.ComputeInUse;
            if (LogicCapacity - item.Definition.LogicCapacity < AppliedLogicCost) return MachineManagementResult.LogicCapacityInUse;
            _slots[(int)kind][index] = null; item.OwnerId = PersistentId.Invalid;
            NotifyChanged();
            return MachineManagementResult.Completed;
        }

        /// <summary>机器上装着的组件件数（跨三个类别）。</summary>
        public int InstalledComponentCount
        {
            get
            {
                int count = 0;
                foreach (var group in _slots) foreach (var item in group) if (item != null) count++;
                return count;
            }
        }

        /// <summary>
        /// 一键卸下全部：**要么每一件都卸下，要么一件都不动**（规格 05-机器整备：
        /// 「确认全部卸下影响并原子回库」）。
        ///
        /// 为什么不能在门禁之后逐件调 <see cref="Remove"/>：逐件会在中途撞上余量不足而失败，
        /// 留下「卸了一半」的机器——而原子正是要排除那个结果。
        /// 所以先算「全部卸下之后」的余量（卸完只剩基础容量，算力与逻辑占用必须归零），
        /// 通过了再一次性清空。
        ///
        /// 一件都没装时返回 <see cref="MachineManagementResult.MissingComponent"/>：
        /// 这个意图本身没有意义，如实拒绝而不是报成功。
        ///
        /// 来源比单槽拆卸更严：**只有整备环境（Library）**。规格把「一键卸下」列在
        /// 「未部署机器处于整备环境，可以……一键卸下」之下，而已部署机器的硬件修改
        /// 「必须现场逐项完成」——批量拆一台正在服役的机器不是这一版设计里的动作。
        /// </summary>
        internal MachineManagementResult RemoveAll(ManagementOrigin origin)
        {
            if (origin != ManagementOrigin.Library) return MachineManagementResult.InvalidOrigin;
            var gate = HardwareGate(origin);
            if (gate != MachineManagementResult.Completed) return gate;
            if (InstalledComponentCount == 0) return MachineManagementResult.MissingComponent;
            if (Definition.BaseCapacity < UsedCapacity) return MachineManagementResult.CapacityInUse;
            if (ReservedCompute > 0) return MachineManagementResult.ComputeInUse;
            if (ComputeWaitingCount > 0) return MachineManagementResult.ComputeInUse;
            if (AppliedLogicCost > 0) return MachineManagementResult.LogicCapacityInUse;

            foreach (var group in _slots)
            {
                for (int index = 0; index < group.Length; index++)
                {
                    if (group[index] == null) continue;
                    group[index].OwnerId = PersistentId.Invalid;
                    group[index] = null;
                }
            }

            NotifyChanged();
            return MachineManagementResult.Completed;
        }
    }
}
