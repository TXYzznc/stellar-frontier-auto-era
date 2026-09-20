using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 节点组件选择器（规格 13-算法编辑器/节点组件选择：候选与合同说明）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 候选来自「某个算法节点需要什么组件」这一上下文，而算法实例与节点库都还没有数据源
    /// （见 <see cref="AlgorithmReadModels.NotWiredReason"/>）。本页因此呈现整页不可用，
    /// 只保留返回与关闭。
    /// </summary>
    public sealed partial class NodeComponentPickerForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：本 Form 只有一页。</summary>
        public const int PageNodeComponentPicker = 0;

        private IAlgorithmReadModel _algorithms;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, PageNodeComponentPicker);
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
            ShowPageUnavailable(snapshot.UnavailableReason ?? "候选组件暂不可用。",
                _nodeComponentPickerLoadingState, _nodeComponentPickerEmptyState, _nodeComponentPickerErrorState,
                _nodeComponentPickerSuccessState, _nodeComponentPickerDisabledState,
                _nodeComponentPickerCandidatesBody, _nodeComponentPickerContractBody);
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
