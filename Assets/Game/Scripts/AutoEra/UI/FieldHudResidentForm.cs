using AutoEra.UI.Contracts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// FieldHudResidentForm 的 GF 桥。结构由 Docs/Development/UI-PrefabLayouts/FieldHudResidentForm.contract.json 生成
    /// （设计来源：Docs/GameDesign/03-玩家体验/界面规格）。
    ///
    /// 绑定字段在同名的 FieldHudResidentForm.Fields.cs 里（同一 partial 类）；本文件只有类逻辑：
    /// 规格页序切换、取消意图与默认焦点。Grp_PageHost 下的内容页顺序即规格页序。
    /// </summary>
    public sealed partial class FieldHudResidentForm : AutoEraShellFormBase
    {
        /// <summary>
        /// 常驻 HUD 只承载导航和状态展示，不应锁住现场移动或世界对象交互。
        /// 详情侧栏仍由 FieldHudDetailForm 自己声明是否阻断世界输入。
        /// </summary>
        public override bool BlocksWorldInput => false;

        public Button HudHubButton => FindButton("Btn_HudNavigationHub");
        public Button HudMachinesButton => FindButton("Btn_HudNavigationMachines");

        private readonly Dictionary<string, Button> _buttonCache = new Dictionary<string, Button>(16, StringComparer.Ordinal);

        private Button FindButton(string name)
        {
            _buttonCache.TryGetValue(name, out Button button);
            return button;
        }

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null) _buttonCache[buttons[i].name] = buttons[i];
            }
            EnsurePageRoots();
        }

        protected override void OnAutoEraOpen()
        {
            EnsurePageRoots();
            ApplyDefaultFocus(null, null);
        }

        /// <summary>按规格页序切换内容页；越界调用无副作用。</summary>
        public bool ShowFormPage(int page)
        {
            EnsurePageRoots();
            return ShowPage(_pageRoots, page);
        }
        public void ShowWorldTime(long worldMilliseconds) { }

        private void EnsurePageRoots()
        {
            if (_pageRoots != null && _pageRoots.Length >= 5)
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
                Debug.LogError($"[AutoEra][FieldHudResident] 找不到 Grp_PageHost，无法激活 HUD 内容页。form={name}");
                return;
            }

            _pageRoots = new GameObject[host.childCount];
            for (int i = 0; i < host.childCount; i++)
            {
                _pageRoots[i] = host.GetChild(i).gameObject;
            }
        }

        protected override void OnOperationPresentationChanged(
            AutoEraUiOperationSnapshot snapshot, AutoEraUiOperationPresentation presentation) { }
    }
}
