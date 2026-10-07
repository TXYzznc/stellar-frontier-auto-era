using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>曲线网格与命中共享采样点。包围盒只用于剔除，空白处不命中。</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AlgorithmGraphEdgeGraphic : MaskableGraphic, ICanvasRaycastFilter,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
    {
        private const int Segments = 64;
        private readonly Vector2[] _points = new Vector2[Segments + 1];
        private Color _edgeColor = new Color(0.55f, 0.75f, 0.84f, 1f);
        private float _thickness = 2.5f;
        public ulong FromNode { get; private set; }
        public ulong ToNode { get; private set; }
        public string OutputPort { get; private set; }
        public string InputPort { get; private set; }
        public event Action<bool> HoverChanged;
        public event Action Clicked;

        public void Bind(ulong fromNode, string outputPort, ulong toNode, string inputPort)
        {
            FromNode = fromNode;
            OutputPort = outputPort;
            ToNode = toNode;
            InputPort = inputPort;
        }

        public void SetPoints(Vector2 from, Vector2 to, bool selected, bool hovered = false)
        {
            // 水平端口切线；反向边限制手柄长度，避免超长回环。
            Vector2 delta = to - from;
            float bend = Mathf.Clamp(Mathf.Abs(delta.x) * 0.4f + Mathf.Abs(delta.y) * 0.12f, 40f, 180f);
            Vector2 c1 = from + Vector2.right * bend;
            Vector2 c2 = to - Vector2.right * bend;
            Vector2 min = Vector2.Min(from, to);
            Vector2 max = Vector2.Max(from, to);
            for (int i = 0; i <= Segments; i++)
            {
                float t = i / (float)Segments;
                float u = 1f - t;
                Vector2 point = u * u * u * from + 3f * u * u * t * c1
                    + 3f * u * t * t * c2 + t * t * t * to;
                _points[i] = point;
                min = Vector2.Min(min, point);
                max = Vector2.Max(max, point);
            }
            Vector2 center = (min + max) * 0.5f;
            rectTransform.anchorMin = rectTransform.anchorMax = rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = center;
            rectTransform.sizeDelta = max - min + Vector2.one * 64f;
            rectTransform.localScale = Vector3.one;
            for (int i = 0; i <= Segments; i++) _points[i] -= center;
            SetHighlight(selected, hovered);
        }

        public void SetHighlight(bool selected, bool hovered)
        {
            _thickness = selected ? 4f : hovered ? 3.5f : 2.5f;
            _edgeColor = selected ? new Color(1f, 0.72f, 0.28f, 1f)
                : hovered ? new Color(0.8f, 0.93f, 1f, 1f) : new Color(0.55f, 0.75f, 0.84f, 1f);
            SetVerticesDirty();
        }

        public Vector3 GetWorldPoint(float t)
        {
            float sample = Mathf.Clamp01(t) * Segments;
            int i = Mathf.Min((int)sample, Segments - 1);
            return rectTransform.TransformPoint(Vector2.Lerp(_points[i], _points[i + 1], sample - i));
        }

        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!raycastTarget) return false;
            // 以屏幕像素计算命中宽度，缩放画布和 CanvasScaler 不改变可点击性。
            Vector2 previous = RectTransformUtility.WorldToScreenPoint(eventCamera, GetWorldPoint(0f));
            for (int i = 1; i <= Segments; i++)
            {
                Vector2 current = RectTransformUtility.WorldToScreenPoint(eventCamera,
                    rectTransform.TransformPoint(_points[i]));
                Vector2 segment = current - previous;
                float t = segment.sqrMagnitude < 0.0001f ? 0f
                    : Mathf.Clamp01(Vector2.Dot(screenPoint - previous, segment) / segment.sqrMagnitude);
                if ((screenPoint - previous - segment * t).sqrMagnitude <= 64f) return true;
                previous = current;
            }
            return false;
        }

        public void OnPointerEnter(PointerEventData e) => HoverChanged?.Invoke(true);
        public void OnPointerExit(PointerEventData e) => HoverChanged?.Invoke(false);
        // 消费曲线按下，避免事件继续冒泡到画布并启动平移。
        public void OnPointerDown(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) e.Use();
        }
        public void OnPointerUp(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) e.Use();
        }
        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) Clicked?.Invoke();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            // 连续条带共享截面，避免每段独立四边形在急转弯留下缝隙。
            for (int i = 0; i <= Segments; i++)
            {
                Vector2 tangent = _points[Mathf.Min(i + 1, Segments)] - _points[Mathf.Max(0, i - 1)];
                Vector2 normal = new Vector2(-tangent.y, tangent.x).normalized * (_thickness * 0.5f);
                vh.AddVert(_points[i] - normal, _edgeColor, Vector2.zero);
                vh.AddVert(_points[i] + normal, _edgeColor, Vector2.zero);
                if (i == 0) continue;
                int index = i * 2;
                vh.AddTriangle(index - 2, index - 1, index + 1);
                vh.AddTriangle(index - 2, index + 1, index);
            }
        }
    }
}
