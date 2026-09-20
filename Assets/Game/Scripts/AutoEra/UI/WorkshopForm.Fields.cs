using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// WorkshopForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/WorkshopForm.contract.json 生成，请勿手改）。
    ///
    /// 与 WorkshopForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// WorkshopForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class WorkshopForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _workshopRecipesContent;
        public RectTransform WorkshopRecipesContent => _workshopRecipesContent;
        [SerializeField] private TMP_Text _workshopRecipesBody;
        public TMP_Text WorkshopRecipesBody => _workshopRecipesBody;
        [SerializeField] private GameObject _workshopRecipesTemplate;
        public GameObject WorkshopRecipesTemplate => _workshopRecipesTemplate;
        [SerializeField] private RectTransform _workshopQueueContent;
        public RectTransform WorkshopQueueContent => _workshopQueueContent;
        [SerializeField] private TMP_Text _workshopQueueBody;
        public TMP_Text WorkshopQueueBody => _workshopQueueBody;
        [SerializeField] private GameObject _workshopQueueTemplate;
        public GameObject WorkshopQueueTemplate => _workshopQueueTemplate;
        [SerializeField] private RectTransform _workshopOutputContent;
        public RectTransform WorkshopOutputContent => _workshopOutputContent;
        [SerializeField] private TMP_Text _workshopOutputBody;
        public TMP_Text WorkshopOutputBody => _workshopOutputBody;
        [SerializeField] private GameObject _workshopOutputTemplate;
        public GameObject WorkshopOutputTemplate => _workshopOutputTemplate;
        [SerializeField] private GameObject _workshopLoadingState;
        public GameObject WorkshopLoadingState => _workshopLoadingState;
        [SerializeField] private GameObject _workshopEmptyState;
        public GameObject WorkshopEmptyState => _workshopEmptyState;
        [SerializeField] private GameObject _workshopErrorState;
        public GameObject WorkshopErrorState => _workshopErrorState;
        [SerializeField] private GameObject _workshopSuccessState;
        public GameObject WorkshopSuccessState => _workshopSuccessState;
        [SerializeField] private GameObject _workshopDisabledState;
        public GameObject WorkshopDisabledState => _workshopDisabledState;
        [SerializeField] private RectTransform _recipeDetailProductContent;
        public RectTransform RecipeDetailProductContent => _recipeDetailProductContent;
        [SerializeField] private TMP_Text _recipeDetailProductBody;
        public TMP_Text RecipeDetailProductBody => _recipeDetailProductBody;
        [SerializeField] private GameObject _recipeDetailProductTemplate;
        public GameObject RecipeDetailProductTemplate => _recipeDetailProductTemplate;
        [SerializeField] private RectTransform _recipeDetailRequirementsContent;
        public RectTransform RecipeDetailRequirementsContent => _recipeDetailRequirementsContent;
        [SerializeField] private TMP_Text _recipeDetailRequirementsBody;
        public TMP_Text RecipeDetailRequirementsBody => _recipeDetailRequirementsBody;
        [SerializeField] private GameObject _recipeDetailRequirementsTemplate;
        public GameObject RecipeDetailRequirementsTemplate => _recipeDetailRequirementsTemplate;
        [SerializeField] private GameObject _recipeDetailLoadingState;
        public GameObject RecipeDetailLoadingState => _recipeDetailLoadingState;
        [SerializeField] private GameObject _recipeDetailEmptyState;
        public GameObject RecipeDetailEmptyState => _recipeDetailEmptyState;
        [SerializeField] private GameObject _recipeDetailErrorState;
        public GameObject RecipeDetailErrorState => _recipeDetailErrorState;
        [SerializeField] private GameObject _recipeDetailSuccessState;
        public GameObject RecipeDetailSuccessState => _recipeDetailSuccessState;
        [SerializeField] private GameObject _recipeDetailDisabledState;
        public GameObject RecipeDetailDisabledState => _recipeDetailDisabledState;
        [SerializeField] private RectTransform _manufacturingQueueCurrentContent;
        public RectTransform ManufacturingQueueCurrentContent => _manufacturingQueueCurrentContent;
        [SerializeField] private TMP_Text _manufacturingQueueCurrentBody;
        public TMP_Text ManufacturingQueueCurrentBody => _manufacturingQueueCurrentBody;
        [SerializeField] private GameObject _manufacturingQueueCurrentTemplate;
        public GameObject ManufacturingQueueCurrentTemplate => _manufacturingQueueCurrentTemplate;
        [SerializeField] private RectTransform _manufacturingQueueWaitingContent;
        public RectTransform ManufacturingQueueWaitingContent => _manufacturingQueueWaitingContent;
        [SerializeField] private TMP_Text _manufacturingQueueWaitingBody;
        public TMP_Text ManufacturingQueueWaitingBody => _manufacturingQueueWaitingBody;
        [SerializeField] private GameObject _manufacturingQueueWaitingTemplate;
        public GameObject ManufacturingQueueWaitingTemplate => _manufacturingQueueWaitingTemplate;
        [SerializeField] private GameObject _manufacturingQueueLoadingState;
        public GameObject ManufacturingQueueLoadingState => _manufacturingQueueLoadingState;
        [SerializeField] private GameObject _manufacturingQueueEmptyState;
        public GameObject ManufacturingQueueEmptyState => _manufacturingQueueEmptyState;
        [SerializeField] private GameObject _manufacturingQueueErrorState;
        public GameObject ManufacturingQueueErrorState => _manufacturingQueueErrorState;
        [SerializeField] private GameObject _manufacturingQueueSuccessState;
        public GameObject ManufacturingQueueSuccessState => _manufacturingQueueSuccessState;
        [SerializeField] private GameObject _manufacturingQueueDisabledState;
        public GameObject ManufacturingQueueDisabledState => _manufacturingQueueDisabledState;
        [SerializeField] private RectTransform _outputCacheItemsContent;
        public RectTransform OutputCacheItemsContent => _outputCacheItemsContent;
        [SerializeField] private TMP_Text _outputCacheItemsBody;
        public TMP_Text OutputCacheItemsBody => _outputCacheItemsBody;
        [SerializeField] private GameObject _outputCacheItemsTemplate;
        public GameObject OutputCacheItemsTemplate => _outputCacheItemsTemplate;
        [SerializeField] private RectTransform _outputCacheBlockContent;
        public RectTransform OutputCacheBlockContent => _outputCacheBlockContent;
        [SerializeField] private TMP_Text _outputCacheBlockBody;
        public TMP_Text OutputCacheBlockBody => _outputCacheBlockBody;
        [SerializeField] private GameObject _outputCacheBlockTemplate;
        public GameObject OutputCacheBlockTemplate => _outputCacheBlockTemplate;
        [SerializeField] private GameObject _outputCacheLoadingState;
        public GameObject OutputCacheLoadingState => _outputCacheLoadingState;
        [SerializeField] private GameObject _outputCacheEmptyState;
        public GameObject OutputCacheEmptyState => _outputCacheEmptyState;
        [SerializeField] private GameObject _outputCacheErrorState;
        public GameObject OutputCacheErrorState => _outputCacheErrorState;
        [SerializeField] private GameObject _outputCacheSuccessState;
        public GameObject OutputCacheSuccessState => _outputCacheSuccessState;
        [SerializeField] private GameObject _outputCacheDisabledState;
        public GameObject OutputCacheDisabledState => _outputCacheDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
