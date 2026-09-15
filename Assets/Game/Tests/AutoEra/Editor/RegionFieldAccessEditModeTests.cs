using AutoEra.World.Region;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class RegionFieldAccessEditModeTests
    {
        [Test]
        public void Access_UsesSeparateEnterExitDistanceHeightAndViewport()
        {
            var gate = new RegionFieldAccess();
            var center = new Vector3(.5f, .5f, 1);
            Assert.That(gate.Evaluate(false, 11, 39, center), Is.True);
            Assert.That(gate.Evaluate(false, 14, 42, center), Is.False);
            Assert.That(gate.Evaluate(true, 14, 42, center), Is.True);
            Assert.That(gate.Evaluate(true, 17, 42, center), Is.False);
            Assert.That(gate.Evaluate(false, 11, 39, new Vector3(.01f, .5f, 1)), Is.False);
            Assert.That(gate.Evaluate(true, 11, 39, new Vector3(.01f, .5f, 1)), Is.True);
            Assert.That(gate.Evaluate(true, 11, 39, new Vector3(.5f, .5f, -1)), Is.False);
        }

        [Test]
        public void Access_InvalidMeasurementsOrConfigurationFailClosed()
        {
            var gate = new RegionFieldAccess();
            Assert.That(gate.Evaluate(true, float.NaN, 10, Vector3.one), Is.False);
            gate.ExitDistance = 1;
            Assert.That(gate.Evaluate(true, 0, 10, new Vector3(.5f,.5f,1)), Is.False);
            gate.ExitDistance = 16;
            gate.EnterViewportMargin = float.NaN;
            Assert.That(gate.Evaluate(true, 0, 10, new Vector3(.5f,.5f,1)), Is.False);
        }
    }
}
