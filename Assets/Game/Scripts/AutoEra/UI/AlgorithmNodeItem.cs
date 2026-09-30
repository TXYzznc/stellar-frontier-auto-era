using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>算法画布节点 Item。节点自身只负责视觉和交互事件，不直接访问算法服务。</summary>
    public sealed class AlgorithmNodeItem : UIItemBase
    {
        private Button _selectButton;
        private TMP_Text _nameLabel;
        private AlgorithmGraphNodeDragHandler _drag;
        private ulong _nodeId;
        private Action<ulong> _selected;
        private Action<ulong, Vector2> _moved;
        public ulong NodeId => _nodeId;

        protected override void OnInit()
        {
            _selectButton = transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect")?.GetComponent<Button>();
            _nameLabel = transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName")?.GetComponent<TMP_Text>();
            _drag = GetComponent<AlgorithmGraphNodeDragHandler>();
            if (_drag == null) _drag = gameObject.AddComponent<AlgorithmGraphNodeDragHandler>();
            if (_selectButton != null) _selectButton.onClick.AddListener(RaiseSelected);
        }

        public void Bind(ulong nodeId, string label, bool selected, Action<ulong> onSelected, Action<ulong, Vector2> onMoved)
        {
            _nodeId = nodeId;
            _selected = onSelected;
            _moved = onMoved;
            if (_nameLabel != null) _nameLabel.SetText(label ?? string.Empty);
            Debug.Log("[AutoEra][AlgorithmNodeItem] 绑定节点：" + _nodeId + "，名称=" + (label ?? "<null>")
                + "，文本节点=" + (_nameLabel != null));
            if (_selectButton != null)
            {
                Image image = _selectButton.GetComponent<Image>();
                if (image != null) image.color = selected
                    ? new Color(0.98f, 0.67f, 0.24f, 1f)
                    : new Color(0.32f, 0.50f, 0.64f, 1f);
            }
            if (_drag != null)
            {
                ulong id = _nodeId;
                _drag.OnMoved = position => _moved?.Invoke(id, position);
            }
        }

        private void RaiseSelected() => _selected?.Invoke(_nodeId);
    }

    public sealed class AlgorithmNodeItemObject : UIItemObject
    {
        public AlgorithmNodeItem Logic => itemLogic as AlgorithmNodeItem;
    }
}
