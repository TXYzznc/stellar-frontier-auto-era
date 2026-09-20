using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// ExitFlowForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/ExitFlowForm.contract.json 生成，请勿手改）。
    ///
    /// 与 ExitFlowForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// ExitFlowForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class ExitFlowForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private Button _retryButton;
        public Button RetryButton => _retryButton;
        [SerializeField] private Button _resumeButton;
        public Button ResumeButton => _resumeButton;
        [SerializeField] private Button _forceButton;
        public Button ForceButton => _forceButton;
        [SerializeField] private RectTransform _exitFlowStageContent;
        public RectTransform ExitFlowStageContent => _exitFlowStageContent;
        [SerializeField] private TMP_Text _exitFlowStageBody;
        public TMP_Text ExitFlowStageBody => _exitFlowStageBody;
        [SerializeField] private GameObject _exitFlowStageTemplate;
        public GameObject ExitFlowStageTemplate => _exitFlowStageTemplate;
        [SerializeField] private RectTransform _exitFlowFailureContent;
        public RectTransform ExitFlowFailureContent => _exitFlowFailureContent;
        [SerializeField] private TMP_Text _exitFlowFailureBody;
        public TMP_Text ExitFlowFailureBody => _exitFlowFailureBody;
        [SerializeField] private GameObject _exitFlowFailureTemplate;
        public GameObject ExitFlowFailureTemplate => _exitFlowFailureTemplate;
        [SerializeField] private GameObject _exitFlowLoadingState;
        public GameObject ExitFlowLoadingState => _exitFlowLoadingState;
        [SerializeField] private GameObject _exitFlowEmptyState;
        public GameObject ExitFlowEmptyState => _exitFlowEmptyState;
        [SerializeField] private GameObject _exitFlowErrorState;
        public GameObject ExitFlowErrorState => _exitFlowErrorState;
        [SerializeField] private GameObject _exitFlowSuccessState;
        public GameObject ExitFlowSuccessState => _exitFlowSuccessState;
        [SerializeField] private GameObject _exitFlowDisabledState;
        public GameObject ExitFlowDisabledState => _exitFlowDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
