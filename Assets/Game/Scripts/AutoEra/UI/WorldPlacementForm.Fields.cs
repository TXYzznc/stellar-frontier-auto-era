using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// WorldPlacementForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/WorldPlacementForm.contract.json 生成，请勿手改）。
    ///
    /// 与 WorldPlacementForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// WorldPlacementForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class WorldPlacementForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private GameObject _firstInteractable;
        public GameObject FirstInteractable => _firstInteractable;
        [SerializeField] private Button _machineDeploymentRotateButton;
        public Button MachineDeploymentRotateButton => _machineDeploymentRotateButton;
        [SerializeField] private Button _machineDeploymentConfirmButton;
        public Button MachineDeploymentConfirmButton => _machineDeploymentConfirmButton;
        [SerializeField] private Button _machineDeploymentCancelButton;
        public Button MachineDeploymentCancelButton => _machineDeploymentCancelButton;
        [SerializeField] private RectTransform _buildPlacementSelectionContent;
        public RectTransform BuildPlacementSelectionContent => _buildPlacementSelectionContent;
        [SerializeField] private TMP_Text _buildPlacementSelectionBody;
        public TMP_Text BuildPlacementSelectionBody => _buildPlacementSelectionBody;
        [SerializeField] private GameObject _buildPlacementSelectionTemplate;
        public GameObject BuildPlacementSelectionTemplate => _buildPlacementSelectionTemplate;
        [SerializeField] private RectTransform _buildPlacementValidityContent;
        public RectTransform BuildPlacementValidityContent => _buildPlacementValidityContent;
        [SerializeField] private TMP_Text _buildPlacementValidityBody;
        public TMP_Text BuildPlacementValidityBody => _buildPlacementValidityBody;
        [SerializeField] private GameObject _buildPlacementValidityTemplate;
        public GameObject BuildPlacementValidityTemplate => _buildPlacementValidityTemplate;
        [SerializeField] private RectTransform _buildPlacementHintsContent;
        public RectTransform BuildPlacementHintsContent => _buildPlacementHintsContent;
        [SerializeField] private TMP_Text _buildPlacementHintsBody;
        public TMP_Text BuildPlacementHintsBody => _buildPlacementHintsBody;
        [SerializeField] private GameObject _buildPlacementHintsTemplate;
        public GameObject BuildPlacementHintsTemplate => _buildPlacementHintsTemplate;
        [SerializeField] private GameObject _buildPlacementLoadingState;
        public GameObject BuildPlacementLoadingState => _buildPlacementLoadingState;
        [SerializeField] private GameObject _buildPlacementEmptyState;
        public GameObject BuildPlacementEmptyState => _buildPlacementEmptyState;
        [SerializeField] private GameObject _buildPlacementErrorState;
        public GameObject BuildPlacementErrorState => _buildPlacementErrorState;
        [SerializeField] private GameObject _buildPlacementSuccessState;
        public GameObject BuildPlacementSuccessState => _buildPlacementSuccessState;
        [SerializeField] private GameObject _buildPlacementDisabledState;
        public GameObject BuildPlacementDisabledState => _buildPlacementDisabledState;
        [SerializeField] private RectTransform _machineDeploymentSelectionContent;
        public RectTransform MachineDeploymentSelectionContent => _machineDeploymentSelectionContent;
        [SerializeField] private TMP_Text _machineDeploymentSelectionBody;
        public TMP_Text MachineDeploymentSelectionBody => _machineDeploymentSelectionBody;
        [SerializeField] private GameObject _machineDeploymentSelectionTemplate;
        public GameObject MachineDeploymentSelectionTemplate => _machineDeploymentSelectionTemplate;
        [SerializeField] private RectTransform _machineDeploymentValidityContent;
        public RectTransform MachineDeploymentValidityContent => _machineDeploymentValidityContent;
        [SerializeField] private TMP_Text _machineDeploymentValidityBody;
        public TMP_Text MachineDeploymentValidityBody => _machineDeploymentValidityBody;
        [SerializeField] private GameObject _machineDeploymentValidityTemplate;
        public GameObject MachineDeploymentValidityTemplate => _machineDeploymentValidityTemplate;
        [SerializeField] private RectTransform _machineDeploymentHintsContent;
        public RectTransform MachineDeploymentHintsContent => _machineDeploymentHintsContent;
        [SerializeField] private TMP_Text _machineDeploymentHintsBody;
        public TMP_Text MachineDeploymentHintsBody => _machineDeploymentHintsBody;
        [SerializeField] private GameObject _machineDeploymentHintsTemplate;
        public GameObject MachineDeploymentHintsTemplate => _machineDeploymentHintsTemplate;
        [SerializeField] private GameObject _machineDeploymentLoadingState;
        public GameObject MachineDeploymentLoadingState => _machineDeploymentLoadingState;
        [SerializeField] private GameObject _machineDeploymentEmptyState;
        public GameObject MachineDeploymentEmptyState => _machineDeploymentEmptyState;
        [SerializeField] private GameObject _machineDeploymentErrorState;
        public GameObject MachineDeploymentErrorState => _machineDeploymentErrorState;
        [SerializeField] private GameObject _machineDeploymentSuccessState;
        public GameObject MachineDeploymentSuccessState => _machineDeploymentSuccessState;
        [SerializeField] private GameObject _machineDeploymentDisabledState;
        public GameObject MachineDeploymentDisabledState => _machineDeploymentDisabledState;
        [SerializeField] private RectTransform _worldBindingPurposeContent;
        public RectTransform WorldBindingPurposeContent => _worldBindingPurposeContent;
        [SerializeField] private TMP_Text _worldBindingPurposeBody;
        public TMP_Text WorldBindingPurposeBody => _worldBindingPurposeBody;
        [SerializeField] private GameObject _worldBindingPurposeTemplate;
        public GameObject WorldBindingPurposeTemplate => _worldBindingPurposeTemplate;
        [SerializeField] private RectTransform _worldBindingCandidateContent;
        public RectTransform WorldBindingCandidateContent => _worldBindingCandidateContent;
        [SerializeField] private TMP_Text _worldBindingCandidateBody;
        public TMP_Text WorldBindingCandidateBody => _worldBindingCandidateBody;
        [SerializeField] private GameObject _worldBindingCandidateTemplate;
        public GameObject WorldBindingCandidateTemplate => _worldBindingCandidateTemplate;
        [SerializeField] private RectTransform _worldBindingHintsContent;
        public RectTransform WorldBindingHintsContent => _worldBindingHintsContent;
        [SerializeField] private TMP_Text _worldBindingHintsBody;
        public TMP_Text WorldBindingHintsBody => _worldBindingHintsBody;
        [SerializeField] private GameObject _worldBindingHintsTemplate;
        public GameObject WorldBindingHintsTemplate => _worldBindingHintsTemplate;
        [SerializeField] private GameObject _worldBindingLoadingState;
        public GameObject WorldBindingLoadingState => _worldBindingLoadingState;
        [SerializeField] private GameObject _worldBindingEmptyState;
        public GameObject WorldBindingEmptyState => _worldBindingEmptyState;
        [SerializeField] private GameObject _worldBindingErrorState;
        public GameObject WorldBindingErrorState => _worldBindingErrorState;
        [SerializeField] private GameObject _worldBindingSuccessState;
        public GameObject WorldBindingSuccessState => _worldBindingSuccessState;
        [SerializeField] private GameObject _worldBindingDisabledState;
        public GameObject WorldBindingDisabledState => _worldBindingDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
