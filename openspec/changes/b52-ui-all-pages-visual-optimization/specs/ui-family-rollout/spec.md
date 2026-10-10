## ADDED Requirements

### Requirement: Rollout requires user acceptance of representative structure
全量结构推广 MUST 等待 b51 用户明确通过及对应截图版本记录。
#### Scenario: Representative review is pending
- **WHEN** 自动测试通过但用户尚未验收代表原型
- **THEN** b52 保持未执行，不能修改其余页面结构。
#### Scenario: Representative review passes
- **WHEN** 用户明确通过代表原型
- **THEN** 按覆盖矩阵分批推进结构，无需重新请求相同授权；正式美术仍独立验收。

### Requirement: Every authored UI workspace is covered
系统 SHALL 对当前 40 个 Prefab 和各自全部分页维护覆盖与验收记录，家族采用适合功能的不同构图。
#### Scenario: A family is delivered
- **WHEN** 某家族的结构改版完成
- **THEN** 记录每页的正常/空/不可用/长文本及返回路径结果，保留功能、权限、输入和状态数据来源。
#### Scenario: A prefab is shared with the pilot
- **WHEN** 优化 b51 已修改宿主的其它分页
- **THEN** 同时回归代表页，不覆盖用户已通过的结构原则。

### Requirement: Layout changes preserve domain boundaries
系统 MUST 保持未实现业务不可用，保留现有安全确认与读写权限。
#### Scenario: A visually complete page lacks a domain provider
- **WHEN** 页面没有正式领域数据
- **THEN** 展示明确未接入状态，不以示例数据宣称功能交付。
