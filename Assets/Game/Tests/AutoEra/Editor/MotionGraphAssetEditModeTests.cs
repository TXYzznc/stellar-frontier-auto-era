using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionGraphAssetEditModeTests
    {
        [Test]
        public void Graph_AcceptsVersionedStableTypedDefinitions()
        {
            MotionGraphAsset graph = ScriptableObject.CreateInstance<MotionGraphAsset>();
            try
            {
                graph.Configure(1, "carrier_drive", "1.0.0",
                    new[] { new MotionParameterDefinition("speed", MotionParameterType.Float) },
                    new[] { new MotionNodeDefinition("rotate_wheel", MotionNodeKind.ContinuousRotate, "wheel_fl", "speed"), new MotionNodeDefinition("wait", MotionNodeKind.Wait, null, null) },
                    new[] { new MotionConnectionDefinition("rotate_wheel", "wait") });
                Assert.That(graph.TryValidate(out string error), Is.True, error);
            }
            finally { Object.DestroyImmediate(graph); }
        }

        [Test]
        public void Graph_RejectsDuplicateNodesAndUnknownConnections()
        {
            MotionGraphAsset graph = ScriptableObject.CreateInstance<MotionGraphAsset>();
            try
            {
                graph.Configure(1, "carrier_drive", "1.0.0", null,
                    new[] { new MotionNodeDefinition("node", MotionNodeKind.Wait, null, null), new MotionNodeDefinition("node", MotionNodeKind.Wait, null, null) },
                    new[] { new MotionConnectionDefinition("node", "unknown") });
                Assert.That(graph.TryValidate(out string error), Is.False);
                Assert.That(error, Does.Contain("duplicate node ID"));
            }
            finally { Object.DestroyImmediate(graph); }
        }

        [Test]
        public void Graph_RequiresItsDeclaredContractFamilyWhenPreviewing()
        {
            GameObject root = new GameObject("rig");
            MotionGraphAsset graph = ScriptableObject.CreateInstance<MotionGraphAsset>();
            try
            {
                MotionRig rig = root.AddComponent<MotionRig>();
                rig.Configure(new[] { new MotionJointBinding("joint", root.transform, MotionJointChannel.Rotation, Vector3.up, -1f, 1f, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero) }, "water_sprayer_prototype");
                graph.Configure(1, "saw_cut", "1.0.0", null, new[] { new MotionNodeDefinition("joint", MotionNodeKind.Rotate, "joint", null) }, null, "rotary_saw_prototype");

                Assert.That(graph.IsCompatibleWith(rig), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(graph);
                Object.DestroyImmediate(root);
            }
        }
    }
}
