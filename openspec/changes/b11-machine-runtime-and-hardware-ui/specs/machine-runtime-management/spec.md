## ADDED Requirements

### Requirement: First version configuration and existing foundation reuse
系统 SHALL 复用已验ID/时间/GF服务，按现行设计建立第一版配置覆盖清单及受控数据，禁止重复ID、非法能力/等级、虚假资源路径和手写生成物。

#### Scenario: Missing art resource
- **WHEN** 配置对象尚无正式美术交付
- **THEN** 标记未交付依赖并给出明确不可实例化原因，已就绪对象及纯业务测试不被全局阻断，不宣称P0-011全部通过

### Requirement: Machine identity and component ownership
机器 SHALL 保留永久ID、型号历史单调序号、唯一可改显示名及组件实例归属；两类载体槽数/能力按当前配置，货舱作为容量效应器计入唯一机器容器。

#### Scenario: Rename and reinstall
- **WHEN** 机器合法重命名或在停止且有现场权限时安装/拆卸
- **THEN** ID引用不失效、序号不复用、同一组件不重复占有，拒绝非法操作且无半更新

### Requirement: Independent state and permissions
系统 SHALL 区分激活、停止/运行、休眠、供电、损坏和连接；现场首次激活/整机断电/硬件管理限制沿正式设计，中枢不得获得现场限定权限。

#### Scenario: Power and connection recovery
- **WHEN** 外部供电或信号输入失效后恢复
- **THEN** 保留配置并按既有规则重新判断状态，不伪装现场激活，不混同断电与玩家停止

### Requirement: Task behavior and compute scheduling
系统 SHALL 沿已冻结规则实现机器五档优先级/同级FIFO、有界队列拒绝、效应器独立串行和安全切换、共享算力池与逻辑容量；不以每帧轮询算法、不新增玩家手动作业。

#### Scenario: Concurrent effectors and compute wait
- **WHEN** 两个效应器接收有效行为而其中请求暂缺算力
- **THEN** 不同效应器可独立调度，同一效应器串行；等待有明确原因，释放后确定性继续，不能超卖算力或随机失败

#### Scenario: Safe interruption and full queue
- **WHEN** 队列已满或高优先级请求到达不可立即中断的行为
- **THEN** 新任务拒绝不覆盖旧任务，抢占仅按规则在安全点发生，结果发布一次且可追踪

### Requirement: Evidence boundaries
交付 SHALL 含相关普通编译、单元/生命周期集成、数据/资源引用验证与排除项；测试夹具与真实玩家端到端分开记录。

#### Scenario: Services precede later systems
- **WHEN** 使用供电、信号或行为测试适配输入
- **THEN** 只计当前服务接口覆盖，不声明完整电网、导航、算法、生产或存档已完成

