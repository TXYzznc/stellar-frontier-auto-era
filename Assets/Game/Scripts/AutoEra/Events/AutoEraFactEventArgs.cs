using System;
using AutoEra.World.Identity;
using GameFramework.Event;

namespace AutoEra.Events
{
    /// <summary>
    /// Base class for every fact that may reach the event bus. The bus never carries
    /// commands or queries; commands open a correlation ticket and queries use read APIs.
    /// Derived types add their own typed payload and keep Clear symmetric.
    /// </summary>
    public abstract class AutoEraFactEventArgs : GameEventArgs
    {
        public EventDomain Domain { get; private set; }
        public CorrelationId Correlation { get; private set; }
        public CorrelationId Causation { get; private set; }
        public PersistentId Source { get; private set; }
        public string Action { get; private set; }
        public bool Terminal { get; private set; }
        public EventOutcome Outcome { get; private set; }
        public long WorldMilliseconds { get; private set; }
        public ulong Sequence { get; private set; }

        protected void Initialize(EventDomain domain, CorrelationId correlation, PersistentId source,
            string action, bool terminal, EventOutcome outcome)
        {
            if (!correlation.IsValid) throw new ArgumentException("A fact requires a valid correlation.", nameof(correlation));
            Domain = domain;
            Correlation = correlation;
            Source = source;
            Action = action ?? string.Empty;
            Terminal = terminal;
            Outcome = terminal ? outcome : EventOutcome.None;
        }

        /// <summary>Stamps dispatch metadata; called by the event service right before publishing.</summary>
        internal void Stamp(long worldMilliseconds, ulong sequence)
        {
            WorldMilliseconds = worldMilliseconds;
            Sequence = sequence;
        }

        /// <summary>Sets the parent trigger when this fact belongs to a chained trigger.</summary>
        protected void SetCausation(CorrelationId causation)
        {
            Causation = causation;
        }

        public override void Clear()
        {
            Domain = default;
            Correlation = default;
            Causation = default;
            Source = default;
            Action = null;
            Terminal = false;
            Outcome = default;
            WorldMilliseconds = 0L;
            Sequence = 0UL;
        }
    }
}