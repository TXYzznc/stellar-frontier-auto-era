# 第 4.5 段：门与传送带表现

日期：2026-09-02

- `SlidingDoorPresentation` 覆盖单扇／双扇开闭；安全占用期间暂停关闭并回弹到安全全开位置，解除占用后可按请求恢复。
- `ConveyorPresentation` 以效率系数计算循环表现速度；堵塞时准确保留当前 UV 偏移，解除堵塞后从该位置连续恢复。
- QA job `1b2d68d6`：`DoorAndConveyorPresentationEditModeTests` / EditMode 2/2 通过，失败 0；结束时 Unity 非 PlayMode、未编译、无更新或域重载待处理。
