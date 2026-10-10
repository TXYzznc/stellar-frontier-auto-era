using AutoEra.UI.Contracts;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// FieldHudDetailForm 的 GF 桥。结构由 Docs/Development/UI-PrefabLayouts/FieldHudDetailForm.contract.json 生成
    /// （设计来源：Docs/GameDesign/03-玩家体验/界面规格）。
    ///
    /// 绑定字段在同名的 FieldHudDetailForm.Fields.cs 里（同一 partial 类）；本文件只有类逻辑：
    /// 规格页序切换、取消意图与默认焦点。Grp_PageHost 下的内容页顺序即规格页序。
    /// </summary>
    public sealed partial class FieldHudDetailForm : AutoEraShellFormBase
    {
        private static readonly UiDetailField[] NoFields = Array.Empty<UiDetailField>();

        private static readonly Dictionary<int, string> PrimaryContentNames = new Dictionary<int, string>
        {
            { 6, "Content_MachineHardwareSlots" },
            { 7, "Content_MachineAlgorithmParameters" },
            { 8, "Content_MachineDiagnosticsTasks" },
            { 9, "Content_FarmRecord" },
            { 10, "Content_ForestRecord" },
            { 11, "Content_MineralRecord" },
            { 12, "Content_WaterRecord" },
            { 13, "Content_PumpProduction" },
            { 14, "Content_BuildingOverviewIdentity" },
            { 15, "Content_ConstructionCost" },
            { 16, "Content_WarehouseBuildingCapacity" },
            { 17, "Content_GeneratorPower" },
            { 18, "Content_SolarPower" },
            { 19, "Content_BatteryStorage" },
            { 20, "Content_ConveyorState" },
            { 21, "Content_SensorRecordsSamples" },
        };

        /// <summary>详情侧栏是现场 HUD 的扩展面板，不应暂停镜头移动或世界选取。</summary>
        public override bool BlocksWorldInput => false;

        public RectTransform BuildingOverviewIdentityContent => FindRect("Content_BuildingOverviewIdentity");
        public Button FarmRecordButton => FindButton("Btn_FarmRecord");
        public Button MachineOverviewDiagnosticButton => FindButton("Btn_MachineOverviewDiagnostic");
        public Button MachineOverviewFocusButton => FindButton("Btn_MachineOverviewFocus");

        private readonly Dictionary<string, Button> _buttonCache = new Dictionary<string, Button>(32, StringComparer.Ordinal);
        private readonly Dictionary<string, RectTransform> _rectCache = new Dictionary<string, RectTransform>(32, StringComparer.Ordinal);
        private readonly Dictionary<string, Transform> _transformCache = new Dictionary<string, Transform>(256, StringComparer.Ordinal);
        private int _machineOverviewFormId;
        private Button _machineOverviewFocusProxy;
        private IRegionReadModel _regionReadModel;

        private Button FindButton(string name)
        {
            if (_buttonCache.TryGetValue(name, out Button cached)) return cached;
            return null;
        }

        private RectTransform FindRect(string name)
        {
            _rectCache.TryGetValue(name, out RectTransform cached);
            return cached;
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            CacheVisualReferences();
            EnsurePageRoots();
            Button backButton = FindButton("Btn_FormBack");
            if (backButton != null)
            {
                // 详情侧栏只保留一个出口；返回与关闭在此没有不同的上一层页面。
                backButton.gameObject.SetActive(false);
            }
            BindExitButton("Btn_FormClose");
            BindDetailActions();
            Debug.Log($"[AutoEra][FieldHudDetail] OnInit name={name} pageRoots={(_pageRoots == null ? -1 : _pageRoots.Length)}");
        }

        private void CacheVisualReferences()
        {
            _buttonCache.Clear();
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null) _buttonCache[buttons[i].name] = buttons[i];
            }

            _rectCache.Clear();
            RectTransform[] rects = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i] != null) _rectCache[rects[i].name] = rects[i];
            }

            _transformCache.Clear();
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] != null && !_transformCache.ContainsKey(transforms[i].name))
                {
                    _transformCache.Add(transforms[i].name, transforms[i]);
                }
            }

            // 机器总览页已经迁移为子 Form；保留一个无视觉代理，兼容旧的外部查询合同，
            // 让尚未更新的调用方仍然走同一条 FocusSelectedObject 意图路径。
            if (!_buttonCache.ContainsKey("Btn_MachineOverviewFocus"))
            {
                GameObject proxy = new GameObject("Btn_MachineOverviewFocusProxy", typeof(RectTransform), typeof(Button));
                proxy.transform.SetParent(transform, false);
                proxy.SetActive(false);
                _machineOverviewFocusProxy = proxy.GetComponent<Button>();
                _machineOverviewFocusProxy.onClick.AddListener(FocusSelectedObject);
                _buttonCache["Btn_MachineOverviewFocus"] = _machineOverviewFocusProxy;
            }
        }

        private void BindDetailActions()
        {
            BindButton("Btn_MachineOverviewAlgorithm", OpenAlgorithmEditor);
            BindButton("Btn_MachineAlgorithmEdit", OpenAlgorithmEditor);
            BindButton("Btn_MachineAlgorithmTemplate", OpenAlgorithmLibrary);
            BindButton("Btn_MachineOverviewDiagnostic", OpenMachineHistory);
            BindButton("Btn_MachineDiagnosticsTaskRecord", OpenMachineHistory);
            BindButton("Btn_MachineDiagnosticsRunRecord", OpenMachineHistory);
            BindButton("Btn_MachineOverviewHardware", OpenHardwarePage);
            DisableUnavailableActions();

            string[] recordButtons =
            {
                "Btn_FarmRecord", "Btn_ForestRecord", "Btn_MineralRecord", "Btn_WaterRecord",
                "Btn_WarehouseBuildingRecords"
            };
            for (int i = 0; i < recordButtons.Length; i++)
            {
                BindButton(recordButtons[i], OpenMachineHistory);
            }

            string[] knowledgeButtons =
            {
                "Btn_FarmKnowledge", "Btn_ForestKnowledge", "Btn_MineralKnowledge", "Btn_WaterKnowledge"
            };
            for (int i = 0; i < knowledgeButtons.Length; i++)
            {
                BindButton(knowledgeButtons[i], OpenCropKnowledge);
            }

            string[] focusButtons =
            {
                "Btn_MachineOverviewFocus", "Btn_MachineDiagnosticsLocate", "Btn_FarmFocus",
                "Btn_ForestFocus", "Btn_MineralFocus", "Btn_WaterFocus", "Btn_PumpFocus",
                "Btn_ConstructionFocus", "Btn_BuildingOverviewFocus"
            };
            for (int i = 0; i < focusButtons.Length; i++)
            {
                BindButton(focusButtons[i], FocusSelectedObject);
            }
        }

        private void DisableUnavailableActions()
        {
            // 这些领域命令目前没有接入权威操作服务；保持禁用比显示可点击但无结果更诚实。
            string[] unavailable =
            {
                "Btn_MachineOverviewActivate", "Btn_MachineOverviewSleep", "Btn_MachineOverviewPower",
                "Btn_MachineOverviewRecover", "Btn_MachineOverviewRename"
            };
            for (int i = 0; i < unavailable.Length; i++)
            {
                Button button = FindButton(unavailable[i]);
                if (button != null)
                {
                    button.interactable = false;
                }
            }
        }

        private void BindButton(string buttonName, UnityEngine.Events.UnityAction action)
        {
            Button button = FindButton(buttonName);
            if (button == null)
            {
                return;
            }

            button.onClick.AddListener(action);
            Debug.Log($"[AutoEra][FieldHudDetail] 已绑定详情动作 {buttonName}");
        }

        private void OpenAlgorithmEditor()
        {
            Debug.Log("[AutoEra][FieldHudDetail] 点击算法：打开 AlgorithmEditorForm");
            AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm,
                new AutoEraUiPageRequest(AlgorithmEditorForm.PageEditor));
        }

        private void OpenDiagnosticsPage()
        {
            Debug.Log("[AutoEra][FieldHudDetail] 点击诊断：切换到机器诊断页");
            ShowFormPage(8);
        }

        private void OpenAlgorithmLibrary()
        {
            Debug.Log("[AutoEra][FieldHudDetail] 点击算法模板：打开 AlgorithmLibraryForm");
            AutoEraUiNavigator.Open(this, UIViews.AlgorithmLibraryForm);
        }

        private void OpenMachineHistory()
        {
            Debug.Log("[AutoEra][FieldHudDetail] 点击记录：打开 RecordReaderForm");
            AutoEraUiNavigator.Open(this, UIViews.RecordReaderForm,
                new AutoEraUiPageRequest(RecordReaderForm.PageMachineHistory));
        }

        private void OpenCropKnowledge()
        {
            Debug.Log("[AutoEra][FieldHudDetail] 点击知识：打开 CropKnowledgeForm");
            AutoEraUiNavigator.Open(this, UIViews.CropKnowledgeForm);
        }

        private void OpenHardwarePage()
        {
            Debug.Log("[AutoEra][FieldHudDetail] 点击硬件：切换到机器硬件页");
            ShowFormPage(6);
        }

        private void FocusSelectedObject()
        {
            if (SessionOrNull?.RegionInput != null && SessionOrNull.RegionInput.FocusSelection())
            {
                Debug.Log("[AutoEra][FieldHudDetail] 点击聚焦：已定位当前对象");
            }
            else
            {
                Debug.LogWarning("[AutoEra][FieldHudDetail] 点击聚焦：当前没有可定位对象");
            }
        }

        private void BindExitButton(string buttonName)
        {
            Button button = FindButton(buttonName);
            if (button == null)
            {
                Debug.LogWarning($"[AutoEra][FieldHudDetail] 找不到出口按钮 {buttonName}");
                return;
            }

            button.onClick.AddListener(CloseSelf);
            EnsureExitButtonLabel(button, buttonName == "Btn_FormBack" ? "返回" : "关闭");
            Debug.Log($"[AutoEra][FieldHudDetail] 已绑定出口按钮 {buttonName} serial={UIForm.SerialId}");
        }

        private static void EnsureExitButtonLabel(Button button, string text)
        {
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null)
            {
                GameObject labelObject = new GameObject("Txt_" + button.name + "Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(button.transform, false);
                label = labelObject.GetComponent<TextMeshProUGUI>();
            }

            RectTransform rect = label.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(4f, 2f);
            rect.offsetMax = new Vector2(-4f, -2f);
            label.text = text;
            label.fontSize = 18f;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }

        protected override void OnAutoEraOpen()
        {
            ReleaseRegionReadModel();
            _regionReadModel = RegionReadModels.Create(SessionOrNull);
            _regionReadModel.Changed += OnRegionReadModelChanged;
            ObserveProduction();
            EnsurePageRoots();
            Debug.Log($"[AutoEra][FieldHudDetail] OnOpen name={name} pageRoots={(_pageRoots == null ? -1 : _pageRoots.Length)} active={gameObject.activeInHierarchy}");
            // FieldHudForm 通过打开参数传入对象类型对应的规格页。首次打开时还没有
            // 机会调用 ShowSelectionPage，因此必须在生命周期入口消费请求；否则
            // Grp_PageHost 下的页面全部保持隐藏，表现为详情 Form 已生成但内容为空。
            if (TryGetRequest(out AutoEraUiPageRequest pageRequest))
            {
                bool shown = ShowFormPage(pageRequest.Page);
                Debug.Log($"[AutoEra][FieldHudDetail] request page={pageRequest.Page} shown={shown} current={CurrentPage}");
            }
            else
            {
                Debug.LogWarning("[AutoEra][FieldHudDetail] OnOpen 未收到 AutoEraUiPageRequest，页面保持默认状态。");
            }

            ApplyDefaultFocus(null, null);
            RenderCurrentPage();
        }

        protected override void OnAutoEraClose(bool isShutdown)
        {
            CloseAllSubUIForms();
            _machineOverviewFormId = 0;
            ReleaseRegionReadModel();
        }

        protected override void OnAutoEraRecycle()
        {
            CloseAllSubUIForms();
            _machineOverviewFormId = 0;
            ReleaseRegionReadModel();
            base.OnAutoEraRecycle();
        }

        private bool OpenMachineOverviewSubForm()
        {
            if (_machineOverviewFormId > 0 && (GF.UI.IsLoadingUIForm(_machineOverviewFormId) || GF.UI.HasUIForm(_machineOverviewFormId))) return true;
            _machineOverviewFormId = AutoEraUiNavigator.OpenSub(this, UIViews.FieldHudMachineOverviewForm, new FieldHudMachineOverviewForm.Request(this));
            return _machineOverviewFormId > 0;
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page)
        {
            EnsurePageRoots();
            if (IsMachineOverviewPage(page)) return OpenMachineOverviewSubForm();
            if (_machineOverviewFormId > 0) CloseSubUIForm(_machineOverviewFormId);
            bool shown = ShowPage(_pageRoots, page);
            GameObject activeRoot = page >= 0 && _pageRoots != null && page < _pageRoots.Length ? _pageRoots[page] : null;
            Debug.Log($"[AutoEra][FieldHudDetail] ShowFormPage page={page} shown={shown} roots={(_pageRoots == null ? -1 : _pageRoots.Length)} activeRoot={(activeRoot == null ? "<null>" : activeRoot.name)} activeSelf={(activeRoot != null && activeRoot.activeSelf)} activeInHierarchy={(activeRoot != null && activeRoot.activeInHierarchy)}");
            RenderCurrentPage();
            return shown;
        }
        public bool ShowSelectionPage(int page) => ShowFormPage(page);

        private bool IsMachineOverviewPage(int page)
        {
            GameObject pageRoot = page >= 0 && _pageRoots != null && page < _pageRoots.Length ? _pageRoots[page] : null;
            return page == 5 || (pageRoot != null && pageRoot.name == "Panel_PageMachineOverview");
        }

        private void EnsurePageRoots()
        {
            if (_pageRoots != null && _pageRoots.Length >= 22)
            {
                return;
            }

            Transform host = null;
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == "Grp_PageHost")
                {
                    host = transforms[i];
                    break;
                }
            }

            if (host == null)
            {
                Debug.LogError($"[AutoEra][FieldHudDetail] 找不到 Grp_PageHost，无法激活详情页。form={name}");
                return;
            }

            _pageRoots = new GameObject[22];
            string[] pageNames =
            {
                "Panel_PageMachineOverview", "Panel_PageMachineHardware", "Panel_PageMachineAlgorithm",
                "Panel_PageMachineDiagnostics", "Panel_PageFarm", "Panel_PageForest", "Panel_PageMineral",
                "Panel_PageWater", "Panel_PagePump", "Panel_PageBuildingOverview", "Panel_PageConstruction",
                "Panel_PageWarehouseBuilding", "Panel_PageGenerator", "Panel_PageSolar", "Panel_PageBattery",
                "Panel_PageConveyor", "Panel_PageSensorRecords"
            };
            for (int i = 0; i < pageNames.Length; i++)
            {
                Transform page = host.Find(pageNames[i]);
                if (page != null) _pageRoots[i + 5] = page.gameObject;
            }

            // 机器总览页由独立子 Form 按需承载，父壳保留空槽以维持稳定页索引。
            _pageRoots[5] = null;
            Debug.Log($"[AutoEra][FieldHudDetail] 从 Grp_PageHost 回填 pageRoots={_pageRoots.Length}");
        }

        private void OnRegionReadModelChanged(RegionDomainSection section)
        { if(CurrentPage==10 || CurrentPage==11) _productionDirty=true; else RenderCurrentPage(); }

        private void ReleaseRegionReadModel()
        {
            ReleaseProductionObservation();
            if (_regionReadModel == null) return;
            _regionReadModel.Changed -= OnRegionReadModelChanged;
            _regionReadModel.Dispose();
            _regionReadModel = null;
        }

        private void RenderCurrentPage()
        {
            if (_regionReadModel == null || CurrentPage < 6 || CurrentPage > 21 || CurrentPage == 5)
            {
                return;
            }

            GameObject pageRoot = _pageRoots != null && CurrentPage < _pageRoots.Length ? _pageRoots[CurrentPage] : null;
            if (pageRoot == null) return;
            if(CurrentPage==10 || CurrentPage==11)
            { _productionDirty=!RenderProductionPage(pageRoot,_regionReadModel.Snapshot); return; }

            string contentName = PrimaryContentNames.TryGetValue(CurrentPage, out string mapped)
                ? mapped
                : null;
            Transform content = FindPageTransform(pageRoot, contentName, "Content_");
            Transform template = FindPageTransform(pageRoot, null, "Item_");
            if (template == null || content == null) return;

            RectTransform contentRect = content as RectTransform;
            GameObject templateObject = template.gameObject;
            RegionDomainSnapshot snapshot = _regionReadModel.Snapshot;
            string pageKey = pageRoot.name.StartsWith("Panel_Page", StringComparison.Ordinal)
                ? pageRoot.name.Substring("Panel_Page".Length)
                : pageRoot.name;
            GameObject loading = FindPageObject(pageRoot, "Grp_" + pageKey + "LoadingState");
            GameObject empty = FindPageObject(pageRoot, "Grp_" + pageKey + "EmptyState");
            GameObject error = FindPageObject(pageRoot, "Grp_" + pageKey + "ErrorState");
            GameObject success = FindPageObject(pageRoot, "Grp_" + pageKey + "SuccessState");
            GameObject disabled = FindPageObject(pageRoot, "Grp_" + pageKey + "DisabledState");

            if (snapshot.State == UiDataState.Unavailable)
            {
                ShowPageUnavailable(snapshot.UnavailableReason, loading, empty, error, success, disabled);
                RenderDetailRows(templateObject, contentRect, NoFields);
                return;
            }

            bool hasSelection = snapshot.HasSelection;
            SetState(loading, false);
            SetState(error, false);
            SetState(disabled, false);
            SetState(success, false);
            SetState(empty, !hasSelection);
            if (hasSelection)
            {
                RenderDetailRows(templateObject, contentRect, snapshot.Detail);
            }
            else
            {
                RenderDetailRows(templateObject, contentRect, NoFields);
                WriteStateCard(empty, "请选择一个现场对象查看详情。");
            }
        }

        private Transform FindPageTransform(GameObject pageRoot, string exactName, string prefix)
        {
            if (!string.IsNullOrEmpty(exactName) && _transformCache.TryGetValue(exactName, out Transform exact) && exact.IsChildOf(pageRoot.transform))
            {
                return exact;
            }

            Transform[] transforms = pageRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (!string.IsNullOrEmpty(prefix) && transforms[i].name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return transforms[i];
                }
            }

            return null;
        }

        private static GameObject FindPageObject(GameObject pageRoot, string name)
        {
            Transform[] transforms = pageRoot.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == name) return transforms[i].gameObject;
            }

            return null;
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
