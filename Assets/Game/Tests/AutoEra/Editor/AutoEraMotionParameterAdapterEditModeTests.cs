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

        [Test]
        public void Adapter_MapsAuthoritativeSnapshotWithoutGameplayOutcomeDependencies()
        {
            var context = new MotionParameterContext();
            var adapter = new AutoEraMotionParameterAdapter(context);
            var snapshot = new AutoEraMotionPresentationSnapshot(
                false, 0.25f, 2, new Pose(Vector3.one, Quaternion.identity), AutoEraMotionPresentationStatus.Completed);

            adapter.ApplyAuthoritativePresentationSnapshot(snapshot);

            Assert.That(context.TryGetFloat("normalizedProgress", out float progress) && progress == 0.25f, Is.True);
            Assert.That(context.TryGetBoolean("presentationCompleted", out bool completed) && completed, Is.True);
            Assert.That(context.TryGetBoolean("presentationCancelled", out bool cancelled) && !cancelled, Is.True);
            Assert.That(typeof(AutoEraMotionParameterAdapter).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Any(field => typeof(MotionExecutor).IsAssignableFrom(field.FieldType) || typeof(Transform).IsAssignableFrom(field.FieldType)), Is.False);
        }
    }
}
