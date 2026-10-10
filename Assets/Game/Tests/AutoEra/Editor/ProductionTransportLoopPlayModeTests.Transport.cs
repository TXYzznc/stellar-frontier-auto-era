using System;
using System.Collections;
using System.IO;
using AutoEra.Algorithms;
using AutoEra.Application;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.UI;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed partial class ProductionTransportLoopPlayModeTests
    {
        private static UnityGameFramework.Runtime.UIComponent Ui => UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.UIComponent>();
        private static IEnumerator VerifyTransport(InitialRegionScene entry,AutoEraWorldSession session,AutoEraApplicationContext application,PersistentId point,string item)
        {
            Assert.That(entry.TryGetTransferEndpoint(point,out var source),Is.True);
            RegionTransferEndpoint destination=null;
            foreach(var value in entry.Region.Objects) if(entry.TryGetTransferEndpoint(value.Id,out var endpoint) && endpoint.Warehouse) destination=endpoint;
            Assert.That(destination,Is.Not.Null);
            Assert.That(session.Resources.Authority.Balance(item),Is.Zero);
            var catalog=MachineCatalog.FromLoadedGameData(); catalog.TryGetMachine(10011,out var carrier); catalog.TryGetComponent(20011,out var coreDefinition);
            catalog.TryGetComponent(21011,out var sensorDefinition); catalog.TryGetComponent(22011,out var armDefinition);
            var machine=session.Machines.Create(carrier); var core=session.Machines.CreateComponent(coreDefinition); var sensor=session.Machines.CreateComponent(sensorDefinition); var arm=session.Machines.CreateComponent(armDefinition);
            Assert.That(session.Machines.Install(machine.Id,ManagementOrigin.Library,core.Id,0),Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(session.Machines.Install(machine.Id,ManagementOrigin.Library,sensor.Id,0),Is.EqualTo(MachineManagementResult.Completed));
            Assert.That(session.Machines.Install(machine.Id,ManagementOrigin.Library,arm.Id,0),Is.EqualTo(MachineManagementResult.Completed));
            using(var flow=new MachineDeploymentFlow(session,entry.Region))
            { Assert.That(flow.TryBegin(machine.Id,out var reason),Is.True,reason); flow.Preview.Move(new Vector2(source.DockPosition.x,source.DockPosition.z-2)); Assert.That(flow.TryCommit(out _,out reason),Is.True,reason); }
            Assert.That(entry.TrySpawnMachine(machine.Id,out var spawn),Is.True,spawn); yield return Wait(null,()=>entry.FindMachineView(machine.Id)!=null);
            Assert.That(entry.MachineRuntimes.TryGet(machine.Id,out var runtime),Is.True); machine.Activate(ManagementOrigin.Field); machine.UpdateEnvironment(true,true); machine.SetRunState(ManagementOrigin.Field,MachineRunState.Running);
            yield return Wait(entry,()=>entry.ProductionTools.ReadyCount==2,runtime,arm.Id,"机械臂加载");
            Assert.That(runtime.Hardware.TryBindEndpoint(arm.Id,source.Id,out var armGeneration,out var error),Is.True,error);
            Assert.That(runtime.Hardware.TryBindEndpoint(sensor.Id,source.Id,out var sensorGeneration,out error),Is.True,error);
            var graph=InitialAlgorithmTemplates.Transport(); session.IdAllocator.TryAllocate(out var id); graph.DocumentId=id.Value;
            graph.Nodes.Find(node=>node.Id==5).Default=Position(source.DockPosition);
            graph.Nodes.Find(node=>node.Id==6).Default=Position(destination.DockPosition);
            graph.Nodes.Find(node=>node.Id==7).Default=AlgorithmValue.Numeric(2);
            graph.Nodes.Find(node=>node.Id==8).Default=new AlgorithmValue { Type=AlgorithmType.Of(AlgorithmValueKind.Enumeration),EnumValue=int.Parse(item) };
            graph.Bindings.Add(Binding("source_cached",sensor.Id,source.Id,sensorGeneration)); graph.Bindings.Add(Binding("source_amount",sensor.Id,source.Id,sensorGeneration));
            graph.Bindings.Add(Binding("arm_load",arm.Id,source.Id,armGeneration)); graph.Bindings.Add(Binding("arm_unload",arm.Id,destination.Id,armGeneration));
            Assert.That(runtime.Instances.AddDraft(graph),Is.True); Assert.That(runtime.TryActivateDraft(graph.DocumentId,out error),Is.True,error);
            var uiSession=AutoEraUiSession.ForWorld(application,session,entry.Region,machineRuntimes:entry.MachineRuntimes);
            var debugger=UnityGameFramework.Runtime.GameEntry.GetComponent<UnityGameFramework.Runtime.DebuggerComponent>(); if(debugger!=null) debugger.ActiveWindow=false;
            foreach(var form in Ui.GetAllLoadedUIForms()) if(form.Logic is MainMenuForm) Ui.CloseUIForm(form.SerialId);
            int warehouseId=AutoEraUiNavigator.Open(UIViews.WarehouseForm,uiSession);
            yield return Wait(null,()=>Ui.HasUIForm(warehouseId)); var warehouse=(WarehouseForm)Ui.GetUIForm(warehouseId).Logic;
            Assert.That(warehouse.IsDomainWired,Is.True);
            entry.Region.Select(point,false);
            int fieldId=AutoEraUiNavigator.Open(UIViews.FieldHudDetailForm,uiSession,new AutoEraUiPageRequest(item==ResourceItemCatalog.Wood?10:11));
            yield return Wait(null,()=>Ui.HasUIForm(fieldId)); yield return UiCapture(item+"-field-1920.png"); Ui.CloseUIForm(fieldId);
            yield return Wait(entry,()=>runtime.Context.Cargo.Count(item)==1,runtime,arm.Id);
            machine.SetPowerSwitch(ManagementOrigin.Field,false); long cargo=runtime.Context.Cargo.Used;
            for(int i=0;i<20;i++) { entry.Advance(.05); yield return null; }
            Assert.That(runtime.Context.Cargo.Used,Is.EqualTo(cargo));
            Assert.That(session.Resources.Transport.TryRead(machine.Id,item,out var partial),Is.True); Assert.That(partial.Pending,Is.EqualTo(1));
            yield return UiCapture(item+"-loading-1920.png"); Ui.CloseUIForm(warehouseId); yield return null; Assert.That(warehouse.InventorySnapshot,Is.Null);
            machine.SetPowerSwitch(ManagementOrigin.Field,true);
            yield return Wait(entry,()=>session.Resources.Authority.Balance(item)==2,runtime,arm.Id);
            Assert.That(runtime.Context.Cargo.Count(item),Is.Zero); Assert.That(session.Resources.Transport.TryRead(machine.Id,item,out var delivered),Is.True);
            Assert.That(delivered.Source,Is.EqualTo(source.Id)); Assert.That(delivered.Destination,Is.EqualTo(destination.Id)); Assert.That(delivered.Loaded,Is.EqualTo(2)); Assert.That(delivered.Delivered,Is.EqualTo(2));
            Assert.That(session.Resources.Authority.TryReadContainer(source.Owner,out var pile),Is.True); Assert.That(pile.Used,Is.Zero);
            Assert.That(session.Resources.Authority.TryCapture(out var inventory),Is.True); int units=0;
            foreach(var receipt in inventory.Receipts) if(receipt.Destination.Id==destination.Id) { units+=receipt.ActualUnits; Assert.That(receipt.TaskId,Is.EqualTo(delivered.Task)); }
            Assert.That(units,Is.EqualTo(2));
            int reopened=AutoEraUiNavigator.Open(UIViews.WarehouseForm,uiSession); yield return Wait(null,()=>Ui.HasUIForm(reopened));
            warehouse=(WarehouseForm)Ui.GetUIForm(reopened).Logic; var row=FindItem(warehouse.InventorySnapshot,item); Assert.That(row.Balance,Is.EqualTo(2)); Assert.That(row.CargoUnits,Is.Zero); Assert.That(row.GroundUnits,Is.Zero);
            yield return UiCapture(item+"-warehouse-1920.png"); warehouse.ShowFormPage(2); yield return UiCapture(item+"-receipts-1920.png"); Ui.CloseUIForm(reopened);
            for(int i=0;i<20;i++) { entry.Advance(.05); yield return null; } Assert.That(session.Resources.Authority.Balance(item),Is.EqualTo(2),"Repeated triggers must not duplicate delivery.");
        }
        private static UiInventoryItem FindItem(WarehouseDomainSnapshot snapshot,string item)
        { foreach(var row in snapshot.Items) if(row.Id==item) return row; Assert.Fail("Missing actual inventory row "+item); return default; }
        private static AlgorithmValue Position(Vector3 position) => new AlgorithmValue { Type=AlgorithmType.Of(AlgorithmValueKind.Position),X=position.x,Y=position.y,Z=position.z };
        private static AlgorithmBinding Binding(string key,PersistentId component,PersistentId target,ulong generation) => new AlgorithmBinding { Key=key,ComponentId=component.Value,TargetId=target.Value,Generation=generation,Type=AlgorithmType.Of(AlgorithmValueKind.Number) };
        private static IEnumerator UiCapture(string name)
        {
            yield return null; yield return null; Assert.That(Screen.width,Is.EqualTo(1920)); Assert.That(Screen.height,Is.EqualTo(1080));
            const string directory="openspec/changes/b45-p4011-p4014-p4015-p4016-production-transport-loop/evidence"; Directory.CreateDirectory(directory); string path=Path.GetFullPath(Path.Combine(directory,name));
            if(File.Exists(path)) File.Delete(path); ScreenCapture.CaptureScreenshot(path); double until=Time.realtimeSinceStartupAsDouble+10;
            while(!File.Exists(path) && Time.realtimeSinceStartupAsDouble<until) yield return null; Assert.That(File.Exists(path),Is.True);
        }
    }
}
