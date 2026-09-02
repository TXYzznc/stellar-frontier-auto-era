# 第 5.6 段：视觉候选合同兼容性

日期：2026-09-02

- `wheeled_carrier_visual_candidate.prefab` 是从功能原型派生的视觉候选 Variant；它不移动 LogicRoot、RigRoot、VisualRoot 或 AuthorityCollisionRoot。
- 已重用第 5.3 段的结构、动作和可视演示证据；本候选的独立层级兼容性测试补充证明视觉替换不改变功能绑定。
- QA job `a215eca9`：`FunctionalRigVisualCandidateEditModeTests` / EditMode 1/1 通过，失败 0；结束时 Unity 非 PlayMode、未编译、无更新或域重载待处理。
