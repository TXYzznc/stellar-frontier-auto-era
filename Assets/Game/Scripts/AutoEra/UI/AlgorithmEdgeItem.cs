using System;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>算法画布连线 Item。线段与命中按钮独立于节点生命周期。</summary>
    public sealed class AlgorithmEdgeItem : UIItemBase
    {
        private Button _selectButton;
        private ulong _from;
        private string _output;
        private ulong _to;
        private string _input;
        private Action<ulong, string, ulong, string> _selected;
        public ulong FromNode => _from;
        public ulong ToNode => _to;

        protected override void OnInit()
        {
            _selectButton = transform.Find("Grp_AlgorithmEdge/Btn_AlgorithmEdgeSelect")?.GetComponent<Button>();
            if (_selectButton != null) _selectButton.onClick.AddListener(RaiseSelected);
        }

        public void Bind(ulong from, string output, ulong to, string input, bool selected,
            Action<ulong, string, ulong, string> onSelected)
        {
            _from = from;
            _output = output ?? string.Empty;
            _to = to;
            _input = input ?? string.Empty;
            _selected = onSelected;
            if (_selectButton != null)
            {
                Image image = _selectButton.GetComponent<Image>();
                if (image != null) image.color = selected
                    ? new Color(0.98f, 0.67f, 0.24f, 1f)
                    : new Color(0.32f, 0.50f, 0.64f, 1f);
            }
        }

        public void SetGeometry(Vector2 from, Vector2 to)
        {
            RectTransform rect = transform as RectTransform;
            if (rect == null) return;
            Vector2 delta = to - from;
            rect.anchoredPosition = (from + to) * 0.5f;
            rect.sizeDelta = new Vector2(Mathf.Max(16f, delta.magnitude), 4f);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        public void RefreshGeometry(Func<ulong, Vector2?> resolve)
        {
            if (resolve == null) return;
            Vector2? from = resolve(_from);
            Vector2? to = resolve(_to);
            if (from.HasValue && to.HasValue) SetGeometry(from.Value, to.Value);
        }

        private void RaiseSelected() => _selected?.Invoke(_from, _output, _to, _input);
    }

    public sealed class AlgorithmEdgeItemObject : UIItemObject
    {
        public AlgorithmEdgeItem Logic => itemLogic as AlgorithmEdgeItem;
    }
}
