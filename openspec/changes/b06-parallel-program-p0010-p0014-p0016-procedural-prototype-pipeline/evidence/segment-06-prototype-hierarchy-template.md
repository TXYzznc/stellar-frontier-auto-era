# 第 1.3 段：功能原型层级模板

日期：2026-09-02

## 交付

- 新建 `Assets/Game/Prefabs/FunctionalPrototypes/FunctionalRigHierarchyTemplate.prefab`；该资产由 `AutoEra/Functional Prototypes/Create Hierarchy Template` 编辑器菜单从临时对象生成，不修改或保存任何产品场景。
- 模板根 `FunctionalRigHierarchyTemplate` 下的直接子级为 `LogicRoot`、`RigRoot`、`AuthorityCollisionRoot`；`VisualRoot` 是 `RigRoot` 的直接子级，`Joint_Template` 与 `VisualSlot_Template` 仅位于视觉分支。
- `AuthorityCollisionRoot` 持有 `BoxCollider`。`FunctionalRigPrototypeHierarchy` 明确保存四个根引用并校验上述边界，因此视觉关节变换不会成为逻辑根或权威碰撞根的父级。
- 编辑器 Builder 仅负责创建此独立 Prefab，使用后销毁临时对象；运行时组件不使用全局查找，也不驱动运动 Tick。

## 验证

- 执行菜单后，Unity 8090 普通编译检查：`isCompiling=false`、`isUpdating=false`，Console Error 为 0。
- QA 队列任务 `b06-functional-rig-hierarchy-editmode` 已完成；job `2adc800a` 运行 `AutoEra.Tests.Editor.FunctionalRigPrototypeHierarchyEditModeTests`（EditMode），总计 2，通过 2，失败 0，跳过 0，不确定 0，耗时 6 秒。
- 测试结束时 Unity 为“星际拓荒：自动纪元”/ 2022.3.62f3c1，非 PlayMode、未暂停、未编译、未更新、无域重载待处理；测试窗口已释放 8090。
