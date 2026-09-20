using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AiFriendlyFrame.Editor.Tests
{
    /// <summary>
    /// GameViewUiQuickSelect 编辑模式几何命中（PickByGraphicGeometry）的回归测试。
    /// 测试对象全部建在 PreviewScene 中，且只把本测试创建的 Canvas 交给拾取逻辑，
    /// 因此既不触碰当前打开的场景，也不受场景内既有 UI 影响。
    /// </summary>
    public sealed class GameViewUiQuickSelectEditModeTests
    {
        private readonly List<Canvas> _canvases = new List<Canvas>();

        private Scene _previewScene;

        [SetUp]
        public void SetUp()
        {
            _canvases.Clear();
            _previewScene = EditorSceneManager.NewPreviewScene();
        }

        [TearDown]
        public void TearDown()
        {
            if (_previewScene.IsValid())
                EditorSceneManager.ClosePreviewScene(_previewScene);

            _canvases.Clear();
        }

        [Test]
        public void PickByGraphicGeometry_ReturnsGraphicUnderScreenPoint()
        {
            RectTransform image = CreateImage(CreateCanvas(), "Img", new Vector2(200f, 200f));

            GameObject picked = Pick(ScreenPointOf(image));

            Assert.That(picked, Is.SameAs(image.gameObject));
        }

        [Test]
        public void PickByGraphicGeometry_PrefersLaterSiblingWhenOverlapping()
        {
            RectTransform canvas = CreateCanvas();
            CreateImage(canvas, "Back", new Vector2(200f, 200f));
            RectTransform front = CreateImage(canvas, "Front", new Vector2(200f, 200f));

            GameObject picked = Pick(ScreenPointOf(front));

            Assert.That(picked, Is.SameAs(front.gameObject));
        }

        [Test]
        public void PickByGraphicGeometry_IgnoresNonRaycastTarget()
        {
            RectTransform image = CreateImage(CreateCanvas(), "NoRaycast", new Vector2(200f, 200f), raycastTarget: false);

            GameObject picked = Pick(ScreenPointOf(image));

            Assert.That(picked, Is.Null);
        }

        [Test]
        public void PickByGraphicGeometry_IgnoresGraphicsBlockedByCanvasGroup()
        {
            RectTransform canvas = CreateCanvas();

            var groupObject = new GameObject("Group", typeof(RectTransform), typeof(CanvasGroup));
            groupObject.transform.SetParent(canvas, false);
            groupObject.GetComponent<CanvasGroup>().blocksRaycasts = false;

            RectTransform image = CreateImage(groupObject.transform, "Img", new Vector2(200f, 200f));

            GameObject picked = Pick(ScreenPointOf(image));

            Assert.That(picked, Is.Null);
        }

        [Test]
        public void PickByGraphicGeometry_ReturnsNullWhenNothingHit()
        {
            RectTransform image = CreateImage(CreateCanvas(), "Img", new Vector2(100f, 100f));
            Vector2 faraway = ScreenPointOf(image) + new Vector2(100000f, 100000f);

            GameObject picked = Pick(faraway);

            Assert.That(picked, Is.Null);
        }

        private GameObject Pick(Vector2 screenPosition)
        {
            return GameViewUiQuickSelect.PickByGraphicGeometry(screenPosition, _canvases.ToArray());
        }

        private RectTransform CreateCanvas()
        {
            var canvasObject = new GameObject("GameViewUiQuickSelectTests_Canvas", typeof(Canvas));
            SceneManager.MoveGameObjectToScene(canvasObject, _previewScene);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvases.Add(canvas);

            return (RectTransform)canvasObject.transform;
        }

        private static RectTransform CreateImage(Transform parent, string name, Vector2 size, bool raycastTarget = true)
        {
            var imageObject = new GameObject(name, typeof(Image));
            imageObject.transform.SetParent(parent, false); // SetParent 会把对象一并移入父对象所在场景

            var image = imageObject.GetComponent<Image>();
            image.raycastTarget = raycastTarget;

            RectTransform rect = image.rectTransform;
            rect.sizeDelta = size;

            return rect;
        }

        /// <summary>
        /// ScreenSpaceOverlay 下（拾取时相机参数为 null），屏幕点与世界坐标 XY 同空间，
        /// 因此可直接用图形的世界位置构造测试点，不依赖 Canvas 的实际布局尺寸。
        /// </summary>
        private static Vector2 ScreenPointOf(RectTransform rect)
        {
            Vector3 world = rect.position;
            return new Vector2(world.x, world.y);
        }
    }
}
