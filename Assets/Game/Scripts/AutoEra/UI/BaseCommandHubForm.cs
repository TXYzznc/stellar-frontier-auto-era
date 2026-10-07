using AutoEra.UI.Contracts;
using AutoEra.World.Region;
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
        private IEnergyReadModel _energyReadModel;
        private int _energyFormId;
        private GameObject _energyDisabledProxy;
        private GameObject _energySuccessProxy;
        private AutoEraUiSession _session;

        /// <summary>渲染能源页控件时置位：防止「渲染赋值」被当成玩家输入而回写成领域修改。</summary>
        private bool _renderingEnergy;

        // 「充电策略」的未提交草稿。规格在 HubEnergy.md 里写明两点：字段控件「值变化或编辑结束
        // 汇入同一用户意图」，以及「字段控制与确认按钮职责分离」，而 Btn_HubEnergyConfigure
        // 的效果是「最终提交再验权限」。合起来就是：开关与滑条只改草稿，写入由配置按钮那一次提交完成。
        private bool _pendingChargingAllowed;
        private float _pendingChargeTargetRatio;
        private bool _hasPendingEnergyEdit;

        /// <summary>上一次提交被拒的原因；成功提交后清空。展示在概要正文里，不静默失败。</summary>
        private string _energyWriteReason;

        /// <summary>「对象与系统」详情行的复用缓冲：机器域的行 + 运行时的行。</summary>
        private readonly List<UiDetailField> _detailRows = new List<UiDetailField>(24);

        /// <summary>中枢只列「已连接」（已部署到现场）的机器——库中机器是蓝图，不属中枢远程视图。</summary>
        private readonly List<UiMachineRow> _deployedMachines = new List<UiMachineRow>(16);

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            EnsureEnergyCompatibilityProxies();

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

            // 能源系统详情是二级页（顶栏没有它的一级入口）：总览页的「查看能源」是它的入口。
            if (_hubOverviewEnergyButton != null)
                _hubOverviewEnergyButton.onClick.AddListener(() => ShowHubPage(PageEnergy));
        }

        protected override void OnAutoEraOpen()
        {
            // 调用方可以指定落在哪一页（例如 HUD 的「待处理任务」直达任务页）；
            // 未指定时按规格页序从总览开始。
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageOverview;
            ShowPage(_pageRoots, initialPage == PageEnergy ? PageOverview : ToPhysicalPage(initialPage));
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
            _session = session;
            _machineReadModel.Changed += OnMachineSectionChanged;

            RenderObjects(MachineDomainSection.List);
            RenderObjects(MachineDomainSection.Detail);

            // 统计页的「记录」栏来自事件域日志，所以枢纽同时持有机器域与事件域两个读模型。
            _eventReadModel = EventReadModels.Create(session);
            _eventReadModel.Changed += OnEventSectionChanged;
            RenderStatistics(_eventReadModel.Snapshot);

            // 能源页的数据来源是区域电网的结算快照。能源没有领域推送（结算是按节拍发生的），
            // 因此这里先取一次；之后由页面在打开/切换时刷新。
            _energyReadModel = EnergyReadModels.Create(session);
            if (_energyDisabledProxy != null) _energyDisabledProxy.SetActive(session == null);
            if (_energySuccessProxy != null) _energySuccessProxy.SetActive(false);
            if (initialPage == PageEnergy) OpenEnergySubForm();
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            CloseAllSubUIForms();
            _energyFormId = 0;
            ReleaseMachineReadModel();
            ReleaseEventReadModel();
            ReleaseEnergyReadModel();
            _session = null;
        }

        protected override void OnAutoEraRecycle()
        {
            CloseAllSubUIForms();
            _energyFormId = 0;
            ReleaseMachineReadModel();
            ReleaseEventReadModel();
            ReleaseEnergyReadModel();
            _session = null;
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
            // **读取不是提交**：规格要求「success 只由权威结果触发」「未发提交不伪造 success」，
            // 而这一页只是把已有记录读出来。何况这五个状态组是**覆盖在内容区上的浮层**
            // （900×634，默认 inactive，消息面板内置占位文案），激活它就会把
            // 「Success：—」盖在真实记录上。所以有数据时不点亮任何状态组。
            SetState(_hubStatsSuccessState, false);
            SetState(_hubStatsDisabledState, unavailable);
            // 激活的状态组会盖住正文，所以原因必须写进那张卡片自己，否则玩家只看到占位文案。
            WriteStateCard(_hubStatsEmptyState, empty ? "这个区域还没有产生任何记录。" : null);
            WriteStateCard(_hubStatsDisabledState, unavailable ? snapshot.UnavailableReason : null);

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

        private void ReleaseEnergyReadModel()
        {
            if (_energyReadModel == null) return;
            _energyReadModel.Dispose();
            _energyReadModel = null;
        }

        private int ToPhysicalPage(int page)
        {
            if (page < PageEnergy) return page;
            return page - 1;
        }

        private bool OpenEnergySubForm()
        {
            if (_energyReadModel == null) return false;
            if (_energyFormId > 0 && (GF.UI.IsLoadingUIForm(_energyFormId) || GF.UI.HasUIForm(_energyFormId)))
                return true;
            UIParams parameters = UIParams.Create();
            SessionOrNull?.WriteTo(parameters);
            parameters.Set(AutoEraUiParamKeys.Request, new BaseCommandEnergyForm.Request(_energyReadModel));
            _energyFormId = OpenSubUIForm(UIViews.BaseCommandEnergyForm, 0, parameters);
            return _energyFormId > 0;
        }

        private void EnsureEnergyCompatibilityProxies()
        {
            _energyDisabledProxy = CreateStateProxy("Grp_HubEnergyDisabledState", "能源页不可用：当前没有区域会话。");
            _energySuccessProxy = CreateStateProxy("Grp_HubEnergySuccessState", string.Empty);
            _energyDisabledProxy.SetActive(false);
            _energySuccessProxy.SetActive(false);
        }

        private GameObject CreateStateProxy(string name, string text)
        {
            GameObject state = new GameObject(name, typeof(RectTransform));
            state.transform.SetParent(transform, false);
            GameObject body = new GameObject("Txt_StateBody", typeof(RectTransform), typeof(CanvasRenderer), typeof(TMPro.TextMeshProUGUI));
            body.transform.SetParent(state.transform, false);
            TMPro.TextMeshProUGUI label = body.GetComponent<TMPro.TextMeshProUGUI>();
            label.text = text;
            label.raycastTarget = false;
            return state;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowHubPage(int page)
        {
            if (page == PageEnergy)
            {
                return OpenEnergySubForm();
            }

            return ShowPage(_pageRoots, ToPhysicalPage(page));
        }

        /// <summary>
        /// 机器域当前数据状态；null 表示读模型尚未创建（打开时没有走到建立数据源那一步）。
        /// 界面自身用它解释空态，测试用它判定「会话有没有真的透传进来」。
        /// </summary>
        public UiDataState? MachineDataState => _machineReadModel?.Snapshot.State;

        /// <summary>事件域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? EventDataState => _eventReadModel?.Snapshot.State;

        /// <summary>能源域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? EnergyDataState => _energyReadModel?.Snapshot.State;

        /// <summary>能源页能看到的设施台数（发电＋蓄电）。测试与调试用。</summary>
        public int EnergyFacilityCount => _energyReadModel?.Snapshot.Facilities.Count ?? 0;

        /// <summary>能源页能看到的用电对象数。测试与调试用。</summary>
        public int EnergyConsumerCount => _energyReadModel?.Snapshot.Consumers.Count ?? 0;

        private BaseCommandEnergyForm EnergyFormOrNull
        {
            get
            {
                if (_energyFormId <= 0 || !GF.UI.HasUIForm(_energyFormId)) return null;
                return GF.UI.GetUIForm(_energyFormId)?.Logic as BaseCommandEnergyForm;
            }
        }

        public Button HubEnergyConfigureButton => EnergyFormOrNull?.HubEnergyConfigureButton;
        public Toggle HubEnergyChargingAllowedToggle => EnergyFormOrNull?.HubEnergyChargingAllowedToggle;
        public Slider HubEnergyChargeTargetSlider => EnergyFormOrNull?.HubEnergyChargeTargetSlider;
        public Button HubEnergyHistoryButton => EnergyFormOrNull?.HubEnergyHistoryButton;
        public RectTransform HubEnergyFacilitiesContent => EnergyFormOrNull?.HubEnergyFacilitiesContent;
        public GameObject HubEnergyEmptyState => EnergyFormOrNull?.HubEnergyEmptyState;
        public GameObject HubEnergyDisabledState => EnergyFormOrNull?.HubEnergyDisabledState ?? _energyDisabledProxy;
        public GameObject HubEnergySuccessState => EnergyFormOrNull?.HubEnergySuccessState ?? _energySuccessProxy;

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

            // 中枢只列已部署到现场的机器（DoD「只显示已连接机器」）：库中机器是蓝图，
            // 不是中枢要远程管理的对象；「库中」与「已部署」的分页属于机器库（MachineLibraryForm）。
            _deployedMachines.Clear();
            if (snapshot.Machines != null)
            {
                for (int i = 0; i < snapshot.Machines.Count; i++)
                {
                    if (snapshot.Machines[i].Deployed) _deployedMachines.Add(snapshot.Machines[i]);
                }
            }

            RenderListRows(_hubObjectsIndexTemplate, _hubObjectsIndexContent, _deployedMachines.Count,
                (position, item) => item.Bind(position, _deployedMachines[position].Name,
                    _deployedMachines[position].Status, OnMachineRowClicked));

            if (_hubObjectsIndexBody != null && snapshot.State == UiDataState.Ready)
            {
                _hubObjectsIndexBody.SetText(_deployedMachines.Count == 0
                    ? "还没有已部署到现场的机器。"
                    : "机器 " + AutoEraUiFormat.Count(_deployedMachines.Count) + " 台（已连接）");
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
            // 机器域的行之后再追加**运行时状态**（任务队列与算力占用）：那是枢纽这一页
            // 在接线后真正多出来的信息，而长期统计聚合仍然没有数据来源（统计页照旧陈述原因）。
            _detailRows.Clear();
            if (snapshot.Detail != null)
            {
                _detailRows.AddRange(snapshot.Detail);
            }

            AppendRuntimeDetail();
            RenderDetailRows(_hubObjectsDetailTemplate, _hubObjectsDetailContent, _detailRows);

            if (_hubObjectsDetailBody != null)
            {
                _hubObjectsDetailBody.SetText(string.Empty);
            }
        }

        /// <summary>
        /// 追加区域运行时的真实状态（任务队列／算力占用／导航／算法实例）。
        ///
        /// 每一层缺失都给**可辨原因**，而不是留空：没有运行时注册表、机器没有运行时、
        /// 以及运行时存在这三种情况对玩家的含义完全不同（「区域没接线」「这台机器没接上」
        /// 「一切就绪」）。枢纽不创建任何东西，只是把已有的事读出来。
        /// </summary>
        private void AppendRuntimeDetail()
        {
            if (_machineReadModel == null)
            {
                return;
            }

            if (_session == null || !_session.HasMachineRuntimes)
            {
                _detailRows.Add(new UiDetailField("运行时",
                    "区域机器运行时尚未建立：进入区域并部署一台机器后，这里会显示它的任务与算力占用。"));
                return;
            }

            if (!_session.MachineRuntimes.TryGet(_machineReadModel.SelectedId, out RegionMachineRuntime runtime))
            {
                _detailRows.Add(new UiDetailField("运行时",
                    "该机器还没有运行时（可能尚未部署，或区域刚重建）——领域状态不受影响。"));
                return;
            }

            _detailRows.Add(new UiDetailField("运行时", "已建立"));
            _detailRows.Add(new UiDetailField("导航", runtime.HasNavigation
                ? "已绑定"
                : runtime.IsNavigationDegraded
                    ? "降级：" + runtime.NavigationUnavailableReason
                    : "不需要（不可移动）"));
            _detailRows.Add(new UiDetailField("任务队列",
                "等待 " + runtime.Context.Tasks.WaitingCount));
            _detailRows.Add(new UiDetailField("算力占用",
                "使用 " + runtime.Context.Compute.Used + " ／ 等待 " + runtime.Context.Compute.WaitingCount
                + " ／ 逻辑上限 " + runtime.Context.Compute.LogicCapacity));
            _detailRows.Add(new UiDetailField("算法实例", runtime.Instances == null
                ? "—"
                : AutoEraUiFormat.Count(runtime.Instances.ListInstances().Length) + " 个（在算法工作台查看）"));
            _detailRows.Add(new UiDetailField("传感器", runtime.Context.Sensors != null ? "已建立" : "—"));
        }

        private void OnMachineRowClicked(int index)
        {
            if (_machineReadModel == null)
            {
                return;
            }

            // 索引对应中枢过滤后的「已连接」列表，不是读模型的原始全量列表。
            if (index < 0 || index >= _deployedMachines.Count)
            {
                return;
            }

            _machineReadModel.Select(_deployedMachines[index].Id);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
