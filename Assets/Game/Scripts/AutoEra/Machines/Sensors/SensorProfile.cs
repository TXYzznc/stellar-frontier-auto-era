using System;

namespace AutoEra.Machines.Sensors
{
    public enum SensorKind { ObjectState, Soil }
    public sealed class SensorProfile
    {
        public int ComponentDefinitionId { get; }
        public int Level { get; }
        public SensorKind Kind { get; }
        public long IntervalMilliseconds => 1000;
        public int ComputeCost => 10;
        public float Range => Kind == SensorKind.ObjectState ? 6f : 4f;
        public SensorProfile(int componentDefinitionId, int level, SensorKind kind)
        {
            if (componentDefinitionId <= 0 || level < 1 || level > 2 || !Enum.IsDefined(typeof(SensorKind), kind))
                throw new ArgumentException("Invalid sensor profile.");
            ComponentDefinitionId = componentDefinitionId; Level = level; Kind = kind;
        }
    }
}
