using System;
using System.Collections.Generic;
using AutoEra.Logistics;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.ResourcePoints
{
    public enum TreeStage { Growing, Mature, Falling, Stump, Removed }
    public readonly struct TreeReadout
    {
        public PersistentId Id { get; }
        [Newtonsoft.Json.JsonProperty(Required = Newtonsoft.Json.Required.Always)] public Vector3 Position { get; }
        public double Height { get; }
        public double HP { get; }
        public double HPRatio => Height > 0 ? HP / Height : 0;
        public TreeStage Stage { get; }
        public ulong FellingSequence { get; }
        public double UpperLength { get; }
        public Vector3 FallDirection { get; }
        public double FractureHeight { get; }
        public long FallingStartedAt { get; }
        internal TreeReadout(ForestTree tree)
        { Id = tree.Id; Position = tree.Position; Height = tree.Height; HP = tree.HP; Stage = tree.Stage;
            FellingSequence = tree.FellingSequence; UpperLength = tree.UpperLength; FallDirection = tree.FallDirection;
            FractureHeight = tree.FractureHeight; FallingStartedAt = tree.FallingDeadline - 10000; }
        [Newtonsoft.Json.JsonConstructor]
        internal TreeReadout(PersistentId id, Vector3 position, double height, double hp, TreeStage stage,
            ulong fellingSequence, double upperLength, Vector3 fallDirection, double fractureHeight, long fallingStartedAt)
        {
            if (!id.IsValid || !ProductionRules.Finite(height) || !ProductionRules.Finite(hp) || height < 0 || hp < 0 || hp > height ||
                !ProductionRules.Finite(upperLength) || upperLength < 0 || !ProductionRules.Finite(fractureHeight) || fractureHeight < 0 || !Enum.IsDefined(typeof(TreeStage),stage))
                throw new ArgumentException("Invalid tree readout snapshot.");
            Id=id; Position=position; Height=height; HP=hp; Stage=stage; FellingSequence=fellingSequence;
            UpperLength=upperLength; FallDirection=fallDirection; FractureHeight=fractureHeight; FallingStartedAt=fallingStartedAt;
        }
    }
    internal sealed class ForestTree
    {
        internal PersistentId Id;
        internal Vector3 Position, FallDirection;
        internal double Height, HP, UpperLength, FractureHeight;
        internal TreeStage Stage;
        internal ulong FellingSequence;
        internal long FallingDeadline, RecoverAt;
        internal ulong LastDamageSequence;
        internal double LastDamage;
        internal long LastDamageTime;
        internal bool LastDamageFelled;
    }

    /// <summary>Stable world-owned trees; detached readouts and rebuildable presentation never own growth or wood quantity.</summary>
    public sealed partial class ForestProduction : IDisposable
    {
        private readonly PersistentObjectRegistry _registry;
        private readonly ProductionRules _rules;
        private readonly CargoOwnershipAuthority _cargo;
        private readonly List<ForestTree> _trees = new List<ForestTree>();
        private readonly Dictionary<PersistentId, ForestTree> _byId = new Dictionary<PersistentId, ForestTree>();
        private long _now;
        private bool _disposed;
        private double _woodRemainder;
        public PersistentId Id { get; }
        public CargoOwner GroundOwner { get; }
        public long Revision { get; private set; }
        public bool IsActive => !_disposed;
        public int Count => _trees.Count;
        public double WoodRemainder => _woodRemainder;
        public double MaximumHeight => _rules.MaximumHeight;
        public int MatureCount { get { int count = 0; foreach (var tree in _trees) if (tree.Stage == TreeStage.Mature) count++; return count; } }
        public long CachedUnits => _cargo.TryReadContainer(GroundOwner, out var pile) ? pile.Used : 0;
        public ForestProduction(PersistentId point, PersistentIdAllocator ids, PersistentObjectRegistry registry, ProductionRules rules,
            CargoOwnershipAuthority cargo, Vector3[] positions, long now)
        {
            if (!point.IsValid || positions == null || positions.Length == 0 || now < 0 || ids == null) throw new ArgumentException("Invalid forest.");
            Id = point; _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules)); _cargo = cargo ?? throw new ArgumentNullException(nameof(cargo));
            GroundOwner = new CargoOwner(CargoOwnerKind.WorldFree, point);
            var unique = new HashSet<Vector3>();
            foreach (var position in positions)
                if (!Finite(position) || !unique.Add(position)) throw new ArgumentException("Unique finite tree positions required.");
            try
            {
                foreach (var position in positions)
                {
                    if (!ids.TryAllocate(out var id)) throw new InvalidOperationException("Tree identities exhausted.");
                    var tree = new ForestTree { Id = id, Position = position, Height = rules.MatureHeight, HP = rules.MatureHeight, Stage = TreeStage.Mature };
                    if (_registry.TryRegister(id, PersistentObjectKind.Tree, tree) != PersistentRegistryResult.Success) throw new InvalidOperationException("Duplicate tree identity.");
                    _trees.Add(tree); _byId.Add(id, tree);
                }
                if (!_cargo.TryReadContainer(GroundOwner, out _)) _cargo.RegisterContainer(GroundOwner, CargoContainerKind.WorldFree, long.MaxValue);
            }
            catch { foreach (var tree in _trees) _registry.TryUnregister(tree.Id, PersistentObjectKind.Tree, tree); throw; }
            _now = now;
        }
        private static bool Finite(Vector3 p) => ProductionRules.Finite(p.x) && ProductionRules.Finite(p.y) && ProductionRules.Finite(p.z);
        public TreeReadout ReadAt(int index) => new TreeReadout(_trees[index]);
        public bool TryRead(PersistentId id, out TreeReadout value)
        { value = default; if (_disposed || !_byId.TryGetValue(id, out var tree) || tree.Stage == TreeStage.Removed) return false; value = new TreeReadout(tree); return true; }
        public TreeReadout[] CaptureTrees()
        { var values = new TreeReadout[_trees.Count]; for (int i = 0; i < values.Length; i++) values[i] = new TreeReadout(_trees[i]); return values; }
        public void Advance(long now)
        {
            if (_disposed) return;
            if (now < _now) throw new ArgumentOutOfRangeException(nameof(now));
            foreach (var tree in _trees)
            {
                long from = _now;
                if (tree.Stage == TreeStage.Falling && now >= tree.FallingDeadline) TrySettle(tree.Id, tree.FellingSequence, tree.FallingDeadline, out _);
                if (tree.Stage == TreeStage.Stump && now >= tree.RecoverAt)
                { from = tree.RecoverAt; tree.Height = tree.HP = _rules.SaplingHeight; tree.Stage = TreeStage.Growing; Revision++; }
                if (tree.Stage != TreeStage.Growing && tree.Stage != TreeStage.Mature) continue;
                double grown = _rules.Grow(tree.Height, (now - from) / 1000d);
                if (grown == tree.Height) continue;
                tree.HP = Math.Min(grown, tree.HP + (grown - tree.Height) * 2);
                tree.Height = grown; tree.Stage = grown >= _rules.MatureHeight ? TreeStage.Mature : TreeStage.Growing; Revision++;
            }
            _now = now;
        }
        public bool TryCut(PersistentId id, double effectiveDamage, double fellingRatio, long now, out bool fell, out string reason)
            => ApplyDamage(id, effectiveDamage, fellingRatio, true, 0, now, out fell, out reason);
        /// <summary>All other effective damage uses the shared 10%-to-100% low-health rule, never the precise saw rule.</summary>
        public bool TryDamage(PersistentId id, double effectiveDamage, ulong damageSequence, long now, out bool fell, out string reason)
            => ApplyDamage(id, effectiveDamage, .25, false, damageSequence, now, out fell, out reason);
        private bool ApplyDamage(PersistentId id, double damage, double ratio, bool precise, ulong hit, long now, out bool fell, out string reason)
        {
            fell = false; reason = null;
            if (_disposed || !_byId.TryGetValue(id, out var tree) || tree.Stage == TreeStage.Removed) { reason = "TreeMissing"; return false; }
            if (!precise)
            {
                if (hit == 0 || hit < tree.LastDamageSequence) { reason = "StaleDamageSequence"; return false; }
                if (hit == tree.LastDamageSequence)
                {
                    if (damage != tree.LastDamage || now != tree.LastDamageTime) { reason = "ConflictingDamageRetry"; return false; }
                    fell = tree.LastDamageFelled; return true;
                }
            }
            if (tree.Stage != TreeStage.Growing && tree.Stage != TreeStage.Mature) { reason = "TreeNotStanding"; return false; }
            if (!ProductionRules.Finite(damage) || damage <= 0 || !ProductionRules.Finite(ratio) || ratio < 0 || ratio > .25 || now < _now)
            { reason = "InvalidDamage"; return false; }
            if (tree.FellingSequence == ulong.MaxValue || now > long.MaxValue - 10000) { reason = "FellingSequenceOrTimeExhausted"; return false; }
            double next = precise ? Math.Min(tree.HP, Math.Max(tree.Height * ratio, tree.HP - damage)) : Math.Max(0, tree.HP - damage);
            bool felling = precise ? next <= tree.Height * ratio : next == 0 ||
                next / tree.Height <= .25 && Random01(tree.Id.Value, hit) < .1 + .9 * (1 - next / tree.Height / .25);
            tree.HP = next; Revision++;
            if (!precise) { tree.LastDamageSequence = hit; tree.LastDamage = damage; tree.LastDamageTime = now; tree.LastDamageFelled = felling; }
            if (!felling) return true;
            tree.FractureHeight = next; tree.UpperLength = tree.Height - next; tree.FellingSequence++;
            double angle = Random01(tree.Id.Value, tree.FellingSequence) * Math.PI * 2;
            tree.FallDirection = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
            tree.Stage = TreeStage.Falling;
            // Technical recovery timeout, independent of gameplay yield or damage rate. Physics can settle sooner.
            tree.FallingDeadline = checked(now + 10000); fell = true; return true;
        }
        private static double Random01(ulong id, ulong sequence)
        { unchecked { ulong x = id ^ sequence * 0x9E3779B97F4A7C15UL; x ^= x >> 30; x *= 0xBF58476D1CE4E5B9UL; x ^= x >> 27; x *= 0x94D049BB133111EBUL; x ^= x >> 31; return (x >> 11) * (1d / 9007199254740992d); } }
        public bool TrySettle(PersistentId id, ulong sequence, long now, out string reason)
        {
            reason = null;
            if (_disposed || !_byId.TryGetValue(id, out var tree) || tree.FellingSequence != sequence) { reason = "StaleTreeSettlement"; return false; }
            if (_cargo.TryReadProduction(id, sequence, out _)) return true;
            if (tree.Stage != TreeStage.Falling || now < _now) { reason = "TreeNotFalling"; return false; }
            double output = _woodRemainder + tree.UpperLength * _rules.WoodPerMeter;
            if (!ProductionRules.Finite(output) || output > int.MaxValue) { reason = "WoodQuantityOverflow"; return false; }
            int whole = (int)Math.Floor(output + 1e-10);
            var contribution = new FellContribution(this, tree, sequence, output - whole, checked(now + (long)(_rules.StumpRecoverySeconds * 1000)));
            return _cargo.TryProduce(id, sequence, GroundOwner, ResourceItemCatalog.Wood, whole, contribution, out _, out reason);
        }
        private sealed class FellContribution : IResourceProductionContribution
        {
            private readonly ForestProduction _forest; private readonly ForestTree _tree; private readonly ulong _sequence;
            private readonly double _fraction; private readonly long _recovery;
            internal FellContribution(ForestProduction forest, ForestTree tree, ulong sequence, double fraction, long recovery)
            { _forest = forest; _tree = tree; _sequence = sequence; _fraction = fraction; _recovery = recovery; }
            public bool Validate(out string reason) { reason = _tree.Stage == TreeStage.Falling && _tree.FellingSequence == _sequence ? null : "TreeChanged"; return reason == null; }
            public void Commit()
            { _tree.Stage = TreeStage.Stump; _tree.Height = _tree.HP = _tree.FractureHeight; _tree.RecoverAt = _recovery; _forest._woodRemainder = Math.Max(0, _fraction); _forest.Revision++; }
        }
        public bool RemoveTree(PersistentId id)
        {
            if (!_byId.TryGetValue(id, out var tree) || tree.Stage == TreeStage.Removed) return false;
            if (tree.Stage == TreeStage.Falling) return false; // Unsettled wood must cross its safe output boundary first.
            tree.Stage = TreeStage.Removed; Revision++; _registry.TryUnregister(id, PersistentObjectKind.Tree, tree); return true;
        }
        public void Dispose()
        { if (_disposed) return; _disposed = true; foreach (var tree in _trees) _registry.TryUnregister(tree.Id, PersistentObjectKind.Tree, tree); }
    }
}
