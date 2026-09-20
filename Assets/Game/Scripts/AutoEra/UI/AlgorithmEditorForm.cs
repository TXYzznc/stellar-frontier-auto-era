using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法工作台（规格 13-算法编辑器：编辑与诊断两种模式、公开参数为精简内容页）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 编辑与诊断都建立在「某台机器有一个算法实例」之上，而生产运行路径还没有创建机器执行
    /// 上下文与算法实例服务（见 <see cref="AlgorithmReadModels.NotWiredReason"/>），
    /// 因此本页呈现整页不可用：图画布、节点库、检视器、问题清单与公开参数五个区域都写明原因，
    /// 业务按钮全部禁用，只保留返回、关闭与顶栏导航。
    ///
    /// 注：`Content_AlgorithmGraph` 上挂的是 <see cref="AutoEraGraphLayoutGroup"/>——规格提到
    /// `GraphLayoutGroup` 是设计提出、仓库未实现的组件，原型阶段用它满足「Content_ 必须有
    /// LayoutGroup」的结构契约，同时不覆写画布自由坐标。
    /// </summary>
    public sealed partial class AlgorithmEditorForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：0 算法编辑、1 公开参数。</summary>
        public const int PageEditor = 0;
        public const int PagePublicParameters = 1;

        private static readonly int[] NavigationPageIndex = { PageEditor, -1 };

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

                    button.onClick.AddListener(() => ShowEditorPage(page));
                }
            }

            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageEditor;
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
        public bool ShowEditorPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            string reason = snapshot.UnavailableReason ?? "算法实例暂不可用。";

            ShowPageUnavailable(reason,
                _algorithmEditorLoadingState, _algorithmEditorEmptyState, _algorithmEditorErrorState,
                _algorithmEditorSuccessState, _algorithmEditorDisabledState,
                _algorithmEditorNodesBody, _algorithmEditorInspectorBody, _algorithmEditorProblemsBody);

            ShowPageUnavailable(reason,
                _publicParametersLoadingState, _publicParametersEmptyState, _publicParametersErrorState,
                _publicParametersSuccessState, _publicParametersDisabledState,
                _publicParametersParametersBody, _publicParametersImpactBody);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
