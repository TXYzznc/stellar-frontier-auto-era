namespace AutoEra.UI
{
    /// <summary>
    /// 一个数据区域的呈现状态。
    ///
    /// <see cref="Unavailable"/> 是刻意保留的第三态：它把「该数据域的系统尚未接入」建模成
    /// 一个**正常、可展示**的状态，而不是异常，也不是伪造数据。依据界面通用合同：
    /// 无数据与零值区别显示；不存在对象显示失效页并禁用写操作；未发提交不伪造 success。
    ///
    /// 它对应契约里已经存在的区域状态组节点（<c>Grp_&lt;区域&gt;EmptyState</c> /
    /// <c>Grp_&lt;区域&gt;DisabledState</c>，默认 inactive、同区域只激活一个）。
    /// </summary>
    public enum UiDataState
    {
        /// <summary>数据就绪，显示真实内容。</summary>
        Ready,

        /// <summary>数据域可用但没有记录：显示空态说明。</summary>
        Empty,

        /// <summary>该数据域的系统尚未接入：显示 — 与具体原因，写操作禁用。</summary>
        Unavailable,
    }
}
