using System;
using UnityEngine;

namespace AutoEra.Logistics
{
    [CreateAssetMenu(menuName = "AutoEra/Resources/Transfer Rules")]
    public sealed class ResourceTransferConfig : ScriptableObject
    {
        [SerializeField] private float _preparationSeconds = .5f;
        [SerializeField] private float _unitsPerSecond = 5;
        [SerializeField] private float _levelOneRange = 2;
        [SerializeField] private float _levelTwoRange = 2.2f;
        [SerializeField] private float _levelTwoSpeed = 1.2f;
        public ResourceTransferRules Read() => new ResourceTransferRules(_preparationSeconds, _unitsPerSecond, _levelOneRange, _levelTwoRange, _levelTwoSpeed);
    }
    public sealed class ResourceTransferRules
    {
        public double PreparationSeconds { get; }
        public double UnitsPerSecond { get; }
        public double LevelOneRange { get; }
        public double LevelTwoRange { get; }
        public double LevelTwoSpeed { get; }
        public ResourceTransferRules(double preparation, double rate, double range, double upgradedRange, double upgradedSpeed)
        {
            if (!Valid(preparation) || preparation < 0 || !Valid(rate) || rate <= 0 || !Valid(range) || range <= 0 ||
                !Valid(upgradedRange) || upgradedRange < range || !Valid(upgradedSpeed) || upgradedSpeed <= 0) throw new ArgumentException("Invalid transfer rules.");
            PreparationSeconds = preparation; UnitsPerSecond = rate; LevelOneRange = range; LevelTwoRange = upgradedRange; LevelTwoSpeed = upgradedSpeed;
        }
        private static bool Valid(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        public double Speed(int level) => level == 1 ? 1 : level == 2 ? LevelTwoSpeed : throw new ArgumentOutOfRangeException(nameof(level));
        public double Range(int level) => level == 1 ? LevelOneRange : level == 2 ? LevelTwoRange : throw new ArgumentOutOfRangeException(nameof(level));
    }
}
