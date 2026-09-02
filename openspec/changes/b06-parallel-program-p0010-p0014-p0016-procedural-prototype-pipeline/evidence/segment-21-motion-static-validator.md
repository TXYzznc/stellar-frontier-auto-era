# 第 3.3 段：Motion 静态校验入口

日期：2026-09-02

- `MotionStaticValidator` 汇总 Rig 和 Graph 校验，并检测 Graph 节点对不存在 Rig 关节的引用。
- 静态入口复用既有 Rig 范围／重复 ID 与 Graph 参数／节点／连接验证，便于在导出或预览前阻止错误。
- 普通编译 0 Error；QA job `8923ce66`，`MotionStaticValidatorEditModeTests` / EditMode 1/1 通过。
