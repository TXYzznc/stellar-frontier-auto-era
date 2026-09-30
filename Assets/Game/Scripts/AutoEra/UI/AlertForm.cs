using System.Collections.Generic;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 警报列表与详情（规格 15-警报列表与详情）。
    ///
    /// 绑定字段在同名的 AlertForm.Fields.cs 里（同一 partial 类）；本文件只有类逻辑。
    ///
    /// 数据来源只有一条：打开参数里的 <see cref="AutoEraUiSession"/> 的区域警报账本
    /// （<see cref="AlertReadModels.Create"/>）。界面唯一的写入口是「标记已读」——它只改阅读状态，
    /// 不解决问题（规格明确「无手动清除故障或重置真实状态按钮」）。「定位」经区域选择聚焦来源对象；
    /// 「相关详情」的跨界面跳转（机器／能源／算法／任务）属后续批次，按钮保持禁用并在详情里说明。
    /// </summary>
    public sealed partial class AlertForm : AutoEraShellFormBase
    {
        /// <summary>列表筛选模式：全部／仅活跃／仅历史（恢复）。</summary>
        private enum AlertFilter
        {
            All,
            Active,
            History,
        }

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private IAlertReadModel _alerts;
        private AutoEraUiSession _session;
        private AlertFilter _filter = AlertFilter.All;
        private readonly List<UiAlertRow> _visibleAlerts = new List<UiAlertRow>(32);

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            if (_alertsFilterButton != null) _alertsFilterButton.onClick.AddListener(OnFilterClicked);
            if (_alertsReadButton != null) _alertsReadButton.onClick.AddListener(OnReadClicked);
            if (_alertsLocateButton != null) _alertsLocateButton.onClick.AddListener(OnLocateClicked);
            // 「相关详情」跨界面跳转（机器／能源／算法／任务）尚未接入：按钮保持禁用，
            // 详情栏里用 AlertDetails 已经给出完整原因／影响／恢复条件，跳转属后续批次。
            if (_alertsDetailsButton != null) _alertsDetailsButton.interactable = false;
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);

            _session = TryGetSession(out AutoEraUiSession session) ? session : null;
            _alerts = AlertReadModels.Create(_session);
            _alerts.Changed += OnAlertsChanged;
            Render(_alerts.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseAlerts();

        protected override void OnAutoEraRecycle()
        {
            ReleaseAlerts();
            base.OnAutoEraRecycle();
        }

        /// <summary>警报域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlertDataState => _alerts?.Snapshot.State;

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void ReleaseAlerts()
        {
            if (_alerts == null) return;
            _alerts.Changed -= OnAlertsChanged;
            _alerts.Dispose();
            _alerts = null;
        }

        private void OnAlertsChanged() => Render(_alerts.Snapshot);

        private void Render(AlertDomainSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                ShowPageUnavailable(snapshot.Reason, _alertsLoadingState, _alertsEmptyState, _alertsErrorState,
                    _alertsSuccessState, _alertsDisabledState, _alertsListBody, _alertsDetailBody);
                RenderDetailRows(_alertsListTemplate, _alertsListContent, NoFields);
                RenderDetailRows(_alertsDetailTemplate, _alertsDetailContent, NoFields);
                SetActionsInteractable(false, false, false);
                return;
            }

            bool hasAlerts = snapshot.Alerts.Count > 0;
            SetState(_alertsLoadingState, false);
            SetState(_alertsEmptyState, !hasAlerts);
            SetState(_alertsErrorState, false);
            SetState(_alertsSuccessState, hasAlerts);
            SetState(_alertsDisabledState, false);

            // 列表：按筛选模式过滤（账本已排好序：先活跃后历史），行＝标题＋描述，点行选中。
            _visibleAlerts.Clear();
            for (int i = 0; i < snapshot.Alerts.Count; i++)
            {
                UiAlertRow row = snapshot.Alerts[i];
                if (_filter == AlertFilter.Active && !row.IsActive) continue;
                if (_filter == AlertFilter.History && row.IsActive) continue;
                _visibleAlerts.Add(row);
            }

            RenderListRows(_alertsListTemplate, _alertsListContent, _visibleAlerts.Count,
                (index, item) => item.Bind(index, _visibleAlerts[index].Title, _visibleAlerts[index].Describe(),
                    OnRowClicked));
            SetText(_alertsListBody, hasAlerts
                ? "活跃 " + snapshot.ActiveCount + " · 未读 " + snapshot.UnreadCount + " · "
                    + FilterLabel()
                : "当前没有警报。");

            // 详情：选中行展开完整字段（原因／影响／真实恢复条件等）。
            if (snapshot.HasSelection)
            {
                var detail = new List<UiDetailField>(12);
                AlertDetails.Build(snapshot.Selected, detail);
                RenderDetailRows(_alertsDetailTemplate, _alertsDetailContent, detail);
                SetText(_alertsDetailBody, snapshot.Selected.Title);
            }
            else
            {
                RenderDetailRows(_alertsDetailTemplate, _alertsDetailContent, NoFields);
                SetText(_alertsDetailBody, "点列表中的一条警报查看详情。");
            }

            SetActionsInteractable(true, snapshot.HasSelection, snapshot.HasSelection);
        }

        /// <summary>点一行警报 → 交给读模型按稳定 Id 选中，供详情栏展开。</summary>
        private void OnRowClicked(int index)
        {
            if (_alerts == null || index < 0 || index >= _visibleAlerts.Count) return;
            _alerts.Select(_visibleAlerts[index].Id);
        }

        /// <summary>筛选按钮：全部 → 仅活跃 → 仅历史 循环。</summary>
        private void OnFilterClicked()
        {
            _filter = _filter == AlertFilter.All ? AlertFilter.Active
                : _filter == AlertFilter.Active ? AlertFilter.History
                : AlertFilter.All;
            if (_alerts != null) Render(_alerts.Snapshot);
        }

        /// <summary>标记已读：只改阅读状态，不解决问题（读模型会经 Changed 刷新）。</summary>
        private void OnReadClicked()
        {
            if (_alerts == null || !_alerts.Snapshot.HasSelection) return;
            _alerts.MarkRead(_alerts.Snapshot.Selected.Id);
        }

        /// <summary>定位：选中来源对象并回到现场；区域口径（多储能合计）或对象已移除时说明原因。</summary>
        private void OnLocateClicked()
        {
            if (_alerts == null || !_alerts.Snapshot.HasSelection) return;

            UiAlertRow selected = _alerts.Snapshot.Selected;
            if (!selected.Source.IsValid)
            {
                SetText(_alertsDetailBody, "这是区域级警报（多储能合计口径），没有单一对象可定位。");
                return;
            }

            if (_session == null || !_session.HasRegion || !_session.Region.Select(selected.Source, false))
            {
                SetText(_alertsDetailBody, "来源对象已不在当前区域，无法定位。");
                return;
            }

            CloseSelf();
        }

        private void SetActionsInteractable(bool filter, bool read, bool locate)
        {
            if (_alertsFilterButton != null) _alertsFilterButton.interactable = filter;
            if (_alertsReadButton != null) _alertsReadButton.interactable = read;
            if (_alertsLocateButton != null) _alertsLocateButton.interactable = locate;
        }

        private string FilterLabel() =>
            _filter == AlertFilter.Active ? "仅活跃" : _filter == AlertFilter.History ? "仅历史" : "全部";

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null) text.SetText(value ?? string.Empty);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
