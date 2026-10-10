namespace AutoEra.Save
{
    /// <summary>Scheduler and report are one domain section in the same atomic world snapshot.</summary>
    public sealed class OfflineContinuationSnapshot
    {
        public int Version = 1;
        public AutoEra.World.Time.OfflineEventSnapshot Scheduler;
        public OfflineReportSnapshot Report;
    }
}
