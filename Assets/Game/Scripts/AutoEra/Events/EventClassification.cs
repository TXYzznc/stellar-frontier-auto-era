namespace AutoEra.Events
{
    /// <summary>Accountable project domains. Producers attach facts to exactly one domain.</summary>
    public enum EventDomain
    {
        Task = 0,
        Algorithm = 1,
        Effector = 2,
        Resource = 3,
        Alert = 4,
    }

    /// <summary>Journal record kind. The event bus only ever receives Fact records.</summary>
    public enum EventKind
    {
        Command = 0,
        Fact = 1,
    }

    /// <summary>Final result of a trigger. Only terminal facts set a value other than None.</summary>
    public enum EventOutcome
    {
        None = 0,
        Succeeded = 1,
        Failed = 2,
        Cancelled = 3,
    }
}