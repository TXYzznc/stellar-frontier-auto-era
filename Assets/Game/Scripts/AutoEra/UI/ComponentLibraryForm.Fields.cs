using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// ComponentLibraryForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/ComponentLibraryForm.contract.json 生成，请勿手改）。
    ///
    /// 与 ComponentLibraryForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// ComponentLibraryForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class ComponentLibraryForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _looseComponentsCatalogContent;
        public RectTransform LooseComponentsCatalogContent => _looseComponentsCatalogContent;
        [SerializeField] private TMP_Text _looseComponentsCatalogBody;
        public TMP_Text LooseComponentsCatalogBody => _looseComponentsCatalogBody;
        [SerializeField] private GameObject _looseComponentsCatalogTemplate;
        public GameObject LooseComponentsCatalogTemplate => _looseComponentsCatalogTemplate;
        [SerializeField] private RectTransform _looseComponentsDetailsContent;
        public RectTransform LooseComponentsDetailsContent => _looseComponentsDetailsContent;
        [SerializeField] private TMP_Text _looseComponentsDetailsBody;
        public TMP_Text LooseComponentsDetailsBody => _looseComponentsDetailsBody;
        [SerializeField] private GameObject _looseComponentsDetailsTemplate;
        public GameObject LooseComponentsDetailsTemplate => _looseComponentsDetailsTemplate;
        [SerializeField] private GameObject _looseComponentsLoadingState;
        public GameObject LooseComponentsLoadingState => _looseComponentsLoadingState;
        [SerializeField] private GameObject _looseComponentsEmptyState;
        public GameObject LooseComponentsEmptyState => _looseComponentsEmptyState;
        [SerializeField] private GameObject _looseComponentsErrorState;
        public GameObject LooseComponentsErrorState => _looseComponentsErrorState;
        [SerializeField] private GameObject _looseComponentsSuccessState;
        public GameObject LooseComponentsSuccessState => _looseComponentsSuccessState;
        [SerializeField] private GameObject _looseComponentsDisabledState;
        public GameObject LooseComponentsDisabledState => _looseComponentsDisabledState;
        [SerializeField] private RectTransform _installedComponentsCatalogContent;
        public RectTransform InstalledComponentsCatalogContent => _installedComponentsCatalogContent;
        [SerializeField] private TMP_Text _installedComponentsCatalogBody;
        public TMP_Text InstalledComponentsCatalogBody => _installedComponentsCatalogBody;
        [SerializeField] private GameObject _installedComponentsCatalogTemplate;
        public GameObject InstalledComponentsCatalogTemplate => _installedComponentsCatalogTemplate;
        [SerializeField] private RectTransform _installedComponentsDetailsContent;
        public RectTransform InstalledComponentsDetailsContent => _installedComponentsDetailsContent;
        [SerializeField] private TMP_Text _installedComponentsDetailsBody;
        public TMP_Text InstalledComponentsDetailsBody => _installedComponentsDetailsBody;
        [SerializeField] private GameObject _installedComponentsDetailsTemplate;
        public GameObject InstalledComponentsDetailsTemplate => _installedComponentsDetailsTemplate;
        [SerializeField] private GameObject _installedComponentsLoadingState;
        public GameObject InstalledComponentsLoadingState => _installedComponentsLoadingState;
        [SerializeField] private GameObject _installedComponentsEmptyState;
        public GameObject InstalledComponentsEmptyState => _installedComponentsEmptyState;
        [SerializeField] private GameObject _installedComponentsErrorState;
        public GameObject InstalledComponentsErrorState => _installedComponentsErrorState;
        [SerializeField] private GameObject _installedComponentsSuccessState;
        public GameObject InstalledComponentsSuccessState => _installedComponentsSuccessState;
        [SerializeField] private GameObject _installedComponentsDisabledState;
        public GameObject InstalledComponentsDisabledState => _installedComponentsDisabledState;
        [SerializeField] private RectTransform _componentDetailIdentityContent;
        public RectTransform ComponentDetailIdentityContent => _componentDetailIdentityContent;
        [SerializeField] private TMP_Text _componentDetailIdentityBody;
        public TMP_Text ComponentDetailIdentityBody => _componentDetailIdentityBody;
        [SerializeField] private GameObject _componentDetailIdentityTemplate;
        public GameObject ComponentDetailIdentityTemplate => _componentDetailIdentityTemplate;
        [SerializeField] private RectTransform _componentDetailAttributesContent;
        public RectTransform ComponentDetailAttributesContent => _componentDetailAttributesContent;
        [SerializeField] private TMP_Text _componentDetailAttributesBody;
        public TMP_Text ComponentDetailAttributesBody => _componentDetailAttributesBody;
        [SerializeField] private GameObject _componentDetailAttributesTemplate;
        public GameObject ComponentDetailAttributesTemplate => _componentDetailAttributesTemplate;
        [SerializeField] private RectTransform _componentDetailCompareContent;
        public RectTransform ComponentDetailCompareContent => _componentDetailCompareContent;
        [SerializeField] private TMP_Text _componentDetailCompareBody;
        public TMP_Text ComponentDetailCompareBody => _componentDetailCompareBody;
        [SerializeField] private GameObject _componentDetailCompareTemplate;
        public GameObject ComponentDetailCompareTemplate => _componentDetailCompareTemplate;
        [SerializeField] private GameObject _componentDetailLoadingState;
        public GameObject ComponentDetailLoadingState => _componentDetailLoadingState;
        [SerializeField] private GameObject _componentDetailEmptyState;
        public GameObject ComponentDetailEmptyState => _componentDetailEmptyState;
        [SerializeField] private GameObject _componentDetailErrorState;
        public GameObject ComponentDetailErrorState => _componentDetailErrorState;
        [SerializeField] private GameObject _componentDetailSuccessState;
        public GameObject ComponentDetailSuccessState => _componentDetailSuccessState;
        [SerializeField] private GameObject _componentDetailDisabledState;
        public GameObject ComponentDetailDisabledState => _componentDetailDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
        [SerializeField] private Button[] _navButtons;
        public Button[] NavButtons => _navButtons;
    }
}
