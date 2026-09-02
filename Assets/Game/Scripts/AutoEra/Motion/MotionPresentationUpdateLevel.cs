namespace AutoEra.Motion
{
    public enum MotionPresentationUpdateLevel { Near, Mid, Far, Invisible }

    public static class MotionPresentationUpdatePolicy
    {
        public static bool ShouldEvaluate(MotionPresentationUpdateLevel level) => level != MotionPresentationUpdateLevel.Invisible;
        public static float GetMinimumIntervalSeconds(MotionPresentationUpdateLevel level)
        {
            switch (level)
            {
                case MotionPresentationUpdateLevel.Near: return 0f;
                case MotionPresentationUpdateLevel.Mid: return 1f / 15f;
                case MotionPresentationUpdateLevel.Far: return 0.5f;
                default: return float.PositiveInfinity;
            }
        }
    }
}
