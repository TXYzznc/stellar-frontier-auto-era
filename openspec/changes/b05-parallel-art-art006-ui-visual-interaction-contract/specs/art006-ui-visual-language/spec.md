## ADDED Requirements

### Requirement: 清朗工程视觉语言与语义 Token
ART-006 UI SHALL 使用暖灰／浅色表面、深蓝灰结构、克制橙色主操作的清朗工程视觉基线，并以 `Surface`、`Text`、`Action`、`Focus`、`Danger`、`Warning`、`Success`、`Disabled` 语义 Token 表达跨页面状态。绿色、黄色、红色分别表达正常、需处理／等待、故障／严重错误；橙色不得承担警告或危险，异星洋红只用于资源／特殊事件。所有关键状态 MUST 同时具备文字、图标、形状、位置或节奏中的至少一种非颜色线索。

#### Scenario: 危险状态跨页面一致
- **WHEN** HUD、对象面板和确认弹窗同时呈现同一危险操作或故障
- **THEN** 它们使用 `Danger` 语义而非橙色主操作语义，并保留可读文字和非颜色区分

### Requirement: 字体、字号、图标和面板资源边界
ART-006 SHALL 使用唯一来源的 TMP Font Asset，并采用 `48/40/32/24/18/16/14 px` 字号阶梯；关键操作与状态文字 MUST 不小于 16 px。图标显示尺寸 SHALL 为 `20/28/36 px`，对应源资源 SHALL 为 `32/64/128 px`，采用线性工程风格和光学一致笔画。面板 SHALL 基于同一九宫格基底派生标准信息、强调操作和模态确认三类变体；每项导出资源 MUST 登记可编辑源、用途、状态、切片、九宫格边界、导入设置和替代规则。

#### Scenario: 资源交付前检查
- **WHEN** 后续任务提交按钮、面板或图标资源
- **THEN** 交付包包含可编辑源、导出 Sprite、九宫格／切片说明和 Sprite (2D and UI)、Clamp、无 Mipmap 的导入建议，且图片不含本地化文字或动态数值

### Requirement: 小型共享 UI Kit 边界
只有在两个或以上页面复用且职责稳定的面板、按钮、图标、状态标记或告警卡 SHALL 进入共享 UI Kit；页面专属结构 MUST 保持在所属 UIForm。视觉稿、资源和 Prefab SHALL 引用语义 Token，不得为页面局部散落硬编码色值或无复用价值的整页子 Prefab。

#### Scenario: 判断共享组件资格
- **WHEN** 新页面提出复用一个组件
- **THEN** 若该组件没有至少两个明确页面消费者或职责尚不稳定，它保留为页面专属结构而不进入共享 UI Kit
