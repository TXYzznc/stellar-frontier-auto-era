using System;
using System.Collections.Generic;
using AutoEra.World.Identity;
using UnityEngine;

namespace AutoEra.World.Region
{
    public enum WorkRequestResult { Granted, Waiting, InvalidRequester, InvalidTarget, OutsideWorkArea }
    public enum WorkRequestState { None, Granted, Waiting, InvalidRequester, InvalidTarget }

    /// <summary>One exclusive work channel. Separate channels permit farm care and general work in parallel.</summary>
    public sealed class RegionWorkQueue : IDisposable
    {
        private readonly InitialRegion _region;
        private readonly PersistentId _target;
        private readonly Rect _workArea;
        private readonly List<PersistentId> _waiting = new List<PersistentId>();
        private readonly Dictionary<PersistentId, int> _priorities = new Dictionary<PersistentId, int>();
        private readonly RegionObject _targetObject;
        private readonly string _channel;
        private bool _disposed;
        public RegionWorkQueue(InitialRegion region, PersistentId target, Rect workArea, string channel = "作业")
        {
            _region = region ?? throw new ArgumentNullException(nameof(region));
            if (string.IsNullOrWhiteSpace(channel) || !region.TryGet(target, out RegionObject targetObject) || !RegionPlacement.IsFinite(workArea.position) ||
                !RegionPlacement.IsFinite(workArea.size) || workArea.width <= 0 || workArea.height <= 0)
                throw new ArgumentException("Invalid work target or area.");
            _target = target; _workArea = workArea;
            _targetObject = targetObject; _channel = channel;
            _region.ObjectRemoved += OnRemoved;
            Publish();
        }
        public PersistentId Owner { get; private set; }
        public int WaitingCount => _waiting.Count;
        public Rect WorkArea => _workArea;
        public bool BelongsTo(InitialRegion region, PersistentId target) => !_disposed && ReferenceEquals(region, _region) && target == _target;
        public event Action Changed;
        /// <summary>Reservation only; querying never enqueues or claims that the machine has arrived.</summary>
        public WorkRequestState GetRequestState(PersistentId machine)
        {
            if (_disposed || !_region.TryGet(_target, out _)) return WorkRequestState.InvalidTarget;
            if (!_region.TryGet(machine, out RegionObject obj) || obj.Kind != PersistentObjectKind.Machine)
                return WorkRequestState.InvalidRequester;
            if (Owner == machine) return WorkRequestState.Granted;
            return _waiting.Contains(machine) ? WorkRequestState.Waiting : WorkRequestState.None;
        }
        public WorkRequestResult Request(PersistentId machine, Vector2 workPosition, int priority = 0)
        {
            if (_disposed || !_region.TryGet(_target, out _)) return WorkRequestResult.InvalidTarget;
            if (!_region.TryGet(machine, out RegionObject obj) || obj.Kind != PersistentObjectKind.Machine)
                return WorkRequestResult.InvalidRequester;
            if (!RegionPlacement.IsFinite(workPosition) || !_workArea.Contains(workPosition)) return WorkRequestResult.OutsideWorkArea;
            if (Owner == machine) return WorkRequestResult.Granted;
            if (_waiting.Contains(machine)) return WorkRequestResult.Waiting;
            if (!Owner.IsValid) { Owner = machine; Publish(); return WorkRequestResult.Granted; }
            int index = 0;
            while (index < _waiting.Count && _priorities[_waiting[index]] >= priority) index++;
            _waiting.Insert(index, machine); _priorities.Add(machine, priority);
            Publish(); return WorkRequestResult.Waiting;
        }
        public bool Release(PersistentId machine)
        {
            if (_disposed) return false;
            bool changed = _waiting.Remove(machine);
            _priorities.Remove(machine);
            if (Owner == machine)
            {
                Owner = PersistentId.Invalid; changed = true;
                while (_waiting.Count > 0)
                {
                    PersistentId next = _waiting[0]; _waiting.RemoveAt(0);
                    _priorities.Remove(next);
                    if (_region.TryGet(next, out _)) { Owner = next; break; }
                }
            }
            if (changed) Publish();
            return changed;
        }
        private void OnRemoved(PersistentId id)
        {
            if (id == _target) { Owner = PersistentId.Invalid; _waiting.Clear(); _priorities.Clear(); Publish(); Dispose(); }
            else Release(id);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _region.ObjectRemoved -= OnRemoved;
            Owner = PersistentId.Invalid; _waiting.Clear(); _priorities.Clear(); _disposed = true;
            _targetObject.SetWorkChannel(_channel, PersistentId.Invalid, 0, true);
            Changed = null;
        }

        private void Publish()
        {
            _targetObject.SetWorkChannel(_channel, Owner, _waiting.Count);
            Changed?.Invoke();
        }
    }
}
