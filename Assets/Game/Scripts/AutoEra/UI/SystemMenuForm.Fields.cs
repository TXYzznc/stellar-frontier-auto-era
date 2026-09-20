using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// SystemMenuForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/SystemMenuForm.contract.json 生成，请勿手改）。
    ///
    /// 与 SystemMenuForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// SystemMenuForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class SystemMenuForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private Button _resumeButton;
        public Button ResumeButton => _resumeButton;
        [SerializeField] private Button _settingsButton;
        public Button SettingsButton => _settingsButton;
        [SerializeField] private Button _helpButton;
        public Button HelpButton => _helpButton;
        [SerializeField] private Button _returnToMenuButton;
        public Button ReturnToMenuButton => _returnToMenuButton;
        [SerializeField] private Button _quitButton;
        public Button QuitButton => _quitButton;
        [SerializeField] private RectTransform _systemMenuSessionContent;
        public RectTransform SystemMenuSessionContent => _systemMenuSessionContent;
        [SerializeField] private TMP_Text _systemMenuSessionBody;
        public TMP_Text SystemMenuSessionBody => _systemMenuSessionBody;
        [SerializeField] private GameObject _systemMenuSessionTemplate;
        public GameObject SystemMenuSessionTemplate => _systemMenuSessionTemplate;
        [SerializeField] private RectTransform _systemMenuCommandsContent;
        public RectTransform SystemMenuCommandsContent => _systemMenuCommandsContent;
        [SerializeField] private TMP_Text _systemMenuCommandsBody;
        public TMP_Text SystemMenuCommandsBody => _systemMenuCommandsBody;
        [SerializeField] private GameObject _systemMenuCommandsTemplate;
        public GameObject SystemMenuCommandsTemplate => _systemMenuCommandsTemplate;
        [SerializeField] private GameObject _systemMenuLoadingState;
        public GameObject SystemMenuLoadingState => _systemMenuLoadingState;
        [SerializeField] private GameObject _systemMenuEmptyState;
        public GameObject SystemMenuEmptyState => _systemMenuEmptyState;
        [SerializeField] private GameObject _systemMenuErrorState;
        public GameObject SystemMenuErrorState => _systemMenuErrorState;
        [SerializeField] private GameObject _systemMenuSuccessState;
        public GameObject SystemMenuSuccessState => _systemMenuSuccessState;
        [SerializeField] private GameObject _systemMenuDisabledState;
        public GameObject SystemMenuDisabledState => _systemMenuDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
