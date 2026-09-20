using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// AlgorithmBindingForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/AlgorithmBindingForm.contract.json 生成，请勿手改）。
    ///
    /// 与 AlgorithmBindingForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// AlgorithmBindingForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class AlgorithmBindingForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _pendingBindingsBindingsContent;
        public RectTransform PendingBindingsBindingsContent => _pendingBindingsBindingsContent;
        [SerializeField] private TMP_Text _pendingBindingsBindingsBody;
        public TMP_Text PendingBindingsBindingsBody => _pendingBindingsBindingsBody;
        [SerializeField] private GameObject _pendingBindingsBindingsTemplate;
        public GameObject PendingBindingsBindingsTemplate => _pendingBindingsBindingsTemplate;
        [SerializeField] private RectTransform _pendingBindingsRequirementContent;
        public RectTransform PendingBindingsRequirementContent => _pendingBindingsRequirementContent;
        [SerializeField] private TMP_Text _pendingBindingsRequirementBody;
        public TMP_Text PendingBindingsRequirementBody => _pendingBindingsRequirementBody;
        [SerializeField] private GameObject _pendingBindingsRequirementTemplate;
        public GameObject PendingBindingsRequirementTemplate => _pendingBindingsRequirementTemplate;
        [SerializeField] private GameObject _pendingBindingsLoadingState;
        public GameObject PendingBindingsLoadingState => _pendingBindingsLoadingState;
        [SerializeField] private GameObject _pendingBindingsEmptyState;
        public GameObject PendingBindingsEmptyState => _pendingBindingsEmptyState;
        [SerializeField] private GameObject _pendingBindingsErrorState;
        public GameObject PendingBindingsErrorState => _pendingBindingsErrorState;
        [SerializeField] private GameObject _pendingBindingsSuccessState;
        public GameObject PendingBindingsSuccessState => _pendingBindingsSuccessState;
        [SerializeField] private GameObject _pendingBindingsDisabledState;
        public GameObject PendingBindingsDisabledState => _pendingBindingsDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
