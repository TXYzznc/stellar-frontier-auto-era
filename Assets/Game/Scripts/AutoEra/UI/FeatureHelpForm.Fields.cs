using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// FeatureHelpForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/FeatureHelpForm.contract.json 生成，请勿手改）。
    ///
    /// 与 FeatureHelpForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// FeatureHelpForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class FeatureHelpForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _featureHelpExplanationContent;
        public RectTransform FeatureHelpExplanationContent => _featureHelpExplanationContent;
        [SerializeField] private TMP_Text _featureHelpExplanationBody;
        public TMP_Text FeatureHelpExplanationBody => _featureHelpExplanationBody;
        [SerializeField] private GameObject _featureHelpExplanationTemplate;
        public GameObject FeatureHelpExplanationTemplate => _featureHelpExplanationTemplate;
        [SerializeField] private RectTransform _featureHelpNextContent;
        public RectTransform FeatureHelpNextContent => _featureHelpNextContent;
        [SerializeField] private TMP_Text _featureHelpNextBody;
        public TMP_Text FeatureHelpNextBody => _featureHelpNextBody;
        [SerializeField] private GameObject _featureHelpNextTemplate;
        public GameObject FeatureHelpNextTemplate => _featureHelpNextTemplate;
        [SerializeField] private GameObject _featureHelpLoadingState;
        public GameObject FeatureHelpLoadingState => _featureHelpLoadingState;
        [SerializeField] private GameObject _featureHelpEmptyState;
        public GameObject FeatureHelpEmptyState => _featureHelpEmptyState;
        [SerializeField] private GameObject _featureHelpErrorState;
        public GameObject FeatureHelpErrorState => _featureHelpErrorState;
        [SerializeField] private GameObject _featureHelpSuccessState;
        public GameObject FeatureHelpSuccessState => _featureHelpSuccessState;
        [SerializeField] private GameObject _featureHelpDisabledState;
        public GameObject FeatureHelpDisabledState => _featureHelpDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
