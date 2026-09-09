using AutoEra.UI.Contracts;
using UnityEngine;

namespace AutoEra.UI
{
    [System.Serializable]
    public sealed class AutoEraHubPageBinding
    {
        [SerializeField] private AutoEraHubPage _page;
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private GameObject _activeNavigationVisual;

        public AutoEraHubPage Page => _page;

        public void Apply(bool active)
        {
            if (_pageRoot != null)
            {
                _pageRoot.SetActive(active);
            }

            if (_activeNavigationVisual != null)
            {
                _activeNavigationVisual.SetActive(active);
            }
        }
    }

    /// <summary>GF bridge for the V04 command hub. Subpages and impact preview stay inside this one UIForm.</summary>
    public sealed class BaseCommandHubForm : AutoEraUiFormBase
    {
        [SerializeField] private AutoEraHubPage _initialPage = AutoEraHubPage.Overview;
        [SerializeField] private AutoEraHubPageBinding[] _pageBindings;
        [SerializeField] private AutoEraUiOperationVisualBinding[] _operationBindings;
        [SerializeField] private GameObject _rulesImpactOverlay;

        private AutoEraHubPageSelection _pageSelection;

        public AutoEraHubPage ActivePage => _pageSelection == null ? _initialPage : _pageSelection.ActivePage;

        protected override void OnAutoEraOpen()
        {
            _pageSelection = new AutoEraHubPageSelection(_initialPage);
            ApplyPageSelection();
            SetRulesImpactVisible(false);
        }

        public void SelectPage(int page)
        {
            if (page < (int)AutoEraHubPage.Overview || page > (int)AutoEraHubPage.Statistics)
            {
                return;
            }

            if (_pageSelection == null)
            {
                _pageSelection = new AutoEraHubPageSelection(_initialPage);
            }

            _pageSelection.Select((AutoEraHubPage)page);
            ApplyPageSelection();
        }

        public void SetRulesImpactVisible(bool visible)
        {
            if (_rulesImpactOverlay != null)
            {
                _rulesImpactOverlay.SetActive(visible);
            }
        }

        protected override bool OnAutoEraIntent(AutoEraUiIntent intent)
        {
            if (_pageSelection == null)
            {
                return false;
            }

            if (intent == AutoEraUiIntent.NavigatePrevious)
            {
                _pageSelection.Move(-1);
                ApplyPageSelection();
                return true;
            }

            if (intent == AutoEraUiIntent.NavigateNext)
            {
                _pageSelection.Move(1);
                ApplyPageSelection();
                return true;
            }

            return false;
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
                    binding.Apply(presentation);
                }
            }
        }

        private void ApplyPageSelection()
        {
            if (_pageBindings == null)
            {
                return;
            }

            for (int index = 0; index < _pageBindings.Length; index++)
            {
                AutoEraHubPageBinding binding = _pageBindings[index];
                if (binding != null)
                {
                    binding.Apply(binding.Page == ActivePage);
                }
            }
        }
    }
}
