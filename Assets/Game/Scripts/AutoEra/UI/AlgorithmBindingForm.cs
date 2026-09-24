using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 集中待绑定（规格 13-算法编辑器/待绑定：绑定清单与需求说明）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 「待绑定」来自算法实例的草稿文档——所以本页的状态取决于**这台机器有没有算法实例**：
    /// 算法域缺能力时是 Unavailable（写明缺什么），域活着但没有实例/图时是 Empty
    /// （写明还缺「选择实例 → 读它的传感器与对象端点」这条通道），两者不混成一个状态。
    /// </summary>
    public sealed partial class AlgorithmBindingForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：本 Form 只有一页。</summary>
        public const int PagePendingBindings = 0;

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private IAlgorithmReadModel _algorithms;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, PagePendingBindings);
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);

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
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            // 状态通道要分开：Unavailable＝算法域缺能力（怎么点都不会有数据）；
            // Empty/Ready＝算法域活着，缺的是「一个已应用的算法图」——绑定属于图，不属于机器。
            if (snapshot.State == UiDataState.Unavailable)
            {
                ShowPageUnavailable(snapshot.UnavailableReason ?? "待绑定项暂不可用。",
                    _pendingBindingsLoadingState, _pendingBindingsEmptyState, _pendingBindingsErrorState,
                    _pendingBindingsSuccessState, _pendingBindingsDisabledState,
                    _pendingBindingsBindingsBody, _pendingBindingsRequirementBody);
            }
            else
            {
                ShowPageEmpty(
                    "算法绑定需要一张已应用的算法图：本页还没有接上「选择算法实例 → 读它的传感器与对象端点」"
                    + "这条通道，因此不显示任何待绑定项。",
                    _pendingBindingsLoadingState, _pendingBindingsEmptyState, _pendingBindingsErrorState,
                    _pendingBindingsSuccessState, _pendingBindingsDisabledState,
                    _pendingBindingsBindingsBody, _pendingBindingsRequirementBody);
            }

            RenderDetailRows(_pendingBindingsBindingsTemplate, _pendingBindingsBindingsContent, NoFields);
            RenderDetailRows(_pendingBindingsRequirementTemplate, _pendingBindingsRequirementContent, NoFields);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
