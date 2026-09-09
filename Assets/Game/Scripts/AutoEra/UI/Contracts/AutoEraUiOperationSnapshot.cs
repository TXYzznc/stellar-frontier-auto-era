using System;

namespace AutoEra.UI.Contracts
{
    public enum AutoEraUiOperationStatus
    {
        Idle,
        InProgress,
        Succeeded,
        Failed,
        Cancelled,
        RequestLost
    }

    /// <summary>
    /// Immutable UI-facing projection of an operation owned by the underlying system.
    /// The UI may present this snapshot, but must never infer a terminal state from elapsed time.
    /// </summary>
    public sealed class AutoEraUiOperationSnapshot
    {
        public AutoEraUiOperationSnapshot(
            string requestId,
            AutoEraUiOperationStatus status,
            string phaseText,
            float? trustedProgress,
            bool canSafelyCancel,
            bool retryRequiresConfirmation,
            string detailTargetId,
            string sourceId)
        {
            RequestId = requestId ?? string.Empty;
            Status = status;
            PhaseText = phaseText ?? string.Empty;
            TrustedProgress = NormalizeProgress(trustedProgress);
            CanSafelyCancel = canSafelyCancel;
            RetryRequiresConfirmation = retryRequiresConfirmation;
            DetailTargetId = detailTargetId ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
        }

        public string RequestId { get; }
        public AutoEraUiOperationStatus Status { get; }
        public string PhaseText { get; }
        public float? TrustedProgress { get; }
        public bool CanSafelyCancel { get; }
        public bool RetryRequiresConfirmation { get; }
        public string DetailTargetId { get; }
        public string SourceId { get; }

        public bool IsTerminal => Status == AutoEraUiOperationStatus.Succeeded
            || Status == AutoEraUiOperationStatus.Failed
            || Status == AutoEraUiOperationStatus.Cancelled
            || Status == AutoEraUiOperationStatus.RequestLost;

        private static float? NormalizeProgress(float? value)
        {
            if (!value.HasValue || float.IsNaN(value.Value) || float.IsInfinity(value.Value))
            {
                return null;
            }

            return value.Value < 0f || value.Value > 1f ? null : value;
        }
    }

    public readonly struct AutoEraUiOperationPresentation
    {
        private AutoEraUiOperationPresentation(
            bool showSpinner,
            bool showProgress,
            bool showLongWaitHint,
            bool showCancel,
            bool showRetry,
            bool retryRequiresConfirmation,
            bool showDetails)
        {
            ShowSpinner = showSpinner;
            ShowProgress = showProgress;
            ShowLongWaitHint = showLongWaitHint;
            ShowCancel = showCancel;
            ShowRetry = showRetry;
            RetryRequiresConfirmation = retryRequiresConfirmation;
            ShowDetails = showDetails;
        }

        public bool ShowSpinner { get; }
        public bool ShowProgress { get; }
        public bool ShowLongWaitHint { get; }
        public bool ShowCancel { get; }
        public bool ShowRetry { get; }
        public bool RetryRequiresConfirmation { get; }
        public bool ShowDetails { get; }

        /// <summary>
        /// longWaitHint is a presentation-only UX signal. It never alters the authoritative status.
        /// </summary>
        public static AutoEraUiOperationPresentation Create(AutoEraUiOperationSnapshot snapshot, bool longWaitHint)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            bool inProgress = snapshot.Status == AutoEraUiOperationStatus.InProgress;
            bool failed = snapshot.Status == AutoEraUiOperationStatus.Failed
                || snapshot.Status == AutoEraUiOperationStatus.RequestLost;
            bool hasDetails = !string.IsNullOrEmpty(snapshot.DetailTargetId);

            return new AutoEraUiOperationPresentation(
                inProgress,
                inProgress && snapshot.TrustedProgress.HasValue,
                inProgress && longWaitHint,
                inProgress && snapshot.CanSafelyCancel,
                failed,
                failed && snapshot.RetryRequiresConfirmation,
                hasDetails);
        }
    }
}
