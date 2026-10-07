using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    public sealed partial class AlgorithmPublicParametersForm
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private RectTransform _parametersContent;
        [SerializeField] private TMP_Text _parametersBody;
        [SerializeField] private GameObject _parametersTemplate;
        [SerializeField] private RectTransform _impactContent;
        [SerializeField] private TMP_Text _impactBody;
        [SerializeField] private GameObject _impactTemplate;
        [SerializeField] private GameObject _loadingState;
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _errorState;
        [SerializeField] private GameObject _successState;
        [SerializeField] private GameObject _disabledState;
        [SerializeField] private Button _changeButton;
        [SerializeField] private Button _defaultButton;
        [SerializeField] private Button _applyButton;
        [SerializeField] private TMP_InputField _numberValue;
        [SerializeField] private Toggle _booleanValue;
    }
}
