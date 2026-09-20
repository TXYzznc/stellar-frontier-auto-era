using System;
using System.Collections.Generic;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.UI
{
    /// <summary>区域域内发生变化的区域。</summary>
    public enum RegionDomainSection
    {
        /// <summary>区域里的对象集合发生变化（注册、部署、撤收）。</summary>
        Objects,

        /// <summary>当前选中对象或它的公开状态发生变化。</summary>
        Selection,
    }

    /// <summary>区域对象列表的一行。</summary>
    public readonly struct UiRegionObjectRow
    {
        public UiRegionObjectRow(PersistentId id, string name, string kind, string state, bool selected, bool isMachine)
        {
            Id = id;
            Name = name;
            Kind = kind;
            State = state;
            Selected = selected;
            IsMachine = isMachine;
        }

        public PersistentId Id { get; }
        public string Name { get; }

        /// <summary>对象类别（机器／建筑／资源点），用于分组与筛选。</summary>
        public string Kind { get; }

        /// <summary>公开状态摘要（状态文本 + 可公开的资源量）。</summary>
        public string State { get; }

        public bool Selected { get; }

        /// <summary>是否是机器：机器详情另有机器域读模型，这里只用于分组。</summary>
        public bool IsMachine { get; }
    }

    /// <summary>
    /// 区域域的只读快照。
    ///
    /// 区域域有**三种变化源**：对象集合增删（`ObjectsChanged`）、选中变化（`SelectionChanged`）、
    /// 以及单个对象自己的公开状态变化（每个 <see cref="RegionObject"/> 的 `Changed`）。
    /// 读模型把三者都收敛成两个区域（列表／选中），页面因此不需要知道有几条通知路径。
    /// </summary>
    public readonly struct RegionDomainSnapshot
    {
        public RegionDomainSnapshot(
            UiDataState state,
            string unavailableReason,
            IReadOnlyList<UiRegionObjectRow> objects,
            IReadOnlyList<UiRegionObjectRow> machines,
            IReadOnlyList<UiRegionObjectRow> sites,
            IReadOnlyList<UiDetailField> detail,
            PersistentId selectedId)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Objects = objects;
            Machines = machines;
            Sites = sites;
            Detail = detail;
            SelectedId = selectedId;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }

        /// <summary>区域里的全部对象。</summary>
        public IReadOnlyList<UiRegionObjectRow> Objects { get; }

        /// <summary>其中的机器（已部署的机器在这里出现）。</summary>
        public IReadOnlyList<UiRegionObjectRow> Machines { get; }

        /// <summary>其中的非机器对象：建筑与资源点。</summary>
        public IReadOnlyList<UiRegionObjectRow> Sites { get; }

        /// <summary>选中对象的详情字段。</summary>
        public IReadOnlyList<UiDetailField> Detail { get; }

        public PersistentId SelectedId { get; }

        public int Count => Objects == null ? 0 : Objects.Count;
        public int MachineCount => Machines == null ? 0 : Machines.Count;
        public int SiteCount => Sites == null ? 0 : Sites.Count;
        public bool HasSelection => SelectedId.IsValid && Detail != null && Detail.Count > 0;

        public static RegionDomainSnapshot Unavailable(string reason) =>
            new RegionDomainSnapshot(UiDataState.Unavailable, reason, null, null, null, null, PersistentId.Invalid);
    }

    /// <summary>
    /// 区域域读取模型：区域对象列表 + 选中对象详情。
    ///
    /// 选中由区域自己持有（`InitialRegion.Select` 带 inputBlocked 语义，界面不该绕过它），
    /// 所以本模型**只读选中**，只是把区域发出的选中变化翻译成「选中区变了」。
    /// </summary>
    public interface IRegionReadModel : IDisposable
    {
        RegionDomainSnapshot Snapshot { get; }

        event Action<RegionDomainSection> Changed;

        /// <summary>重新读取全部对象并对账订阅；区域不可用时是安全的空操作。</summary>
        void Refresh();

        /// <summary>
        /// 按 Id 取某个对象的公开详情字段；不存在或不可用时返回空。
        ///
        /// 存在的理由是「预览一个还没被选中的对象」：选择器要展示候选的公开信息，而选中详情
        /// 只对区域当前选中项有效。用它可以拿到同样的字段，而不必让读模型交出领域对象
        /// （否则界面又会绕过快照直接摸领域对象）。
        /// </summary>
        IReadOnlyList<UiDetailField> DescribeObject(PersistentId id);
    }

    /// <summary>区域不可用时的诚实空实现：只报 Unavailable，不伪造对象。</summary>
    internal sealed class UnavailableRegionReadModel : IRegionReadModel
    {
        public UnavailableRegionReadModel(string reason)
        {
            Snapshot = RegionDomainSnapshot.Unavailable(reason);
        }

        public RegionDomainSnapshot Snapshot { get; }

        public event Action<RegionDomainSection> Changed
        {
            add { }
            remove { }
        }

        public void Refresh() { }

        public IReadOnlyList<UiDetailField> DescribeObject(PersistentId id) => Array.Empty<UiDetailField>();

        public void Dispose() { }
    }

    /// <summary>基于 <see cref="InitialRegion"/> 的只读实现。</summary>
    internal sealed class RegionReadModel : IRegionReadModel
    {
        private readonly InitialRegion _region;
        private readonly List<UiRegionObjectRow> _objects = new List<UiRegionObjectRow>(32);
        private readonly List<UiRegionObjectRow> _machines = new List<UiRegionObjectRow>(16);
        private readonly List<UiRegionObjectRow> _sites = new List<UiRegionObjectRow>(32);
        private readonly List<UiDetailField> _detail = new List<UiDetailField>(12);
        private readonly List<RegionObject> _scratch = new List<RegionObject>(32);
        private readonly HashSet<RegionObject> _subscribed = new HashSet<RegionObject>();
        private RegionDomainSnapshot _snapshot;
        private PersistentId _selected = PersistentId.Invalid;
        private bool _disposed;

        public RegionReadModel(InitialRegion region)
        {
            _region = region ?? throw new ArgumentNullException(nameof(region));
            _region.ObjectsChanged += OnRegionObjectsChanged;
            _region.SelectionChanged += OnRegionSelectionChanged;
            Refresh();
        }

        public RegionDomainSnapshot Snapshot => _snapshot;

        public event Action<RegionDomainSection> Changed;

        public void Refresh()
        {
            if (_disposed || !_region.IsActive)
            {
                return;
            }

            _scratch.Clear();
            foreach (RegionObject obj in _region.Objects)
            {
                _scratch.Add(obj);
            }

            ReconcileObjectSubscriptions();

            _objects.Clear();
            _machines.Clear();
            _sites.Clear();
            for (int i = 0; i < _scratch.Count; i++)
            {
                RegionObject obj = _scratch[i];
                var row = new UiRegionObjectRow(
                    obj.Id, obj.Name, DescribeKind(obj), DescribeState(obj),
                    obj.Id == _region.SelectedId, obj.Machine != null);
                _objects.Add(row);
                if (row.IsMachine)
                {
                    _machines.Add(row);
                }
                else
                {
                    _sites.Add(row);
                }
            }

            _selected = _region.SelectedId;
            RebuildDetail();
            PublishSnapshot(RegionDomainSection.Objects);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _region.ObjectsChanged -= OnRegionObjectsChanged;
            _region.SelectionChanged -= OnRegionSelectionChanged;
            foreach (RegionObject obj in _subscribed)
            {
                obj.Changed -= OnRegionObjectChanged;
            }

            _subscribed.Clear();
            _objects.Clear();
            _machines.Clear();
            _sites.Clear();
            _detail.Clear();
            Changed = null;
        }

        private void OnRegionObjectsChanged() => Refresh();

        private void OnRegionObjectChanged(RegionObject obj) => Refresh();

        private void OnRegionSelectionChanged()
        {
            if (_disposed || !_region.IsActive)
            {
                return;
            }

            _selected = _region.SelectedId;
            RebuildDetail();
            // 快照是值类型且持有数组副本，必须重建——只发事件的话页面读到的还是上一份快照
            // （存档域踩过同一个坑：选中之后详情栏一直是空的）。
            PublishSnapshot(RegionDomainSection.Selection);
        }

        /// <summary>
        /// 对账订阅：新出现的对象订阅、已消失的对象退订，已有的不动。
        /// 必须对账而不是全量重建，否则每次刷新都会累积重复订阅。
        /// </summary>
        private void ReconcileObjectSubscriptions()
        {
            _removeScratch.Clear();
            foreach (RegionObject obj in _subscribed)
            {
                if (!_scratch.Contains(obj))
                {
                    _removeScratch.Add(obj);
                }
            }

            for (int i = 0; i < _removeScratch.Count; i++)
            {
                _removeScratch[i].Changed -= OnRegionObjectChanged;
                _subscribed.Remove(_removeScratch[i]);
            }

            for (int i = 0; i < _scratch.Count; i++)
            {
                RegionObject obj = _scratch[i];
                if (_subscribed.Add(obj))
                {
                    obj.Changed += OnRegionObjectChanged;
                }
            }
        }

        private readonly List<RegionObject> _removeScratch = new List<RegionObject>(8);

        /// <summary>重建快照并通知；每次内容变化都必须走这里，否则页面读到的是上一份快照。</summary>
        private void PublishSnapshot(RegionDomainSection section)
        {
            _snapshot = new RegionDomainSnapshot(
                _scratch.Count == 0 ? UiDataState.Empty : UiDataState.Ready,
                null,
                _objects.ToArray(),
                _machines.ToArray(),
                _sites.ToArray(),
                _detail.ToArray(),
                _selected);

            Publish(section);
        }

        private void Publish(RegionDomainSection section) => Changed?.Invoke(section);

        public IReadOnlyList<UiDetailField> DescribeObject(PersistentId id)
        {
            var fields = new List<UiDetailField>(12);
            if (!_disposed && _region.TryGet(id, out RegionObject found))
            {
                AppendDetail(found, fields);
            }

            return fields;
        }

        private void RebuildDetail()
        {
            _detail.Clear();
            if (!_selected.IsValid || !_region.TryGet(_selected, out RegionObject obj))
            {
                return;
            }

            AppendDetail(obj, _detail);
        }

        /// <summary>把对象的公开信息写进字段列表；选中详情与「按 Id 取详情」共用这一份逻辑。</summary>
        private static void AppendDetail(RegionObject obj, List<UiDetailField> target)
        {
            target.Add(new UiDetailField("名称", obj.Name));
            target.Add(new UiDetailField("类别", DescribeKind(obj)));
            target.Add(new UiDetailField("状态", obj.PublicStatus));
            target.Add(new UiDetailField("位置", DescribePosition(obj.Position)));
            target.Add(new UiDetailField("占地", AutoEraUiFormat.Count((int)Math.Round(obj.Size.x)) + " × " + AutoEraUiFormat.Count((int)Math.Round(obj.Size.y))));
            target.Add(new UiDetailField("朝向", AutoEraUiFormat.Count((int)Math.Round(obj.Yaw)) + "°"));
            target.Add(new UiDetailField("阻挡通行", obj.BlocksNavigation ? "是" : "否"));

            if (obj.ResourceIsInfinite)
            {
                target.Add(new UiDetailField("资源量", "无限"));
            }
            else if (obj.PublicResourceAmount.HasValue)
            {
                target.Add(new UiDetailField("资源量", AutoEraUiFormat.Count((int)obj.PublicResourceAmount.Value)));
            }
            else
            {
                target.Add(new UiDetailField("资源量", AutoEraUiFormat.Missing));
            }

            if (!string.IsNullOrEmpty(obj.WorkSummary))
            {
                target.Add(new UiDetailField("作业", obj.WorkSummary));
            }
        }

        private static string DescribeKind(RegionObject obj)
        {
            if (obj.Machine != null)
            {
                return "机器";
            }

            switch (obj.Kind)
            {
                case PersistentObjectKind.Building: return "建筑";
                case PersistentObjectKind.ResourcePoint: return "资源点";
                default: return obj.Kind.ToString();
            }
        }

        private static string DescribeState(RegionObject obj)
        {
            if (obj.ResourceIsInfinite)
            {
                return obj.PublicStatus + " · 无限";
            }

            return obj.PublicResourceAmount.HasValue
                ? obj.PublicStatus + " · " + AutoEraUiFormat.Count((int)obj.PublicResourceAmount.Value)
                : obj.PublicStatus;
        }

        private static string DescribePosition(UnityEngine.Vector2 position) =>
            AutoEraUiFormat.Count((int)Math.Round(position.x)) + ", " + AutoEraUiFormat.Count((int)Math.Round(position.y));
    }

    /// <summary>区域界面的统一入口：从打开参数里的会话取区域。</summary>
    public static class RegionReadModels
    {
        public static IRegionReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableRegionReadModel("没有界面会话：区域数据不可用。");
            }

            if (!session.HasWorld)
            {
                return new UnavailableRegionReadModel("区域属于某个世界，请先从主菜单进入。");
            }

            if (!session.HasRegion)
            {
                return new UnavailableRegionReadModel("区域尚未加载或已经卸载：现场数据暂不可用。");
            }

            return new RegionReadModel(session.Region);
        }

        /// <summary>
        /// 直接给区域。用于会话没有携带区域、但有别的来源注入它的场合——
        /// 例如现场 HUD 由场景（InitialRegionScene.BindHud）告知自己服务哪个区域。
        ///
        /// 刻意不叫 <c>Create</c> 重载：那样 <c>Create(null)</c> 会在两个重载之间产生歧义。
        /// </summary>
        public static IRegionReadModel CreateForRegion(InitialRegion region)
        {
            return region != null
                ? new RegionReadModel(region)
                : new UnavailableRegionReadModel("区域尚未加载或已经卸载：现场数据暂不可用。");
        }
    }
}
