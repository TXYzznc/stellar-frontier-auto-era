using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法画布节点的拖拽处理器（规格 13-算法编辑器/画布）。
    ///
    /// 挂在画布节点实例上：拖拽时临时禁用父 <see cref="ScrollRect"/>（避免拖拽与滚动打架），
    /// 按 <see cref="PointerEventData.delta"/> 平移自身，松手后把新坐标经
    /// <see cref="OnMoved"/> 回交给工作台写回 <c>MoveNode</c>。工作台负责实例/节点身份，
    /// 本组件只报告坐标，不关心域身份。
    /// </summary>
    public sealed class AlgorithmGraphNodeDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public System.Action<Vector2> OnMoved;
        public System.Action<Vector2> OnDragging;

        private RectTransform _rect;
        private ScrollRect _scroll;
        private bool _dragging;
        private RectTransform _dragParent;
        private Vector2 _pointerOffset;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _scroll = GetComponentInParent<ScrollRect>();
            _dragParent = _rect != null ? _rect.parent as RectTransform : null;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _dragging = true;
            if (_rect != null && _dragParent != null
                && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _dragParent, eventData.position, eventData.pressEventCamera, out Vector2 point))
            {
                _pointerOffset = _rect.anchoredPosition - point;
            }
            if (_scroll != null)
            {
                _scroll.enabled = false;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || _rect == null)
            {
                return;
            }

            if (_dragParent != null
                && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _dragParent, eventData.position, eventData.pressEventCamera, out Vector2 point))
            {
                _rect.anchoredPosition = point + _pointerOffset;
            }
            else
            {
                _rect.anchoredPosition += eventData.delta;
            }
            OnDragging?.Invoke(_rect.anchoredPosition);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            if (_scroll != null)
            {
                _scroll.enabled = true;
            }

            OnMoved?.Invoke(_rect != null ? _rect.anchoredPosition : Vector2.zero);
        }
    }
}
