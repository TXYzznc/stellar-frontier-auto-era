using AutoEra.Motion;
using NUnit.Framework;
using System.Reflection;
using AutoEra.World.Region;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionWorkBridgeEditModeTests
    {
        [Test]
        public void BridgeRequiresExplicitAuthorityInputs()
        {
            Assert.Throws<System.ArgumentNullException>(() => new MotionWorkBridge(null, null, null, "x", null));
        }

        [Test]
        public void WorkRequestPreservesAuthoritativeQueueResult()
        {
            MethodInfo method = typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.RequestWork));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ReturnType, Is.EqualTo(typeof(WorkRequestResult)));
        }

        [Test]
        public void WorkReleaseIsExplicitAndBoolean()
        {
            MethodInfo method = typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.ReleaseWork));
            Assert.That(method, Is.Not.Null);
            Assert.That(method.ReturnType, Is.EqualTo(typeof(bool)));
        }

        [Test]
        public void BridgeExposesAuthorityLifecycleAndSamplingBoundaries()
        {
            Assert.That(typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.Begin)), Is.Not.Null);
            Assert.That(typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.Sample)), Is.Not.Null);
            Assert.That(typeof(MotionWorkBridge).GetMethod(nameof(MotionWorkBridge.Dispose)), Is.Not.Null);
            Assert.That(typeof(MotionWorkBridge).GetProperty(nameof(MotionWorkBridge.TaskId)), Is.Not.Null);
        }
    }
}
