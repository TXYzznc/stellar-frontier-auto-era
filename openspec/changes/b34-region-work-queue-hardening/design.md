## Context

P1-005 的作业点申请与等待队列已在更早批次落地：RegionWorkQueue（独占通道 + 优先级 FIFO 等待 + 释放唤醒链 + 目标/申请者销毁对称），MachineNavigation 与 MotionWorkBridge 分别做导航侧与表现侧投影。本批是「审计后只补真实缺口」，不改既有语义。

## Decisions

### 优先级升级：只升不降、同级保持 FIFO

等待中的机器带新优先级重新申请时，仅在**严格更高**（`priority > _priorities[machine]`）时从等待列表移出并按新优先级重插；同级或更低保持原位。理由：重新申请（幂等调用）不应让同级等待者被插队，也不应惩罚降级调用——只有「确实提高了优先级」才有重排的必要。插入仍用 `while (index < Count && _priorities[waiting[index]] >= priority)` 保证同优先级 FIFO 稳定。

### WorkState 跟随权威队列而非桥内缓存

MotionWorkBridge 的 `WorkState` 过去只在 `RequestWork`/`ReleaseWork` 里更新，是申请时刻的快照。改为 `RequestWork` 成功后订阅 `queue.Changed`，回调里用 `queue.GetRequestState(machine)` 同步——被唤醒（Waiting→Granted）或异常降级（None/InvalidRequester/InvalidTarget）都反映权威事实，无需上层再次申请。订阅/退订与 `_workQueue`/`_workMachine` 生命周期对称：换队列先退旧、`ReleaseWork` 退订、`Dispose` 释放占用后 `DetachQueue`。

## Risks / Trade-offs

- 订阅 `queue.Changed` 后，队列每次 Publish 都会回调一次 `GetRequestState`（O(1) 区域查询），作业点生命周期内频率极低，可接受。
- `OnWorkQueueChanged` 在队列 Dispose 后不触发（Dispose 清空 `Changed`），桥持有已释放队列引用时 `GetRequestState` 返回 InvalidTarget——安全降级而非异常。
- 优先级升级的语义变化仅影响「等待中再次带更高优先级申请」的调用方，现有测试无此断言，不破坏既有行为。
