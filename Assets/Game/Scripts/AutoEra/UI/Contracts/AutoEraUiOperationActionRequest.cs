namespace AutoEra.UI.Contracts
{
    public enum AutoEraUiOperationAction
    {
        Cancel,
        Retry,
        ViewDetails
    }

    /// <summary>A UI request is not a result; its owner validates it against current authority before executing.</summary>
    public readonly struct AutoEraUiOperationActionRequest
    {
        public AutoEraUiOperationActionRequest(AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationAction action)
        {
            RequestId = snapshot == null ? string.Empty : snapshot.RequestId;
            SourceId = snapshot == null ? string.Empty : snapshot.SourceId;
            DetailTargetId = snapshot == null ? string.Empty : snapshot.DetailTargetId;
            RetryRequiresConfirmation = snapshot != null && snapshot.RetryRequiresConfirmation;
            Action = action;
        }

        public string RequestId { get; }
        public string SourceId { get; }
        public string DetailTargetId { get; }
        public bool RetryRequiresConfirmation { get; }
        public AutoEraUiOperationAction Action { get; }
    }
}
