using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class MotionPreviewEvaluatorEditModeTests
    {
        [Test]
        public void Evaluator_DrivesDeclaredJointAndRestoresBindPose()
        {
            GameObject root = new GameObject("preview-rig");
            try
            {
                MotionRig rig = root.AddComponent<MotionRig>();
                rig.Configure(new[] { new MotionJointBinding("joint", root.transform, MotionJointChannel.Rotation, Vector3.up, -30f, 30f, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero) });
                MotionGraphAsset graph = ScriptableObject.CreateInstance<MotionGraphAsset>();
                graph.Configure(1, "rotate", "1.0.0", System.Array.Empty<MotionParameterDefinition>(), new[] { new MotionNodeDefinition("rotate", MotionNodeKind.Rotate, "joint", string.Empty) }, System.Array.Empty<MotionConnectionDefinition>());

                Assert.That(MotionPreviewEvaluator.Apply(graph, rig, 1f, 0f), Is.True);
                Assert.That(Mathf.Abs(root.transform.localEulerAngles.y - 30f), Is.LessThan(0.01f));
                MotionPreviewEvaluator.RestoreBindPose(rig);
                Assert.That(Quaternion.Angle(root.transform.localRotation, Quaternion.identity), Is.LessThan(0.01f));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void Evaluator_DrivesConveyorPresentationFromTheSamePreviewTimeline()
        {
            GameObject root = new GameObject("preview-conveyor-rig");
            try
            {
                MotionRig rig = root.AddComponent<MotionRig>();
                rig.Configure(new[] { new MotionJointBinding("roller", root.transform, MotionJointChannel.Rotation, Vector3.right, -3600f, 3600f, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero) });
                GameObject belt = new GameObject("belt");
                belt.transform.SetParent(root.transform, false);
                ConveyorLoopRigPreview preview = belt.AddComponent<ConveyorLoopRigPreview>();
                MeshFilter meshFilter = belt.GetComponent<MeshFilter>();
                preview.RebuildForEditor();
                Vector3 initialTreadPosition = meshFilter.sharedMesh.vertices[64 * 4];
                MotionGraphAsset graph = ScriptableObject.CreateInstance<MotionGraphAsset>();
                graph.Configure(1, "conveyor", "1.0.0", System.Array.Empty<MotionParameterDefinition>(), new[] { new MotionNodeDefinition("roller", MotionNodeKind.ContinuousRotate, "roller", string.Empty) }, System.Array.Empty<MotionConnectionDefinition>());

                Assert.That(MotionPreviewEvaluator.Apply(graph, rig, 0.5f, 1f), Is.True);
                Assert.That(meshFilter.sharedMesh.vertices[64 * 4], Is.Not.EqualTo(initialTreadPosition));
                MotionPreviewEvaluator.RestoreBindPose(rig);
                Assert.That(meshFilter.sharedMesh.vertices[64 * 4], Is.EqualTo(initialTreadPosition));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
