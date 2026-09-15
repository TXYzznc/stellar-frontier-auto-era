## ADDED Requirements

### Requirement: Complete machine management visual flow
美术 SHALL 连续设计机器概况、硬件槽位、组件选择、安装/拆卸反馈完整流程，复用ART-006风格，先与客户端对齐字段/意图/错误状态，覆盖轮式与固定旋转两载体差异。

#### Scenario: One coherent review batch
- **WHEN** 本批设计与效果图完成
- **THEN** 实际通知制作人检查结构功能并报告用户，不逐按钮求确认，不直接让用户跳过制作人审查

### Requirement: User visual gate and continuous production
效果图 SHALL 经用户明确通过后才完成相应新素材与候选Prefab；通过后不再逐阶段申请开工，无新增素材则直接复用已有资源。

#### Scenario: Approved candidate proceeds
- **WHEN** 用户确认本批视觉并没有新的合同冲突
- **THEN** 美术连续处理必要切图/装配/自验，客户端受控接入正式项目；不覆盖未经确认的其它UI

### Requirement: Authoritative operations and observable outcomes
UI SHALL 从业务状态读取显示并通过已有InputModule/GF生命周期提交管理意图，显示合法权限、安装归属、拒绝原因、异步/等待/失败/取消，禁止伪造成功或手动作业。

#### Scenario: Hardware unavailable remotely
- **WHEN** 中枢远程查看机器或机器尚未安全停止
- **THEN** 现场限定的安装/拆卸不可执行且原因可见，重命名等已许可管理操作按既有合同可用

### Requirement: Focused delivery and later editor design
验收 SHALL 以1920×1080为基准，复用现有确认/焦点/异步规则；本批不实现算法节点编辑器，后续设计独立入队。

#### Scenario: Business binding is not a redesign
- **WHEN** 页面需要新增业务字段
- **THEN** 客户端复用通过样式补节点，美术只复核新增部分，不重设计整套中枢

