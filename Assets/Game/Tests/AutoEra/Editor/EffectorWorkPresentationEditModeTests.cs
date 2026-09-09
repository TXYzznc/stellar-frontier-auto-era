using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class EffectorWorkPresentationEditModeTests
    {
        [Test]
        public void WaterSpray_ClosesValveBeforeCompletingOrHolding()
        {
            EffectorWorkPhase phase = EffectorWorkPhase.Idle;
            Assert.That(EffectorWorkPresentation.TryTransition(EffectorWorkKind.WaterSpray, phase, EffectorWorkSignal.Start, out phase), Is.True);
            Assert.That(EffectorWorkPresentation.TryTransition(EffectorWorkKind.WaterSpray, phase, EffectorWorkSignal.Aligned, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.Working));
            Assert.That(EffectorWorkPresentation.TryTransition(EffectorWorkKind.WaterSpray, phase, EffectorWorkSignal.Interrupt, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.ValveClosing));
            Assert.That(EffectorWorkPresentation.TryTransition(EffectorWorkKind.WaterSpray, phase, EffectorWorkSignal.ValveClosed, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.Completed));
        }

        [Test]
        public void SawCut_RetractsAndSpinsDownBeforeCompletion()
        {
            EffectorWorkPhase phase = EffectorWorkPhase.Idle;
            Advance(EffectorWorkKind.SawCut, ref phase, EffectorWorkSignal.Start, EffectorWorkSignal.Aligned, EffectorWorkSignal.Ready, EffectorWorkSignal.Ready, EffectorWorkSignal.ContactMade);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.Working));
            Advance(EffectorWorkKind.SawCut, ref phase, EffectorWorkSignal.WorkCompleted, EffectorWorkSignal.Retracted, EffectorWorkSignal.SpindleStopped);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.Completed));
        }

        [Test]
        public void DrillMine_PowerLossRetractsBeforeDustAndSpindown()
        {
            EffectorWorkPhase phase = EffectorWorkPhase.Idle;
            Advance(EffectorWorkKind.DrillMine, ref phase, EffectorWorkSignal.Start, EffectorWorkSignal.Aligned, EffectorWorkSignal.Ready, EffectorWorkSignal.ContactMade);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.Working));
            Assert.That(EffectorWorkPresentation.TryTransition(EffectorWorkKind.DrillMine, phase, EffectorWorkSignal.PowerLost, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.Retracting));
            Advance(EffectorWorkKind.DrillMine, ref phase, EffectorWorkSignal.Retracted, EffectorWorkSignal.DustSettled, EffectorWorkSignal.SpindleStopped);
            Assert.That(phase, Is.EqualTo(EffectorWorkPhase.Completed));
        }

        [Test]
        public void SawPreview_DrivesDeclaredLiftAndFeedWithoutOverwritingBindPose()
        {
            GameObject root = new GameObject("preview");
            try
            {
                Transform yaw = Child(root.transform, "yaw", Vector3.zero);
                Transform pitch = Child(yaw, "pitch", Vector3.zero);
                Transform lift = Child(pitch, "lift", new Vector3(0f, 0.35f, 0.25f));
                Transform feed = Child(lift, "feed", new Vector3(0f, 0f, 0.3f));
                Transform rotor = Child(feed, "rotor", new Vector3(0f, 0f, 0.55f));
                EffectorWorkRigPreview preview = root.AddComponent<EffectorWorkRigPreview>();
                preview.Configure(EffectorWorkKind.SawCut, yaw, pitch, lift, feed, rotor, null);

                preview.Apply(EffectorWorkPhase.Working, 1f, 0.5f);

                Assert.That(lift.localPosition, Is.EqualTo(new Vector3(0f, 1.1f, 0.25f)));
                Assert.That(feed.localPosition, Is.EqualTo(new Vector3(0f, 0f, 0.8f)));
                Assert.That(yaw.localEulerAngles.y, Is.EqualTo(30f).Within(0.01f));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void DrillPreview_PressesDownFromItsDeclaredBindPose()
        {
            GameObject root = new GameObject("preview");
            try
            {
                Transform yaw = Child(root.transform, "yaw", Vector3.zero);
                Transform press = Child(yaw, "press", new Vector3(0f, 0.95f, 0f));
                Transform rotor = Child(press, "rotor", new Vector3(0f, -0.4f, 0.2f));
                EffectorWorkRigPreview preview = root.AddComponent<EffectorWorkRigPreview>();
                preview.Configure(EffectorWorkKind.DrillMine, yaw, null, press, null, rotor, null);

                preview.Apply(EffectorWorkPhase.Pressing, 0.5f, 0.5f);

                Assert.That(press.localPosition.y, Is.EqualTo(-0.3f).Within(0.001f));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void WaterPreview_RendersArcAndLandingMarkerOnlyWhileSpraying()
        {
            GameObject root = new GameObject("preview");
            try
            {
                Transform valve = Child(root.transform, "valve", new Vector3(0f, 1f, 0f));
                GameObject arcObject = new GameObject("arc");
                LineRenderer arc = arcObject.AddComponent<LineRenderer>();
                GameObject markerObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Transform marker = markerObject.transform;
                EffectorWorkRigPreview preview = root.AddComponent<EffectorWorkRigPreview>();
                preview.Configure(EffectorWorkKind.WaterSpray, null, null, null, null, null, valve);
                preview.ConfigureVisualFeedback(arc, marker, null);

                preview.Apply(EffectorWorkPhase.Working, 0.5f, 0f);

                Assert.That(arc.enabled, Is.True);
                Assert.That(arc.positionCount, Is.EqualTo(12));
                Assert.That(marker.gameObject.activeSelf, Is.True);
                Assert.That(marker.position.y, Is.LessThan(valve.position.y));

                preview.Apply(EffectorWorkPhase.ValveClosing, 0.5f, 0f);

                Assert.That(arc.enabled, Is.False);
                Assert.That(marker.gameObject.activeSelf, Is.False);
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static void Advance(EffectorWorkKind kind, ref EffectorWorkPhase phase, params EffectorWorkSignal[] signals)
        {
            foreach (EffectorWorkSignal signal in signals)
            {
                Assert.That(EffectorWorkPresentation.TryTransition(kind, phase, signal, out phase), Is.True, signal.ToString());
            }
        }

        private static Transform Child(Transform parent, string name, Vector3 localPosition)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            return child;
        }
    }
}
