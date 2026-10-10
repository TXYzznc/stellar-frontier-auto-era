using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>算法画布的缩放与左键空白区平移。节点坐标仍由 Form 管理，组件只改变视图状态。</summary>
    public sealed class AlgorithmGraphCanvasInteraction : MonoBehaviour,
        IScrollHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private float _minZoom = 0.25f;
        [SerializeField] private float _maxZoom = 2f;
        [SerializeField] private float _zoomStep = 0.1f;

        private RectTransform _content;
        private RectTransform _viewport;
        private ScrollRect _scroll;
        private bool _panning;

        public float Zoom => _content != null ? _content.localScale.x : 1f;

        private void Awake()
        {
            _viewport = transform as RectTransform;
            _content = _viewport != null ? _viewport.GetComponentInChildren<RectTransform>() : null;
            _scroll = GetComponentInParent<ScrollRect>();
        }

        public void Initialize(RectTransform content)
        {
            _content = content;
            _viewport = transform as RectTransform;
            _scroll = GetComponentInParent<ScrollRect>();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (_content == null || _viewport == null || Mathf.Abs(eventData.scrollDelta.y) < 0.01f)
                return;

            float oldZoom = Zoom;
            float newZoom = Mathf.Clamp(oldZoom + (eventData.scrollDelta.y > 0f ? _zoomStep : -_zoomStep),
                Mathf.Min(_minZoom, oldZoom), _maxZoom);
            if (Mathf.Approximately(oldZoom, newZoom)) return;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _viewport, eventData.position, eventData.pressEventCamera, out Vector2 pointer))
                return;

            Vector2 contentPoint = (pointer - _content.anchoredPosition) / oldZoom;
            _content.localScale = new Vector3(newZoom, newZoom, 1f);
            _content.anchoredPosition = pointer - contentPoint * newZoom;
            eventData.Use();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left
                && eventData.button != PointerEventData.InputButton.Middle) return;
            _panning = true;
            if (_scroll != null) _scroll.enabled = false;
            eventData.Use();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_panning || _content == null) return;
            _content.anchoredPosition += eventData.delta;
            eventData.Use();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left
                && eventData.button != PointerEventData.InputButton.Middle) return;
            _panning = false;
            if (_scroll != null) _scroll.enabled = true;
            eventData.Use();
        }
    }
}
