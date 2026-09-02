using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionPrimitivesEditModeTests
    {
        [Test]
        public void RotationTranslationAndOpenClose_ClampNormalizedProgress()
        {
            Quaternion rotation = MotionPrimitives.Rotate(Quaternion.identity, Vector3.up, -90f, 90f, 2f);
            Vector3 forward = rotation * Vector3.forward;
            Assert.That(Vector3.Angle(forward, Vector3.right), Is.LessThan(0.01f));
            Assert.That(MotionPrimitives.Translate(Vector3.zero, Vector3.right, 0f, 2f, 2f), Is.EqualTo(new Vector3(2f, 0f, 0f)));
            Assert.That(MotionPrimitives.OpenClose(0f, 10f, -1f), Is.EqualTo(0f));
        }

        [Test]
        public void AimContinuousOscillationAndWait_AreDeterministic()
        {
            Quaternion aimed = MotionPrimitives.Aim(Quaternion.identity, Vector3.zero, Vector3.forward, Vector3.forward, Vector3.up, 1f);
            Assert.That(Quaternion.Angle(aimed, Quaternion.identity), Is.LessThan(0.01f));
            Quaternion rotating = MotionPrimitives.ContinuousRotate(Quaternion.identity, Vector3.up, 90f, 1f);
            Assert.That(Vector3.Angle(rotating * Vector3.forward, Vector3.right), Is.LessThan(0.01f));
            Assert.That(MotionPrimitives.Oscillate(-1f, 1f, 1f, 0.25f), Is.EqualTo(1f).Within(0.0001f));
            Assert.That(MotionPrimitives.Wait(0.5f, 1f), Is.False);
            Assert.That(MotionPrimitives.Wait(1f, 1f), Is.True);
        }
    }
}
