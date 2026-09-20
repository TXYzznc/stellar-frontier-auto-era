using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// HelpForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/HelpForm.contract.json 生成，请勿手改）。
    ///
    /// 与 HelpForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// HelpForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class HelpForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _helpTopicsContent;
        public RectTransform HelpTopicsContent => _helpTopicsContent;
        [SerializeField] private TMP_Text _helpTopicsBody;
        public TMP_Text HelpTopicsBody => _helpTopicsBody;
        [SerializeField] private GameObject _helpTopicsTemplate;
        public GameObject HelpTopicsTemplate => _helpTopicsTemplate;
        [SerializeField] private RectTransform _helpArticleContent;
        public RectTransform HelpArticleContent => _helpArticleContent;
        [SerializeField] private TMP_Text _helpArticleBody;
        public TMP_Text HelpArticleBody => _helpArticleBody;
        [SerializeField] private GameObject _helpArticleTemplate;
        public GameObject HelpArticleTemplate => _helpArticleTemplate;
        [SerializeField] private GameObject _helpLoadingState;
        public GameObject HelpLoadingState => _helpLoadingState;
        [SerializeField] private GameObject _helpEmptyState;
        public GameObject HelpEmptyState => _helpEmptyState;
        [SerializeField] private GameObject _helpErrorState;
        public GameObject HelpErrorState => _helpErrorState;
        [SerializeField] private GameObject _helpSuccessState;
        public GameObject HelpSuccessState => _helpSuccessState;
        [SerializeField] private GameObject _helpDisabledState;
        public GameObject HelpDisabledState => _helpDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
