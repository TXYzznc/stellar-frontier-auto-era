## ADDED Requirements

### Requirement: 任务查询节点数据返回

任务查询节点 SHALL 在同一求值批次内向数据端口返回 `found`（布尔）与 `task`（对象引用），供下游分支显式防重。

#### Scenario: 存在未结束任务
- **WHEN** 本机任务队列存在与查询名匹配的未结束任务
- **THEN** `found` 求值为真且 `task` 返回该任务引用

#### Scenario: 无匹配任务
- **WHEN** 本机任务队列不存在与查询名匹配的未结束任务
- **THEN** `found` 求值为假且 `task` 为无效引用

### Requirement: 任务查询成本口径

任务查询节点 SHALL 按高层任务控制节点计 4 点逻辑成本。

#### Scenario: 成本查询
- **WHEN** 查询任务查询节点的逻辑成本
- **THEN** 成本 SHALL 为 4
