using System;
using AutoEra.World.Identity;
using AutoEra.World.Region;

namespace AutoEra.Machines.Sensors
{
    public enum SensorReadReason
    { None, NotInstalled, NotRunning, Sleeping, PowerOff, NoTarget, TargetMissing, RegionUnavailable, ProviderUnavailable, UnsupportedTarget, AnchorUnavailable, OutOfRange, WaitingCompute, InvalidData, Disposed }
    public sealed class SensorReadEvent : EventArgs
    {
        public PersistentId SensorId { get; }
        public PersistentObjectReference Target { get; }
        public ulong Generation { get; }
        public ulong Sequence { get; }
        public long WorldMilliseconds { get; }
        public SensorReadReason Reason { get; }
        public SensorSnapshot Snapshot { get; }
        internal SensorReadEvent(PersistentId sensorId, PersistentObjectReference target, ulong generation, ulong sequence,
            long time, SensorReadReason reason, SensorSnapshot snapshot)
        { SensorId = sensorId; Target = target; Generation = generation; Sequence = sequence; WorldMilliseconds = time; Reason = reason; Snapshot = snapshot; }
    }
}
