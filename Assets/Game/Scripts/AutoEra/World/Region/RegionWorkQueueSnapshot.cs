using System;
using System.Collections.Generic;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public sealed class RegionWorkQueueSnapshot
    {
        public ulong Target, Owner;
        public string Channel;
        public Vector2 AreaPosition, AreaSize;
        public RegionWorkWaiterSnapshot[] Waiting;
    }
    public sealed class RegionWorkWaiterSnapshot
    {
        public ulong Machine;
        public int Priority;
    }
    public sealed partial class RegionWorkQueue
    {
        public RegionWorkQueueSnapshot CapturePersistentState()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(RegionWorkQueue));
            var waiting = new RegionWorkWaiterSnapshot[_waiting.Count];
            for (int i=0;i<waiting.Length;i++) waiting[i] = new RegionWorkWaiterSnapshot { Machine=_waiting[i].Value, Priority=_priorities[_waiting[i]] };
            return new RegionWorkQueueSnapshot { Target=_target.Value, Owner=Owner.Value, Channel=_channel,
                AreaPosition=_workArea.position, AreaSize=_workArea.size, Waiting=waiting };
        }

        /// <summary>Bind the original channel geometry first, then quietly restore its owner and FIFO.</summary>
        public void RestorePersistentState(RegionWorkQueueSnapshot snapshot)
        {
            if (_disposed || Owner.IsValid || _waiting.Count != 0) throw new InvalidOperationException("Work queue restoration requires an empty channel.");
            if (snapshot?.Waiting == null || snapshot.Target != _target.Value || snapshot.Channel != _channel ||
                snapshot.AreaPosition != _workArea.position || snapshot.AreaSize != _workArea.size ||
                !RegionPlacement.IsFinite(snapshot.AreaPosition) || !RegionPlacement.IsFinite(snapshot.AreaSize) ||
                !_region.TryGet(_target,out _)) throw new ArgumentException("Work queue identity or geometry changed.");
            var ids = new HashSet<PersistentId>(); var waiting = new List<PersistentId>(); var priorities = new Dictionary<PersistentId,int>();
            void ValidateMachine(PersistentId id)
            {
                if (!id.IsValid || !ids.Add(id) || !_region.TryGetMachine(id,out _)) throw new ArgumentException("Invalid work queue machine.");
            }
            var owner = new PersistentId(snapshot.Owner);
            if (owner.IsValid) ValidateMachine(owner);
            else if (snapshot.Waiting.Length != 0) throw new ArgumentException("An unowned work queue cannot retain waiting machines.");
            int previous = int.MaxValue;
            foreach (var item in snapshot.Waiting)
            {
                if (item == null || item.Priority > previous) throw new ArgumentException("Invalid work queue priority order.");
                var id = new PersistentId(item.Machine); ValidateMachine(id);
                previous = item.Priority; waiting.Add(id); priorities.Add(id,item.Priority);
            }
            Owner = owner;
            _waiting.AddRange(waiting); foreach (var pair in priorities) _priorities.Add(pair.Key,pair.Value);
            // This updates only the candidate's derived public readout; no channel Changed event is replayed.
            _targetObject.SetWorkChannel(_channel,Owner,_waiting.Count);
        }
    }
}
