# B49 场景索引（实施中）

前置：B48去重176项原生证据、B45两个真实生产运输闭环、B47保存退出及候选核心。2026-10-09报告/原子检查点合同91项原生验证通过，实际范围见segment-01-report-and-checkpoint-contracts.md。

| spec场景 | 执行入口 | 当前边界 |
|---|---|---|
| Storage becomes full | 真实货物权威/运输提供者及Online/Offline对照 | 提供者待实现，不按时间乘收益 |
| Fuel runs out | 下一能源边界与领域对照 | 待实现 |
| Catchup stops mid journey / No valid path | 原导航路径和责任恢复 | 真实提供者待实现，不瞬移 |
| Recipe not selected / Reward requires claiming | 必需领域能力门与原决策合同 | 实际施工/成长未交付，完整验收保持阻塞 |
| Reopen report / Crash near completion | OfflineSettlementReportEditModeTests / OfflineCheckpointEditModeTests | 合同及真实文件IO通过；完整实际领域报告/正式继续入口仍待依赖，不在报告读取时执行结算 |
| Budget spread over frames | 真实进度UI与命令封锁 | 待接；运行只1920×1080 |
