using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>Project-level entry for reducing non-essential UI motion while retaining textual state feedback.</summary>
    public sealed class AutoEraReduceMotionEntry : MonoBehaviour
    {
        private const string DisabledLabel = "减弱动态：关";
        private const string EnabledLabel = "减弱动态：开";

        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        public static bool IsReducedMotionEnabled { get; private set; }
        public static event Action<bool> ReducedMotionChanged;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(Toggle);
            }

            RefreshLabel();
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(Toggle);
            }
        }

        public void Toggle()
        {
            IsReducedMotionEnabled = !IsReducedMotionEnabled;
            ReducedMotionChanged?.Invoke(IsReducedMotionEnabled);
            RefreshLabel();
        }

        private void RefreshLabel()
        {
            if (_label != null)
            {
                _label.text = IsReducedMotionEnabled ? EnabledLabel : DisabledLabel;
            }
        }
    }
}
