using AutoEra.UI.Contracts;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 基地中枢（规格 04-基地中枢 五页 ＋ 05-中枢机器详情）。
    ///
    /// 结构完全由 Docs/Development/UI-PrefabLayouts/BaseCommandHubForm.contract.json 生成，
    /// 本脚本只实现意图，不自行搭建层级；绑定字段在同名的 .Fields.cs 里。
    ///
    /// 数据来源只有一条：打开参数里的 AutoEraUiSession（<see cref="AutoEraUiParamKeys.Session"/>）
    /// → 机器域读模型。界面不持有花名册，也没有旁路注入点。
    /// </summary>
    public sealed partial class BaseCommandHubForm : AutoEraShellFormBase
    {
        // 绑定字段由 BaseCommandHubForm.Fields.cs 依契约生成，与本文件同属一个 partial 类；
        // 新增节点引用请改契约后重新生成，不要在此手写字段。

        /// <summary>规格页索引，顺序即 Grp_PageHost 下的内容页顺序。</summary>
        public const int PageOverview = 0;
        public const int PageTasks = 1;
        public const int PageObjects = 2;
        public const int PageEnergy = 3;
        public const int PageRules = 4;
        public const int PageStatistics = 5;
        public const int PageRemoteMachine = 6;

        /// <summary>
        /// 顶栏导航按钮 → 规格页索引。
        /// 来源：00-共享外壳-prefab-layout.md 的 BaseCommandHubForm 导航表
        /// （总览／待处理任务／对象与系统／规则自动化／统计）。
        /// 能源系统详情与中枢机器详情是二级页，没有一级导航入口，因此不在本表内。
        /// </summary>
        private static readonly int[] NavigationPageIndex =
        {
            PageOverview, PageTasks, PageObjects, PageRules, PageStatistics
        };

        // 「对象与系统」页（阶段 1 样板页）：左索引 + 右详情 + 五个互斥状态组。

        private IMachineReadModel _machineReadModel;
        private IEventReadModel _eventReadModel;

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);

            if (_navButtons != null)
            {
                for (int i = 0; i < _navButtons.Length; i++)
                {
                    Button button = _navButtons[i];
                    if (button == null || i >= NavigationPageIndex.Length)
                    {
                        continue;
                    }

                    int page = NavigationPageIndex[i];
                    button.onClick.AddListener(() => ShowHubPage(page));
                }
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(RequestCancel);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestCancel);
            }
        }

        protected override void OnAutoEraOpen()
        {
            // 调用方可以指定落在哪一页（例如 HUD 的「待处理任务」直达任务页）；
            // 未指定时按规格页序从总览开始。
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageOverview;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null
                    ? _navButtons[0].gameObject
                    : null);

            // 服务会话由打开方（流程或父界面）经 UIParams 注入。缺会话时读取模型返回
            // Unavailable 实现而不是抛异常——界面负责说明原因，不伪造数据。
            _machineReadModel = TryGetSession(out AutoEraUiSession session)
                ? MachineReadModels.Create(session)
                : MachineReadModels.Create(null);
            _machineReadModel.Changed += OnMachineSectionChanged;

            RenderObjects(MachineDomainSection.List);
            RenderObjects(MachineDomainSection.Detail);

            // 统计页的「记录」栏来自事件域日志，所以枢纽同时持有机器域与事件域两个读模型。
            _eventReadModel = EventReadModels.Create(session);
            _eventReadModel.Changed += OnEventSectionChanged;
            RenderStatistics(_eventReadModel.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            ReleaseMachineReadModel();
            ReleaseEventReadModel();
        }

        protected override void OnAutoEraRecycle()
        {
            ReleaseMachineReadModel();
            ReleaseEventReadModel();
            base.OnAutoEraRecycle();
        }

        private void ReleaseEventReadModel()
        {
            if (_eventReadModel == null)
            {
                return;
            }

            _eventReadModel.Changed -= OnEventSectionChanged;
            _eventReadModel.Dispose();
            _eventReadModel = null;
        }

        private void OnEventSectionChanged(EventDomainSection section) => RenderStatistics(_eventReadModel.Snapshot);

        /// <summary>
        /// 统计页：记录栏用真实日志；指标栏与长期汇总栏需要统计聚合层，尚未接入，因此陈述原因。
        /// 规格把「只读历史与长期汇总」归统计、把「需要玩家处理的」归待办，这里只做前者。
        /// </summary>
        private void RenderStatistics(EventDomainSnapshot snapshot)
        {
            bool unavailable = snapshot.State == UiDataState.Unavailable;
            IReadOnlyList<UiEventRow> records = snapshot.AllRecords;

            int shown = 0;
            if (!unavailable && _hubStatsRecordsTemplate != null && _hubStatsRecordsContent != null)
            {
                // 纯展示行：传 null 回调即禁用其按钮，避免只读内容抢焦点。
                shown = RenderListRows(_hubStatsRecordsTemplate, _hubStatsRecordsContent, records.Count,
                    (position, item) => item.Bind(position,
                        records[position].Kind + " · " + records[position].Action, records[position].Describe(), null));
            }

            bool empty = !unavailable && shown == 0;
            SetState(_hubStatsLoadingState, false);
            SetState(_hubStatsErrorState, false);
            SetState(_hubStatsEmptyState, empty);
            SetState(_hubStatsSuccessState, !unavailable && !empty);
            SetState(_hubStatsDisabledState, unavailable);

            if (_hubStatsRecordsBody != null)
            {
                _hubStatsRecordsBody.SetText(unavailable
                    ? snapshot.UnavailableReason ?? "记录不可用"
                    : empty
                        ? "这个区域还没有产生任何记录。"
                        : "记录 " + AutoEraUiFormat.Count(shown) + " 条");
            }

            const string aggregationMissing = "统计聚合尚未接入：本栏列出的是原始记录，不是汇总指标。";
            if (_hubStatsMetricsBody != null) _hubStatsMetricsBody.SetText(aggregationMissing);
            if (_hubStatsValuesBody != null) _hubStatsValuesBody.SetText(aggregationMissing);
            RenderDetailRows(_hubStatsMetricsTemplate, _hubStatsMetricsContent, NoFields);
            RenderDetailRows(_hubStatsValuesTemplate, _hubStatsValuesContent, NoFields);
        }

        private void ReleaseMachineReadModel()
        {
            if (_machineReadModel == null)
            {
                return;
            }

            _machineReadModel.Changed -= OnMachineSectionChanged;
            _machineReadModel.Dispose();
            _machineReadModel = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowHubPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>
        /// 机器域当前数据状态；null 表示读模型尚未创建（打开时没有走到建立数据源那一步）。
        /// 界面自身用它解释空态，测试用它判定「会话有没有真的透传进来」。
        /// </summary>
        public UiDataState? MachineDataState => _machineReadModel?.Snapshot.State;

        /// <summary>事件域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? EventDataState => _eventReadModel?.Snapshot.State;

        /// <summary>统计页能看到的记录条数。测试与调试用。</summary>
        public int EventRecordCount => _eventReadModel?.Snapshot.Count ?? 0;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        // -------------------------------------------------- 对象与系统页（样板页）

        private void OnMachineSectionChanged(MachineDomainSection section) => RenderObjects(section);

        private void RenderObjects(MachineDomainSection section)
        {
            if (_machineReadModel == null)
            {
                return;
            }

            MachineDomainSnapshot snapshot = _machineReadModel.Snapshot;
            // 渲染失败只影响本区内容：记录异常并保持界面可用，不让一次绑定缺失把整页拖垮。
            try
            {
                ApplyObjectsState(snapshot);

                if (section == MachineDomainSection.List)
                {
                    RenderObjectsIndex(snapshot);
                }
                else
                {
                    RenderObjectsDetail(snapshot);
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        /// <summary>同一区域只激活一个状态组，且不覆盖底部返回按钮。</summary>
        private void ApplyObjectsState(MachineDomainSnapshot snapshot)
        {
            SetState(_hubObjectsLoadingState, false);
            SetState(_hubObjectsSuccessState, false);
            SetState(_hubObjectsErrorState, false);
            SetState(_hubObjectsEmptyState, snapshot.State == UiDataState.Empty);
            SetState(_hubObjectsDisabledState, snapshot.State == UiDataState.Unavailable);

            if (snapshot.State == UiDataState.Ready)
            {
                return;
            }

            if (_hubObjectsIndexBody != null)
            {
                _hubObjectsIndexBody.SetText(DescribeObjectsState(snapshot));
            }

            if (_hubObjectsDetailBody != null)
            {
                _hubObjectsDetailBody.SetText(string.Empty);
            }
        }

        private static string DescribeObjectsState(MachineDomainSnapshot snapshot) =>
            snapshot.State == UiDataState.Unavailable
                ? snapshot.UnavailableReason ?? "机器数据不可用"
                : "当前没有机器；在机器库中创建或部署后在此列出。";


        private void RenderObjectsIndex(MachineDomainSnapshot snapshot)
        {
            if (_hubObjectsIndexTemplate == null || _hubObjectsIndexContent == null)
            {
                // 契约保证这两个引用非空；为空说明预制体绑定丢失，明确报出来而不是静默不渲染。
                Debug.LogWarning(
                    "[AutoEra][BaseCommandHubForm] 索引区未渲染："
                    + $"模板={(_hubObjectsIndexTemplate == null ? "空" : "有")}、"
                    + $"容器={(_hubObjectsIndexContent == null ? "空" : "有")}、"
                    + $"数据状态={snapshot.State}、条数={snapshot.Count}");
                return;
            }

            RenderListRows(_hubObjectsIndexTemplate, _hubObjectsIndexContent, snapshot.Count,
                (position, item) => item.Bind(position, snapshot.Machines[position].Name,
                    snapshot.Machines[position].Status, OnMachineRowClicked));

            if (_hubObjectsIndexBody != null && snapshot.State == UiDataState.Ready)
            {
                _hubObjectsIndexBody.SetText("机器 " + AutoEraUiFormat.Count(snapshot.Count) + " 台");
            }
        }

        private void RenderObjectsDetail(MachineDomainSnapshot snapshot)
        {
            if (_hubObjectsDetailTemplate == null || _hubObjectsDetailContent == null)
            {
                return;
            }

            if (!snapshot.HasSelection)
            {
                // 未选中：把上一次的行收干净（传 0 也会清空池），而不是留着旧内容。
                RenderListRows(_hubObjectsDetailTemplate, _hubObjectsDetailContent, 0, null);
                if (_hubObjectsDetailBody != null)
                {
                    _hubObjectsDetailBody.SetText("未选择对象：在左侧索引中选择一行查看详情");
                }

                return;
            }

            // 详情行是纯展示，传 null 回调即禁用其按钮，避免纯展示内容抢焦点。
            RenderDetailRows(_hubObjectsDetailTemplate, _hubObjectsDetailContent, snapshot.Detail);

            if (_hubObjectsDetailBody != null)
            {
                _hubObjectsDetailBody.SetText(string.Empty);
            }
        }

        private void OnMachineRowClicked(int index)
        {
            if (_machineReadModel == null)
            {
                return;
            }

            MachineDomainSnapshot snapshot = _machineReadModel.Snapshot;
            if (snapshot.Machines == null || index < 0 || index >= snapshot.Machines.Count)
            {
                return;
            }

            _machineReadModel.Select(snapshot.Machines[index].Id);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
