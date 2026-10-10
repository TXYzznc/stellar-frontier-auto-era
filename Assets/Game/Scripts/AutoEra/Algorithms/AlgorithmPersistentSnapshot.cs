using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.Algorithms
{
    /// <summary>Detached facts. Physical task IDs are references to the other world sections, never reconstructed here.</summary>
    public sealed class AlgorithmPersistentSnapshot
    {
        public ulong InstanceId, Generation, RunSequence;
        public AlgorithmDocument Applied;
        public Dictionary<string, AlgorithmValue> State;
        public Dictionary<string, PersistentId> StateLeases;
        public AlgorithmPreparedSnapshot Prepared;
        public AlgorithmTrigger[] Events;
        public AlgorithmDelaySnapshot[] Delays;
        public AlgorithmHistorySnapshot[] History;
        public bool Invalid;
        public AlgorithmPauseReason PauseReasons;
        public string Reason;
    }

    public sealed class AlgorithmDelaySnapshot
    {
        public AlgorithmTrigger Trigger;
        public long RemainingMilliseconds;
        public PersistentId ComputeLeaseId;
    }

    public sealed class AlgorithmHistorySnapshot
    {
        public ulong RunId, FailedNode;
        public AlgorithmTrigger Trigger;
        public ulong[] Path;
        public Dictionary<ulong, AlgorithmValue> NodeValues;
        public AlgorithmDocument ExecutedDocument;
        public string Error;
        public int Cost;

        internal static AlgorithmHistorySnapshot Capture(AlgorithmRunRecord record)
        {
            var values = new Dictionary<ulong, AlgorithmValue>();
            foreach (var pair in record.CopyNodeValues()) values.Add(pair.Key, pair.Value);
            return new AlgorithmHistorySnapshot { RunId = record.RunId, FailedNode = record.FailedNode,
                Trigger = record.CopyTrigger(), Path = record.CopyPath(), NodeValues = values,
                ExecutedDocument = record.CopyExecutedDocument(), Error = record.Error, Cost = record.Cost };
        }

        internal bool TryBuild(ulong instance, long now, out AlgorithmRunRecord record)
        {
            record = null;
            if (RunId == 0 || Cost < 0 || Trigger == null || Trigger.InstanceId != instance || Trigger.Time < 0 || Trigger.Time > now ||
                ExecutedDocument == null || Trigger.Revision != ExecutedDocument.Revision || Path == null || NodeValues == null ||
                !AlgorithmValidator.TryCompile(ExecutedDocument, int.MaxValue, out var plan, out _)) return false;
            var nodes = new HashSet<ulong>();
            foreach (var node in ExecutedDocument.Nodes) nodes.Add(node.Id);
            if (FailedNode != 0 && !nodes.Contains(FailedNode)) return false;
            var batch = new AlgorithmBatch { Error = Error, FailedNode = FailedNode, Cost = Cost };
            foreach (ulong id in Path) { if (!nodes.Contains(id)) return false; batch.Path.Add(id); }
            foreach (var pair in NodeValues)
            {
                if (!nodes.Contains(pair.Key) || !AlgorithmPersistentValidation.Value(pair.Value)) return false;
                batch.NodeValues.Add(pair.Key, pair.Value.Copy());
            }
            if (!AlgorithmPersistentValidation.TriggerValues(Trigger)) return false;
            record = new AlgorithmRunRecord(RunId, Trigger, batch, plan);
            return true;
        }
    }

    internal static class AlgorithmPersistentValidation
    {
        internal static bool Value(AlgorithmValue value) => AlgorithmValidator.Finite(value) &&
            Enum.IsDefined(typeof(AlgorithmValueKind), value.Type.Kind) && value.Type.Capabilities != null &&
            (value.Type.Kind != AlgorithmValueKind.TreeGrid || !value.IsValid || value.Trees != null);

        internal static bool TriggerValues(AlgorithmTrigger trigger)
        {
            if (trigger == null || trigger.Inputs == null || string.IsNullOrEmpty(trigger.Port)) return false;
            foreach (var pair in trigger.Inputs) if (string.IsNullOrEmpty(pair.Key) || !Value(pair.Value)) return false;
            return true;
        }
    }

    public sealed partial class AlgorithmRuntime
    {
        /// <summary>The caller must also capture physical queues and task responsibility at the same world boundary.</summary>
        public bool TryCapturePersistent(long now, out AlgorithmPersistentSnapshot snapshot)
        {
            snapshot = null;
            if (_disposed || _pumping || now < 0 || (_current == null) != (_batch == null) || (_current == null) != (_lease == null)) return false;
            if (_lease != null && _lease.State != ComputeState.Waiting && _lease.State != ComputeState.Running) return false;
            var events = new List<AlgorithmTrigger>(_events.Count);
            foreach (var item in _events) events.Add(item.Copy());
            var delays = new List<AlgorithmDelaySnapshot>(_delays.Count);
            foreach (var item in _delays) delays.Add(new AlgorithmDelaySnapshot { Trigger = item.Trigger.Copy(),
                RemainingMilliseconds = Math.Max(0, item.Due - (Paused ? _pauseAt : now)), ComputeLeaseId = item.Lease?.Id ?? PersistentId.Invalid });
            var leases = new Dictionary<string, PersistentId>(StringComparer.Ordinal);
            foreach (var pair in _stateLeases) leases.Add(pair.Key, pair.Value.Id);
            var history = new List<AlgorithmHistorySnapshot>(_history.Count);
            foreach (var item in _history) history.Add(AlgorithmHistorySnapshot.Capture(item));
            snapshot = new AlgorithmPersistentSnapshot { InstanceId = InstanceId.Value, Generation = Generation, RunSequence = _runSequence,
                Applied = CopyApplied(), State = CopyState(), Events = events.ToArray(), Delays = delays.ToArray(), History = history.ToArray(),
                StateLeases = leases, Prepared = _current == null ? null : AlgorithmPreparedSnapshot.Capture(_current, _batch, _lease.Id),
                Invalid = Invalid, PauseReasons = _pauseReasons, Reason = LastReason };
            return true;
        }

        /// <summary>Only an isolated, freshly constructed runtime may receive persistent state. No cancellation or Startup is emitted.</summary>
        public bool RestorePersistent(AlgorithmPersistentSnapshot snapshot, long now)
        {
            if (_disposed || _pumping || _lease != null || _current != null || Generation != 1 || _events.Count != 0 ||
                _delays.Count != 0 || _history.Count != 0 || _state.Count != 0 || snapshot == null || now < 0 ||
                snapshot.InstanceId != InstanceId.Value || snapshot.Generation == 0 || snapshot.Generation == ulong.MaxValue ||
                snapshot.State == null || snapshot.StateLeases == null || snapshot.Events == null || snapshot.Delays == null || snapshot.History == null ||
                snapshot.Events.Length > 32 || snapshot.History.Length > 50 ||
                ((int)snapshot.PauseReasons & ~7) != 0 ||
                !AlgorithmValidator.TryCompile(snapshot.Applied, _compute.LogicCapacity, out var plan, out _)) return false;
            var nodes = new HashSet<ulong>();
            foreach (var node in snapshot.Applied.Nodes) if (!node.Deleted) nodes.Add(node.Id);
            bool Trigger(AlgorithmTrigger trigger) => trigger != null && trigger.InstanceId == InstanceId.Value &&
                trigger.Generation == snapshot.Generation && trigger.Revision == snapshot.Applied.Revision && nodes.Contains(trigger.NodeId) &&
                trigger.Time >= 0 && trigger.Time <= now && AlgorithmPersistentValidation.TriggerValues(trigger);
            foreach (var pair in snapshot.State) if (string.IsNullOrEmpty(pair.Key) || !AlgorithmPersistentValidation.Value(pair.Value)) return false;
            foreach (var trigger in snapshot.Events) if (!Trigger(trigger)) return false;
            foreach (var delay in snapshot.Delays)
                if (delay == null || !Trigger(delay.Trigger) || delay.RemainingMilliseconds < 0 || delay.RemainingMilliseconds > long.MaxValue - now) return false;
            var claims = new HashSet<PersistentId>();
            ComputeRequest Lease(PersistentId id, int cost, ComputeClass category)
            {
                if (!id.IsValid || !claims.Add(id) || !_compute.TryFindPersistentRequest(id, out var lease) || lease.Source != InstanceId ||
                    lease.Cost != cost || lease.Class != category || lease.MergeKind != ComputeMergeKind.None || lease.CanYieldAtBoundary) return null;
                return lease;
            }
            var stateLeases = new Dictionary<string, ComputeRequest>(StringComparer.Ordinal);
            foreach (var pair in snapshot.StateLeases)
            {
                if (!snapshot.State.ContainsKey(pair.Key)) return false;
                var lease = Lease(pair.Value, 2, ComputeClass.Continuation); if (lease == null) return false; stateLeases.Add(pair.Key, lease);
            }
            var delayLeases = new List<ComputeRequest>(snapshot.Delays.Length);
            foreach (var delay in snapshot.Delays)
            {
                var lease = delay.ComputeLeaseId.IsValid ? Lease(delay.ComputeLeaseId, 2, ComputeClass.Continuation) : null;
                if (delay.ComputeLeaseId.IsValid && lease == null) return false; delayLeases.Add(lease);
            }
            AlgorithmBatch prepared = null; ComputeRequest preparationLease = null;
            var newKeys = new List<string>();
            if (snapshot.Prepared != null)
            {
                if (!Trigger(snapshot.Prepared.Trigger) || !snapshot.Prepared.TryBuild(snapshot.Applied, out prepared)) return false;
                foreach (var pair in prepared.Writes) if (!snapshot.State.ContainsKey(pair.Key)) newKeys.Add(pair.Key);
                long requested = prepared.Cost + 2L * newKeys.Count;
                foreach (var intent in prepared.Intents) if (intent.Kind == AlgorithmNodeKind.Delay) requested += 2;
                if (requested > int.MaxValue || (preparationLease = Lease(snapshot.Prepared.ComputeLeaseId, (int)requested, ComputeClass.Evaluation)) == null) return false;
            }
            var history = new List<AlgorithmRunRecord>(snapshot.History.Length);
            ulong previous = 0;
            foreach (var row in snapshot.History)
            {
                if (row == null || row.RunId <= previous || row.RunId > snapshot.RunSequence || !row.TryBuild(InstanceId.Value, now, out var record)) return false;
                history.Add(record); previous = row.RunId;
            }
            // All validation completed before changing candidate state. Physical authorities are restored afterward.
            _plan = plan; _evaluator = NewEvaluator(plan); Generation = snapshot.Generation + 1;
            foreach (var pair in snapshot.State) _state.Add(pair.Key, pair.Value.Copy());
            foreach (var pair in stateLeases) _stateLeases.Add(pair.Key, pair.Value);
            foreach (var trigger in snapshot.Events) { var copy = trigger.Copy(); copy.Generation = Generation; _events.Enqueue(copy); }
            for (int i = 0; i < snapshot.Delays.Length; i++) { var delay = snapshot.Delays[i]; var copy = delay.Trigger.Copy(); copy.Generation = Generation;
                _delays.Add(new Deferred { Trigger = copy, Due = now + delay.RemainingMilliseconds, Lease = delayLeases[i] }); }
            if (snapshot.Prepared != null)
            {
                _current = snapshot.Prepared.Trigger.Copy(); _current.Generation = Generation; _batch = prepared; _lease = preparationLease;
                _newStateKeys.Clear(); _newStateKeys.AddRange(newKeys);
            }
            foreach (var record in history) _history.Enqueue(record);
            _runSequence = snapshot.RunSequence; Invalid = snapshot.Invalid; LastReason = snapshot.Reason;
            _pauseReasons = snapshot.PauseReasons; _pauseAt = now;
            return true;
        }
    }

    public sealed class AlgorithmTemplateSnapshot
    {
        public ulong Id, Version;
        public string Name;
        public bool System;
        public AlgorithmDocument Document;
    }

    public sealed partial class AlgorithmTemplateLibrary
    {
        public AlgorithmTemplateSnapshot[] CapturePersistent()
        {
            var rows = new List<AlgorithmTemplateSnapshot>(_items.Count);
            foreach (var pair in _items) rows.Add(new AlgorithmTemplateSnapshot { Id = pair.Key, Version = pair.Value.Version,
                Name = pair.Value.Name, System = pair.Value.System, Document = pair.Value.Document.Copy() });
            rows.Sort((a, b) => a.Id.CompareTo(b.Id)); return rows.ToArray();
        }

        public bool RestorePersistent(AlgorithmTemplateSnapshot[] snapshot)
        {
            if (_items.Count != 0 || snapshot == null) return false;
            var entries = new Dictionary<ulong, Template>();
            foreach (var row in snapshot)
            {
                if (row == null || row.Id == 0 || row.Version == 0 || string.IsNullOrWhiteSpace(row.Name) || entries.ContainsKey(row.Id) ||
                    row.Document?.Bindings == null || row.Document.Bindings.Count != 0 ||
                    !AlgorithmValidator.TryCompile(row.Document, int.MaxValue, out _, out _, true)) return false;
                entries.Add(row.Id, new Template { Name = row.Name, Version = row.Version, System = row.System, Document = row.Document.Copy() });
            }
            foreach (var pair in entries) { _items.Add(pair.Key, pair.Value); _ids.TryRestore(new PersistentId(pair.Key)); }
            return true;
        }
    }

    public sealed class AlgorithmApplySnapshot
    {
        public ulong RequestId, DraftRevision, ExpectedAppliedRevision, HardwareRevision;
        public AlgorithmApplyState State;
        public string Reason;
        public AlgorithmDocument Document;
        public bool ParametersOnly, WarningsConfirmed;
        internal static AlgorithmApplySnapshot Capture(AlgorithmApplyRequest request) => request == null ? null :
            new AlgorithmApplySnapshot { RequestId = request.RequestId, DraftRevision = request.DraftRevision,
                ExpectedAppliedRevision = request.ExpectedAppliedRevision, HardwareRevision = request.HardwareRevision,
                State = request.State, Reason = request.Reason, Document = request.Document?.Copy(),
                ParametersOnly = request.ParametersOnly, WarningsConfirmed = request.WarningsConfirmed };
        internal AlgorithmApplyRequest Build() => new AlgorithmApplyRequest { RequestId = RequestId, DraftRevision = DraftRevision,
            ExpectedAppliedRevision = ExpectedAppliedRevision, HardwareRevision = HardwareRevision, State = State, Reason = Reason,
            Document = Document?.Copy(), ParametersOnly = ParametersOnly, WarningsConfirmed = WarningsConfirmed };
    }

    public sealed class AlgorithmInstanceSnapshot
    {
        public ulong InstanceId;
        public AlgorithmDocument Draft, Saved;
        public AlgorithmPersistentSnapshot Runtime;
        public AlgorithmApplySnapshot Request;
    }

    public sealed partial class AlgorithmInstanceService
    {
        public bool TryCapturePersistent(long now, out AlgorithmInstanceSnapshot[] snapshot)
        {
            snapshot = null;
            if (_disposed || _publishing != null || now < 0) return false;
            var rows = new List<AlgorithmInstanceSnapshot>(_entries.Count);
            foreach (var pair in _entries)
            {
                AlgorithmPersistentSnapshot runtime = null;
                if (pair.Value.Runtime != null && !pair.Value.Runtime.TryCapturePersistent(now, out runtime)) return false;
                rows.Add(new AlgorithmInstanceSnapshot { InstanceId = pair.Key, Draft = pair.Value.Draft.Copy(), Saved = pair.Value.Saved.Copy(),
                    Request = AlgorithmApplySnapshot.Capture(pair.Value.Request), Runtime = runtime });
            }
            rows.Sort((a, b) => a.InstanceId.CompareTo(b.InstanceId)); snapshot = rows.ToArray(); return true;
        }

        /// <summary>Factory creates isolated runtimes/sinks. Attach them to the adapter only after this method succeeds.</summary>
        public bool RestorePersistent(AlgorithmInstanceSnapshot[] snapshot, long now,
            Func<ulong, AlgorithmPlan, AlgorithmRuntime> createRuntime, out string reason)
        {
            reason = "InvalidAlgorithmSnapshot";
            if (_disposed || _entries.Count != 0 || snapshot == null || now < 0 || createRuntime == null) return false;
            var candidates = new Dictionary<ulong, Entry>();
            var requestIds = new HashSet<ulong>(); int total = 0; bool committed = false;
            try
            {
                foreach (var row in snapshot)
                {
                    if (row == null || row.InstanceId == 0 || candidates.ContainsKey(row.InstanceId) ||
                        !ValidDocument(row.Draft, row.InstanceId) || !ValidDocument(row.Saved, row.InstanceId)) return false;
                    var entry = new Entry { Draft = row.Draft.Copy(), Saved = row.Saved.Copy() };
                    candidates.Add(row.InstanceId, entry);
                    if (row.Runtime != null)
                    {
                        if (row.Runtime.InstanceId != row.InstanceId || row.Runtime.Applied?.DocumentId != row.InstanceId ||
                            !AlgorithmValidator.TryCompile(row.Runtime.Applied, _compute.LogicCapacity - total, out var plan, out _)) return false;
                        entry.Runtime = createRuntime(row.InstanceId, plan);
                        if (entry.Runtime == null || !entry.Runtime.RestorePersistent(row.Runtime, now)) return false;
                        total += entry.Runtime.LogicCost;
                    }
                    if (row.Request != null)
                    {
                        var request = row.Request;
                        if (row.Runtime == null || request.RequestId == 0 || !requestIds.Add(request.RequestId) ||
                            !Enum.IsDefined(typeof(AlgorithmApplyState), request.State) || request.State == AlgorithmApplyState.Applying ||
                            !ValidDocument(request.Document, row.InstanceId) || request.DraftRevision != request.Document.Revision) return false;
                        entry.Request = request.Build();
                    }
                }
                foreach (ulong id in requestIds) if (candidates.ContainsKey(id)) return false;
                if (!_compute.TryApplyLogicCost(total)) { reason = "InsufficientLogicCapacity"; return false; }
                foreach (var pair in candidates)
                {
                    _entries.Add(pair.Key, pair.Value); _ids.TryRestore(new PersistentId(pair.Key));
                    if (pair.Value.Request != null) _ids.TryRestore(new PersistentId(pair.Value.Request.RequestId));
                    if (pair.Value.Runtime != null) pair.Value.Runtime.Changed += OnRuntimeChanged;
                }
                _orderDirty = true; _now = now; committed = true; reason = null; return true;
            }
            finally
            {
                if (!committed) foreach (var entry in candidates.Values) entry.Runtime?.Dispose();
            }
        }

        private static bool ValidDocument(AlgorithmDocument document, ulong instance) => document != null &&
            document.DocumentId == instance && document.SchemaVersion == 1 && document.LanguageVersion == "1" && document.Revision != 0 &&
            document.Nodes != null && document.Edges != null && document.Bindings != null;
    }
}
