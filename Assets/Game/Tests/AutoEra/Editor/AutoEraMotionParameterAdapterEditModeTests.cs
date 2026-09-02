using System.Linq;
using AutoEra.Motion;
using AutoEra.Motion.Adapter;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class AutoEraMotionParameterAdapterEditModeTests
    {
        [Test]
        public void Adapter_WritesTypedPresentationParametersWithoutTransformFields()
        {
            var context = new MotionParameterContext();
            var adapter = new AutoEraMotionParameterAdapter(context);
            adapter.ApplyPresentationState(true, 2f, 3, new Pose(new Vector3(1f, 2f, 3f), Quaternion.identity));
            Assert.That(context.TryGetBoolean("interrupted", out bool interrupted) && interrupted, Is.True);
            Assert.That(context.TryGetFloat("normalizedProgress", out float progress), Is.True);
            Assert.That(progress, Is.EqualTo(1f));
            Assert.That(typeof(AutoEraMotionParameterAdapter).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Any(field => typeof(Transform).IsAssignableFrom(field.FieldType) || typeof(MotionRig).IsAssignableFrom(field.FieldType)), Is.False);
        }
    }
}
