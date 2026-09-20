using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// NodeComponentPickerForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/NodeComponentPickerForm.contract.json 生成，请勿手改）。
    ///
    /// 与 NodeComponentPickerForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// NodeComponentPickerForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class NodeComponentPickerForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _nodeComponentPickerCandidatesContent;
        public RectTransform NodeComponentPickerCandidatesContent => _nodeComponentPickerCandidatesContent;
        [SerializeField] private TMP_Text _nodeComponentPickerCandidatesBody;
        public TMP_Text NodeComponentPickerCandidatesBody => _nodeComponentPickerCandidatesBody;
        [SerializeField] private GameObject _nodeComponentPickerCandidatesTemplate;
        public GameObject NodeComponentPickerCandidatesTemplate => _nodeComponentPickerCandidatesTemplate;
        [SerializeField] private RectTransform _nodeComponentPickerContractContent;
        public RectTransform NodeComponentPickerContractContent => _nodeComponentPickerContractContent;
        [SerializeField] private TMP_Text _nodeComponentPickerContractBody;
        public TMP_Text NodeComponentPickerContractBody => _nodeComponentPickerContractBody;
        [SerializeField] private GameObject _nodeComponentPickerContractTemplate;
        public GameObject NodeComponentPickerContractTemplate => _nodeComponentPickerContractTemplate;
        [SerializeField] private GameObject _nodeComponentPickerLoadingState;
        public GameObject NodeComponentPickerLoadingState => _nodeComponentPickerLoadingState;
        [SerializeField] private GameObject _nodeComponentPickerEmptyState;
        public GameObject NodeComponentPickerEmptyState => _nodeComponentPickerEmptyState;
        [SerializeField] private GameObject _nodeComponentPickerErrorState;
        public GameObject NodeComponentPickerErrorState => _nodeComponentPickerErrorState;
        [SerializeField] private GameObject _nodeComponentPickerSuccessState;
        public GameObject NodeComponentPickerSuccessState => _nodeComponentPickerSuccessState;
        [SerializeField] private GameObject _nodeComponentPickerDisabledState;
        public GameObject NodeComponentPickerDisabledState => _nodeComponentPickerDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
