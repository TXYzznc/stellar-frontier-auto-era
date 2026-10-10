## ADDED Requirements

### Requirement: Implementation respects the user's plan-first hold
本 change MUST 在用户明确恢复实施之前保持方案审阅状态，不继续修改产品资产、代码或运行 Unity 验证。

#### Scenario: User asks for OpenSpec before implementation
- **WHEN** 用户要求先写 OpenSpec 并暂停实施
- **THEN** 仅完善方案文档，保留并标注此前未验收中间产物，不把局部工作或自动检查视为用户批准。

### Requirement: Representative workspaces have different visual hierarchies
系统 SHALL 将机器整备、HUD 常驻与机器概况、中枢总览组织为物件工作台、世界边缘检查和管理概况三种构图；保留功能规则。

#### Scenario: Machine preparation is opened
- **WHEN** 玩家选择一台机器并打开整备
- **THEN** 载体/就绪信息、明确占位的机器展示区、可选槽位和原有动作同时可达，安装卸载仍经过原确认服务。

#### Scenario: World HUD is visible
- **WHEN** HUD 打开且选中或未选中机器
- **THEN** 五个常驻模块共同布局，机器侧栏按原选择语义呈现，空白世界区域仍可操作。

#### Scenario: Hub overview is opened
- **WHEN** 玩家打开中枢总览
- **THEN** 概况、运行与关注区以不同尺度组织，入口靠近关联内容，其它分页及返回仍可使用。

### Requirement: Prototype production preserves the existing runtime contract
系统 MUST 保留数据源、事务、输入和权限；只修改目标工作区，不让未接线能力变成可用功能。

#### Scenario: A domain is unavailable
- **WHEN** 相关经营或成长领域尚未接入
- **THEN** 原型显示明确不可用原因，不显示虚构数值、曲线或成功操作。

#### Scenario: A non-target page is opened
- **WHEN** 玩家切换到机器库其它分页或中枢其它分页
- **THEN** 原结构、绑定和业务意图保持；公开参数、能源与机器概况拆分不被撤销。

### Requirement: Authored layout and evidence are reproducible
交付 MUST 包含合同一致的节点树、四项 RectTransform 数据、状态与射线规则、1920×1080 截图和验证结果；原型与正式美术状态分开。

#### Scenario: A representative prefab is rebuilt
- **WHEN** 从本批合同定向生成 Prefab
- **THEN** 字段引用有效，模板保持 inactive，根与边缘锚点正确，展示占位标明未制作正式资源。

#### Scenario: User reviews the prototype
- **WHEN** 自动门禁和截图检查完成
- **THEN** 原型提交用户验收，只有用户明确通过才解锁 b52 结构推广，不能自动代签。
