using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>
    /// 选择类界面的请求与结果载体。
    ///
    /// 调用方 new 一个、连同会话一起放进打开参数，界面选择完成后把结果**写回同一个对象**，
    /// 调用方随后直接读它的属性即可——不需要回调注册，也就没有「回调在界面关闭后才到达」
    /// 这类时序问题。这是本工程里「界面把结果交回调用方」的既定做法。
    ///
    /// 三类结果互斥：<see cref="Confirmed"/>（选定了某个对象）、<see cref="Cancelled"/>
    /// （玩家取消）、以及两者都不是（界面被外部关掉，调用方应按未决处理）。
    /// </summary>
    public sealed class AutoEraUiSelectionRequest
    {
        public AutoEraUiSelectionRequest(bool includeMachines = true, bool includeSites = true)
        {
            IncludeMachines = includeMachines;
            IncludeSites = includeSites;
        }

        /// <summary>候选是否包含机器。</summary>
        public bool IncludeMachines { get; }

        /// <summary>候选是否包含建筑与资源点。</summary>
        public bool IncludeSites { get; }

        /// <summary>玩家确认选中的对象；未确认时为无效 Id。</summary>
        public PersistentId SelectedId { get; private set; } = PersistentId.Invalid;

        /// <summary>玩家是否确认了选择。</summary>
        public bool Confirmed { get; private set; }

        /// <summary>玩家是否取消了选择。</summary>
        public bool Cancelled { get; private set; }

        /// <summary>是否还在等待玩家决定（既没确认也没取消）。</summary>
        public bool IsPending => !Confirmed && !Cancelled;

        public void Confirm(PersistentId id)
        {
            SelectedId = id;
            Confirmed = id.IsValid;
            Cancelled = false;
        }

        public void Cancel()
        {
            SelectedId = PersistentId.Invalid;
            Confirmed = false;
            Cancelled = true;
        }

        /// <summary>候选过滤：界面用它决定哪些区域对象可以出现在列表里。</summary>
        public bool Accepts(UiRegionObjectRow row) => row.IsMachine ? IncludeMachines : IncludeSites;
    }
}
