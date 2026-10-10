# UI加载、性能与证据收口

原始任务映射：P8-007、P8-011、P8-012。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: UI acceptance is fixed but layout remains adaptive
本批UI SHALL 只在1920×1080运行验收，同时满足可适配的RectTransform结构合同。

#### Scenario: Inspect reference layout
- **WHEN** 1920×1080打开所改页面全部关键状态
- **THEN** 无重叠裁切或不可点击入口，不要求其他分辨率截图或运行检查。

#### Scenario: Inspect prefab sizing ownership
- **WHEN** 检查根、HUD、模态、列表及布局组
- **THEN** GF统一缩放、语义锚点、正确pivot/offset及单一尺寸控制者成立。

### Requirement: Lazy pages release correctly
按需页面 SHALL 在关闭或换会话时释放请求、列表项和订阅，不影响世界运行。

#### Scenario: Close while loading
- **WHEN** 父页面关闭后子Form异步加载完成
- **THEN** 迟到结果被回收，不重新出现或锁住输入。

#### Scenario: Repeated open close
- **WHEN** 同一页面打开关闭100次并回到相同状态
- **THEN** 存活Form、行对象、订阅及输入阻塞不随次数累积。

### Requirement: Idle UI avoids repeated work
无领域变化的常驻UI SHALL 避免持续托管分配和无意义布局重建。

#### Scenario: Idle unchanged world view
- **WHEN** 页面可见但其数据修订未变化
- **THEN** 不重复创建快照数组/字符串或提交相同布局刷新。

### Requirement: Performance gates use measured builds
性能验收 SHALL 使用可重现Development Player基准和长时运行结果。

#### Scenario: Load tiers and soak
- **WHEN** 在固定配置下运行10/50/100机器并以50台持续60分钟
- **THEN** 记录CPU分位数、GC、内存、队列和UI延迟；预热后无持续资源增长，结果与功能门同时通过。

### Requirement: Evidence distinguishes implementation and acceptance
状态文档 SHALL 区分代码存在、运行接通、玩家可操作与正式验收，不回写用户任务表。

#### Scenario: Old report disagrees
- **WHEN** 历史交接报告与当前可验证代码/测试不一致
- **THEN** 保留来源与日期，链接新证据并标明未执行项，不能将规划任务标为已完成。


