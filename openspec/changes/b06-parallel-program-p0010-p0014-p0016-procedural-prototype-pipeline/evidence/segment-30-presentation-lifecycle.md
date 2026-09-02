# 第 4.7 段：表现复用生命周期

日期：2026-09-02

- `MotionPresentationLeasePool` 在释放时清空原型标识、进度和更新等级；再次取得时恢复 Near 级绑定基线并递增播放轮次。
- 该边界覆盖对象池复用、重进场景及连续两轮播放，避免进度或 LOD 状态残留与累积漂移。
- QA job `84159e94`：`MotionPresentationLeasePoolEditModeTests` / EditMode 2/2 通过，失败 0；结束时 Unity 非 PlayMode、未编译、无更新或域重载待处理。
