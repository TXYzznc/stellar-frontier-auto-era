using AutoEra.UI.Contracts;
using UnityEngine;

namespace AutoEra.UI
{
    /// <summary>GF bridge for the V04 field HUD. Object panel and critical alert remain child layers.</summary>
    public sealed class FieldHudForm : AutoEraUiFormBase
    {
        [SerializeField] private AutoEraUiOperationVisualBinding[] _operationBindings;

        protected override void OnAutoEraOpen()
        {
            AutoEraReduceMotionEntry.ReducedMotionChanged += HandleReducedMotionChanged;
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            AutoEraReduceMotionEntry.ReducedMotionChanged -= HandleReducedMotionChanged;
        }

        protected override void OnAutoEraRecycle()
        {
            AutoEraReduceMotionEntry.ReducedMotionChanged -= HandleReducedMotionChanged;
        }

        protected override void OnOperationPresentationChanged(AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation)
        {
            if (_operationBindings == null)
            {
                return;
            }

            for (int index = 0; index < _operationBindings.Length; index++)
            {
                AutoEraUiOperationVisualBinding binding = _operationBindings[index];
                if (binding != null && binding.SourceId == snapshot.SourceId)
                {
                    binding.Apply(snapshot, presentation);
                }
            }
        }

        private void HandleReducedMotionChanged(bool enabled)
        {
            if (_operationBindings == null)
            {
                return;
            }

            for (int index = 0; index < _operationBindings.Length; index++)
            {
                _operationBindings[index]?.RefreshMotionPreference();
            }
        }
    }
}
