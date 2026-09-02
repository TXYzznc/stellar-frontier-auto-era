using AutoEra.Motion;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class EffectorPresentationSequenceEditModeTests
    {
        [Test]
        public void Sequence_CompletesAttachAndDetachThroughExplicitPhases()
        {
            EffectorPresentationPhase phase = EffectorPresentationPhase.Detached;
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.Align, out phase), Is.True);
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.Connect, out phase), Is.True);
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.Lock, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorPresentationPhase.Locked));
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.Unlock, out phase), Is.True);
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.Detach, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorPresentationPhase.Detached));
        }

        [Test]
        public void Sequence_CancelsAndSafelyHoldsUntilPowerRecovery()
        {
            EffectorPresentationPhase phase = EffectorPresentationPhase.Aligned;
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.Connect, out phase), Is.True);
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.Cancel, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorPresentationPhase.Cancelled));
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.PowerLost, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorPresentationPhase.SafetyHolding));
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.PowerRestored, out phase), Is.True);
            Assert.That(EffectorPresentationSequence.TryTransition(phase, EffectorPresentationCommand.CompleteRecovery, out phase), Is.True);
            Assert.That(phase, Is.EqualTo(EffectorPresentationPhase.Aligned));
        }

        [Test]
        public void Sequence_RejectsInvalidTransitionWithoutChangingPhase()
        {
            bool changed = EffectorPresentationSequence.TryTransition(
                EffectorPresentationPhase.Detached, EffectorPresentationCommand.Lock, out EffectorPresentationPhase phase);

            Assert.That(changed, Is.False);
            Assert.That(phase, Is.EqualTo(EffectorPresentationPhase.Detached));
        }
    }
}
