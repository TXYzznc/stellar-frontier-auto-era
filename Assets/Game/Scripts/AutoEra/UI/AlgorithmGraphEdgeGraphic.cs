using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>算法画布的程序化连线。端点直接取节点端口 RectTransform，不依赖 Edge Item 预制体。</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AlgorithmGraphEdgeGraphic : MaskableGraphic
    {
        private Vector2 _from;
        private Vector2 _to;
        private Color _edgeColor = new Color(0.35f, 0.75f, 0.95f, 0.95f);
        public ulong FromNode { get; private set; }
        public ulong ToNode { get; private set; }
        public string OutputPort { get; private set; }
        public string InputPort { get; private set; }

        public void Bind(ulong fromNode, string outputPort, ulong toNode, string inputPort)
        {
            FromNode = fromNode;
            OutputPort = outputPort;
            ToNode = toNode;
            InputPort = inputPort;
        }

        public void SetPoints(Vector2 from, Vector2 to, bool selected)
        {
            _from = from;
            _to = to;
            _edgeColor = selected
                ? new Color(1f, 0.68f, 0.2f, 1f)
                : new Color(0.35f, 0.75f, 0.95f, 0.95f);

            RectTransform rect = transform as RectTransform;
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(4000f, 4000f);
            }
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Vector2 delta = _to - _from;
            float bend = Mathf.Max(60f, Mathf.Abs(delta.x) * 0.45f);
            Vector2 c1 = _from + new Vector2(bend, 0f);
            Vector2 c2 = _to - new Vector2(bend, 0f);
            const int segments = 24;
            const float thickness = 4f;

            for (int i = 0; i < segments; i++)
            {
                float t0 = i / (float)segments;
                float t1 = (i + 1) / (float)segments;
                Vector2 p0 = Bezier(_from, c1, c2, _to, t0);
                Vector2 p1 = Bezier(_from, c1, c2, _to, t1);
                Vector2 normal = new Vector2(-(p1.y - p0.y), p1.x - p0.x).normalized * (thickness * 0.5f);
                int index = vh.currentVertCount;
                AddVertex(vh, p0 - normal);
                AddVertex(vh, p0 + normal);
                AddVertex(vh, p1 + normal);
                AddVertex(vh, p1 - normal);
                vh.AddTriangle(index, index + 1, index + 2);
                vh.AddTriangle(index, index + 2, index + 3);
            }
        }

        private void AddVertex(VertexHelper vh, Vector2 position)
        {
            vh.AddVert(position, _edgeColor, Vector2.zero);
        }

        private static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float u = 1f - t;
            return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
        }
    }
}
