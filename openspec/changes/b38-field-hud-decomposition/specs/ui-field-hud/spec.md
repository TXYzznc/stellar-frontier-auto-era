## MODIFIED Requirements

### Requirement: 世界输入占用由模态界面声明

常驻 HUD 和现场半屏内容 MUST NOT 把世界输入标记为全局阻塞；只有当前活动的模态管理界面 MUST 可以阻塞镜头、世界点选和放置输入。

#### Scenario: 关闭全屏界面后恢复世界输入

- **WHEN** 玩家从现场 HUD 打开任意全屏管理界面并返回
- **THEN** `AutoEraUiRuntime.BlocksWorldInput` 最终为 false
- **AND** WASD、鼠标点选和对象聚焦恢复可用

### Requirement: HudStatus 四个入口必须有明确导航

顶部状态栏的成长、算力、能源和资源按钮 MUST 连接到现有页面或中枢对应页；未接入的领域 MUST 显示可解释空态。

#### Scenario: 点击顶部状态按钮

- **WHEN** 玩家点击成长、算力、能源或资源入口
- **THEN** 系统打开对应的 ProgressReport 或 BaseCommandHub 页面
- **AND** 透传当前 `AutoEraUiSession`

### Requirement: HUD 内容按职责独立加载

常驻摘要模块与现场内容页 MUST 可以作为独立 UIForm Prefab 动态打开和关闭，且 MUST NOT 复制领域读模型或会话数据源。

#### Scenario: 选中机器打开现场内容

- **WHEN** 玩家在区域中选中机器
- **THEN** 常驻 HUD 保持显示
- **AND** 机器现场内容以独立 Form 打开并读取同一世界会话

### Requirement: 算法编辑和诊断操作区互斥

算法编辑操作区与诊断操作区 MUST NOT 同时显示；诊断操作区未启用时 MUST 隐藏，不得覆盖编辑按钮。

#### Scenario: 打开算法编辑页

- **WHEN** 算法编辑器以编辑模式打开
- **THEN** `Grp_AlgorithmEditorActions` 可见
- **AND** `Grp_AlgorithmDiagnosisActions` 不可见
