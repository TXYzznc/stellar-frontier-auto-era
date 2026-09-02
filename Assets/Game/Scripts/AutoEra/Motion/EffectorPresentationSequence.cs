namespace AutoEra.Motion
{
    public enum EffectorPresentationPhase
    {
        Detached,
        Aligned,
        Unlocked,
        Connecting,
        Locked,
        SafetyHolding,
        Recovering,
        Cancelled
    }

    public enum EffectorPresentationCommand
    {
        Align,
        Unlock,
        Detach,
        Connect,
        Lock,
        Cancel,
        PowerLost,
        PowerRestored,
        CompleteRecovery
    }

    public static class EffectorPresentationSequence
    {
        public static bool TryTransition(
            EffectorPresentationPhase current,
            EffectorPresentationCommand command,
            out EffectorPresentationPhase next)
        {
            next = current;
            switch (command)
            {
                case EffectorPresentationCommand.Align:
                    if (current == EffectorPresentationPhase.Detached || current == EffectorPresentationPhase.Cancelled)
                        next = EffectorPresentationPhase.Aligned;
                    break;
                case EffectorPresentationCommand.Unlock:
                    if (current == EffectorPresentationPhase.Locked)
                        next = EffectorPresentationPhase.Unlocked;
                    break;
                case EffectorPresentationCommand.Detach:
                    if (current == EffectorPresentationPhase.Unlocked)
                        next = EffectorPresentationPhase.Detached;
                    break;
                case EffectorPresentationCommand.Connect:
                    if (current == EffectorPresentationPhase.Aligned)
                        next = EffectorPresentationPhase.Connecting;
                    break;
                case EffectorPresentationCommand.Lock:
                    if (current == EffectorPresentationPhase.Connecting)
                        next = EffectorPresentationPhase.Locked;
                    break;
                case EffectorPresentationCommand.Cancel:
                    if (current == EffectorPresentationPhase.Aligned || current == EffectorPresentationPhase.Connecting)
                        next = EffectorPresentationPhase.Cancelled;
                    break;
                case EffectorPresentationCommand.PowerLost:
                    if (current != EffectorPresentationPhase.Detached)
                        next = EffectorPresentationPhase.SafetyHolding;
                    break;
                case EffectorPresentationCommand.PowerRestored:
                    if (current == EffectorPresentationPhase.SafetyHolding)
                        next = EffectorPresentationPhase.Recovering;
                    break;
                case EffectorPresentationCommand.CompleteRecovery:
                    if (current == EffectorPresentationPhase.Recovering)
                        next = EffectorPresentationPhase.Aligned;
                    break;
            }

            return next != current;
        }
    }
}
