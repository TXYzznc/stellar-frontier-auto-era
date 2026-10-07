using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    public sealed partial class BaseCommandEnergyForm
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _hubEnergyConfigureButton;
        [SerializeField] private Toggle _hubEnergyChargingAllowedToggle;
        [SerializeField] private Slider _hubEnergyChargeTargetSlider;
        [SerializeField] private Button _hubEnergyHistoryButton;
        [SerializeField] private RectTransform _hubEnergySummaryContent;
        [SerializeField] private TMP_Text _hubEnergySummaryBody;
        [SerializeField] private GameObject _hubEnergySummaryTemplate;
        [SerializeField] private RectTransform _hubEnergyFacilitiesContent;
        [SerializeField] private TMP_Text _hubEnergyFacilitiesBody;
        [SerializeField] private GameObject _hubEnergyFacilitiesTemplate;
        [SerializeField] private RectTransform _hubEnergyConsumersContent;
        [SerializeField] private TMP_Text _hubEnergyConsumersBody;
        [SerializeField] private GameObject _hubEnergyConsumersTemplate;
        [SerializeField] private GameObject _hubEnergyLoadingState;
        [SerializeField] private GameObject _hubEnergyEmptyState;
        [SerializeField] private GameObject _hubEnergyErrorState;
        [SerializeField] private GameObject _hubEnergySuccessState;
        [SerializeField] private GameObject _hubEnergyDisabledState;

        public Button HubEnergyConfigureButton => _hubEnergyConfigureButton;
        public Toggle HubEnergyChargingAllowedToggle => _hubEnergyChargingAllowedToggle;
        public Slider HubEnergyChargeTargetSlider => _hubEnergyChargeTargetSlider;
        public Button HubEnergyHistoryButton => _hubEnergyHistoryButton;
        public RectTransform HubEnergySummaryContent => _hubEnergySummaryContent;
        public RectTransform HubEnergyFacilitiesContent => _hubEnergyFacilitiesContent;
        public RectTransform HubEnergyConsumersContent => _hubEnergyConsumersContent;
        public GameObject HubEnergyLoadingState => _hubEnergyLoadingState;
        public GameObject HubEnergyEmptyState => _hubEnergyEmptyState;
        public GameObject HubEnergyErrorState => _hubEnergyErrorState;
        public GameObject HubEnergySuccessState => _hubEnergySuccessState;
        public GameObject HubEnergyDisabledState => _hubEnergyDisabledState;
    }
}
