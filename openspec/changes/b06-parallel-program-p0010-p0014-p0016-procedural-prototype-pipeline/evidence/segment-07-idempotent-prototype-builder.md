# 第 1.4 段：幂等功能原型 Builder

日期：2026-09-02

## 交付

- `FunctionalRigPrototypeBuilder` 只接受已通过 `FunctionalRigContractValidator` 的合同和已验证的层级模板；以类别前缀加合同稳定 ID 复用关节、锚点、净空体、碰撞包络、视觉槽及基础几何。
- 同一合同再次构建时保留已有 Unity 对象实例，重新校正父级、名称、本地 Pose、缩放和碰撞体尺寸；合同删除的生成节点会被清理，不影响未标记的未来正式视觉内容。
- 每个视觉槽下生成一个可替换的 Cube 基础几何，缩放与合同 `ExpectedBounds` 一致，并移除视觉几何自带 Collider；合同碰撞包络只在 `AuthorityCollisionRoot` 下生成 `BoxCollider`。
- Builder 保持在 Editor 程序集中，资产生成仍由 Editor 工作流执行。EditMode 测试通过反射调用公开 Builder 接口，避免改变既有 `AutoEra.Editor.Tests` 程序集引用或新增 asmdef。

## 验证

- 普通 Unity 刷新编译后：`isCompiling=false`、`isUpdating=false`，Console Error 为 0。
- QA 队列任务 `b06-functional-rig-builder-editmode` 已完成；job `986b5d3b` 运行 `AutoEra.Tests.Editor.FunctionalRigPrototypeBuilderEditModeTests`（EditMode），总计 1，通过 1，失败 0，跳过 0，不确定 0，耗时 6 秒。
- 测试覆盖稳定 ID 对象复用、合同尺寸更新、视觉槽父级、视觉 Geometry 无 Collider，以及权威碰撞体仍处于独立碰撞根。
