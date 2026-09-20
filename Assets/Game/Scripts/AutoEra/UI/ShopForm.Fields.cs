using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// ShopForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/ShopForm.contract.json 生成，请勿手改）。
    ///
    /// 与 ShopForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// ShopForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class ShopForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _shopBuyGoodsContent;
        public RectTransform ShopBuyGoodsContent => _shopBuyGoodsContent;
        [SerializeField] private TMP_Text _shopBuyGoodsBody;
        public TMP_Text ShopBuyGoodsBody => _shopBuyGoodsBody;
        [SerializeField] private GameObject _shopBuyGoodsTemplate;
        public GameObject ShopBuyGoodsTemplate => _shopBuyGoodsTemplate;
        [SerializeField] private RectTransform _shopBuyOfferContent;
        public RectTransform ShopBuyOfferContent => _shopBuyOfferContent;
        [SerializeField] private TMP_Text _shopBuyOfferBody;
        public TMP_Text ShopBuyOfferBody => _shopBuyOfferBody;
        [SerializeField] private GameObject _shopBuyOfferTemplate;
        public GameObject ShopBuyOfferTemplate => _shopBuyOfferTemplate;
        [SerializeField] private GameObject _shopBuyLoadingState;
        public GameObject ShopBuyLoadingState => _shopBuyLoadingState;
        [SerializeField] private GameObject _shopBuyEmptyState;
        public GameObject ShopBuyEmptyState => _shopBuyEmptyState;
        [SerializeField] private GameObject _shopBuyErrorState;
        public GameObject ShopBuyErrorState => _shopBuyErrorState;
        [SerializeField] private GameObject _shopBuySuccessState;
        public GameObject ShopBuySuccessState => _shopBuySuccessState;
        [SerializeField] private GameObject _shopBuyDisabledState;
        public GameObject ShopBuyDisabledState => _shopBuyDisabledState;
        [SerializeField] private RectTransform _shopSellStockContent;
        public RectTransform ShopSellStockContent => _shopSellStockContent;
        [SerializeField] private TMP_Text _shopSellStockBody;
        public TMP_Text ShopSellStockBody => _shopSellStockBody;
        [SerializeField] private GameObject _shopSellStockTemplate;
        public GameObject ShopSellStockTemplate => _shopSellStockTemplate;
        [SerializeField] private RectTransform _shopSellQuoteContent;
        public RectTransform ShopSellQuoteContent => _shopSellQuoteContent;
        [SerializeField] private TMP_Text _shopSellQuoteBody;
        public TMP_Text ShopSellQuoteBody => _shopSellQuoteBody;
        [SerializeField] private GameObject _shopSellQuoteTemplate;
        public GameObject ShopSellQuoteTemplate => _shopSellQuoteTemplate;
        [SerializeField] private GameObject _shopSellLoadingState;
        public GameObject ShopSellLoadingState => _shopSellLoadingState;
        [SerializeField] private GameObject _shopSellEmptyState;
        public GameObject ShopSellEmptyState => _shopSellEmptyState;
        [SerializeField] private GameObject _shopSellErrorState;
        public GameObject ShopSellErrorState => _shopSellErrorState;
        [SerializeField] private GameObject _shopSellSuccessState;
        public GameObject ShopSellSuccessState => _shopSellSuccessState;
        [SerializeField] private GameObject _shopSellDisabledState;
        public GameObject ShopSellDisabledState => _shopSellDisabledState;
        [SerializeField] private RectTransform _offerDetailContentsContent;
        public RectTransform OfferDetailContentsContent => _offerDetailContentsContent;
        [SerializeField] private TMP_Text _offerDetailContentsBody;
        public TMP_Text OfferDetailContentsBody => _offerDetailContentsBody;
        [SerializeField] private GameObject _offerDetailContentsTemplate;
        public GameObject OfferDetailContentsTemplate => _offerDetailContentsTemplate;
        [SerializeField] private RectTransform _offerDetailTermsContent;
        public RectTransform OfferDetailTermsContent => _offerDetailTermsContent;
        [SerializeField] private TMP_Text _offerDetailTermsBody;
        public TMP_Text OfferDetailTermsBody => _offerDetailTermsBody;
        [SerializeField] private GameObject _offerDetailTermsTemplate;
        public GameObject OfferDetailTermsTemplate => _offerDetailTermsTemplate;
        [SerializeField] private GameObject _offerDetailLoadingState;
        public GameObject OfferDetailLoadingState => _offerDetailLoadingState;
        [SerializeField] private GameObject _offerDetailEmptyState;
        public GameObject OfferDetailEmptyState => _offerDetailEmptyState;
        [SerializeField] private GameObject _offerDetailErrorState;
        public GameObject OfferDetailErrorState => _offerDetailErrorState;
        [SerializeField] private GameObject _offerDetailSuccessState;
        public GameObject OfferDetailSuccessState => _offerDetailSuccessState;
        [SerializeField] private GameObject _offerDetailDisabledState;
        public GameObject OfferDetailDisabledState => _offerDetailDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
        [SerializeField] private Button[] _navButtons;
        public Button[] NavButtons => _navButtons;
    }
}
