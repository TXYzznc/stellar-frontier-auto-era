using System;
using AutoEra.Buildings;
using AutoEra.Algorithms;
using AutoEra.Logistics;
using AutoEra.ResourcePoints;
using AutoEra.World.Identity;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class ForestMineralProductionEditModeTests
    {
        private PersistentIdAllocator _ids;
        private PersistentObjectRegistry _registry;
        private CargoOwnershipAuthority _cargo;
        private ProductionRules _rules;
        private ForestProduction _forest;
        private PersistentId Next() { Assert.That(_ids.TryAllocate(out var id), Is.True); return id; }
        [SetUp] public void Setup()
        {
            _ids = new PersistentIdAllocator(); _registry = new PersistentObjectRegistry(_ids);
            _cargo = new CargoOwnershipAuthority(_ids, new ResourceItemCatalog(new[] {
                new ResourceItemDefinition(ResourceItemCatalog.Wood, CargoItemClass.CommonResource),
                new ResourceItemDefinition(ResourceItemCatalog.Ore, CargoItemClass.CommonResource) }), _registry);
            var config = ScriptableObject.CreateInstance<ForestMineralProductionConfig>();
            _rules = config.Read(); UnityEngine.Object.DestroyImmediate(config);
            _forest = new ForestProduction(Next(), _ids, _registry, _rules, _cargo, new[] { Vector3.zero, new Vector3(2, 0, 0) }, 0);
        }
        [TearDown] public void Teardown() { _forest.Dispose(); _cargo.Dispose(); }
        private MineralProduction Deposit(int size = 0) => new MineralProduction(Next(), size, 123, _rules, _cargo, 0);
        [Test] public void HeightCurve_IsInvariantAcrossTickPartitionsAndHealsTwiceGrowth()
        {
            double whole = _rules.Grow(.5, 500), partitioned = .5;
            for (int i = 0; i < 500; i++) partitioned = _rules.Grow(partitioned, 1);
            Assert.That(partitioned, Is.EqualTo(whole).Within(1e-9));
            var tree = _forest.ReadAt(0); Assert.That(_forest.TryCut(tree.Id, .3, 0, 0, out _, out _), Is.True);
            _forest.Advance(10000); var grown = _forest.ReadAt(0);
            Assert.That(grown.HP, Is.EqualTo(tree.HP - .3 + 2 * (grown.Height - tree.Height)).Within(1e-8));
            Assert.That(grown.Height, Is.LessThanOrEqualTo(_rules.MaximumHeight));
        }
        [Test] public void ResumeCut_UsesOriginalTreeAndProducesOnceAfterSettlement()
        {
            var tree = _forest.ReadAt(0);
            Assert.That(_forest.TryCut(tree.Id, 1, .25, 0, out var fell, out _), Is.True); Assert.That(fell, Is.False);
            double pausedHP = _forest.ReadAt(0).HP; // No contribution submitted while powered off.
            Assert.That(pausedHP, Is.EqualTo(2));
            Assert.That(_forest.TryCut(tree.Id, 1.25, .25, 0, out fell, out _), Is.True); Assert.That(fell, Is.True);
            var fallen = _forest.ReadAt(0); Assert.That(_forest.CachedUnits, Is.Zero);
            Assert.That(fallen.UpperLength, Is.EqualTo(2.25));
            Assert.That(_forest.TrySettle(tree.Id, fallen.FellingSequence, 0, out var reason), Is.True, reason);
            Assert.That(_forest.TrySettle(tree.Id, fallen.FellingSequence, 0, out reason), Is.True, reason);
            Assert.That(_forest.CachedUnits, Is.EqualTo(2)); Assert.That(_forest.WoodRemainder, Is.EqualTo(.25).Within(1e-9));
        }
        [Test] public void StumpRecovery_RetainsIdAndPositionAndRestoresFullSapling()
        {
            var tree = _forest.ReadAt(0);
            _forest.TryCut(tree.Id, 3, 0, 0, out _, out _); _forest.TrySettle(tree.Id, 1, 0, out _);
            _forest.Advance(119999); Assert.That(_forest.ReadAt(0).Stage, Is.EqualTo(TreeStage.Stump));
            _forest.Advance(120000); var sapling = _forest.ReadAt(0);
            Assert.That(sapling.Id, Is.EqualTo(tree.Id)); Assert.That(sapling.Position, Is.EqualTo(tree.Position));
            Assert.That(sapling.Height, Is.EqualTo(.5)); Assert.That(sapling.HP, Is.EqualTo(.5));
        }
        [Test] public void MissingTree_IsPermanentInvalidationAndNeverSubstitutesNeighbour()
        {
            var first = _forest.ReadAt(0); var other = _forest.ReadAt(1);
            Assert.That(_forest.RemoveTree(first.Id), Is.True);
            Assert.That(_forest.TryCut(first.Id, 5, 0, 0, out _, out var reason), Is.False);
            Assert.That(reason, Is.EqualTo("TreeMissing")); Assert.That(_forest.ReadAt(1).HP, Is.EqualTo(other.HP));
        }
        [Test] public void FallingTree_HasDeterministicDirectionAndTechnicalTimeout()
        {
            var tree = _forest.ReadAt(0); _forest.TryCut(tree.Id, 2.25, .25, 0, out _, out _);
            var falling = _forest.ReadAt(0); Assert.That(falling.FallDirection.magnitude, Is.EqualTo(1).Within(1e-6));
            _forest.Advance(9999); Assert.That(_forest.CachedUnits, Is.Zero);
            _forest.Advance(10000); Assert.That(_forest.CachedUnits, Is.EqualTo(2));
            _forest.Advance(10001); Assert.That(_forest.CachedUnits, Is.EqualTo(2));
        }
        [Test] public void FractionalWood_CarriesBetweenTreesAndMergesOneGroundBatch()
        {
            for (int i = 0; i < 2; i++)
            { var tree = _forest.ReadAt(i); _forest.TryCut(tree.Id, 2.4, .2, 0, out _, out _); _forest.TrySettle(tree.Id, 1, 0, out _); }
            Assert.That(_forest.CachedUnits, Is.EqualTo(4)); Assert.That(_forest.WoodRemainder, Is.EqualTo(.8).Within(1e-8));
            Assert.That(_cargo.TryCapture(out var snapshot), Is.True); Assert.That(snapshot.Lots.Count, Is.EqualTo(1));
        }
        [TestCase(0, 100, 6)] [TestCase(1, 250, 8)] [TestCase(2, 500, 10)]
        public void MineralTier_StockAndDisplayAreIndependent(int size, int stock, int rocks)
        { var deposit = Deposit(size); Assert.That(deposit.TotalUnits, Is.EqualTo(stock)); Assert.That(deposit.VisibleRockCount, Is.EqualTo(rocks)); }
        [Test] public void MineralVisibility_UsesCeilingAndStableSeedRanking()
        {
            var first = Deposit(); var second = Deposit();
            first.TryDrill(Next(), 1, 80, 1, 80000, out _, out _); second.TryDrill(Next(), 1, 80, 1, 80000, out _, out _);
            Assert.That(first.RemainingUnits, Is.EqualTo(80)); Assert.That(first.VisibleRockCount, Is.EqualTo(5));
            for (int i = 0; i < 6; i++) Assert.That(first.IsRockVisible(i), Is.EqualTo(second.IsRockVisible(i)));
            first.TryDrill(Next(), 1, 316, 1, 396000, out _, out _); Assert.That(first.RemainingUnits, Is.EqualTo(1)); Assert.That(first.VisibleRockCount, Is.EqualTo(1));
            first.TryDrill(Next(), 1, 4, 1, 400000, out _, out _); Assert.That(first.VisibleRockCount, Is.Zero);
        }
        [Test] public void MidUnitPowerChange_ConservesFractionAndLinearDamage()
        {
            var deposit = Deposit(); var behavior = Next();
            deposit.TryDrill(behavior, 1, 2, 0, 2000, out _, out _); Assert.That(deposit.FractionalContribution, Is.EqualTo(.1).Within(1e-9));
            deposit.TryDrill(behavior, 2, 2, 1, 4000, out _, out _); Assert.That(deposit.FractionalContribution, Is.EqualTo(.6).Within(1e-9));
            deposit.TryDrill(behavior, 3, 2, 1, 6000, out var receipt, out _); Assert.That(receipt.Units, Is.EqualTo(1));
            Assert.That(deposit.ProducedUnits + deposit.FractionalContribution + deposit.RemainingExact, Is.EqualTo(100).Within(1e-9));
            Assert.That(deposit.CachedUnits, Is.EqualTo(1));
        }
        [Test] public void RepeatedDrillCompletion_DoesNotRepeatFractionOrStockDebit()
        {
            var deposit = Deposit(); var behavior = Next();
            deposit.TryDrill(behavior, 1, 5, 1, 5000, out var first, out _);
            deposit.TryDrill(behavior, 1, 5, 1, 5000, out var second, out _);
            Assert.That(second.LotId, Is.EqualTo(first.LotId)); Assert.That(deposit.CachedUnits, Is.EqualTo(1));
            Assert.That(deposit.FractionalContribution, Is.EqualTo(.25).Within(1e-9));
        }
        [Test] public void OutputObserverFailure_CommitsResourceAndReceiptBeforeThrowAndCanRetry()
        {
            var deposit = Deposit(); var behavior = Next();
            _cargo.Produced += _ => { throw new InvalidOperationException("test observer"); };
            Assert.Throws<AggregateException>(() => deposit.TryDrill(behavior, 1, 4, 1, 4000, out _, out _));
            Assert.That(deposit.ProducedUnits, Is.EqualTo(1)); Assert.That(deposit.CachedUnits, Is.EqualTo(1));
            Assert.That(deposit.TryDrill(behavior, 1, 4, 1, 4000, out _, out _), Is.True);
            Assert.That(deposit.CachedUnits, Is.EqualTo(1));
        }
        [Test] public void DepletionCleanup_IsNextWorldDayAndRetainsGroundCargo()
        {
            var deposit = Deposit(); Assert.That(deposit.TryDrill(Next(), 1, 10000, 1, 5000, out _, out _), Is.True);
            Assert.That(deposit.CachedUnits, Is.EqualTo(100)); Assert.That(deposit.RemainingExact, Is.Zero);
            Assert.That(deposit.TryCleanup(deposit.CleanupAt - 1), Is.False); Assert.That(deposit.TryCleanup(deposit.CleanupAt), Is.True);
            Assert.That(deposit.CachedUnits, Is.EqualTo(100)); Assert.That(deposit.TryCleanup(deposit.CleanupAt), Is.False);
        }
        [Test] public void GroundDisplayCap_DoesNotStopLogicalOutputAndReservationSurvivesMerge()
        {
            var deposit = Deposit(); var behavior = Next(); deposit.TryDrill(behavior, 1, 80, 1, 80000, out _, out _);
            Assert.That(_cargo.TryFindAvailableLot(deposit.GroundOwner, null, out var lot), Is.True);
            var target = _cargo.RegisterContainer(new CargoOwner(CargoOwnerKind.Receiver, Next()), CargoContainerKind.MachineCargo, 30);
            Assert.That(_cargo.TryReserve(Next(), Next(), lot.Id, lot.Version, target.Owner, target.Generation, 10, out var reservation, out _), Is.True);
            deposit.TryDrill(behavior, 2, 80, 1, 160000, out _, out _);
            Assert.That(deposit.CachedUnits, Is.EqualTo(40)); Assert.That(_cargo.Commit(reservation, 1, 10).ActualUnits, Is.EqualTo(10));
            Assert.That(deposit.CachedUnits + target.Used, Is.EqualTo(deposit.ProducedUnits));
        }
        [Test] public void Snapshot_DetachesReceiptsAndCannotCaptureInsideOutputCallback()
        {
            var deposit = Deposit(); bool callbackCaptured = true;
            _cargo.Produced += receipt => callbackCaptured = _cargo.TryCapture(out _);
            deposit.TryDrill(Next(), 1, 4, 1, 4000, out _, out _);
            Assert.That(callbackCaptured, Is.False); Assert.That(_cargo.TryCapture(out var before), Is.True);
            deposit.TryDrill(Next(), 1, 4, 1, 8000, out _, out _);
            Assert.That(before.ProductionReceipts.Count, Is.EqualTo(1)); Assert.That(before.Lots[0].Lot.Units, Is.EqualTo(1));
        }
        [Test] public void InvalidConfigurationAndContributions_RejectWithoutInventoryMutation()
        {
            var deposit = Deposit(); Assert.That(deposit.TryDrill(Next(), 1, double.NaN, 1, 0, out _, out _), Is.False);
            Assert.That(deposit.TryDrill(Next(), 1, 4, 2, 0, out _, out _), Is.False); Assert.That(deposit.ProducedUnits, Is.Zero);
            Assert.Throws<ArgumentException>(() => new ProductionRules(new[] { 8, 12, 20 }, 2, .5, 3, 4,
                new[] { new Vector2(.5f, 1), new Vector2(4, 0) }, 120, 1, .25, 0, 1, 5));
        }
        [Test] public void TreeGrid_FilterSelectAndReadUseTypedPermanentReferences()
        {
            var grid = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.TreeGrid), Trees = new AlgorithmTreeGrid(_forest.CaptureTrees()) };
            var selected = AlgorithmGridOperations.Select(AlgorithmGridOperations.Filter(grid, "mature"));
            Assert.That(selected.Type.ObjectCategory, Is.EqualTo("Tree")); Assert.That(selected.ObjectId, Is.EqualTo(_forest.ReadAt(0).Id.Value));
            Assert.That(AlgorithmGridOperations.Read(grid, selected, "height").Number, Is.EqualTo(3));
            Assert.That(AlgorithmGridOperations.Read(grid, selected, "position").Type.Kind, Is.EqualTo(AlgorithmValueKind.Position));
            Assert.That(grid.Copy().Trees, Is.SameAs(grid.Trees), "The payload is immutable and can be shared by copies.");
        }
        [Test] public void EmptyFilteredGrid_HasInvalidSelectionAndCannotInventATree()
        {
            var grid = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.TreeGrid), Trees = new AlgorithmTreeGrid(_forest.CaptureTrees()) };
            var selected = AlgorithmGridOperations.Select(AlgorithmGridOperations.Filter(grid, "damaged"));
            Assert.That(selected.IsValid, Is.False); Assert.That(selected.ObjectId, Is.Zero);
            Assert.That(AlgorithmGridOperations.Filter(grid, "user-script").IsValid, Is.False);
        }
        [Test] public void SubUnitContributions_DoNotCreateCargoReceiptsEveryFrame()
        {
            var deposit = Deposit(); var behavior = Next();
            for (ulong i = 1; i <= 100; i++) Assert.That(deposit.TryDrill(behavior, i, .001, 1, (long)i, out _, out _), Is.True);
            Assert.That(deposit.FractionalContribution, Is.EqualTo(.025).Within(1e-10));
            Assert.That(_cargo.TryCapture(out var snapshot), Is.True); Assert.That(snapshot.ProductionReceipts.Count, Is.Zero); Assert.That(snapshot.Lots.Count, Is.Zero);
            Assert.That(deposit.TryDrill(behavior, 100, .001, 1, 100, out _, out _), Is.True);
            Assert.That(deposit.FractionalContribution, Is.EqualTo(.025).Within(1e-10));
            Assert.That(deposit.TryDrill(behavior, 99, .001, 1, 100, out _, out _), Is.False);
        }
        [Test] public void OrdinaryDamage_RetryIsIdempotentAndConflictingSequenceIsRejected()
        {
            var tree = _forest.ReadAt(0);
            Assert.That(_forest.TryDamage(tree.Id, .2, 1, 0, out _, out _), Is.True);
            double hp = _forest.ReadAt(0).HP;
            Assert.That(_forest.TryDamage(tree.Id, .2, 1, 0, out _, out _), Is.True);
            Assert.That(_forest.ReadAt(0).HP, Is.EqualTo(hp));
            Assert.That(_forest.TryDamage(tree.Id, .3, 1, 0, out _, out var reason), Is.False);
            Assert.That(reason, Is.EqualTo("ConflictingDamageRetry"));
        }
        [Test] public void TreeGridRead_RejectsForgedOutputTypeAndUnrelatedObjectCategory()
        {
            var document = new AlgorithmDocument { DocumentId = Next().Value };
            document.Nodes.Add(new AlgorithmNode { Id = 1, Kind = AlgorithmNodeKind.GridRead, Field = "position", ValueType = AlgorithmType.Of(AlgorithmValueKind.Number) });
            Assert.That(AlgorithmValidator.TryCompile(document, 40, out _, out var issues), Is.False);
            Assert.That(issues.Exists(issue => issue.Code == "TypeUnitCapabilityMismatch"), Is.True);
            var grid = new AlgorithmValue { Type = AlgorithmType.Of(AlgorithmValueKind.TreeGrid), Trees = new AlgorithmTreeGrid(_forest.CaptureTrees()) };
            var unrelated = new AlgorithmValue { Type = new AlgorithmType { Kind = AlgorithmValueKind.Object, ObjectCategory = "Machine" }, ObjectId = _forest.ReadAt(0).Id.Value };
            Assert.That(AlgorithmGridOperations.Read(grid, unrelated, "height").IsValid, Is.False);
        }
        [Test] public void InvalidTreePositions_AreRejectedBeforeAllocatingTreesOrGroundCargo()
        {
            var point = Next(); var nextId = _ids.NextId;
            Assert.Throws<ArgumentException>(() => new ForestProduction(point, _ids, _registry, _rules, _cargo, new[] { Vector3.zero, Vector3.zero }, 0));
            Assert.That(_ids.NextId, Is.EqualTo(nextId));
            Assert.That(_cargo.TryReadContainer(new CargoOwner(CargoOwnerKind.WorldFree, point), out _), Is.False);
        }
        [Test] public void ApprovedMeshClip_SeparatesTrianglesAtActualCutAndRetainsMaterialChannels()
        {
            var mesh = new Mesh { vertices = new[] { new Vector3(0,-1,0), new Vector3(-1,1,0), new Vector3(1,1,0) },
                triangles = new[] { 0,1,2 }, uv = new[] { Vector2.zero, Vector2.up, Vector2.one } };
            mesh.RecalculateNormals(); Mesh lower = null, upper = null;
            try
            {
                lower = TreeMeshSlice.Clip(mesh, Matrix4x4.identity, 0, false); upper = TreeMeshSlice.Clip(mesh, Matrix4x4.identity, 0, true);
                foreach (var vertex in lower.vertices) Assert.That(vertex.y, Is.LessThanOrEqualTo(0));
                foreach (var vertex in upper.vertices) Assert.That(vertex.y, Is.GreaterThanOrEqualTo(0));
                Assert.That(lower.triangles.Length, Is.EqualTo(3)); Assert.That(upper.triangles.Length, Is.EqualTo(6));
                Assert.That(lower.uv.Length, Is.EqualTo(lower.vertexCount)); Assert.That(upper.subMeshCount, Is.EqualTo(mesh.subMeshCount));
            }
            finally { UnityEngine.Object.DestroyImmediate(mesh); if (lower != null) UnityEngine.Object.DestroyImmediate(lower); if (upper != null) UnityEngine.Object.DestroyImmediate(upper); }
        }
        [Test] public void ResourceCleanup_RemovesOnlyExplicitAttachmentsAndRetainsUnrelatedBuilding()
        {
            using (var context = new AutoEra.Application.AutoEraApplicationCompositionRoot().Create())
            {
                context.TryCreateWorldSession(0, out var world);
                using (var region = new AutoEra.World.Region.InitialRegion(world, new Rect(-40,-40,80,80)))
                {
                    var point = region.Register(PersistentObjectKind.ResourcePoint, "矿脉", Vector2.zero, Vector2.one, blocksNavigation: false);
                    var attachment = region.Register(PersistentObjectKind.Building, "附属设施", new Vector2(3,0), Vector2.one);
                    var independent = region.Register(PersistentObjectKind.Building, "玩家设施", new Vector2(6,0), Vector2.one);
                    Assert.That(region.AttachResourceFacility(attachment.Id, point.Id), Is.True);
                    Assert.That(region.RemoveResourcePoint(point.Id), Is.True);
                    Assert.That(region.TryGet(attachment.Id, out _), Is.False); Assert.That(region.TryGet(independent.Id, out _), Is.True);
                }
            }
        }
    }
}
