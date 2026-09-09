# Segment 55：传送带滚筒弧长同步

## 冻结关系

- 履带线速度固定为`0.9 m/s`。
- 主动／从动滚筒半径为`0.25 m`，四个支承滚筒半径为`0.14 m`。
- 每一滚筒的角位移由`角度 = 时间 × 履带线速度 ÷ 滚筒半径`求得；回推时`半径 × 角位移（弧度）`必须等于同一时段的履带移动距离。
- 因而支承滚筒比端滚筒转得更快，但接触面的线速度相同；不再对所有滚筒套用同一角速度。

## QA

- `ConveyorLoopPresentationEditModeTests` — job `7f15d73f`，2/2。
- `FunctionalRigAcceptanceDemoDirectorEditModeTests` — job `ea259bf0`，1/1。
- `FunctionalRigMotionGraphCatalogEditModeTests` — job `ce7d2125`，13/13。

合计16/16通过，失败0。结束时 Unity 8090 为Bypass、非PlayMode、未编译、未更新；Console 0 Warning、0 Error。
