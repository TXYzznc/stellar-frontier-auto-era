using System;

namespace AutoEra.UI.Contracts
{
    public enum AutoEraDangerInteractionMode
    {
        ConfirmationDialog,
        HoldToConfirm
    }

    public enum AutoEraConfirmationInitialFocus
    {
        Cancel
    }

    /// <summary>
    /// Immutable result from resolving the description resource bound to one dangerous action.
    /// The UI supplies the localized body; this contract only preserves its safety state.
    /// </summary>
    public sealed class AutoEraConfirmationDescriptionResolution
    {
        public const string MissingConfigurationTitle = "确认描述配置缺失";

        private AutoEraConfirmationDescriptionResolution(string body, string descriptionKey, string missingParameter, bool isResolved)
        {
            Body = body ?? string.Empty;
            DescriptionKey = descriptionKey ?? string.Empty;
            MissingParameter = missingParameter ?? string.Empty;
            IsResolved = isResolved;
        }

        public string Body { get; }
        public string DescriptionKey { get; }
        public string MissingParameter { get; }
        public bool IsResolved { get; }
        public bool IsValid => IsResolved && string.IsNullOrEmpty(DescriptionKey) == false;
        public string ErrorTitle => IsValid ? string.Empty : MissingConfigurationTitle;
        public string ErrorDetail => IsValid ? string.Empty : BuildErrorDetail();

        public static AutoEraConfirmationDescriptionResolution Valid(string descriptionKey, string localizedBody)
        {
            return new AutoEraConfirmationDescriptionResolution(localizedBody, descriptionKey, string.Empty, true);
        }

        public static AutoEraConfirmationDescriptionResolution Missing(string descriptionKey, string missingParameter)
        {
            return new AutoEraConfirmationDescriptionResolution(string.Empty, descriptionKey, missingParameter, false);
        }

        public string BuildConsoleDiagnostic()
        {
            return string.Format("{0}: descriptionKey='{1}', missingParameter='{2}'", MissingConfigurationTitle, DescriptionKey, MissingParameter);
        }

        private string BuildErrorDetail()
        {
            if (string.IsNullOrEmpty(DescriptionKey))
            {
                return MissingConfigurationTitle + "\n描述索引缺失";
            }

            return string.IsNullOrEmpty(MissingParameter)
                ? string.Format("{0}：{1}", MissingConfigurationTitle, DescriptionKey)
                : string.Format("{0}：{1}\n缺失参数：{2}", MissingConfigurationTitle, DescriptionKey, MissingParameter);
        }
    }

    /// <summary>Presentation-independent safety result for the fixed cancel / Danger confirmation layout.</summary>
    public readonly struct AutoEraDangerConfirmationPresentation
    {
        public AutoEraDangerConfirmationPresentation(AutoEraConfirmationDescriptionResolution description)
        {
            Description = description ?? AutoEraConfirmationDescriptionResolution.Missing(string.Empty, string.Empty);
        }

        public AutoEraConfirmationDescriptionResolution Description { get; }
        public bool IsDangerActionEnabled => Description.IsValid;
        public bool IsCancelEnabled => true;
        public AutoEraConfirmationInitialFocus InitialFocus => AutoEraConfirmationInitialFocus.Cancel;
    }

    /// <summary>State-only 1.2-second hold gate. Input routing and ring rendering are supplied by the UI layer.</summary>
    public sealed class AutoEraHoldToConfirmTracker
    {
        public const float RequiredHoldSeconds = 1.2f;

        private float _heldSeconds;

        public bool IsHolding { get; private set; }
        public bool IsCompleted { get; private set; }
        public float Progress => _heldSeconds / RequiredHoldSeconds;

        public void Begin()
        {
            _heldSeconds = 0f;
            IsCompleted = false;
            IsHolding = true;
        }

        /// <summary>Returns true once when the configured hold duration is reached.</summary>
        public bool Advance(float deltaSeconds)
        {
            if (!IsHolding || IsCompleted || deltaSeconds <= 0f)
            {
                return false;
            }

            _heldSeconds = Math.Min(RequiredHoldSeconds, _heldSeconds + deltaSeconds);
            if (_heldSeconds < RequiredHoldSeconds)
            {
                return false;
            }

            IsCompleted = true;
            IsHolding = false;
            return true;
        }

        /// <summary>Release always cancels an incomplete gesture and clears its visible progress.</summary>
        public void Release()
        {
            if (IsCompleted)
            {
                return;
            }

            _heldSeconds = 0f;
            IsHolding = false;
        }
    }
}
