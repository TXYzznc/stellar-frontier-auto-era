using System;
using System.Collections.Generic;
using AutoEra.UI.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 记录阅读（规格 15-记录：机器历史、算法历史、能源历史）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 三页共用同一个事件域读模型，只是取不同的记录分组：
    /// <list type="bullet">
    /// <item><b>机器历史</b>取任务域与执行域的事实——机器做的事以这两类记账；</item>
    /// <item><b>算法历史</b>取算法域的事实；</item>
    /// <item><b>能源历史</b>当前恒为空并说明原因：事件分类里没有能源域，能源系统本身也还没接入，
    ///       这里不拿资源域冒充能源。</item>
    /// </list>
    ///
    /// 工具栏动作（过滤、定位、诊断、跳能源）依赖未接入的过滤与定位通道，由
    /// <see cref="AutoEraShellFormBase.DisableDomainActions"/> 统一禁用；
    /// **列表行不受影响**，所以玩家仍然可以点行选中一条记录并读它的追溯链——那是日志本就能给的东西。
    /// </summary>
    public sealed partial class RecordReaderForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 机器历史、1 算法历史、2 能源历史。</summary>
        public const int PageMachineHistory = 0;
        public const int PageAlgorithmHistory = 1;
        public const int PageEnergyHistory = 2;

        private const string MachineEmptyHint = "这个区域还没有机器相关的记录。";
        private const string AlgorithmEmptyHint = "还没有算法相关的记录；算法域接入后会出现。";
        private static readonly string EnergyEmptyHint = EventReadModels.EnergyHistoryUnavailable;

        /// <summary>一页历史的全部绑定打包在一起，避免渲染方法收十几个参数。</summary>
        private readonly struct HistoryPage
        {
            public HistoryPage(
                GameObject listTemplate, RectTransform listContent, TMP_Text listBody,
                GameObject detailTemplate, RectTransform detailContent, TMP_Text detailBody,
                GameObject loading, GameObject empty, GameObject error, GameObject success, GameObject disabled,
                Func<EventDomainSnapshot, IReadOnlyList<UiEventRow>> pick, List<ulong> sequences, string emptyHint)
            {
                ListTemplate = listTemplate;
                ListContent = listContent;
                ListBody = listBody;
                DetailTemplate = detailTemplate;
                DetailContent = detailContent;
                DetailBody = detailBody;
                Loading = loading;
                Empty = empty;
                Error = error;
                Success = success;
                Disabled = disabled;
                Pick = pick;
                Sequences = sequences;
                EmptyHint = emptyHint;
            }

            public GameObject ListTemplate { get; }
            public RectTransform ListContent { get; }
            public TMP_Text ListBody { get; }
            public GameObject DetailTemplate { get; }
            public RectTransform DetailContent { get; }
            public TMP_Text DetailBody { get; }
            public GameObject Loading { get; }
            public GameObject Empty { get; }
            public GameObject Error { get; }
            public GameObject Success { get; }
            public GameObject Disabled { get; }

            /// <summary>取本页对应的记录分组。用委托而不是枚举，省掉一次 switch。</summary>
            public Func<EventDomainSnapshot, IReadOnlyList<UiEventRow>> Pick { get; }

            /// <summary>本页「列表位置 → 派发序号」的映射；行回调靠它把点击翻译成选中。</summary>
            public List<ulong> Sequences { get; }

            public string EmptyHint { get; }
        }

        private readonly List<ulong> _machineSequences = new List<ulong>(64);
        private readonly List<ulong> _algorithmSequences = new List<ulong>(32);
        private readonly List<ulong> _energySequences = new List<ulong>(8);
        private readonly List<UiDetailField> _emptyDetail = new List<UiDetailField>(0);

        private IEventReadModel _events;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageMachineHistory;
            ShowPage(_pageRoots, initialPage);
            // 本 Form 没有顶栏导航（三页由调用方按域进入），首焦点落在安全返回上。
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);

            _events = EventReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _events.Changed += OnEventSectionChanged;

            Render(_events.Snapshot);
            DisableDomainActions();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseEvents();

        protected override void OnAutoEraRecycle()
        {
            ReleaseEvents();
            base.OnAutoEraRecycle();
        }

        private void ReleaseEvents()
        {
            if (_events == null)
            {
                return;
            }

            _events.Changed -= OnEventSectionChanged;
            _events.Dispose();
            _events = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>事件域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? EventDataState => _events?.Snapshot.State;

        /// <summary>当前读到的记录总数。测试与调试用。</summary>
        public int EventRecordCount => _events?.Snapshot.Count ?? 0;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnEventSectionChanged(EventDomainSection section) => Render(_events.Snapshot);

        private void Render(EventDomainSnapshot snapshot)
        {
            RenderPage(snapshot, MachinePage());
            RenderPage(snapshot, AlgorithmPage());
            RenderPage(snapshot, EnergyPage());
        }

        private void RenderPage(EventDomainSnapshot snapshot, HistoryPage page)
        {
            bool unavailable = snapshot.State == UiDataState.Unavailable;
            IReadOnlyList<UiEventRow> records = unavailable
                ? Array.Empty<UiEventRow>()
                : page.Pick(snapshot) ?? Array.Empty<UiEventRow>();

            page.Sequences.Clear();
            int shown = 0;
            if (page.ListTemplate != null && page.ListContent != null)
            {
                // 每页的行回调捕获**本页自己的序号表**，避免三页共用回调时索引撞车。
                List<ulong> own = page.Sequences;
                shown = RenderListRows(page.ListTemplate, page.ListContent, records.Count,
                    (position, item) =>
                    {
                        own.Add(records[position].Sequence);
                        item.Bind(position, records[position].Kind + " · " + records[position].Action,
                            records[position].Describe(), click => OnRowSelected(own, click));
                    });
            }

            bool empty = !unavailable && shown == 0;
            SetState(page.Loading, false);
            SetState(page.Error, false);
            SetState(page.Empty, empty);
            SetState(page.Success, !unavailable && !empty);
            SetState(page.Disabled, unavailable);

            if (page.ListBody != null)
            {
                page.ListBody.SetText(unavailable
                    ? snapshot.UnavailableReason ?? "记录不可用"
                    : empty
                        ? page.EmptyHint
                        : "记录 " + AutoEraUiFormat.Count(shown) + " 条");
            }

            // 详情栏只在「当前选中项属于本页」时有内容，否则显示提示。
            bool selectedHere = snapshot.HasSelection && page.Sequences.Contains(snapshot.SelectedSequence);
            RenderDetailRows(page.DetailTemplate, page.DetailContent, selectedHere ? snapshot.Detail : _emptyDetail);
            if (page.DetailBody != null)
            {
                page.DetailBody.SetText(selectedHere
                    ? string.Empty
                    : unavailable
                        ? snapshot.UnavailableReason ?? "记录不可用"
                        : "未选择记录：点左侧任意一条查看它的追溯。");
            }
        }

        private HistoryPage MachinePage() => new HistoryPage(
            _machineHistoryListTemplate, _machineHistoryListContent, _machineHistoryListBody,
            _machineHistoryDetailTemplate, _machineHistoryDetailContent, _machineHistoryDetailBody,
            _machineHistoryLoadingState, _machineHistoryEmptyState, _machineHistoryErrorState,
            _machineHistorySuccessState, _machineHistoryDisabledState,
            snapshot => snapshot.MachineRecords, _machineSequences, MachineEmptyHint);

        private HistoryPage AlgorithmPage() => new HistoryPage(
            _algorithmHistoryListTemplate, _algorithmHistoryListContent, _algorithmHistoryListBody,
            _algorithmHistoryDetailTemplate, _algorithmHistoryDetailContent, _algorithmHistoryDetailBody,
            _algorithmHistoryLoadingState, _algorithmHistoryEmptyState, _algorithmHistoryErrorState,
            _algorithmHistorySuccessState, _algorithmHistoryDisabledState,
            snapshot => snapshot.AlgorithmRecords, _algorithmSequences, AlgorithmEmptyHint);

        private HistoryPage EnergyPage() => new HistoryPage(
            _energyHistoryListTemplate, _energyHistoryListContent, _energyHistoryListBody,
            _energyHistoryDetailTemplate, _energyHistoryDetailContent, _energyHistoryDetailBody,
            _energyHistoryLoadingState, _energyHistoryEmptyState, _energyHistoryErrorState,
            _energyHistorySuccessState, _energyHistoryDisabledState,
            snapshot => snapshot.EnergyRecords, _energySequences, EnergyEmptyHint);

        private void OnRowSelected(List<ulong> sequences, int listIndex)
        {
            if (_events == null || listIndex < 0 || listIndex >= sequences.Count)
            {
                return;
            }

            _events.Select(sequences[listIndex]);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
