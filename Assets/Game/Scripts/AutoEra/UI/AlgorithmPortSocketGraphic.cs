using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>Shader Graph 风格的端口圆点：外圈表达可用状态，内圈表达连接状态。</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AlgorithmPortSocketGraphic : MaskableGraphic
    {
        private const int Segments = 20;
        private Color _color = new Color(0.25f, 0.72f, 0.94f, 1f);
        private bool _connected;
        private bool _pending;

        public void SetVisual(Color color, bool connected, bool pending)
        {
            _color = color;
            _connected = connected;
            _pending = pending;
            raycastTarget = false;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Color halo = _color;
            halo.a = _pending ? 0.55f : _connected ? 0.32f : 0.16f;
            DrawDisc(vh, Vector2.zero, _pending ? 9f : 8f, halo);

            Color core = _connected || _pending ? _color : new Color(_color.r, _color.g, _color.b, 0.28f);
            DrawDisc(vh, Vector2.zero, _pending ? 5.5f : 5f, core);
        }

        private static void DrawDisc(VertexHelper vh, Vector2 center, float radius, Color color)
        {
            int start = vh.currentVertCount;
            vh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i <= Segments; i++)
            {
                float angle = Mathf.PI * 2f * i / Segments;
                Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                vh.AddVert(point, color, Vector2.zero);
            }

            for (int i = 0; i < Segments; i++)
            {
                vh.AddTriangle(start, start + i + 1, start + i + 2);
            }
        }
    }
}
