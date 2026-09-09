using System.Reflection;
using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class FunctionalRigAcceptanceDemoDirectorEditModeTests
    {
        [Test]
        public void Director_ReceivesAllCurrentMovablePrototypeRoots()
        {
            GameObject root = new GameObject("director-test");
            try
            {
                FunctionalRigAcceptanceDemoDirector director = root.AddComponent<FunctionalRigAcceptanceDemoDirector>();
                Transform[] rigs = new Transform[10];
                for (int index = 0; index < rigs.Length; index++)
                {
                    rigs[index] = new GameObject("rig-" + index).transform;
                    rigs[index].SetParent(root.transform);
                }
                director.Configure(rigs[0], rigs[1], rigs[2], rigs[3], rigs[4], rigs[5], rigs[6], rigs[7], rigs[8], rigs[9]);

                Assert.That(GetTransform(director, "_waterRig"), Is.SameAs(rigs[5]));
                Assert.That(GetTransform(director, "_sawRig"), Is.SameAs(rigs[6]));
                Assert.That(GetTransform(director, "_drillRig"), Is.SameAs(rigs[7]));
                Assert.That(GetTransform(director, "_cargoRig"), Is.SameAs(rigs[8]));
                Assert.That(GetTransform(director, "_fixedRotaryRig"), Is.SameAs(rigs[9]));
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static Transform GetTransform(FunctionalRigAcceptanceDemoDirector director, string fieldName)
        {
            return (Transform)typeof(FunctionalRigAcceptanceDemoDirector).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(director);
        }
    }
}
