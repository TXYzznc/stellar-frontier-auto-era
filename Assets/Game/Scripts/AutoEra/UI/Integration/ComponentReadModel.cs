using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>组件域内发生变化的区域。</summary>
    public enum ComponentDomainSection
    {
        /// <summary>散件列表发生变化（新增、安装、卸载）。</summary>
        Loose,

        /// <summary>已安装列表发生变化。</summary>
        Installed,

        /// <summary>当前选中组件的详情发生变化。</summary>
        Detail,
    }

    /// <summary>
    /// 组件列表的一行：稳定身份 ＋ 目录展示名 ＋ 安装位置。
    ///
    /// 名字与规格来自**目录**（`ComponentDefinitions` 表），安装位置来自**花名册**（实例的 OwnerId）。
    /// 两者刻意分开：同一个型号可以有很多件实例，实例的身份与位置是花名册的事，
    /// 型号的名字与价格是数据表的事，界面把它们合成一行。
    /// </summary>
    public readonly struct UiComponentRow
    {
        public UiComponentRow(PersistentId id, ComponentDisplayRow definition, bool installed,
            string ownerName, HardwareKind? slotKind, int slotIndex, bool enabled)
        {
            Id = id;
            Definition = definition;
            Installed = installed;
            OwnerName = ownerName;
            SlotKind = slotKind;
            SlotIndex = slotIndex;
            Enabled = enabled;
        }

        public PersistentId Id { get; }

        /// <summary>型号展示行；定义表里查不到时为 null（界面据此说明原因，不编一个名字）。</summary>
        public ComponentDisplayRow Definition { get; }

        public bool Installed { get; }
        public string OwnerName { get; }
        public HardwareKind? SlotKind { get; }
        public int SlotIndex { get; }
        public bool Enabled { get; }

        public string Name => Definition != null ? Definition.Name : "未知型号";

        /// <summary>槽位的人读标签，例如「核心槽 0」；散件时为「—」。</summary>
        public string SlotLabel => SlotKind.HasValue
            ? (SlotKind.Value switch
            {
                HardwareKind.Sensor => "传感器槽 ",
                HardwareKind.Core => "核心槽 ",
                HardwareKind.Effector => "执行器槽 ",
                _ => "槽 ",
            }) + SlotIndex
            : "—";

        /// <summary>列表行的副标题：位置 ＋ 状态。</summary>
        public string Status => Installed
            ? (OwnerName ?? "已安装") + " ／ " + SlotLabel + (Enabled ? " ／ 启用" : " ／ 停用")
            : "散件 ／ " + (Definition != null ? Definition.KindLabel : "未知");
    }

    /// <summary>
    /// 合并后的散件行：**完全相同的未安装组件按「类型／型号／等级」合并显示数量**，
    /// 展开后仍保留各组件实例（见 `02-系统设计/01-机器自动化与算法.md` 的组件库一节）。
    ///
    /// 合并键刻意是「类型＋型号＋等级」而不是「型号」：同一型号的不同等级是两种东西，
    /// 合并它们会把可升级关系当成重复堆叠。
    /// </summary>
    public readonly struct UiComponentGroup
    {
        public UiComponentGroup(HardwareKind kind, int modelId, int level, ComponentDisplayRow definition,
            IReadOnlyList<PersistentId> members)
        {
            Kind = kind;
            ModelId = modelId;
            Level = level;
            Definition = definition;
            Members = members;
        }

        public HardwareKind Kind { get; }
        public int ModelId { get; }
        public int Level { get; }

        /// <summary>型号展示行；目录里查不到时为 null。</summary>
        public ComponentDisplayRow Definition { get; }

        /// <summary>组内各组件实例的稳定身份（展开后要保留它们，不是只留一个计数）。</summary>
        public IReadOnlyList<PersistentId> Members { get; }

        public int Count => Members == null ? 0 : Members.Count;
        public string Name => Definition != null ? Definition.Name : "未知型号";

        /// <summary>列表行标签：合并显示数量。</summary>
        public string Label => Name + " × " + Count;

        /// <summary>列表行副标题：类别等级 ＋ 数量。</summary>
        public string Status => (Definition != null ? Definition.KindLabel : "未知") + " ／ 散件 " + Count + " 件";
    }

    /// <summary>组件域的只读快照。</summary>
    public readonly struct ComponentDomainSnapshot
    {
        public ComponentDomainSnapshot(UiDataState state, string unavailableReason,
            IReadOnlyList<UiComponentRow> loose, IReadOnlyList<UiComponentRow> installed,
            IReadOnlyList<UiComponentGroup> looseGroups,
            IReadOnlyList<UiDetailField> detail, PersistentId selectedId, int selectedGroupIndex = -1)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Loose = loose;
            Installed = installed;
            LooseGroups = looseGroups;
            Detail = detail;
            SelectedId = selectedId;
            SelectedGroupIndex = selectedGroupIndex;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }

        /// <summary>散件实例（展开后的个体；详情与选中用它）。</summary>
        public IReadOnlyList<UiComponentRow> Loose { get; }

        /// <summary>已安装：按机器与被占用的槽位枚举出来的组件实例。</summary>
        public IReadOnlyList<UiComponentRow> Installed { get; }

        /// <summary>散件列表**显示用**的合并行（完全相同者合并数量）。</summary>
        public IReadOnlyList<UiComponentGroup> LooseGroups { get; }

        public IReadOnlyList<UiDetailField> Detail { get; }
        public PersistentId SelectedId { get; }

        /// <summary>选中整组散件时的组下标；-1 表示没有组选中。</summary>
        public int SelectedGroupIndex { get; }

        /// <summary>散件个体数（不是合并后的行数）。</summary>
        public int LooseCount => Loose == null ? 0 : Loose.Count;

        public int InstalledCount => Installed == null ? 0 : Installed.Count;

        /// <summary>散件合并后的行数。</summary>
        public int LooseGroupCount => LooseGroups == null ? 0 : LooseGroups.Count;

        /// <summary>是否选中了「一组散件」而不是某一件实例。</summary>
        public bool HasGroupSelection => SelectedGroupIndex >= 0;

        public bool HasSelection => SelectedId.IsValid || HasGroupSelection;

        /// <summary>选中的散件组；没有组选中时为 null。</summary>
        public UiComponentGroup? SelectedGroup =>
            LooseGroups != null && SelectedGroupIndex >= 0 && SelectedGroupIndex < LooseGroups.Count
                ? LooseGroups[SelectedGroupIndex]
                : (UiComponentGroup?)null;

        public static ComponentDomainSnapshot Unavailable(string reason) =>
            new ComponentDomainSnapshot(UiDataState.Unavailable, reason, null, null, null, null, PersistentId.Invalid);
    }

    /// <summary>
    /// 组件域读取模型。
    ///
    /// **这一域不需要新建模拟系统**，因为两半都已经在生产里存在：
    /// <list type="bullet">
    /// <item>型号与规格：`ComponentDefinitions` 数据表（22 行），由 `MachineCatalog` 解析；</item>
    /// <item>实例与安装位置：`MachineRoster.Components` 与每台机器的槽位。</item>
    /// </list>
    /// 之前它显示「未接入」的原因是**界面没有观察入口**：花名册把组件当成内部结构，
    /// 没有任何读模型去枚举它。所以这一项是接线，不是新建领域。
    ///
    /// 安装位置刻意**从机器槽位枚举**而不是读 `ComponentInstance.OwnerId` 反查：
    /// 实例上只记了「属于哪台机器」，槽位与类别只有机器自己的槽位表知道；
    /// 反过来扫槽位既能一次拿到机器名、类别与槽位号，也不需要在实例上加字段。
    /// </summary>
    public interface IComponentReadModel : IDisposable
    {
        ComponentDomainSnapshot Snapshot { get; }

        event Action<ComponentDomainSection> Changed;

        void Refresh();

        /// <summary>按**实例**的稳定 Id 选中；不存在时返回 false，不改变当前选中。</summary>
        bool Select(PersistentId componentId);

        /// <summary>
        /// 按**合并键**（类型／型号／等级）选中一整组散件。
        /// 组内成员仍逐个保留在快照里——规格要求「展开后仍保留各组件实例」。
        /// </summary>
        bool SelectGroup(HardwareKind kind, int modelId, int level);

        void ClearSelection();
    }

    /// <summary>组件域未接入时的诚实空实现：只报 Unavailable，不伪造组件。</summary>
    internal sealed class UnavailableComponentReadModel : IComponentReadModel
    {
        public UnavailableComponentReadModel(string reason)
        {
            Snapshot = ComponentDomainSnapshot.Unavailable(reason);
        }

        public ComponentDomainSnapshot Snapshot { get; }

        public event Action<ComponentDomainSection> Changed
        {
            add { }
            remove { }
        }

        public void Refresh() { }

        public bool Select(PersistentId componentId) => false;

        public bool SelectGroup(HardwareKind kind, int modelId, int level) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }

    /// <summary>真实实现：目录（型号规格）＋ 花名册（实例与安装位置）。</summary>
    internal sealed class RosterComponentReadModel : IComponentReadModel
    {
        private readonly MachineRoster _roster;
        private readonly MachineCatalog _catalog;
        private readonly List<UiComponentRow> _loose = new List<UiComponentRow>(16);
        private readonly List<UiComponentRow> _installed = new List<UiComponentRow>(16);
        private readonly List<UiComponentGroup> _looseGroups = new List<UiComponentGroup>(8);
        private readonly List<PersistentId> _groupMembers = new List<PersistentId>(4);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(16);
        private ComponentDomainSnapshot _snapshot;
        private PersistentId _selectedId = PersistentId.Invalid;
        private int _selectedGroupIndex = -1;
        private bool _disposed;

        public RosterComponentReadModel(MachineRoster roster, MachineCatalog catalog)
        {
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _catalog = catalog;
            _roster.Changed += OnRosterChanged;
            Publish();
        }

        public ComponentDomainSnapshot Snapshot => _snapshot;

        public event Action<ComponentDomainSection> Changed;

        public void Refresh() => Publish();

        public bool Select(PersistentId componentId)
        {
            if (!componentId.IsValid || !TryFind(componentId, out _))
            {
                return false;
            }

            _selectedId = componentId;
            _selectedGroupIndex = -1;
            Publish(ComponentDomainSection.Detail);
            return true;
        }

        public bool SelectGroup(HardwareKind kind, int modelId, int level)
        {
            if (!TryFindGroup(kind, modelId, level, out int index))
            {
                return false;
            }

            _selectedGroupIndex = index;
            _selectedId = PersistentId.Invalid;
            Publish(ComponentDomainSection.Detail);
            return true;
        }

        public void ClearSelection()
        {
            if (!_selectedId.IsValid && _selectedGroupIndex < 0)
            {
                return;
            }

            _selectedId = PersistentId.Invalid;
            _selectedGroupIndex = -1;
            Publish(ComponentDomainSection.Detail);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _roster.Changed -= OnRosterChanged;
            Changed = null;
        }

        private void OnRosterChanged()
        {
            if (!_disposed)
            {
                Publish();
            }
        }

        /// <summary>
        /// 重建快照后再发事件。**值类型快照必须在每次内容变化时重建**：
        /// 只发事件会让页面继续读到上一份数组（这条在存档域、区域域与算法域各踩过一次）。
        /// </summary>
        private void Publish(ComponentDomainSection section = ComponentDomainSection.Loose)
        {
            _loose.Clear();
            _installed.Clear();

            // 已安装：扫每台机器的槽位——槽位是「位置」的权威来源（机器名、类别、槽位号都在这里）。
            foreach (MachineInstance machine in _roster.Machines)
            {
                foreach (HardwareKind kind in SlotKinds)
                {
                    int slots = machine.Definition.SlotCount(kind);
                    for (int index = 0; index < slots; index++)
                    {
                        ComponentInstance component = machine.GetComponent(kind, index);
                        if (component == null)
                        {
                            continue;
                        }

                        _installed.Add(Row(component, true, machine.Name, kind, index, machine.IsComponentWorking(kind, index)));
                    }
                }
            }

            // 散件：在花名册里、但没有归属机器。
            foreach (ComponentInstance component in _roster.Components)
            {
                if (component.OwnerId.IsValid)
                {
                    continue;
                }

                _loose.Add(Row(component, false, null, null, -1, component.Enabled));
            }

            BuildLooseGroups();

            // 选中项消失了（被安装、被卸载、或搬到了别的机器）就清掉——不能继续展示一个不存在的实例。
            if (_selectedId.IsValid && !TryFind(_selectedId, out _))
            {
                _selectedId = PersistentId.Invalid;
            }

            // 组的合并键也可能整组消失（最后一件被装走），同样要清掉组选中。
            if (_selectedGroupIndex >= 0 && _selectedGroupIndex >= _looseGroups.Count)
            {
                _selectedGroupIndex = -1;
            }

            BuildDetail();

            // 两列都空时是 Empty（域接线了、只是没有组件），不是 Unavailable。
            bool empty = _loose.Count == 0 && _installed.Count == 0;
            _snapshot = new ComponentDomainSnapshot(
                empty ? UiDataState.Empty : UiDataState.Ready,
                empty ? ComponentReadModels.NoComponentReason : null,
                _loose.ToArray(),
                _installed.ToArray(),
                _looseGroups.ToArray(),
                _detail.ToArray(),
                _selectedId,
                _selectedGroupIndex);

            Changed?.Invoke(section);
        }

        private static readonly HardwareKind[] SlotKinds = { HardwareKind.Sensor, HardwareKind.Core, HardwareKind.Effector };

        /// <summary>
        /// 把散件按「类型／型号／等级」合并成显示行。
        ///
        /// 合并键的三段都必要：同一型号的不同**等级**是两种东西（一级可升二级），
        /// 按型号合并会把升级关系显示成「堆了 6 件」。
        /// 组内成员的稳定身份逐个保留——规格要求「展开后仍保留各组件实例」，
        /// 只留一个计数会让「哪一件装到哪台机器」无法回答。
        /// 顺序按首次出现的顺序，因此花名册稳定时列表不跳动。
        /// </summary>
        private void BuildLooseGroups()
        {
            _looseGroups.Clear();
            for (int i = 0; i < _loose.Count; i++)
            {
                UiComponentRow row = _loose[i];
                ComponentDisplayRow definition = row.Definition;
                HardwareKind kind = definition != null ? definition.Kind : HardwareKind.Sensor;
                int modelId = definition != null ? definition.ModelId : 0;
                int level = definition != null ? definition.Level : 0;

                int found = -1;
                for (int g = 0; g < _looseGroups.Count; g++)
                {
                    if (_looseGroups[g].Kind == kind && _looseGroups[g].ModelId == modelId && _looseGroups[g].Level == level)
                    {
                        found = g;
                        break;
                    }
                }

                if (found < 0)
                {
                    _groupMembers.Clear();
                    _groupMembers.Add(row.Id);
                    _looseGroups.Add(new UiComponentGroup(kind, modelId, level, definition, _groupMembers.ToArray()));
                }
                else
                {
                    UiComponentGroup group = _looseGroups[found];
                    var members = new List<PersistentId>(group.Members) { row.Id };
                    _looseGroups[found] = new UiComponentGroup(group.Kind, group.ModelId, group.Level, group.Definition, members);
                }
            }
        }

        private bool TryFindGroup(HardwareKind kind, int modelId, int level, out int index)
        {
            for (int g = 0; g < _looseGroups.Count; g++)
            {
                if (_looseGroups[g].Kind == kind && _looseGroups[g].ModelId == modelId && _looseGroups[g].Level == level)
                {
                    index = g;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        private UiComponentRow Row(ComponentInstance component, bool installed, string ownerName,
            HardwareKind? slotKind, int slotIndex, bool enabled)
        {
            _catalog.TryGetComponentRow(component.Definition, out ComponentDisplayRow definition);
            return new UiComponentRow(component.Id, definition, installed, ownerName, slotKind, slotIndex, enabled);
        }

        private bool TryFind(PersistentId id, out UiComponentRow row)
        {
            for (int i = 0; i < _installed.Count; i++)
            {
                if (_installed[i].Id == id)
                {
                    row = _installed[i];
                    return true;
                }
            }

            for (int i = 0; i < _loose.Count; i++)
            {
                if (_loose[i].Id == id)
                {
                    row = _loose[i];
                    return true;
                }
            }

            row = default;
            return false;
        }

        private void BuildDetail()
        {
            _detail.Clear();

            // 选中一整组散件：详情给「合并了什么」以及**逐个成员实例**（规格：展开后仍保留各实例）。
            if (_selectedGroupIndex >= 0 && _selectedGroupIndex < _looseGroups.Count)
            {
                UiComponentGroup group = _looseGroups[_selectedGroupIndex];
                _detail.Add(new UiDetailField("型号", group.Name));
                _detail.Add(new UiDetailField("类别", group.Definition != null ? group.Definition.KindLabel : "未知"));
                _detail.Add(new UiDetailField("数量", group.Count.ToString()));
                _detail.Add(new UiDetailField("实例", MemberList(group)));

                ComponentDisplayRow groupDefinition = group.Definition;
                if (groupDefinition != null)
                {
                    AppendSpecification(groupDefinition);
                }
                else
                {
                    _detail.Add(new UiDetailField("规格", "目录里没有这件组件的型号行（数据表与实例不一致）"));
                }

                return;
            }

            if (!_selectedId.IsValid)
            {
                return;
            }

            UiComponentRow row = default;
            bool found = false;
            for (int i = 0; i < _installed.Count && !found; i++)
            {
                if (_installed[i].Id == _selectedId)
                {
                    row = _installed[i];
                    found = true;
                }
            }

            for (int i = 0; i < _loose.Count && !found; i++)
            {
                if (_loose[i].Id == _selectedId)
                {
                    row = _loose[i];
                    found = true;
                }
            }

            if (!found)
            {
                return;
            }

            _detail.Add(new UiDetailField("实例", row.Id.Value.ToString()));
            _detail.Add(new UiDetailField("型号", row.Name));
            _detail.Add(new UiDetailField("位置", row.Installed ? row.Status : "散件（未安装）"));

            ComponentDisplayRow definition = row.Definition;
            if (definition == null)
            {
                // 目录里查不到型号时**不编造规格**：说清是目录缺行，而不是显示一排 0。
                _detail.Add(new UiDetailField("规格", "目录里没有这件组件的型号行（数据表与实例不一致）"));
                return;
            }

            AppendSpecification(definition);
        }

        private static string MemberList(UiComponentGroup group)
        {
            const int limit = 8;
            var text = new System.Text.StringBuilder(64);
            int shown = group.Members != null ? System.Math.Min(group.Members.Count, limit) : 0;
            for (int i = 0; i < shown; i++)
            {
                if (i > 0)
                {
                    text.Append("、");
                }

                text.Append(group.Members[i].Value);
            }

            if (group.Members != null && group.Members.Count > shown)
            {
                text.Append("…（共 ").Append(group.Members.Count).Append(" 件）");
            }

            return text.ToString();
        }

        private void AppendSpecification(ComponentDisplayRow definition)
        {
            _detail.Add(new UiDetailField("类别", definition.KindLabel));
            _detail.Add(new UiDetailField("承载", definition.AddedCapacity.ToString()));
            _detail.Add(new UiDetailField("算力", definition.ComputeCapacity.ToString()));
            _detail.Add(new UiDetailField("逻辑算力", definition.LogicCapacity.ToString()));
            _detail.Add(new UiDetailField("带行为", definition.HasBehavior ? "是" : "否"));
            _detail.Add(new UiDetailField("购入价", definition.PurchasePrice.ToString()));
            _detail.Add(new UiDetailField("回收价", definition.RecyclePrice.ToString()));
            _detail.Add(new UiDetailField("待机功耗", definition.IdlePower.ToString("0.##")));
            _detail.Add(new UiDetailField("工作功耗", definition.WorkingPower.ToString("0.##")));
            _detail.Add(new UiDetailField("可用性", AvailabilityLabel(definition.Availability)));
        }

        internal static string AvailabilityLabel(DefinitionAvailability availability) => availability switch
        {
            DefinitionAvailability.Ready => "已就绪",
            DefinitionAvailability.PendingConfiguration => "待配置",
            DefinitionAvailability.PendingResource => "待资源",
            _ => "未知",
        };
    }

    /// <summary>组件界面的统一入口：把「组件域为什么不可用」讲清楚。</summary>
    public static class ComponentReadModels
    {
        /// <summary>花名册里一件组件都没有。这是 Empty，不是 Unavailable。</summary>
        public const string NoComponentReason =
            "当前没有组件：组件由组件库创建或购入后出现在这里（经济域尚未接入，因此现在只能看到已有的组件）。";

        public static IComponentReadModel Create(AutoEraUiSession session) => Create(session, null);

        /// <summary>
        /// 建立读模型；<paramref name="catalog"/> 为 null 时从已加载的数据表取。
        ///
        /// 显式传入目录的入口是必要的：解析数据表是数据表自己的事，而调用方可能**已经有一份**
        /// （测试、以及将来的运行时目录缓存）。为了测试而让所有人都去走 `GF.DataTable`
        /// 会把「界面依赖框架全局状态」这条悄悄种回去。
        /// </summary>
        public static IComponentReadModel Create(AutoEraUiSession session, MachineCatalog catalog)
        {
            if (session == null)
            {
                return new UnavailableComponentReadModel("没有界面会话：组件数据不可用。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableComponentReadModel("组件属于某个世界里的机器，请先从主菜单进入区域。");
            }

            if (session.World.Machines == null || !session.World.Machines.IsActive)
            {
                return new UnavailableComponentReadModel("世界会话里没有可用的机器花名册：组件实例无处可查。");
            }

            if (catalog == null)
            {
                try
                {
                    catalog = MachineCatalog.FromLoadedGameData();
                }
                catch (InvalidOperationException)
                {
                    // 「数据表还没加载」在启动早期是正常状态，界面据此说明原因即可。
                    // 刻意**不吞 FormatException**：数据表里的行有问题必须响亮地失败，
                    // 而不是显示成空目录。
                    catalog = null;
                }
                catch (NullReferenceException)
                {
                    // 框架尚未就绪时 GF.DataTable 本身可能还是 null。
                    catalog = null;
                }
            }

            if (catalog == null)
            {
                return new UnavailableComponentReadModel("组件定义表尚未加载：型号名称与规格没有数据来源。");
            }

            return new RosterComponentReadModel(session.World.Machines, catalog);
        }
    }
}
