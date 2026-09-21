using System;

namespace AutoEra.Machines
{
    public enum HardwareKind { Sensor, Core, Effector }
    public enum DefinitionAvailability { Ready, PendingConfiguration, PendingResource }

    /// <summary>Validated immutable configuration, separate from machine state.</summary>
    public sealed class MachineDefinition
    {
        public int Id { get; }
        public string Name { get; }
        public int Level { get; }
        public int SensorSlots { get; }
        public int CoreSlots { get; }
        public int EffectorSlots { get; }
        public int BaseCapacity { get; }
        public bool CanMove { get; }
        public bool CanRotate { get; }
        public double MaximumIntegrity { get; }

        /// <summary>交互占地沿本地 X（米）。来源见 <c>MachineDefinitions</c> 数据表；轴显式命名是为了避免按「长×宽」读表时把 X/Z 换错。</summary>
        public double FootprintX { get; }

        /// <summary>交互占地沿本地 Z（米）。</summary>
        public double FootprintZ { get; }

        /// <summary>
        /// 占地是否已配置。0 表示数据表没有给该型号配置占地——这是「未配置」而不是「零尺寸」，
        /// 放置流程必须以「未配置占地」拒绝，不得替它猜一个值。
        /// </summary>
        public bool HasFootprint => FootprintX > 0d && FootprintZ > 0d;

        /// <summary>落位校验要的占地向量；未配置时为 <see cref="UnityEngine.Vector2.zero"/>，调用方必须先用 <see cref="HasFootprint"/> 判。</summary>
        public UnityEngine.Vector2 Footprint => new UnityEngine.Vector2((float)FootprintX, (float)FootprintZ);

        /// <summary>
        /// 实体预制体名（相对 <c>Assets/Game/Prefabs/Entity/</c>，例如 <c>Machines/WheeledCarrier</c>）。
        ///
        /// 刻意放在定义上而不是让调用方去查 <c>MachineCatalog</c>：目录按**数据行 Id**（如 10011）索引，
        /// 而运行时只拿得到 <c>MachineDefinition.Id</c>（ModelId，如 1001），
        /// 中间少一层映射就会让「有实例却找不到预制体」变成必然。空串表示未配置。
        /// </summary>
        public string Prefab { get; }

        /// <summary>是否配置了可用于实例化的预制体名。</summary>
        public bool HasPrefab => !string.IsNullOrWhiteSpace(Prefab);

        /// <summary>
        /// 载体待机功率：已供电但没有执行主要工作时。
        ///
        /// 与 <see cref="WorkingPower"/> 是**两个独立字段**，不套用「稳定功率的 10%」这类统一倍率
        /// （规格 13-统一数值模型：待机约为稳定功率的 10%，但两者作为独立字段配置）。
        /// 数值来自数据表，代码里不假定。
        /// </summary>
        public double IdlePower { get; }

        /// <summary>载体稳定功率：执行主要工作（移动／转向）时。</summary>
        public double WorkingPower { get; }

