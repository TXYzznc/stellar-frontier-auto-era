using AutoEra.UI.Contracts;
using AutoEra.World.Region;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>基地中枢的能源详情页，按需作为独立 UIForm 打开。</summary>
    public sealed partial class BaseCommandEnergyForm : AutoEraShellFormBase
    {
        private IEnergyReadModel _energyReadModel;
        private bool _renderingEnergy;
        private bool _pendingChargingAllowed;
        private float _pendingChargeTargetRatio;
        private bool _hasPendingEnergyEdit;
        private string _energyWriteReason;
        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        public sealed class Request
        {
            public IEnergyReadModel ReadModel { get; }
            public Request(IEnergyReadModel readModel) { ReadModel = readModel; }
        }

        private bool _ownsReadModel;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(CloseSelf);
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseSelf);
            if (_hubEnergyConfigureButton != null) _hubEnergyConfigureButton.onClick.AddListener(OnEnergyConfigureClicked);
            if (_hubEnergyChargingAllowedToggle != null) _hubEnergyChargingAllowedToggle.onValueChanged.AddListener(OnEnergyChargingAllowedChanged);
            if (_hubEnergyChargeTargetSlider != null) _hubEnergyChargeTargetSlider.onValueChanged.AddListener(OnEnergyChargeTargetChanged);
            if (_hubEnergyHistoryButton != null)
            {
                _hubEnergyHistoryButton.onClick.AddListener(() =>
                    AutoEraUiNavigator.Open(this, UIViews.RecordReaderForm,
                        new AutoEraUiPageRequest(RecordReaderForm.PageEnergyHistory)));
            }
        }

        protected override void OnAutoEraOpen()
        {
            Transform page = transform.Find("Panel_Frame/Grp_PageHost/Panel_PageHubEnergy");
            if (page != null) page.gameObject.SetActive(true);
            _ownsReadModel = !TryGetRequest(out Request request) || request.ReadModel == null;
            _energyReadModel = _ownsReadModel ? EnergyReadModels.Create(SessionOrNull) : request.ReadModel;
            _energyReadModel.Changed += OnEnergyChanged;
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);
            RenderEnergy(_energyReadModel.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseEnergyReadModel();

        protected override void OnAutoEraRecycle()
        {
            ReleaseEnergyReadModel();
            base.OnAutoEraRecycle();
        }

        private void ReleaseEnergyReadModel()
        {
            if (_energyReadModel == null) return;
            _energyReadModel.Changed -= OnEnergyChanged;
            if (_ownsReadModel) _energyReadModel.Dispose();
            _energyReadModel = null;
            _ownsReadModel = false;
        }

        private void OnEnergyChanged() => RenderEnergy(_energyReadModel.Snapshot);

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

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
