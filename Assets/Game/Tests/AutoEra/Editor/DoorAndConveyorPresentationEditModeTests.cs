using AutoEra.Motion;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class DoorAndConveyorPresentationEditModeTests
    {
        [Test]
        public void Door_UsesBothPanelsAndReboundsOpenForSafetyOccupancy()
        {
            SlidingDoorPresentationState doubleDoor = SlidingDoorPresentation.Evaluate(
                SlidingDoorPresentationMode.DoublePanel, false, true, 0.4f);
            SlidingDoorPresentationState singleDoor = SlidingDoorPresentation.Evaluate(
                SlidingDoorPresentationMode.SinglePanel, true, false, 0.25f);

            Assert.That(doubleDoor.FirstPanelOpen, Is.EqualTo(1f));
            Assert.That(doubleDoor.SecondPanelOpen, Is.EqualTo(1f));
            Assert.That(doubleDoor.PausedForOccupancy, Is.True);
            Assert.That(doubleDoor.ReboundingToSafeOpen, Is.True);
            Assert.That(singleDoor.FirstPanelOpen, Is.EqualTo(0.25f));
            Assert.That(singleDoor.SecondPanelOpen, Is.Zero);
        }

        [Test]
        public void Conveyor_HoldsExactOffsetWhenBlockedAndResumesAtEfficiencySpeed()
        {
            ConveyorPresentationState blocked = ConveyorPresentation.Advance(0.7f, 2f, 0.5f, true, 0.25f);
            ConveyorPresentationState resumed = ConveyorPresentation.Advance(blocked.UvOffset, 2f, 0.5f, false, 0.25f);

            Assert.That(blocked.HoldingForBlockage, Is.True);
            Assert.That(blocked.UvOffset, Is.EqualTo(0.7f));
            Assert.That(blocked.EffectiveSpeed, Is.Zero);
            Assert.That(resumed.HoldingForBlockage, Is.False);
            Assert.That(resumed.EffectiveSpeed, Is.EqualTo(1f));
            Assert.That(resumed.UvOffset, Is.EqualTo(0.95f).Within(0.0001f));
        }
    }
}
