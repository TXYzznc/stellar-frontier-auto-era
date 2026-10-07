using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// AlgorithmEditorForm 的视觉层：集中管理颜色层级、面板强调线、按钮状态和进入动效，
    /// 避免 Form 逻辑里散落大量只服务于表现的对象操作。
    /// </summary>
    public sealed class AlgorithmEditorVisualStyle : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.055f, 0.078f, 0.115f, 0.98f);
        private static readonly Color CanvasColor = new Color(0.032f, 0.055f, 0.085f, 1f);
        private static readonly Color InspectorColor = new Color(0.060f, 0.073f, 0.120f, 0.98f);
        private static readonly Color AccentCyan = new Color(0.32f, 0.82f, 1f, 0.92f);
        private static readonly Color AccentAmber = new Color(1f, 0.67f, 0.22f, 0.98f);
        private static readonly Color AccentRed = new Color(1f, 0.31f, 0.32f, 0.98f);
        private static readonly Color AccentViolet = new Color(0.67f, 0.52f, 1f, 0.98f);

        private Transform _root;
        private Coroutine _entrance;

        public void Apply(Transform root)
        {
            _root = root;
            ApplyPanel("Panel_AlgorithmEditorNodes", PanelColor, AccentCyan);
            ApplyPanel("Panel_AlgorithmEditorCanvas", CanvasColor, AccentCyan);
            ApplyPanel("Panel_AlgorithmEditorInspector", InspectorColor, AccentViolet);
            ApplyPanel("Panel_AlgorithmEditorProblems", new Color(0.105f, 0.065f, 0.090f, 0.98f), AccentAmber);
            ApplyButtons(root);
        }

        public void EnsureGraphBackdrop(RectTransform viewport)
        {
            if (viewport == null) return;
            Transform existing = viewport.Find("AlgorithmGraphBackdrop");
            if (existing == null)
            {
                GameObject go = new GameObject("AlgorithmGraphBackdrop", typeof(RectTransform), typeof(CanvasRenderer));
                go.transform.SetParent(viewport, false);
                existing = go.transform;
            }

            RectTransform rect = existing as RectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsFirstSibling();
            AlgorithmGraphBackdrop backdrop = existing.GetComponent<AlgorithmGraphBackdrop>();
            if (backdrop == null) backdrop = existing.gameObject.AddComponent<AlgorithmGraphBackdrop>();
            backdrop.raycastTarget = false;

            Transform hintTransform = viewport.Find("AlgorithmGraphInteractionHint");
            if (hintTransform == null)
            {
                GameObject hintObject = new GameObject("AlgorithmGraphInteractionHint",
                    typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                hintObject.transform.SetParent(viewport, false);
                hintTransform = hintObject.transform;
            }

            RectTransform hintRect = hintTransform as RectTransform;
            hintRect.anchorMin = new Vector2(1f, 0f);
            hintRect.anchorMax = new Vector2(1f, 0f);
            hintRect.pivot = new Vector2(1f, 0f);
            hintRect.anchoredPosition = new Vector2(-16f, 12f);
            hintRect.sizeDelta = new Vector2(360f, 24f);
            TMP_Text hint = hintTransform.GetComponent<TMP_Text>();
            hint.text = "滚轮 缩放   ·   中键 / 空白区 拖拽";
            hint.fontSize = 14f;
            hint.alignment = TextAlignmentOptions.BottomRight;
            hint.color = new Color(0.56f, 0.78f, 0.86f, 0.68f);
            hint.raycastTarget = false;
            TMP_Text source = _root != null ? _root.GetComponentInChildren<TMP_Text>(true) : null;
            if (source != null) hint.font = source.font;
            hintTransform.SetAsLastSibling();
        }

        public void PlayEntrance(GameObject page)
        {
            if (page == null) return;
            if (_entrance != null) StopCoroutine(_entrance);
            _entrance = StartCoroutine(EntranceRoutine(page.transform));
        }

        private void ApplyPanel(string name, Color color, Color accent)
        {
            Transform panel = FindChild(_root, name);
            if (panel == null) return;
            Image image = panel.GetComponent<Image>();
            if (image != null) image.color = color;

            Outline outline = panel.GetComponent<Outline>();
            if (outline == null) outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.42f);
            outline.effectDistance = new Vector2(1f, -1f);
            outline.useGraphicAlpha = true;

            Shadow shadow = panel.GetComponent<Shadow>();
            if (shadow == null) shadow = panel.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.34f);
            shadow.effectDistance = new Vector2(0f, -6f);
            shadow.useGraphicAlpha = true;

            Transform accentTransform = panel.Find("__VisualAccent");
            if (accentTransform == null)
            {
                GameObject accentObject = new GameObject("__VisualAccent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                accentObject.transform.SetParent(panel, false);
                accentTransform = accentObject.transform;
            }

            RectTransform accentRect = accentTransform as RectTransform;
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.anchoredPosition = Vector2.zero;
            accentRect.sizeDelta = new Vector2(3f, 0f);
            Image accentImage = accentTransform.GetComponent<Image>();
            accentImage.color = accent;
            accentImage.raycastTarget = false;
        }

        private static void ApplyButtons(Transform root)
        {
            if (root == null) return;
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null) continue;
                Color accent = ResolveButtonAccent(button.name);
                ColorBlock colors = button.colors;
                colors.normalColor = new Color(accent.r, accent.g, accent.b, 0.82f);
                colors.highlightedColor = new Color(Mathf.Min(1f, accent.r + 0.16f), Mathf.Min(1f, accent.g + 0.16f), Mathf.Min(1f, accent.b + 0.16f), 1f);
                colors.pressedColor = new Color(accent.r * 0.72f, accent.g * 0.72f, accent.b * 0.72f, 1f);
                colors.selectedColor = colors.highlightedColor;
                colors.disabledColor = new Color(0.28f, 0.32f, 0.38f, 0.42f);
                colors.fadeDuration = 0.14f;
                button.colors = colors;

                Shadow shadow = button.GetComponent<Shadow>();
                if (shadow == null) shadow = button.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(accent.r, accent.g, accent.b, 0.18f);
                shadow.effectDistance = new Vector2(0f, -2f);
                shadow.useGraphicAlpha = true;
            }
        }

        private static Color ResolveButtonAccent(string name)
        {
            if (name.IndexOf("Delete", System.StringComparison.OrdinalIgnoreCase) >= 0) return AccentRed;
            if (name.IndexOf("Apply", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Add", System.StringComparison.OrdinalIgnoreCase) >= 0) return AccentAmber;
            if (name.IndexOf("Diagnos", System.StringComparison.OrdinalIgnoreCase) >= 0) return AccentViolet;
            return AccentCyan;
        }

        private IEnumerator EntranceRoutine(Transform page)
        {
            CanvasGroup group = page.GetComponent<CanvasGroup>();
            if (group == null) group = page.gameObject.AddComponent<CanvasGroup>();
            Vector3 target = page.localScale;
            Vector3 start = target * 0.985f;
            page.localScale = start;
            group.alpha = 0f;
            const float duration = 0.24f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                page.localScale = Vector3.LerpUnclamped(start, target, eased);
                group.alpha = Mathf.SmoothStep(0f, 1f, t);
                yield return null;
            }
            page.localScale = target;
            group.alpha = 1f;
            _entrance = null;
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChild(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
