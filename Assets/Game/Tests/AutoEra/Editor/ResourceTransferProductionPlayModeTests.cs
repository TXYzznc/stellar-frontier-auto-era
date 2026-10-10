using System;
using System.Collections;
using AutoEra.Application;
using AutoEra.Logistics;
using AutoEra.Machines;
using AutoEra.World;
using AutoEra.World.Identity;
using AutoEra.World.Region;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace AutoEra.Tests.Editor
{
    public sealed class ResourceTransferProductionPlayModeTests
    {
        [UnityTest, Timeout(300000)]
        public IEnumerator FormalRegion_WarehouseAndMachineShareAuthorityWithHiddenAndReleasedViews()
        {
            const string launch = "Assets/Game/Scene/Launch.unity";
            if (!SceneManager.GetSceneByPath(launch).isLoaded && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))
                SceneManager.SetActiveScene(EditorSceneManager.OpenScene(launch, OpenSceneMode.Additive));
            yield return new EnterPlayMode(); yield return Verify(); yield return new ExitPlayMode();
        }
        private static IEnumerator Verify()
        {
            yield return Wait(() => MachineCatalog.IsGameDataLoaded);
            Assert.That(Screen.width, Is.EqualTo(1920)); Assert.That(Screen.height, Is.EqualTo(1080));
            const string path = "Assets/Game/Scene/InitialRegion.unity";
            var loading = EditorSceneManager.LoadSceneAsyncInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Additive));
            while (!loading.isDone) yield return null;
            var scene = SceneManager.GetSceneByPath(path); InitialRegionScene entry = null;
            foreach (var root in scene.GetRootGameObjects()) if (root.TryGetComponent(out InitialRegionScene found)) entry = found;
            Assert.That(entry, Is.Not.Null);
            using (var context = new AutoEraApplicationCompositionRoot().Create())
            {
                context.TryCreateWorldSession(0, out var session);
                try
                {
                    bool ready = false; string failure = null;
                    entry.InitializeRuntime(session, () => ready = true, e => failure = e);
                    yield return Wait(() => ready || failure != null); Assert.That(failure, Is.Null);
                    Assert.That(session.Resources.CatalogReady, Is.True);
                    CargoContainer warehouse = null;
                    foreach (var obj in entry.Region.Objects)
                        if (session.Resources.Authority.TryReadContainer(new CargoOwner(CargoOwnerKind.Receiver, obj.Id), out var candidate) && candidate.Kind == CargoContainerKind.Warehouse)
                            warehouse = candidate;
                    Assert.That(warehouse, Is.Not.Null, "The actual warehouse prefab must bind its configured inventory during formal initialization.");
                    Assert.That(warehouse.Capacity, Is.EqualTo(120));
                    var catalog = MachineCatalog.FromLoadedGameData(); catalog.TryGetMachine(10011, out var definition);
                    var machine = session.Machines.Create(definition);
                    using (var flow = new MachineDeploymentFlow(session, entry.Region))
                    {
                        Assert.That(flow.TryBegin(machine.Id, out var reason), Is.True, reason); flow.Preview.Move(new Vector2(20, -25));
                        Assert.That(flow.TryCommit(out _, out reason), Is.True, reason);
                    }
                    Assert.That(entry.TrySpawnMachine(machine.Id, out var spawn), Is.True, spawn);
                    yield return Wait(() => entry.FindMachineView(machine.Id) != null);
                    Assert.That(entry.MachineRuntimes.TryGet(machine.Id, out var runtime), Is.True);
                    var authority = session.Resources.Authority;
                    session.IdAllocator.TryAllocate(out var sourceId);
                    var ground = authority.RegisterContainer(new CargoOwner(CargoOwnerKind.WorldFree, sourceId), CargoContainerKind.WorldFree, long.MaxValue);
                    // Prepare an initial physical lot only. Runtime, warehouse, cargo projection and scene wiring are created by production entry points.
                    Assert.That(authority.TryMint(ground.Owner, ResourceItemCatalog.Ore, 10, out var lot, out var mintReason), Is.True, mintReason);
                    var machineOwner = session.Resources.MachineOwner(machine);
                    authority.TryReadContainer(machineOwner, out var cargo);
                    session.IdAllocator.TryAllocate(out var transaction); session.IdAllocator.TryAllocate(out var task);
                    Assert.That(authority.TryReserve(transaction, task, lot.Id, lot.Version, machineOwner, cargo.Generation, 10, out var load, out var reasonLoad), Is.True, reasonLoad);
                    authority.Commit(load, 1, 10);
                    Assert.That(runtime.Context.Cargo, Is.SameAs(session.Resources.GetCargo(machine)));
                    Assert.That(runtime.Context.Cargo.Count(ResourceItemCatalog.Ore), Is.EqualTo(10)); Assert.That(machine.UsedCapacity, Is.EqualTo(10));
                    Assert.That(authority.TryReadLot(lot.Id, out lot), Is.True);
                    session.IdAllocator.TryAllocate(out transaction); session.IdAllocator.TryAllocate(out task);
                    Assert.That(authority.TryReserve(transaction, task, lot.Id, lot.Version, warehouse.Owner, warehouse.Generation, 10, out var unload, out var unloadReason), Is.True, unloadReason);
                    var view = entry.FindMachineView(machine.Id); view.gameObject.SetActive(false); yield return null;
                    Assert.That(authority.TryReadLot(lot.Id, out lot), Is.True); Assert.That(lot.Units, Is.EqualTo(10));
                    Assert.That(lot.Owner, Is.EqualTo(machineOwner)); Assert.That(lot.Version, Is.EqualTo(2));
                    Assert.That(runtime.Context.Cargo.Count(ResourceItemCatalog.Ore), Is.EqualTo(10));
                    Assert.That(authority.TryCapture(out var snapshot), Is.True);
                    bool pending = false;
                    foreach (var responsibility in snapshot.Transactions)
                        if (responsibility.Reservation.TransactionId == unload.TransactionId)
                        { pending = responsibility.RemainingReservedUnits == 10 && responsibility.Reservation.TaskId == task; }
                    Assert.That(pending, Is.True, "Culling preserves the unresolved delivery and its task identity.");
                    authority.Commit(unload, 1, 10); authority.Commit(unload, 1, 10);
                    Assert.That(authority.Balance(ResourceItemCatalog.Ore), Is.EqualTo(10)); Assert.That(warehouse.Used, Is.Zero);
                    Assert.That(runtime.Context.Cargo.Used, Is.Zero); Assert.That(machine.UsedCapacity, Is.Zero);
                    // Region/runtime disposal leaves world-owned inventory and settled receipts intact.
                    entry.Release();
                    Assert.That(session.Resources.GetCargo(machine).Used, Is.Zero); Assert.That(authority.Balance(ResourceItemCatalog.Ore), Is.EqualTo(10));
                    Assert.That(authority.TryReadResult(unload.TransactionId, 1, out var result), Is.True); Assert.That(result.ActualUnits, Is.EqualTo(10));
                }
                finally { entry.Release(); }
            }
            var unloading = SceneManager.UnloadSceneAsync(scene); while (unloading != null && !unloading.isDone) yield return null;
        }
        private static IEnumerator Wait(Func<bool> predicate)
        { double until = Time.realtimeSinceStartupAsDouble + 30; while (!predicate() && Time.realtimeSinceStartupAsDouble < until) yield return null; Assert.That(predicate(), Is.True, "Formal production initialization timed out."); }
    }
}
