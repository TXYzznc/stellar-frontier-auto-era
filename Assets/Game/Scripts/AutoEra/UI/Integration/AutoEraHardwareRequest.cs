using AutoEra.Machines;
using AutoEra.World.Identity;

namespace AutoEra.UI
{
    /// <summary>
    /// 硬件修改请求：交给 `OperationDialogForm` 的「硬件修改确认」页（规格 17-HardwareConfirm）。
    ///
    /// 它只描述**待确认的意图**，不做任何修改——真正的执行由 `MachineHardwareOperation` 在提交后负责
    /// （来源门禁、等待安全停机、失败回滚都在那里）。这样界面就不可能绕过确认直接改硬件，
    /// 也不可能自己实现一套「什么时候能改」的判断而与领域不一致。
    ///
    /// 刻意**不带来源（origin）**：来源由确认页按机器的部署状态推导（未部署 → 整备环境 Library，
    /// 已部署 → 现场 Field），因为那正是领域门禁的判据；让调用方传一个可能与机器状态矛盾的来源，
    /// 只会制造一个「界面说是整备、领域说该现场」的假分歧。
    ///
    /// 两种形态：**单槽**（装入／拆下某一格）与**一键卸下全部**（规格 05-机器整备
    /// `Btn_MachinePreparationUnload`「确认全部卸下影响并原子回库」）。后者没有槽位，
    /// 所以用 <see cref="AllSlots"/> 这个哨兵值表达，而不是随便填一个 0 号槽。
    /// </summary>
    public sealed class AutoEraHardwareRequest
    {
        /// <summary>「全部槽位」哨兵。**不是**一个合法槽位序号：合法序号从 0 开始。</summary>
        public const int AllSlots = -1;

        public AutoEraHardwareRequest(PersistentId machineId, HardwareKind kind, int slotIndex,
            PersistentId componentId, bool remove)
        {
            MachineId = machineId;
            Kind = kind;
            SlotIndex = slotIndex;
            ComponentId = componentId;
            Remove = remove;
            RemoveAll = slotIndex == AllSlots;
        }

        /// <summary>
        /// 一键卸下：把整台机器上装着的组件**原子**回库（全部成功或全不成功）。
        ///
        /// 入口是规格 05-机器整备的「一键卸下」；它只对未部署机器成立，
        /// 但这里**不自己判断部署状态**——那是领域门禁的事，界面照旧把意图交出去。
        /// </summary>
        public static AutoEraHardwareRequest UnloadAll(PersistentId machineId)
            => new AutoEraHardwareRequest(machineId, default, AllSlots, PersistentId.Invalid, remove: true);

        public PersistentId MachineId { get; }
        public HardwareKind Kind { get; }

        /// <summary>目标槽位序号（0 起）；一键卸下时为 <see cref="AllSlots"/>。</summary>
        public int SlotIndex { get; }

        /// <summary>要装上去的组件实例；**拆下时是 <see cref="PersistentId.Invalid"/>**。</summary>
        public PersistentId ComponentId { get; }

        /// <summary>true ＝ 拆下该槽位现有组件；false ＝ 把 <see cref="ComponentId"/> 装入该槽位。</summary>
        public bool Remove { get; }

        /// <summary>true ＝ 一键卸下这台机器上的全部组件（此时 <see cref="SlotIndex"/> 无意义）。</summary>
        public bool RemoveAll { get; }

        /// <summary>动机文字，用于确认页的一句总述。</summary>
        public string Intent => RemoveAll
            ? "一键卸下这台机器上装着的全部组件"
            : Remove
                ? "拆下 " + AutoEraUiFormat.Slot(Kind, SlotIndex) + " 上的组件"
                : "把组件装入 " + AutoEraUiFormat.Slot(Kind, SlotIndex);
    }
}
