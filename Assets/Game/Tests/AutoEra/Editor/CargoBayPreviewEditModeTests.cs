using AutoEra.Motion;
using NUnit.Framework;
using UnityEngine;

namespace AutoEra.Tests.Editor
{
    public sealed class CargoBayPreviewEditModeTests
    {
        [Test]
        public void FillPreview_UsesSingleNormalizedContainerStateForDoorsAndLoadVisual()
        {
            GameObject root = new GameObject("cargo");
            try
            {
                Transform left = Child(root.transform, "left", new Vector3(-0.72f, 0f, 1.45f));
                Transform right = Child(root.transform, "right", new Vector3(0.72f, 0f, 1.45f));
                Transform tray = Child(root.transform, "tray", new Vector3(0f, -0.42f, 0.15f));
                Transform load = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                load.SetParent(tray, false);
                CargoBayPreview preview = root.AddComponent<CargoBayPreview>();
                preview.Configure(left, right, tray, load);
                preview.ApplyFill(1f, true);

                Assert.That(left.localPosition.x, Is.EqualTo(-1.17f).Within(0.001f));
                Assert.That(right.localPosition.x, Is.EqualTo(1.17f).Within(0.001f));
                Assert.That(tray.localPosition.z, Is.EqualTo(0.9f).Within(0.001f));
                Assert.That(load.localScale.y, Is.EqualTo(1.45f).Within(0.001f));

                preview.ApplyTransfer(0.5f, 0f);
                Assert.That(left.localPosition, Is.EqualTo(new Vector3(-0.72f, 0f, 1.45f)));
                Assert.That(right.localPosition, Is.EqualTo(new Vector3(0.72f, 0f, 1.45f)));
                Assert.That(tray.localPosition, Is.EqualTo(new Vector3(0f, -0.42f, 0.15f)));
            }
            finally { Object.DestroyImmediate(root); }
        }

        private static Transform Child(Transform parent, string name, Vector3 localPosition)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            return child;
        }
    }
}
