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

        /// <summary>
        /// 记录一条**自发事实**：没有命令触发、也没有关联链，由系统自己在运行中产生
        /// （例如区域电网判定某台机器因缺电停机、储能电量耗尽）。
        ///
        /// 与 <see cref="PublishFact{TFact}"/> 的区别只有两点，都是有意的：
        /// ① 不要求 correlation——电网的停机不是某条命令的结果，硬造一个关联 Id 会污染追溯链；
        /// ② 不经过事件总线——第一版没有任何订阅者，广播一条无人监听的事件只是额外开销。
        /// 因此这类记录的追溯查询会如实回答「没有可追溯的关联链」，而不是假装已结案。
        /// </summary>
        public void PublishNotice(EventDomain domain, PersistentId source, string action)
        {
            ThrowIfDisposed();
            if (!Enum.IsDefined(typeof(EventDomain), domain)) throw new ArgumentOutOfRangeException(nameof(domain));
            if (string.IsNullOrWhiteSpace(action)) throw new ArgumentException("A notice requires an action.", nameof(action));

            _journal.Append(new EventJournalRecord(EventKind.Fact, domain, CorrelationId.Invalid, CorrelationId.Invalid,
                source, action, _clock.WorldMilliseconds, _journal.NextSequence(), true, EventOutcome.None));
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