using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// WorldObjectPickerForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/WorldObjectPickerForm.contract.json 生成，请勿手改）。
    ///
    /// 与 WorldObjectPickerForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// WorldObjectPickerForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class WorldObjectPickerForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private Button _selectButton;
        public Button SelectButton => _selectButton;
        [SerializeField] private Button _worldButton;
        public Button WorldButton => _worldButton;
        [SerializeField] private Button _confirmButton;
        public Button ConfirmButton => _confirmButton;
        [SerializeField] private Button _cancelButton;
        public Button CancelButton => _cancelButton;
        [SerializeField] private RectTransform _worldObjectPickerCandidatesContent;
        public RectTransform WorldObjectPickerCandidatesContent => _worldObjectPickerCandidatesContent;
        [SerializeField] private TMP_Text _worldObjectPickerCandidatesBody;
        public TMP_Text WorldObjectPickerCandidatesBody => _worldObjectPickerCandidatesBody;
        [SerializeField] private GameObject _worldObjectPickerCandidatesTemplate;
        public GameObject WorldObjectPickerCandidatesTemplate => _worldObjectPickerCandidatesTemplate;
        [SerializeField] private RectTransform _worldObjectPickerPreviewContent;
        public RectTransform WorldObjectPickerPreviewContent => _worldObjectPickerPreviewContent;
        [SerializeField] private TMP_Text _worldObjectPickerPreviewBody;
        public TMP_Text WorldObjectPickerPreviewBody => _worldObjectPickerPreviewBody;
        [SerializeField] private GameObject _worldObjectPickerPreviewTemplate;
        public GameObject WorldObjectPickerPreviewTemplate => _worldObjectPickerPreviewTemplate;
        [SerializeField] private GameObject _worldObjectPickerLoadingState;
        public GameObject WorldObjectPickerLoadingState => _worldObjectPickerLoadingState;
        [SerializeField] private GameObject _worldObjectPickerEmptyState;
        public GameObject WorldObjectPickerEmptyState => _worldObjectPickerEmptyState;
        [SerializeField] private GameObject _worldObjectPickerErrorState;
        public GameObject WorldObjectPickerErrorState => _worldObjectPickerErrorState;
        [SerializeField] private GameObject _worldObjectPickerSuccessState;
        public GameObject WorldObjectPickerSuccessState => _worldObjectPickerSuccessState;
        [SerializeField] private GameObject _worldObjectPickerDisabledState;
        public GameObject WorldObjectPickerDisabledState => _worldObjectPickerDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
