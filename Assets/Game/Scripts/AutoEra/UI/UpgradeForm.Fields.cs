using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// UpgradeForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/UpgradeForm.contract.json 生成，请勿手改）。
    ///
    /// 与 UpgradeForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// UpgradeForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class UpgradeForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _carrierUpgradeBeforeContent;
        public RectTransform CarrierUpgradeBeforeContent => _carrierUpgradeBeforeContent;
        [SerializeField] private TMP_Text _carrierUpgradeBeforeBody;
        public TMP_Text CarrierUpgradeBeforeBody => _carrierUpgradeBeforeBody;
        [SerializeField] private GameObject _carrierUpgradeBeforeTemplate;
        public GameObject CarrierUpgradeBeforeTemplate => _carrierUpgradeBeforeTemplate;
        [SerializeField] private RectTransform _carrierUpgradeAfterContent;
        public RectTransform CarrierUpgradeAfterContent => _carrierUpgradeAfterContent;
        [SerializeField] private TMP_Text _carrierUpgradeAfterBody;
        public TMP_Text CarrierUpgradeAfterBody => _carrierUpgradeAfterBody;
        [SerializeField] private GameObject _carrierUpgradeAfterTemplate;
        public GameObject CarrierUpgradeAfterTemplate => _carrierUpgradeAfterTemplate;
        [SerializeField] private RectTransform _carrierUpgradeCostContent;
        public RectTransform CarrierUpgradeCostContent => _carrierUpgradeCostContent;
        [SerializeField] private TMP_Text _carrierUpgradeCostBody;
        public TMP_Text CarrierUpgradeCostBody => _carrierUpgradeCostBody;
        [SerializeField] private GameObject _carrierUpgradeCostTemplate;
        public GameObject CarrierUpgradeCostTemplate => _carrierUpgradeCostTemplate;
        [SerializeField] private GameObject _carrierUpgradeLoadingState;
        public GameObject CarrierUpgradeLoadingState => _carrierUpgradeLoadingState;
        [SerializeField] private GameObject _carrierUpgradeEmptyState;
        public GameObject CarrierUpgradeEmptyState => _carrierUpgradeEmptyState;
        [SerializeField] private GameObject _carrierUpgradeErrorState;
        public GameObject CarrierUpgradeErrorState => _carrierUpgradeErrorState;
        [SerializeField] private GameObject _carrierUpgradeSuccessState;
        public GameObject CarrierUpgradeSuccessState => _carrierUpgradeSuccessState;
        [SerializeField] private GameObject _carrierUpgradeDisabledState;
        public GameObject CarrierUpgradeDisabledState => _carrierUpgradeDisabledState;
        [SerializeField] private RectTransform _componentUpgradeBeforeContent;
        public RectTransform ComponentUpgradeBeforeContent => _componentUpgradeBeforeContent;
        [SerializeField] private TMP_Text _componentUpgradeBeforeBody;
        public TMP_Text ComponentUpgradeBeforeBody => _componentUpgradeBeforeBody;
        [SerializeField] private GameObject _componentUpgradeBeforeTemplate;
        public GameObject ComponentUpgradeBeforeTemplate => _componentUpgradeBeforeTemplate;
        [SerializeField] private RectTransform _componentUpgradeAfterContent;
        public RectTransform ComponentUpgradeAfterContent => _componentUpgradeAfterContent;
        [SerializeField] private TMP_Text _componentUpgradeAfterBody;
        public TMP_Text ComponentUpgradeAfterBody => _componentUpgradeAfterBody;
        [SerializeField] private GameObject _componentUpgradeAfterTemplate;
        public GameObject ComponentUpgradeAfterTemplate => _componentUpgradeAfterTemplate;
        [SerializeField] private RectTransform _componentUpgradeCostContent;
        public RectTransform ComponentUpgradeCostContent => _componentUpgradeCostContent;
        [SerializeField] private TMP_Text _componentUpgradeCostBody;
        public TMP_Text ComponentUpgradeCostBody => _componentUpgradeCostBody;
        [SerializeField] private GameObject _componentUpgradeCostTemplate;
        public GameObject ComponentUpgradeCostTemplate => _componentUpgradeCostTemplate;
        [SerializeField] private GameObject _componentUpgradeLoadingState;
        public GameObject ComponentUpgradeLoadingState => _componentUpgradeLoadingState;
        [SerializeField] private GameObject _componentUpgradeEmptyState;
        public GameObject ComponentUpgradeEmptyState => _componentUpgradeEmptyState;
        [SerializeField] private GameObject _componentUpgradeErrorState;
        public GameObject ComponentUpgradeErrorState => _componentUpgradeErrorState;
        [SerializeField] private GameObject _componentUpgradeSuccessState;
        public GameObject ComponentUpgradeSuccessState => _componentUpgradeSuccessState;
        [SerializeField] private GameObject _componentUpgradeDisabledState;
        public GameObject ComponentUpgradeDisabledState => _componentUpgradeDisabledState;
        [SerializeField] private RectTransform _modificationImpactChangeContent;
        public RectTransform ModificationImpactChangeContent => _modificationImpactChangeContent;
        [SerializeField] private TMP_Text _modificationImpactChangeBody;
        public TMP_Text ModificationImpactChangeBody => _modificationImpactChangeBody;
        [SerializeField] private GameObject _modificationImpactChangeTemplate;
        public GameObject ModificationImpactChangeTemplate => _modificationImpactChangeTemplate;
        [SerializeField] private RectTransform _modificationImpactImpactContent;
        public RectTransform ModificationImpactImpactContent => _modificationImpactImpactContent;
        [SerializeField] private TMP_Text _modificationImpactImpactBody;
        public TMP_Text ModificationImpactImpactBody => _modificationImpactImpactBody;
        [SerializeField] private GameObject _modificationImpactImpactTemplate;
        public GameObject ModificationImpactImpactTemplate => _modificationImpactImpactTemplate;
        [SerializeField] private GameObject _modificationImpactLoadingState;
        public GameObject ModificationImpactLoadingState => _modificationImpactLoadingState;
        [SerializeField] private GameObject _modificationImpactEmptyState;
        public GameObject ModificationImpactEmptyState => _modificationImpactEmptyState;
        [SerializeField] private GameObject _modificationImpactErrorState;
        public GameObject ModificationImpactErrorState => _modificationImpactErrorState;
        [SerializeField] private GameObject _modificationImpactSuccessState;
        public GameObject ModificationImpactSuccessState => _modificationImpactSuccessState;
        [SerializeField] private GameObject _modificationImpactDisabledState;
        public GameObject ModificationImpactDisabledState => _modificationImpactDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
