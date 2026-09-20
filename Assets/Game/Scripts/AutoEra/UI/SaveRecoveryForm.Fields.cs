using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// SaveRecoveryForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/SaveRecoveryForm.contract.json 生成，请勿手改）。
    ///
    /// 与 SaveRecoveryForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// SaveRecoveryForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class SaveRecoveryForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private Button _restoreButton;
        public Button RestoreButton => _restoreButton;
        [SerializeField] private Button _returnButton;
        public Button ReturnButton => _returnButton;
        [SerializeField] private RectTransform _recoveryDiagnosisContent;
        public RectTransform RecoveryDiagnosisContent => _recoveryDiagnosisContent;
        [SerializeField] private TMP_Text _recoveryDiagnosisBody;
        public TMP_Text RecoveryDiagnosisBody => _recoveryDiagnosisBody;
        [SerializeField] private GameObject _recoveryDiagnosisTemplate;
        public GameObject RecoveryDiagnosisTemplate => _recoveryDiagnosisTemplate;
        [SerializeField] private RectTransform _recoveryCandidateContent;
        public RectTransform RecoveryCandidateContent => _recoveryCandidateContent;
        [SerializeField] private TMP_Text _recoveryCandidateBody;
        public TMP_Text RecoveryCandidateBody => _recoveryCandidateBody;
        [SerializeField] private GameObject _recoveryCandidateTemplate;
        public GameObject RecoveryCandidateTemplate => _recoveryCandidateTemplate;
        [SerializeField] private GameObject _recoveryLoadingState;
        public GameObject RecoveryLoadingState => _recoveryLoadingState;
        [SerializeField] private GameObject _recoveryEmptyState;
        public GameObject RecoveryEmptyState => _recoveryEmptyState;
        [SerializeField] private GameObject _recoveryErrorState;
        public GameObject RecoveryErrorState => _recoveryErrorState;
        [SerializeField] private GameObject _recoverySuccessState;
        public GameObject RecoverySuccessState => _recoverySuccessState;
        [SerializeField] private GameObject _recoveryDisabledState;
        public GameObject RecoveryDisabledState => _recoveryDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
