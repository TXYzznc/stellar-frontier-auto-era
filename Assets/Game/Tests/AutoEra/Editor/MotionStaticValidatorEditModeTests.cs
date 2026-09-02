using System;
using System.Reflection;
using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionStaticValidatorEditModeTests
    {
        [Test]
        public void Validator_ReportsGraphJointMissingFromRig()
        {
            GameObject root = new GameObject("Rig");
            MotionGraphAsset graph = ScriptableObject.CreateInstance<MotionGraphAsset>();
            try
            {
                MotionRig rig = root.AddComponent<MotionRig>();
                Transform joint = new GameObject("Known").transform; joint.SetParent(root.transform, false);
                rig.Configure(new[] { new MotionJointBinding("known", joint, MotionJointChannel.Rotation, Vector3.up, -1f, 1f, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero) });
                graph.Configure(1, "graph", "1", null, new[] { new MotionNodeDefinition("node", MotionNodeKind.Rotate, "missing", null) }, null);
                string[] errors = (string[])FindValidator().GetMethod("Validate", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { rig, graph });
                Assert.That(errors, Has.Some.Contains("missing rig joint"));
            }
            finally { UnityEngine.Object.DestroyImmediate(graph); UnityEngine.Object.DestroyImmediate(root); }
        }

        private static Type FindValidator()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) { Type type = assembly.GetType("AutoEra.Editor.Motion.MotionStaticValidator", false); if (type != null) return type; }
            Assert.Fail("MotionStaticValidator must be loaded."); return null;
        }
    }
}
