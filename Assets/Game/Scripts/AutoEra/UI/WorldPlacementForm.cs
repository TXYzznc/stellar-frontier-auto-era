using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 世界放置（规格 07-世界放置：建造放置、机器部署、世界绑定三页）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 三页都在描述「把一个东西放到世界里的某个位置」：选目标、看校验、看提示、确认。
    /// 这些都需要**放置事务**——它要同时改区域布局、机器绑定与世界状态，而生产运行路径
    /// 还没有这条通道（`InitialRegion.CanPlace` 只做校验，真正的落位流程没有调用者）。
    /// 因此本页呈现整页不可用：三页都写明原因，业务按钮全部禁用。
    ///
    /// 取消类按钮被安全出口规则保留——一个尚未接入的域不该把玩家关在里面。
    /// 本 Form 没有 Btn_FormBack（全屏根），所以取消是这里唯一的出口。
    /// </summary>
    public sealed partial class WorldPlacementForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 建造放置、1 机器部署、2 世界绑定。</summary>
        public const int PageBuildPlacement = 0;
        public const int PageMachineDeployment = 1;
        public const int PageWorldBinding = 2;

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private const string PlacementMissing =
            "世界放置事务尚未接入运行路径：区域布局、机器绑定与世界状态的落位流程还没有调用者，"
            + "因此选择目标、校验与确认暂不可用。";

        private IRegionReadModel _region;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            // 取消是唯一出口：关闭本页等同于取消本次放置（不写任何世界状态，因此总是安全）。
            foreach (Button button in GetComponentsInChildren<Button>(true))
            {
                if (button != null && button.name.EndsWith("Cancel", System.StringComparison.Ordinal))
                {
                    button.onClick.AddListener(CloseSelf);
                }
            }
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageBuildPlacement;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(null, _firstInteractable);

            _region = RegionReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _region.Changed += OnRegionSectionChanged;

            Render(_region.Snapshot);
            DisableDomainActions();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseRegion();

        protected override void OnAutoEraRecycle()
        {
            ReleaseRegion();
            base.OnAutoEraRecycle();
        }

        private void ReleaseRegion()
        {
            if (_region == null)
            {
                return;
            }

            _region.Changed -= OnRegionSectionChanged;
            _region.Dispose();
            _region = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>区域域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? RegionDataState => _region?.Snapshot.State;

        private void OnRegionSectionChanged(RegionDomainSection section) => Render(_region.Snapshot);

        private void Render(RegionDomainSnapshot snapshot)
        {
            // 三页的形状一致：选择栏 + 校验栏 + 提示栏。选择栏与校验栏本来要显示
            // 「选中的目标 / 当前位置可否放置」，但落位流程不存在，所以三栏都陈述原因。
            RenderPlacementPage(snapshot,
                _buildPlacementSelectionBody, _buildPlacementValidityBody, _buildPlacementHintsBody,
                _buildPlacementSelectionTemplate, _buildPlacementSelectionContent,
                _buildPlacementValidityTemplate, _buildPlacementValidityContent,
                _buildPlacementHintsTemplate, _buildPlacementHintsContent,
                _buildPlacementLoadingState, _buildPlacementEmptyState, _buildPlacementErrorState,
                _buildPlacementSuccessState, _buildPlacementDisabledState);

            RenderPlacementPage(snapshot,
                _machineDeploymentSelectionBody, _machineDeploymentValidityBody, _machineDeploymentHintsBody,
                _machineDeploymentSelectionTemplate, _machineDeploymentSelectionContent,
                _machineDeploymentValidityTemplate, _machineDeploymentValidityContent,
                _machineDeploymentHintsTemplate, _machineDeploymentHintsContent,
                _machineDeploymentLoadingState, _machineDeploymentEmptyState, _machineDeploymentErrorState,
                _machineDeploymentSuccessState, _machineDeploymentDisabledState);

            RenderPlacementPage(snapshot,
                _worldBindingPurposeBody, _worldBindingCandidateBody, _worldBindingHintsBody,
                _worldBindingPurposeTemplate, _worldBindingPurposeContent,
                _worldBindingCandidateTemplate, _worldBindingCandidateContent,
                _worldBindingHintsTemplate, _worldBindingHintsContent,
                _worldBindingLoadingState, _worldBindingEmptyState, _worldBindingErrorState,
                _worldBindingSuccessState, _worldBindingDisabledState);
        }

        private void RenderPlacementPage(
            RegionDomainSnapshot snapshot,
            TMPro.TMP_Text firstBody,
            TMPro.TMP_Text secondBody,
            TMPro.TMP_Text thirdBody,
            GameObject firstTemplate,
            RectTransform firstContent,
            GameObject secondTemplate,
            RectTransform secondContent,
            GameObject thirdTemplate,
            RectTransform thirdContent,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState)
        {
            ShowPageUnavailable(
                snapshot.State == UiDataState.Unavailable
                    ? snapshot.UnavailableReason ?? PlacementMissing
                    : PlacementMissing,
                loadingState, emptyState, errorState, successState, disabledState,
                firstBody, secondBody, thirdBody);

            // 三栏都没有数据来源，显式清空而不是留着预制体里的示例行。
            RenderDetailRows(firstTemplate, firstContent, NoFields);
            RenderDetailRows(secondTemplate, secondContent, NoFields);
            RenderDetailRows(thirdTemplate, thirdContent, NoFields);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
