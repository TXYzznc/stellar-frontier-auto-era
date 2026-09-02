# 第 2.9 段：Motion 确定性与热路径回归

日期：2026-09-02

- 固定总时长不同分片的持续旋转得到等价姿态；重复中断恢复始终从当前测量 Pose 接续。
- 通道冲突、生命周期与释放后重试由 `MotionExecutorEditModeTests` 覆盖；中断策略由 `MotionInterruptionEditModeTests` 覆盖。
- 基础原语 10,000 次热循环在 EditMode 中无持续托管分配。
- QA job `f2edd874`：`MotionDeterminismEditModeTests` / EditMode 3/3 通过，失败／跳过／不确定均为 0，耗时 7 秒；普通编译 Console Error=0。
