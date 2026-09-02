# 第 2.7 段：参数上下文与 Adapter 边界

日期：2026-09-02

- `MotionParameterContext` 提供布尔、整数、浮点、Vector3 和 Quaternion 强类型参数；目标 Pose 与环境输入使用窄接口。
- `AutoEraMotionParameterAdapter` 仅把权威展示输入写为参数，不持有 Transform、MotionRig 或执行器引用，不能直接操作关节或决定玩法结果。
- 普通编译 0 Error；QA job `8caad71f`，`AutoEraMotionParameterAdapterEditModeTests` / EditMode 1/1 通过，失败／跳过／不确定均为 0。轮询期间短暂编译／服务重连后正常恢复。
