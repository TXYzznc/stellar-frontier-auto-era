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

            // 能源系统详情是二级页（顶栏没有它的一级入口）：总览页的「查看能源」是它的入口。
            if (_hubOverviewEnergyButton != null)
            {
                _hubOverviewEnergyButton.onClick.AddListener(() => ShowHubPage(PageEnergy));
            }

            if (_hubEnergyConfigureButton != null)
            {
                _hubEnergyConfigureButton.onClick.AddListener(OnEnergyConfigureClicked);
            }

            if (_hubEnergyChargingAllowedToggle != null)
            {
                _hubEnergyChargingAllowedToggle.onValueChanged.AddListener(OnEnergyChargingAllowedChanged);
            }

            if (_hubEnergyChargeTargetSlider != null)
            {
                _hubEnergyChargeTargetSlider.onValueChanged.AddListener(OnEnergyChargeTargetChanged);
            }

            // 「查看能源事件」→ 记录阅读的能源停机记录页（规格 04-HubEnergy：进入 15-能源停机记录）。
            // 走导航服务而不是自己 OpenUIForm：会话要透传，返回键的顺位由 GF 的 UIGroup 处理。
            if (_hubEnergyHistoryButton != null)
            {
                _hubEnergyHistoryButton.onClick.AddListener(() =>
                    AutoEraUiNavigator.Open(this, UIViews.RecordReaderForm,
                        new AutoEraUiPageRequest(RecordReaderForm.PageEnergyHistory)));
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
            _energyReadModel.Changed += OnEnergyChanged;
            RenderEnergy(_energyReadModel.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            ReleaseMachineReadModel();
            ReleaseEventReadModel();
            ReleaseEnergyReadModel();
            _session = null;
        }

        protected override void OnAutoEraRecycle()
        {
            ReleaseMachineReadModel();
            ReleaseEventReadModel();
            ReleaseEnergyReadModel();
            _session = null;
            base.OnAutoEraRecycle();
        }

        private void OnEnergyChanged() => RenderEnergy(_energyReadModel.Snapshot);

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

        // -------------------------------------------------- 能源系统详情页（规格 04-HubEnergy）

        /// <summary>
        /// 能源页：供需概要、发电与蓄电设施、用电对象三栏。
        ///
        /// 规模来自区域电网的快照（<c>EnergyGridSnapshot</c>），也就是**结算真正用的那份数据**——
        /// 界面不自己再算一遍功率，否则「界面说 8.2、停机判定说 5.5」这种偏差迟早会出现。
        ///
        /// 三个写入口只有燃料设施的充电许可与目标储电比例（规格：仅燃料设施开放）；
        /// 发电站开关属于现场操作，本页不提供。
        /// </summary>
        private void RenderEnergy(EnergyDomainSnapshot snapshot)
        {
            bool unavailable = snapshot.State == UiDataState.Unavailable;
            bool ready = snapshot.State == UiDataState.Ready;
            bool empty = snapshot.State == UiDataState.Empty;
            string reason = snapshot.Reason ?? "能源数据不可用";

            SetState(_hubEnergyLoadingState, false);
            SetState(_hubEnergyErrorState, false);
            SetState(_hubEnergyEmptyState, empty);
            SetState(_hubEnergyDisabledState, unavailable);
            // 同「统计页」：这五个状态组是覆盖在内容区上的不透明卡片，而读一次快照不是提交，
            // 点亮 success 只会把内置占位文案「Success：—」盖在真实供需数据上。
            SetState(_hubEnergySuccessState, false);
            // 卡片会盖住三栏正文，所以原因必须写进卡片自己。
            WriteStateCard(_hubEnergyEmptyState, empty ? reason : null);
            WriteStateCard(_hubEnergyDisabledState, unavailable ? reason : null);

            RenderDetailRows(_hubEnergySummaryTemplate, _hubEnergySummaryContent,
                ready ? snapshot.Summary : NoFields);

            if (_hubEnergySummaryBody != null)
            {
                _hubEnergySummaryBody.SetText(ready ? DescribeEnergySummary(snapshot) : reason);
            }

            int facilities = RenderFacilityRows(snapshot);
            if (_hubEnergyFacilitiesBody != null)
            {
                _hubEnergyFacilitiesBody.SetText(ready
                    ? facilities == 0
                        ? "本区域还没有发电或蓄电设施。"
                        : "共 " + AutoEraUiFormat.Count(facilities) + " 台设施"
                    : reason);
            }

            int consumers = RenderConsumerRows(snapshot);
            if (_hubEnergyConsumersBody != null)
            {
                _hubEnergyConsumersBody.SetText(ready
                    ? consumers == 0
                        ? "本区域还没有已部署的用电机器。"
                        : "共 " + AutoEraUiFormat.Count(consumers) + " 个用电对象（第一版只有机器有耗电模型，建筑尚未接入）。"
                    : reason);
            }

            ApplyEnergyControls(snapshot);
        }

        private int RenderFacilityRows(EnergyDomainSnapshot snapshot)
        {
            // 不可用／空态时设施列表本来就是空的，这里照常调用即把上一次的行收干净。
            if (_hubEnergyFacilitiesTemplate == null || _hubEnergyFacilitiesContent == null) return 0;
            return RenderListRows(_hubEnergyFacilitiesTemplate, _hubEnergyFacilitiesContent, snapshot.Facilities.Count,
                (position, item) =>
                {
                    UiEnergyFacilityRow row = snapshot.Facilities[position];
                    item.Bind(position, row.Name + "（" + row.Kind + "）", row.Detail, OnFacilityRowClicked);
                });
        }

        private int RenderConsumerRows(EnergyDomainSnapshot snapshot)
        {
            if (_hubEnergyConsumersTemplate == null || _hubEnergyConsumersContent == null) return 0;
            return RenderListRows(_hubEnergyConsumersTemplate, _hubEnergyConsumersContent, snapshot.Consumers.Count,
                (position, item) =>
                {
                    UiEnergyConsumerRow row = snapshot.Consumers[position];
                    // 定位属于现场操作（要进入世界），本页不提供，因此这里不给点击回调。
                    item.Bind(position, row.Group + " · " + row.Name + "（" + row.State + "）",
                        Power(row.Power) + "　" + row.Priority
                        + (row.StoppedByShortage ? "　因缺电停机" : string.Empty), null);
                });
        }

        private void OnFacilityRowClicked(int index)
        {
            if (_energyReadModel == null || !_energyReadModel.Select(index)) return;

            // 草稿属于「上一次选中的那台设施」：换了选中对象就作废，绝不错写到新对象上。
            ClearPendingEnergyEdit();
            RenderEnergy(_energyReadModel.Snapshot);
        }

        /// <summary>概要正文：选中了什么、有无未提交的修改、上一次提交为什么没生效。</summary>
        private string DescribeEnergySummary(EnergyDomainSnapshot snapshot)
        {
            string text;
            if (!snapshot.HasSelection)
            {
                text = "在中间一列选中一台设施，即可在这里配置它的充电策略。";
            }
            else if (snapshot.Selected.SupportsChargingPolicy)
            {
                text = "已选中「" + snapshot.Selected.Name + "」：燃料发电设施开放充电许可与目标储电比例。";
            }
            else
            {
                text = "已选中「" + snapshot.Selected.Name + "」：这类设施没有充电策略设置，"
                    + "只有燃料发电设施开放充电许可与目标比例。";
            }

            if (_hasPendingEnergyEdit)
            {
                text += "　未提交的修改：允许为蓄电池充电＝" + (_pendingChargingAllowed ? "是" : "否")
                    + "、目标储电比例＝" + Percent(_pendingChargeTargetRatio)
                    + "；点「配置选中发电设施」提交。";
            }

            if (!string.IsNullOrEmpty(_energyWriteReason))
            {
                // 提交被拒时必须说出来，否则玩家只会看到「按了没反应」。
                text += "　上一次提交未生效：" + _energyWriteReason;
            }

            return text + "　估算时间按当前净功率给出，会随负载、昼夜和设施状态变化。";
        }

        /// <summary>
        /// 选中设施的充电策略控件：只有燃料设施可点，其余禁用（原因写在概要正文里）。
        ///
        /// 控件显示的是**草稿优先**——玩家改过但还没提交的值必须留在控件上，
        /// 不能让一次无关的重绘把它弹回旧值。
        /// </summary>
        private void ApplyEnergyControls(EnergyDomainSnapshot snapshot)
        {
            bool configurable = !snapshot.State.Equals(UiDataState.Unavailable) && snapshot.HasSelection
                && snapshot.Selected.SupportsChargingPolicy;

            bool allowed = _hasPendingEnergyEdit ? _pendingChargingAllowed : snapshot.Selected.ChargingAllowed;
            float ratio = _hasPendingEnergyEdit ? _pendingChargeTargetRatio : snapshot.Selected.ChargeTargetRatio;

            _renderingEnergy = true;
            try
            {
                if (_hubEnergyChargingAllowedToggle != null)
                {
                    _hubEnergyChargingAllowedToggle.interactable = configurable;
                    if (configurable) _hubEnergyChargingAllowedToggle.SetIsOnWithoutNotify(allowed);
                }

                if (_hubEnergyChargeTargetSlider != null)
                {
                    _hubEnergyChargeTargetSlider.interactable = configurable;
                    _hubEnergyChargeTargetSlider.minValue = 0f;
                    _hubEnergyChargeTargetSlider.maxValue = 1f;
                    if (configurable) _hubEnergyChargeTargetSlider.SetValueWithoutNotify(ratio);
                }

                // 没有未提交的修改时按钮不可点：那一次点击没有内容可提交，
                // 而不是「按了没反应」。
                SetInteractable(_hubEnergyConfigureButton, configurable && _hasPendingEnergyEdit);
            }
            finally
            {
                _renderingEnergy = false;
            }
        }

        private bool CanEditEnergy =>
            !_renderingEnergy && _energyReadModel != null
            && _energyReadModel.Snapshot.State != UiDataState.Unavailable
            && _energyReadModel.Snapshot.HasSelection
            && _energyReadModel.Snapshot.Selected.SupportsChargingPolicy;

        /// <summary>第一次编辑时用当前已生效的值垫底，之后以草稿为准（两个字段汇入同一份意图）。</summary>
        private void EnsurePendingEnergyDraft()
        {
            if (_hasPendingEnergyEdit) return;
            UiEnergyFacilityRow selected = _energyReadModel.Snapshot.Selected;
            _pendingChargingAllowed = selected.ChargingAllowed;
            _pendingChargeTargetRatio = selected.ChargeTargetRatio;
            _hasPendingEnergyEdit = true;
            _energyWriteReason = null;
        }

        private void ClearPendingEnergyEdit()
        {
            _hasPendingEnergyEdit = false;
            _energyWriteReason = null;
        }

        private void OnEnergyChargingAllowedChanged(bool allowed)
        {
            if (!CanEditEnergy) return;
            EnsurePendingEnergyDraft();
            _pendingChargingAllowed = allowed;
            RenderEnergy(_energyReadModel.Snapshot);
        }

        private void OnEnergyChargeTargetChanged(float ratio)
        {
            if (!CanEditEnergy) return;
            EnsurePendingEnergyDraft();
            _pendingChargeTargetRatio = ratio;
            RenderEnergy(_energyReadModel.Snapshot);
        }

        /// <summary>
        /// 提交草稿（规格：最终提交再验权限）。成功才清空草稿；失败保留草稿并写明原因，
        /// 让玩家修正后重试——不静默丢弃输入，也不由界面自己判权限。
        /// </summary>
        private void OnEnergyConfigureClicked()
        {
            if (_energyReadModel == null || !_hasPendingEnergyEdit) return;

            string reason;
            if (!_energyReadModel.SetChargingAllowed(_pendingChargingAllowed, out reason)
                || !_energyReadModel.SetChargeTargetRatio(_pendingChargeTargetRatio, out reason))
            {
                _energyWriteReason = string.IsNullOrEmpty(reason) ? "领域拒绝了这次修改。" : reason;
            }
            else
            {
                ClearPendingEnergyEdit();
            }

            RenderEnergy(_energyReadModel.Snapshot);
        }

        private static void SetInteractable(Button button, bool value)
        {
            if (button != null) button.interactable = value;
        }

        private static string Percent(float ratio) =>
            (ratio * 100f).ToString("0", System.Globalization.CultureInfo.InvariantCulture) + "%";

        private static string Power(float value) =>
            value.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + " 功率";

        private void ReleaseEnergyReadModel()
        {
            if (_energyReadModel == null)
            {
                return;
            }

            _energyReadModel.Changed -= OnEnergyChanged;
            _energyReadModel.Dispose();
            _energyReadModel = null;
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

        /// <summary>能源域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? EnergyDataState => _energyReadModel?.Snapshot.State;

        /// <summary>能源页能看到的设施台数（发电＋蓄电）。测试与调试用。</summary>
        public int EnergyFacilityCount => _energyReadModel?.Snapshot.Facilities.Count ?? 0;

        /// <summary>能源页能看到的用电对象数。测试与调试用。</summary>
        public int EnergyConsumerCount => _energyReadModel?.Snapshot.Consumers.Count ?? 0;

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
