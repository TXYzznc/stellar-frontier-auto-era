using AutoEra.Algorithms;
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

        /// <summary>点一个待绑定项 → 打开节点组件选择器（带实例 + 端点上下文）。</summary>
        private void OnBindingClicked(int index)
        {
            if (_algorithms == null)
            {
                return;
            }

            AlgorithmDomainSnapshot snapshot = _algorithms.Snapshot;
            if (!snapshot.SelectedInstance.HasValue || index < 0 || index >= snapshot.PendingBindingCount)
            {
                return;
            }

            UiAlgorithmBindingRow binding = snapshot.PendingBindings[index];
            AutoEraUiNavigator.Open(this, UIViews.NodeComponentPickerForm,
                new AutoEraAlgorithmBindingPickRequest(snapshot.SelectedInstance.Value.Id, binding.BindingKey, binding.Kind));
        }

        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            // 状态通道分开：Unavailable＝算法域缺能力；Empty＝有实例但无待绑定端点；Ready＝有待绑定端点。
            if (snapshot.State == UiDataState.Unavailable)
            {
                ShowPageUnavailable(snapshot.UnavailableReason ?? "待绑定项暂不可用。",
                    _pendingBindingsLoadingState, _pendingBindingsEmptyState, _pendingBindingsErrorState,
                    _pendingBindingsSuccessState, _pendingBindingsDisabledState,
                    _pendingBindingsBindingsBody, _pendingBindingsRequirementBody);
                RenderDetailRows(_pendingBindingsBindingsTemplate, _pendingBindingsBindingsContent, NoFields);
                RenderDetailRows(_pendingBindingsRequirementTemplate, _pendingBindingsRequirementContent, NoFields);
                return;
            }

            bool hasInstances = snapshot.InstanceCount > 0;
            bool hasBindings = snapshot.PendingBindingCount > 0;

            SetState(_pendingBindingsLoadingState, false);
            SetState(_pendingBindingsEmptyState, !hasBindings);
            SetState(_pendingBindingsErrorState, false);
            SetState(_pendingBindingsSuccessState, hasBindings);
            SetState(_pendingBindingsDisabledState, false);

            if (hasBindings)
            {
                RenderListRows(_pendingBindingsBindingsTemplate, _pendingBindingsBindingsContent, snapshot.PendingBindingCount,
                    (index, item) => item.Bind(index, snapshot.PendingBindings[index].Label, snapshot.PendingBindings[index].Status, OnBindingClicked));
                SetText(_pendingBindingsBindingsBody, "待绑定 " + snapshot.PendingBindingCount + " 项（点一行选择要绑定的组件）。");
            }
            else
            {
                RenderDetailRows(_pendingBindingsBindingsTemplate, _pendingBindingsBindingsContent, NoFields);
                SetText(_pendingBindingsBindingsBody, hasInstances
                    ? "本实例已无待绑定端点。"
                    : AlgorithmReadModels.NoInstanceReason);
            }

            RenderDetailRows(_pendingBindingsRequirementTemplate, _pendingBindingsRequirementContent,
                new[] { new UiDetailField("绑定需求", "系统模板不预置绑定，实例化后由玩家逐项绑定实际组件与目标对象。") });
            SetText(_pendingBindingsRequirementBody, hasInstances
                ? "Input 端点读传感器字段，Effector 端点驱动效应器动作；绑定后进入工作台检查并应用。"
                : "绑定属于算法实例：请先在机器上创建一个算法实例。");
        }

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null)
            {
                text.SetText(value ?? string.Empty);
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
