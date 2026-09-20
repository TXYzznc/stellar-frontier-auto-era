using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// RuleHelpForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/RuleHelpForm.contract.json 生成，请勿手改）。
    ///
    /// 与 RuleHelpForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// RuleHelpForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class RuleHelpForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _ruleHelpDescriptionContent;
        public RectTransform RuleHelpDescriptionContent => _ruleHelpDescriptionContent;
        [SerializeField] private TMP_Text _ruleHelpDescriptionBody;
        public TMP_Text RuleHelpDescriptionBody => _ruleHelpDescriptionBody;
        [SerializeField] private GameObject _ruleHelpDescriptionTemplate;
        public GameObject RuleHelpDescriptionTemplate => _ruleHelpDescriptionTemplate;
        [SerializeField] private GameObject _ruleHelpLoadingState;
        public GameObject RuleHelpLoadingState => _ruleHelpLoadingState;
        [SerializeField] private GameObject _ruleHelpEmptyState;
        public GameObject RuleHelpEmptyState => _ruleHelpEmptyState;
        [SerializeField] private GameObject _ruleHelpErrorState;
        public GameObject RuleHelpErrorState => _ruleHelpErrorState;
        [SerializeField] private GameObject _ruleHelpSuccessState;
        public GameObject RuleHelpSuccessState => _ruleHelpSuccessState;
        [SerializeField] private GameObject _ruleHelpDisabledState;
        public GameObject RuleHelpDisabledState => _ruleHelpDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
