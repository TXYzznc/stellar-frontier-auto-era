## Context

警报账本（同源合并／恢复转历史／排序）、监控（真值跨越式报／销）与读模型（选中／标记已读／三态）都已交付并测试。`AlertForm` 是这条链上唯一没接的 UI。生产入口已通：`FieldHudForm._hudAlertsOpenButton` → `AutoEraUiNavigator.Open`（透传会话）→ `InitialRegionScene._alerts`（账本）经 `AutoEraWorldProcedure` 写进会话 → `AlertReadModels.Create(session)`。

## Decisions

### 定位＝区域选中来源对象，不做跨界面导航

「定位」复用 `InitialRegion.Select(source, false)`：选中来源对象（现场高亮由既有选择视觉承担）并关闭表单回到现场。来源为 Invalid（多储能合计口径）或对象已移除时，在详情栏说明「无单一对象可定位／已不在区域」——不伪造聚焦。跨界面「相关详情」跳转留后续批次，按钮禁用。

### 读模型补来源身份

`UiAlertRow` 原先只有解析后的 `SourceName`，定位需要原始 `PersistentId`。补 `Source` 字段：机器／设施身份原样透传，区域口径为 Invalid。界面按 `Source.IsValid` 决定能否定位。

### 筛选做活跃／历史循环，不做等级／来源细分

账本 `CopyInto` 已把行排成「先活跃后历史」，筛选只需在全部／仅活跃／仅历史三档间循环。等级与来源分列是更细的交互，属后续。

### 预制体字段绑定走 UnitySkills

与算法编辑器同源：脚本与契约加了字段但预制体未回写时，真实游戏里按钮静默空转。用 `prefab_instantiate` 临时实例 → `component_set_serialized_property`（referencePath 指向 `Grp_AlertsActions` 下四键）→ `prefab_apply` → 删实例 → 文本核验。

## Risks / Trade-offs

- 「相关详情」按钮禁用而非移除：保留规格节点位置，接续批次补跨界面导航时只需放开。
- 定位依赖区域当前仍有该对象；对象销毁后诚实说明，不自动按名字替换（规格：失效对象禁用写操作并说明）。
- 生成器重写全部 `.Fields.cs` 的 EOL 噪声：只保留 AlertForm 与 AlgorithmEditorForm（b33 已改）两份，其余 `git checkout` 还原。
