## Why

当前 UI 批次尚未形成可验收闭环：ProgressReportForm 的出口按钮无法关闭，普通界面的返回与关闭语义重复，AlgorithmEditorForm 仍缺少绑定、连线、参数和诊断闭环，FieldHudForm 仍是重型单体 Prefab。

## What Changes

- 统一普通界面的安全出口语义，默认只保留“返回”，并修复 ProgressReportForm 的显式关闭。
- 完成算法模板实例化、节点编辑、端点绑定、连线、公开参数、应用和基本诊断流程。
- 将 FieldHudForm 的常驻 HUD 与现场详情页拆成可独立加载的 UIForm Prefab，保留会话透传和输入策略。
- 补齐 EditMode/PlayMode 验收证据，确保返回后世界输入和对象交互恢复。

## Scope

不修改任务表，不重写 GF_X UI 栈；业务实现仍位于 `Assets/Game/Scripts/AutoEra/`，框架层只在已有关闭/输入合同确有缺陷时修改并留痕。
