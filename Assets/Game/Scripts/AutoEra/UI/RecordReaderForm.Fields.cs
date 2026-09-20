using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// RecordReaderForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/RecordReaderForm.contract.json 生成，请勿手改）。
    ///
    /// 与 RecordReaderForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// RecordReaderForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class RecordReaderForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _machineHistoryListContent;
        public RectTransform MachineHistoryListContent => _machineHistoryListContent;
        [SerializeField] private TMP_Text _machineHistoryListBody;
        public TMP_Text MachineHistoryListBody => _machineHistoryListBody;
        [SerializeField] private GameObject _machineHistoryListTemplate;
        public GameObject MachineHistoryListTemplate => _machineHistoryListTemplate;
        [SerializeField] private RectTransform _machineHistoryDetailContent;
        public RectTransform MachineHistoryDetailContent => _machineHistoryDetailContent;
        [SerializeField] private TMP_Text _machineHistoryDetailBody;
        public TMP_Text MachineHistoryDetailBody => _machineHistoryDetailBody;
        [SerializeField] private GameObject _machineHistoryDetailTemplate;
        public GameObject MachineHistoryDetailTemplate => _machineHistoryDetailTemplate;
        [SerializeField] private GameObject _machineHistoryLoadingState;
        public GameObject MachineHistoryLoadingState => _machineHistoryLoadingState;
        [SerializeField] private GameObject _machineHistoryEmptyState;
        public GameObject MachineHistoryEmptyState => _machineHistoryEmptyState;
        [SerializeField] private GameObject _machineHistoryErrorState;
        public GameObject MachineHistoryErrorState => _machineHistoryErrorState;
        [SerializeField] private GameObject _machineHistorySuccessState;
        public GameObject MachineHistorySuccessState => _machineHistorySuccessState;
        [SerializeField] private GameObject _machineHistoryDisabledState;
        public GameObject MachineHistoryDisabledState => _machineHistoryDisabledState;
        [SerializeField] private RectTransform _algorithmHistoryListContent;
        public RectTransform AlgorithmHistoryListContent => _algorithmHistoryListContent;
        [SerializeField] private TMP_Text _algorithmHistoryListBody;
        public TMP_Text AlgorithmHistoryListBody => _algorithmHistoryListBody;
        [SerializeField] private GameObject _algorithmHistoryListTemplate;
        public GameObject AlgorithmHistoryListTemplate => _algorithmHistoryListTemplate;
        [SerializeField] private RectTransform _algorithmHistoryDetailContent;
        public RectTransform AlgorithmHistoryDetailContent => _algorithmHistoryDetailContent;
        [SerializeField] private TMP_Text _algorithmHistoryDetailBody;
        public TMP_Text AlgorithmHistoryDetailBody => _algorithmHistoryDetailBody;
        [SerializeField] private GameObject _algorithmHistoryDetailTemplate;
        public GameObject AlgorithmHistoryDetailTemplate => _algorithmHistoryDetailTemplate;
        [SerializeField] private GameObject _algorithmHistoryLoadingState;
        public GameObject AlgorithmHistoryLoadingState => _algorithmHistoryLoadingState;
        [SerializeField] private GameObject _algorithmHistoryEmptyState;
        public GameObject AlgorithmHistoryEmptyState => _algorithmHistoryEmptyState;
        [SerializeField] private GameObject _algorithmHistoryErrorState;
        public GameObject AlgorithmHistoryErrorState => _algorithmHistoryErrorState;
        [SerializeField] private GameObject _algorithmHistorySuccessState;
        public GameObject AlgorithmHistorySuccessState => _algorithmHistorySuccessState;
        [SerializeField] private GameObject _algorithmHistoryDisabledState;
        public GameObject AlgorithmHistoryDisabledState => _algorithmHistoryDisabledState;
        [SerializeField] private RectTransform _energyHistoryListContent;
        public RectTransform EnergyHistoryListContent => _energyHistoryListContent;
        [SerializeField] private TMP_Text _energyHistoryListBody;
        public TMP_Text EnergyHistoryListBody => _energyHistoryListBody;
        [SerializeField] private GameObject _energyHistoryListTemplate;
        public GameObject EnergyHistoryListTemplate => _energyHistoryListTemplate;
        [SerializeField] private RectTransform _energyHistoryDetailContent;
        public RectTransform EnergyHistoryDetailContent => _energyHistoryDetailContent;
        [SerializeField] private TMP_Text _energyHistoryDetailBody;
        public TMP_Text EnergyHistoryDetailBody => _energyHistoryDetailBody;
        [SerializeField] private GameObject _energyHistoryDetailTemplate;
        public GameObject EnergyHistoryDetailTemplate => _energyHistoryDetailTemplate;
        [SerializeField] private GameObject _energyHistoryLoadingState;
        public GameObject EnergyHistoryLoadingState => _energyHistoryLoadingState;
        [SerializeField] private GameObject _energyHistoryEmptyState;
        public GameObject EnergyHistoryEmptyState => _energyHistoryEmptyState;
        [SerializeField] private GameObject _energyHistoryErrorState;
        public GameObject EnergyHistoryErrorState => _energyHistoryErrorState;
        [SerializeField] private GameObject _energyHistorySuccessState;
        public GameObject EnergyHistorySuccessState => _energyHistorySuccessState;
        [SerializeField] private GameObject _energyHistoryDisabledState;
        public GameObject EnergyHistoryDisabledState => _energyHistoryDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
