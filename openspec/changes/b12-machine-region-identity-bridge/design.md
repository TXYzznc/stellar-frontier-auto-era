## Context

InitialRegion.Register 与 MachineRoster.Create 目前各分配并注册 ID；RegionWorkQueue 仅接受区域对象。二者同属现有 Hotfix 程序集，无须 asmdef 或注册表核心变更。B11 的机器 UI 不在本单元。

## Goals / Non-Goals

Goals：同 ID 查询；单区域绑定；失败无半状态；复用作业通道；名册回收、区域退出、世界退出对称清理。

Non-Goals：不生成实体/Prefab，不改视觉，不实现导航、UI、能量网、物流、玩家算法、磁盘存档或自动移动。

## Decisions

### Decision: 名册拥有身份，区域拥有投影

| 候选 | 优点 | 代价／风险 | 结论 |
|---|---|---|---|
| A：InitialRegion 对名册机器增加窄绑定路径 | 原队列可直接消费；注册表仍解析唯一 MachineInstance；改动在授权目录 | 需显式区分区域自有对象与借用表示 | 采用 |
| B：注册表支持同 ID 多表示 | 所有系统都能列出视图 | 扩大注册表语义、触及范围外核心及全部调用者 | 不采用 |
| C：另建映射 ID 或完全重写区域对象为机器 | 可隔离旧代理路径 | 双 ID 违反冻结结果；重写扩大兼容影响 | 不采用 |

`InitialRegion` 改为 partial，新增 `InitialRegion.Machines.cs` 实现 `DeployMachine(id, position, size, out model, yaw, blocksNavigation)` 和 `TryGetMachine`。查找仅限当前 session.Machines，且核验世界注册表解析对象一致。不 Allocate，不 Register。`RegionObject.Machine` 为可选只读引用；旧 Register 路径仍拥有自身注册，兼容 B10 代理与测试。

MachineInstance 增加仅运行时 internal 区域所有者 token，防止两个区域同时绑定；不写入快照。重复同参数绑定返回 AlreadyBound，参数变化拒绝而不移动；固定载体 CanMove 不变。验证数值、占地、身份、销毁状态、其它区域占用后才发布部署。

### Decision: 区域退出与库回收分离

区域 Remove/Dispose 只移除投影、作业占用和订阅，不取消名册身份，也不把场景卸载当作玩家回收。名册 RecoverToLibrary 原有停机／维修门保持；成功后 Changed 使区域解绑，失败保持原状。名称变化同步投影。

MachineRoster 增加 Disposed 事实通知；InitialRegion 订阅后在名册/世界释放时 Dispose。无需修改 AutoEraWorldSession：其现有 Machines.Dispose 调用即可触发清理。区域 Dispose 先置关闭状态，再移除全部对象，阻止清理期间重新申请；解除名册事件。旧区域自有对象正常反注册，名册投影不反注册。

### Decision: 预约状态不是抵达

RegionWorkQueue 增加 `GetRequestState` 只读结果（None/Granted/Waiting/InvalidRequester/InvalidTarget），Request/Release 继续使用原优先级/FIFO且不抢占当前 Owner。调用者可直接用 DeployMachine 的同 ID 申请；不增加第二个队列或包装调度器。工作位置参数仅沿用区域合法性检查，不承诺世界位姿已抵达。

## Risks / Trade-offs

- 区域表示与永久实体类型不同 → 注册表始终返回 MachineInstance，区域查询返回带 Machine 引用的 RegionObject；用测试验证。
- 生命周期事件重入 → 字典、token、部署状态先一致再通知，关闭标记先于卸载事件；测试在通知中查询一致性。
- 卸载时 Deployed 仍为 true → 明确这是持久部署事实而非区域加载标志；下一区域可重新绑定，只有合法回收改变事实。
- 旧代理路径保留 → 不声称正式机器场景生成已接通，后续实体/UI派发另行处理。

## Migration Plan

加法扩展现有服务，不重写资源或已有存档。新方法仅由本单元测试调用；相关机器与区域测试全部重跑。回退只移除本次窄扩展和测试，不回滚其它窗口工作。

## Open Questions

无待用户产品决定。后续导航/实际实体绑定不属于本次授权。
