using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AutoEra.UI
{
    /// <summary>
    /// 管理／模态外壳页面的共享行为：页面切换、默认焦点、状态组与详情行的渲染辅助。
    ///
    /// 刻意**不持有 SerializeField**：每个 Form 的结构差异（有没有导航、有没有返回）
    /// 由各自的契约声明，绑定字段必须留在具体 Form 上，这样 L3 的「每个 SerializeField
    /// 非空且路径等于契约」才能逐页判定，而不会因为基类字段在本页不存在而误判。
    /// </summary>
    public abstract class AutoEraShellFormBase : AutoEraUiFormBase
    {
        private int _currentPage = -1;

        /// <summary>当前激活页序号；-1 表示尚未选择页面。</summary>
        public int CurrentPage => _currentPage;

        /// <summary>
        /// 同一时刻只激活一个页面根。越界或未变化的调用不产生副作用，返回 false。
        /// </summary>
        protected bool ShowPage(GameObject[] pageRoots, int index)
        {
            if (pageRoots == null || index < 0 || index >= pageRoots.Length)
            {
                return false;
            }

            if (_currentPage == index && pageRoots[index] != null && pageRoots[index].activeSelf)
            {
                return true;
            }

            for (int i = 0; i < pageRoots.Length; i++)
            {
                if (pageRoots[i] != null)
                {
                    pageRoots[i].SetActive(i == index);
                }
            }

            _currentPage = index;
            return true;
        }

        /// <summary>
        /// 首焦点优先落在安全取消／返回，其次才是传入的页面首个可用交互。
        /// </summary>
        protected void ApplyDefaultFocus(GameObject safeCancel, GameObject firstInteractable)
        {
            GameObject target = IsFocusable(safeCancel) ? safeCancel : firstInteractable;
            if (IsFocusable(target) && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(target);
            }
        }

        /// <summary>
        /// 同一区域只激活一个状态组（规格 06 的区域级状态通道）。
        /// 调用方对每个状态组各调一次，传入的条件互斥，因此不需要额外的状态枚举。
        /// </summary>
        protected static void SetState(GameObject state, bool active)
        {
            if (state != null && state.activeSelf != active)
            {
                state.SetActive(active);
            }
        }

        /// <summary>
        /// 用对象池渲染一列条目：先清空该模板的池，再按 count 逐个 Spawn 并交给 binder 绑定。
        ///
        /// 所有列表都必须走这一条路（`SpawnItem` + `Item_*Template`）：现场与枢纽的列表会被频繁重建，
        /// 直接 Instantiate/Destroy 会持续制造 GC 峰值。集中在这里还避免某处忘记先清空池——
        /// 那会让旧行与新行叠加显示。
        ///
        /// count 为 0 时**仍然会清空**：「这次没有数据」同样要把上一次的行收干净。
        /// binder 里给条目传 null 回调即表示纯展示行（其按钮会被禁用，避免只读内容抢焦点）。
        /// </summary>
        protected int RenderListRows(GameObject template, RectTransform content, int count,
            System.Action<int, UiListRowItem> binder)
        {
            if (template == null || content == null)
            {
                return 0;
            }

            UnspawnAllItem<UiListRowItem>(template);
            for (int i = 0; i < count; i++)
            {
                UiListRowItem item = SpawnItem<UiListRowItem>(template, content);
                binder?.Invoke(i, item);
            }

            return count;
        }

        /// <summary>
        /// 把详情字段渲染成 Item 行。纯展示内容传入 null 回调即禁用其按钮，
        /// 避免只读行抢走焦点。多页 Form 的「元数据／健康／详情」栏都走这一条路，因此放在基类。
        /// </summary>
        protected void RenderDetailRows(GameObject template, RectTransform content, IReadOnlyList<UiDetailField> fields)
        {
            if (template == null || content == null || fields == null)
            {
                return;
            }

            RenderListRows(template, content, fields.Count,
                (index, item) => item.Bind(index, fields[index].Label, fields[index].Value, null));
        }

        /// <summary>
        /// 把一个「整页不可用」的原因渲染到该页的状态组与说明文本上。
        ///
        /// 用于「该域尚未接入」这类整页级状态：规格要求页面读错／无权限态可以覆盖内容但保留关闭，
        /// 且状态必须能解释自己。参数顺序固定为该页契约的五状态组。
        /// </summary>
        protected void ShowPageUnavailable(
            string reason,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState,
            params TMPro.TMP_Text[] bodies)
        {
            SetState(loadingState, false);
            SetState(emptyState, false);
            SetState(errorState, false);
            SetState(successState, false);
            SetState(disabledState, true);
            WriteStateCard(disabledState, reason);

            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] != null)
                {
                    bodies[i].SetText(reason);
                }
            }
        }

        /// <summary>
        /// 把原因写进**正在激活的那个状态组自己的说明卡片**。
        ///
        /// 为什么必须有这一步：五个 `Grp_*State` 是**覆盖在内容区上的不透明卡片**
        /// （520×120 居中，Image 的 alpha 为 1），而预制体里卡片的文案是规格说明列的占位
        /// （「Disabled：—」「Empty：—」）。只激活状态组就会把一句占位话盖在真实原因上——
        /// 玩家看到的是「Disabled：—」，而不是「区域没有加载」。写进 bodies 的那份原因
        /// 正好被卡片挡住，等于没写。
        ///
        /// 按**结构**找卡片文本、不按绑定找：卡片文案不是页面的业务接入点，
        /// 给 33 个 Form 各加五个 SerializeField 只为读一句状态说明，代价远大于收益。
        /// 这一条与 <see cref="DisableDomainActions"/> 同源——都靠结构名／结构位置，
        /// 因此对尚未接线的页面同样生效。
        /// </summary>
        protected static void WriteStateCard(GameObject state, string reason)
        {
            if (state == null || string.IsNullOrEmpty(reason))
            {
                return;
            }

            TMPro.TMP_Text[] texts = state.GetComponentsInChildren<TMPro.TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    texts[i].SetText(reason);
                }
            }
        }

        /// <summary>
        /// 把「本域已经接线，但当前没有内容」的原因渲染到状态组与说明文本上。
        ///
        /// 与 <see cref="ShowPageUnavailable"/> 的区别只在状态通道：这里是 **Empty**，不是 Disabled。
        /// 这个区别对玩家和排查的人都有意义——「域没接线」意味着怎么点都不会有数据，
        /// 「域接线了但这次没有内容」意味着换一台机器／建一个对象就会出现。
        /// 把它们合成一个状态，界面就是在撒谎。
        /// </summary>
        protected void ShowPageEmpty(
            string reason,
            GameObject loadingState,
            GameObject emptyState,
            GameObject errorState,
            GameObject successState,
            GameObject disabledState,
            params TMPro.TMP_Text[] bodies)
        {
            SetState(loadingState, false);
            SetState(emptyState, true);
            SetState(errorState, false);
            SetState(successState, false);
            SetState(disabledState, false);
            WriteStateCard(emptyState, reason);

            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] != null)
                {
                    bodies[i].SetText(reason);
                }
            }
        }

        /// <summary>
        /// 禁用本页所有业务动作，只保留安全出口（返回／关闭）与顶栏页导航。
        ///
        /// 用于「整个域尚未接入」的整页状态：与其为该域逐个声明并禁用几十个按钮
        /// （那要上百个绑定，且每个新按钮都要记得加），不如把除出口以外的入口全部关掉——
        /// 反正这个域当前什么都做不了。判据取自结构名（00-共享外壳的出口与 Grp_Navigation），
        /// 不依赖任何绑定字段，因此对尚未接入的界面也能生效。
        /// </summary>
        protected void DisableDomainActions()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null || IsSafeExit(button) || IsUnderNavigation(button) || IsInsideItemTemplate(button))
                {
                    continue;
                }

                button.interactable = false;
            }
        }

        /// <summary>
        /// Item 模板里的按钮是**数据行**，不是本域的工具栏动作。
        /// 它们由 Item 逻辑按数据启用/禁用（例如详情行传 null 回调即禁用），整域禁用不该波及——
        /// 否则「浏览只读记录」这类只需要点行的页面会变得完全不可交互。
        /// </summary>
        private static bool IsInsideItemTemplate(Button button)
        {
            for (Transform current = button.transform; current != null; current = current.parent)
            {
                if (current.name.StartsWith("Item_", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 安全出口：返回、关闭，以及任何「取消」按钮。
        /// 取消在语义上永远是安全动作（Abort 不会改世界状态），所以它不该被整域禁用波及——
        /// 否则一个尚未接入的域会把玩家关在里面。
        /// </summary>
        private static bool IsSafeExit(Button button) =>
            button.name == "Btn_FormBack"
            || button.name == "Btn_FormClose"
            || button.name.EndsWith("Cancel", System.StringComparison.Ordinal);

        private static bool IsUnderNavigation(Button button)
        {
            for (Transform current = button.transform.parent; current != null; current = current.parent)
            {
                if (current.name.StartsWith("Grp_Navigation", System.StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsFocusable(GameObject candidate)
        {
            if (candidate == null || !candidate.activeInHierarchy)
            {
                return false;
            }

            var selectable = candidate.GetComponent<Selectable>();
            return selectable != null && selectable.IsInteractable();
        }

        protected override void OnAutoEraRecycle()
        {
            _currentPage = -1;
            base.OnAutoEraRecycle();
        }
    }
}
