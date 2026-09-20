using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// ShopForm 的 GF 桥。结构由 Docs/Development/UI-PrefabLayouts/ShopForm.contract.json 生成
    /// （设计来源：Docs/GameDesign/03-玩家体验/界面规格）。
    ///
    /// 绑定字段在同名的 ShopForm.Fields.cs 里（同一 partial 类）；本文件只有类逻辑：
    /// 规格页序切换、取消意图与默认焦点。Grp_PageHost 下的内容页顺序即规格页序。
    /// </summary>
    public sealed partial class ShopForm : AutoEraShellFormBase
    {
        /// <summary>本界面所属领域尚未接入运行路径；原因写在每一页的空态里。</summary>
        public const string NotWiredReason =
            "交易域尚未接入运行路径：报价与结算服务都还没有创建者。";

        /// <summary>
        /// 本域是否已接入运行路径。领域接入后请把本 Form 从生成器的 NOT_WIRED 集合移除，
        /// 生成器就不会再注入「整页未就绪」这段，届时改为接读模型。
        /// </summary>
        public bool IsDomainWired => false;

        /// <summary>
        /// 导航按钮 → 规格页索引（来源：00-共享外壳-prefab-layout.md 中该 Form 的导航表）。
        /// -1 表示该按钮没有对应内容页（二级详情从页内进入而非顶栏导航）。
        /// </summary>
        private static readonly int[] NavigationPageIndex = { 0, 1 };

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_navButtons != null)
            {
                for (int i = 0; i < _navButtons.Length; i++)
                {
                    Button button = _navButtons[i];
                    if (button == null || i >= NavigationPageIndex.Length) continue;
                    int page = NavigationPageIndex[i];
                    if (page < 0) continue;
                    button.onClick.AddListener(() => ShowFormPage(page));
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

        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, _navButtons != null && _navButtons.Length > 0 && _navButtons[0] != null ? _navButtons[0].gameObject : null);

            // 本界面所属领域尚未接入运行路径：把原因写到每一页的状态组与说明文本上，
            // 并禁用本域的业务动作（安全出口、页导航与列表项不受影响）。
            ShowPageUnavailable(NotWiredReason, _shopBuyLoadingState, _shopBuyEmptyState, _shopBuyErrorState, _shopBuySuccessState, _shopBuyDisabledState, _shopBuyGoodsBody, _shopBuyOfferBody);
            ShowPageUnavailable(NotWiredReason, _shopSellLoadingState, _shopSellEmptyState, _shopSellErrorState, _shopSellSuccessState, _shopSellDisabledState, _shopSellStockBody, _shopSellQuoteBody);
            ShowPageUnavailable(NotWiredReason, _offerDetailLoadingState, _offerDetailEmptyState, _offerDetailErrorState, _offerDetailSuccessState, _offerDetailDisabledState, _offerDetailContentsBody, _offerDetailTermsBody);
            DisableDomainActions();
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