        public MachineDefinition(int id, string name, int level, int sensors, int cores, int effectors,
            int capacity, bool canMove, bool canRotate, double maximumIntegrity,
            double footprintX = 0d, double footprintZ = 0d, string prefab = null,
            double idlePower = 0d, double workingPower = 0d)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(name) || level < 1 || level > 2 ||
                sensors < 0 || cores < 0 || effectors < 0 || capacity < 0 ||
                maximumIntegrity <= 0 || double.IsInfinity(maximumIntegrity) || double.IsNaN(maximumIntegrity))
                throw new ArgumentException("Invalid machine definition.");
            // 占地允许整体缺省（0/0＝未配置），但不允许半个、负数或非有限值：
            // 「只有一个轴配了值」几乎一定是在数据表里填错了一格，宁可在这里立刻失败。
            if (!IsFootprintComponent(footprintX) || !IsFootprintComponent(footprintZ) ||
                (footprintX > 0d) != (footprintZ > 0d))
                throw new ArgumentException("Invalid machine footprint.");
            if (!IsPower(idlePower) || !IsPower(workingPower) || workingPower < idlePower)
                throw new ArgumentException("Invalid machine power profile.");
            Id = id; Name = name; Level = level; SensorSlots = sensors; CoreSlots = cores;
            EffectorSlots = effectors; BaseCapacity = capacity; CanMove = canMove;
            CanRotate = canRotate; MaximumIntegrity = maximumIntegrity;
            FootprintX = footprintX; FootprintZ = footprintZ; Prefab = prefab;
            IdlePower = idlePower; WorkingPower = workingPower;
        }

        private static bool IsFootprintComponent(double value) =>
            value >= 0d && !double.IsInfinity(value) && !double.IsNaN(value);

        private static bool IsPower(double value) =>
            value >= 0d && !double.IsInfinity(value) && !double.IsNaN(value);

        public int SlotCount(HardwareKind kind)
        {
            switch (kind)
            {
                case HardwareKind.Sensor: return SensorSlots;
                case HardwareKind.Core: return CoreSlots;
                case HardwareKind.Effector: return EffectorSlots;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }

    public sealed class ComponentDefinition
    {
        public int Id { get; }
        public HardwareKind Kind { get; }
        public int Level { get; }
        public int AddedCapacity { get; }
        public int ComputeCapacity { get; }
        public int LogicCapacity { get; }
        public bool HasBehavior { get; }

        /// <summary>组件待机功率：已供电但没有在执行主要工作时。</summary>
        public double IdlePower { get; }

        /// <summary>组件稳定功率：传感器持续采样、计算核心运行算法或效应器执行动作时。</summary>
        public double WorkingPower { get; }

        public ComponentDefinition(int id, HardwareKind kind, int level, int addedCapacity,
            int computeCapacity, int logicCapacity, bool hasBehavior,
            double idlePower = 0d, double workingPower = 0d)
        {
            if (id <= 0 || !Enum.IsDefined(typeof(HardwareKind), kind) || level < 1 || level > 2 ||
                addedCapacity < 0 || computeCapacity < 0 || logicCapacity < 0 ||
                (kind != HardwareKind.Core && (computeCapacity != 0 || logicCapacity != 0)) ||
                (kind != HardwareKind.Effector && (addedCapacity != 0 || hasBehavior)) ||
                (addedCapacity > 0 && hasBehavior))
                throw new ArgumentException("Invalid component definition.");
            if (idlePower < 0d || workingPower < 0d || double.IsNaN(idlePower) || double.IsNaN(workingPower) ||
                double.IsInfinity(idlePower) || double.IsInfinity(workingPower) || workingPower < idlePower)
                throw new ArgumentException("Invalid component power profile.");
            Id = id; Kind = kind; Level = level; AddedCapacity = addedCapacity;
            ComputeCapacity = computeCapacity; LogicCapacity = logicCapacity; HasBehavior = hasBehavior;
            IdlePower = idlePower; WorkingPower = workingPower;
        }
    }

    /// <summary>
    /// 组件定义的**展示行**（只读）。
    ///
    /// 与 <see cref="ComponentDefinition"/> 的分工是「谁用」：定义是运行时装配需要的最小集
    /// （型号、类别、能力），展示行还带名字、价格、功率与可用性——这些只存在于数据表里，
    /// 界面要展示的正是它们。没有这一层，界面就只能显示「型号 2001 等级 1」这种对玩家无意义的标识。
    ///
    /// **行 Id 与数据表一致**（`ModelId * 10 + Level`，与目录的校验规则同源），
    /// 所以从一份 <see cref="ComponentDefinition"/> 可以反查回展示行——实例详情就是用这条关系
    /// 拿到名字与规格的。
    /// </summary>
    public sealed class ComponentDisplayRow
    {
        public ComponentDisplayRow(int rowId, int modelId, string name, HardwareKind kind, int level,
            int addedCapacity, int computeCapacity, int logicCapacity, bool hasBehavior,
            int purchasePrice, int recyclePrice, double idlePower, double workingPower,
            DefinitionAvailability availability, string prefab)
        {
            RowId = rowId; ModelId = modelId; Name = name; Kind = kind; Level = level;
            AddedCapacity = addedCapacity; ComputeCapacity = computeCapacity; LogicCapacity = logicCapacity;
            HasBehavior = hasBehavior; PurchasePrice = purchasePrice; RecyclePrice = recyclePrice;
            IdlePower = idlePower; WorkingPower = workingPower; Availability = availability; Prefab = prefab;
        }

        /// <summary>数据表行 Id（`ModelId * 10 + Level`）。</summary>
        public int RowId { get; }
        public int ModelId { get; }
        public string Name { get; }
        public HardwareKind Kind { get; }
        public int Level { get; }
        public int AddedCapacity { get; }
        public int ComputeCapacity { get; }
        public int LogicCapacity { get; }
        public bool HasBehavior { get; }
        public int PurchasePrice { get; }
        public int RecyclePrice { get; }
        public double IdlePower { get; }
        public double WorkingPower { get; }
        public DefinitionAvailability Availability { get; }
        public string Prefab { get; }

        /// <summary>该型号是否已经可以装配。其余状态（缺资源／待配置）是「不可用的原因」来源，不隐藏。</summary>
        public bool IsReady => Availability == DefinitionAvailability.Ready;

        /// <summary>类别与等级的中文展示片段，例如「核心 1 级」。</summary>
        public string KindLabel => Kind switch
        {
            HardwareKind.Sensor => "传感器",
            HardwareKind.Core => "核心",
            HardwareKind.Effector => "执行器",
            _ => "未知",
        } + " " + Level + " 级";
    }
}
