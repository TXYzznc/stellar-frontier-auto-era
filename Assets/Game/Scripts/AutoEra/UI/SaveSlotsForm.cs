using System;
using AutoEra.Save;
using AutoEra.UI.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 存档槽列表（规格 01-启动与存档/SaveSlots、SaveDetail、NewProgress）。
    ///
    /// 三个页面同属一个 Form，共用 Grp_PageHost，同一时刻只激活一页。
    /// 结构由 Docs/Development/UI-PrefabLayouts/SaveSlotsForm.contract.json 生成；
    /// 绑定字段在同名的 .Fields.cs 里。
    ///
    /// 数据来源只有一条：打开参数里的 AutoEraUiSession → 存档域读模型。
    /// 读模型背后是文件系统且**没有领域推送**，所以这里只有「打开时读一次 + 操作后显式刷新」，
    /// 不假装有事件流。
    ///
    /// 读档与建档需要世界进度层把存档内容解释成世界状态，该层尚未接入，因此这两个按钮
    /// 被**禁用并写明原因**——按规格「不存在／无权限时禁用写操作并说明」，不给假按钮。
    /// </summary>
    public sealed partial class SaveSlotsForm : AutoEraShellFormBase
    {
        // 绑定字段由 SaveSlotsForm.Fields.cs 依契约生成，与本文件同属一个 partial 类；
        // 新增节点引用请改契约后重新生成，不要在此手写字段。

        /// <summary>规格页序：0 存档槽列表、1 存档详情、2 新建进度。</summary>
        public const int PageSlots = 0;
        public const int PageDetail = 1;
        public const int PageNewProgress = 2;

        /// <summary>世界进度层未接入时给玩家的说明（禁用状态必须能解释自己）。</summary>
        private const string ProgressLayerMissing = "世界进度层尚未接入：读取与新建存档暂不可用，槽位列表与详情可以查看。";

        private ISaveSlotReadModel _saveSlots;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            // 新建进度页带回退按钮：它回存档槽列表，而不是关掉整个界面。
            if (_newProgressBackButton != null) _newProgressBackButton.onClick.AddListener(() => ShowSlotPage(PageSlots));
            if (_selectButton != null) _selectButton.onClick.AddListener(OpenDetailPage);
            if (_detailsButton != null) _detailsButton.onClick.AddListener(OpenDetailPage);
            if (_createButton != null) _createButton.onClick.AddListener(() => ShowSlotPage(PageNewProgress));
            if (_saveDetailDeleteButton != null) _saveDetailDeleteButton.onClick.AddListener(DeleteSelectedSlot);
            if (_saveDetailRecoverButton != null) _saveDetailRecoverButton.onClick.AddListener(RecoverSelectedSlot);
        }

        protected override void OnAutoEraOpen()
        {
            // 调用方可以指定落在哪一页（主菜单的「新游戏」直达新建进度页）。
            int initialPage = TryGetRequest(out AutoEraUiPageRequest pageRequest) ? pageRequest.Page : PageSlots;
            ShowPage(_pageRoots, initialPage);
            ApplyDefaultFocus(
                _backButton != null ? _backButton.gameObject : null,
                _continueButton != null ? _continueButton.gameObject : null);

            _saveSlots = SaveSlotReadModels.Create(TryGetSession(out AutoEraUiSession session) ? session : null);
            _saveSlots.Changed += OnSaveSlotSectionChanged;

            RenderSlotList(_saveSlots.Snapshot);
            RenderSlotDetail(_saveSlots.Snapshot);
            ApplyWriteAvailability();
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleaseSaveSlots();

        protected override void OnAutoEraRecycle()
        {
            ReleaseSaveSlots();
            base.OnAutoEraRecycle();
        }

        private void ReleaseSaveSlots()
        {
            if (_saveSlots == null)
            {
                return;
            }

            _saveSlots.Changed -= OnSaveSlotSectionChanged;
            _saveSlots.Dispose();
            _saveSlots = null;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowSlotPage(int page) => ShowPage(_pageRoots, page);

        /// <summary>当前槽位列表的数据状态；null 表示读模型尚未建立。测试与空态解释用。</summary>
        public UiDataState? SaveSlotDataState => _saveSlots?.Snapshot.State;

        private void RequestCancel() => TryHandleIntent(AutoEraUiIntent.Cancel);

        private void OpenDetailPage()
        {
            // 「选择」在槽位列表语义上等于「看这一格」；真正的读档入口是详情页的继续（当前禁用）。
            if (_saveSlots != null && !_saveSlots.Snapshot.HasSelection && _saveSlots.Snapshot.Count > 0)
            {
                _saveSlots.Select(0);
            }

            ShowSlotPage(PageDetail);
        }

        private void DeleteSelectedSlot()
        {
            if (_saveSlots == null || !_saveSlots.Snapshot.HasSelection)
            {
                return;
            }

            int index = _saveSlots.SelectedIndex;
            if (!TryGetSession(out AutoEraUiSession session) || session.SaveSlots == null)
            {
                return;
            }

            session.SaveSlots.Delete(index);
            _saveSlots.ClearSelection();
            _saveSlots.Refresh();
        }

        /// <summary>
        /// 恢复 = 重新读取：<see cref="AutoEra.Save.SaveSlotService.Read"/> 在主文件损坏时
        /// 已经会回退到备份，所以这里不需要另写一条恢复逻辑，只要把最新结果重新读回来。
        /// </summary>
        private void RecoverSelectedSlot() => _saveSlots?.Refresh();

        private void OnSaveSlotSectionChanged(SaveSlotDomainSection section)
        {
            if (_saveSlots == null)
            {
                return;
            }

            if (section == SaveSlotDomainSection.List)
            {
                RenderSlotList(_saveSlots.Snapshot);
            }
            else
            {
                RenderSlotDetail(_saveSlots.Snapshot);
            }
        }

        // -------------------------------------------------- 槽位列表页

        private void RenderSlotList(SaveSlotDomainSnapshot snapshot)
        {
            ApplySlotListState(snapshot);

            if (_slotRowTemplate == null || _slotListContent == null)
            {
                Debug.LogWarning("[AutoEra][SaveSlotsForm] 槽位列表未渲染：行模板或容器未绑进契约。");
                return;
            }

            RenderListRows(_slotRowTemplate, _slotListContent, snapshot.Count,
                (position, item) => item.Bind(position, snapshot.Slots[position].Title,
                    snapshot.Slots[position].Summary + " · " + snapshot.Slots[position].WorldTime, OnSlotRowClicked));

            if (_saveSlotsSlotsBody != null && snapshot.State == UiDataState.Ready)
            {
                _saveSlotsSlotsBody.SetText("存档 " + AutoEraUiFormat.Count(OccupiedCount(snapshot)) + " / " + SaveSlotService.SlotCount);
            }
        }

        private static int OccupiedCount(SaveSlotDomainSnapshot snapshot)
        {
            int occupied = 0;
            for (int i = 0; i < snapshot.Count; i++)
            {
                if (snapshot.Slots[i].Occupied)
                {
                    occupied++;
                }
            }

            return occupied;
        }

        private void ApplySlotListState(SaveSlotDomainSnapshot snapshot)
        {
            SetState(_saveSlotsLoadingState, false);
            SetState(_saveSlotsSuccessState, false);
            SetState(_saveSlotsErrorState, false);
            SetState(_saveSlotsEmptyState, snapshot.State == UiDataState.Empty);
            SetState(_saveSlotsDisabledState, snapshot.State == UiDataState.Unavailable);

            if (snapshot.State == UiDataState.Ready)
            {
                if (snapshot.HasRecoverableSlot && _saveSlotsSlotsBody != null)
                {
                    _saveSlotsSlotsBody.SetText("有存档需要恢复：选中它并查看详情。");
                }

                return;
            }

            if (_saveSlotsSlotsBody != null)
            {
                _saveSlotsSlotsBody.SetText(snapshot.State == UiDataState.Unavailable
                    ? snapshot.UnavailableReason ?? "存档不可用"
                    : "还没有存档；新建进度后会出现在这里。");
            }
        }

        private void OnSlotRowClicked(int index)
        {
            if (_saveSlots == null)
            {
                return;
            }

            _saveSlots.Select(index);
            RenderSlotPreview(_saveSlots.Snapshot);
        }

        // -------------------------------------------------- 槽位详情页

        private void RenderSlotDetail(SaveSlotDomainSnapshot snapshot)
        {
            ApplySlotDetailState(snapshot);
            RenderSlotPreview(snapshot);

            if (!snapshot.HasSelection)
            {
                if (_saveDetailMetadataBody != null)
                {
                    _saveDetailMetadataBody.SetText("未选择槽位：回到列表选择一格查看详情。");
                }

                return;
            }

            RenderDetailRows(_saveDetailMetadataTemplate, _saveDetailMetadataContent, snapshot.Metadata);
            RenderDetailRows(_saveDetailHealthTemplate, _saveDetailHealthContent, snapshot.Health);

            if (_saveDetailMetadataBody != null)
            {
                _saveDetailMetadataBody.SetText(string.Empty);
            }

            if (_saveDetailHealthBody != null)
            {
                _saveDetailHealthBody.SetText(string.Empty);
            }
        }

        private void RenderSlotPreview(SaveSlotDomainSnapshot snapshot)
        {
            if (_saveSlotsPreviewBody == null)
            {
                return;
            }

            if (snapshot.State == UiDataState.Unavailable)
            {
                _saveSlotsPreviewBody.SetText(snapshot.UnavailableReason ?? "存档不可用");
                return;
            }

            if (!snapshot.HasSelection)
            {
                _saveSlotsPreviewBody.SetText("选择左侧一个槽位查看摘要。");
                return;
            }

            UiSaveSlotRow row = snapshot.Slots[snapshot.SelectedIndex];
            _saveSlotsPreviewBody.SetText(row.Title + "：" + row.Summary + "（" + row.WorldTime + "）");
        }

        private void ApplySlotDetailState(SaveSlotDomainSnapshot snapshot)
        {
            SetState(_saveDetailLoadingState, false);
            SetState(_saveDetailSuccessState, snapshot.HasSelection);
            SetState(_saveDetailErrorState, false);
            SetState(_saveDetailEmptyState, snapshot.State != UiDataState.Unavailable && !snapshot.HasSelection);
            SetState(_saveDetailDisabledState, snapshot.State == UiDataState.Unavailable);
        }

        // -------------------------------------------------- 未就绪写操作

        private void ApplyWriteAvailability()
        {
            // 需要世界进度层的三个入口：读取当前槽、从详情读取、创建新进度。
            SetInteractable(_continueButton, false);
            SetInteractable(_saveDetailContinueButton, false);
            SetInteractable(_newProgressCreateButton, false);

            if (_newProgressTargetBody != null)
            {
                _newProgressTargetBody.SetText(ProgressLayerMissing);
            }

            if (_newProgressConsequencesBody != null)
            {
                _newProgressConsequencesBody.SetText("本页只说明将要发生什么，不执行创建。");
            }
        }

        private static void SetInteractable(Button button, bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }


        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
