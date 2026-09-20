using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// WorkshopForm 的 GF 桥。结构由 Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json 生成
    /// （设计来源：Docs/GameDesign/03-玩家体验/界面规格）。
    ///
    /// 绑定字段在同名的 WorkshopForm.Fields.cs 里（同一 partial 类）；本文件只有类逻辑：
    /// 规格页序切换、取消意图与默认焦点。Grp_PageHost 下的内容页顺序即规格页序。
    /// </summary>
    public sealed partial class WorkshopForm : AutoEraShellFormBase
    {
        /// <summary>本界面所属领域尚未接入运行路径；原因写在每一页的空态里。</summary>
        public const string NotWiredReason =
            "工坊与配方域尚未接入运行路径：配方、队列与产出都没有创建者。";

        /// <summary>
        /// 本域是否已接入运行路径。领域接入后请把本 Form 从生成器的 NOT_WIRED 集合移除，
        /// 生成器就不会再注入「整页未就绪」这段，届时改为接读模型。
        /// </summary>
        public bool IsDomainWired => false;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(RequestCancel);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestCancel);
            }

        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);

            // 本界面所属领域尚未接入运行路径：把原因写到每一页的状态组与说明文本上，
            // 并禁用本域的业务动作（安全出口、页导航与列表项不受影响）。
            ShowPageUnavailable(NotWiredReason, _workshopLoadingState, _workshopEmptyState, _workshopErrorState, _workshopSuccessState, _workshopDisabledState, _workshopRecipesBody, _workshopQueueBody, _workshopOutputBody);
            ShowPageUnavailable(NotWiredReason, _recipeDetailLoadingState, _recipeDetailEmptyState, _recipeDetailErrorState, _recipeDetailSuccessState, _recipeDetailDisabledState, _recipeDetailProductBody, _recipeDetailRequirementsBody);
            ShowPageUnavailable(NotWiredReason, _manufacturingQueueLoadingState, _manufacturingQueueEmptyState, _manufacturingQueueErrorState, _manufacturingQueueSuccessState, _manufacturingQueueDisabledState, _manufacturingQueueCurrentBody, _manufacturingQueueWaitingBody);
            ShowPageUnavailable(NotWiredReason, _outputCacheLoadingState, _outputCacheEmptyState, _outputCacheErrorState, _outputCacheSuccessState, _outputCacheDisabledState, _outputCacheItemsBody, _outputCacheBlockBody);
            DisableDomainActions();
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
