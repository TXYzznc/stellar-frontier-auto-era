## ADDED Requirements

### Requirement: 工作台布局层级清晰
算法编辑器 SHALL 将节点库、节点画布、检视器、问题反馈和上下文工具组织为稳定的工作台层级，并在顶部显示算法状态、节点计数、连线计数和当前编辑/诊断模式。

#### Scenario: 打开编辑器
- **WHEN** 玩家打开一个已有算法实例
- **THEN** 左侧显示节点库，中间显示画布，右侧显示检视器，画布下方显示问题反馈，顶部显示当前模式和图结构摘要

#### Scenario: 切换诊断模式
- **WHEN** 玩家从编辑模式切换到诊断模式
- **THEN** 标题徽标和上下文工具条明确显示诊断状态，编辑命令进入禁用或隐藏状态，画布和检视器仍保留

### Requirement: 节点和连线使用独立 Item 预制体
算法编辑器 SHALL 从 `Assets/Game/Prefabs/UI/Item/AlgorithmNodeItem.prefab` 动态创建节点，从 `Assets/Game/Prefabs/UI/Item/AlgorithmEdgeItem.prefab` 动态创建连线；Form 不得继续实例化同时包含节点组和连线组的万能模板。

#### Scenario: 渲染图快照
- **WHEN** 快照包含 N 个节点和 M 条有效连线
- **THEN** 画布创建 N 个节点 Item 和 M 个连线 Item，每个实例绑定稳定 Id，节点与连线的视觉和交互状态互不残留

#### Scenario: 节点 Item 绑定端口
- **WHEN** 节点 Item 被绑定
- **THEN** 它在自身内部生成输入/输出端口行，并通过节点 Id、端口 key 将点击事件回传给 Form

### Requirement: 画布支持工作台交互
画布 SHALL 支持滚轮缩放、中键平移、节点拖拽、端口拖线和将选中对象定位到可见区域；拖拽结束后才写回持久化节点坐标。

#### Scenario: 缩放画布
- **WHEN** 玩家在画布内滚轮
- **THEN** 画布以指针位置为中心调整缩放，节点相对布局保持稳定，缩放范围限制在预设最小值和最大值内

#### Scenario: 拖拽节点
- **WHEN** 玩家拖拽节点并释放鼠标
- **THEN** 节点 Item 移动到新位置，画布更新连线位置，并调用一次节点移动命令写回坐标

#### Scenario: 端口拖线
- **WHEN** 玩家从输出端口拖向兼容输入端口
- **THEN** 目标端口出现可连接反馈，释放后创建连线；不兼容或已占用输入保持原图并显示原因

### Requirement: 游戏化状态反馈可读
工作台 SHALL 使用稳定的节点分类色、选中高亮、待连接反馈、错误/警告徽标和模式色带增强反馈，并为颜色状态提供文字或图标辅助说明。

#### Scenario: 选中节点
- **WHEN** 玩家点击节点
- **THEN** 节点显示选中描边或高亮，右侧检视器更新为该节点内容，底部工具条更新为节点相关操作

#### Scenario: 验证出现问题
- **WHEN** 算法验证返回错误或警告
- **THEN** 问题区显示数量和问题行，节点或端口显示对应状态提示，玩家可以定位到相关对象

### Requirement: Item 资产位置和模板状态符合 GF 规范
新增节点和连线 Item SHALL 位于 `Assets/Game/Prefabs/UI/Item`，具有明确的 `Item_` 根命名、必要的 LayoutElement/交互组件，并以默认 inactive 的模板或 prefab 资源供 Form 引用。

#### Scenario: Prefab 结构检查
- **WHEN** 执行算法编辑器 Prefab 绑定检查
- **THEN** Form 的节点/连线引用解析到 Item 目录中的独立 prefab，模板不包含另一种图元素的隐藏备用树
