using System;
using AutoEra.Logistics;
using AutoEra.World.Identity;

namespace AutoEra.ResourcePoints
{
    /// <summary>Independent deposit stock and fractional contribution; sparse rocks are a projection only.</summary>
    public sealed partial class MineralProduction
    {
        private readonly CargoOwnershipAuthority _cargo;
        private readonly ProductionRules _rules;
        private readonly int[] _rockOrder;
        private double _fraction;
        private readonly System.Collections.Generic.Dictionary<PersistentId, ResourceProductionReceipt> _lastSteps = new System.Collections.Generic.Dictionary<PersistentId, ResourceProductionReceipt>();
        private long _now;
        public PersistentId Id { get; }
        public CargoOwner GroundOwner { get; }
        public int TotalUnits { get; }
        public int ProducedUnits { get; private set; }
        public double RemainingExact => TotalUnits - ProducedUnits - _fraction;
        public int RemainingUnits => TotalUnits - ProducedUnits;
        public double FractionalContribution => _fraction;
        internal bool MatchesPersistentDrillSequence(PersistentId behavior,ulong sequence)
            => _lastSteps.TryGetValue(behavior,out var last) ? last.Sequence==sequence : sequence==0;
        public int FullRockCount { get; }
        public int VisibleRockCount => RemainingExact <= 0 ? 0 : Math.Max(1, (int)Math.Ceiling(FullRockCount * RemainingExact / TotalUnits));
        public long DepletedAt { get; private set; } = -1;
        public long CleanupAt { get; private set; } = -1;
        public bool IsRemoved { get; private set; }
        public long Revision { get; private set; }
        public long CachedUnits => _cargo.TryReadContainer(GroundOwner, out var pile) ? pile.Used : 0;
        public MineralProduction(PersistentId id, int size, ulong seed, ProductionRules rules, CargoOwnershipAuthority cargo, long now)
        {
            if (!id.IsValid || size < 0 || size > 2 || now < 0) throw new ArgumentException("Invalid mineral deposit.");
            Id = id; _rules = rules ?? throw new ArgumentNullException(nameof(rules)); _cargo = cargo ?? throw new ArgumentNullException(nameof(cargo));
            TotalUnits = size == 0 ? 100 : size == 1 ? 250 : 500; FullRockCount = size == 0 ? 6 : size == 1 ? 8 : 10;
            GroundOwner = new CargoOwner(CargoOwnerKind.WorldFree, id);
            if (!_cargo.TryReadContainer(GroundOwner, out _)) _cargo.RegisterContainer(GroundOwner, CargoContainerKind.WorldFree, long.MaxValue);
            _rockOrder = new int[FullRockCount]; for (int i = 0; i < FullRockCount; i++) _rockOrder[i] = i;
            for (int i = FullRockCount - 1; i > 0; i--)
            { unchecked { seed ^= seed << 13; seed ^= seed >> 7; seed ^= seed << 17; } int j = (int)(seed % (uint)(i + 1)); int swap = _rockOrder[i]; _rockOrder[i] = _rockOrder[j]; _rockOrder[j] = swap; }
            _now = now;
        }
        public bool IsRockVisible(int index)
        { if (index < 0 || index >= FullRockCount) return false; for (int i = 0; i < VisibleRockCount; i++) if (_rockOrder[i] == index) return true; return false; }
        public bool TryDrill(PersistentId behavior, ulong sequence, double seconds, double power, long now, out ResourceProductionReceipt receipt, out string reason)
        {
            receipt = default; reason = null;
            if (_cargo.TryReadProduction(behavior, sequence, out receipt))
            { if (!receipt.Owner.Equals(GroundOwner)) { reason = "ProductionIdentityConflict"; return false; } return true; }
            if (_lastSteps.TryGetValue(behavior, out var last) && sequence <= last.Sequence)
            { if (sequence == last.Sequence) { receipt = last; return true; } reason = "StaleProductionSequence"; return false; }
            if (IsRemoved || RemainingExact <= 0) { reason = "DepositDepleted"; return false; }
            if (!behavior.IsValid || sequence == 0 || !ProductionRules.Finite(seconds) || seconds <= 0 || now < _now || !ProductionRules.Finite(power) || power < 0 || power > 1)
            { reason = "InvalidDrillingContribution"; return false; }
            double output = Math.Min(TotalUnits - ProducedUnits, _fraction + _rules.DrillDamage(power) / _rules.MiningResistance * seconds);
            int whole = (int)Math.Floor(output + 1e-10); double fraction = Math.Max(0, output - whole);
            if (whole == 0)
            {
                _fraction = fraction; _now = now; Revision++;
                receipt = new ResourceProductionReceipt(behavior, sequence, GroundOwner, ResourceItemCatalog.Ore, 0, default);
                _lastSteps[behavior] = receipt; return true;
            }
            long day = AutoEra.Energy.DaylightCycle.DayMilliseconds;
            if (now / day >= long.MaxValue / day - 1) { reason = "WorldTimeExhausted"; return false; }
            var contribution = new DrillContribution(this, behavior, sequence, ProducedUnits, whole, fraction, now);
            return _cargo.TryProduce(behavior, sequence, GroundOwner, ResourceItemCatalog.Ore, whole, contribution, out receipt, out reason);
        }
        private sealed class DrillContribution : IResourceProductionContribution
        {
            private readonly MineralProduction _deposit; private readonly int _previous, _whole; private readonly double _fraction; private readonly long _now;
            private readonly PersistentId _behavior; private readonly ulong _sequence;
            internal DrillContribution(MineralProduction deposit, PersistentId behavior, ulong sequence, int previous, int whole, double fraction, long now)
            { _deposit = deposit; _behavior = behavior; _sequence = sequence; _previous = previous; _whole = whole; _fraction = fraction; _now = now; }
            public bool Validate(out string reason) { reason = !_deposit.IsRemoved && _deposit.ProducedUnits == _previous ? null : "DepositChanged"; return reason == null; }
            public void Commit()
            {
                _deposit.ProducedUnits += _whole; _deposit._fraction = _fraction; _deposit._now = _now; _deposit.Revision++;
                _deposit._lastSteps[_behavior] = new ResourceProductionReceipt(_behavior, _sequence, _deposit.GroundOwner, ResourceItemCatalog.Ore, _whole, default);
                if (_deposit.RemainingExact <= 0)
                { _deposit.DepletedAt = _now; long day = AutoEra.Energy.DaylightCycle.DayMilliseconds; _deposit.CleanupAt = checked((_now / day + 1) * day); }
            }
        }
        public bool TryCleanup(long now)
        { if (IsRemoved || CleanupAt < 0 || now < CleanupAt) return false; IsRemoved = true; Revision++; return true; }
    }
}
