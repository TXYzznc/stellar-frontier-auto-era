# 第 3.1 段：Motion Inspector 配置与错误定位

日期：2026-09-02

- `MotionRig` 和 `MotionGraphAsset` 均有专用 Inspector：保留标准配置字段，并在面板内显示静态校验结果。
- 参数、阶段／节点、连接和中断策略均保持声明式配置；错误直接以 Inspector HelpBox 暴露。
- 普通编译 0 Error；QA job `8b08ee27`，`MotionAuthoringInspectorEditModeTests` / EditMode 2/2 通过。
