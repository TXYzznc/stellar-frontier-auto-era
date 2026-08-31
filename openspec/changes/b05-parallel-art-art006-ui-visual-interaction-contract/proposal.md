## Why

ART-006 已完成 UI 视觉方向、信息层级、危险操作、异步反馈与可访问性的用户决策收口，但这些结论尚未形成可供样张、美术资源、GF UIForm、Prefab 与客户端接入共同执行的正式合同。现在需要在实现前建立单一、可验证的 UI 视觉与交互边界，避免后续页面各自解释 Token、状态和焦点规则。

## What Changes

- 建立“清朗工程面板”视觉语言、语义 Token、唯一字体字号阶梯、图标尺寸与小型共享 UI Kit 的 ART-006 合同。
- 固化 HUD、对象优先右侧面板、任务中枢、规则页面及影响预览三张代表样张应覆盖的信息层级、入口出口、状态与异常行为。
- 固化危险确认、描述文本资源绑定、异步执行／失败／长时等待、焦点恢复与无障碍输入的交互合同。
- 规定后续样张、可编辑源、Sprite 切片／九宫格导入、`prefab-layout.md`、GF UIForm 生命周期、客户端状态摘要和验收证据的前置要求。
- 本变更只创建 apply 前的合同与实施任务；不生成 UI 样张或资源，不创建 Prefab／代码，不驱动 Unity／DCC，也不修改任务表。

## Capabilities

### New Capabilities

- `art006-ui-visual-language`: 定义 ART-006 的视觉 Token、字体字号、图标、面板与共享 UI Kit 边界。
- `art006-ui-interaction-state-contract`: 定义对象优先导航、危险操作、确认描述、异步／长时等待、焦点与可访问性状态。
- `art006-ui-page-delivery-contract`: 定义三张代表样张、页面规格、资源／切片、Prefab 布局、GF 接入和多比例验收的交付合同。

### Modified Capabilities

- 无。

## Impact

- 后续 UI 美术、文本资源、Sprite 导入、样张与资源清单将受本合同约束。
- 后续客户端 UIForm、InputModule、异步状态与状态摘要接口须按本合同接入。
- 依赖 `Docs/GameDesign/90-设计管理/方案讨论增量归档.md`、ART-006 派发单和 `Docs/Development/GF-UI-Standards/README.md` 及 01–08；不修改现有运行时代码或场景。
