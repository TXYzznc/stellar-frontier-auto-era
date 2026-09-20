namespace AutoEra.UI
{
    /// <summary>
    /// 打开「存档恢复」界面时指定要处理的槽位（经 <see cref="AutoEraUiParamKeys.Request"/> 传入）。
    ///
    /// 与 <see cref="AutoEraUiPageRequest"/> 分开：一个界面可能既要指定页、又要指定对象，
    /// 把「第几页」和「哪个槽位」塞进同一个类型会让调用点变得猜谜。
    /// </summary>
    public sealed class AutoEraUiSaveSlotRequest
    {
        public AutoEraUiSaveSlotRequest(int slotIndex)
        {
            SlotIndex = slotIndex;
        }

        /// <summary>目标槽位索引；无效值（负数）表示「没有指定槽位」。</summary>
        public int SlotIndex { get; }
    }
}
