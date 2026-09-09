using System;
using AutoEra.UI.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>Pointer-facing presentation for the fixed 1.2-second dangerous-action hold gate.</summary>
    public sealed class AutoEraHoldToConfirmView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const string HoldingHint = "继续按住以确认";

        [SerializeField] private Image _progressRing;
        [SerializeField] private TMP_Text _hintText;
        [SerializeField] private GameObject _progressRoot;

        private readonly AutoEraHoldToConfirmTracker _tracker = new AutoEraHoldToConfirmTracker();

        public event Action Confirmed;
        public bool IsHolding => _tracker.IsHolding;
        public float Progress => _tracker.Progress;

        private void Awake()
        {
            ResetVisual();
        }

        private void Update()
        {
            if (!_tracker.IsHolding)
            {
                return;
            }

            bool completed = _tracker.Advance(Time.unscaledDeltaTime);
            UpdateVisual();
            if (completed)
            {
                Confirmed?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BeginHold();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelHold();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelHold();
        }

        public void BeginHold()
        {
            _tracker.Begin();
            UpdateVisual();
        }

        public void AdvanceForPreview(float deltaSeconds)
        {
            if (_tracker.Advance(deltaSeconds))
            {
                Confirmed?.Invoke();
            }

            UpdateVisual();
        }

        public void CancelHold()
        {
            _tracker.Release();
            ResetVisual();
        }

        private void UpdateVisual()
        {
            SetActive(_progressRoot, _tracker.IsHolding);
            if (_progressRing != null)
            {
                _progressRing.fillAmount = _tracker.Progress;
            }

            if (_hintText != null)
            {
                _hintText.text = HoldingHint;
            }
        }

        private void ResetVisual()
        {
            SetActive(_progressRoot, false);
            if (_progressRing != null)
            {
                _progressRing.fillAmount = 0f;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
