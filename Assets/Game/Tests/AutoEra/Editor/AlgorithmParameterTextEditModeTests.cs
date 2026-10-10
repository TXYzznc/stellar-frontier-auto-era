using AutoEra.Algorithms;
using NUnit.Framework;

namespace AutoEra.Tests.Editor
{
    public sealed class AlgorithmParameterTextEditModeTests
    {
        [Test] public void ItemIdentifierRemainsEnumeration()
        { var type=AlgorithmType.Of(AlgorithmValueKind.Enumeration); type.EnumFamily="ResourceItem"; Assert.That(AlgorithmParameterText.TryParse(type,"30003",out var value),Is.True); Assert.That(value.EnumValue,Is.EqualTo(30003)); Assert.That(value.Type.Kind,Is.EqualTo(AlgorithmValueKind.Enumeration)); Assert.That(value.Type.EnumFamily,Is.EqualTo(type.EnumFamily)); Assert.That(value.Type,Is.Not.SameAs(type)); }
        [Test] public void CoordinateRetainsAllThreeComponents()
        { Assert.That(AlgorithmParameterText.TryParse(AlgorithmType.Of(AlgorithmValueKind.Position),"-7, 0, 8.6",out var value),Is.True); Assert.That(value.X,Is.EqualTo(-7)); Assert.That(value.Y,Is.Zero); Assert.That(value.Z,Is.EqualTo(8.6)); Assert.That(AlgorithmParameterText.TryParse(value.Type,AlgorithmParameterText.Format(value),out var copied),Is.True); Assert.That(copied.Z,Is.EqualTo(value.Z)); }
        [TestCase("NaN"),TestCase("Infinity"),TestCase("1e999")]
        public void NonFiniteNumbersRejected(string text) => Assert.That(AlgorithmParameterText.TryParse(AlgorithmType.Of(AlgorithmValueKind.Number,"s"),text,out _),Is.False);
        [Test] public void NumberKeepsUnitAndPrecision()
        { var type=AlgorithmType.Of(AlgorithmValueKind.Number,"s"); Assert.That(AlgorithmParameterText.TryParse(type,"0.123456789",out var value),Is.True); Assert.That(value.Type.Unit,Is.EqualTo("s")); Assert.That(value.Number,Is.EqualTo(.123456789)); }
        [Test] public void MalformedCoordinateAndFractionalEnumRejected()
        { Assert.That(AlgorithmParameterText.TryParse(AlgorithmType.Of(AlgorithmValueKind.Position),"1,2",out _),Is.False); Assert.That(AlgorithmParameterText.TryParse(AlgorithmType.Of(AlgorithmValueKind.Enumeration),"30003.5",out _),Is.False); }
    }
}
