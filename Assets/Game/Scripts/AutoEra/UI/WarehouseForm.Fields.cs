using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// WarehouseForm 的契约绑定字段（由 tools/ui_contract_to_form_script.py 依据
    /// Docs/Development/UI-PrefabLayouts/WarehouseForm.contract.json 生成，请勿手改）。
    ///
    /// 与 WarehouseForm.cs 构成同一个 partial 类：字段与只读属性集中在这里，业务逻辑写在
    /// WarehouseForm.cs。新增节点引用只需改契约再重新生成，不必手写字段。
    /// </summary>
    public sealed partial class WarehouseForm
    {
        [SerializeField] private RectTransform _pageHost;
        public RectTransform PageHost => _pageHost;
        [SerializeField] private Button _backButton;
        public Button BackButton => _backButton;
        [SerializeField] private Button _closeButton;
        public Button CloseButton => _closeButton;
        [SerializeField] private RectTransform _inventoryCatalogContent;
        public RectTransform InventoryCatalogContent => _inventoryCatalogContent;
        [SerializeField] private TMP_Text _inventoryCatalogBody;
        public TMP_Text InventoryCatalogBody => _inventoryCatalogBody;
        [SerializeField] private GameObject _inventoryCatalogTemplate;
        public GameObject InventoryCatalogTemplate => _inventoryCatalogTemplate;
        [SerializeField] private RectTransform _inventoryDetailContent;
        public RectTransform InventoryDetailContent => _inventoryDetailContent;
        [SerializeField] private TMP_Text _inventoryDetailBody;
        public TMP_Text InventoryDetailBody => _inventoryDetailBody;
        [SerializeField] private GameObject _inventoryDetailTemplate;
        public GameObject InventoryDetailTemplate => _inventoryDetailTemplate;
        [SerializeField] private GameObject _inventoryLoadingState;
        public GameObject InventoryLoadingState => _inventoryLoadingState;
        [SerializeField] private GameObject _inventoryEmptyState;
        public GameObject InventoryEmptyState => _inventoryEmptyState;
        [SerializeField] private GameObject _inventoryErrorState;
        public GameObject InventoryErrorState => _inventoryErrorState;
        [SerializeField] private GameObject _inventorySuccessState;
        public GameObject InventorySuccessState => _inventorySuccessState;
        [SerializeField] private GameObject _inventoryDisabledState;
        public GameObject InventoryDisabledState => _inventoryDisabledState;
        [SerializeField] private RectTransform _itemDetailItemContent;
        public RectTransform ItemDetailItemContent => _itemDetailItemContent;
        [SerializeField] private TMP_Text _itemDetailItemBody;
        public TMP_Text ItemDetailItemBody => _itemDetailItemBody;
        [SerializeField] private GameObject _itemDetailItemTemplate;
        public GameObject ItemDetailItemTemplate => _itemDetailItemTemplate;
        [SerializeField] private RectTransform _itemDetailStockContent;
        public RectTransform ItemDetailStockContent => _itemDetailStockContent;
        [SerializeField] private TMP_Text _itemDetailStockBody;
        public TMP_Text ItemDetailStockBody => _itemDetailStockBody;
        [SerializeField] private GameObject _itemDetailStockTemplate;
        public GameObject ItemDetailStockTemplate => _itemDetailStockTemplate;
        [SerializeField] private GameObject _itemDetailLoadingState;
        public GameObject ItemDetailLoadingState => _itemDetailLoadingState;
        [SerializeField] private GameObject _itemDetailEmptyState;
        public GameObject ItemDetailEmptyState => _itemDetailEmptyState;
        [SerializeField] private GameObject _itemDetailErrorState;
        public GameObject ItemDetailErrorState => _itemDetailErrorState;
        [SerializeField] private GameObject _itemDetailSuccessState;
        public GameObject ItemDetailSuccessState => _itemDetailSuccessState;
        [SerializeField] private GameObject _itemDetailDisabledState;
        public GameObject ItemDetailDisabledState => _itemDetailDisabledState;
        [SerializeField] private RectTransform _warehouseRecordsEventsContent;
        public RectTransform WarehouseRecordsEventsContent => _warehouseRecordsEventsContent;
        [SerializeField] private TMP_Text _warehouseRecordsEventsBody;
        public TMP_Text WarehouseRecordsEventsBody => _warehouseRecordsEventsBody;
        [SerializeField] private GameObject _warehouseRecordsEventsTemplate;
        public GameObject WarehouseRecordsEventsTemplate => _warehouseRecordsEventsTemplate;
        [SerializeField] private RectTransform _warehouseRecordsDetailContent;
        public RectTransform WarehouseRecordsDetailContent => _warehouseRecordsDetailContent;
        [SerializeField] private TMP_Text _warehouseRecordsDetailBody;
        public TMP_Text WarehouseRecordsDetailBody => _warehouseRecordsDetailBody;
        [SerializeField] private GameObject _warehouseRecordsDetailTemplate;
        public GameObject WarehouseRecordsDetailTemplate => _warehouseRecordsDetailTemplate;
        [SerializeField] private GameObject _warehouseRecordsLoadingState;
        public GameObject WarehouseRecordsLoadingState => _warehouseRecordsLoadingState;
        [SerializeField] private GameObject _warehouseRecordsEmptyState;
        public GameObject WarehouseRecordsEmptyState => _warehouseRecordsEmptyState;
        [SerializeField] private GameObject _warehouseRecordsErrorState;
        public GameObject WarehouseRecordsErrorState => _warehouseRecordsErrorState;
        [SerializeField] private GameObject _warehouseRecordsSuccessState;
        public GameObject WarehouseRecordsSuccessState => _warehouseRecordsSuccessState;
        [SerializeField] private GameObject _warehouseRecordsDisabledState;
        public GameObject WarehouseRecordsDisabledState => _warehouseRecordsDisabledState;
        [SerializeField] private GameObject[] _pageRoots;
        public GameObject[] PageRoots => _pageRoots;
    }
}
