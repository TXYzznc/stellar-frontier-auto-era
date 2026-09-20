using System;
using System.Collections.Generic;
using AutoEra.Save;

namespace AutoEra.UI
{
    /// <summary>存档域内发生变化的区域，使页面只刷新对应栏位，而不是整页重刷。</summary>
    public enum SaveSlotDomainSection
    {
        /// <summary>槽位列表发生变化（新建、删除、损坏）。</summary>
        List,

        /// <summary>当前选中槽位的详情发生变化。</summary>
        Detail,
    }

    /// <summary>存档列表的一行：槽位号 + 摘要 + 时间 + 占用状态。</summary>
    public readonly struct UiSaveSlotRow
    {
        public UiSaveSlotRow(int index, string title, string summary, string worldTime, bool occupied, bool needsRecovery)
        {
            Index = index;
            Title = title;
            Summary = summary;
            WorldTime = worldTime;
            Occupied = occupied;
            NeedsRecovery = needsRecovery;
        }

        public int Index { get; }

        /// <summary>槽位标题，例如「槽位 1」。</summary>
        public string Title { get; }

        /// <summary>存档摘要；空槽为「空槽位」。</summary>
        public string Summary { get; }

        /// <summary>世界时间快照；空槽为 <see cref="AutoEraUiFormat.Missing"/>。</summary>
        public string WorldTime { get; }

        /// <summary>该槽位是否存在存档文件。</summary>
        public bool Occupied { get; }

        /// <summary>存档存在但读不出来（损坏或格式过新），需要走恢复流程。</summary>
        public bool NeedsRecovery { get; }
    }

