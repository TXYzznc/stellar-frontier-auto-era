using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// AlgorithmEditorForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/AlgorithmEditorForm.contract.json 生成，请勿手改）。
    ///
    /// 与 AlgorithmEditorForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// AlgorithmEditorForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class AlgorithmEditorForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _algorithmEditorNodesContent;
        public RectTransform AlgorithmEditorNodesContent => _algorithmEditorNodesContent;
        [SerializeField] private TMP_Text _algorithmEditorNodesBody;
        public TMP_Text AlgorithmEditorNodesBody => _algorithmEditorNodesBody;
        [SerializeField] private GameObject _algorithmEditorNodesTemplate;
        public GameObject AlgorithmEditorNodesTemplate => _algorithmEditorNodesTemplate;
        [SerializeField] private TMP_InputField _algorithmEditorNodeSearch;
        public TMP_InputField AlgorithmEditorNodeSearch => _algorithmEditorNodeSearch;
        [SerializeField] private RectTransform _algorithmGraphContent;
        public RectTransform AlgorithmGraphContent => _algorithmGraphContent;
        [SerializeField] private GameObject _algorithmNodeItemPrefab;
        public GameObject AlgorithmNodeItemPrefab => _algorithmNodeItemPrefab;
        [SerializeField] private GameObject _algorithmEdgeItemPrefab;
        public GameObject AlgorithmEdgeItemPrefab => _algorithmEdgeItemPrefab;
        [SerializeField] private RectTransform _algorithmEditorInspectorContent;
        public RectTransform AlgorithmEditorInspectorContent => _algorithmEditorInspectorContent;
        [SerializeField] private TMP_Text _algorithmEditorInspectorBody;
        public TMP_Text AlgorithmEditorInspectorBody => _algorithmEditorInspectorBody;
        [SerializeField] private GameObject _algorithmEditorInspectorTemplate;
        public GameObject AlgorithmEditorInspectorTemplate => _algorithmEditorInspectorTemplate;
        [SerializeField] private RectTransform _algorithmEditorProblemsContent;
        public RectTransform AlgorithmEditorProblemsContent => _algorithmEditorProblemsContent;
        [SerializeField] private TMP_Text _algorithmEditorProblemsBody;
        public TMP_Text AlgorithmEditorProblemsBody => _algorithmEditorProblemsBody;
        [SerializeField] private GameObject _algorithmEditorProblemsTemplate;
        public GameObject AlgorithmEditorProblemsTemplate => _algorithmEditorProblemsTemplate;
        [SerializeField] private GameObject _algorithmEditorLoadingState;
        public GameObject AlgorithmEditorLoadingState => _algorithmEditorLoadingState;
        [SerializeField] private GameObject _algorithmEditorEmptyState;
        public GameObject AlgorithmEditorEmptyState => _algorithmEditorEmptyState;
        [SerializeField] private GameObject _algorithmEditorErrorState;
        public GameObject AlgorithmEditorErrorState => _algorithmEditorErrorState;
        [SerializeField] private GameObject _algorithmEditorSuccessState;
        public GameObject AlgorithmEditorSuccessState => _algorithmEditorSuccessState;
        [SerializeField] private GameObject _algorithmEditorDisabledState;
        public GameObject AlgorithmEditorDisabledState => _algorithmEditorDisabledState;
        [SerializeField] private RectTransform _publicParametersParametersContent;
        public RectTransform PublicParametersParametersContent => _publicParametersParametersContent;
        [SerializeField] private TMP_Text _publicParametersParametersBody;
        public TMP_Text PublicParametersParametersBody => _publicParametersParametersBody;
        [SerializeField] private GameObject _publicParametersParametersTemplate;
        public GameObject PublicParametersParametersTemplate => _publicParametersParametersTemplate;
        [SerializeField] private RectTransform _publicParametersImpactContent;
        public RectTransform PublicParametersImpactContent => _publicParametersImpactContent;
        [SerializeField] private TMP_Text _publicParametersImpactBody;
        public TMP_Text PublicParametersImpactBody => _publicParametersImpactBody;
        [SerializeField] private GameObject _publicParametersImpactTemplate;
        public GameObject PublicParametersImpactTemplate => _publicParametersImpactTemplate;
        [SerializeField] private GameObject _publicParametersLoadingState;
        public GameObject PublicParametersLoadingState => _publicParametersLoadingState;
        [SerializeField] private GameObject _publicParametersEmptyState;
        public GameObject PublicParametersEmptyState => _publicParametersEmptyState;
        [SerializeField] private GameObject _publicParametersErrorState;
        public GameObject PublicParametersErrorState => _publicParametersErrorState;
        [SerializeField] private GameObject _publicParametersSuccessState;
        public GameObject PublicParametersSuccessState => _publicParametersSuccessState;
        [SerializeField] private GameObject _publicParametersDisabledState;
        public GameObject PublicParametersDisabledState => _publicParametersDisabledState;
        [SerializeField] private Button _algorithmEditorApplyButton;
        public Button AlgorithmEditorApplyButton => _algorithmEditorApplyButton;
        [SerializeField] private Button _algorithmEditorAddButton;
        public Button AlgorithmEditorAddButton => _algorithmEditorAddButton;
        [SerializeField] private Button _algorithmUndoButton;
        public Button AlgorithmUndoButton => _algorithmUndoButton;
        [SerializeField] private Button _algorithmRedoButton;
        public Button AlgorithmRedoButton => _algorithmRedoButton;
        [SerializeField] private Button _algorithmDeleteSelectedButton;
        public Button AlgorithmDeleteSelectedButton => _algorithmDeleteSelectedButton;
        [SerializeField] private Button _algorithmEditorBindButton;
        public Button AlgorithmEditorBindButton => _algorithmEditorBindButton;
        [SerializeField] private Button _algorithmEditorValidateButton;
        public Button AlgorithmEditorValidateButton => _algorithmEditorValidateButton;
        [SerializeField] private Button _algorithmEditorDiagnoseButton;
        public Button AlgorithmEditorDiagnoseButton => _algorithmEditorDiagnoseButton;
        [SerializeField] private Button _algorithmEditorTemplateButton;
        public Button AlgorithmEditorTemplateButton => _algorithmEditorTemplateButton;
        [SerializeField] private Button _publicParametersChangeButton;
        public Button PublicParametersChangeButton => _publicParametersChangeButton;
        [SerializeField] private Button _publicParametersDefaultButton;
        public Button PublicParametersDefaultButton => _publicParametersDefaultButton;
        [SerializeField] private Button _publicParametersApplyButton;
        public Button PublicParametersApplyButton => _publicParametersApplyButton;
        [SerializeField] private TMP_InputField _publicParametersNumberValue;
        public TMP_InputField PublicParametersNumberValue => _publicParametersNumberValue;
        [SerializeField] private Toggle _publicParametersBooleanValue;
        public Toggle PublicParametersBooleanValue => _publicParametersBooleanValue;
        [SerializeField] private Button _algorithmDiagnosisLocateButton;
        public Button AlgorithmDiagnosisLocateButton => _algorithmDiagnosisLocateButton;
        [SerializeField] private Button _algorithmDiagnosisReturnEditButton;
        public Button AlgorithmDiagnosisReturnEditButton => _algorithmDiagnosisReturnEditButton;
        [SerializeField] private Button _algorithmDiagnosisNextStepButton;
        public Button AlgorithmDiagnosisNextStepButton => _algorithmDiagnosisNextStepButton;
        [SerializeField] private Button _algorithmDiagnosisPreviousStepButton;
        public Button AlgorithmDiagnosisPreviousStepButton => _algorithmDiagnosisPreviousStepButton;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
        [SerializeField] private Button[] _navButtons;
        public Button[] NavButtons => _navButtons;
    }
}
