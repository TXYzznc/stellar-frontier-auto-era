using System;
using AutoEra.Algorithms;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>算法画布节点 Item。节点自身只负责视觉和交互事件，不直接访问算法服务。</summary>
    public sealed class AlgorithmNodeItem : UIItemBase,
        IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private Button _selectButton;
        private Image _selectImage;
        private Outline _selectOutline;
        private Image _headerAccent;
        private Image _sideAccent;
        private TMP_Text _nameLabel;
        private AlgorithmGraphNodeDragHandler _drag;
        private ulong _nodeId;
        private Action<ulong> _selected;
        private Action<ulong, Vector2> _moved;
        private bool _selectedState;
        private bool _pointerOver;
        private bool _pointerDown;
        public ulong NodeId => _nodeId;

        protected override void OnInit()
        {
            _selectButton = transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect")?.GetComponent<Button>();
            _nameLabel = transform.Find("Grp_AlgorithmNode/Btn_AlgorithmNodeSelect/Txt_AlgorithmNodeName")?.GetComponent<TMP_Text>();
            _drag = GetComponent<AlgorithmGraphNodeDragHandler>();
            if (_drag == null) _drag = gameObject.AddComponent<AlgorithmGraphNodeDragHandler>();
            if (_selectButton != null)
            {
                _selectImage = _selectButton.GetComponent<Image>();
                _selectOutline = _selectButton.GetComponent<Outline>();
                if (_selectOutline == null) _selectOutline = _selectButton.gameObject.AddComponent<Outline>();
                _selectOutline.effectDistance = new Vector2(2f, -2f);
                _selectOutline.useGraphicAlpha = true;
                _selectButton.onClick.AddListener(RaiseSelected);
            }

            _headerAccent = CreateAccent("__NodeHeaderAccent", new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -4f), new Vector2(0f, 4f));
            _sideAccent = CreateAccent("__NodeSideAccent", new Vector2(0f, 0f), new Vector2(0f, 1f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(4f, 0f));

            Shadow shadow = GetComponent<Shadow>();
            if (shadow == null) shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.38f);
            shadow.effectDistance = new Vector2(0f, -5f);
            shadow.useGraphicAlpha = true;

            TMP_Text[] portTexts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < portTexts.Length; i++)
            {
                if (portTexts[i] == _nameLabel) continue;
                portTexts[i].fontSize = 16f;
                portTexts[i].enableWordWrapping = false;
                portTexts[i].overflowMode = TextOverflowModes.Ellipsis;
                portTexts[i].color = new Color(0.72f, 0.86f, 0.94f, 1f);
            }

            EnsurePortSocket("Grp_AlgorithmNode/List_AlgorithmInputPorts/Viewport_AlgorithmInputPorts/Content_AlgorithmInputPorts/Item_AlgorithmInputPortTemplate/Btn_AlgorithmInputPort", true);
            EnsurePortSocket("Grp_AlgorithmNode/List_AlgorithmOutputPorts/Viewport_AlgorithmOutputPorts/Content_AlgorithmOutputPorts/Item_AlgorithmOutputPortTemplate/Btn_AlgorithmOutputPort", false);
        }

        public void ApplyPortVisual(Transform row, AlgorithmValueKind kind, bool input, bool connected, bool pending)
        {
            if (row == null) return;
            Transform buttonTransform = row.Find(input ? "Btn_AlgorithmInputPort" : "Btn_AlgorithmOutputPort");
            if (buttonTransform == null) return;

            Button button = buttonTransform.GetComponent<Button>();
            Image image = buttonTransform.GetComponent<Image>();
            Color socketColor = PortColor(kind);
            if (image != null)
            {
                image.color = new Color(socketColor.r * 0.25f, socketColor.g * 0.25f,
                    socketColor.b * 0.28f, pending ? 0.95f : 0.82f);
            }

            TMP_Text label = buttonTransform.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.alignment = input ? TextAlignmentOptions.Left : TextAlignmentOptions.Right;
                label.margin = input ? new Vector4(22f, 0f, 6f, 0f) : new Vector4(6f, 0f, 22f, 0f);
                label.color = new Color(0.78f, 0.88f, 0.94f, 1f);
            }

            AlgorithmPortSocketGraphic socket = buttonTransform.Find("Img_AlgorithmPortSocket")?.GetComponent<AlgorithmPortSocketGraphic>();
            socket?.SetVisual(socketColor, connected, pending);
            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
                colors.pressedColor = new Color(0.82f, 0.9f, 0.96f, 1f);
                button.colors = colors;
            }
        }

        private void EnsurePortSocket(string buttonPath, bool input)
        {
            Transform button = transform.Find(buttonPath);
            if (button == null || button.Find("Img_AlgorithmPortSocket") != null) return;

            GameObject socketObject = new GameObject("Img_AlgorithmPortSocket", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(AlgorithmPortSocketGraphic));
            socketObject.layer = gameObject.layer;
            socketObject.transform.SetParent(button, false);
            RectTransform rect = socketObject.transform as RectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = input ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            rect.anchoredPosition = input ? new Vector2(8f, 0f) : new Vector2(-8f, 0f);
            rect.sizeDelta = new Vector2(20f, 20f);
        }

        private static Color PortColor(AlgorithmValueKind kind)
        {
            switch (kind)
            {
                case AlgorithmValueKind.Event: return new Color(1f, 0.52f, 0.28f, 1f);
                case AlgorithmValueKind.Boolean: return new Color(0.38f, 0.9f, 0.56f, 1f);
                case AlgorithmValueKind.Number: return new Color(0.3f, 0.7f, 1f, 1f);
                case AlgorithmValueKind.Enumeration: return new Color(0.78f, 0.5f, 1f, 1f);
                case AlgorithmValueKind.Object:
                case AlgorithmValueKind.Objects: return new Color(1f, 0.72f, 0.3f, 1f);
                case AlgorithmValueKind.Position: return new Color(0.28f, 0.88f, 0.92f, 1f);
                case AlgorithmValueKind.Communication: return new Color(0.35f, 0.86f, 0.78f, 1f);
                case AlgorithmValueKind.SoilGrid:
                case AlgorithmValueKind.CropGrid:
                case AlgorithmValueKind.TreeGrid: return new Color(0.62f, 0.82f, 0.42f, 1f);
                default: return new Color(0.62f, 0.72f, 0.82f, 1f);
            }
        }

        public void Bind(ulong nodeId, string label, bool selected, Action<ulong> onSelected, Action<ulong, Vector2> onMoved)
        {
            _nodeId = nodeId;
            _selected = onSelected;
            _moved = onMoved;
            _selectedState = selected;
            _pointerOver = false;
            _pointerDown = false;
            if (_nameLabel != null) _nameLabel.SetText(label ?? string.Empty);
            // 对象池 Spawn 会先激活 Button，Unity 的 ColorTint 会短暂写入 prefab
            // 的基准色。绑定首帧必须直接落到目标颜色，避免重绘整张图时所有节点闪蓝。
            ApplyVisualState(false);
            if (_drag != null)
            {
                ulong id = _nodeId;
                _drag.OnMoved = position => _moved?.Invoke(id, position);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _pointerOver = true;
            ApplyVisualState();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _pointerOver = false;
            _pointerDown = false;
            ApplyVisualState();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDown = true;
            ApplyVisualState();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pointerDown = false;
            ApplyVisualState();
        }

        private void ApplyVisualState(bool animate = true)
        {
            Color targetColor = _selectedState
                ? new Color(0.98f, 0.67f, 0.24f, 1f)
                : _pointerOver
                    ? new Color(0.12f, 0.38f, 0.56f, 1f)
                    : new Color(0.08f, 0.20f, 0.32f, 1f);
            if (_selectImage != null)
            {
                if (animate)
                {
                    _selectImage.CrossFadeColor(targetColor, 0.12f, true, true);
                }
                else
                {
                    _selectImage.color = targetColor;
                    _selectImage.canvasRenderer.SetColor(targetColor);
                }
            }

            Color accent = _selectedState
                ? new Color(1f, 0.68f, 0.24f, 1f)
                : _pointerOver
                    ? new Color(0.42f, 0.88f, 1f, 1f)
                    : new Color(0.22f, 0.70f, 0.92f, 0.9f);
            if (_headerAccent != null) _headerAccent.color = accent;
            if (_sideAccent != null) _sideAccent.color = accent;

            if (_selectOutline != null)
            {
                _selectOutline.enabled = _selectedState || _pointerOver;
                _selectOutline.effectColor = _selectedState
                    ? new Color(1f, 0.72f, 0.28f, 0.95f)
                    : new Color(0.52f, 0.86f, 1f, 0.85f);
            }

            RectTransform rect = transform as RectTransform;
            if (rect != null)
            {
                // 节点尺寸是端口布局的几何基准。Shader Graph 的悬停反馈使用描边和颜色，
                // 不通过缩放改变端口位置，避免连线在交互时产生跳动。
                rect.localScale = Vector3.one;
            }
        }

        private Image CreateAccent(string name, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            Transform existing = transform.Find(name);
            GameObject target = existing != null ? existing.gameObject
                : new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            if (existing == null) target.transform.SetParent(transform, false);
            target.transform.SetAsLastSibling();
            RectTransform rect = target.transform as RectTransform;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            Image image = target.GetComponent<Image>();
            image.raycastTarget = false;
            return image;
        }

        private void RaiseSelected() => _selected?.Invoke(_nodeId);
    }

    public sealed class AlgorithmNodeItemObject : UIItemObject
    {
        public AlgorithmNodeItem Logic => itemLogic as AlgorithmNodeItem;
    }
}
