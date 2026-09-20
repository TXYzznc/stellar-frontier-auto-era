using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// TutorialForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/TutorialForm.contract.json 生成，请勿手改）。
    ///
    /// 与 TutorialForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// TutorialForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class TutorialForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _tutorialLessonContent;
        public RectTransform TutorialLessonContent => _tutorialLessonContent;
        [SerializeField] private TMP_Text _tutorialLessonBody;
        public TMP_Text TutorialLessonBody => _tutorialLessonBody;
        [SerializeField] private GameObject _tutorialLessonTemplate;
        public GameObject TutorialLessonTemplate => _tutorialLessonTemplate;
        [SerializeField] private RectTransform _tutorialLinksContent;
        public RectTransform TutorialLinksContent => _tutorialLinksContent;
        [SerializeField] private TMP_Text _tutorialLinksBody;
        public TMP_Text TutorialLinksBody => _tutorialLinksBody;
        [SerializeField] private GameObject _tutorialLinksTemplate;
        public GameObject TutorialLinksTemplate => _tutorialLinksTemplate;
        [SerializeField] private GameObject _tutorialLoadingState;
        public GameObject TutorialLoadingState => _tutorialLoadingState;
        [SerializeField] private GameObject _tutorialEmptyState;
        public GameObject TutorialEmptyState => _tutorialEmptyState;
        [SerializeField] private GameObject _tutorialErrorState;
        public GameObject TutorialErrorState => _tutorialErrorState;
        [SerializeField] private GameObject _tutorialSuccessState;
        public GameObject TutorialSuccessState => _tutorialSuccessState;
        [SerializeField] private GameObject _tutorialDisabledState;
        public GameObject TutorialDisabledState => _tutorialDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
