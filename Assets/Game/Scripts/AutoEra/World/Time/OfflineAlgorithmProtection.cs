using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.World.Time
{
    public sealed class OfflineAlgorithmObservation
    {
        public ulong Algorithm,Generation,Cause,ProgressRevision;
        public long WorldMilliseconds;
        public bool Protected;
        internal OfflineAlgorithmObservation Copy()=>new OfflineAlgorithmObservation {Algorithm=Algorithm,Generation=Generation,Cause=Cause,ProgressRevision=ProgressRevision,WorldMilliseconds=WorldMilliseconds,Protected=Protected};
    }
    public interface IOfflineAlgorithmProtection : IOfflineEventExecutor
    {
        // Return a complete semantic revision only when this event is a deterministic algorithm transition.
        // Include dependent resource fractions, task/compute queues and relevant input changes; diagnostic run counters are not progress.
        bool TryObserveAlgorithm(OfflineScheduledEvent item,out OfflineAlgorithmObservation observation);
        bool TryStopNoProgressAlgorithm(PersistentId algorithm,out string reason);
    }
    internal sealed class OfflineAlgorithmCycleGuard
    {
        private readonly Dictionary<ulong,OfflineAlgorithmObservation> _observations=new Dictionary<ulong,OfflineAlgorithmObservation>();
        internal bool Observe(OfflineAlgorithmObservation value)
        {
            if(value==null || value.Algorithm==0 || value.Generation==0 || value.Cause==0 || value.WorldMilliseconds<0 || value.Protected)throw new ArgumentException("Incomplete algorithm progress observation.");
            bool cycle=_observations.TryGetValue(value.Algorithm,out var previous) && previous.WorldMilliseconds==value.WorldMilliseconds &&
                previous.Generation==value.Generation && previous.Cause==value.Cause && previous.ProgressRevision==value.ProgressRevision;
            if(previous==null) {_observations.Add(value.Algorithm,value.Copy());previous=_observations[value.Algorithm];}
            else {previous.Generation=value.Generation;previous.Cause=value.Cause;previous.ProgressRevision=value.ProgressRevision;previous.WorldMilliseconds=value.WorldMilliseconds;}
            previous.Protected=cycle;return cycle;
        }
        internal OfflineAlgorithmObservation[] Capture()
        {
            var rows=new List<OfflineAlgorithmObservation>();foreach(var value in _observations.Values)rows.Add(value.Copy());rows.Sort((a,b)=>a.Algorithm.CompareTo(b.Algorithm));return rows.ToArray();
        }
        internal bool Restore(OfflineAlgorithmObservation[] rows,long worldMilliseconds)
        {
            if(rows==null || _observations.Count!=0)return false;
            var ids=new HashSet<ulong>();foreach(var row in rows)
                if(row==null || row.Algorithm==0 || row.Generation==0 || row.Cause==0 || row.WorldMilliseconds<0 || row.WorldMilliseconds>worldMilliseconds || !ids.Add(row.Algorithm))return false;
            foreach(var row in rows)_observations.Add(row.Algorithm,row.Copy());return true;
        }
        internal void Rebind(PersistentId id,ulong original,ulong restored)
        {
            if(!id.IsValid || original==0 || restored==0)throw new ArgumentException("Invalid original algorithm generation mapping.");
            if(_observations.TryGetValue(id.Value,out var value))
            {if(value.Generation!=original)throw new ArgumentException("Algorithm observation belongs to a different generation.");value.Generation=restored;}
        }
    }
}
