using System.Collections.Generic;
using AutoEra.Algorithms;
using AutoEra.Machines;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 节点组件选择器（规格 13-算法编辑器/节点组件选择：候选与合同说明）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 候选来自「机器上已安装、可被算法端点绑定的组件」（传感器读输入、效应器执行动作），
    /// 由机器域算法读模型 <c>AlgorithmDomainSnapshot.ComponentCandidates</c> 提供；
    /// 按端点类别筛选组件，再列出该组件支持的区域目标；通过生产绑定入口取得实际绑定代次。
    /// </summary>
    public sealed partial class NodeComponentPickerForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：本 Form 只有一页。</summary>
        public const int PageNodeComponentPicker = 0;

        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private IAlgorithmReadModel _algorithms;
        private AutoEraAlgorithmBindingPickRequest _request;
        private readonly List<UiAlgorithmComponentCandidate> _visibleCandidates = new List<UiAlgorithmComponentCandidate>(8);
        private ulong _componentId;
        private IReadOnlyList<UiAlgorithmTargetCandidate> _targets;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            _request = TryGetRequest(out AutoEraAlgorithmBindingPickRequest request) ? request : null;
        }

        protected override void OnAutoEraOpen()
        {
            _request = TryGetRequest(out AutoEraAlgorithmBindingPickRequest request) ? request : null;
            _componentId = 0; _targets = null;
            ShowPage(_pageRoots, PageNodeComponentPicker);
            ApplyDefaultFocus(_backButton != null ? _backButton.gameObject : null, null);

            _algorithms = AlgorithmReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _algorithms.Changed += OnAlgorithmSectionChanged;

            Render(_algorithms.Snapshot);
            DisableDomainActions();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseAlgorithms();

        protected override void OnAutoEraRecycle()
        {
            ReleaseAlgorithms();
            base.OnAutoEraRecycle();
        }

        private void ReleaseAlgorithms()
        {
            if (_algorithms == null)
            {
                return;
            }

            _algorithms.Changed -= OnAlgorithmSectionChanged;
            _algorithms.Dispose();
            _algorithms = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>算法域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? AlgorithmDataState => _algorithms?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OnAlgorithmSectionChanged(AlgorithmDomainSection section) => Render(_algorithms.Snapshot);

        private void Render(AlgorithmDomainSnapshot snapshot)
        {
            if (_componentId != 0) { RenderTargets(); return; }
            if (snapshot.State == UiDataState.Unavailable)
            {
                ShowPageUnavailable(snapshot.UnavailableReason ?? "候选组件暂不可用。",
                    _nodeComponentPickerLoadingState, _nodeComponentPickerEmptyState, _nodeComponentPickerErrorState,
                    _nodeComponentPickerSuccessState, _nodeComponentPickerDisabledState,
                    _nodeComponentPickerCandidatesBody, _nodeComponentPickerContractBody);
                RenderDetailRows(_nodeComponentPickerCandidatesTemplate, _nodeComponentPickerCandidatesContent, NoFields);
                RenderDetailRows(_nodeComponentPickerContractTemplate, _nodeComponentPickerContractContent, NoFields);
                return;
            }

            // 按端点类别筛选候选（无请求时列出全部，作为只读候选查看）。
            _visibleCandidates.Clear();
            if (snapshot.ComponentCandidates != null)
            {
                for (int i = 0; i < snapshot.ComponentCandidates.Count; i++)
                {
                    UiAlgorithmComponentCandidate candidate = snapshot.ComponentCandidates[i];
                    if (_request == null || MatchesKind(candidate.Kind, _request.Kind))
                    {
                        _visibleCandidates.Add(candidate);
                    }
                }
            }

            int candidateCount = _visibleCandidates.Count;
            bool hasCandidates = candidateCount > 0;

            SetState(_nodeComponentPickerLoadingState, false);
            SetState(_nodeComponentPickerEmptyState, !hasCandidates);
            SetState(_nodeComponentPickerErrorState, false);
            SetState(_nodeComponentPickerSuccessState, hasCandidates);
            SetState(_nodeComponentPickerDisabledState, false);

            RenderListRows(_nodeComponentPickerCandidatesTemplate, _nodeComponentPickerCandidatesContent, candidateCount,
                (index, item) => item.Bind(index,
                    _visibleCandidates[index].Label,
                    _visibleCandidates[index].Status,
                    OnCandidateClicked));

            SetText(_nodeComponentPickerCandidatesBody, hasCandidates
                ? "候选 " + candidateCount + " 件，点一行绑定到 "
                    + (_request != null ? _request.Intent : "端点") + "。"
                : _request != null
                    ? "这台机器还没有安装可绑定的"
                        + (_request.Kind == AlgorithmNodeKind.Input ? "传感器" : "效应器")
                        + "组件：装上对应组件后，这里才会出现候选。"
                    : "这台机器还没有安装可绑定的组件：装一个传感器或效应器后，这里才会出现候选。");

            RenderDetailRows(_nodeComponentPickerContractTemplate, _nodeComponentPickerContractContent, NoFields);
        }

        /// <summary>点选候选 → 回写 Rebind（组件 Id；目标对象由后续世界对象选择器补齐）并返回。</summary>
        private void OnCandidateClicked(int index)
        {
            if (_algorithms == null || _request == null || index < 0 || index >= _visibleCandidates.Count)
            {
                return;
            }

            ulong component = _visibleCandidates[index].ComponentId;
            if (!_algorithms.Rebind(_request.InstanceId, _request.BindingKey, component, 0, 0))
            { SetText(_nodeComponentPickerCandidatesBody, _algorithms.Snapshot.CommandUnavailableReason ?? "组件已变化，请重试。"); return; }
            _componentId = component; RenderTargets();
        }
        private void RenderTargets()
        {
            _targets = _algorithms.ReadBindingTargets(_componentId);
            SetState(_nodeComponentPickerEmptyState, _targets.Count == 0);
            SetState(_nodeComponentPickerSuccessState, false);
            RenderListRows(_nodeComponentPickerCandidatesTemplate, _nodeComponentPickerCandidatesContent, _targets.Count,
                (index, item) => item.Bind(index, _targets[index].Name, _targets[index].Status, OnTargetClicked));
            SetText(_nodeComponentPickerCandidatesBody, _targets.Count == 0
                ? "当前区域没有支持此组件的目标；组件选择已保留，可返回检查。"
                : "已选组件，接下来选择它要读取或作业的目标。");
        }
        private void OnTargetClicked(int index)
        {
            if (_targets == null || index < 0 || index >= _targets.Count) return;
            if (_algorithms.Rebind(_request.InstanceId, _request.BindingKey, _componentId, _targets[index].Id, 0)) RequestCancel();
            else SetText(_nodeComponentPickerCandidatesBody, _algorithms.Snapshot.CommandUnavailableReason ?? "目标已变化，请重新选择。");
        }

        private static bool MatchesKind(HardwareKind hardwareKind, AlgorithmNodeKind nodeKind)
            => nodeKind == AlgorithmNodeKind.Input
                ? hardwareKind == HardwareKind.Sensor
                : hardwareKind == HardwareKind.Effector;

        private static void SetText(TMPro.TMP_Text text, string value)
        {
            if (text != null)
            {
                text.SetText(value ?? string.Empty);
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
