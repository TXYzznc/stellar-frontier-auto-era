using System;
using AutoEra.World.Identity;

namespace AutoEra.Events
{
    /// <summary>One immutable journal entry. Strings are held by reference; records never mutate.</summary>
    public readonly struct EventJournalRecord
    {
        public EventJournalRecord(EventKind kind, EventDomain domain, CorrelationId correlation, CorrelationId causation,
            PersistentId source, string action, long worldMilliseconds, ulong sequence, bool terminal, EventOutcome outcome)
        {
            Kind = kind;
            Domain = domain;
            Correlation = correlation;
            Causation = causation;
            Source = source;
            Action = action ?? string.Empty;
            WorldMilliseconds = worldMilliseconds;
            Sequence = sequence;
            Terminal = terminal;
            Outcome = outcome;
        }

        public EventKind Kind { get; }
        public EventDomain Domain { get; }
        public CorrelationId Correlation { get; }
        public CorrelationId Causation { get; }
        public PersistentId Source { get; }
        public string Action { get; }
        public long WorldMilliseconds { get; }
        public ulong Sequence { get; }
        public bool Terminal { get; }
        public EventOutcome Outcome { get; }
    }

    /// <summary>Resolved trace of one trigger. Diagnostic-only allocation path.</summary>
    public sealed class EventTrace
    {
        internal EventTrace(EventJournalRecord command, bool hasCommand, int factCount, EventJournalRecord terminal, bool resolved)
        {
            Command = command;
            HasCommand = hasCommand;
            FactCount = factCount;
            Terminal = terminal;
            Resolved = resolved;
        }

        public EventJournalRecord Command { get; }
        public bool HasCommand { get; }
        public int FactCount { get; }
        public EventJournalRecord Terminal { get; }
        public bool Resolved { get; }
        public EventOutcome Outcome => Resolved ? Terminal.Outcome : EventOutcome.None;
    }

    /// <summary>
    /// Fixed-capacity ring buffer of journal records. Preallocated storage, constant memory,
    /// oldest records are overwritten. Appends and scans are main-thread only.
    /// </summary>
    public sealed class EventJournal
    {
        private readonly EventJournalRecord[] _records;
        private int _head;
        private int _count;
        private ulong _appended;

        public EventJournal(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            _records = new EventJournalRecord[capacity];
        }

        public int Count => _count;
        public int Capacity => _records.Length;

        public void Append(in EventJournalRecord record)
        {
            _records[_head] = record;
            _head = (_head + 1) % _records.Length;
            if (_count < _records.Length) _count++;
        }

        /// <summary>Monotonic dispatch sequence, independent of ring wraparound.</summary>
        public ulong NextSequence()
        {
            return ++_appended;
        }

        /// <summary>
        /// 把最近的记录按「由旧到新」追加到目标列表（**不清空**目标，调用方自行复用）。
        ///
        /// 这是给只读展示用的快照：调用方拿到的是一份拷贝，环形缓冲随后继续被覆盖也不影响它。
        /// 之所以写成「写入调用方列表」而不是返回数组，是为了让界面在每次刷新时零分配。
        /// maxCount ≤ 0 表示取当前全部。
        /// </summary>
        public void CopyRecent(System.Collections.Generic.List<EventJournalRecord> destination, int maxCount = 0)
        {
            if (destination == null || _count == 0)
            {
                return;
            }

            int take = maxCount > 0 && maxCount < _count ? maxCount : _count;
            int start = _count < _records.Length ? 0 : _head;
            int skip = _count - take;
            for (int i = 0; i < take; i++)
            {
                destination.Add(_records[(start + skip + i) % _records.Length]);
            }
        }

        public bool TryGetTrace(CorrelationId correlation, out EventTrace trace)
        {
            if (!correlation.IsValid)
            {
                trace = null;
                return false;
            }

            bool hasCommand = false;
            EventJournalRecord command = default;
            EventJournalRecord terminal = default;
            int factCount = 0;
            bool resolved = false;
            // Oldest-to-newest scan keeps the terminal fact as the last match.
            int start = _count < _records.Length ? 0 : _head;
            for (int i = 0; i < _count; i++)
            {
                ref EventJournalRecord record = ref _records[(start + i) % _records.Length];
                if (record.Correlation != correlation) continue;
                if (record.Kind == EventKind.Command)
                {
                    command = record;
                    hasCommand = true;
                }
                else
                {
                    factCount++;
                    if (record.Terminal) { terminal = record; resolved = true; }
                }
            }

            if (!hasCommand && factCount == 0)
            {
                trace = null;
                return false;
            }

            trace = new EventTrace(command, hasCommand, factCount, terminal, resolved);
            return true;
        }
    }
}