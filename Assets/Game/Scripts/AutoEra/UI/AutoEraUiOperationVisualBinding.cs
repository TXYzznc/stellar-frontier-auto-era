using AutoEra.UI.Contracts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>Visual references and state-gated interaction hooks for one authoritative operation source.</summary>
    [System.Serializable]
    public sealed class AutoEraUiOperationVisualBinding
    {
        [SerializeField] private string _sourceId = string.Empty;
        [SerializeField] private GameObject _spinner;
        [SerializeField] private GameObject _progress;
        [SerializeField] private GameObject _longWaitHint;
        [SerializeField] private GameObject _cancelAction;
        [SerializeField] private GameObject _retryAction;
        [SerializeField] private GameObject _detailsAction;
        [SerializeField] private TMP_Text _phaseText;
        [SerializeField] private Image _progressFill;
        [SerializeField] private GameObject _successState;
        [SerializeField] private GameObject _failureState;
        [SerializeField] private GameObject _cancelledState;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _detailsButton;

        private AutoEraUiOperationSnapshot _latestSnapshot;
        private AutoEraUiOperationPresentation _latestPresentation;
        private Action<AutoEraUiOperationActionRequest> _actionHandler;
        private bool _listenersBound;
        private string _successRequestId = string.Empty;

        public string SourceId => _sourceId;

        public void SetActionHandler(Action<AutoEraUiOperationActionRequest> actionHandler)
        {
            _actionHandler = actionHandler;
            EnsureListeners();
        }

        public void Apply(AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation)
        {
            if (snapshot == null)
            {
                return;
            }

            _latestSnapshot = snapshot;
            _latestPresentation = presentation;
            EnsureListeners();

            SetActive(_spinner, ShouldShowSpinner());
            SetActive(_progress, presentation.ShowProgress);
            SetActive(_longWaitHint, presentation.ShowLongWaitHint);
            SetActive(_cancelAction, presentation.ShowCancel);
            SetActive(_retryAction, presentation.ShowRetry);
            SetActive(_detailsAction, presentation.ShowDetails);

            if (_phaseText != null)
            {
                _phaseText.text = presentation.ShowLongWaitHint
                    ? BuildLongWaitText(snapshot.PhaseText)
                    : snapshot.PhaseText;
            }

            if (_progressFill != null)
            {
                _progressFill.fillAmount = presentation.ShowProgress && snapshot.TrustedProgress.HasValue
                    ? snapshot.TrustedProgress.Value
                    : 0f;
            }

            UpdateSuccessState(snapshot);
            SetActive(_failureState, snapshot.Status == AutoEraUiOperationStatus.Failed
                || snapshot.Status == AutoEraUiOperationStatus.RequestLost);
            SetActive(_cancelledState, snapshot.Status == AutoEraUiOperationStatus.Cancelled);
            SetInteractable(_cancelButton, presentation.ShowCancel);
            SetInteractable(_retryButton, presentation.ShowRetry);
            SetInteractable(_detailsButton, presentation.ShowDetails);
        }

        private void EnsureListeners()
        {
            if (_listenersBound)
            {
                return;
            }

            if (_cancelButton != null)
            {
                _cancelButton.onClick.AddListener(RequestCancel);
            }

            if (_retryButton != null)
            {
                _retryButton.onClick.AddListener(RequestRetry);
            }

            if (_detailsButton != null)
            {
                _detailsButton.onClick.AddListener(RequestDetails);
            }

            _listenersBound = true;
        }

        private void RequestCancel()
        {
            if (_latestSnapshot != null && _latestPresentation.ShowCancel)
            {
                RaiseAction(AutoEraUiOperationAction.Cancel);
            }
        }

        private void RequestRetry()
        {
            if (_latestSnapshot != null && _latestPresentation.ShowRetry)
            {
                RaiseAction(AutoEraUiOperationAction.Retry);
            }
        }

        private void RequestDetails()
        {
            if (_latestSnapshot != null && _latestPresentation.ShowDetails)
            {
                RaiseAction(AutoEraUiOperationAction.ViewDetails);
            }
        }

        private void RaiseAction(AutoEraUiOperationAction action)
        {
            _actionHandler?.Invoke(new AutoEraUiOperationActionRequest(_latestSnapshot, action));
        }

        private void UpdateSuccessState(AutoEraUiOperationSnapshot snapshot)
        {
            if (_successState == null)
            {
                return;
            }

            if (snapshot.Status != AutoEraUiOperationStatus.Succeeded)
            {
                _successRequestId = string.Empty;
                SetActive(_successState, false);
                return;
            }

            if (string.Equals(_successRequestId, snapshot.RequestId, StringComparison.Ordinal))
            {
                return;
            }

            _successRequestId = snapshot.RequestId;
            AutoEraUiVisualTimer.ShowFor(_successState, 2f);
        }

        public void RefreshMotionPreference()
        {
            SetActive(_spinner, ShouldShowSpinner());
        }

        private bool ShouldShowSpinner()
        {
            return _latestSnapshot != null && _latestPresentation.ShowSpinner && !AutoEraReduceMotionEntry.IsReducedMotionEnabled;
        }

        private static string BuildLongWaitText(string phaseText)
        {
            const string longWaitText = "仍在处理中／查看详情";
            return string.IsNullOrEmpty(phaseText) ? longWaitText : phaseText + " · " + longWaitText;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void SetInteractable(Button target, bool interactable)
        {
            if (target != null)
            {
                target.interactable = interactable;
            }
        }
    }
}
