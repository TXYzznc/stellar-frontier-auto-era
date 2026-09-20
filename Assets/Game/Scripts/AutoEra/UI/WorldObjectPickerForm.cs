using System.Collections.Generic;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 世界对象选择器（规格 05-世界对象选择：候选列表 + 预览）。
    ///
    /// 绑定字段在同名的 .Fields.cs 里，结构由契约生成。
    ///
    /// 它回答一个很窄的问题：**在区域里挑一个对象，把结果交回调用方**。
    /// 候选来自区域域读模型，按请求对象的过滤条件筛选（要不要机器、要不要建筑与资源点）。
    /// 界面内部的选中**只影响预览**，不碰区域的选中状态——区域选中是现场输入的事，
    /// 选择器没有资格改它。
    ///
    /// 结果写回调用方传进来的 <see cref="AutoEraUiSelectionRequest"/>：确认、取消、或两者都不是
    /// （被外部关掉），三种情况调用方都能区分。
    ///
    /// 「在世界中选择」需要世界拾取（光标 → 对象）这条通道，尚未接入，因此该按钮禁用并说明。
    /// </summary>
    public sealed partial class WorldObjectPickerForm : AutoEraShellFormBase
    {
        /// <summary>规格页序：本 Form 只有一页。</summary>
        public const int PagePicker = 0;

        private const string WorldPickMissing = "在区域中直接拾取尚未接入：请在上面的候选列表里选择。";

        private readonly List<int> _candidateIndices = new List<int>(32);

        private IRegionReadModel _region;
        private AutoEraUiSelectionRequest _request;
        private int _selectedIndex = -1;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(CancelSelection);
            if (_closeButton != null) _closeButton.onClick.AddListener(CancelSelection);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(CancelSelection);
            if (_confirmButton != null) _confirmButton.onClick.AddListener(ConfirmSelection);
            if (_selectButton != null) _selectButton.onClick.AddListener(SelectFirstCandidateIfNeeded);
            if (_worldButton != null) _worldButton.onClick.AddListener(ReportWorldPickUnavailable);
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, PagePicker);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _confirmButton != null ? _confirmButton.gameObject : null);

            _request = TryGetRequest(out AutoEraUiSelectionRequest request) ? request : null;
            _region = RegionReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _region.Changed += OnRegionSectionChanged;

            Render(_region.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseRegion();

        protected override void OnAutoEraRecycle()
        {
            ReleaseRegion();
            base.OnAutoEraRecycle();
        }

        private void ReleaseRegion()
        {
            if (_region == null)
            {
                return;
            }

            _region.Changed -= OnRegionSectionChanged;
            _region.Dispose();
            _region = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>当前候选数量（已按请求过滤）。测试与调试用。</summary>
        public int CandidateCount => _candidateIndices.Count;

        /// <summary>区域域当前数据状态；null 表示读模型尚未建立。测试与调试用。</summary>
        public UiDataState? RegionDataState => _region?.Snapshot.State;

        /// <summary>当前内部选中的对象；未选中时为无效 Id。测试与调试用。</summary>
        public PersistentId PendingSelection =>
            _selectedIndex >= 0 && _selectedIndex < _candidateIndices.Count
                ? _region.Snapshot.Objects[_candidateIndices[_selectedIndex]].Id
                : PersistentId.Invalid;

        private void OnRegionSectionChanged(RegionDomainSection section) => Render(_region.Snapshot);

        private void Render(RegionDomainSnapshot snapshot)
        {
            _candidateIndices.Clear();

            bool unavailable = snapshot.State == UiDataState.Unavailable;
            int shown = 0;
            if (!unavailable && _worldObjectPickerCandidatesTemplate != null && _worldObjectPickerCandidatesContent != null)
            {
                // 先按请求过滤出候选索引，再整体交给池渲染：过滤与渲染分开，行索引与快照索引不会混。
                _candidateIndices.Clear();
                for (int i = 0; i < snapshot.Count; i++)
                {
                    UiRegionObjectRow row = snapshot.Objects[i];
                    if (_request == null || _request.Accepts(row))
                    {
                        _candidateIndices.Add(i);
                    }
                }

                shown = RenderListRows(_worldObjectPickerCandidatesTemplate, _worldObjectPickerCandidatesContent,
                    _candidateIndices.Count,
                    (position, item) => item.Bind(position, snapshot.Objects[_candidateIndices[position]].Name,
                        snapshot.Objects[_candidateIndices[position]].Kind + " · " + snapshot.Objects[_candidateIndices[position]].State,
                        OnCandidateClicked));
            }

            // 候选集合变了，原来的内部选中不再对应任何一行。
            if (_selectedIndex >= _candidateIndices.Count)
            {
                _selectedIndex = -1;
            }

            bool empty = !unavailable && shown == 0;
            SetState(_worldObjectPickerLoadingState, false);
            SetState(_worldObjectPickerErrorState, false);
            SetState(_worldObjectPickerEmptyState, empty);
            SetState(_worldObjectPickerSuccessState, !unavailable && !empty);
            SetState(_worldObjectPickerDisabledState, unavailable);

            if (_worldObjectPickerCandidatesBody != null)
            {
                _worldObjectPickerCandidatesBody.SetText(unavailable
                    ? snapshot.UnavailableReason ?? "区域数据不可用"
                    : empty
                        ? "区域里没有符合条件的选择目标。"
                        : "候选 " + AutoEraUiFormat.Count(shown) + " 个");
            }

            RenderPreview(snapshot, unavailable);
            ApplyConfirmAvailability();
        }

        private void RenderPreview(RegionDomainSnapshot snapshot, bool unavailable)
        {
            if (_selectedIndex < 0 || _selectedIndex >= _candidateIndices.Count)
            {
                RenderDetailRows(_worldObjectPickerPreviewTemplate, _worldObjectPickerPreviewContent, null);
                if (_worldObjectPickerPreviewBody != null)
                {
                    _worldObjectPickerPreviewBody.SetText(unavailable
                        ? snapshot.UnavailableReason ?? "区域数据不可用"
                        : "未选择目标：在左侧候选里选一个查看它的公开信息。");
                }

                return;
            }

            // 预览用读模型给出的公开详情字段：选择器不接触领域对象，也不替对象编造
            // 超出公开范围的字段。
            PersistentId id = snapshot.Objects[_candidateIndices[_selectedIndex]].Id;
            RenderDetailRows(_worldObjectPickerPreviewTemplate, _worldObjectPickerPreviewContent, _region.DescribeObject(id));
            if (_worldObjectPickerPreviewBody != null)
            {
                _worldObjectPickerPreviewBody.SetText(string.Empty);
            }
        }

        private void OnCandidateClicked(int listIndex)
        {
            _selectedIndex = listIndex;
            RenderPreview(_region.Snapshot, _region.Snapshot.State == UiDataState.Unavailable);
            ApplyConfirmAvailability();
        }

        private void SelectFirstCandidateIfNeeded()
        {
            if (_selectedIndex < 0 && _candidateIndices.Count > 0)
            {
                OnCandidateClicked(0);
            }
        }

        private void ApplyConfirmAvailability()
        {
            if (_confirmButton != null)
            {
                _confirmButton.interactable = _selectedIndex >= 0;
            }
        }

        private void ConfirmSelection()
        {
            PersistentId id = PendingSelection;
            if (!id.IsValid)
            {
                return;
            }

            _request?.Confirm(id);
            CloseSelf();
        }

        private void CancelSelection()
        {
            _request?.Cancel();
            CloseSelf();
        }

        private void ReportWorldPickUnavailable()
        {
            if (_worldObjectPickerCandidatesBody != null)
            {
                _worldObjectPickerCandidatesBody.SetText(WorldPickMissing);
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
