using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionExecutorEditModeTests
    {
        [Test]
        public void Executor_ArbitratesJointOwnershipAndLifecycle()
        {
            GameObject root = new GameObject("Motion");
            try
            {
                Transform joint = new GameObject("Joint").transform;
                joint.SetParent(root.transform, false);
                MotionRig rig = root.AddComponent<MotionRig>();
                rig.Configure(new[] { new MotionJointBinding("joint", joint, MotionJointChannel.Rotation, Vector3.up, -90f, 90f, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero) });
                MotionExecutor executor = root.AddComponent<MotionExecutor>();
                executor.Configure(rig);

                Assert.That(executor.TryPrepare("first", new[] { "joint" }), Is.True);
                Assert.That(executor.TryPrepare("second", new[] { "joint" }), Is.False);
                Assert.That(executor.TryTransition("first", MotionExecutionState.Prepared, MotionExecutionState.Running), Is.True);
                Assert.That(executor.TryTransition("first", MotionExecutionState.Prepared, MotionExecutionState.Completed), Is.False);
                Assert.That(executor.TryTransition("first", MotionExecutionState.Running, MotionExecutionState.Completed), Is.True);
                Assert.That(executor.TryRelease("first"), Is.True);
                Assert.That(executor.TryPrepare("second", new[] { "joint" }), Is.True);
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
