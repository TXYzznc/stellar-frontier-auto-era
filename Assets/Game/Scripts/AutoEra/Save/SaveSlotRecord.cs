using System;

namespace AutoEra.Save
{
    /// <summary>
    /// Persisted record of a single save slot. Metadata (version, world time, summary) is kept
    /// separate from the formal save content, which is carried as an opaque JSON blob until the
    /// world progression layer (P7-002+) interprets it.
    /// </summary>
    [Serializable]
    public sealed class SaveSlotRecord
    {
        /// <summary>Current save format version; increment on breaking format changes and add a migration.</summary>
        public const int CurrentVersion = 1;

        /// <summary>Save format version (reserved migration slot).</summary>
        public int Version { get; set; }

        /// <summary>Snapshot of the deterministic world clock in milliseconds at save time. Reading it never advances or blocks the clock.</summary>
        public long WorldTimeMilliseconds { get; set; }

        /// <summary>Human-readable slot summary shown in the save list.</summary>
        public string Summary { get; set; }

        /// <summary>Formal save content as an opaque JSON string.</summary>
        public string ContentJson { get; set; }
    }
}
