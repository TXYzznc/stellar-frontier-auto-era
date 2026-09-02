# 第 1.6 段：固定功能原型验证面板

日期：2026-09-02

## 交付

- 新增固定 Editor 菜单入口 `AutoEra/Functional Prototypes/Validation Panel`。
- 面板只读选择 `TextAsset` 合同 JSON 与 `FunctionalRigPrototypeHierarchy`，可分别执行合同 Codec 验证和合同加结构验证，并显示完整诊断列表。
- 面板不写入合同、Prefab、产品场景或玩法状态；构建仍由独立 Builder 明确触发。

## 验证

- 普通 Unity 刷新编译后：`isCompiling=false`、`isUpdating=false`，Console Error 为 0。
- QA 队列任务 `b06-functional-rig-validation-panel-editmode` 已完成；job `4160fbd1` 运行 `AutoEra.Tests.Editor.FunctionalRigPrototypeValidationPanelEditModeTests`（EditMode），总计 1，通过 1，失败 0，跳过 0，不确定 0，耗时 6 秒。
- 测试确认面板固定入口复用 `FunctionalRigContractJson`，无效 JSON 返回合同字段诊断且不产生合同实例。
