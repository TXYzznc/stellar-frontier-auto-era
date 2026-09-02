using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionInterruptionEditModeTests
    {
        [Test]
        public void Recovery_InterpolatesFromMeasuredPoseWithoutUsingPreviousTarget()
        {
            GameObject root = new GameObject("Rig");
            try
            {
                MotionJointBinding binding = new MotionJointBinding("joint", root.transform, MotionJointChannel.Translation, Vector3.right, 0f, 1f, Vector3.zero, Vector3.zero, new Vector3(10f, 0f, 0f), Vector3.zero);
                MotionLocalPose measured = new MotionLocalPose(new Vector3(4f, 0f, 0f), Quaternion.identity);
                MotionLocalPose halfway = MotionInterruption.Resolve(MotionInterruptionPolicy.Retract, binding, measured, 0.5f);
                Assert.That(halfway.Position, Is.EqualTo(new Vector3(7f, 0f, 0f)));
                Assert.That(MotionInterruption.Resolve(MotionInterruptionPolicy.Reset, binding, measured, 1f).Position, Is.EqualTo(Vector3.zero));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void HoldAndImmediateStop_PreserveMeasuredPose()
        {
            GameObject root = new GameObject("Rig");
            try
            {
                MotionJointBinding binding = new MotionJointBinding("joint", root.transform, MotionJointChannel.Rotation, Vector3.up, 0f, 1f, Vector3.zero, Vector3.zero, Vector3.one, Vector3.up);
                MotionLocalPose measured = new MotionLocalPose(new Vector3(2f, 3f, 4f), Quaternion.Euler(0f, 30f, 0f));
                Assert.That(MotionInterruption.Resolve(MotionInterruptionPolicy.Hold, binding, measured, 1f).Position, Is.EqualTo(measured.Position));
                Assert.That(Quaternion.Angle(MotionInterruption.Resolve(MotionInterruptionPolicy.ImmediateStop, binding, measured, 1f).Rotation, measured.Rotation), Is.LessThan(0.001f));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
