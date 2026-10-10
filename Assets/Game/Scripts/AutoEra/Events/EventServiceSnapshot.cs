using System;
using System.Collections.Generic;
using AutoEra.World.Identity;

namespace AutoEra.Events
{
    public sealed class EventServiceSnapshot
    {
        public ulong CorrelationsAllocatedThrough, DispatchSequence;
        public EventJournalSnapshotRecord[] Records;
    }
    public sealed class EventJournalSnapshotRecord
    {
        public EventKind Kind;
        public EventDomain Domain;
        public ulong Correlation, Causation, Source, Sequence;
        public string Action;
        public long WorldMilliseconds;
        public bool Terminal;
        public EventOutcome Outcome;
    }
    public sealed partial class EventJournal
    {
        internal ulong CapturedSequence => _appended;
        internal void RestoreSequence(ulong value) { _appended = value; }
    }
    public sealed partial class CorrelationAllocator
    {
        internal ulong AllocatedThrough => _nextValue == 0 ? ulong.MaxValue : _nextValue - 1;
        internal void RestoreThrough(ulong value) { _nextValue = value == ulong.MaxValue ? 0 : value + 1; }
    }
    public sealed partial class AutoEraEventService
    {
        public EventServiceSnapshot Capture()
        {
            ThrowIfDisposed();
            var recent = new List<EventJournalRecord>(_journal.Count); _journal.CopyRecent(recent);
            var records = new EventJournalSnapshotRecord[recent.Count];
            for (int i=0;i<records.Length;i++)
            {
                var value = recent[i]; records[i] = new EventJournalSnapshotRecord
                { Kind=value.Kind, Domain=value.Domain, Correlation=value.Correlation.Value, Causation=value.Causation.Value,
                    Source=value.Source.Value, Action=value.Action, WorldMilliseconds=value.WorldMilliseconds, Sequence=value.Sequence, Terminal=value.Terminal, Outcome=value.Outcome };
            }
            return new EventServiceSnapshot { CorrelationsAllocatedThrough=_correlations.AllocatedThrough, DispatchSequence=_journal.CapturedSequence, Records=records };
        }
        public void Restore(EventServiceSnapshot snapshot)
        {
            ThrowIfDisposed();
            if (_journal.Count != 0 || _journal.CapturedSequence != 0 || _correlations.AllocatedThrough != 0) throw new InvalidOperationException("Event restoration requires an empty candidate.");
            if (snapshot?.Records == null || snapshot.Records.Length > _journal.Capacity) throw new ArgumentException("Invalid event journal snapshot.");
            ulong previous = 0; var commands = new HashSet<ulong>(); var records = new List<EventJournalRecord>(snapshot.Records.Length);
            foreach (var value in snapshot.Records)
            {
                if (value == null || !Enum.IsDefined(typeof(EventKind),value.Kind) || !Enum.IsDefined(typeof(EventDomain),value.Domain) ||
                    !Enum.IsDefined(typeof(EventOutcome),value.Outcome) || string.IsNullOrWhiteSpace(value.Action) || value.WorldMilliseconds < 0 || value.WorldMilliseconds > _clock.WorldMilliseconds ||
                    value.Correlation > snapshot.CorrelationsAllocatedThrough || value.Causation > snapshot.CorrelationsAllocatedThrough)
                    throw new ArgumentException("Invalid journal record.");
                if (value.Kind == EventKind.Command)
                {
                    if (value.Correlation == 0 || !commands.Add(value.Correlation) || value.Sequence != 0 || value.Terminal || value.Outcome != EventOutcome.None)
                        throw new ArgumentException("Invalid command journal record.");
                }
                else
                {
                    if (value.Sequence <= previous || value.Sequence > snapshot.DispatchSequence) throw new ArgumentException("Invalid fact sequence.");
                    previous = value.Sequence;
                }
                records.Add(new EventJournalRecord(value.Kind,value.Domain,new CorrelationId(value.Correlation),new CorrelationId(value.Causation),
                    new PersistentId(value.Source),value.Action,value.WorldMilliseconds,value.Sequence,value.Terminal,value.Outcome));
            }
            // Silent reconstruction: do not publish old facts to live subscribers.
            foreach (var value in records) _journal.Append(value);
            _journal.RestoreSequence(snapshot.DispatchSequence); _correlations.RestoreThrough(snapshot.CorrelationsAllocatedThrough);
        }
        internal void ActivatePublisher(IEventPublisher publisher)
        {
            ThrowIfDisposed();
            if (_publisher != NullEventPublisher.Instance) throw new InvalidOperationException("Candidate publisher is already active.");
            _publisher = publisher ?? NullEventPublisher.Instance;
        }
        internal void SuspendPublisher()
        { ThrowIfDisposed();_publisher=NullEventPublisher.Instance; }
    }
}
