using AutoEra.UI.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    [System.Serializable]
    public sealed class AutoEraHubPageBinding
    {
        [SerializeField] private AutoEraHubPage _page;
        [SerializeField] private GameObject _pageRoot;
        [SerializeField] private GameObject _activeNavigationVisual;
        [SerializeField] private TMP_Text _navigationLabel;
        [SerializeField] private Color _activeLabelColor = Color.white;
        [SerializeField] private Color _inactiveLabelColor = Color.white;

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

            if (_navigationLabel != null)
            {
                _navigationLabel.color = active ? _activeLabelColor : _inactiveLabelColor;
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
        [SerializeField] private AutoEraDangerConfirmationView _dangerConfirmationView;
        [SerializeField] private Selectable _defaultFocus;

        private AutoEraHubPageSelection _pageSelection;
        [SerializeField] private TMP_Text _machineRosterSummary;
        private AutoEra.Machines.MachineRoster _machines;
        public void BindMachines(AutoEra.Machines.MachineRoster roster)
        {
            if (_machines != null) { _machines.Changed -= RefreshMachines; _machines.Disposed -= OnMachinesDisposed; }
            _machines = roster;
            if (_machines != null) { _machines.Changed += RefreshMachines; _machines.Disposed += OnMachinesDisposed; }
            RefreshMachines();
        }
        private void OnMachinesDisposed(AutoEra.Machines.MachineRoster roster) => BindMachines(null);
        private void RefreshMachines()
        {
            if (_machineRosterSummary == null) return;
            int total = 0, connected = 0, running = 0;
            if (_machines != null && _machines.IsActive)
                foreach (var machine in _machines.Machines) { total++; if(machine.Connected) connected++; if(machine.CanRun) running++; }
            _machineRosterSummary.richText = false;
            _machineRosterSummary.SetText(_machines == null ? "机器运行数据尚未绑定" : "机器：" + total + " · 已连接：" + connected + " · 可运行：" + running);
        }

        public AutoEraHubPage ActivePage => _pageSelection == null ? _initialPage : _pageSelection.ActivePage;

        protected override void OnAutoEraOpen()
        {
            _pageSelection = new AutoEraHubPageSelection(_initialPage);
            ApplyPageSelection();
            SetRulesImpactVisible(false);
            BindOperationActionHandlers();
            AutoEraReduceMotionEntry.ReducedMotionChanged += HandleReducedMotionChanged;
            if (_defaultFocus != null && _defaultFocus.isActiveAndEnabled && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_defaultFocus.gameObject);
            }
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

        public void ShowDangerConfirmation(AutoEraConfirmationDescriptionResolution description)
        {
            if (_dangerConfirmationView != null)
            {
                _dangerConfirmationView.Show(description);
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

        protected override void OnAutoEraClose(bool isShutdown)
        {
            BindMachines(null);
            AutoEraReduceMotionEntry.ReducedMotionChanged -= HandleReducedMotionChanged;
        }

        protected override void OnAutoEraRecycle()
        {
            BindMachines(null);
            AutoEraReduceMotionEntry.ReducedMotionChanged -= HandleReducedMotionChanged;
        }

        protected override bool OnBeforeFormIntent(AutoEraUiIntent intent)
        {
            return _dangerConfirmationView != null && _dangerConfirmationView.TryHandleIntent(intent);
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

        private void BindOperationActionHandlers()
        {
            if (_operationBindings == null)
            {
                return;
            }

            for (int index = 0; index < _operationBindings.Length; index++)
            {
                if (_operationBindings[index] != null)
                {
                    _operationBindings[index].SetActionHandler(RaiseOperationActionRequest);
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
