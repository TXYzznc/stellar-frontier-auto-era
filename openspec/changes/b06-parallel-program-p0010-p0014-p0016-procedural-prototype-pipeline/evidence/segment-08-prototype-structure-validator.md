# 第 1.5 段：功能原型结构校验

日期：2026-09-02

## 交付

- `FunctionalRigPrototypeStructureValidator` 将合同、模板层级和所有生成稳定 ID 汇总为完整错误列表，供后续固定验证面板直接显示。
- 校验覆盖生成对象缺失／重复、关节／锚点／净空体／碰撞包络／视觉槽的声明父级、Bind Pose 与尺寸；碰撞包络必须保持在独立 `AuthorityCollisionRoot` 下并含 `BoxCollider`。
- 可见 Renderer 必须沿父级追溯到已声明视觉槽。视觉 Geometry 必须仍位于槽下，且中心／尺寸／旋转匹配合同并不持有 Collider；因此游离、无支撑、错误父级、Transform 漂移和视觉槽越界均会失败。

## 验证

- 普通 Unity 刷新编译后：`isCompiling=false`、`isUpdating=false`，Console Error 为 0。
- QA 队列任务 `b06-functional-rig-structure-validator-editmode` 已完成；job `a52c9aad` 运行 `AutoEra.Tests.Editor.FunctionalRigPrototypeStructureValidatorEditModeTests`（EditMode），总计 2，通过 2，失败 0，跳过 0，不确定 0，耗时 7 秒。
- 负例同时移除锚点、错误重设视觉 Geometry 父级并制造 Transform 漂移、加入无视觉槽支撑的 Cube；验证器报告每一类错误。
