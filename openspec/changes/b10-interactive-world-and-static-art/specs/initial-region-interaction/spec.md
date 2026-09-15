## ADDED Requirements

### Requirement: Reuse accepted foundations
实施者 SHALL 先核对既有基础与证据，仅补缺项，不因旧任务状态重写已完成系统。

#### Scenario: Existing implementation with stale task status
- **WHEN** 表中未完成项已有正式代码或验收证据
- **THEN** 将其列为复用/补验，明确剩余差异而不重复实现

### Requirement: Initial region object interaction
初始区域 SHALL 使用既有ID与对象边界注册机器、建筑和资源点，经InputModule选择对象并通过既有UI显示公开摘要；无选中、对象失效与UI拦截 SHALL 正确处理。

#### Scenario: Select and invalidate an object
- **WHEN** 玩家选中对象后对象被注销
- **THEN** 侧栏不保留可执行的失效对象操作，且输入不穿透UI

### Requirement: Placement and work requests
占地、旋转吸附、作业申请及等待队列 SHALL 遵循现行设计；地表矿脉不得恢复旧立体矿山禁行与手动钻探点要求。

#### Scenario: Competing requests
- **WHEN** 多机器申请同一有限作业位且当前拥有者释放
- **THEN** 按既有顺序规则移交，无重复拥有或重叠作业

### Requirement: Stage acceptance
本阶段 SHALL 提供从正式入口进入初始区域、选择与状态显示、占地及作业竞争的可复跑证据，不将展示用状态视为完整资源生产。

#### Scenario: Complete stage review
- **WHEN** 请求G1阶段验收
- **THEN** 列出实际通过用例、未覆盖功能和用户可查看入口，且无缺失引用及相关Console错误
