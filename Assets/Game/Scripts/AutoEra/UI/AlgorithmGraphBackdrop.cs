using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法画布的低对比度空间网格。它只绘制背景，不参与命中测试，
    /// 让节点位置、拖拽方向和缩放比例在第一眼就有空间参照。
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AlgorithmGraphBackdrop : MaskableGraphic
    {
        [SerializeField] private float _gridSize = 32f;
        [SerializeField] private int _majorEvery = 4;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Rect rect = rectTransform.rect;
            AddQuad(vh, rect.min, rect.max, new Color(0.018f, 0.035f, 0.060f, 0.92f));

            float size = Mathf.Max(8f, _gridSize);
            int majorEvery = Mathf.Max(1, _majorEvery);
            int column = 0;
            for (float x = Mathf.Ceil(rect.xMin / size) * size; x <= rect.xMax; x += size, column++)
            {
                bool major = column % majorEvery == 0;
                Color line = major
                    ? new Color(0.18f, 0.42f, 0.58f, 0.18f)
                    : new Color(0.16f, 0.35f, 0.48f, 0.075f);
                AddLine(vh, new Vector2(x, rect.yMin), new Vector2(x, rect.yMax), major ? 1.2f : 0.7f, line);
            }

            int row = 0;
            for (float y = Mathf.Ceil(rect.yMin / size) * size; y <= rect.yMax; y += size, row++)
            {
                bool major = row % majorEvery == 0;
                Color line = major
                    ? new Color(0.18f, 0.42f, 0.58f, 0.18f)
                    : new Color(0.16f, 0.35f, 0.48f, 0.075f);
                AddLine(vh, new Vector2(rect.xMin, y), new Vector2(rect.xMax, y), major ? 1.2f : 0.7f, line);
            }

            // 中央基准轴：在拖动、缩放和定位节点时提供一个稳定的视觉锚点。
            AddLine(vh, new Vector2(0f, rect.yMin), new Vector2(0f, rect.yMax), 1.5f,
                new Color(0.35f, 0.78f, 0.95f, 0.22f));
            AddLine(vh, new Vector2(rect.xMin, 0f), new Vector2(rect.xMax, 0f), 1.5f,
                new Color(0.35f, 0.78f, 0.95f, 0.22f));

            float bracket = Mathf.Min(28f, Mathf.Min(rect.width, rect.height) * 0.12f);
            Color bracketColor = new Color(0.45f, 0.86f, 1f, 0.32f);
            AddLine(vh, new Vector2(rect.xMin + 12f, rect.yMax - 12f),
                new Vector2(rect.xMin + 12f + bracket, rect.yMax - 12f), 2f, bracketColor);
            AddLine(vh, new Vector2(rect.xMin + 12f, rect.yMax - 12f),
                new Vector2(rect.xMin + 12f, rect.yMax - 12f - bracket), 2f, bracketColor);
            AddLine(vh, new Vector2(rect.xMax - 12f, rect.yMin + 12f),
                new Vector2(rect.xMax - 12f - bracket, rect.yMin + 12f), 2f, bracketColor);
            AddLine(vh, new Vector2(rect.xMax - 12f, rect.yMin + 12f),
                new Vector2(rect.xMax - 12f, rect.yMin + 12f + bracket), 2f, bracketColor);
        }

        private static void AddQuad(VertexHelper vh, Vector2 min, Vector2 max, Color color)
        {
            int index = vh.currentVertCount;
            vh.AddVert(new Vector3(min.x, min.y), color, Vector2.zero);
            vh.AddVert(new Vector3(min.x, max.y), color, Vector2.zero);
            vh.AddVert(new Vector3(max.x, max.y), color, Vector2.zero);
            vh.AddVert(new Vector3(max.x, min.y), color, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }

        private static void AddLine(VertexHelper vh, Vector2 from, Vector2 to, float thickness, Color color)
        {
            Vector2 direction = to - from;
            if (direction.sqrMagnitude < 0.001f) return;
            Vector2 normal = new Vector2(-direction.y, direction.x).normalized * (thickness * 0.5f);
            int index = vh.currentVertCount;
            vh.AddVert(from - normal, color, Vector2.zero);
            vh.AddVert(from + normal, color, Vector2.zero);
            vh.AddVert(to + normal, color, Vector2.zero);
            vh.AddVert(to - normal, color, Vector2.zero);
            vh.AddTriangle(index, index + 1, index + 2);
            vh.AddTriangle(index, index + 2, index + 3);
        }
    }
}
