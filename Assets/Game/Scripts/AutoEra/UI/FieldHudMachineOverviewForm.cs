using System;
using System.Collections.Generic;
using AutoEra.UI.Contracts;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>现场机器总览页的独立 UIDialog 子 Form。</summary>
    public sealed class FieldHudMachineOverviewForm : AutoEraShellFormBase
    {
        private static readonly UiDetailField[] NoFields = System.Array.Empty<UiDetailField>();

        /// <summary>
        /// 机器总览是现场详情侧栏的内容页，只占用 UI 自身区域；打开后仍允许镜头、选取和世界对象交互。
        /// 拆分前该页面由 FieldHudDetailForm 承载，详情 Form 的 BlocksWorldInput 就是 false。
        /// </summary>
        public override bool BlocksWorldInput => false;

        public sealed class Request
        {
            public FieldHudDetailForm Parent { get; }
            public Request(FieldHudDetailForm parent) { Parent = parent; }
        }

        private readonly Dictionary<string, Button> _buttons = new Dictionary<string, Button>(16, StringComparer.Ordinal);
        private FieldHudDetailForm _parent;
        private IRegionReadModel _regionReadModel;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++) if (buttons[i] != null) _buttons[buttons[i].name] = buttons[i];
            Bind("Btn_MachineOverviewAlgorithm", () => AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm,
                new AutoEraUiPageRequest(AlgorithmEditorForm.PageEditor)));
            Bind("Btn_MachineAlgorithmEdit", () => AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm,
                new AutoEraUiPageRequest(AlgorithmEditorForm.PageEditor)));
            Bind("Btn_MachineAlgorithmTemplate", () => AutoEraUiNavigator.Open(this, UIViews.AlgorithmLibraryForm));
            Bind("Btn_MachineOverviewDiagnostic", () => AutoEraUiNavigator.Open(this, UIViews.RecordReaderForm,
                new AutoEraUiPageRequest(RecordReaderForm.PageMachineHistory)));
            Bind("Btn_MachineOverviewHardware", () => { _parent?.ShowSelectionPage(6); CloseSelf(); });
            Bind("Btn_MachineOverviewFocus", FocusSelection);
            string[] unavailable = { "Btn_MachineOverviewActivate", "Btn_MachineOverviewSleep", "Btn_MachineOverviewPower", "Btn_MachineOverviewRecover", "Btn_MachineOverviewRename" };
            for (int i = 0; i < unavailable.Length; i++) if (_buttons.TryGetValue(unavailable[i], out Button button)) button.interactable = false;
            if (_buttons.TryGetValue("Btn_FieldClose", out Button close)) close.onClick.AddListener(CloseSelf);
        }

        protected override void OnAutoEraOpen()
        {
            ReleaseReadModel();
            _regionReadModel = RegionReadModels.Create(SessionOrNull);
            _regionReadModel.Changed += OnRegionChanged;
            TryGetRequest(out Request request);
            _parent = request?.Parent;
            Transform page = transform.Find("Panel_PageMachineOverview");
            if (page != null) page.gameObject.SetActive(true);
            ApplyDefaultFocus(null, null);
            Render(_regionReadModel.Snapshot);
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            ReleaseReadModel();
            _parent = null;
        }

        protected override void OnAutoEraRecycle()
        {
            ReleaseReadModel();
            _parent = null;
            base.OnAutoEraRecycle();
        }

        private void Bind(string name, UnityEngine.Events.UnityAction action)
        {
            if (_buttons.TryGetValue(name, out Button button)) button.onClick.AddListener(action);
        }

        private void FocusSelection()
        {
            if (SessionOrNull?.RegionInput != null && SessionOrNull.RegionInput.FocusSelection()) return;
            Debug.LogWarning("[AutoEra][FieldHudMachineOverview] 当前没有可定位对象");
        }

        private void OnRegionChanged(RegionDomainSection section) => Render(_regionReadModel.Snapshot);

        private void ReleaseReadModel()
        {
            if (_regionReadModel == null) return;
            _regionReadModel.Changed -= OnRegionChanged;
            _regionReadModel.Dispose();
            _regionReadModel = null;
        }

        private void Render(RegionDomainSnapshot snapshot)
        {
            Transform page = transform.Find("Panel_PageMachineOverview");
            if (page == null) return;

            Transform identityContent = FindChild(page, "Content_MachineOverviewIdentity");
            Transform capacityContent = FindChild(page, "Content_MachineOverviewCapacity");
            Transform identityTemplate = FindTemplate(identityContent);
            Transform capacityTemplate = FindTemplate(capacityContent);
            RenderDetailRows(identityTemplate?.gameObject, identityContent as RectTransform,
                snapshot.HasSelection ? snapshot.Detail : NoFields);
            RenderDetailRows(capacityTemplate?.gameObject, capacityContent as RectTransform,
                snapshot.HasSelection ? snapshot.Detail : NoFields);

            SetState(FindChild(page, "Grp_MachineOverviewLoadingState")?.gameObject, false);
            SetState(FindChild(page, "Grp_MachineOverviewErrorState")?.gameObject, false);
            SetState(FindChild(page, "Grp_MachineOverviewSuccessState")?.gameObject, snapshot.HasSelection);
            SetState(FindChild(page, "Grp_MachineOverviewEmptyState")?.gameObject, !snapshot.HasSelection && snapshot.State != UiDataState.Unavailable);
            SetState(FindChild(page, "Grp_MachineOverviewDisabledState")?.gameObject, snapshot.State == UiDataState.Unavailable);

            if (snapshot.State == UiDataState.Unavailable)
            {
                WriteStateCard(FindChild(page, "Grp_MachineOverviewDisabledState")?.gameObject, snapshot.UnavailableReason);
            }
            else if (!snapshot.HasSelection)
            {
                WriteStateCard(FindChild(page, "Grp_MachineOverviewEmptyState")?.gameObject, "请选择一个机器查看详情。");
            }
        }

        private static Transform FindChild(Transform root, string name)
        {
            if (root == null) return null;
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++) if (transforms[i].name == name) return transforms[i];
            return null;
        }

        private static Transform FindTemplate(Transform content)
        {
            if (content == null) return null;
            Transform[] transforms = content.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name.StartsWith("Item_", StringComparison.Ordinal)) return transforms[i];
            }
            return null;
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
