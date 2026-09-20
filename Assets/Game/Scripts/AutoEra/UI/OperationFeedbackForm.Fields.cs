using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// OperationFeedbackForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/OperationFeedbackForm.contract.json 生成，请勿手改）。
    ///
    /// 与 OperationFeedbackForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// OperationFeedbackForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class OperationFeedbackForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _safeWaitRequestContent;
        public RectTransform SafeWaitRequestContent => _safeWaitRequestContent;
        [SerializeField] private TMP_Text _safeWaitRequestBody;
        public TMP_Text SafeWaitRequestBody => _safeWaitRequestBody;
        [SerializeField] private GameObject _safeWaitRequestTemplate;
        public GameObject SafeWaitRequestTemplate => _safeWaitRequestTemplate;
        [SerializeField] private RectTransform _safeWaitWaitingContent;
        public RectTransform SafeWaitWaitingContent => _safeWaitWaitingContent;
        [SerializeField] private TMP_Text _safeWaitWaitingBody;
        public TMP_Text SafeWaitWaitingBody => _safeWaitWaitingBody;
        [SerializeField] private GameObject _safeWaitWaitingTemplate;
        public GameObject SafeWaitWaitingTemplate => _safeWaitWaitingTemplate;
        [SerializeField] private GameObject _safeWaitLoadingState;
        public GameObject SafeWaitLoadingState => _safeWaitLoadingState;
        [SerializeField] private GameObject _safeWaitEmptyState;
        public GameObject SafeWaitEmptyState => _safeWaitEmptyState;
        [SerializeField] private GameObject _safeWaitErrorState;
        public GameObject SafeWaitErrorState => _safeWaitErrorState;
        [SerializeField] private GameObject _safeWaitSuccessState;
        public GameObject SafeWaitSuccessState => _safeWaitSuccessState;
        [SerializeField] private GameObject _safeWaitDisabledState;
        public GameObject SafeWaitDisabledState => _safeWaitDisabledState;
        [SerializeField] private RectTransform _actionResultResultContent;
        public RectTransform ActionResultResultContent => _actionResultResultContent;
        [SerializeField] private TMP_Text _actionResultResultBody;
        public TMP_Text ActionResultResultBody => _actionResultResultBody;
        [SerializeField] private GameObject _actionResultResultTemplate;
        public GameObject ActionResultResultTemplate => _actionResultResultTemplate;
        [SerializeField] private RectTransform _actionResultNextContent;
        public RectTransform ActionResultNextContent => _actionResultNextContent;
        [SerializeField] private TMP_Text _actionResultNextBody;
        public TMP_Text ActionResultNextBody => _actionResultNextBody;
        [SerializeField] private GameObject _actionResultNextTemplate;
        public GameObject ActionResultNextTemplate => _actionResultNextTemplate;
        [SerializeField] private GameObject _actionResultLoadingState;
        public GameObject ActionResultLoadingState => _actionResultLoadingState;
        [SerializeField] private GameObject _actionResultEmptyState;
        public GameObject ActionResultEmptyState => _actionResultEmptyState;
        [SerializeField] private GameObject _actionResultErrorState;
        public GameObject ActionResultErrorState => _actionResultErrorState;
        [SerializeField] private GameObject _actionResultSuccessState;
        public GameObject ActionResultSuccessState => _actionResultSuccessState;
        [SerializeField] private GameObject _actionResultDisabledState;
        public GameObject ActionResultDisabledState => _actionResultDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