    /// <summary>
    /// 存档域的只读快照。
    ///
    /// 与机器域不同，存档域**没有领域变化事件**：它背后是文件系统，变化只由玩家操作引起，
    /// 因此这里只有显式 <see cref="ISaveSlotReadModel.Refresh"/>，不假装有推送。
    /// </summary>
    public readonly struct SaveSlotDomainSnapshot
    {
        public SaveSlotDomainSnapshot(
            UiDataState state,
            string unavailableReason,
            IReadOnlyList<UiSaveSlotRow> slots,
            IReadOnlyList<UiDetailField> metadata,
            IReadOnlyList<UiDetailField> health,
            int selectedIndex)
        {
            State = state;
            UnavailableReason = unavailableReason;
            Slots = slots;
            Metadata = metadata;
            Health = health;
            SelectedIndex = selectedIndex;
        }

        public UiDataState State { get; }
        public string UnavailableReason { get; }
        public IReadOnlyList<UiSaveSlotRow> Slots { get; }

        /// <summary>详情页的元数据栏（槽位、摘要、世界时间、格式版本）。</summary>
        public IReadOnlyList<UiDetailField> Metadata { get; }

        /// <summary>详情页的健康栏（文件状态、备份、可恢复性）。</summary>
        public IReadOnlyList<UiDetailField> Health { get; }

        public int SelectedIndex { get; }

        public int Count => Slots == null ? 0 : Slots.Count;
        public bool HasSelection => SelectedIndex >= 0 && SelectedIndex < Count;

        /// <summary>存在「有文件但读不出来」的槽位：界面据此提示走恢复流程而不是当成空槽。</summary>
        public bool HasRecoverableSlot
        {
            get
            {
                if (Slots == null)
                {
                    return false;
                }

                for (int i = 0; i < Slots.Count; i++)
                {
                    if (Slots[i].NeedsRecovery)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static SaveSlotDomainSnapshot Unavailable(string reason) =>
            new SaveSlotDomainSnapshot(UiDataState.Unavailable, reason, null, null, null, -1);
    }

    /// <summary>
    /// 存档域读取模型：快照 + 选中 + 显式刷新。
    ///
    /// 它被启动存档三页共用（列表页、详情页、新建进度页都要看「哪个槽位被选中、它是空的还是坏的」），
    /// 所以值得单独存在，而不是让每个页面各自去读文件。
    /// </summary>
    public interface ISaveSlotReadModel : IDisposable
    {
        SaveSlotDomainSnapshot Snapshot { get; }

        /// <summary>按区域通知变化；没有领域推送，只在显式操作后触发。</summary>
        event Action<SaveSlotDomainSection> Changed;

        int SelectedIndex { get; }

        /// <summary>重新读取全部槽位（打开界面、删除存档后调用）。</summary>
        void Refresh();

        bool Select(int slotIndex);

        void ClearSelection();
    }

    /// <summary>存档服务不可用时的诚实空实现：只报 Unavailable，不伪造槽位。</summary>
    internal sealed class UnavailableSaveSlotReadModel : ISaveSlotReadModel
    {
        public UnavailableSaveSlotReadModel(string reason)
        {
            Snapshot = SaveSlotDomainSnapshot.Unavailable(reason);
        }

        public SaveSlotDomainSnapshot Snapshot { get; }

        public event Action<SaveSlotDomainSection> Changed
        {
            add { }
            remove { }
        }

        public int SelectedIndex => -1;

        public void Refresh() { }

        public bool Select(int slotIndex) => false;

        public void ClearSelection() { }

        public void Dispose() { }
    }

    /// <summary>基于 <see cref="SaveSlotService"/> 的只读实现。</summary>
    internal sealed class SaveSlotReadModel : ISaveSlotReadModel
    {
        private readonly SaveSlotService _service;
        private readonly List<UiSaveSlotRow> _rows = new List<UiSaveSlotRow>(SaveSlotService.SlotCount);
        private readonly List<UiDetailField> _metadata = new List<UiDetailField>(8);
        private readonly List<UiDetailField> _health = new List<UiDetailField>(4);
        private SaveSlotDomainSnapshot _snapshot;
        private UiDataState _state = UiDataState.Empty;
        private int _selected = -1;
        private bool _disposed;

        public SaveSlotReadModel(SaveSlotService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            Refresh();
        }

        public SaveSlotDomainSnapshot Snapshot => _snapshot;

        public event Action<SaveSlotDomainSection> Changed;

        public int SelectedIndex => _selected;

        public void Refresh()
        {
            if (_disposed)
            {
                return;
            }

            _rows.Clear();
            bool anyOccupied = false;
            for (int slot = 0; slot < SaveSlotService.SlotCount; slot++)
            {
                SaveSlotReadResult result = _service.Read(slot);
                bool occupied = result.Status != SaveSlotReadStatus.Empty;
                anyOccupied |= occupied;
                _rows.Add(BuildRow(slot, result, occupied));
            }

            if (_selected >= SaveSlotService.SlotCount)
            {
                _selected = -1;
            }

            _state = anyOccupied ? UiDataState.Ready : UiDataState.Empty;
            RebuildDetail();
            PublishSnapshot(ListSection);
        }

        public bool Select(int slotIndex)
        {
            if (_disposed || !SaveSlotService.IsValidSlotIndex(slotIndex))
            {
                return false;
            }

            if (_selected == slotIndex)
            {
                return true;
            }

            _selected = slotIndex;
            RebuildDetail();
            PublishSnapshot(DetailSection);
            return true;
        }

        public void ClearSelection()
        {
            if (_disposed || _selected < 0)
            {
                return;
            }

            _selected = -1;
            RebuildDetail();
            PublishSnapshot(DetailSection);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Changed = null;
            _rows.Clear();
            _metadata.Clear();
            _health.Clear();
        }

        /// <summary>
        /// 重建快照并通知。
        ///
        /// 快照是值类型且持有数组副本，所以每次变化都必须重新构造：只改 _metadata/_health
        /// 而沿用旧快照的话，页面读到的仍是上一份数据（实测踩过：选中槽位后详情栏一直空着）。
        /// </summary>
        private void PublishSnapshot(SaveSlotDomainSection section)
        {
            _snapshot = new SaveSlotDomainSnapshot(
                _state,
                null,
                _rows.ToArray(),
                _metadata.ToArray(),
                _health.ToArray(),
                _selected);

            Publish(section);
        }

        private void Publish(SaveSlotDomainSection section) => Changed?.Invoke(section);

        private static UiSaveSlotRow BuildRow(int slot, SaveSlotReadResult result, bool occupied)
        {
            string title = "槽位 " + (slot + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
            switch (result.Status)
            {
                case SaveSlotReadStatus.Success:
                    return new UiSaveSlotRow(slot, title, result.Record.Summary, AutoEraUiFormat.WorldTime(result.Record.WorldTimeMilliseconds), true, false);
                case SaveSlotReadStatus.Corrupt:
                    return new UiSaveSlotRow(slot, title, "存档损坏，可尝试恢复", AutoEraUiFormat.Missing, true, true);
                case SaveSlotReadStatus.NewerVersion:
                    return new UiSaveSlotRow(slot, title, "存档版本高于当前版本", AutoEraUiFormat.Missing, true, true);
                default:
                    return new UiSaveSlotRow(slot, title, "空槽位", AutoEraUiFormat.Missing, false, false);
            }
        }

        private void RebuildDetail()
        {
            _metadata.Clear();
            _health.Clear();

            if (_selected < 0 || _selected >= _rows.Count)
            {
                return;
            }

            UiSaveSlotRow row = _rows[_selected];
            SaveSlotReadResult result = _service.Read(_selected);

            _metadata.Add(new UiDetailField("槽位", row.Title));
            _metadata.Add(new UiDetailField("摘要", row.Occupied ? row.Summary : AutoEraUiFormat.Missing));
            _metadata.Add(new UiDetailField("世界时间", row.WorldTime));
            _metadata.Add(new UiDetailField("存档格式", result.IsSuccess
                ? "版本 " + result.Record.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : AutoEraUiFormat.Missing));

            _health.Add(new UiDetailField("文件", DescribeFileState(result.Status)));
            _health.Add(new UiDetailField("备份", result.Status == SaveSlotReadStatus.Success ? "主文件可用" : "可能需从备份恢复"));
            _health.Add(new UiDetailField("可读取", result.IsSuccess ? "是" : "否"));
        }

        private static string DescribeFileState(SaveSlotReadStatus status)
        {
            switch (status)
            {
                case SaveSlotReadStatus.Success: return "正常";
                case SaveSlotReadStatus.Empty: return "无存档";
                case SaveSlotReadStatus.Corrupt: return "损坏";
                case SaveSlotReadStatus.NewerVersion: return "版本过新";
                default: return AutoEraUiFormat.Missing;
            }
        }

        private static SaveSlotDomainSection ListSection => SaveSlotDomainSection.List;

        private static SaveSlotDomainSection DetailSection => SaveSlotDomainSection.Detail;
    }

    /// <summary>页面侧入口：从打开参数里的会话取存档服务。</summary>
    public static class SaveSlotReadModels
    {
        public static ISaveSlotReadModel Create(AutoEraUiSession session)
        {
            if (session == null)
            {
                return new UnavailableSaveSlotReadModel("没有界面会话：本页无法读取存档列表。");
            }

            if (session.SaveSlots == null)
            {
                return new UnavailableSaveSlotReadModel("存档服务未接入：本页无法读取存档列表。");
            }

            return new SaveSlotReadModel(session.SaveSlots);
        }
    }
}
