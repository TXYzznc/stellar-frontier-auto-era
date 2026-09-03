using AutoEra.Motion;
using NUnit.Framework;

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

        private static void Advance(EffectorWorkKind kind, ref EffectorWorkPhase phase, params EffectorWorkSignal[] signals)
        {
            foreach (EffectorWorkSignal signal in signals)
            {
                Assert.That(EffectorWorkPresentation.TryTransition(kind, phase, signal, out phase), Is.True, signal.ToString());
            }
        }
    }
}
