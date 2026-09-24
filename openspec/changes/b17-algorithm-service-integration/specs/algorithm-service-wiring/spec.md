## ADDED Requirements

### Requirement: 世界会话提供算法模板库
世界会话 SHALL 在创建时初始化一个算法模板库，并通过公开属性暴露，供 UI 与其它世界内服务读取模板列表与详情；世界释放时模板库随之失效。

#### Scenario: 进入世界后可读模板库
- **WHEN** 一个世界会话处于 Active 状态
- **THEN** 其算法模板库非空，且可枚举模板列表

#### Scenario: 世界释放后模板库失效
- **WHEN** 世界会话被释放
- **THEN** 后续通过世界读取模板库不再返回可用数据（失效或抛 ObjectDisposedException）

### Requirement: UI 读模型返回真实模板列表
当界面会话处于可用世界内时，算法读模型 SHALL 从世界模板库读取模板列表（稳定 ID、名称、版本、是否系统模板），而不是返回固定的「不可用」状态。

#### Scenario: 世界内打开算法库返回模板列表
- **WHEN** 界面会话 HasWorld 为 true 且世界模板库非空
- **THEN** 读模型快照 State 为可用，Templates 包含世界模板库的全部模板

#### Scenario: 世界外或无世界返回不可用原因
- **WHEN** 界面会话没有可用世界
- **THEN** 读模型快照 State 为 Unavailable，UnavailableReason 说明需要进入区域

### Requirement: 模板详情可读取
选中模板后，读模型 SHALL 提供该模板的详情（至少包含名称、版本、是否系统模板，以及可展示的节点/端口摘要）。

#### Scenario: 选中模板返回详情
- **WHEN** 读模型对某个模板 ID 调用 Select 且返回成功
- **THEN** 快照的 SelectedIndex 指向该模板，Detail 非空且包含该模板摘要
