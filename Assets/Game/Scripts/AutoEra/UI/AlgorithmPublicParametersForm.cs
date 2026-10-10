using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.UI.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 算法工作台的公开参数子界面。
    ///
    /// 公开参数拥有独立的滚动列表、编辑控件和应用动作，使用独立 UIForm 后不再随
    /// AlgorithmEditorForm 一起实例化。它通过相同的 AutoEraUiSession 创建机器域读模型，
    /// 所有修改仍由 AlgorithmInstanceService 作为唯一写入口处理。
    /// </summary>
    public sealed partial class AlgorithmPublicParametersForm : AutoEraShellFormBase
    {
        private static readonly UiDetailField[] NoFields = new UiDetailField[0];
        private IAlgorithmReadModel _algorithms;
        private bool _ownsReadModel;
        private readonly List<UiAlgorithmNodeRow> _parameters = new List<UiAlgorithmNodeRow>(16);
        private System.Action<int, UiListRowItem> _bindParameter;
        private System.Action<int> _selectParameter;

        internal sealed class Request
        {
            public Request(IAlgorithmReadModel readModel) { ReadModel = readModel; }
            public IAlgorithmReadModel ReadModel { get; }
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            _bindParameter = BindParameter;
            _selectParameter = SelectParameterRow;
            if (_backButton != null) _backButton.onClick.AddListener(CloseSelf);
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseSelf);
            if (_applyButton != null) _applyButton.onClick.AddListener(ApplyDraft);
            if (_changeButton != null) _changeButton.onClick.AddListener(SelectFirstParameter);
            if (_defaultButton != null) _defaultButton.onClick.AddListener(ResetSelectedParameter);
            if (_numberValue != null) _numberValue.onEndEdit.AddListener(OnNumberParameterEdited);
            if (_booleanValue != null) _booleanValue.onValueChanged.AddListener(OnBooleanParameterEdited);
        }

        protected override void OnAutoEraOpen()
        {
            _ownsReadModel = !TryGetRequest(out Request request) || request.ReadModel == null;
            _algorithms = _ownsReadModel ? AlgorithmReadModels.Create(SessionOrNull) : request.ReadModel;
            _algorithms.Changed += OnAlgorithmChanged;
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null,
                _changeButton != null ? _changeButton.gameObject : null);
            Render(_algorithms.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseReadModel();

        protected override void OnAutoEraRecycle()
        {
            ReleaseReadModel();
            base.OnAutoEraRecycle();
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation)
        {
        }

        private void ReleaseReadModel()
        {
            if (_algorithms == null) return;
            _algorithms.Changed -= OnAlgorithmChanged;
            if (_ownsReadModel) _algorithms.Dispose();
            _algorithms = null;
        }

        private void OnAlgorithmChanged(AlgorithmDomainSection section)
        {
            if (_algorithms != null) Render(_algorithms.Snapshot);
        }

        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                ShowPageUnavailable(snapshot.UnavailableReason ?? "算法实例暂不可用。",
                    _loadingState, _emptyState, _errorState, _successState, _disabledState,
                    _parametersBody, _impactBody);
                RenderDetailRows(_parametersTemplate, _parametersContent, NoFields);
                RenderDetailRows(_impactTemplate, _impactContent, NoFields);
                SetAvailability(false, false);
                return;
            }

            bool hasInstance = snapshot.InstanceCount > 0;
            SetState(_loadingState, false);
            SetState(_emptyState, !hasInstance);
            SetState(_errorState, false);
            SetState(_successState, false);
            SetState(_disabledState, false);

            RenderParameterRows(snapshot);
            RenderDetailRows(_impactTemplate, _impactContent, NoFields);
            SetText(_parametersBody, hasInstance
                ? "选择公开参数行，修改数值或布尔值，再点击应用草稿。"
                : AlgorithmReadModels.NoInstanceReason);
            SetText(_impactBody, hasInstance
                ? "修改只影响草稿；应用时会进行统一校验。"
                : AlgorithmReadModels.NoInstanceReason);

            bool canEdit = snapshot.State == UiDataState.Ready && snapshot.SelectedInstance.HasValue;
            AlgorithmApplyState state = canEdit ? snapshot.SelectedInstance.Value.RequestState : AlgorithmApplyState.None;
            SetAvailability(canEdit && state == AlgorithmApplyState.None,
                canEdit && (state == AlgorithmApplyState.None || state == AlgorithmApplyState.AwaitingWarningConfirmation));
        }

        private void SetAvailability(bool canEdit, bool canApply)
        {
            if (_changeButton != null) _changeButton.interactable = canEdit;
            if (_defaultButton != null) _defaultButton.interactable = canEdit;
            if (_applyButton != null) _applyButton.interactable = canApply;
            if (_numberValue != null) _numberValue.interactable = canEdit;
            if (_booleanValue != null) _booleanValue.interactable = canEdit;
        }

        private void RenderParameterRows(AlgorithmDomainSnapshot snapshot)
        {
            if (_parametersTemplate == null || _parametersContent == null || snapshot.GraphNodes == null)
                return;

            _parameters.Clear();
            for (int i = 0; i < snapshot.GraphNodes.Count; i++)
            {
                UiAlgorithmNodeRow row = snapshot.GraphNodes[i];
                if (row.Label.StartsWith("Parameter ", System.StringComparison.OrdinalIgnoreCase)
                    || row.Label.StartsWith("参数 ", System.StringComparison.OrdinalIgnoreCase))
                {
                    _parameters.Add(row);
                }
            }

            RenderListRows(_parametersTemplate, _parametersContent, _parameters.Count, _bindParameter);
            SetText(_parametersBody, _parameters.Count == 0 ? "当前算法没有公开参数。" : "选择参数后可修改草稿值。");

            UiAlgorithmNodeRow? selected = snapshot.SelectedNode;
            if (!selected.HasValue || selected.Value.Default == null) return;
            AlgorithmValue value = selected.Value.Default;
            if (value.Type != null && value.Type.Kind == AlgorithmValueKind.Boolean)
            {
                if (_booleanValue != null) _booleanValue.SetIsOnWithoutNotify(value.Boolean);
            }
            else if (_numberValue != null && value.Type != null)
            {
                _numberValue.contentType=TMP_InputField.ContentType.Standard;
                _numberValue.SetTextWithoutNotify(AlgorithmParameterText.Format(value));
                SetText(_impactBody,value.Type.Kind==AlgorithmValueKind.Position?"位置按 x, y, z 输入；坐标仍需合法停靠。":value.Type.Kind==AlgorithmValueKind.Enumeration?"物品参数输入实际物品编号，例如木材30002、矿石30003。":"修改只影响草稿；应用时进行统一校验。");
            }
        }

        private void BindParameter(int index, UiListRowItem item)
        {
            UiAlgorithmNodeRow row = _parameters[index];
            item.Bind(index, row.Label, FormatParameterValue(row.Default), _selectParameter);
        }

        private void SelectParameterRow(int index)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue
                || _algorithms.Snapshot.GraphNodes == null) return;
            int seen = 0;
            for (int i = 0; i < _algorithms.Snapshot.GraphNodes.Count; i++)
            {
                UiAlgorithmNodeRow row = _algorithms.Snapshot.GraphNodes[i];
                if (!(row.Label.StartsWith("Parameter ", System.StringComparison.OrdinalIgnoreCase)
                    || row.Label.StartsWith("参数 ", System.StringComparison.OrdinalIgnoreCase))) continue;
                if (seen++ == index)
                {
                    _algorithms.SelectNode(row.Id);
                    return;
                }
            }
        }

        private void SelectFirstParameter()
        {
            SelectParameterRow(0);
            SetText(_impactBody, "参数已选中；修改值后仍需通过统一应用验证。");
        }

        private void ResetSelectedParameter()
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue
                || !_algorithms.Snapshot.SelectedNode.HasValue)
            {
                SetText(_impactBody, "请先选择一个公开参数。");
                return;
            }

            bool reset = _algorithms.ResetNodeDefault(_algorithms.Snapshot.SelectedInstance.Value.Id,
                _algorithms.Snapshot.SelectedNode.Value.Id);
            SetText(_impactBody, reset
                ? "已恢复到保存版本的默认值，应用前仍可继续修改。"
                : "该参数没有可恢复的保存默认值。");
        }

        private void OnNumberParameterEdited(string text)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue
                || !_algorithms.Snapshot.SelectedNode.HasValue) return;
            var type=_algorithms.Snapshot.SelectedNode.Value.Default?.Type;
            if(!AlgorithmParameterText.TryParse(type,text,out var value)) { SetText(_impactBody,"输入格式无效：数值需有限，枚举需整数，位置需 x, y, z。"); return; }
            _algorithms.SetNodeDefault(_algorithms.Snapshot.SelectedInstance.Value.Id,
                _algorithms.Snapshot.SelectedNode.Value.Id, value);
        }

        private void OnBooleanParameterEdited(bool value)
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue
                || !_algorithms.Snapshot.SelectedNode.HasValue) return;
            _algorithms.SetNodeDefault(_algorithms.Snapshot.SelectedInstance.Value.Id,
                _algorithms.Snapshot.SelectedNode.Value.Id, AlgorithmValue.Bool(value));
        }

        private void ApplyDraft()
        {
            if (_algorithms == null || !_algorithms.Snapshot.SelectedInstance.HasValue) return;
            UiAlgorithmInstanceRow instance = _algorithms.Snapshot.SelectedInstance.Value;
            if (instance.RequestState == AlgorithmApplyState.AwaitingWarningConfirmation)
                _algorithms.ConfirmWarnings(instance.Id, instance.RequestId);
            else
                _algorithms.Apply(instance.Id);
        }

        private static string FormatParameterValue(AlgorithmValue value)
        {
            if (value == null || value.Type == null) return "—";
            return AlgorithmParameterText.Format(value);
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null) text.SetText(value ?? string.Empty);
        }
    }
}
