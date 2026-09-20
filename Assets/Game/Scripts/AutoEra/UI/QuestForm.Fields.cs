using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// QuestForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/QuestForm.contract.json 生成，请勿手改）。
    ///
    /// 与 QuestForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// QuestForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class QuestForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _questsListContent;
        public RectTransform QuestsListContent => _questsListContent;
        [SerializeField] private TMP_Text _questsListBody;
        public TMP_Text QuestsListBody => _questsListBody;
        [SerializeField] private GameObject _questsListTemplate;
        public GameObject QuestsListTemplate => _questsListTemplate;
        [SerializeField] private RectTransform _questsDetailContent;
        public RectTransform QuestsDetailContent => _questsDetailContent;
        [SerializeField] private TMP_Text _questsDetailBody;
        public TMP_Text QuestsDetailBody => _questsDetailBody;
        [SerializeField] private GameObject _questsDetailTemplate;
        public GameObject QuestsDetailTemplate => _questsDetailTemplate;
        [SerializeField] private GameObject _questsLoadingState;
        public GameObject QuestsLoadingState => _questsLoadingState;
        [SerializeField] private GameObject _questsEmptyState;
        public GameObject QuestsEmptyState => _questsEmptyState;
        [SerializeField] private GameObject _questsErrorState;
        public GameObject QuestsErrorState => _questsErrorState;
        [SerializeField] private GameObject _questsSuccessState;
        public GameObject QuestsSuccessState => _questsSuccessState;
        [SerializeField] private GameObject _questsDisabledState;
        public GameObject QuestsDisabledState => _questsDisabledState;
        [SerializeField] private RectTransform _questRewardsRewardsContent;
        public RectTransform QuestRewardsRewardsContent => _questRewardsRewardsContent;
        [SerializeField] private TMP_Text _questRewardsRewardsBody;
        public TMP_Text QuestRewardsRewardsBody => _questRewardsRewardsBody;
        [SerializeField] private GameObject _questRewardsRewardsTemplate;
        public GameObject QuestRewardsRewardsTemplate => _questRewardsRewardsTemplate;
        [SerializeField] private RectTransform _questRewardsUnlocksContent;
        public RectTransform QuestRewardsUnlocksContent => _questRewardsUnlocksContent;
        [SerializeField] private TMP_Text _questRewardsUnlocksBody;
        public TMP_Text QuestRewardsUnlocksBody => _questRewardsUnlocksBody;
        [SerializeField] private GameObject _questRewardsUnlocksTemplate;
        public GameObject QuestRewardsUnlocksTemplate => _questRewardsUnlocksTemplate;
        [SerializeField] private GameObject _questRewardsLoadingState;
        public GameObject QuestRewardsLoadingState => _questRewardsLoadingState;
        [SerializeField] private GameObject _questRewardsEmptyState;
        public GameObject QuestRewardsEmptyState => _questRewardsEmptyState;
        [SerializeField] private GameObject _questRewardsErrorState;
        public GameObject QuestRewardsErrorState => _questRewardsErrorState;
        [SerializeField] private GameObject _questRewardsSuccessState;
        public GameObject QuestRewardsSuccessState => _questRewardsSuccessState;
        [SerializeField] private GameObject _questRewardsDisabledState;
        public GameObject QuestRewardsDisabledState => _questRewardsDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
        [SerializeField] private Button[] _navButtons;
        public Button[] NavButtons => _navButtons;
    }
}
