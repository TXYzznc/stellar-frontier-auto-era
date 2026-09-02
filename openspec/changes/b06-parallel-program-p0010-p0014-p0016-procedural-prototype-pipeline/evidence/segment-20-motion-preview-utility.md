# 第 3.2 段：Motion 预览与恢复工具

日期：2026-09-02

- 选中 MotionRig 后可通过固定 Editor 菜单一键恢复所有绑定姿态；操作使用 Undo，不保存额外运行时状态。
- 选中 Rig 显示每个关节的局部轴 Gizmo，支持范围／锚点调试的基础可视上下文。
- 普通编译 0 Error；QA job `1e68582c`，`MotionRigPreviewUtilityEditModeTests` / EditMode 1/1 通过。
