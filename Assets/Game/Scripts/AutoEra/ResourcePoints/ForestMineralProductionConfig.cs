using System;
using UnityEngine;

namespace AutoEra.ResourcePoints
{
    [CreateAssetMenu(menuName = "AutoEra/林木与矿脉生产配置")]
    public sealed class ForestMineralProductionConfig : ScriptableObject
    {
        [SerializeField] private int[] _treeCounts = { 8, 12, 20 };
        [SerializeField] private float _treeSpacing = 2;
        [SerializeField] private float _saplingHeight = .5f, _matureHeight = 3, _maximumHeight = 4;
        [SerializeField] private Vector2[] _growth = { new Vector2(.5f, .002f), new Vector2(1, .008f), new Vector2(3, .002f), new Vector2(4, 0) };
        [SerializeField] private float _stumpRecoverySeconds = 120, _woodPerMeter = 1, _sawDamage = .25f;
        [SerializeField] private float _miningResistance = 20, _drillMinimumDamage = 1, _drillMaximumDamage = 5;
        public ProductionRules Read() => new ProductionRules(_treeCounts, _treeSpacing, _saplingHeight, _matureHeight, _maximumHeight,
            _growth, _stumpRecoverySeconds, _woodPerMeter, _sawDamage, _miningResistance, _drillMinimumDamage, _drillMaximumDamage);
    }

    /// <summary>Immutable, validated authoring values. No live tree/deposit state is kept in the asset.</summary>
    public sealed partial class ProductionRules
    {
        private readonly int[] _counts;
        private readonly Vector2[] _curve;
        public double SaplingHeight { get; }
        public double MatureHeight { get; }
        public double MaximumHeight { get; }
        public double Spacing { get; }
        public double StumpRecoverySeconds { get; }
        public double WoodPerMeter { get; }
        public double SawDamage { get; }
        public double MiningResistance { get; }
        public double DrillMinimumDamage { get; }
        public double DrillMaximumDamage { get; }
        public int TreeCount(int size) => _counts[size];
        public ProductionRules(int[] counts, double spacing, double sapling, double mature, double maximum, Vector2[] curve,
            double recovery, double wood, double saw, double resistance, double drillMin, double drillMax)
        {
            if (counts == null || counts.Length != 3 || curve == null || curve.Length < 2) throw new ArgumentException("Production configuration missing.");
            foreach (int count in counts) if (count < 1) throw new ArgumentException("Tree counts must be positive.");
            if (!Positive(spacing) || !Positive(sapling) || !Positive(mature) || !Positive(maximum) || !(sapling < mature && mature < maximum) ||
                !Positive(recovery) || !Positive(wood) || !Positive(saw) || !Positive(resistance) || !Positive(drillMin) || !Positive(drillMax) || drillMin > drillMax)
                throw new ArgumentException("Invalid production units or range.");
            for (int i = 0; i < curve.Length; i++)
                if (!Finite(curve[i].x) || !Finite(curve[i].y) || curve[i].y < 0 || (i > 0 && curve[i].x <= curve[i - 1].x))
                    throw new ArgumentException("Growth curve must have increasing heights and nonnegative speed.");
            if (Math.Abs(curve[0].x - sapling) > 1e-6 || Math.Abs(curve[curve.Length - 1].x - maximum) > 1e-6 || curve[curve.Length - 1].y != 0)
                throw new ArgumentException("Growth curve endpoints must match tree heights.");
            _counts = (int[])counts.Clone(); _curve = (Vector2[])curve.Clone(); Spacing = spacing;
            SaplingHeight = sapling; MatureHeight = mature; MaximumHeight = maximum; StumpRecoverySeconds = recovery;
            WoodPerMeter = wood; SawDamage = saw; MiningResistance = resistance; DrillMinimumDamage = drillMin; DrillMaximumDamage = drillMax;
        }
        public static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        private static bool Positive(double value) => Finite(value) && value > 0;
        public double DrillDamage(double power)
        { if (!Finite(power) || power < 0 || power > 1) throw new ArgumentOutOfRangeException(nameof(power)); return DrillMinimumDamage + (DrillMaximumDamage - DrillMinimumDamage) * power; }
        /// <summary>Analytic dh/dt integration gives the same height under different tick partitions and offline jumps.</summary>
        public double Grow(double height, double seconds)
        {
            if (!Finite(height) || !Finite(seconds) || height < SaplingHeight || height > MaximumHeight || seconds < 0) throw new ArgumentOutOfRangeException(nameof(height));
            for (int i = 0; i < _curve.Length - 1 && seconds > 0; i++)
            {
                if (height >= _curve[i + 1].x) continue;
                double lower = _curve[i].x, upper = _curve[i + 1].x;
                double slope = (_curve[i + 1].y - _curve[i].y) / (upper - lower);
                double speed = _curve[i].y + slope * (height - lower);
                if (speed <= 0) return height;
                double boundarySpeed = _curve[i + 1].y;
                double toBoundary = Math.Abs(slope) < 1e-12 ? (upper - height) / speed :
                    boundarySpeed <= 0 ? double.PositiveInfinity : Math.Log(boundarySpeed / speed) / slope;
                double elapsed = Math.Min(seconds, toBoundary);
                height = Math.Abs(slope) < 1e-12 ? height + speed * elapsed : height + speed * (Math.Exp(slope * elapsed) - 1) / slope;
                height = Math.Min(upper, height); seconds -= elapsed;
                if (elapsed == toBoundary) height = upper;
            }
            return Math.Min(MaximumHeight, height);
        }
    }
}
