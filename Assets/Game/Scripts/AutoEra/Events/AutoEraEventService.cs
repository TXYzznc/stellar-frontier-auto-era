using System;
using AutoEra.World.Identity;
using AutoEra.World.Time;
using GameFramework;

namespace AutoEra.Events
{
    /// <summary>
    /// Session-owned accountability service. Commands open correlations, facts carry them to
    /// the bus and the journal resolves one trigger to its source and final outcome.
    /// </summary>
    public sealed class AutoEraEventService : IDisposable
    {
        public const int DefaultJournalCapacity = 512;

        private readonly WorldClock _clock;
        private readonly IEventPublisher _publisher;
        private readonly EventJournal _journal;
        private readonly CorrelationAllocator _correlations = new CorrelationAllocator();
        private bool _isDisposed;

        public AutoEraEventService(WorldClock clock, IEventPublisher publisher, int journalCapacity = DefaultJournalCapacity)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _publisher = publisher ?? NullEventPublisher.Instance;
            _journal = new EventJournal(journalCapacity);
        }

        public EventJournal Journal => _journal;

        /// <summary>Opens a command correlation. Commands never reach the event bus.</summary>
        public CorrelationId OpenCommand(EventDomain domain, PersistentId source, string action)
        {
            ThrowIfDisposed();
            if (!Enum.IsDefined(typeof(EventDomain), domain)) throw new ArgumentOutOfRangeException(nameof(domain));
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("A command requires an action.", nameof(action));
            if (!_correlations.TryAllocate(out CorrelationId correlation))
                throw new InvalidOperationException("Correlation allocation exhausted.");
            _journal.Append(new EventJournalRecord(EventKind.Command, domain, correlation, CorrelationId.Invalid,
                source, action, _clock.WorldMilliseconds, 0UL, false, EventOutcome.None));
            return correlation;
        }

        /// <summary>
        /// Publishes a fact: stamps dispatch metadata, journals it and hands ownership to the
        /// publisher. The fact must have been acquired from the reference pool and initialized.
        /// </summary>
        public void PublishFact<TFact>(TFact fact) where TFact : AutoEraFactEventArgs
        {
            ThrowIfDisposed();
            if (fact == null) throw new ArgumentNullException(nameof(fact));
            if (!fact.Correlation.IsValid) throw new ArgumentException("A fact requires a valid correlation.", nameof(fact));
            if (!Enum.IsDefined(typeof(EventDomain), fact.Domain)) throw new ArgumentException("A fact requires a defined domain.", nameof(fact));
            long worldMilliseconds = _clock.WorldMilliseconds;
            ulong sequence = _journal.NextSequence();
            fact.Stamp(worldMilliseconds, sequence);
            _journal.Append(new EventJournalRecord(EventKind.Fact, fact.Domain, fact.Correlation, fact.Causation,
                fact.Source, fact.Action, worldMilliseconds, sequence, fact.Terminal, fact.Outcome));
            _publisher.Publish(fact);
        }

        /// <summary>Read-only trace lookup; never mutates the journal or allocates correlations.</summary>
        public bool TryGetTrace(CorrelationId correlation, out EventTrace trace)
        {
            ThrowIfDisposed();
            return _journal.TryGetTrace(correlation, out trace);
        }

        public void Dispose()
        {
            _isDisposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException(nameof(AutoEraEventService));
        }
    }
}