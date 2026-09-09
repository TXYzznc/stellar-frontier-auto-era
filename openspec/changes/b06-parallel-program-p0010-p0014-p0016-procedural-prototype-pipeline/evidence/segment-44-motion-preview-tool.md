# 第44段：统一动作预览工具

日期：2026-09-03

## 实现

- `MotionGraphAsset`支持受限的正式动作引用，并按Rig绑定检查兼容性和循环引用。
- `MotionPreviewEvaluator`只驱动声明关节，可恢复绑定姿态；不读写任务、效应器队列或玩法状态。
- 新增Editor“动作预览”窗口，支持选择`MotionRig`、列出兼容动作、播放、暂停、安全中断、重置、加入临时组合并保存为正式`MotionGraphAsset`。
- 保存的组合图仅包含稳定动作资产引用和既有受限组合节点；不记录场景对象、Transform、目标、任务或运行状态。

## 验证

- QA job `7c1876d5`：`MotionPreviewEvaluatorEditModeTests` 1/1通过。
- QA job `1cc6d0d0`：`MotionGraphAssetEditModeTests` 2/2通过。
- 合计3/3通过，失败0；Unity 2022.3.62f3c1为Bypass、非PlayMode、未编译、未更新、无域重载待处理。
