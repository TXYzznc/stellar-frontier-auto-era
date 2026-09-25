## Context

契约生成器 `tools/ui_contract_to_form_script.py` 只从契约 JSON 的 `bindings` 数组生成 `.Fields.cs`（`build_fields` 遍历 bindings，`Button` → `Button` 类型）。三个「创建实例」按钮在 `layout` 树里有节点定义（含 `Button` 组件与「创建实例」文案），但从未进 `bindings`，所以字段缺失。`AlgorithmLibraryForm` 在 `HANDWRITTEN` 集合里：生成器只重写 `.Fields.cs`，`.cs` 业务逻辑手写。

## Goals / Non-Goals

**Goals:**

- 契约补齐三个按钮字段映射，`.Fields.cs` 生成对应字段。
- 库页按钮接线：有模板时可点，点击创建草稿实例并跳转算法工作台。
- 移除「模板读模型未接线」时代的 `DisableDomainActions()`。

**Non-Goals:** 绑定重绑、参数编辑、编译应用、无机器专项提示。

## Decisions

1. **按钮字段命名**：`_systemTemplatesCreateButton`／`_playerTemplatesCreateButton`／`_templateDetailCreateButton`，与 `_backButton` 的「按域前缀 + CreateButton」命名一致，kind `Button`。节点路径按契约 `layout` 树：`AlgorithmLibraryForm/Panel_Frame/Grp_PageHost/Panel_Page{System|Player}Templates/Grp_{System|Player}TemplatesActions/Btn_{System|Player}TemplatesCreate` 与 `…/Panel_PageTemplateDetail/Grp_TemplateDetailActions/Btn_TemplateDetailCreate`。
2. **按钮启用条件**：`snapshot.Count > 0`（有模板即启用）。「有选中机器」不放进启用条件——库页从 FieldHud 机器按钮打开，`session.Region.SelectedId` 已指向当前机器；且库页快照（`TemplateAlgorithmReadModel`）本就不含机器信息，硬塞会破坏模板读模型边界。极端无运行时由 `InstantiateTemplate` 返回 0 兜底（不导航）。
3. **点击行为**：`OnCreateInstanceFromTemplate` 取 `snapshot.SelectedTemplate`（空则取第一个可用模板），调 `_algorithms.InstantiateTemplate(templateId)`；返回非 0 才 `AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm)`。算法工作台机器域读模型 `_autoSelectPending` 会选中新实例并渲染图 + 校验问题（b20/b21 已验证）。
4. **移除 `DisableDomainActions`**：模板读模型 b17 已接线，库页不再是「整域禁用」；改为在 `Render` 里对三个按钮 `SetInteractable(count > 0)`。返回/关闭/导航按钮本就由 `OnInit` 接线，不受影响。

## Risks / Trade-offs

- **导航目标**：设计文档流程是「创建实例 → 集中待绑定 → 编辑器」，但绑定面板尚未接线；本批跳转编辑器（编辑器能显示「缺少绑定」问题，是「集中待绑定」第一步的可验证替身），绑定面板接线后（后续批）再调导航目标。
- **无机器场景静默失败**：库页从 FieldHud 机器按钮进入，正常不会触发；保留「返回 0 不导航」作为安全兜底，避免误开一个空编辑器。
- **生成器重写 `.Fields.cs`**：只影响 `AlgorithmLibraryForm.Fields.cs`（生成器按全部契约重写），其他 Form 的 `.Fields.cs` 内容应逐字节不变——用 git diff 确认只有本 Form 变化。
