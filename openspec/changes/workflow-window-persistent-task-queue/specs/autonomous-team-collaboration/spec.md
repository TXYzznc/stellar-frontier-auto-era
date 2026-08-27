## ADDED Requirements

### Requirement: 长期窗口使用持久单Active任务队列
每个长期专业窗口 MUST 同时只执行一个Active任务。新的独立任务 MUST 进入持久Pending队列，MUST NOT 通过普通窗口消息替换或打断Active。Pending MUST 按明确优先级排序，同优先级 MUST 按入队顺序FIFO执行。

#### Scenario: 第二张图在第一张制作期间下发
- **WHEN** 3D原画正在制作第一张图且收到第二张图的独立任务
- **THEN** 第二张图进入Pending，第一张完成保存、自验和汇报后才领取第二张

#### Scenario: Pending任务优先级更高
- **WHEN** 新Pending任务优先级高于当前Active
- **THEN** 只调整Pending内部顺序，不替换Active

### Requirement: 运行中窗口不接收普通派发消息
下发方 MUST 在目标窗口运行时仅写入持久队列，MUST NOT 发送普通新任务消息。只有目标窗口空闲且没有Active时，MAY 发送启动消息唤醒窗口。

#### Scenario: 员工窗口正在运行
- **WHEN** 下发方新增一个不改变当前产物的后续任务
- **THEN** 下发方只入队且不调用窗口消息接口，员工在当前任务收尾后自行领取

### Requirement: 抢占仅发生在批准的安全检查点
Active任务 MAY 仅因S0／S1或资源安全风险、用户明确立即切换、当前产物已确定作废而抢占。抢占 MUST 记录批准来源和安全检查点；被抢占任务 MUST 进入Suspended，并在紧急任务完成后优先恢复。

#### Scenario: 普通高优先级任务到达
- **WHEN** 高优先级任务不满足合法抢占条件
- **THEN** 任务进入Pending且当前Active继续

#### Scenario: 当前产物确定作废
- **WHEN** 已批准的新合同证明继续当前产物必然返工且窗口已保存安全检查点
- **THEN** 下发方记录`invalidated`抢占，切换新任务，并保留原任务恢复或关闭证据
