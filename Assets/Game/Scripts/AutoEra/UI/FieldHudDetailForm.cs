using AutoEra.UI.Contracts;
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
        /// <summary>详情侧栏是现场 HUD 的扩展面板，不应暂停镜头移动或世界选取。</summary>
        public override bool BlocksWorldInput => false;

        public RectTransform BuildingOverviewIdentityContent => FindRect("Content_BuildingOverviewIdentity");
        public Button FarmRecordButton => FindButton("Btn_FarmRecord");
        public Button MachineOverviewDiagnosticButton => FindButton("Btn_MachineOverviewDiagnostic");
        public Button MachineOverviewFocusButton => FindButton("Btn_MachineOverviewFocus");

        private Button FindButton(string name)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++) if (buttons[i].name == name) return buttons[i];
            return null;
        }

        private RectTransform FindRect(string name)
        {
            RectTransform[] rects = GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++) if (rects[i].name == name) return rects[i];
            return null;
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
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
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page)
        {
            EnsurePageRoots();
            bool shown = ShowPage(_pageRoots, page);
            GameObject activeRoot = page >= 0 && _pageRoots != null && page < _pageRoots.Length ? _pageRoots[page] : null;
            Debug.Log($"[AutoEra][FieldHudDetail] ShowFormPage page={page} shown={shown} roots={(_pageRoots == null ? -1 : _pageRoots.Length)} activeRoot={(activeRoot == null ? "<null>" : activeRoot.name)} activeSelf={(activeRoot != null && activeRoot.activeSelf)} activeInHierarchy={(activeRoot != null && activeRoot.activeInHierarchy)}");
            return shown;
        }
        public bool ShowSelectionPage(int page) => ShowFormPage(page);

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

            // 规格页序从 5 开始；Grp_PageHost 的最后一个子节点是关闭按钮，不能当页面。
            List<GameObject> pageChildren = new List<GameObject>(host.childCount);
            for (int i = 0; i < host.childCount; i++)
            {
                Transform child = host.GetChild(i);
                if (child != null && child.gameObject != null && child.name != "Btn_FieldClose")
                {
                    pageChildren.Add(child.gameObject);
                }
            }

            _pageRoots = new GameObject[22];
            for (int i = 0; i < pageChildren.Count && i + 5 < _pageRoots.Length; i++)
            {
                _pageRoots[i + 5] = pageChildren[i];
            }

            Debug.Log($"[AutoEra][FieldHudDetail] 从 Grp_PageHost 回填 pageRoots={_pageRoots.Length} pageChildren={pageChildren.Count}");
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}

