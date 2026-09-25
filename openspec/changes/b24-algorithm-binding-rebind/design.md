## Context

`AlgorithmBinding { Key, ComponentId, TargetId, Generation, Type, Available }` 是草稿文档的绑定数据；`AlgorithmValidator` 对非模板编译要求 `Input` 节点「有 `BindingKey`、`Bindings` 有对应 `Key`、`ComponentId != 0`、`TargetId != 0`、`Compatible(binding.Type, node.ValueType)`」、`Effector` 节点「有 `BindingKey`、有绑定、`ComponentId != 0`」，否则报 `RequiredBinding`。模板实例化后 `Bindings` 被清空，故所有 `Input`/`Effector` 端点都是「待绑定」。

## Goals / Non-Goals

**Goals:** 待绑定项可见（读模型）、`Rebind` 命令（领域层）、绑定面板渲染真实清单（表单）。

**Non-Goals:** 组件选择器交互、传感器运行时注册、编译应用。

## Decisions

1. **`Rebind` 是 `Edit` 的聚焦变体**：`Edit` 替换整份草稿，`Rebind` 只改 `Bindings` 里一个 `Key` 的三元组。`Type` 从对应节点（`BindingKey` 匹配、未删除）的 `ValueType` 派生（保留拷贝语义），`Revision` 自增，走与 `Edit` 相同的 `Changed` 事件。
2. **待绑定项判定**：`Kind == Input || Effector` 且未删除、`BindingKey` 非空，且 `Bindings` 里无对应 `Key`（或 `ComponentId == 0`）。每个待绑定项产出 `UiAlgorithmBindingRow`，`Input` 显示 `Field`（读哪个字段）、`Effector` 显示 `Action`（做什么动作）。
3. **快照字段**：`PendingBindings`（`IReadOnlyList<UiAlgorithmBindingRow>`）+ `PendingBindingCount`，与 `Issues` 并列；库页/不可用域为 null。
4. **`Rebind` 挂 `IAlgorithmReadModel`**：与 b22 `InstantiateTemplate` 同策略（最小改动，读模型已持有实例服务）；机器域实现，其他域返回 false。
5. **绑定面板渲染**：`Ready` 且有选中实例时列 `PendingBindings`（需求栏列「绑定需求：系统模板不预置绑定」）；`Empty`/无实例时保持空态说明；移除 `DisableDomainActions()`。

## Risks / Trade-offs

- **`Type` 派生**：Input 节点的 `ValueType` 即传感器值类型（如 `Number/humidity`），与 `AlgorithmValidator.Compatible` 的检查口径一致；Effector 不校验 `Type`，派生仍是安全默认。
- **组件选择器未接**：`Rebind` 的 `componentId`/`targetId` 参数本批由调用方（测试/后续 UI）提供，绑定面板只展示不触发选择——这是「接线」与「交互」的边界，交互后续批补。
- **`BuildGraph` 每次快照重建待绑定项**：节点数小（模板 ≤ 数十节点），`Find` 线性匹配可接受，不引入索引。
