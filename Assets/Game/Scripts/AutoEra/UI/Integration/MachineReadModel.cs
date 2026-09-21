using System;
using System.Collections.Generic;
using System.Globalization;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>机器域内发生变化的区域，使页面只刷新对应栏位，而不是整页重刷。</summary>
    public enum MachineDomainSection
    {
        /// <summary>索引列表发生变化（增删机、状态变化）。</summary>
        List,

        /// <summary>当前选中对象的详情发生变化。</summary>
        Detail,
    }

    /// <summary>索引栏的一行：稳定身份 + 名称 + 状态摘要 + 部署状态。</summary>
    public readonly struct UiMachineRow
    {
        public UiMachineRow(PersistentId id, string name, string status, bool deployed)
        {
            Id = id;
            Name = name;
            Status = status;
            Deployed = deployed;
        }

        public PersistentId Id { get; }
        public string Name { get; }
        public string Status { get; }

        /// <summary>
        /// 是否已部署到世界。机器库按它分成「库中机器」与「已部署机器」两页，
        /// 而这两页看的是同一个花名册，所以部署状态必须由行携带，而不是让页面各自去查领域对象。
        /// </summary>
        public bool Deployed { get; }
    }

    /// <summary>详情栏的一行：字段名 + 值，对上契约为 RowLabel / RowValue 两个文本。</summary>
    public readonly struct UiDetailField
    {
        public UiDetailField(string label, string value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }
        public string Value { get; }
    }

    /// <summary>
    /// 机器域的只读快照。界面只读它，不接触 <see cref="MachineRoster"/> 的内部结构。
    ///
    /// <see cref="State"/> 为 <see cref="UiDataState.Unavailable"/> 时，
    /// <see cref="UnavailableReason"/> 必须给出可展示的原因。
    ///
    /// 除列表与详情外还带**整备页三栏**（载体概况／组件整备／部署准备）：这三栏是同一份
    /// 机器快照的三个切面，让它们各自去问花名册会造出第二条数据路，而规格要求数据来源唯一。
    /// </summary>
    public readonly struct MachineDomainSnapshot
    {
        public MachineDomainSnapshot(
            UiDataState state,
            string unavailableReason,
            IReadOnlyList<UiMachineRow> machines,
            IReadOnlyList<UiDetailField> detail,
            IReadOnlyList<UiDetailField> carrier = null,
            IReadOnlyList<UiDetailField> assembly = null,
            IReadOnlyList<UiDetailField> readiness = null)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Machines = machines;
            Detail = detail;
            Carrier = carrier;
            Assembly = assembly;
            Readiness = readiness;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }
        public IReadOnlyList<UiMachineRow> Machines { get; }
        public IReadOnlyList<UiDetailField> Detail { get; }

        /// <summary>整备页·载体概况：名称／型号／等级／部署状态／容量与兼容安装位。</summary>
        public IReadOnlyList<UiDetailField> Carrier { get; }

        /// <summary>整备页·组件整备：逐个实际槽位的占用情况 ＋ 库存候选摘要 ＋ 一键卸下影响。</summary>
        public IReadOnlyList<UiDetailField> Assembly { get; }

        /// <summary>整备页·部署准备：硬件配置／算法能力需求／部署解锁条件／出售资格。</summary>
        public IReadOnlyList<UiDetailField> Readiness { get; }

        public int Count => Machines == null ? 0 : Machines.Count;
        public bool HasSelection => Detail != null && Detail.Count > 0;

        public static MachineDomainSnapshot Unavailable(string reason) =>
            new MachineDomainSnapshot(UiDataState.Unavailable, reason, null, null);
    }

    /// <summary>
    /// 机器域读取模型：快照 + 变化订阅 + 选中。
    ///
    /// 它存在的理由不是「多包一层」，而是三件界面自己做不了的事：把领域变化聚合成
    /// 「列表变了 / 详情变了」、只暴露页面需要的字段、以及在领域尚未接入时给出
    /// Unavailable 而不是抛异常。它被多个页面复用（HUD 机器页、机器库、机器整备、
    /// 选择器、本页），因此值得单独存在。
    /// </summary>
    public interface IMachineReadModel : IDisposable
    {
        MachineDomainSnapshot Snapshot { get; }

        /// <summary>按区域通知变化，页面只刷对应栏位。</summary>
        event Action<MachineDomainSection> Changed;

        PersistentId SelectedId { get; }

        bool Select(PersistentId id);

        void ClearSelection();
    }

    /// <summary>领域未接入／无世界会话时的诚实空实现：只报 Unavailable，不伪造数据。</summary>
    internal sealed class UnavailableMachineReadModel : IMachineReadModel
    {
        public UnavailableMachineReadModel(string reason)
        {
            Snapshot = MachineDomainSnapshot.Unavailable(reason);
        }

        public MachineDomainSnapshot Snapshot { get; }

        public event Action<MachineDomainSection> Changed
        {
            add { }
            remove { }
        }

        public PersistentId SelectedId => PersistentId.Invalid;

        public bool Select(PersistentId id) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }

    /// <summary>基于 <see cref="MachineRoster"/> 的只读实现。持有期间订阅领域变化，释放时退订。</summary>
    internal sealed class MachineReadModel : IMachineReadModel
    {
        private readonly MachineRoster _roster;
        private readonly MachineCatalog _catalog;
        private readonly List<UiMachineRow> _rows = new List<UiMachineRow>();
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(16);
        private readonly List<UiDetailField> _carrier = new List<UiDetailField>(8);
        private readonly List<UiDetailField> _assembly = new List<UiDetailField>(12);
        private readonly List<UiDetailField> _readiness = new List<UiDetailField>(8);
        private MachineDomainSnapshot _snapshot;
        private PersistentId _selected = PersistentId.Invalid;
        private bool _disposed;

        public MachineReadModel(MachineRoster roster, MachineCatalog catalog = null)
        {
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _catalog = catalog;
            _roster.Changed += OnRosterChanged;
            Rebuild();
        }

        public MachineDomainSnapshot Snapshot => _snapshot;

        public event Action<MachineDomainSection> Changed;

        public PersistentId SelectedId => _selected;

        public bool Select(PersistentId id)
        {
            if (_disposed) return false;
            if (!_roster.TryGet(id, out _)) return false;
            if (_selected == id) return true;
            _selected = id;
            RebuildDetail();
            Changed?.Invoke(MachineDomainSection.Detail);
            return true;
        }

        public void ClearSelection()
        {
            if (_disposed || !_selected.IsValid) return;
            _selected = PersistentId.Invalid;
            RebuildDetail();
            Changed?.Invoke(MachineDomainSection.Detail);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _roster.Changed -= OnRosterChanged;
            Changed = null;
            _rows.Clear();
            _detail.Clear();
            _carrier.Clear();
            _assembly.Clear();
            _readiness.Clear();
        }

        private void OnRosterChanged()
        {
            if (_disposed) return;

            Rebuild();
            // 选中对象可能已被移除：失效时明确清掉，不自动改选另一台。
            if (_selected.IsValid && !_roster.TryGet(_selected, out _))
            {
                _selected = PersistentId.Invalid;
            }

            RebuildDetail();
            Changed?.Invoke(MachineDomainSection.List);
        }

        private void Rebuild()
        {
            _rows.Clear();
            foreach (MachineInstance machine in _roster.Machines)
            {
                _rows.Add(new UiMachineRow(machine.Id, machine.Name, AutoEraUiFormat.MachineSummary(machine), machine.Deployed));
            }

            _rows.Sort(CompareRows);
            PublishState();
        }

        private static int CompareRows(UiMachineRow left, UiMachineRow right) =>
            left.Id.Value.CompareTo(right.Id.Value);

        private void RebuildDetail()
        {
            _detail.Clear();
            _carrier.Clear();
            _assembly.Clear();
            _readiness.Clear();
            if (_selected.IsValid && _roster.TryGet(_selected, out MachineInstance machine))
            {
                AppendDetail(machine);
                AppendCarrier(machine);
                AppendAssembly(machine);
                AppendReadiness(machine);
            }

            PublishState();
        }

        private void AppendDetail(MachineInstance machine)
        {
            _detail.Add(new UiDetailField("名称", machine.Name));
            _detail.Add(new UiDetailField("型号", machine.Definition != null ? machine.Definition.Name : AutoEraUiFormat.Missing));
            _detail.Add(new UiDetailField("序列", machine.ModelSerial.ToString(CultureInfo.InvariantCulture)));
            _detail.Add(new UiDetailField("部署", machine.Deployed ? "已部署" : "库中"));
            _detail.Add(new UiDetailField("运行", AutoEraUiFormat.RunState(machine.RequestedRunState)));
            _detail.Add(new UiDetailField("供电", machine.Powered ? "正常" : "不可用"));
            _detail.Add(new UiDetailField("连接", machine.Connected ? "已连接" : "未连接"));
            _detail.Add(new UiDetailField("完整度", AutoEraUiFormat.Integrity(machine.Integrity)));
            _detail.Add(new UiDetailField("算力", AutoEraUiFormat.Count(machine.ComputeCapacity) + " / 已用 " + AutoEraUiFormat.Count(machine.UsedCapacity)));
            _detail.Add(new UiDetailField("逻辑", AutoEraUiFormat.Count(machine.LogicCapacity)));
            _detail.Add(new UiDetailField("容量", AutoEraUiFormat.Count(machine.TotalCapacity)));
        }

        /// <summary>整备页·载体概况（规格 05-机器整备：独立实例名称、型号、等级、部署状态；容量与兼容安装位）。</summary>
        private void AppendCarrier(MachineInstance machine)
        {
            MachineDefinition definition = machine.Definition;
            _carrier.Add(new UiDetailField("实例", machine.Name));
            _carrier.Add(new UiDetailField("型号", definition != null ? definition.Name : AutoEraUiFormat.Missing));
            _carrier.Add(new UiDetailField("等级", definition != null ? definition.Level.ToString(CultureInfo.InvariantCulture) : AutoEraUiFormat.Missing));
            _carrier.Add(new UiDetailField("部署", machine.Deployed ? "已部署" : "库中（整备环境）"));
            _carrier.Add(new UiDetailField("基础容量", definition != null
                ? AutoEraUiFormat.Count(definition.BaseCapacity)
                : AutoEraUiFormat.Missing));
            _carrier.Add(new UiDetailField("总通用容量", AutoEraUiFormat.Count(machine.TotalCapacity)
                + "（基础 ＋ 已装容量效应器）"));
            _carrier.Add(new UiDetailField("兼容安装位", definition != null
                ? "传感器 " + definition.SensorSlots + " ／ 核心 " + definition.CoreSlots + " ／ 执行器 " + definition.EffectorSlots
                : AutoEraUiFormat.Missing));
        }

        /// <summary>
        /// 整备页·组件整备（规格：实际槽位及组件；库存候选摘要；一键卸下影响）。
        ///
        /// 槽位是**逐个列出**的：只显示「已装 2 件」会让人无法回答「哪一格空着、该往哪装」，
        /// 而那正是这一页要回答的问题。
        /// </summary>
        private void AppendAssembly(MachineInstance machine)
        {
            MachineDefinition definition = machine.Definition;
            if (definition == null)
            {
                _assembly.Add(new UiDetailField("槽位", AutoEraUiFormat.Missing));
                return;
            }

            int installed = 0;
            for (int k = 0; k < SlotKinds.Length; k++)
            {
                HardwareKind kind = SlotKinds[k];
                int slots = definition.SlotCount(kind);
                for (int index = 0; index < slots; index++)
                {
                    ComponentInstance component = machine.GetComponent(kind, index);
                    string slot = KindLabel(kind) + "槽 " + index;
                    if (component == null)
                    {
                        _assembly.Add(new UiDetailField(slot, "空"));
                        continue;
                    }

                    installed++;
                    ComponentDisplayRow row = null;
                    if (_catalog != null)
                    {
                        _catalog.TryGetComponentRow(component.Definition, out row);
                    }

                    string name = row != null ? row.Name : "型号 " + (component.Definition != null ? component.Definition.Id : 0);
                    string state = machine.IsComponentWorking(kind, index) ? "工作中" : "未启用";
                    _assembly.Add(new UiDetailField(slot, name + " ／ " + state));
                }
            }

            _assembly.Add(new UiDetailField("已装组件", AutoEraUiFormat.Count(installed) + " 件"));
            _assembly.Add(new UiDetailField("库存候选", LooseSummary()));
            // 「一键卸下影响」必须在动手之前说清会卸下什么、回到哪里。
            _assembly.Add(new UiDetailField("一键卸下影响", installed == 0
                ? "没有可卸下的组件。"
                : "会卸下 " + installed + " 件组件，它们各自回到组件库，机器的容量与算力随之下降。"));
        }

        /// <summary>整备页·部署准备（规格：硬件配置；算法能力需求；部署解锁条件；出售资格）。</summary>
        private void AppendReadiness(MachineInstance machine)
        {
            MachineDefinition definition = machine.Definition;
            _readiness.Add(new UiDetailField("硬件配置", definition == null
                ? AutoEraUiFormat.Missing
                : "算力 " + AutoEraUiFormat.Count(machine.ComputeCapacity)
                  + " ／ 逻辑 " + AutoEraUiFormat.Count(machine.LogicCapacity)
                  + " ／ 容量 " + AutoEraUiFormat.Count(machine.TotalCapacity)));

            // 未部署机器按设计**没有**执行上下文与算法实例（运行时随部署创建，见批次 1 的候选 A），
            // 所以这一栏不是「未接入」，而是「还没有到那一步」。
            _readiness.Add(new UiDetailField("算法能力需求",
                machine.Deployed
                    ? "已部署：算法实例与算力占用显示在算法工作台与中枢的机器详情里。"
                    : "未部署的机器还没有执行上下文与算法实例（运行时随部署创建）；激活并应用算法后本栏显示实际需求。"));

            _readiness.Add(new UiDetailField("部署解锁条件", UnlockReason));
            _readiness.Add(new UiDetailField("出售资格", SellQualification(machine)));
        }

        private static readonly HardwareKind[] SlotKinds =
            { HardwareKind.Sensor, HardwareKind.Core, HardwareKind.Effector };

        private static string KindLabel(HardwareKind kind) => kind switch
        {
            HardwareKind.Sensor => "传感器",
            HardwareKind.Core => "核心",
            HardwareKind.Effector => "执行器",
            _ => "槽",
        };

        internal const string UnlockReason =
            "解锁条件属于成长解锁域（02-系统设计/08-成长解锁与奖励），生产里还没有创建者，"
            + "因此这里不显示「已解锁／未解锁」——那会是一个编出来的资格。";

        /// <summary>库存候选摘要。花名册拿得到散件数量；具体清单在组件库页。</summary>
        private string LooseSummary()
        {
            int loose = 0;
            foreach (ComponentInstance component in _roster.Components)
            {
                if (!component.OwnerId.IsValid)
                {
                    loose++;
                }
            }

            return loose == 0
                ? "组件库里没有散件（新组件由组件库购入，经济域尚未接入）。"
                : "组件库里有 " + loose + " 件散件可安装；安装入口尚未接线（12-选择器 ＋ 17-硬件确认）。";
        }

        /// <summary>
        /// 出售资格：规格原文是「出售空载完好载体」，所以资格完全由机器自身状态判定——
        /// 未部署、无已装组件、完整度未受损。**价格不在这里**：回收价属于经济域。
        /// </summary>
        private string SellQualification(MachineInstance machine)
        {
            if (machine.Deployed)
            {
                return "不可出售：已部署的载体必须先撤收回库。";
            }

            if (AssemblyInstalledCount(machine) > 0)
            {
                return "不可出售：载体上还装着组件，请先一键卸下（规格：出售空载完好载体）。";
            }

            if (machine.Definition != null && machine.Integrity < machine.Definition.MaximumIntegrity)
            {
                return "不可出售：完整度已受损（" + AutoEraUiFormat.Integrity(machine.Integrity) + "）。";
            }

            return "资格成立：空载、完好、未部署。回收价与结算由交易域给出（尚未接入）。";
        }

        private static int AssemblyInstalledCount(MachineInstance machine)
        {
            MachineDefinition definition = machine.Definition;
            if (definition == null)
            {
                return 0;
            }

            int installed = 0;
            for (int k = 0; k < SlotKinds.Length; k++)
            {
                int slots = definition.SlotCount(SlotKinds[k]);
                for (int index = 0; index < slots; index++)
                {
                    if (machine.GetComponent(SlotKinds[k], index) != null)
                    {
                        installed++;
                    }
                }
            }

            return installed;
        }

        private void PublishState()
        {
            // 领域就绪但没有任何记录 → Empty（显示空态说明），而不是 Ready 或 Unavailable。
            Publish(_rows.Count == 0 ? UiDataState.Empty : UiDataState.Ready, null);
        }

        private void Publish(UiDataState state, string reason)
        {
            _snapshot = new MachineDomainSnapshot(state, reason, _rows, _detail, _carrier, _assembly, _readiness);
        }
    }

    /// <summary>机器域读取模型的创建入口：领域未接入时返回 Unavailable 实现，而不是 null。</summary>
    public static class MachineReadModels
    {
        public static IMachineReadModel Create(AutoEraUiSession session) => Create(session, null);

        /// <summary>
        /// 建立读模型；<paramref name="catalog"/> 为 null 时尝试从已加载数据表取
        /// （只用于把槽位里的型号显示成名字：拿不到就退化成型号编号，**不编造名字**）。
        ///
        /// 显式入口的理由与本项目其它域一致：为了测试让所有人都走 `GF.DataTable`，
        /// 会把「界面依赖框架全局状态」种回来。
        /// </summary>
        public static IMachineReadModel Create(AutoEraUiSession session, MachineCatalog catalog)
        {
            if (session == null)
            {
                return new UnavailableMachineReadModel("缺少服务会话：机器数据不可用");
            }

            if (!session.HasWorld)
            {
                return new UnavailableMachineReadModel("尚未进入世界：机器数据不可用");
            }

            if (catalog == null)
            {
                try
                {
                    catalog = MachineCatalog.FromLoadedGameData();
                }
                catch (InvalidOperationException)
                {
                    catalog = null;
                }
                catch (NullReferenceException)
                {
                    catalog = null;
                }
            }

            return new MachineReadModel(session.World.Machines, catalog);
        }
    }
}
