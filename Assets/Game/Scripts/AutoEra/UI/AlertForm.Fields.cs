using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// AlertForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/AlertForm.contract.json 生成，请勿手改）。
    ///
    /// 与 AlertForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// AlertForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class AlertForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _alertsListContent;
        public RectTransform AlertsListContent => _alertsListContent;
        [SerializeField] private TMP_Text _alertsListBody;
        public TMP_Text AlertsListBody => _alertsListBody;
        [SerializeField] private GameObject _alertsListTemplate;
        public GameObject AlertsListTemplate => _alertsListTemplate;
        [SerializeField] private RectTransform _alertsDetailContent;
        public RectTransform AlertsDetailContent => _alertsDetailContent;
        [SerializeField] private TMP_Text _alertsDetailBody;
        public TMP_Text AlertsDetailBody => _alertsDetailBody;
        [SerializeField] private GameObject _alertsDetailTemplate;
        public GameObject AlertsDetailTemplate => _alertsDetailTemplate;
        [SerializeField] private GameObject _alertsLoadingState;
        public GameObject AlertsLoadingState => _alertsLoadingState;
        [SerializeField] private GameObject _alertsEmptyState;
        public GameObject AlertsEmptyState => _alertsEmptyState;
        [SerializeField] private GameObject _alertsErrorState;
        public GameObject AlertsErrorState => _alertsErrorState;
        [SerializeField] private GameObject _alertsSuccessState;
        public GameObject AlertsSuccessState => _alertsSuccessState;
        [SerializeField] private GameObject _alertsDisabledState;
        public GameObject AlertsDisabledState => _alertsDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
