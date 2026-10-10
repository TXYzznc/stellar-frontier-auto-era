using System.Collections.Generic;
using AutoEra.Logistics;
using AutoEra.ResourcePoints;
using UnityEngine;

namespace AutoEra.UI
{
    public sealed partial class FieldHudDetailForm
    {
        private CargoOwnershipAuthority _productionAuthority;
        private bool _productionDirty;
        private void ObserveProduction()
        {
            _productionAuthority=SessionOrNull?.World?.Resources.Authority;
            if(_productionAuthority!=null) { _productionAuthority.ContainerChanged+=OnProductionInventoryChanged; _productionAuthority.Produced+=OnProductionSettled; }
            _productionDirty=true;
        }
        private void OnProductionInventoryChanged(CargoContainer container) => _productionDirty=true;
        private void OnProductionSettled(ResourceProductionReceipt receipt) => _productionDirty=true;
        private void ReleaseProductionObservation()
        {
            if(_productionAuthority!=null) { _productionAuthority.ContainerChanged-=OnProductionInventoryChanged; _productionAuthority.Produced-=OnProductionSettled; }
            _productionAuthority=null; _productionDirty=false;
        }
        protected override void OnUpdate(float elapseSeconds,float realElapseSeconds)
        { base.OnUpdate(elapseSeconds,realElapseSeconds); if(_productionDirty && (CurrentPage==10 || CurrentPage==11)) RenderCurrentPage(); }
        private bool RenderProductionPage(GameObject page,RegionDomainSnapshot snapshot)
        {
            bool forestPage=CurrentPage==10; string prefix=forestPage?"Forest":"Mineral"; var production=SessionOrNull?.World?.Production;
            ForestProduction forest=null; MineralProduction mineral=null;
            bool ready=snapshot.HasSelection && production!=null && (forestPage?production.TryGetForest(snapshot.SelectedId,out forest):production.TryGetMineral(snapshot.SelectedId,out mineral));
            ResourceInventorySnapshot inventory=null;
            if(ready && (_productionAuthority==null || !_productionAuthority.TryCapture(out inventory))) return false;
            string reason=ready?null:"请选择已接入生产的"+(forestPage?"人工林":"矿脉")+"。";
            SetState(FindPageObject(page,"Grp_"+prefix+"LoadingState"),false); SetState(FindPageObject(page,"Grp_"+prefix+"ErrorState"),false);
            SetState(FindPageObject(page,"Grp_"+prefix+"SuccessState"),false); SetState(FindPageObject(page,"Grp_"+prefix+"DisabledState"),!ready); SetState(FindPageObject(page,"Grp_"+prefix+"EmptyState"),false);
            if(!ready) WriteStateCard(FindPageObject(page,"Grp_"+prefix+"DisabledState"),reason);
            var fields=new List<UiDetailField>(); var sensor=new List<UiDetailField>(); var records=new List<UiDetailField>();
            if(ready)
            {
                if(forestPage)
                {
                    fields.Add(new UiDetailField("成熟树木",forest.MatureCount.ToString())); fields.Add(new UiDetailField("树木总数",forest.Count.ToString())); fields.Add(new UiDetailField("待运木材",forest.CachedUnits.ToString()));
                    int growing=0,falling=0,stumps=0; for(int i=0;i<forest.Count;i++) { var tree=forest.ReadAt(i); if(tree.Stage==TreeStage.Growing) growing++; else if(tree.Stage==TreeStage.Falling) falling++; else if(tree.Stage==TreeStage.Stump) stumps++; }
                    fields.Add(new UiDetailField("生长／倒木／树桩",growing+" / "+falling+" / "+stumps));
                    sensor.Add(new UiDetailField("mature_count",forest.MatureCount.ToString())); sensor.Add(new UiDetailField("cached",forest.CachedUnits.ToString()));
                }
                else
                {
                    fields.Add(new UiDetailField("剩余矿量",mineral.RemainingUnits.ToString())); fields.Add(new UiDetailField("累计采出",mineral.ProducedUnits.ToString())); fields.Add(new UiDetailField("待运矿石",mineral.CachedUnits.ToString()));
                    fields.Add(new UiDetailField("可见矿块",mineral.VisibleRockCount.ToString())); sensor.Add(new UiDetailField("remaining",mineral.RemainingUnits.ToString())); sensor.Add(new UiDetailField("cached",mineral.CachedUnits.ToString()));
                }
                foreach(var receipt in inventory.ProductionReceipts) if(receipt.Owner.Id==snapshot.SelectedId) records.Add(new UiDetailField("生产 "+receipt.ProducerId+" / "+receipt.Sequence,receipt.Item+"　实际 "+receipt.Units));
                foreach(var receipt in inventory.Receipts) if(receipt.Source.Id==snapshot.SelectedId && receipt.ActualUnits>0) records.Add(new UiDetailField("装载任务 "+receipt.TaskId,"实际 "+receipt.ActualUnits+"　事务 "+receipt.TransactionId));
            }
            ProductionColumn(page,prefix,"Public",fields,ready?(forestPage?"成熟树木可采集。":"采出的矿石在现场等待运输。"):reason);
            ProductionColumn(page,prefix,"Sensor",sensor,"可供传感器读取的公开量。");
            ProductionColumn(page,prefix,"Record",records,records.Count==0?"尚无已提交的生产或装载记录。":"实际生产和装载记录。");
            return true;
        }
        private void ProductionColumn(GameObject page,string prefix,string column,IReadOnlyList<UiDetailField> fields,string body)
        {
            var content=FindPageTransform(page,"Content_"+prefix+column,null) as RectTransform; var template=FindPageTransform(page,"Item_"+prefix+column+"Template",null);
            if(content==null || template==null) return; RenderDetailRows(template.gameObject,content,fields);
            var bodyTransform=FindPageTransform(page,"Txt_"+prefix+column+"Body",null); var text=bodyTransform!=null?bodyTransform.GetComponent<TMPro.TMP_Text>():null; if(text!=null) text.SetText(body);
        }
    }
}
