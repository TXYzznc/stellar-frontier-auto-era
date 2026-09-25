## Why

b22 已落地「从模板创建草稿实例」的领域命令 `IAlgorithmReadModel.InstantiateTemplate`，但库页预制体上的三个「创建实例」按钮（`Btn_SystemTemplatesCreate`／`Btn_PlayerTemplatesCreate`／`Btn_TemplateDetailCreate`，文案「创建实例」）在契约 JSON 里**只有节点定义、没有字段映射**，`.Fields.cs` 因此没有对应字段，表单也无法接 `onClick`。同时库页 `OnAutoEraOpen` 仍调用 `DisableDomainActions()`（那是模板读模型接线前的旧行为，b17 已接线模板列表/详情）。本变更补齐按钮字段映射并接线：点「创建实例」→ 在当前选中机器上创建草稿实例 → 打开算法工作台（机器域读模型自动选中新实例，显示图结构与「缺少绑定」校验问题）。

## What Changes

- 契约 JSON 的 `bindings` 补三个按钮字段：`_systemTemplatesCreateButton`／`_playerTemplatesCreateButton`／`_templateDetailCreateButton`（kind `Button`），并重新运行 `tools/ui_contract_to_form_script.py` 生成 `.Fields.cs`。
- `AlgorithmLibraryForm` 移除 `DisableDomainActions()`，改为按快照 `Count > 0` 启用/禁用三个「创建实例」按钮；`OnInit` 给按钮接 `onClick`。
- 新增 `OnCreateInstanceFromTemplate`：取选中模板 Id → `_algorithms.InstantiateTemplate`，成功则 `AutoEraUiNavigator.Open(this, UIViews.AlgorithmEditorForm)`，失败（无选中机器／机器无运行时）保持库页不导航。
- 更新 `AlgorithmLibraryForm.cs` 顶部过时注释（「模板列表/详情读模型尚未接线」已不成立）。

## Capabilities

### Modified Capabilities

- `algorithm-library`: 库页「创建实例」按钮从「整域禁用」变为「有模板即启用，点击在当前机器创建草稿实例并跳转算法工作台」。

## Impact

- 修改 `Docs/Development/UI-PrefabLayouts/AlgorithmLibraryForm.contract.json`（bindings +3）。
- 重新生成 `Assets/Game/Scripts/AutoEra/UI/AlgorithmLibraryForm.Fields.cs`（+3 按钮字段）。
- 修改 `Assets/Game/Scripts/AutoEra/UI/AlgorithmLibraryForm.cs`（接线 + 移除 DisableDomainActions）。
- 不改领域层（b22 已提供 `InstantiateTemplate`）；不改算法工作台。

## Non-Goals

- 不做「集中待绑定面板」接线（绑定重绑，后续批）：创建实例后当前跳转算法工作台（工作台已能显示「缺少绑定」问题），绑定面板接线后调整导航目标。
- 不做「无选中机器」时的专项错误提示 UI（库页从 FieldHud 机器按钮打开，机器上下文本已存在；极端无运行时场景保持库页不导航）。
- 不改 `.cs` 之外的任何表单业务逻辑结构。
