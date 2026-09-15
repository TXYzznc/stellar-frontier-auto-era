using AutoEra.UI.Contracts;
using UnityEngine;
using AutoEra.World.Region;
using TMPro;
using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>GF bridge for the V04 field HUD. Object panel and critical alert remain child layers.</summary>
    public sealed class FieldHudForm : AutoEraUiFormBase
    {
        [SerializeField] private AutoEraUiOperationVisualBinding[] _operationBindings;
        [SerializeField] private GameObject _regionObjectPanel;
        [SerializeField] private TMP_Text _regionObjectSummary;
        [SerializeField] private TMP_Text _regionTimeSummary;
        [SerializeField] private TMP_Text _regionResourceSummary;
        [SerializeField] private TMP_Text _regionAlertSummary;
        [SerializeField] private GameObject[] _regionAlternativeStates;
        [SerializeField] private MachineHardwarePanel _machinePanel;
        private InitialRegion _region;
        private MachineRoster _machines;
        private PersistentId _lastMachineSelection;
        public bool BlocksWorldInput => _machinePanel != null && _machinePanel.IsOpen;
        private RegionHudPresenter _regionPresenter;
        private bool _fieldAccessible = true;
        private bool _managementOpen;

        public void SetFieldAccess(bool accessible, bool managementOpen)
        {
            if (_fieldAccessible == accessible && _managementOpen == managementOpen) return;
            _fieldAccessible = accessible;
            _managementOpen = managementOpen;
            _machinePanel?.SetAccess(accessible ? ManagementOrigin.Field : ManagementOrigin.Hub);
            _machinePanel?.SetCovered(managementOpen);
            RefreshRegion();
        }

        public void BindMachines(MachineRoster machines)
        { _machines = machines; AutoEraUiRuntime.BindMachineRoster(machines); _lastMachineSelection = PersistentId.Invalid; RefreshRegion(); }
        public bool OpenMachine(PersistentId id, ManagementOrigin origin)
        {
            if (_machinePanel == null || _machines == null || !_machines.TryGet(id, out _)) return false;
            _machinePanel.Open(_machines, id, origin); return true;
        }

        public void BindRegion(InitialRegion region)
        {
            ReleaseRegion();
            if (region == null) return;
            _region = region;
            _regionPresenter = new RegionHudPresenter(region);
            _regionPresenter.Changed += RefreshRegion;
            if (_regionObjectSummary != null) _regionObjectSummary.richText = false;
            if (_regionResourceSummary != null) _regionResourceSummary.SetText("区域对象：{0}", region.Count);
            if (_regionAlertSummary != null) _regionAlertSummary.SetText("区域交互已接入；生产与告警系统未接入");
            if (_regionAlternativeStates != null)
                foreach (GameObject state in _regionAlternativeStates) if (state != null) state.SetActive(false);
            RefreshRegion();
        }

        public void ShowWorldTime(long worldMilliseconds)
        {
            if (_regionTimeSummary != null)
                _regionTimeSummary.SetText("世界时间：{0}秒", worldMilliseconds / 1000L);
        }

        private void RefreshRegion()
        {
            if (_regionResourceSummary != null)
                _regionResourceSummary.SetText("区域对象：{0}", _regionPresenter != null ? _regionPresenter.ObjectCount : 0);
            bool visible = !_managementOpen && _regionPresenter != null && _regionPresenter.HasSelection;
            PersistentId selected = _region != null ? _region.SelectedId : PersistentId.Invalid;
            if (selected != _lastMachineSelection)
            {
                _lastMachineSelection = selected;
                if (_machinePanel != null) _machinePanel.Release();
                if (selected.IsValid && _region != null && _region.TryGetMachine(selected, out _))
                    OpenMachine(selected, _fieldAccessible ? ManagementOrigin.Field : ManagementOrigin.Hub);
            }
            if (_machinePanel != null && _machinePanel.IsOpen) visible = false;
            if (_regionObjectPanel != null) _regionObjectPanel.SetActive(visible);
            if (_regionObjectSummary != null) _regionObjectSummary.SetText(visible ? _regionPresenter.ObjectSummary +
                (_fieldAccessible ? string.Empty : "\n只读：移动镜头至设备附近") : string.Empty);
        }

        private void ReleaseRegion()
        {
            _machinePanel?.Release(); _region = null; _machines = null; _lastMachineSelection = PersistentId.Invalid;
            AutoEraUiRuntime.BindMachineRoster(null);
            _regionPresenter?.Dispose();
            _regionPresenter = null;
            RefreshRegion();
        }

        protected override bool OnBeforeFormIntent(AutoEraUiIntent intent) => _machinePanel != null && _machinePanel.HandleIntent(intent);
        protected override void OnAutoEraCover() => _machinePanel?.SetCovered(true);
        protected override void OnAutoEraPause() => _machinePanel?.SetCovered(true);
        protected override void OnAutoEraResume() => _machinePanel?.SetCovered(false);

        protected override void OnAutoEraOpen()
        {
            AutoEraReduceMotionEntry.ReducedMotionChanged += HandleReducedMotionChanged;
            if (_machinePanel != null) _machinePanel.Closed += OnMachinePanelClosed;
        }
        private void OnMachinePanelClosed() { if (_region != null && _region.SelectedId.IsValid) _region.Select(PersistentId.Invalid, false); }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            if (_machinePanel != null) _machinePanel.Closed -= OnMachinePanelClosed;
            ReleaseRegion();
            AutoEraReduceMotionEntry.ReducedMotionChanged -= HandleReducedMotionChanged;
        }

        protected override void OnAutoEraRecycle()
        {
            if (_machinePanel != null) _machinePanel.Closed -= OnMachinePanelClosed;
            ReleaseRegion();
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
