using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionRigEditModeTests
    {
        [Test]
        public void Rig_ResolvesExplicitStableBindingAndPreservesDeclaredPoses()
        {
            GameObject root = new GameObject("Rig");
            try
            {
                Transform joint = new GameObject("Yaw").transform;
                joint.SetParent(root.transform, false);
                MotionRig rig = root.AddComponent<MotionRig>();
                var binding = new MotionJointBinding(
                    "arm_yaw", joint, MotionJointChannel.Rotation, Vector3.up, -90f, 90f,
                    new Vector3(1f, 2f, 3f), new Vector3(0f, 45f, 0f),
                    Vector3.zero, new Vector3(0f, -15f, 0f));
                rig.Configure(new[] { binding });

                Assert.That(rig.TryValidate(out string error), Is.True, error);
                Assert.That(rig.TryGetBinding("arm_yaw", out MotionJointBinding resolved), Is.True);
                Assert.That(resolved, Is.EqualTo(binding));
                Assert.That(resolved.BindLocalPosition, Is.EqualTo(new Vector3(1f, 2f, 3f)));
                Assert.That(Quaternion.Angle(resolved.SafeLocalRotation, Quaternion.Euler(0f, -15f, 0f)), Is.LessThan(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Rig_RejectsDuplicateStableIdsAndInvalidRanges()
        {
            GameObject root = new GameObject("Rig");
            try
            {
                Transform first = new GameObject("First").transform;
                first.SetParent(root.transform, false);
                Transform second = new GameObject("Second").transform;
                second.SetParent(root.transform, false);
                MotionRig rig = root.AddComponent<MotionRig>();
                rig.Configure(new[]
                {
                    new MotionJointBinding("joint", first, MotionJointChannel.Rotation, Vector3.up, -90f, 90f, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero),
                    new MotionJointBinding("joint", second, MotionJointChannel.Translation, Vector3.right, 2f, -2f, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero)
                });

                Assert.That(rig.TryValidate(out string error), Is.False);
                Assert.That(error, Does.Contain("invalid range"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
