using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// SaveSlotsForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json 生成，请勿手改）。
    ///
    /// 与 SaveSlotsForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// SaveSlotsForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class SaveSlotsForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _slotListContent;
        public RectTransform SlotListContent => _slotListContent;
        [SerializeField] private GameObject _slotRowTemplate;
        public GameObject SlotRowTemplate => _slotRowTemplate;
        [SerializeField] private Button _selectButton;
        public Button SelectButton => _selectButton;
        [SerializeField] private Button _detailsButton;
        public Button DetailsButton => _detailsButton;
        [SerializeField] private Button _createButton;
        public Button CreateButton => _createButton;
        [SerializeField] private Button _continueButton;
        public Button ContinueButton => _continueButton;
        [SerializeField] private Button _saveDetailContinueButton;
        public Button SaveDetailContinueButton => _saveDetailContinueButton;
        [SerializeField] private Button _saveDetailDeleteButton;
        public Button SaveDetailDeleteButton => _saveDetailDeleteButton;
        [SerializeField] private Button _saveDetailRecoverButton;
        public Button SaveDetailRecoverButton => _saveDetailRecoverButton;
        [SerializeField] private Button _newProgressCreateButton;
        public Button NewProgressCreateButton => _newProgressCreateButton;
        [SerializeField] private Button _newProgressBackButton;
        public Button NewProgressBackButton => _newProgressBackButton;
        [SerializeField] private TMP_Text _saveSlotsSlotsBody;
        public TMP_Text SaveSlotsSlotsBody => _saveSlotsSlotsBody;
        [SerializeField] private RectTransform _saveSlotsPreviewContent;
        public RectTransform SaveSlotsPreviewContent => _saveSlotsPreviewContent;
        [SerializeField] private TMP_Text _saveSlotsPreviewBody;
        public TMP_Text SaveSlotsPreviewBody => _saveSlotsPreviewBody;
        [SerializeField] private GameObject _saveSlotsPreviewTemplate;
        public GameObject SaveSlotsPreviewTemplate => _saveSlotsPreviewTemplate;
        [SerializeField] private GameObject _saveSlotsLoadingState;
        public GameObject SaveSlotsLoadingState => _saveSlotsLoadingState;
        [SerializeField] private GameObject _saveSlotsEmptyState;
        public GameObject SaveSlotsEmptyState => _saveSlotsEmptyState;
        [SerializeField] private GameObject _saveSlotsErrorState;
        public GameObject SaveSlotsErrorState => _saveSlotsErrorState;
        [SerializeField] private GameObject _saveSlotsSuccessState;
        public GameObject SaveSlotsSuccessState => _saveSlotsSuccessState;
        [SerializeField] private GameObject _saveSlotsDisabledState;
        public GameObject SaveSlotsDisabledState => _saveSlotsDisabledState;
        [SerializeField] private RectTransform _saveDetailMetadataContent;
        public RectTransform SaveDetailMetadataContent => _saveDetailMetadataContent;
        [SerializeField] private TMP_Text _saveDetailMetadataBody;
        public TMP_Text SaveDetailMetadataBody => _saveDetailMetadataBody;
        [SerializeField] private GameObject _saveDetailMetadataTemplate;
        public GameObject SaveDetailMetadataTemplate => _saveDetailMetadataTemplate;
        [SerializeField] private RectTransform _saveDetailHealthContent;
        public RectTransform SaveDetailHealthContent => _saveDetailHealthContent;
        [SerializeField] private TMP_Text _saveDetailHealthBody;
        public TMP_Text SaveDetailHealthBody => _saveDetailHealthBody;
        [SerializeField] private GameObject _saveDetailHealthTemplate;
        public GameObject SaveDetailHealthTemplate => _saveDetailHealthTemplate;
        [SerializeField] private GameObject _saveDetailLoadingState;
        public GameObject SaveDetailLoadingState => _saveDetailLoadingState;
        [SerializeField] private GameObject _saveDetailEmptyState;
        public GameObject SaveDetailEmptyState => _saveDetailEmptyState;
        [SerializeField] private GameObject _saveDetailErrorState;
        public GameObject SaveDetailErrorState => _saveDetailErrorState;
        [SerializeField] private GameObject _saveDetailSuccessState;
        public GameObject SaveDetailSuccessState => _saveDetailSuccessState;
        [SerializeField] private GameObject _saveDetailDisabledState;
        public GameObject SaveDetailDisabledState => _saveDetailDisabledState;
        [SerializeField] private RectTransform _newProgressTargetContent;
        public RectTransform NewProgressTargetContent => _newProgressTargetContent;
        [SerializeField] private TMP_Text _newProgressTargetBody;
        public TMP_Text NewProgressTargetBody => _newProgressTargetBody;
        [SerializeField] private GameObject _newProgressTargetTemplate;
        public GameObject NewProgressTargetTemplate => _newProgressTargetTemplate;
        [SerializeField] private RectTransform _newProgressConsequencesContent;
        public RectTransform NewProgressConsequencesContent => _newProgressConsequencesContent;
        [SerializeField] private TMP_Text _newProgressConsequencesBody;
        public TMP_Text NewProgressConsequencesBody => _newProgressConsequencesBody;
        [SerializeField] private GameObject _newProgressConsequencesTemplate;
        public GameObject NewProgressConsequencesTemplate => _newProgressConsequencesTemplate;
        [SerializeField] private GameObject _newProgressLoadingState;
        public GameObject NewProgressLoadingState => _newProgressLoadingState;
        [SerializeField] private GameObject _newProgressEmptyState;
        public GameObject NewProgressEmptyState => _newProgressEmptyState;
        [SerializeField] private GameObject _newProgressErrorState;
        public GameObject NewProgressErrorState => _newProgressErrorState;
        [SerializeField] private GameObject _newProgressSuccessState;
        public GameObject NewProgressSuccessState => _newProgressSuccessState;
        [SerializeField] private GameObject _newProgressDisabledState;
        public GameObject NewProgressDisabledState => _newProgressDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
