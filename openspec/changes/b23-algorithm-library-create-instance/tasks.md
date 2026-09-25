## 1. 契约与字段

- [x] 1.1 `AlgorithmLibraryForm.contract.json` 的 `bindings` 补三个 `Button` 字段（`_systemTemplatesCreateButton`／`_playerTemplatesCreateButton`／`_templateDetailCreateButton`）
- [x] 1.2 运行 `python tools/ui_contract_to_form_script.py` 重新生成 `.Fields.cs`，确认三个按钮字段出现（6 行新增）；其他 6 个 `.Fields.cs` 的纯 CRLF→LF 行尾变化已 `git checkout` 还原，仅本 Form 与契约保留实质变化

## 2. 表单接线

- [x] 2.1 `AlgorithmLibraryForm.OnInit` 给三个按钮接 `onClick → OnCreateInstanceFromTemplate`
- [x] 2.2 新增 `OnCreateInstanceFromTemplate`：取选中模板 → `InstantiateTemplate` → 成功 `AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm)`
- [x] 2.3 移除 `DisableDomainActions()`，`Render` 里按 `Count > 0` 启用/禁用三个按钮（`SetCreateButtonInteractable`）
- [x] 2.4 更新顶部过时注释（「尚未接线」→ b17/b23 已接线）

## 3. 回归与收口

- [x] 3.1 普通编译 0 错误 0 警告、Console 0 错误
- [x] 3.2 `AlgorithmReadModelEditModeTests` 复跑 19/19 通过（InstantiateTemplate 已在 b22 覆盖）
- [x] 3.3 `openspec validate b23-algorithm-library-create-instance --strict` 通过
- [x] 3.4 更新本 tasks/design 收口，回传结果

> 表单按钮 onClick 是 UI 层接线，无独立 EditMode 单测（需实例化 prefab，属回归门资产态范畴）；正确性由「编译 0 错误 + 契约字段生成 + 读模型 19/19 + 代码审查（OnInit 接线/OnCreateInstanceFromTemplate/SetCreateButtonInteractable）」交叉确认。
