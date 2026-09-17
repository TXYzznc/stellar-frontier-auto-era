namespace AutoEra.Motion
{
    public enum EffectorWorkKind { WaterSpray, SawCut, DrillMine }
    public enum EffectorWorkPhase
    {
        Idle,
        Aiming,
        Lifting,
        SpinningUp,
        Feeding,
        Pressing,
        Working,
        ValveClosing,
        Retracting,
        SpinningDown,
        Dusting,
        Completed,
        SafeHolding
    }

    public enum EffectorWorkSignal
    {
        Start,
        Aligned,
        Ready,
        ContactMade,
        WorkCompleted,
        ValveClosed,
        Retracted,
        SpindleStopped,
        DustSettled,
        Interrupt,
        PowerLost
    }

    /// <summary>Presentation-only work phase transition. Product authority supplies signals and decides the task result.</summary>
    public static class EffectorWorkPresentation
    {
        public static bool TryTransition(
            EffectorWorkKind kind,
            EffectorWorkPhase current,
            EffectorWorkSignal signal,
            out EffectorWorkPhase next)
        {
            next = current;
            if (signal == EffectorWorkSignal.PowerLost)
            {
                next = kind == EffectorWorkKind.WaterSpray ? EffectorWorkPhase.ValveClosing : EffectorWorkPhase.Retracting;
                return next != current;
            }

            if (signal == EffectorWorkSignal.Interrupt)
            {
                if (kind == EffectorWorkKind.WaterSpray) next = EffectorWorkPhase.ValveClosing;
                else if (current == EffectorWorkPhase.Feeding || current == EffectorWorkPhase.Pressing || current == EffectorWorkPhase.Working) next = EffectorWorkPhase.Retracting;
                return next != current;
            }

            switch (kind)
            {
                case EffectorWorkKind.WaterSpray:
                    return TryWaterTransition(current, signal, out next);
                case EffectorWorkKind.SawCut:
                    return TrySawTransition(current, signal, out next);
                case EffectorWorkKind.DrillMine:
                    return TryDrillTransition(current, signal, out next);
                default:
                    return false;
            }
        }

        private static bool TryWaterTransition(EffectorWorkPhase current, EffectorWorkSignal signal, out EffectorWorkPhase next)
        {
            next = current;
            if (current == EffectorWorkPhase.Idle && signal == EffectorWorkSignal.Start) next = EffectorWorkPhase.Aiming;
            else if (current == EffectorWorkPhase.Aiming && signal == EffectorWorkSignal.Aligned) next = EffectorWorkPhase.Working;
            else if (current == EffectorWorkPhase.Working && signal == EffectorWorkSignal.WorkCompleted) next = EffectorWorkPhase.ValveClosing;
            else if (current == EffectorWorkPhase.ValveClosing && signal == EffectorWorkSignal.ValveClosed) next = EffectorWorkPhase.Completed;
            return next != current;
        }

        private static bool TrySawTransition(EffectorWorkPhase current, EffectorWorkSignal signal, out EffectorWorkPhase next)
        {
            next = current;
            if (current == EffectorWorkPhase.Idle && signal == EffectorWorkSignal.Start) next = EffectorWorkPhase.Aiming;
            else if (current == EffectorWorkPhase.Aiming && signal == EffectorWorkSignal.Aligned) next = EffectorWorkPhase.Lifting;
            else if (current == EffectorWorkPhase.Lifting && signal == EffectorWorkSignal.Ready) next = EffectorWorkPhase.SpinningUp;
            else if (current == EffectorWorkPhase.SpinningUp && signal == EffectorWorkSignal.Ready) next = EffectorWorkPhase.Feeding;
            else if (current == EffectorWorkPhase.Feeding && signal == EffectorWorkSignal.ContactMade) next = EffectorWorkPhase.Working;
            else if (current == EffectorWorkPhase.Working && signal == EffectorWorkSignal.WorkCompleted) next = EffectorWorkPhase.Retracting;
            else if (current == EffectorWorkPhase.Retracting && signal == EffectorWorkSignal.Retracted) next = EffectorWorkPhase.SpinningDown;
            else if (current == EffectorWorkPhase.SpinningDown && signal == EffectorWorkSignal.SpindleStopped) next = EffectorWorkPhase.Completed;
            return next != current;
        }

        private static bool TryDrillTransition(EffectorWorkPhase current, EffectorWorkSignal signal, out EffectorWorkPhase next)
        {
            next = current;
            if (current == EffectorWorkPhase.Idle && signal == EffectorWorkSignal.Start) next = EffectorWorkPhase.Aiming;
            else if (current == EffectorWorkPhase.Aiming && signal == EffectorWorkSignal.Aligned) next = EffectorWorkPhase.SpinningUp;
            else if (current == EffectorWorkPhase.SpinningUp && signal == EffectorWorkSignal.Ready) next = EffectorWorkPhase.Pressing;
            else if (current == EffectorWorkPhase.Pressing && signal == EffectorWorkSignal.ContactMade) next = EffectorWorkPhase.Working;
            else if (current == EffectorWorkPhase.Working && signal == EffectorWorkSignal.WorkCompleted) next = EffectorWorkPhase.Retracting;
            else if (current == EffectorWorkPhase.Retracting && signal == EffectorWorkSignal.Retracted) next = EffectorWorkPhase.Dusting;
            else if (current == EffectorWorkPhase.Dusting && signal == EffectorWorkSignal.DustSettled) next = EffectorWorkPhase.SpinningDown;
            else if (current == EffectorWorkPhase.SpinningDown && signal == EffectorWorkSignal.SpindleStopped) next = EffectorWorkPhase.Completed;
            return next != current;
        }
    }
}
