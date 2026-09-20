using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// ComponentPickerForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/ComponentPickerForm.contract.json 生成，请勿手改）。
    ///
    /// 与 ComponentPickerForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// ComponentPickerForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class ComponentPickerForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _componentPickerCandidatesContent;
        public RectTransform ComponentPickerCandidatesContent => _componentPickerCandidatesContent;
        [SerializeField] private TMP_Text _componentPickerCandidatesBody;
        public TMP_Text ComponentPickerCandidatesBody => _componentPickerCandidatesBody;
        [SerializeField] private GameObject _componentPickerCandidatesTemplate;
        public GameObject ComponentPickerCandidatesTemplate => _componentPickerCandidatesTemplate;
        [SerializeField] private RectTransform _componentPickerComparisonContent;
        public RectTransform ComponentPickerComparisonContent => _componentPickerComparisonContent;
        [SerializeField] private TMP_Text _componentPickerComparisonBody;
        public TMP_Text ComponentPickerComparisonBody => _componentPickerComparisonBody;
        [SerializeField] private GameObject _componentPickerComparisonTemplate;
        public GameObject ComponentPickerComparisonTemplate => _componentPickerComparisonTemplate;
        [SerializeField] private GameObject _componentPickerLoadingState;
        public GameObject ComponentPickerLoadingState => _componentPickerLoadingState;
        [SerializeField] private GameObject _componentPickerEmptyState;
        public GameObject ComponentPickerEmptyState => _componentPickerEmptyState;
        [SerializeField] private GameObject _componentPickerErrorState;
        public GameObject ComponentPickerErrorState => _componentPickerErrorState;
        [SerializeField] private GameObject _componentPickerSuccessState;
        public GameObject ComponentPickerSuccessState => _componentPickerSuccessState;
        [SerializeField] private GameObject _componentPickerDisabledState;
        public GameObject ComponentPickerDisabledState => _componentPickerDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
