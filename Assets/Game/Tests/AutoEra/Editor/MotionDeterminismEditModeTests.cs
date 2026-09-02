using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionDeterminismEditModeTests
    {
        [Test]
        public void ContinuousRotation_MatchesAcrossEquivalentTimePartitions()
        {
            Quaternion oneStep = MotionPrimitives.ContinuousRotate(Quaternion.identity, Vector3.up, 90f, 2f);
            Quaternion partitioned = Quaternion.identity;
            for (int index = 1; index <= 20; index++)
            {
                partitioned = MotionPrimitives.ContinuousRotate(Quaternion.identity, Vector3.up, 90f, index * 0.1f);
            }
            Assert.That(Quaternion.Angle(oneStep, partitioned), Is.LessThan(0.001f));
        }

        [Test]
        public void InterruptionRecovery_UsesCurrentMeasuredPoseOnRepeatedRequests()
        {
            GameObject root = new GameObject("Rig");
            try
            {
                MotionJointBinding binding = new MotionJointBinding("joint", root.transform, MotionJointChannel.Translation, Vector3.right, 0f, 1f, Vector3.zero, Vector3.zero, new Vector3(10f, 0f, 0f), Vector3.zero);
                MotionLocalPose first = MotionInterruption.Resolve(MotionInterruptionPolicy.Retract, binding, new MotionLocalPose(Vector3.zero, Quaternion.identity), 0.5f);
                MotionLocalPose second = MotionInterruption.Resolve(MotionInterruptionPolicy.Retract, binding, first, 0.5f);
                Assert.That(second.Position, Is.EqualTo(new Vector3(7.5f, 0f, 0f)));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void PrimitiveHotLoop_DoesNotSustainManagedAllocations()
        {
            for (int index = 0; index < 32; index++)
            {
                MotionPrimitives.ContinuousRotate(Quaternion.identity, Vector3.up, 90f, index * 0.01f);
            }
            long before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 10000; index++)
            {
                MotionPrimitives.ContinuousRotate(Quaternion.identity, Vector3.up, 90f, index * 0.01f);
            }
            long allocated = System.GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.EqualTo(0L));
        }
    }
}
