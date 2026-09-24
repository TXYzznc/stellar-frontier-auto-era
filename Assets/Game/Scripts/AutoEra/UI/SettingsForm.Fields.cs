using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// SettingsForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/SettingsForm.contract.json 生成，请勿手改）。
    ///
    /// 与 SettingsForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// SettingsForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class SettingsForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private Button _displaySettingsResetButton;
        public Button DisplaySettingsResetButton => _displaySettingsResetButton;
        [SerializeField] private Button _windowModeFullscreenButton;
        public Button WindowModeFullscreenButton => _windowModeFullscreenButton;
        [SerializeField] private Button _windowModeBorderlessButton;
        public Button WindowModeBorderlessButton => _windowModeBorderlessButton;
        [SerializeField] private Button _vSyncEnabledButton;
        public Button VSyncEnabledButton => _vSyncEnabledButton;
        [SerializeField] private Button _vSyncDisabledButton;
        public Button VSyncDisabledButton => _vSyncDisabledButton;
        [SerializeField] private Button _frameLimitThirtyButton;
        public Button FrameLimitThirtyButton => _frameLimitThirtyButton;
        [SerializeField] private Button _frameLimitSixtyButton;
        public Button FrameLimitSixtyButton => _frameLimitSixtyButton;
        [SerializeField] private Button _frameLimitUnlimitedButton;
        public Button FrameLimitUnlimitedButton => _frameLimitUnlimitedButton;
        [SerializeField] private Button _qualityPresetLowButton;
        public Button QualityPresetLowButton => _qualityPresetLowButton;
        [SerializeField] private Button _qualityPresetMediumButton;
        public Button QualityPresetMediumButton => _qualityPresetMediumButton;
        [SerializeField] private Button _qualityPresetHighButton;
        public Button QualityPresetHighButton => _qualityPresetHighButton;
        [SerializeField] private Slider _audioSettingsMainVolumeSlider;
        public Slider AudioSettingsMainVolumeSlider => _audioSettingsMainVolumeSlider;
        [SerializeField] private Slider _audioSettingsMusicVolumeSlider;
        public Slider AudioSettingsMusicVolumeSlider => _audioSettingsMusicVolumeSlider;
        [SerializeField] private Slider _audioSettingsAmbientVolumeSlider;
        public Slider AudioSettingsAmbientVolumeSlider => _audioSettingsAmbientVolumeSlider;
        [SerializeField] private Slider _audioSettingsMachineVolumeSlider;
        public Slider AudioSettingsMachineVolumeSlider => _audioSettingsMachineVolumeSlider;
        [SerializeField] private Slider _audioSettingsUiVolumeSlider;
        public Slider AudioSettingsUiVolumeSlider => _audioSettingsUiVolumeSlider;
        [SerializeField] private Button _audioSettingsResetButton;
        public Button AudioSettingsResetButton => _audioSettingsResetButton;
        [SerializeField] private Slider _controlSettingsPanSpeedSlider;
        public Slider ControlSettingsPanSpeedSlider => _controlSettingsPanSpeedSlider;
        [SerializeField] private Slider _controlSettingsRotateSpeedSlider;
        public Slider ControlSettingsRotateSpeedSlider => _controlSettingsRotateSpeedSlider;
        [SerializeField] private Slider _controlSettingsZoomSpeedSlider;
        public Slider ControlSettingsZoomSpeedSlider => _controlSettingsZoomSpeedSlider;
        [SerializeField] private Toggle _controlSettingsInvertHorizontalToggle;
        public Toggle ControlSettingsInvertHorizontalToggle => _controlSettingsInvertHorizontalToggle;
        [SerializeField] private Toggle _controlSettingsInvertVerticalToggle;
        public Toggle ControlSettingsInvertVerticalToggle => _controlSettingsInvertVerticalToggle;
        [SerializeField] private Button _controlSettingsResetButton;
        public Button ControlSettingsResetButton => _controlSettingsResetButton;
        [SerializeField] private RectTransform _audioSettingsVolumesContent;
        public RectTransform AudioSettingsVolumesContent => _audioSettingsVolumesContent;
        [SerializeField] private TMP_Text _audioSettingsVolumesBody;
        public TMP_Text AudioSettingsVolumesBody => _audioSettingsVolumesBody;
        [SerializeField] private GameObject _audioSettingsVolumesTemplate;
        public GameObject AudioSettingsVolumesTemplate => _audioSettingsVolumesTemplate;
        [SerializeField] private RectTransform _audioSettingsPersistenceContent;
        public RectTransform AudioSettingsPersistenceContent => _audioSettingsPersistenceContent;
        [SerializeField] private TMP_Text _audioSettingsPersistenceBody;
        public TMP_Text AudioSettingsPersistenceBody => _audioSettingsPersistenceBody;
        [SerializeField] private GameObject _audioSettingsPersistenceTemplate;
        public GameObject AudioSettingsPersistenceTemplate => _audioSettingsPersistenceTemplate;
        [SerializeField] private GameObject _audioSettingsLoadingState;
        public GameObject AudioSettingsLoadingState => _audioSettingsLoadingState;
        [SerializeField] private GameObject _audioSettingsEmptyState;
        public GameObject AudioSettingsEmptyState => _audioSettingsEmptyState;
        [SerializeField] private GameObject _audioSettingsErrorState;
        public GameObject AudioSettingsErrorState => _audioSettingsErrorState;
        [SerializeField] private GameObject _audioSettingsSuccessState;
        public GameObject AudioSettingsSuccessState => _audioSettingsSuccessState;
        [SerializeField] private GameObject _audioSettingsDisabledState;
        public GameObject AudioSettingsDisabledState => _audioSettingsDisabledState;
        [SerializeField] private RectTransform _controlSettingsCameraContent;
        public RectTransform ControlSettingsCameraContent => _controlSettingsCameraContent;
        [SerializeField] private TMP_Text _controlSettingsCameraBody;
        public TMP_Text ControlSettingsCameraBody => _controlSettingsCameraBody;
        [SerializeField] private GameObject _controlSettingsCameraTemplate;
        public GameObject ControlSettingsCameraTemplate => _controlSettingsCameraTemplate;
        [SerializeField] private RectTransform _controlSettingsBindingsContent;
        public RectTransform ControlSettingsBindingsContent => _controlSettingsBindingsContent;
        [SerializeField] private TMP_Text _controlSettingsBindingsBody;
        public TMP_Text ControlSettingsBindingsBody => _controlSettingsBindingsBody;
        [SerializeField] private GameObject _controlSettingsBindingsTemplate;
        public GameObject ControlSettingsBindingsTemplate => _controlSettingsBindingsTemplate;
        [SerializeField] private GameObject _controlSettingsLoadingState;
        public GameObject ControlSettingsLoadingState => _controlSettingsLoadingState;
        [SerializeField] private GameObject _controlSettingsEmptyState;
        public GameObject ControlSettingsEmptyState => _controlSettingsEmptyState;
        [SerializeField] private GameObject _controlSettingsErrorState;
        public GameObject ControlSettingsErrorState => _controlSettingsErrorState;
        [SerializeField] private GameObject _controlSettingsSuccessState;
        public GameObject ControlSettingsSuccessState => _controlSettingsSuccessState;
        [SerializeField] private GameObject _controlSettingsDisabledState;
        public GameObject ControlSettingsDisabledState => _controlSettingsDisabledState;
        [SerializeField] private RectTransform _displaySettingsDisplayContent;
        public RectTransform DisplaySettingsDisplayContent => _displaySettingsDisplayContent;
        [SerializeField] private TMP_Text _displaySettingsDisplayBody;
        public TMP_Text DisplaySettingsDisplayBody => _displaySettingsDisplayBody;
        [SerializeField] private GameObject _displaySettingsDisplayTemplate;
        public GameObject DisplaySettingsDisplayTemplate => _displaySettingsDisplayTemplate;
        [SerializeField] private RectTransform _displaySettingsQualityContent;
        public RectTransform DisplaySettingsQualityContent => _displaySettingsQualityContent;
        [SerializeField] private TMP_Text _displaySettingsQualityBody;
        public TMP_Text DisplaySettingsQualityBody => _displaySettingsQualityBody;
        [SerializeField] private GameObject _displaySettingsQualityTemplate;
        public GameObject DisplaySettingsQualityTemplate => _displaySettingsQualityTemplate;
        [SerializeField] private GameObject _displaySettingsLoadingState;
        public GameObject DisplaySettingsLoadingState => _displaySettingsLoadingState;
        [SerializeField] private GameObject _displaySettingsEmptyState;
        public GameObject DisplaySettingsEmptyState => _displaySettingsEmptyState;
        [SerializeField] private GameObject _displaySettingsErrorState;
        public GameObject DisplaySettingsErrorState => _displaySettingsErrorState;
        [SerializeField] private GameObject _displaySettingsSuccessState;
        public GameObject DisplaySettingsSuccessState => _displaySettingsSuccessState;
        [SerializeField] private GameObject _displaySettingsDisabledState;
        public GameObject DisplaySettingsDisabledState => _displaySettingsDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
        [SerializeField] private Button[] _navButtons;
        public Button[] NavButtons => _navButtons;
    }
}
