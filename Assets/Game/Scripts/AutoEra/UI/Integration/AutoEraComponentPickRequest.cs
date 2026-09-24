using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>
    /// 「为某个槽位挑一件组件」的请求（规格 12-选择与绑定 · ComponentPicker）。
    ///
    /// 它描述**目标槽位**，不描述要挑哪一件——挑哪一件是玩家在页面里做的决定。
    /// 候选来自组件库（花名册里的散件），这一层不碰库存、价格或经济域。
    ///
    /// 结果写回同一个对象上的 <see cref="Result"/>（本工程「界面把结果交回调用方」的既定做法：
    /// 调用方 new 一个、随打开参数传进来，界面选完直接写回，不需要回调注册，
    /// 也就没有「回调在界面关闭后才到达」这类时序问题）。
    ///
    /// 确认后组件选择器**还会自己打开 17-硬件修改确认**（规格：装配模式返回候选ID并进入17硬件确认），
    /// 因为只有那里能提交给领域执行；选择器自己不安装任何东西。
    /// </summary>
    public sealed class AutoEraComponentPickRequest
    {
        public AutoEraComponentPickRequest(PersistentId machineId, HardwareKind kind, int slotIndex,
            AutoEraUiSelectionRequest result = null)
        {
            MachineId = machineId;
            Kind = kind;
            SlotIndex = slotIndex;
            // 调用方没给结果对象就自己建一个：这样「有没有人来读结果」与「结果能不能写」
            // 是两件事——不传结果的调用方（例如只关心导航）也不会让确认静默地把结果丢掉。
            Result = result ?? new AutoEraUiSelectionRequest();
        }

        public PersistentId MachineId { get; }
        public HardwareKind Kind { get; }

        /// <summary>目标槽位序号（0 起）。</summary>
        public int SlotIndex { get; }

        /// <summary>结果写回处；调用方不传时这里会自己建一个，读 <see cref="SelectedId"/> 即可。</summary>
        public AutoEraUiSelectionRequest Result { get; }

        /// <summary>玩家确认的候选组件；未确认时为无效 Id。</summary>
        public PersistentId SelectedId => Result.SelectedId;

        /// <summary>玩家是否确认了候选。</summary>
        public bool Confirmed => Result.Confirmed;

        /// <summary>玩家是否取消了。</summary>
        public bool Cancelled => Result.Cancelled;

        /// <summary>动机文字，用于页面标题与一句总述。</summary>
        public string Intent => "为 " + AutoEraUiFormat.Slot(Kind, SlotIndex) + " 选择要装入的组件";
    }
}
