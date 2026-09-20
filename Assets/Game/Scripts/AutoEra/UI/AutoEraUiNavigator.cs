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

        /// <summary>
        /// 打开目标界面并把来源会话透传下去。
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
        {
            // 不传 allowEscape：让它保持 null，由 UIExtension 用 UITable 的 EscapeClose 兜底。
            // 传 false 等于写死「不能用返回键关闭」，会把每个界面的登记值压掉（实测踩过一次：
            // SaveSlotsForm 在表里是 true，却因为这里传了 false 而关不掉）。
            UIParams parameters = UIParams.Create();
            session?.WriteTo(parameters);
            if (request != null)
            {
                parameters.Set(AutoEraUiParamKeys.Request, request);
            }

            return GF.UI.OpenUIForm(view, parameters);
        }

        /// <summary>关闭指定界面（返回上一层）。传无效序列号安全无副作用。</summary>
        public static void Close(int serialId)
        {
            if (serialId > InvalidSerialId)
            {
                GF.UI.CloseUIForm(serialId);
            }
        }
    }
}
