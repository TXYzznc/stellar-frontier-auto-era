using AutoEra.Machines;
using AutoEra.UI.Contracts;
using AutoEra.World.Identity;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 安装替换组件选择器（规格 12-选择与绑定 · ComponentPicker）。
    ///
    /// 它回答一个问题：**这一格该装哪一件？** 因此页面只有两栏——候选（散件）与
    /// 「与当前安装比较」，加三个动作：选择（只改预选项）、使用该组件、取消。
    ///
    /// **关键分工**：选择器自己不安装任何东西。确认后它把候选的稳定身份写回调用方的结果对象，
    /// 然后打开 17-硬件修改确认；真正的执行由 `MachineHardwareOperation` 负责
    /// （来源门禁、等待安全停机、失败判定都在那里）。规格原文也是这个顺序：
    /// 「Btn_ComponentPickerConfirm：装配模式返回候选ID并进入17硬件确认」。
    ///
    /// 兼容性判据只有「硬件类别与槽位类别一致」一条，其余不可安装因素（可用性等）照原样展示——
    /// 界面不自己定第二套「什么时候能装」的规则，否则会出现「界面说不让、领域说可以」。
    /// </summary>
    public sealed partial class ComponentPickerForm : AutoEraShellFormBase
    {
        private static readonly UiDetailField[] NoFields = new UiDetailField[0];

        private AutoEraComponentPickRequest _request;
        private IComponentPickerReadModel _picker;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            if (_backButton != null) _backButton.onClick.AddListener(RequestCancel);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestCancel);
            if (_cancelButton != null) _cancelButton.onClick.AddListener(RequestCancel);
            if (_selectButton != null) _selectButton.onClick.AddListener(SelectFocusedCandidate);
            if (_confirmButton != null) _confirmButton.onClick.AddListener(Confirm);
        }

        protected override void OnAutoEraOpen()
        {
            ShowPage(_pageRoots, 0);
            ApplyDefaultFocus(
                _cancelButton != null ? _cancelButton.gameObject : null,
                _confirmButton != null ? _confirmButton.gameObject : null);

            TryGetRequest(out _request);
            _picker = ComponentPickerReadModels.Create(
                TryGetSession(out AutoEraUiSession session) ? session : null, _request);
            _picker.Changed += OnPickerChanged;
            Render(_picker.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown) => ReleasePicker();

        protected override void OnAutoEraRecycle()
        {
            ReleasePicker();
            base.OnAutoEraRecycle();
        }

        /// <summary>
        /// 释放读模型时**不撤销玩家的取消结论**：结果写在调用方的请求对象上，
        /// 属于调用方的状态，不随页面销毁而改变。
        /// </summary>
        private void ReleasePicker()
        {
            if (_picker != null)
            {
                _picker.Changed -= OnPickerChanged;
                _picker.Dispose();
                _picker = null;
            }
        }

        private void OnPickerChanged() => Render(_picker.Snapshot);

        // -------------------------------------------------- 测试与调试读数

        /// <summary>本次打开是否带请求。测试与调试用。</summary>
        public bool HasRequest => _request != null;

        /// <summary>当前读数。测试与调试用。</summary>
        public ComponentPickerSnapshot PickerSnapshot => _picker != null
            ? _picker.Snapshot
            : ComponentPickerSnapshot.Unavailable("读模型尚未建立。");

        /// <summary>「使用该组件」当前是否可用。测试与调试用。</summary>
        public bool CanConfirmSelection => PickerSnapshot.CanConfirm;

        /// <summary>玩家确认选中的候选；未确认时为无效 Id。测试与调试用。</summary>
        public PersistentId PickedComponentId =>
            _request != null ? _request.SelectedId : PersistentId.Invalid;

        // -------------------------------------------------- 渲染

        private void Render(ComponentPickerSnapshot snapshot)
        {
            bool unavailable = snapshot.State == UiDataState.Unavailable;
            bool empty = snapshot.State == UiDataState.Empty;

            SetState(_componentPickerLoadingState, false);
            SetState(_componentPickerEmptyState, empty);
            SetState(_componentPickerErrorState, false);
            SetState(_componentPickerSuccessState, snapshot.State == UiDataState.Ready);
            SetState(_componentPickerDisabledState, unavailable);

            SetText(_title, _request != null
                ? _request.Intent
                : "安装替换组件选择器");
            SetText(_componentPickerCandidatesBody, CandidatesBody(snapshot));
            SetText(_componentPickerComparisonBody, ComparisonBody(snapshot));

            RenderListRows(_componentPickerCandidatesTemplate, _componentPickerCandidatesContent,
                snapshot.CandidateCount,
                (index, item) => item.Bind(index, snapshot.Candidates[index].Label,
                    snapshot.Candidates[index].Value, OnCandidateClicked));
            RenderDetailRows(_componentPickerComparisonTemplate, _componentPickerComparisonContent,
                snapshot.Comparison ?? NoFields);

            // 「选择」只在有候选可预选时可用；「使用该组件」在预选了**兼容**候选时才可用；
            // 「取消」永远是安全出口（规格：首焦点优先取消／返回）。
            SetInteractable(_selectButton, snapshot.CandidateCount > 0);
            SetInteractable(_confirmButton, snapshot.CanConfirm);
            SetInteractable(_cancelButton, true);
        }

        private static string CandidatesBody(ComponentPickerSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                return snapshot.UnavailableReason ?? "组件数据不可用。";
            }

            if (snapshot.CandidateCount == 0)
            {
                return snapshot.UnavailableReason ?? ComponentPickerReadModels.NoCandidateReason;
            }

            int compatible = 0;
            for (int i = 0; i < snapshot.CandidateCount; i++)
            {
                if (snapshot.Candidates[i].Compatible)
                {
                    compatible++;
                }
            }

            // 兼容与不兼容都要报出来：只说「共 5 件」会让人以为每件都能装。
            return "候选 " + snapshot.CandidateCount + " 件（兼容 " + compatible + " 件，"
                + "其余与 " + snapshot.SlotLabel + " 的类别不符）。点一行只改变预选项。";
        }

        private static string ComparisonBody(ComponentPickerSnapshot snapshot)
        {
            if (snapshot.State == UiDataState.Unavailable)
            {
                return snapshot.UnavailableReason ?? "组件数据不可用。";
            }

            if (snapshot.CandidateCount == 0)
            {
                return "没有候选可以比较。无可用项时应沿组件库／商店取得组件（经济域尚未接入）。";
            }

            if (!snapshot.HasSelection)
            {
                return "还没有预选组件：" + (snapshot.ConfirmBlockedReason ?? string.Empty);
            }

            return snapshot.CanConfirm
                ? "确认后会打开 17-硬件修改确认，真正装入由领域执行。"
                : "不能确认：" + (snapshot.ConfirmBlockedReason ?? string.Empty);
        }

        private void OnCandidateClicked(int index)
        {
            if (_picker == null)
            {
                return;
            }

            ComponentPickerSnapshot snapshot = _picker.Snapshot;
            if (snapshot.Candidates == null || index < 0 || index >= snapshot.Candidates.Count)
            {
                return;
            }

            _picker.Select(snapshot.Candidates[index].Id);
        }

        /// <summary>「选择候选」：把当前预选项再选一次（规格：选择只改变预选项）。</summary>
        private void SelectFocusedCandidate()
        {
            if (_picker == null)
            {
                return;
            }

            ComponentPickerSnapshot snapshot = _picker.Snapshot;
            if (!snapshot.HasSelection && snapshot.CandidateCount > 0)
            {
                _picker.Select(snapshot.Candidates[0].Id);
            }
        }

        /// <summary>
        /// 「使用该组件」：把候选的稳定身份写回调用方，并打开 17-硬件修改确认。
        ///
        /// 只写结果、只打开确认页——**不装入任何东西**。装入的判定与执行全在领域侧，
        /// 界面因此不可能绕过确认改硬件。
        /// </summary>
        public void Confirm()
        {
            ComponentPickerSnapshot snapshot = PickerSnapshot;
            if (!snapshot.CanConfirm)
            {
                SetText(_componentPickerComparisonBody,
                    "不能确认：" + (snapshot.ConfirmBlockedReason ?? "还没有可以确认的候选。"));
                return;
            }

            _request.Result.Confirm(snapshot.SelectedId);
            AutoEraUiNavigator.Open(this, UIViews.OperationDialogForm,
                new AutoEraHardwareRequest(_request.MachineId, _request.Kind, _request.SlotIndex,
                    snapshot.SelectedId, remove: false));
        }

        /// <summary>取消：把「玩家取消了」写回调用方（不消费库存，也不改变任何安装位置）。</summary>
        private void RequestCancel()
        {
            _request?.Result.Cancel();
            TryHandleIntent(AutoEraUiIntent.Cancel);
        }

        private static void SetInteractable(Button button, bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
        }

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
