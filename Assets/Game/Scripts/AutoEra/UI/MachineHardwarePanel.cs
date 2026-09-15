using System;
using System.Collections.Generic;
using AutoEra.Machines;
using AutoEra.World.Identity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>Pre-authored child layer owned by FieldHudForm. No device reads or polling of machine data.</summary>
    public sealed class MachineHardwarePanel : MonoBehaviour
    {
        [SerializeField] private Transform[] _nodes;
        [SerializeField] private Sprite _tabNormal, _tabSelected;
        [SerializeField] private Color _tabNormalText, _tabSelectedText;
        [SerializeField] private Sprite _rowNormal, _rowSelected, _rowDisabled;
        [SerializeField] private TMP_InputField _renameInput;
        private readonly Dictionary<string, Transform> _lookup = new Dictionary<string, Transform>();
        private readonly List<GameObject> _rows = new List<GameObject>();
        private MachineHardwarePresenter _presenter;
        private GameObject _trigger;
        private GameObject _layerTrigger;
        private bool _initialized;
        private bool _covered;
        private MachinePanelLayer _lastLayer;
        public MachineHardwarePresenter Presenter => _presenter;
        public bool IsOpen => _presenter != null && gameObject.activeSelf;
        public event Action Closed;

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            foreach (Transform node in _nodes) if (node != null) _lookup.Add(node.name, node);
            Bind("B11_TabOverview", () => _presenter?.ShowPage(false));
            Bind("B11_TabHardware", () => _presenter?.ShowPage(true));
            Bind("B11_OpenHardware", () => _presenter?.ShowPage(true));
            Bind("B11_Run", () => _presenter?.ToggleRun());
            Bind("B11_Rename", Rename);
            Bind("B11_Close", Close);
            Bind("B11_Sensor1_Select", () => Select(HardwareKind.Sensor, 0));
            Bind("B11_Sensor2_Choose", () => Select(HardwareKind.Sensor, 1));
            Bind("B11_Sensor2", () => Select(HardwareKind.Sensor, 1));
            Bind("B11_Core1_Select", () => Select(HardwareKind.Core, 0));
            Bind("B11_Effector1_Select", () => Select(HardwareKind.Effector, 0));
            Bind("B11_Effector2_Choose", () => Select(HardwareKind.Effector, 1));
            Bind("B11_Effector2", () => Select(HardwareKind.Effector, 1));
            Bind("B11_Sensor1_Enable", () => _presenter?.ToggleComponent(HardwareKind.Sensor, 0));
            Bind("B11_Sensor2_Enable", () => _presenter?.ToggleComponent(HardwareKind.Sensor, 1));
            Bind("B11_Effector1_Enable", () => _presenter?.ToggleComponent(HardwareKind.Effector, 0));
            Bind("B11_Effector2_Enable", () => _presenter?.ToggleComponent(HardwareKind.Effector, 1));
            Bind("B11_Remove", () => _presenter?.Preview(true));
            Bind("B11_PickerInstall", () => _presenter?.Preview(false));
            Bind("B11_PickerBack", Back);
            Bind("B11_FeedbackConfirmCancel", Back);
            Bind("B11_FeedbackConfirmAccept", () => _presenter?.Confirm());
            Bind("B11_FeedbackWaitingDismiss", () => _presenter?.CancelWaiting());
            foreach (string suffix in new[] { "Success", "Rejected", "LoadingState", "ErrorState", "ObjectInvalidState" })
                Bind("B11_Feedback" + suffix + "Dismiss", Back);
            if (_renameInput != null) _renameInput.gameObject.SetActive(false);
        }
        private void Bind(string key, UnityEngine.Events.UnityAction action)
        {
            Button button = Node<Button>(key);
            if (button == null) throw new InvalidOperationException("Missing machine UI button: " + key);
            button.onClick.AddListener(action);
        }
        public void Open(MachineRoster roster, PersistentId id, ManagementOrigin origin)
        {
            Close(false); Initialize();
            _trigger = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            _presenter = new MachineHardwarePresenter(roster, id, origin);
            _presenter.Changed += Render;
            _lastLayer = MachinePanelLayer.Invalid;
            gameObject.SetActive(true); Render(); Focus("B11_TabOverview");
        }
        public void SetAccess(ManagementOrigin origin) { _presenter?.SetOrigin(origin); }
        public void SetCovered(bool covered)
        {
            _covered = covered;
            if (TryGetComponent<CanvasGroup>(out var group)) { group.interactable = !covered; group.blocksRaycasts = !covered; }
            if (!covered) Render();
        }
        public bool HandleIntent(AutoEraUiIntent intent)
        {
            if (!IsOpen || _covered) return false;
            if (intent == AutoEraUiIntent.Cancel)
            {
                if (_renameInput != null && _renameInput.gameObject.activeSelf) { _renameInput.gameObject.SetActive(false); Focus("B11_Rename"); }
                else if (_presenter.Layer == MachinePanelLayer.Overview || _presenter.Layer == MachinePanelLayer.Hardware || _presenter.Layer == MachinePanelLayer.Invalid) Close();
                else Back();
                return true;
            }
            if (intent == AutoEraUiIntent.Confirm)
            {
                var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
                if (selected != null && selected.transform.IsChildOf(transform))
                { var button = selected.GetComponent<Button>(); if (button != null && button.IsInteractable()) button.onClick.Invoke(); }
                return true;
            }
            if (_presenter.Layer == MachinePanelLayer.Overview || _presenter.Layer == MachinePanelLayer.Hardware)
                _presenter.ShowPage(intent == AutoEraUiIntent.NavigateNext);
            return true; // A visible child layer always consumes navigation before the world beneath it.
        }
        public void Close() => Close(true);
        public void Release() => Close(false);
        private void Close(bool notify)
        {
            if (_presenter != null) { _presenter.Changed -= Render; _presenter.Dispose(); _presenter = null; }
            foreach (var row in _rows) if (row != null) row.SetActive(false);
            if (_renameInput != null) _renameInput.gameObject.SetActive(false);
            gameObject.SetActive(false);
            if (_trigger != null && _trigger.activeInHierarchy && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_trigger);
            _trigger = null; _layerTrigger = null;
            if (notify) Closed?.Invoke();
        }
        private void Select(HardwareKind kind, int slot)
        {
            _layerTrigger = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            _presenter?.SelectSlot(kind, slot);
        }
        private void Back()
        {
            _presenter?.Back();
            if (_layerTrigger != null && _layerTrigger.activeInHierarchy && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(_layerTrigger);
        }
        private void Rename()
        {
            if (_presenter == null || !_presenter.CanManage || _renameInput == null) return;
            if (!_renameInput.gameObject.activeSelf)
            { _renameInput.gameObject.SetActive(true); _renameInput.text = _presenter.Machine.Name; _renameInput.ActivateInputField(); return; }
            var result = _presenter.Rename(_renameInput.text);
            Text("B11_RemoveHint", result == MachineManagementResult.Completed ? "名称已更新" : Reason(result));
            if (result == MachineManagementResult.Completed) { _renameInput.gameObject.SetActive(false); Focus("B11_Rename"); }
        }
        private void Render()
        {
            if (_presenter == null || _covered) return;
            var layer = _presenter.Layer;
            bool overview = layer == MachinePanelLayer.Overview || layer == MachinePanelLayer.Invalid;
            Set("B11_OverviewLayer", overview);
            Set("B11_PickerLayer", layer == MachinePanelLayer.Picker);
            bool feedback = layer == MachinePanelLayer.Confirm || layer == MachinePanelLayer.Waiting || layer == MachinePanelLayer.Success || layer == MachinePanelLayer.Rejected;
            Set("B11_FeedbackLayer", feedback);
            foreach (string suffix in new[] { "Confirm", "Waiting", "Success", "Rejected", "LoadingState", "ErrorState", "ObjectInvalidState" })
                Set("B11_Feedback" + suffix, suffix == layer.ToString());
            Set("B11_OverviewObjectInvalidState", !_presenter.IsValid);
            Set("B11_OverviewErrorState", false); Set("B11_OverviewLoadingState", false);
            ButtonEnabled("B11_Rename", _presenter.CanManage && !feedback);
            ButtonEnabled("B11_Run", _presenter.CanManage && !feedback && !_presenter.IsWaiting);
            ButtonEnabled("B11_OpenHardware", _presenter.IsValid);
            Set("B11_Remove", !overview); Set("B11_RemoveHint", !overview);
            ApplyTab("Overview", overview); ApplyTab("Hardware", !overview);
            if (!_presenter.IsValid) { Text("B11_Identity", "机器已失效 · 返回可关闭"); return; }
            MachineInstance machine = _presenter.Machine;
            Text("B11_Identity", machine.Name + " · Lv." + machine.Definition.Level + " · #" + machine.Id + (_presenter.Origin == ManagementOrigin.Hub ? " · 远程" : " · 现场"));
            Text("B11_State0", machine.Activated ? "已激活" : "未激活");
            Text("B11_State1", machine.RequestedRunState == MachineRunState.Running ? "运行" : machine.RequestedRunState == MachineRunState.Sleeping ? "休眠" : "已停止");
            Text("B11_State2", machine.Powered ? "已供电" : "未供电");
            Text("B11_State3", machine.Connected ? "已连接" : "未连接");
            Text("B11_Run_Label", machine.RequestedRunState == MachineRunState.Running ? "停止机器" : "运行机器");
            Capacity("Compute", machine.ReservedCompute, machine.ComputeCapacity);
            Capacity("Logic", machine.AppliedLogicCost, machine.LogicCapacity);
            Capacity("Container", machine.UsedCapacity, machine.TotalCapacity);
            Text("B11_OverviewTaskValue", machine.HasActiveBehavior ? "存在执行中行为" : "无执行中行为");
            Text("B11_OverviewWaitValue", machine.HasActiveBehavior && machine.RequestedRunState == MachineRunState.Stopped ? "等待行为安全结束" : machine.ComputeWaitingCount > 0 ? "等待算力：" + machine.ComputeWaitingCount : "无算力等待");
            Text("B11_OverviewPowerValue", machine.Powered ? "供电可用" : "供电不可用");
            Slot("B11_Sensor1", HardwareKind.Sensor, 0, overview); Slot("B11_Sensor2", HardwareKind.Sensor, 1, overview);
            Slot("B11_Core1", HardwareKind.Core, 0, overview);
            Slot("B11_Effector1", HardwareKind.Effector, 0, overview); Slot("B11_Effector2", HardwareKind.Effector, 1, overview);
            ButtonEnabled("B11_Remove", _presenter.CanChangeHardware && machine.GetComponent(_presenter.Kind, _presenter.Slot) != null);
            Text("B11_RemoveHint", _presenter.CanChangeHardware ? "先选择已安装组件，再拆卸" : "当前来源无硬件修改权限");
            if (layer == MachinePanelLayer.Picker) RenderPicker();
            if (layer == MachinePanelLayer.Confirm)
            {
                Text("B11_FeedbackConfirmTitle", _presenter.IsRemoval ? "确认拆卸" : "确认安装");
                Text("B11_FeedbackConfirmAccept_Label", _presenter.IsRemoval ? "确认拆卸" : "确认安装");
                bool valid = _presenter.TryGetImpact(out int compute, out int logic, out int capacity);
                Text("B11_FeedbackConfirmMessage", valid ? "停止新行为，等待安全结束后再次核对并变更硬件。" : "组件归属、槽位或占用已变化，不能执行；请返回重新选择。");
                Text("B11_FeedbackConfirmComputeImpact", "算力：" + machine.ComputeCapacity + " → " + compute);
                Text("B11_FeedbackConfirmLogicImpact", "逻辑容量：" + machine.LogicCapacity + " → " + logic);
                Text("B11_FeedbackConfirmCapacityImpact", "容器容量：" + machine.TotalCapacity + " → " + capacity);
                ButtonEnabled("B11_FeedbackConfirmAccept", valid);
            }
            Text("B11_FeedbackRejectedMessage", Reason(_presenter.Result));
            Text("B11_FeedbackSuccessMessage", "硬件变更完成；机器保持停止。");
            Text("B11_FeedbackWaitingMessage", "等待现有行为安全结束；取消只撤销未执行的硬件变更，不恢复运行。");
            if (_lastLayer != layer)
            {
                if (layer == MachinePanelLayer.Confirm) Focus("B11_FeedbackConfirmCancel");
                else if (layer == MachinePanelLayer.Waiting) Focus("B11_FeedbackWaitingDismiss");
                else if (layer == MachinePanelLayer.Success) Focus("B11_FeedbackSuccessDismiss");
                else if (layer == MachinePanelLayer.Rejected) Focus("B11_FeedbackRejectedDismiss");
                else if (layer == MachinePanelLayer.Picker) Focus("B11_PickerBack");
            }
            _lastLayer = layer;
        }
        private void Slot(string key, HardwareKind kind, int index, bool overview)
        {
            bool exists = index < _presenter.Machine.Definition.SlotCount(kind);
            Set(key, exists && !overview); Set(key + "CardArt", exists && !overview); Set(key + "WellArt", exists && !overview);
            if (!exists) return;
            var item = _presenter.Machine.GetComponent(kind, index);
            Text(key + "_ObjectName", item == null ? "空槽" : ComponentName(item));
            Text(key + "_State", item == null ? "可安装" : item.Enabled ? "已启用" : "已停用");
            Set(key + "_Plus", item == null);
            Set(key + "_Selection", _presenter.Kind == kind && _presenter.Slot == index);
            ButtonEnabled(key + (index == 0 ? "_Select" : "_Choose"), _presenter.CanChangeHardware);
            if(index > 0)
            {
                Set(key + "_Choose", item == null);
                ButtonEnabled(key, _presenter.CanChangeHardware && !_presenter.IsWaiting);
            }
            ButtonEnabled(key + "_Enable", _presenter.CanChangeHardware && !_presenter.IsWaiting && item != null && kind != HardwareKind.Core);
            Set(key + "_Enable", item != null && kind != HardwareKind.Core);
            Text(key + "_Enable_Label", item != null && item.Enabled ? "停用" : "启用");
        }
        private void RenderPicker()
        {
            string kind = _presenter.Kind == HardwareKind.Sensor ? "传感器" : _presenter.Kind == HardwareKind.Core ? "核心" : "效应器";
            Text("B11_PickerTitle", kind + " " + (_presenter.Slot + 1) + " · 选择组件");
            var candidates = _presenter.Candidates;
            Set("B11_PickerEmpty", candidates.Count == 0); Set("B11_PickerContent", candidates.Count > 0);
            Set("B11_PickerLoading", false); Set("B11_PickerError", false); Set("B11_PickerInvalid", false);
            Transform template = _lookup["B11_PickerRowItemTemplate"];
            for (int i = 0; i < candidates.Count; i++)
            {
                if (i >= _rows.Count) _rows.Add(Instantiate(template.gameObject, template.parent, false));
                GameObject row = _rows[i]; row.SetActive(true);
                var item = candidates[i];
                var label = row.transform.Find("B11_PickerRowItemTemplateName").GetComponent<TMP_Text>(); label.richText = false;
                label.text = ComponentName(item) + (item.OwnerId.IsValid ? " · 已安装于 #" + item.OwnerId : "");
                var button = row.GetComponentInChildren<Button>(true); button.onClick.RemoveAllListeners();
                var art = row.transform.Find("B11_PickerRowItemTemplateArt").GetComponent<Image>();
                art.sprite = item.OwnerId.IsValid ? _rowDisabled : item.Id == _presenter.CandidateId ? _rowSelected : _rowNormal;
                button.targetGraphic = art;
                PersistentId id = item.Id; button.onClick.AddListener(() => _presenter?.SelectCandidate(id));
                button.interactable = !item.OwnerId.IsValid;
            }
            for (int i = candidates.Count; i < _rows.Count; i++) _rows[i].SetActive(false);
            var selected = _presenter.SelectedComponent;
            Text("B11_PickerDetailName", selected == null ? "请选择一个组件实例" : ComponentName(selected));
            Text("B11_PickerDetailLevel", selected == null ? "" : "等级：" + selected.Definition.Level);
            Text("B11_PickerDetailCapacity", selected == null ? "" : "增加容量：" + selected.Definition.AddedCapacity);
            var configuration = selected != null && MachineCatalog.IsGameDataLoaded ? GF.DataTable.GetDataTable<AutoEra.DataTable.ComponentDefinitions>().GetDataRow(selected.Definition.Id * 10 + selected.Definition.Level) : null;
            Text("B11_PickerDetailStandby", configuration == null ? "待机功率：未提供配置" : "待机功率：" + configuration.IdlePower);
            Text("B11_PickerDetailWorking", configuration == null ? "工作功率：未提供配置" : "工作功率：" + configuration.WorkingPower);
            Text("B11_PickerDetailDescription", selected == null ? "" : "实例ID：" + selected.Id + "\n" + (selected.OwnerId.IsValid ? "已被安装，不能再次安装" : "未安装"));
            ButtonEnabled("B11_PickerInstall", selected != null && !selected.OwnerId.IsValid && _presenter.CanChangeHardware && _presenter.Machine.GetComponent(_presenter.Kind, _presenter.Slot) == null);
        }
        private static string ComponentName(ComponentInstance item)
        {
            if (MachineCatalog.IsGameDataLoaded)
            {
                var row = GF.DataTable.GetDataTable<AutoEra.DataTable.ComponentDefinitions>().GetDataRow(item.Definition.Id * 10 + item.Definition.Level);
                if (row != null) return GF.Localization.GetString(row.NameKey) + " · #" + item.Id;
            }
            return "组件 " + item.Definition.Id + " · #" + item.Id;
        }
        private void Capacity(string key, int used, int total)
        {
            Text("B11_Overview" + key + "Value", used + " / " + total);
            Text("B11_Overview" + key + "Caption", total == 0 ? "未提供此容量" : "当前占用 / 总容量");
            var rect = Node<RectTransform>("B11_Overview" + key + "FillClip");
            if (rect != null) rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, total > 0 ? 532f * Mathf.Clamp01((float)used / total) : 0);
        }
        private void ApplyTab(string key, bool selected)
        {
            var image = Node<Image>("B11_Tab" + key + "_Art");
            if (image != null) image.sprite = selected ? _tabSelected : _tabNormal;
            var label = Node<TMP_Text>("B11_Tab" + key + "_Label");
            if (label != null) label.color = selected ? _tabSelectedText : _tabNormalText;
        }
        private T Node<T>(string key) where T : Component => _lookup.TryGetValue(key, out var node) ? node.GetComponent<T>() : null;
        private void Set(string key, bool active) { if (_lookup.TryGetValue(key, out var node)) node.gameObject.SetActive(active); }
        private void Text(string key, string text) { var label = Node<TMP_Text>(key); if (label != null) { label.richText = false; label.SetText(text); } }
        private void ButtonEnabled(string key, bool enabled) { var button = Node<Button>(key); if (button != null) button.interactable = enabled; }
        private void Focus(string key) { var button = Node<Button>(key); if (button != null && button.isActiveAndEnabled && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject); }
        private void OnDestroy() { _presenter?.Dispose(); }
        public static string Reason(MachineManagementResult result)
        {
            switch (result)
            {
                case MachineManagementResult.Completed: return "已完成";
                case MachineManagementResult.NotActivated: return "机器尚未现场激活";
                case MachineManagementResult.InvalidOrigin: return "当前来源无操作权限";
                case MachineManagementResult.Disconnected: return "机器未连接";
                case MachineManagementResult.CapacityInUse: return "容器容量正在使用";
                case MachineManagementResult.ComputeInUse: return "算力正在使用";
                case MachineManagementResult.LogicCapacityInUse: return "逻辑容量正在使用";
                case MachineManagementResult.DuplicateName: return "名称已被使用";
                case MachineManagementResult.InvalidName: return "名称无效";
                case MachineManagementResult.Occupied: return "槽位已占用";
                case MachineManagementResult.AlreadyInstalled: return "组件已安装在其它槽位";
                case MachineManagementResult.Destroyed: return "机器已损坏";
                default: return "当前状态不允许操作，请返回重新选择";
            }
        }
    }
}
