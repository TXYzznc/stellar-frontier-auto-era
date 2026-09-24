using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法模板库（规格 12-算法/模板库：系统模板、玩家模板、模板详情）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 算法域的服务层是完整的（<c>AlgorithmInstanceService</c> / <c>AlgorithmTemplateLibrary</c>
    /// 都有实现与集成测试），但**生产运行路径从未创建它们**——见
    /// <see cref="AlgorithmReadModels.NotWiredReason"/>。所以本页当前呈现的是「整页不可用」
    /// 这一真实状态：状态组切到 Disabled，三个页面都写明原因，业务按钮全部禁用，
    /// 只有返回、关闭与两页导航仍然可用。接线完成后本页不需要改动，只需换掉读模型工厂。
    /// </summary>
    public sealed partial class AlgorithmLibraryForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 系统模板、1 玩家模板、2 模板详情。</summary>
        public const int PageSystemTemplates = 0;
        public const int PagePlayerTemplates = 1;
        public const int PageTemplateDetail = 2;

        /// <summary>导航按钮 → 规格页索引（来源：00-共享外壳-prefab-layout.md 的导航表）。</summary>
        private static readonly int[] NavigationPageIndex = { PageSystemTemplates, PagePlayerTemplates };

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private IAlgorithmReadModel _algorithms;

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
                    if (page < 0)
                    {
                        continue;
                    }

                    button.onClick.AddListener(() => ShowLibraryPage(page));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageSystemTemplates;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null ? _navButtons[0].gameObject : null);

            _algorithms = AlgorithmReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _algorithms.Changed += OnAlgorithmSectionChanged;

            Render(_algorithms.Snapshot);
            DisableDomainActions();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseAlgorithms();

        protected override void OnAutoEraRecycle()
        {
            ReleaseAlgorithms();
            base.OnAutoEraRecycle();
        }

        private void ReleaseAlgorithms()
        {
            if (_algorithms == null)
            {
                return;
            }

            _algorithms.Changed -= OnAlgorithmSectionChanged;
            _algorithms.Dispose();
            _algorithms = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowLibraryPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            string reason = snapshot.UnavailableReason ?? "算法模板暂不可用。";
            // Unavailable＝算法域缺能力；Empty/Ready＝算法域活着，只是**模板库**还没接线
            // （模板是存档级数据，不随机器运行时出现）。状态通道必须分开：后者怎么点都不会有数据，
            // 前者换一台机器就可能出现。
            bool unavailable = snapshot.State == UiDataState.Unavailable;

            RenderPage(unavailable, reason,
                _systemTemplatesLoadingState, _systemTemplatesEmptyState, _systemTemplatesErrorState,
                _systemTemplatesSuccessState, _systemTemplatesDisabledState,
                _systemTemplatesCatalogBody, _systemTemplatesDetailBody);

            RenderPage(unavailable, reason,
                _playerTemplatesLoadingState, _playerTemplatesEmptyState, _playerTemplatesErrorState,
                _playerTemplatesSuccessState, _playerTemplatesDisabledState,
                _playerTemplatesCatalogBody, _playerTemplatesDetailBody);

            RenderPage(unavailable, reason,
                _templateDetailLoadingState, _templateDetailEmptyState, _templateDetailErrorState,
                _templateDetailSuccessState, _templateDetailDisabledState,
                _templateDetailDefinitionBody, _templateDetailRequirementsBody);

            // 模板列表仍然没有数据来源，显式清空段落行——不能留着预制体里的示例行冒充真实数据。
            RenderDetailRows(_systemTemplatesCatalogTemplate, _systemTemplatesCatalogContent, NoFields);
            RenderDetailRows(_systemTemplatesDetailTemplate, _systemTemplatesDetailContent, NoFields);
            RenderDetailRows(_playerTemplatesCatalogTemplate, _playerTemplatesCatalogContent, NoFields);
            RenderDetailRows(_playerTemplatesDetailTemplate, _playerTemplatesDetailContent, NoFields);
            RenderDetailRows(_templateDetailDefinitionTemplate, _templateDetailDefinitionContent, NoFields);
            RenderDetailRows(_templateDetailRequirementsTemplate, _templateDetailRequirementsContent, NoFields);
        }

        private void RenderPage(bool unavailable, string reason, GameObject loadingState, GameObject emptyState,
            GameObject errorState, GameObject successState, GameObject disabledState,
            TMPro.TMP_Text firstBody, TMPro.TMP_Text secondBody)
        {
            if (unavailable)
            {
                ShowPageUnavailable(reason, loadingState, emptyState, errorState, successState, disabledState,
                    firstBody, secondBody);
                return;
            }

            ShowPageEmpty(AlgorithmReadModels.NotWiredReason,
                loadingState, emptyState, errorState, successState, disabledState, firstBody, secondBody);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
