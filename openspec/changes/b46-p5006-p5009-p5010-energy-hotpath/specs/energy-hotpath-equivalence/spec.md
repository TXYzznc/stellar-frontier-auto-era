# 电网热路径优化

原始任务映射：P5-006、P5-009、P5-010。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Energy optimization preserves settlement
优化后的电网 SHALL 对相同输入保持既有供需、电量、燃料及停机恢复规则。

#### Scenario: Shortage priority
- **WHEN** 多档优先级负载同时超出供给
- **THEN** 仍按原优先级及同级队列顺序停机，恢复后不重复提交行为单位。

#### Scenario: Daylight and fuel transitions
- **WHEN** 结算跨越日照切换、燃料耗尽和储能边界
- **THEN** 与参考规则结果一致，UI查询不额外推进结算。

### Requirement: Steady energy advancement avoids allocation
预热且参与者与停机集合稳定的电网推进 SHALL 达到零托管分配。

#### Scenario: Stable shortage tick
- **WHEN** 在固定缺电集合下重复推进
- **THEN** Plan和快照不持续分配，结果仍可观察。

#### Scenario: Participant changed
- **WHEN** 新机器部署或优先级改变
- **THEN** 允许一次有据可查的缓存重建，随后稳定Tick恢复零分配。

### Requirement: Snapshots remain immutable
已经发布的电网快照 SHALL 不被后续工作缓冲复用改写。

#### Scenario: Keep old snapshot
- **WHEN** 观察者保存旧停机快照后下一步改变供电状态
- **THEN** 旧快照的ID集合与数值保持不变，新快照反映新状态。

### Requirement: Performance evidence is reproducible
性能结论 SHALL 来自固定构建环境的实际测量，并附正确性结果。

#### Scenario: Compare implementations
- **WHEN** 在相同10/50/100台负载场景比较优化前后
- **THEN** 记录硬件、构建、采样时长、CPU分位数和GC；不得仅以截图FPS宣称优化成功。


