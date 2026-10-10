using System.Collections.Generic;
using GameFramework;
using UnityGameFramework.Runtime;

namespace AutoEra.UI
{
    /// <summary>
    /// 界面导航：按 Docs/GameDesign/03-玩家体验/界面规格/00-页面关系与复用.md 的入口/返回表
    /// 打开目标界面。
    ///
    /// 职责只有两条，但都是「数据来源唯一」的必要条件：
    /// ① 把来源界面的 <see cref="AutoEraUiSession"/> 透传给目标界面——子界面拿数据的路
    ///    仍然是打开参数，而不是自己去某个全局对象里解析服务；
    /// ② 统一装配打开参数（请求对象见 <see cref="AutoEraUiParamKeys.Request"/>）。
    ///
    /// 覆盖与恢复**不在这里实现**：GF 的 UIGroup 在 Refresh 时按上层 Form 的
    /// <c>PauseCoveredUI</c>（UITable 的 PauseCoveredUI 列）自动 Cover/Pause 下层界面，
    /// 关闭时自动 Resume，所以来源界面的分页、滚动与选择天然保留；触发按钮的焦点由
    /// <see cref="AutoEraUiFormBase"/> 在 OnOpen 记忆、OnClose 恢复。
    ///
    /// 返回 = 目标界面自己关闭（Btn_FormBack / Btn_FormClose / Escape），不经过本类。
    /// </summary>
    public static class AutoEraUiNavigator
    {
        /// <summary>无效的界面序列号，与 GF 的失败返回值一致。</summary>
        public const int InvalidSerialId = 0;
        private static readonly Dictionary<int, PendingOpen> Pending = new Dictionary<int, PendingOpen>();
        private static readonly List<int> Finished = new List<int>();

        private sealed class PendingOpen
        {
            private readonly UIParams _parameters;
            private bool _released;
            internal bool Transferred;
            internal PendingOpen(UIParams parameters) { _parameters = parameters; }
            internal void OnOpened(UIFormLogic form)
            {
                Transferred = true;
                Pending.Remove(form.UIForm.SerialId);
            }
            internal void Release()
            {
                if (Transferred || _released) return;
                _released = true;
                ReferencePool.Release(_parameters);
            }
        }

        /// <summary>
        /// 打开独立目标界面并把来源会话透传下去。
        ///
        /// 不遮挡来源界面的视觉子层必须使用 <see cref="OpenSub"/>，让 GF 为其分配
        /// 相对 Canvas 排序；本方法保留给独立顶层界面或会暂停/遮挡来源的界面。
        ///
        /// 来源没有会话时目标界面照常打开——它会呈现「不可用」态并说明原因，
        /// 这比拒绝打开更容易被发现（界面在，但明说数据来源缺失）。
        /// </summary>
        public static int Open(AutoEraUiFormBase source, UIViews view, object request = null)
        {
            return Open(view, source != null ? source.SessionOrNull : null, request);
        }

        /// <summary>供流程等非界面调用方使用：显式给出会话。</summary>
        public static int Open(UIViews view, AutoEraUiSession session, object request = null)
        { return OpenInternal(view, session, request, null, 0); }

        /// <summary>Preserves GF child ordering/ownership while tracking parameters until a cold child actually opens.</summary>
        public static int OpenSub(AutoEraUiFormBase parent, UIViews view, object request = null, int subUiOrder = 0)
        {
            if (parent == null || GF.UI == null || !GF.UI.HasUIForm(parent.Id)) return InvalidSerialId;
            return OpenInternal(view, parent.SessionOrNull, request, parent, subUiOrder);
        }

        private static int OpenInternal(UIViews view, AutoEraUiSession session, object request, AutoEraUiFormBase parent, int subUiOrder)
        {
            var ui = GF.UI;
            if (ui == null) return InvalidSerialId;
            // 不传 allowEscape：让它保持 null，由 UIExtension 用 UITable 的 EscapeClose 兜底。
            // 传 false 等于写死「不能用返回键关闭」，会把每个界面的登记值压掉（实测踩过一次：
            // SaveSlotsForm 在表里是 true，却因为这里传了 false 而关不掉）。
            UIParams parameters = UIParams.Create();
            var ownership = new PendingOpen(parameters);
            parameters.OpenCallback = ownership.OnOpened;
            session?.WriteTo(parameters);
            if (request != null)
            {
                parameters.Set(AutoEraUiParamKeys.Request, request);
            }

            try
            {
                int serialId = parent == null ? ui.OpenUIForm(view, parameters) : parent.OpenSubUIForm(view, subUiOrder, parameters);
                if (serialId <= InvalidSerialId)
                {
                    ownership.Release();
                    return InvalidSerialId;
                }
                if (!ownership.Transferred && ui.IsLoadingUIForm(serialId))
                {
                    Pending.Add(serialId, ownership);
                    AutoEraUiRuntime.TrackPendingNavigation();
                }
                else if (!ownership.Transferred && !ui.HasUIForm(serialId)) ownership.Release();
                return serialId;
            }
            catch
            {
                ownership.Release();
                throw;
            }
        }

        /// <summary>关闭指定界面（返回上一层）。传无效序列号安全无副作用。</summary>
        public static void Close(int serialId)
        {
            // 选择变化、详情按钮和 UIGroup 自动回收可能交错发生；旧序列号已经不存在时，
            // 关闭请求必须是幂等的，不能把正常换选对象升级成 GameFrameworkException。
            if (serialId > InvalidSerialId && GF.UI != null)
            {
                if (GF.UI.IsLoadingUIForm(serialId))
                {
                    // GF cancels the original serial before releasing our parameters; no late OnOpen can consume them.
                    GF.UI.CloseUIForm(serialId);
                    if (Pending.TryGetValue(serialId, out var ownership))
                    {
                        Pending.Remove(serialId);
                        ownership.Release();
                    }
                }
                else if (GF.UI.HasUIForm(serialId)) GF.UI.CloseUIForm(serialId);
            }
        }

        internal static void PollPendingNavigation()
        {
            if (Pending.Count == 0) return;
            var ui = GF.UI;
            Finished.Clear();
            foreach (var pair in Pending)
            {
                if (ui != null && ui.IsLoadingUIForm(pair.Key)) continue;
                if (ui != null && ui.HasUIForm(pair.Key)) pair.Value.Transferred = true;
                Finished.Add(pair.Key);
            }
            foreach (int id in Finished)
            {
                var ownership = Pending[id];
                Pending.Remove(id);
                ownership.Release();
            }
            Finished.Clear();
        }

        internal static void ReleasePendingNavigation()
        {
            var ui = GF.UI;
            foreach (var pair in Pending)
            {
                if (ui != null && ui.IsLoadingUIForm(pair.Key)) ui.CloseUIForm(pair.Key);
                else if (ui != null && ui.HasUIForm(pair.Key)) pair.Value.Transferred = true;
                pair.Value.Release();
            }
            Pending.Clear();
            Finished.Clear();
        }
    }
}
