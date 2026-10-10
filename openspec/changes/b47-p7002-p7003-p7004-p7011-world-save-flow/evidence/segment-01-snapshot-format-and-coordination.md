# B47 增量01：格式、保存协调与任务/事件基础

实施中，B47保持Active；这里只记录已验证单元，不表示世界整体恢复或菜单存读档完成。1.1/1.2已完成，其余任务尚未勾选。

基线52/52，另本段新增32/32、受影响既有调度/事件20/20通过，合计104项不同原生测试，均0失败/跳过。完整job、时间及XML哈希见[本段索引](segment-01-native-index.json)和[基线索引](baseline-index.json)。结构变化退出Play Mode，使用普通Unity编译；有两个既有未使用成员警告，未新增程序集或依赖。

| 单元 | 原生结果 | 已验证范围 |
|---|---|---|
| WorldSnapshotRoundTripEditModeTests | 14/14 | 内部版本、时间、最大永久ID、领域目录及重复/缺段/未知版本拒绝；位置和失效ID引用明确还原 |
| WorldSaveCoordinatorEditModeTests | 9/9 | 主线程边界捕获、60秒、请求合并、旧写入不清新脏状态、失败重试、退出强制请求、生命周期取消；实际后台写入仍使用SaveSlotService校验/备份 |
| MachineTaskSnapshotEditModeTests | 5/5 | 部分活动任务按原ID经过JSON恢复，只继续剩余活动；暂停原因、优先级/FIFO、最近100条历史、无效引用拒绝前不改候选 |
| EventServiceSnapshotEditModeTests | 4/4 | 因果与诊断记录经过JSON恢复、无旧事实广播、事件与关联序号连续、坏时刻/关联/序号拒绝、不溢出复用ID |
| MachineScheduling / MachineQueueBoundary | 3+3 | 既有调度合同保持 |
| EventAccountability / EventSessionIntegration | 7+7 | 既有事件合同保持 |

`WorldSnapshotDocument`在ContentJson中记录内部版本及领域段；外层SaveSlotRecord版本2保持。写入只接受已经脱离实时领域的DTO，源必须将快照数据所有权交给保存协调器，不能随后修改。后台不访问Unity对象；向量值通过闭合转换器写入x/y/z，不遍历normalized等属性。加载先验证完整目录，再由相应领域恢复器验证内容，缺段/未知版本不生成空世界。

`WorldSaveCoordinator`拥有单槽、捕获修订和当前写入任务；应用将共享同一SaveSlotWorldSnapshotWriter，后者对各槽串行写入。一次后台成功仅确认捕获修订，期间新脏数据仍待保存。事务IO开始后完成原子替换，生命周期取消只取消尚未开始的工作；失败保留原有效存档并暴露原因。保存退出的封锁操作、停止模拟、重试/返回游戏/强退UI仍待后续接入。

任务记录来自同一MachineTaskQueue，包含活动计数、关闭链、失败标记和暂停前状态；恢复先完整验证局部候选再写入，不补发queued/started/ended事实。世界层仍需验证跨机器/算法/物理行为引用及全局ID唯一性。

B47 D4明确替代CorrelationId的早期“永不持久化”注释：保存关联分配器与事件环形账本，以便任务/结果链继续。关联ID保持独立空间，不作为永久对象ID。候选事件服务恢复静默，只有整个候选就绪才接正式publisher；该世界提交接线尚未实施。

首次最大uint64头转换失败保留于checkpoint-header-uint64.xml，已修正为显式无损解析并复验。测试程序集没有Newtonsoft直接引用，因此通过产品格式API验证，保持现有asmdef。实际文件试验只用带GUID的Temp子目录，递归清理前核验绝对路径与父目录。

后续：算法变量/延迟/历史/待应用请求、效应器与导航物理阶段、货舱责任、候选世界事务、正式菜单/恢复/保存退出。资源生产/能源完整跨域段继续归属B48/B49，缺段时不能静默丢弃。尚无B47 PlayMode/UI或完整G7通过结论。
