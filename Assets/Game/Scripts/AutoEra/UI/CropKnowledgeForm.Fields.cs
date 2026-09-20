using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// CropKnowledgeForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/CropKnowledgeForm.contract.json 生成，请勿手改）。
    ///
    /// 与 CropKnowledgeForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// CropKnowledgeForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class CropKnowledgeForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _cropKnowledgeIdentityContent;
        public RectTransform CropKnowledgeIdentityContent => _cropKnowledgeIdentityContent;
        [SerializeField] private TMP_Text _cropKnowledgeIdentityBody;
        public TMP_Text CropKnowledgeIdentityBody => _cropKnowledgeIdentityBody;
        [SerializeField] private GameObject _cropKnowledgeIdentityTemplate;
        public GameObject CropKnowledgeIdentityTemplate => _cropKnowledgeIdentityTemplate;
        [SerializeField] private RectTransform _cropKnowledgeKnowledgeContent;
        public RectTransform CropKnowledgeKnowledgeContent => _cropKnowledgeKnowledgeContent;
        [SerializeField] private TMP_Text _cropKnowledgeKnowledgeBody;
        public TMP_Text CropKnowledgeKnowledgeBody => _cropKnowledgeKnowledgeBody;
        [SerializeField] private GameObject _cropKnowledgeKnowledgeTemplate;
        public GameObject CropKnowledgeKnowledgeTemplate => _cropKnowledgeKnowledgeTemplate;
        [SerializeField] private GameObject _cropKnowledgeLoadingState;
        public GameObject CropKnowledgeLoadingState => _cropKnowledgeLoadingState;
        [SerializeField] private GameObject _cropKnowledgeEmptyState;
        public GameObject CropKnowledgeEmptyState => _cropKnowledgeEmptyState;
        [SerializeField] private GameObject _cropKnowledgeErrorState;
        public GameObject CropKnowledgeErrorState => _cropKnowledgeErrorState;
        [SerializeField] private GameObject _cropKnowledgeSuccessState;
        public GameObject CropKnowledgeSuccessState => _cropKnowledgeSuccessState;
        [SerializeField] private GameObject _cropKnowledgeDisabledState;
        public GameObject CropKnowledgeDisabledState => _cropKnowledgeDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
