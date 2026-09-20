using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// MainMenuForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/MainMenuForm.contract.json 生成，请勿手改）。
    ///
    /// 与 MainMenuForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// MainMenuForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class MainMenuForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private Button _enterButton;
        public Button EnterButton => _enterButton;
        [SerializeField] private Button _newButton;
        public Button NewButton => _newButton;
        [SerializeField] private Button _slotsButton;
        public Button SlotsButton => _slotsButton;
        [SerializeField] private Button _settingsButton;
        public Button SettingsButton => _settingsButton;
        [SerializeField] private Button _exitButton;
        public Button ExitButton => _exitButton;
        [SerializeField] private TMP_Text _status;
        public TMP_Text Status => _status;
        [SerializeField] private RectTransform _mainMenuIdentityContent;
        public RectTransform MainMenuIdentityContent => _mainMenuIdentityContent;
        [SerializeField] private GameObject _mainMenuIdentityTemplate;
        public GameObject MainMenuIdentityTemplate => _mainMenuIdentityTemplate;
        [SerializeField] private RectTransform _mainMenuEntryContent;
        public RectTransform MainMenuEntryContent => _mainMenuEntryContent;
        [SerializeField] private TMP_Text _mainMenuEntryBody;
        public TMP_Text MainMenuEntryBody => _mainMenuEntryBody;
        [SerializeField] private GameObject _mainMenuEntryTemplate;
        public GameObject MainMenuEntryTemplate => _mainMenuEntryTemplate;
        [SerializeField] private GameObject _mainMenuLoadingState;
        public GameObject MainMenuLoadingState => _mainMenuLoadingState;
        [SerializeField] private GameObject _mainMenuEmptyState;
        public GameObject MainMenuEmptyState => _mainMenuEmptyState;
        [SerializeField] private GameObject _mainMenuErrorState;
        public GameObject MainMenuErrorState => _mainMenuErrorState;
        [SerializeField] private GameObject _mainMenuSuccessState;
        public GameObject MainMenuSuccessState => _mainMenuSuccessState;
        [SerializeField] private GameObject _mainMenuDisabledState;
        public GameObject MainMenuDisabledState => _mainMenuDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
