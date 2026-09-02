# 第 2.5 段：集中 MotionExecutor

日期：2026-09-02

- `MotionExecutor` 将可变生命周期和关节占用集中在单 Rig 实例；Rig 只保存静态绑定。
- 执行申请在准备阶段一次性取得全部所需稳定关节 ID，冲突立即拒绝；生命周期显式约束 Prepared、Running、Completed、Cancelled、Recovering；释放后才可由新执行申请。
- 普通编译 0 Error；QA job `1304d34b`，`MotionExecutorEditModeTests` 1/1 通过。
