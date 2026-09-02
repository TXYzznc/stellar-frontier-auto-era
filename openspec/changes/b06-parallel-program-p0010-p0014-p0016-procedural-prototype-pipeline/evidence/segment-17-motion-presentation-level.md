# 第 2.8 段：Motion 表现更新等级

日期：2026-09-02

- 定义 Near、Mid、Far、Invisible 四档表现更新策略，只给出是否评估与最小表现采样间隔；没有玩法、碰撞、导航或持久化写入入口。
- 普通编译 0 Error；QA job `cbb97aa8`，`MotionPresentationUpdateLevelEditModeTests` / EditMode 1/1 通过，失败／跳过／不确定均为 0。短暂服务重连后测试完成。
