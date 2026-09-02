using UnityEngine;

namespace AutoEra.Motion
{
    public enum SlidingDoorPresentationMode { SinglePanel, DoublePanel }

    public struct SlidingDoorPresentationState
    {
        public float FirstPanelOpen;
        public float SecondPanelOpen;
        public bool PausedForOccupancy;
        public bool ReboundingToSafeOpen;
    }

    public static class SlidingDoorPresentation
    {
        public static SlidingDoorPresentationState Evaluate(
            SlidingDoorPresentationMode mode,
            bool requestedOpen,
            bool safetyOccupied,
            float normalizedProgress)
        {
            float open = safetyOccupied ? 1f : (requestedOpen ? normalizedProgress : 1f - normalizedProgress);
            open = Mathf.Clamp01(open);
            return new SlidingDoorPresentationState
            {
                FirstPanelOpen = open,
                SecondPanelOpen = mode == SlidingDoorPresentationMode.DoublePanel ? open : 0f,
                PausedForOccupancy = safetyOccupied,
                ReboundingToSafeOpen = safetyOccupied && !requestedOpen
            };
        }
    }

    public struct ConveyorPresentationState
    {
        public float UvOffset;
        public float EffectiveSpeed;
        public bool HoldingForBlockage;
    }

    public static class ConveyorPresentation
    {
        public static ConveyorPresentationState Advance(
            float currentUvOffset,
            float baseSpeed,
            float efficiency,
            bool blocked,
            float deltaSeconds)
        {
            float effectiveSpeed = blocked ? 0f : baseSpeed * Mathf.Clamp01(efficiency);
            return new ConveyorPresentationState
            {
                UvOffset = blocked ? currentUvOffset : Mathf.Repeat(currentUvOffset + effectiveSpeed * deltaSeconds, 1f),
                EffectiveSpeed = effectiveSpeed,
                HoldingForBlockage = blocked
            };
        }
    }
}
