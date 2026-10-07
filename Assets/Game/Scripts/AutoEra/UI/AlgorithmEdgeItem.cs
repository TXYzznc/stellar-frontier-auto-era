using System;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>边只拥有曲线与身份；所有端点使用画布局部坐标，不缩放 Item。</summary>
    public sealed class AlgorithmEdgeItem : UIItemBase
    {
        private AlgorithmGraphEdgeGraphic _graphic;
        private Action<ulong, string, ulong, string> _selected;
        private bool _selectedState;
        private bool _hovered;
        public ulong FromNode { get; private set; }
        public ulong ToNode { get; private set; }
        public string OutputPort { get; private set; }
        public string InputPort { get; private set; }
        public AlgorithmGraphEdgeGraphic Graphic => _graphic;

        protected override void OnInit()
        {
            _graphic = GetComponentInChildren<AlgorithmGraphEdgeGraphic>(true);
            if (_graphic == null)
            {
                var child = new GameObject("AlgorithmEdgeGraphic", typeof(RectTransform), typeof(CanvasRenderer));
                child.layer = gameObject.layer;
                child.transform.SetParent(transform, false);
                _graphic = child.AddComponent<AlgorithmGraphEdgeGraphic>();
            }
            _graphic.raycastTarget = true;
            _graphic.HoverChanged += SetHovered;
            _graphic.Clicked += RaiseSelected;
            Image placeholder = GetComponent<Image>();
            if (placeholder != null)
            {
                placeholder.color = Color.clear;
                placeholder.raycastTarget = false;
            }
        }

        public void Bind(ulong from, string output, ulong to, string input, bool selected,
            Action<ulong, string, ulong, string> onSelected)
        {
            FromNode = from;
            ToNode = to;
            OutputPort = output ?? string.Empty;
            InputPort = input ?? string.Empty;
            _selected = onSelected;
            _selectedState = selected;
            _hovered = false;
            transform.localScale = Vector3.one;
            _graphic.Bind(from, OutputPort, to, InputPort);
            _graphic.SetHighlight(selected, false);
        }

        public bool ConnectsNode(ulong nodeId) => FromNode == nodeId || ToNode == nodeId;

        public void SetGeometry(Vector2 from, Vector2 to, Vector2 center)
        {
            var rect = (RectTransform)transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = center;
            _graphic.SetPoints(from - center, to - center, _selectedState, _hovered);
        }

        private void SetHovered(bool hovered)
        {
            _hovered = hovered;
            _graphic.SetHighlight(_selectedState, hovered);
        }

        private void RaiseSelected() => _selected?.Invoke(FromNode, OutputPort, ToNode, InputPort);
    }

    public sealed class AlgorithmEdgeItemObject : UIItemObject
    {
        public AlgorithmEdgeItem Logic => itemLogic as AlgorithmEdgeItem;
    }
}
