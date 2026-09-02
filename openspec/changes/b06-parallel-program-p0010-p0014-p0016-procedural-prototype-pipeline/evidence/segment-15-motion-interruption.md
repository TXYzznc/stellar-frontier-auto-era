# 第 2.6 段：Motion 中断策略

日期：2026-09-02

- 支持 Hold、Retract、Reset、ImmediateStop。Recover／Reset 统一从当前实测局部 Pose 插值，而非从历史目标继续，避免中断反复后累计漂移。
- Hold 与 ImmediateStop 保持测量姿态；Retract 走安全姿态；Reset 回绑定姿态。
- 普通编译 0 Error；QA job `fc1d29f1`，`MotionInterruptionEditModeTests` 2/2 通过。
