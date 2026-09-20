using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// MachineLibraryForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/MachineLibraryForm.contract.json 生成，请勿手改）。
    ///
    /// 与 MachineLibraryForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// MachineLibraryForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class MachineLibraryForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private Button _preparationRenameButton;
        public Button PreparationRenameButton => _preparationRenameButton;
        [SerializeField] private Button _preparationInstallButton;
        public Button PreparationInstallButton => _preparationInstallButton;
        [SerializeField] private Button _preparationUnloadButton;
        public Button PreparationUnloadButton => _preparationUnloadButton;
        [SerializeField] private Button _preparationUpgradeButton;
        public Button PreparationUpgradeButton => _preparationUpgradeButton;
        [SerializeField] private Button _preparationSellButton;
        public Button PreparationSellButton => _preparationSellButton;
        [SerializeField] private Button _preparationDeployButton;
        public Button PreparationDeployButton => _preparationDeployButton;
        [SerializeField] private Button _undeployedPrepareButton;
        public Button UndeployedPrepareButton => _undeployedPrepareButton;
        [SerializeField] private Button _undeployedRenameButton;
        public Button UndeployedRenameButton => _undeployedRenameButton;
        [SerializeField] private Button _undeployedDeployButton;
        public Button UndeployedDeployButton => _undeployedDeployButton;
        [SerializeField] private Button _undeployedSellButton;
        public Button UndeployedSellButton => _undeployedSellButton;
        [SerializeField] private Button _undeployedViewDeployedButton;
        public Button UndeployedViewDeployedButton => _undeployedViewDeployedButton;
        [SerializeField] private Button _deployedLocateButton;
        public Button DeployedLocateButton => _deployedLocateButton;
        [SerializeField] private Button _deployedHubButton;
        public Button DeployedHubButton => _deployedHubButton;
        [SerializeField] private Button _deployedRenameButton;
        public Button DeployedRenameButton => _deployedRenameButton;
        [SerializeField] private Button _deployedReturnButton;
        public Button DeployedReturnButton => _deployedReturnButton;
        [SerializeField] private RectTransform _machinePreparationCarrierContent;
        public RectTransform MachinePreparationCarrierContent => _machinePreparationCarrierContent;
        [SerializeField] private TMP_Text _machinePreparationCarrierBody;
        public TMP_Text MachinePreparationCarrierBody => _machinePreparationCarrierBody;
        [SerializeField] private GameObject _machinePreparationCarrierTemplate;
        public GameObject MachinePreparationCarrierTemplate => _machinePreparationCarrierTemplate;
        [SerializeField] private RectTransform _machinePreparationAssemblyContent;
        public RectTransform MachinePreparationAssemblyContent => _machinePreparationAssemblyContent;
        [SerializeField] private TMP_Text _machinePreparationAssemblyBody;
        public TMP_Text MachinePreparationAssemblyBody => _machinePreparationAssemblyBody;
        [SerializeField] private GameObject _machinePreparationAssemblyTemplate;
        public GameObject MachinePreparationAssemblyTemplate => _machinePreparationAssemblyTemplate;
        [SerializeField] private RectTransform _machinePreparationReadinessContent;
        public RectTransform MachinePreparationReadinessContent => _machinePreparationReadinessContent;
        [SerializeField] private TMP_Text _machinePreparationReadinessBody;
        public TMP_Text MachinePreparationReadinessBody => _machinePreparationReadinessBody;
        [SerializeField] private GameObject _machinePreparationReadinessTemplate;
        public GameObject MachinePreparationReadinessTemplate => _machinePreparationReadinessTemplate;
        [SerializeField] private GameObject _machinePreparationLoadingState;
        public GameObject MachinePreparationLoadingState => _machinePreparationLoadingState;
        [SerializeField] private GameObject _machinePreparationEmptyState;
        public GameObject MachinePreparationEmptyState => _machinePreparationEmptyState;
        [SerializeField] private GameObject _machinePreparationErrorState;
        public GameObject MachinePreparationErrorState => _machinePreparationErrorState;
        [SerializeField] private GameObject _machinePreparationSuccessState;
        public GameObject MachinePreparationSuccessState => _machinePreparationSuccessState;
        [SerializeField] private GameObject _machinePreparationDisabledState;
        public GameObject MachinePreparationDisabledState => _machinePreparationDisabledState;
        [SerializeField] private RectTransform _undeployedMachinesCatalogContent;
        public RectTransform UndeployedMachinesCatalogContent => _undeployedMachinesCatalogContent;
        [SerializeField] private TMP_Text _undeployedMachinesCatalogBody;
        public TMP_Text UndeployedMachinesCatalogBody => _undeployedMachinesCatalogBody;
        [SerializeField] private GameObject _undeployedMachinesCatalogTemplate;
        public GameObject UndeployedMachinesCatalogTemplate => _undeployedMachinesCatalogTemplate;
        [SerializeField] private RectTransform _undeployedMachinesDetailContent;
        public RectTransform UndeployedMachinesDetailContent => _undeployedMachinesDetailContent;
        [SerializeField] private TMP_Text _undeployedMachinesDetailBody;
        public TMP_Text UndeployedMachinesDetailBody => _undeployedMachinesDetailBody;
        [SerializeField] private GameObject _undeployedMachinesDetailTemplate;
        public GameObject UndeployedMachinesDetailTemplate => _undeployedMachinesDetailTemplate;
        [SerializeField] private GameObject _undeployedMachinesLoadingState;
        public GameObject UndeployedMachinesLoadingState => _undeployedMachinesLoadingState;
        [SerializeField] private GameObject _undeployedMachinesEmptyState;
        public GameObject UndeployedMachinesEmptyState => _undeployedMachinesEmptyState;
        [SerializeField] private GameObject _undeployedMachinesErrorState;
        public GameObject UndeployedMachinesErrorState => _undeployedMachinesErrorState;
        [SerializeField] private GameObject _undeployedMachinesSuccessState;
        public GameObject UndeployedMachinesSuccessState => _undeployedMachinesSuccessState;
        [SerializeField] private GameObject _undeployedMachinesDisabledState;
        public GameObject UndeployedMachinesDisabledState => _undeployedMachinesDisabledState;
        [SerializeField] private RectTransform _deployedMachinesCatalogContent;
        public RectTransform DeployedMachinesCatalogContent => _deployedMachinesCatalogContent;
        [SerializeField] private TMP_Text _deployedMachinesCatalogBody;
        public TMP_Text DeployedMachinesCatalogBody => _deployedMachinesCatalogBody;
        [SerializeField] private GameObject _deployedMachinesCatalogTemplate;
        public GameObject DeployedMachinesCatalogTemplate => _deployedMachinesCatalogTemplate;
        [SerializeField] private RectTransform _deployedMachinesDetailContent;
        public RectTransform DeployedMachinesDetailContent => _deployedMachinesDetailContent;
        [SerializeField] private TMP_Text _deployedMachinesDetailBody;
        public TMP_Text DeployedMachinesDetailBody => _deployedMachinesDetailBody;
        [SerializeField] private GameObject _deployedMachinesDetailTemplate;
        public GameObject DeployedMachinesDetailTemplate => _deployedMachinesDetailTemplate;
        [SerializeField] private GameObject _deployedMachinesLoadingState;
        public GameObject DeployedMachinesLoadingState => _deployedMachinesLoadingState;
        [SerializeField] private GameObject _deployedMachinesEmptyState;
        public GameObject DeployedMachinesEmptyState => _deployedMachinesEmptyState;
        [SerializeField] private GameObject _deployedMachinesErrorState;
        public GameObject DeployedMachinesErrorState => _deployedMachinesErrorState;
        [SerializeField] private GameObject _deployedMachinesSuccessState;
        public GameObject DeployedMachinesSuccessState => _deployedMachinesSuccessState;
        [SerializeField] private GameObject _deployedMachinesDisabledState;
        public GameObject DeployedMachinesDisabledState => _deployedMachinesDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
        [SerializeField] private Button[] _navButtons;
        public Button[] NavButtons => _navButtons;
    }
}
