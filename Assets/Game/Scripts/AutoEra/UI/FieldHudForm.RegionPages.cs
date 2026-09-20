using TMPro;
using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>
    /// 现场 HUD 的区域内容页：四个资源观察页（农田／林地／矿脉／水源）与八个建筑类型页。
    ///
    /// 单独放一个 partial 文件，因为它们形状一致、数量多（12 页），塞进主文件会把
    /// 「HUD 自身的行为」淹掉。绑定字段仍在 FieldHudForm.Fields.cs 里（同一个 partial 类）。
    ///
    /// 数据分配原则（决定每栏显示什么）：
    /// <list type="bullet">
    /// <item><b>公开状态栏</b>用区域域详情——区域对象的公开状态、资源量、位置、占地正是这些页要展示的；</item>
    /// <item><b>传感器／记录栏</b>陈述原因：传感采样与事件记录都还没有接进生产运行路径；</item>
    /// <item><b>建筑页的领域栏</b>（生产／储能／施工进度…）同理陈述原因。只有「建筑总览」的身份栏
    ///       用区域详情——它本来就是身份信息，其余页的第一栏是具体领域数据，拿区域详情去填并不诚实。</item>
    /// </list>
    /// </summary>
    public sealed partial class FieldHudForm
    {
        private const string SensorColumnMissing = "传感器与采样尚未接入运行路径：读数与事件暂不可用。";
        private const string RecordColumnMissing = "记录阅读属于事件域，尚未接入。";

        private void RenderRegionPages(RegionDomainSnapshot snapshot)
        {
            // 四个资源观察页：公开状态栏用区域详情，传感器与记录栏陈述原因。
            RenderSitePage(snapshot,
                _farmPublicTemplate, _farmPublicContent, _farmPublicBody, _farmSensorBody, _farmRecordBody,
                _farmLoadingState, _farmEmptyState, _farmErrorState, _farmSuccessState, _farmDisabledState);

            RenderSitePage(snapshot,
                _forestPublicTemplate, _forestPublicContent, _forestPublicBody, _forestSensorBody, _forestRecordBody,
                _forestLoadingState, _forestEmptyState, _forestErrorState, _forestSuccessState, _forestDisabledState);

            RenderSitePage(snapshot,
                _mineralPublicTemplate, _mineralPublicContent, _mineralPublicBody, _mineralSensorBody, _mineralRecordBody,
                _mineralLoadingState, _mineralEmptyState, _mineralErrorState, _mineralSuccessState, _mineralDisabledState);

            RenderSitePage(snapshot,
                _waterPublicTemplate, _waterPublicContent, _waterPublicBody, _waterSensorBody, _waterRecordBody,
                _waterLoadingState, _waterEmptyState, _waterErrorState, _waterSuccessState, _waterDisabledState);

            // 建筑总览：身份栏用区域详情。
            RenderSitePage(snapshot,
                _buildingOverviewIdentityTemplate, _buildingOverviewIdentityContent, _buildingOverviewIdentityBody,
                _buildingOverviewOperationBody, null,
                _buildingOverviewLoadingState, _buildingOverviewEmptyState, _buildingOverviewErrorState,
                _buildingOverviewSuccessState, _buildingOverviewDisabledState);

            // 其余七个建筑页的第一栏是具体领域数据（生产／储能／施工进度…），没有数据源，
            // 因此两栏都陈述原因，而不是拿区域详情去充数。
            RenderBuildingPage(snapshot, "生产批次",
                _pumpProductionBody, _pumpBlockBody,
                _pumpLoadingState, _pumpEmptyState, _pumpErrorState, _pumpSuccessState, _pumpDisabledState);

            RenderBuildingPage(snapshot, "发电与充能",
                _generatorPowerBody, _generatorChargingBody,
                _generatorLoadingState, _generatorEmptyState, _generatorErrorState, _generatorSuccessState, _generatorDisabledState);

            RenderBuildingPage(snapshot, "光伏与回充",
                _solarPowerBody, _solarRecoveryBody,
                _solarLoadingState, _solarEmptyState, _solarErrorState, _solarSuccessState, _solarDisabledState);

            RenderBuildingPage(snapshot, "储能与估算",
                _batteryStorageBody, _batteryEstimateBody,
                _batteryLoadingState, _batteryEmptyState, _batteryErrorState, _batterySuccessState, _batteryDisabledState);

            RenderBuildingPage(snapshot, "库存容量与近期变动",
                _warehouseBuildingCapacityBody, _warehouseBuildingRecentBody,
                _warehouseBuildingLoadingState, _warehouseBuildingEmptyState, _warehouseBuildingErrorState,
                _warehouseBuildingSuccessState, _warehouseBuildingDisabledState);

            RenderBuildingPage(snapshot, "施工进度与成本",
                _constructionProgressBody, _constructionCostBody,
                _constructionLoadingState, _constructionEmptyState, _constructionErrorState,
                _constructionSuccessState, _constructionDisabledState);

            RenderBuildingPage(snapshot, "输送状态与连接",
                _conveyorStateBody, _conveyorLinkBody,
                _conveyorLoadingState, _conveyorEmptyState, _conveyorErrorState,
                _conveyorSuccessState, _conveyorDisabledState);

            // 传感器记录页整体属于传感域。
            ShowPageUnavailable(SensorColumnMissing,
                _sensorRecordsLoadingState, _sensorRecordsEmptyState, _sensorRecordsErrorState,
                _sensorRecordsSuccessState, _sensorRecordsDisabledState,
                _sensorRecordsSamplesBody, _sensorRecordsEventsBody);
        }

        /// <summary>资源观察页：公开状态栏吃区域详情，另两栏陈述原因。</summary>
        private void RenderSitePage(
            RegionDomainSnapshot snapshot,
            GameObject publicTemplate,
            RectTransform publicContent,
            TMP_Text publicBody,
            TMP_Text sensorBody,
            TMP_Text recordBody,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState)
        {
            ApplyRegionPageState(snapshot, loadingState, emptyState, errorState, successState, disabledState);
            RenderDetailRows(publicTemplate, publicContent, snapshot.HasSelection ? snapshot.Detail : NoFields);

            SetRegionBody(publicBody, snapshot,
                "未选择对象：在区域中选择一个资源点或建筑后这里会显示它的公开状态。");

            SetText(sensorBody, SensorColumnMissing);
            SetText(recordBody, RecordColumnMissing);
        }

        /// <summary>建筑页：两栏都是特定领域数据，因此两栏都陈述原因。</summary>
        private void RenderBuildingPage(
            RegionDomainSnapshot snapshot,
            string domainName,
            TMP_Text firstBody,
            TMP_Text secondBody,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState)
        {
            ApplyRegionPageState(snapshot, loadingState, emptyState, errorState, successState, disabledState);

            string reason = domainName + "尚未接入：本页暂无可显示内容。";
            SetRegionBody(firstBody, snapshot, reason);
            SetText(secondBody, reason);
        }

        /// <summary>
        /// 页级状态：区域不可用 → Disabled；没有选中对象 → Empty；有选中对象 → Success。
        /// 与机器页同一套判据，保证现场所有内容页的状态语义一致。
        /// </summary>
        private void ApplyRegionPageState(
            RegionDomainSnapshot snapshot,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState)
        {
            bool unavailable = snapshot.State == UiDataState.Unavailable;
            bool hasSelection = snapshot.HasSelection;

            SetState(loadingState, false);
            SetState(errorState, false);
            SetState(emptyState, !unavailable && !hasSelection);
            SetState(successState, !unavailable && hasSelection);
            SetState(disabledState, unavailable);
        }

        private static void SetRegionBody(TMP_Text body, RegionDomainSnapshot snapshot, string hasSelectionText)
        {
            if (body == null)
            {
                return;
            }

            if (snapshot.State == UiDataState.Unavailable)
            {
                body.SetText(snapshot.UnavailableReason ?? "区域数据不可用");
                return;
            }

            body.SetText(snapshot.HasSelection ? string.Empty : hasSelectionText);
        }

        private static void SetText(TMP_Text body, string text)
        {
            if (body != null)
            {
                body.SetText(text);
            }
        }
    }
}
