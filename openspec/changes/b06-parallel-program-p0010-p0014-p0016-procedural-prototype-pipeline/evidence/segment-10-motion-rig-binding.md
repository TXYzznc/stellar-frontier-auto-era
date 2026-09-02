# 第 2.1 段：MotionRig 绑定合同

日期：2026-09-02

## 交付

- 新增 `MotionRig` 和显式 `MotionJointBinding`；每项绑定声明稳定关节 ID、Transform、旋转或平移通道、局部轴、范围、绑定姿态与安全姿态。
- Rig 不进行场景查找、不保存动作图或玩法状态；它只提供稳定 ID 到 Transform 的显式查询与结构校验，为后续单 Rig 执行器保留边界。
- 校验拒绝空绑定、空／重复稳定 ID、缺失 Transform、零轴及反向范围。

## 验证

- 普通 Unity 刷新编译后：`isCompiling=false`、`isUpdating=false`，Console Error 为 0。
- QA 队列任务 `b06-motion-rig-editmode` 已完成；job `78990a9b` 运行 `AutoEra.Tests.Editor.MotionRigEditModeTests`（EditMode），总计 2，通过 2，失败 0，跳过 0，不确定 0，耗时 7 秒。
- 测试覆盖稳定 ID 查询、绑定／安全姿态保存，以及重复 ID 与非法范围拒绝。
