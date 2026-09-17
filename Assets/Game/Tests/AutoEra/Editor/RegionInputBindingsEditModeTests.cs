using System.Collections.Generic;
using AutoEra.Input;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class RegionInputBindingsEditModeTests
    {
        [Test]
        public void DefaultManifest_CoversEverySemanticActionWithExpectedDefaults()
        {
            RegionInputBindingSet bindings = RegionInputBindingSet.CreateDefault();
            Assert.That(bindings.Bindings.Count, Is.EqualTo(9));
            Assert.That(bindings.TryGet(RegionInputAction.PanForward, out var panForward), Is.True);
            Assert.That(panForward.Key, Is.EqualTo(KeyCode.W));
            Assert.That(panForward.Trigger, Is.EqualTo(RegionInputTrigger.Hold));
            Assert.That(bindings.TryGet(RegionInputAction.PanBack, out var panBack), Is.True);
            Assert.That(panBack.Key, Is.EqualTo(KeyCode.S));
            Assert.That(bindings.TryGet(RegionInputAction.PanLeft, out var panLeft), Is.True);
            Assert.That(panLeft.Key, Is.EqualTo(KeyCode.A));
            Assert.That(bindings.TryGet(RegionInputAction.PanRight, out var panRight), Is.True);
            Assert.That(panRight.Key, Is.EqualTo(KeyCode.D));
            Assert.That(bindings.TryGet(RegionInputAction.Orbit, out var orbit), Is.True);
            Assert.That(orbit.Key, Is.EqualTo(KeyCode.Mouse1));
            Assert.That(orbit.Trigger, Is.EqualTo(RegionInputTrigger.Hold));
            Assert.That(bindings.TryGet(RegionInputAction.Select, out var select), Is.True);
            Assert.That(select.Key, Is.EqualTo(KeyCode.Mouse0));
            Assert.That(select.Trigger, Is.EqualTo(RegionInputTrigger.Press));
            Assert.That(bindings.TryGet(RegionInputAction.Focus, out var focus), Is.True);
            Assert.That(focus.Key, Is.EqualTo(KeyCode.F));
            Assert.That(focus.Trigger, Is.EqualTo(RegionInputTrigger.Press));
            Assert.That(bindings.TryGet(RegionInputAction.Rotate, out var rotate), Is.True);
            Assert.That(rotate.Key, Is.EqualTo(KeyCode.R));
            Assert.That(bindings.TryGet(RegionInputAction.Cancel, out var cancel), Is.True);
            Assert.That(cancel.Key, Is.EqualTo(KeyCode.Escape));
            Assert.That(cancel.Trigger, Is.EqualTo(RegionInputTrigger.Press));
        }

        [Test]
        public void DefaultManifest_BindsDistinctKeys()
        {
            RegionInputBindingSet bindings = RegionInputBindingSet.CreateDefault();
            var keys = new HashSet<KeyCode>();
            foreach (RegionInputBinding binding in bindings.Bindings)
                Assert.That(keys.Add(binding.Key), Is.True, $"Key {binding.Key} bound twice.");
        }

        [Test]
        public void Create_RejectsDuplicateActionAndDuplicateKey()
        {
            Assert.That(() => RegionInputBindingSet.Create(new[]
                {
                    new RegionInputBinding(RegionInputAction.Focus, RegionInputTrigger.Press, KeyCode.F),
                    new RegionInputBinding(RegionInputAction.Focus, RegionInputTrigger.Press, KeyCode.G)
                }), Throws.TypeOf<System.ArgumentException>());
            Assert.That(() => RegionInputBindingSet.Create(new[]
                {
                    new RegionInputBinding(RegionInputAction.Focus, RegionInputTrigger.Press, KeyCode.F),
                    new RegionInputBinding(RegionInputAction.Rotate, RegionInputTrigger.Press, KeyCode.F)
                }), Throws.TypeOf<System.ArgumentException>());
        }

        [Test]
        public void PartialSet_TryGetMissingAction_ReturnsFalse()
        {
            RegionInputBindingSet bindings = RegionInputBindingSet.Create(new[]
                { new RegionInputBinding(RegionInputAction.Focus, RegionInputTrigger.Press, KeyCode.F) });
            Assert.That(bindings.TryGet(RegionInputAction.Focus, out _), Is.True);
            Assert.That(bindings.TryGet(RegionInputAction.Cancel, out _), Is.False);
        }

        [Test]
        public void Module_ExposesDefaultManifestWithoutAwake()
        {
            var go = new GameObject("RegionInputModuleFixture");
            try
            {
                var module = go.AddComponent<RegionInputModule>();
                Assert.That(module.Bindings, Is.Not.Null);
                Assert.That(module.Bindings.Bindings.Count, Is.EqualTo(9));
                Assert.That(module.Bindings.TryGet(RegionInputAction.Cancel, out var cancel), Is.True);
                Assert.That(cancel.Key, Is.EqualTo(KeyCode.Escape));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }
}