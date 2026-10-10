using System;
using System.Collections.Generic;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    public sealed partial class WarehouseForm : AutoEraShellFormBase
    {
        private WarehouseReadModel _readModel;
        private int _selectedItem, _selectedReceipt;
        public bool IsDomainWired => _readModel != null && _readModel.Snapshot.State==UiDataState.Ready;
        public WarehouseDomainSnapshot InventorySnapshot => _readModel?.Snapshot;
        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton!=null) _backButton.onClick.AddListener(Back);
            if (_closeButton!=null) _closeButton.onClick.AddListener(CloseSelf);
            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                switch (button.name)
                {
                    case "Btn_InventoryInspect": button.onClick.AddListener(()=>ShowFormPage(1)); break;
                    case "Btn_InventoryRecords": button.onClick.AddListener(()=>ShowFormPage(2)); break;
                    case "Btn_ItemDetailBack": button.onClick.AddListener(()=>ShowFormPage(0)); break;
                    case "Btn_InventorySell": case "Btn_ItemDetailSell":
                    case "Btn_WarehouseRecordsFilter": case "Btn_WarehouseRecordsLocate": case "Btn_WarehouseRecordsDestination":
                        button.interactable=false; break;
                }
            }
        }
        protected override void OnAutoEraOpen()
        {
            Release(); _selectedItem=0; _selectedReceipt=0;
            _readModel=new WarehouseReadModel(SessionOrNull?.World?.Resources,ItemName);
            ShowFormPage(0); ApplyDefaultFocus(_backButton!=null?_backButton.gameObject:null,null);
        }
        protected override void OnUpdate(float elapseSeconds,float realElapseSeconds)
        { base.OnUpdate(elapseSeconds,realElapseSeconds); if (_readModel!=null && _readModel.NeedsRefresh && _readModel.Refresh()) Render(); }
        protected override void OnAutoEraClose(bool isShutdown) => Release();
        protected override void OnAutoEraRecycle() { Release(); base.OnAutoEraRecycle(); }
        private void Release() { _readModel?.Dispose(); _readModel=null; }
        private void Back() { if (CurrentPage>0) ShowFormPage(0); else CloseSelf(); }
        public bool ShowFormPage(int page) { if (!ShowPage(_pageRoots,page)) return false; Render(); return true; }
        private void Render()
        {
            if (_readModel==null) return; var snapshot=_readModel.Snapshot; bool unavailable=snapshot.State==UiDataState.Unavailable;
            States(_inventoryLoadingState,_inventoryEmptyState,_inventoryErrorState,_inventorySuccessState,_inventoryDisabledState,unavailable,snapshot.Reason);
            States(_itemDetailLoadingState,_itemDetailEmptyState,_itemDetailErrorState,_itemDetailSuccessState,_itemDetailDisabledState,unavailable,snapshot.Reason);
            States(_warehouseRecordsLoadingState,_warehouseRecordsEmptyState,_warehouseRecordsErrorState,_warehouseRecordsSuccessState,_warehouseRecordsDisabledState,unavailable,snapshot.Reason);
            RenderListRows(_inventoryCatalogTemplate,_inventoryCatalogContent,snapshot.Items.Count,(index,row)=>
            { var item=snapshot.Items[index]; row.Bind(index,item.Name,"余额 "+item.Balance+"　仓库 "+item.WarehouseUnits,SelectItem); });
            RenderDetailRows(_inventoryDetailTemplate,_inventoryDetailContent,snapshot.Summary);
            if (_inventoryCatalogBody!=null) _inventoryCatalogBody.SetText(unavailable?snapshot.Reason:"选择物品查看各处数量。");
            if (_inventoryDetailBody!=null) _inventoryDetailBody.SetText("数量来自实际结算。出售功能尚未接入。");
            var detail=new List<UiDetailField>();
            if (_selectedItem>=0 && _selectedItem<snapshot.Items.Count)
            { var item=snapshot.Items[_selectedItem]; detail.Add(new UiDetailField("物品",item.Name)); detail.Add(new UiDetailField("物品编号",item.Id)); detail.Add(new UiDetailField("全局余额",item.Balance.ToString())); detail.Add(new UiDetailField("仓库本地",item.WarehouseUnits.ToString())); detail.Add(new UiDetailField("机器货舱",item.CargoUnits.ToString())); detail.Add(new UiDetailField("现场待运",item.GroundUnits.ToString())); }
            RenderDetailRows(_itemDetailItemTemplate,_itemDetailItemContent,detail);
            RenderDetailRows(_itemDetailStockTemplate,_itemDetailStockContent,snapshot.Summary);
            if (_itemDetailItemBody!=null) _itemDetailItemBody.SetText("已装载但尚未交付的物品仍属于机器货舱。");
            if (_itemDetailStockBody!=null) _itemDetailStockBody.SetText("基础资源入库后转为全局余额，不占本地库存容量。");
            RenderListRows(_warehouseRecordsEventsTemplate,_warehouseRecordsEventsContent,snapshot.Receipts.Count,(index,row)=>
            { var receipt=snapshot.Receipts[index]; row.Bind(index,"任务 "+receipt.TaskId,"实入 "+receipt.ActualUnits+"　累计 "+receipt.TotalCommittedUnits,SelectReceipt); });
            var record=new List<UiDetailField>();
            if (_selectedReceipt>=0 && _selectedReceipt<snapshot.Receipts.Count)
            { var receipt=snapshot.Receipts[_selectedReceipt]; record.Add(new UiDetailField("任务",receipt.TaskId.ToString())); record.Add(new UiDetailField("事务",receipt.TransactionId.ToString())); record.Add(new UiDetailField("完成序列",receipt.CompletionSequence.ToString())); record.Add(new UiDetailField("来源",receipt.Source.Id.ToString())); record.Add(new UiDetailField("仓库",receipt.Destination.Id.ToString())); record.Add(new UiDetailField("实际单位",receipt.ActualUnits.ToString())); record.Add(new UiDetailField("去向",receipt.Route.ToString())); }
            RenderDetailRows(_warehouseRecordsDetailTemplate,_warehouseRecordsDetailContent,record);
            if (_warehouseRecordsEventsBody!=null) _warehouseRecordsEventsBody.SetText(snapshot.Receipts.Count==0?"尚无实际入库记录。":"每条记录对应一次已提交的单位结算。");
            if (_warehouseRecordsDetailBody!=null) _warehouseRecordsDetailBody.SetText("任务、事务和序列可追溯；筛选与定位尚未接入。");
        }
        private void SelectItem(int index) { _selectedItem=index; ShowFormPage(1); }
        private void SelectReceipt(int index) { _selectedReceipt=index; Render(); }
        private static string ItemName(string id)
        { var table=GF.DataTable.GetDataTable<AutoEra.DataTable.FirstVersionObjects>(); return int.TryParse(id,out var number)?table?.GetDataRow(number)?.Name ?? id:id; }
        private void States(GameObject loading,GameObject empty,GameObject error,GameObject success,GameObject disabled,bool unavailable,string reason)
        { SetState(loading,false); SetState(empty,false); SetState(error,false); SetState(success,false); SetState(disabled,unavailable); if (unavailable) WriteStateCard(disabled,reason); }
        protected override void OnOperationPresentationChanged(AutoEraUiOperationSnapshot snapshot,AutoEraUiOperationPresentation presentation) { }
    }
}
