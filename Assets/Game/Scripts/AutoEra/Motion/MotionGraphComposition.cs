using System;

namespace AutoEra.Motion
{
    /// <summary>Restricted graph-composition semantics. No dynamic scripts or scene references are representable.</summary>
    public static class MotionGraphComposition
    {
        public static bool IsCompositionNode(MotionNodeKind kind)
        {
            return kind == MotionNodeKind.Sequence || kind == MotionNodeKind.Parallel || kind == MotionNodeKind.Loop || kind == MotionNodeKind.ConditionalWait || kind == MotionNodeKind.Branch;
        }

        public static bool IsPrimitiveNode(MotionNodeKind kind)
        {
            return kind == MotionNodeKind.Rotate || kind == MotionNodeKind.Translate || kind == MotionNodeKind.Aim || kind == MotionNodeKind.OpenClose || kind == MotionNodeKind.ContinuousRotate || kind == MotionNodeKind.Oscillate || kind == MotionNodeKind.Wait;
        }

        public static bool IsSupported(MotionNodeKind kind)
        {
            return IsPrimitiveNode(kind) || IsCompositionNode(kind);
        }
    }
}
