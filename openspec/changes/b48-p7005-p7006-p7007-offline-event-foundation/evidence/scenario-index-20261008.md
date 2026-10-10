# B48 场景与证据索引（实施中）

基线七组102项，增量04累计去重176项原生测试通过。完整依赖及写范围见preflight；独立核心证据见segment-04-scheduler-and-domain-contracts.md。B48整批仍待真实经济/成长依赖，未完成项不勾选。

| spec场景 | 当前证据 / 后续验证 | 边界 |
|---|---|---|
| Transaction pending | CargoOwnershipSnapshot7/7、ResourceWorldCargo13/13、ProductionTransport21/21 | 原资源预留/回执/托管/责任已验证；经济购买和奖励领域未就绪 |
| Required domain absent | OfflineEventDeterminism缺模块门；GameplayDomainSnapshot缺段/下游拒绝；WorldRestoreTransaction15/15 | 不以空段替代经济/成长 |
| Same initial state different budgets | OfflineEventDeterminism15/15，注册逆序与预算1/100逐轨迹一致 | 调度合同测试，真实领域全白名单归B49 |
| Obsolete event | OfflineEventDeterminism原代次丢弃、负载隔离 | 真实领域判定有效性 |
| Interrupted catchup | 同刻分批/取消/原runId和UTC恢复/算法保护代次重绑定 | 内存与可序列化检查点；实际文件IO归B49 |
| Frame budget reached | 预算1的多Pump与半批拒绝快照 | B49接实际UI与命令封锁 |
| Local date rolls back | OfflineSettlementMarks的日期/事务合同测试 | 日常补给领域未交付，不执行领取 |
| No progress algorithm | 零时间同修订循环保护、真实进展不暂停、另一领域继续、保护状态恢复 | 真实算法观测/停止/报警提供者归B49，不新增频率阈值 |
