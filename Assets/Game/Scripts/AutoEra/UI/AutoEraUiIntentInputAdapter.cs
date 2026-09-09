using System;

namespace AutoEra.UI
{
    /// <summary>Device adapters emit semantic intents here; this class never polls a device API.</summary>
    public sealed class AutoEraUiIntentInputAdapter
    {
        private readonly Action<AutoEraUiIntent> _submit;

        public AutoEraUiIntentInputAdapter(Action<AutoEraUiIntent> submit)
        {
            _submit = submit;
        }

        public void Submit(AutoEraUiIntent intent)
        {
            _submit?.Invoke(intent);
        }
    }
}
