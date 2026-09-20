using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// AlgorithmLibraryForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json 生成，请勿手改）。
    ///
    /// 与 AlgorithmLibraryForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// AlgorithmLibraryForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class AlgorithmLibraryForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _systemTemplatesCatalogContent;
        public RectTransform SystemTemplatesCatalogContent => _systemTemplatesCatalogContent;
        [SerializeField] private TMP_Text _systemTemplatesCatalogBody;
        public TMP_Text SystemTemplatesCatalogBody => _systemTemplatesCatalogBody;
        [SerializeField] private GameObject _systemTemplatesCatalogTemplate;
        public GameObject SystemTemplatesCatalogTemplate => _systemTemplatesCatalogTemplate;
        [SerializeField] private RectTransform _systemTemplatesDetailContent;
        public RectTransform SystemTemplatesDetailContent => _systemTemplatesDetailContent;
        [SerializeField] private TMP_Text _systemTemplatesDetailBody;
        public TMP_Text SystemTemplatesDetailBody => _systemTemplatesDetailBody;
        [SerializeField] private GameObject _systemTemplatesDetailTemplate;
        public GameObject SystemTemplatesDetailTemplate => _systemTemplatesDetailTemplate;
        [SerializeField] private GameObject _systemTemplatesLoadingState;
        public GameObject SystemTemplatesLoadingState => _systemTemplatesLoadingState;
        [SerializeField] private GameObject _systemTemplatesEmptyState;
        public GameObject SystemTemplatesEmptyState => _systemTemplatesEmptyState;
        [SerializeField] private GameObject _systemTemplatesErrorState;
        public GameObject SystemTemplatesErrorState => _systemTemplatesErrorState;
        [SerializeField] private GameObject _systemTemplatesSuccessState;
        public GameObject SystemTemplatesSuccessState => _systemTemplatesSuccessState;
        [SerializeField] private GameObject _systemTemplatesDisabledState;
        public GameObject SystemTemplatesDisabledState => _systemTemplatesDisabledState;
        [SerializeField] private RectTransform _playerTemplatesCatalogContent;
        public RectTransform PlayerTemplatesCatalogContent => _playerTemplatesCatalogContent;
        [SerializeField] private TMP_Text _playerTemplatesCatalogBody;
        public TMP_Text PlayerTemplatesCatalogBody => _playerTemplatesCatalogBody;
        [SerializeField] private GameObject _playerTemplatesCatalogTemplate;
        public GameObject PlayerTemplatesCatalogTemplate => _playerTemplatesCatalogTemplate;
        [SerializeField] private RectTransform _playerTemplatesDetailContent;
        public RectTransform PlayerTemplatesDetailContent => _playerTemplatesDetailContent;
        [SerializeField] private TMP_Text _playerTemplatesDetailBody;
        public TMP_Text PlayerTemplatesDetailBody => _playerTemplatesDetailBody;
        [SerializeField] private GameObject _playerTemplatesDetailTemplate;
        public GameObject PlayerTemplatesDetailTemplate => _playerTemplatesDetailTemplate;
        [SerializeField] private GameObject _playerTemplatesLoadingState;
        public GameObject PlayerTemplatesLoadingState => _playerTemplatesLoadingState;
        [SerializeField] private GameObject _playerTemplatesEmptyState;
        public GameObject PlayerTemplatesEmptyState => _playerTemplatesEmptyState;
        [SerializeField] private GameObject _playerTemplatesErrorState;
        public GameObject PlayerTemplatesErrorState => _playerTemplatesErrorState;
        [SerializeField] private GameObject _playerTemplatesSuccessState;
        public GameObject PlayerTemplatesSuccessState => _playerTemplatesSuccessState;
        [SerializeField] private GameObject _playerTemplatesDisabledState;
        public GameObject PlayerTemplatesDisabledState => _playerTemplatesDisabledState;
        [SerializeField] private RectTransform _templateDetailDefinitionContent;
        public RectTransform TemplateDetailDefinitionContent => _templateDetailDefinitionContent;
        [SerializeField] private TMP_Text _templateDetailDefinitionBody;
        public TMP_Text TemplateDetailDefinitionBody => _templateDetailDefinitionBody;
        [SerializeField] private GameObject _templateDetailDefinitionTemplate;
        public GameObject TemplateDetailDefinitionTemplate => _templateDetailDefinitionTemplate;
        [SerializeField] private RectTransform _templateDetailRequirementsContent;
        public RectTransform TemplateDetailRequirementsContent => _templateDetailRequirementsContent;
        [SerializeField] private TMP_Text _templateDetailRequirementsBody;
        public TMP_Text TemplateDetailRequirementsBody => _templateDetailRequirementsBody;
        [SerializeField] private GameObject _templateDetailRequirementsTemplate;
        public GameObject TemplateDetailRequirementsTemplate => _templateDetailRequirementsTemplate;
        [SerializeField] private GameObject _templateDetailLoadingState;
        public GameObject TemplateDetailLoadingState => _templateDetailLoadingState;
        [SerializeField] private GameObject _templateDetailEmptyState;
        public GameObject TemplateDetailEmptyState => _templateDetailEmptyState;
        [SerializeField] private GameObject _templateDetailErrorState;
        public GameObject TemplateDetailErrorState => _templateDetailErrorState;
        [SerializeField] private GameObject _templateDetailSuccessState;
        public GameObject TemplateDetailSuccessState => _templateDetailSuccessState;
        [SerializeField] private GameObject _templateDetailDisabledState;
        public GameObject TemplateDetailDisabledState => _templateDetailDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
        [SerializeField] private Button[] _navButtons;
        public Button[] NavButtons => _navButtons;
    }
}
