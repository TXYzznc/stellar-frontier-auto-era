using AutoEra.Motion;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionGraphCompositionEditModeTests
    {
        [Test]
        public void RestrictedNodeSet_SeparatesPrimitivesAndComposition()
        {
            Assert.That(MotionGraphComposition.IsPrimitiveNode(MotionNodeKind.Rotate), Is.True);
            Assert.That(MotionGraphComposition.IsCompositionNode(MotionNodeKind.Sequence), Is.True);
            Assert.That(MotionGraphComposition.IsCompositionNode(MotionNodeKind.Parallel), Is.True);
            Assert.That(MotionGraphComposition.IsCompositionNode(MotionNodeKind.Loop), Is.True);
            Assert.That(MotionGraphComposition.IsCompositionNode(MotionNodeKind.ConditionalWait), Is.True);
            Assert.That(MotionGraphComposition.IsCompositionNode(MotionNodeKind.Branch), Is.True);
        }
    }
}
