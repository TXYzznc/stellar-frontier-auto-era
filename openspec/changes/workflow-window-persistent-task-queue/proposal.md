## Why

当前通过窗口消息直接下发任务时，新消息会打断员工窗口正在执行的工作，导致第一项尚未收尾就切换
到第二项。3D生产还出现主美技术文档尚未验收，三视图／生产说明已经提前启动的问题。需要把任务
投递与窗口消息分离，并用持久队列保护当前任务和阶段门。

## What Changes

- 每个长期职能窗口只允许一个Active任务；独立新任务进入本机持久Pending队列。
- Pending按明确优先级排序，同级FIFO；任何Pending优先级都不能替换Active。
- 运行中窗口不接收普通任务消息；完成保存、自验、证据、汇报和资源释放后自动领取下一项。
- 仅S0／S1或资源安全风险、用户明确立即切换、当前产物已确定作废允许在安全检查点抢占。
- 三视图／生产说明只能在主美技术文档经制作人验收通过后进入3D原画队列。
- 新增标准库Python队列工具、原子写入、锁、状态恢复和自动化测试。

## Capabilities

### Modified Capabilities

- `autonomous-team-collaboration`: 增加持久任务队列、Active保护、优先级＋FIFO和合法抢占合同。
- `art-resource-delivery-contract`: 增加主美技术文档验收先于三视图／生产说明派发的阶段门。

## Impact

- 更新`AGENTS.md`、Claude入口和`Docs/Development/Dispatch/`协作规则。
- 新增`tools/window_task_queue.py`及测试，本机队列文件被Git忽略。
- 不修改用户任务表，不改变产品任务优先级，不自动推送Git。
