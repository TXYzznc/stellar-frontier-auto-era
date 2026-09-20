using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// BuildCatalogForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/BuildCatalogForm.contract.json 生成，请勿手改）。
    ///
    /// 与 BuildCatalogForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// BuildCatalogForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class BuildCatalogForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _blueprintsCatalogContent;
        public RectTransform BlueprintsCatalogContent => _blueprintsCatalogContent;
        [SerializeField] private TMP_Text _blueprintsCatalogBody;
        public TMP_Text BlueprintsCatalogBody => _blueprintsCatalogBody;
        [SerializeField] private GameObject _blueprintsCatalogTemplate;
        public GameObject BlueprintsCatalogTemplate => _blueprintsCatalogTemplate;
        [SerializeField] private RectTransform _blueprintsBlueprintContent;
        public RectTransform BlueprintsBlueprintContent => _blueprintsBlueprintContent;
        [SerializeField] private TMP_Text _blueprintsBlueprintBody;
        public TMP_Text BlueprintsBlueprintBody => _blueprintsBlueprintBody;
        [SerializeField] private GameObject _blueprintsBlueprintTemplate;
        public GameObject BlueprintsBlueprintTemplate => _blueprintsBlueprintTemplate;
        [SerializeField] private GameObject _blueprintsLoadingState;
        public GameObject BlueprintsLoadingState => _blueprintsLoadingState;
        [SerializeField] private GameObject _blueprintsEmptyState;
        public GameObject BlueprintsEmptyState => _blueprintsEmptyState;
        [SerializeField] private GameObject _blueprintsErrorState;
        public GameObject BlueprintsErrorState => _blueprintsErrorState;
        [SerializeField] private GameObject _blueprintsSuccessState;
        public GameObject BlueprintsSuccessState => _blueprintsSuccessState;
        [SerializeField] private GameObject _blueprintsDisabledState;
        public GameObject BlueprintsDisabledState => _blueprintsDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
