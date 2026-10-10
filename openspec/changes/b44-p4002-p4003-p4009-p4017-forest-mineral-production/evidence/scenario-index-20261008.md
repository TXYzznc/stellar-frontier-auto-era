# B44 场景与证据索引

基线环境：主工程Unity 2022.3.62f3c1，已核验8091，1920×1080。普通编译；无FSR结构迭代。前置回归27项的原生XML/JSON位于本目录。

| Spec Scenario | 实际测试入口 | 当前证据边界 |
|---|---|---|
| Resume cutting | ResumeCut_UsesOriginalTreeAndProducesOnceAfterSettlement；正式宿主测试的VerifySaw | 领域与最新正式宿主验证通过，含断电、取消和同树恢复 |
| Target removed | MissingTree_IsPermanentInvalidationAndNeverSubstitutesNeighbour | 永久失效不替换对象 |
| Visual count changes | MineralVisibility_UsesCeilingAndStableSeedRanking；MineralTier_StockAndDisplayAreIndependent | 权威储量与确定性显示映射；不声称矿脉正式美术通过 |
| Deposit cleanup | DepletionCleanup_IsNextWorldDayAndRetainsGroundCargo；ResourceCleanup_RemovesOnlyExplicitAttachmentsAndRetainsUnrelatedBuilding | 换日、地面货物、附属设施与无关设施分别断言；新增测试重新发现，25/25领域验证通过 |
| Power changes mid unit | MidUnitPowerChange_ConservesFractionAndLinearDamage；正式宿主VerifyDrill/VerifySaw | 最新正式宿主1/1通过，原生XML含断电与取消恢复 |
| Ground display cap | GroundDisplayCap_DoesNotStopLogicalOutputAndReservationSurvivesMerge | 逻辑不因显示封顶停产；本批未交付完整b07货物代理视觉 |
| Production updates sensor | 正式宿主VerifyDrill及任务/行为生产事实断言 | 正式区域Advance驱动；测试只准备装配、部署、绑定与图输入 |

失败留痕：checkpoint-tool-readiness-timeout.xml、checkpoint-onshow-view-not-discoverable.xml、checkpoint-contact-used-entire-tool-bounds.xml及旧模板费用失败XML。未通过结果不覆盖或改写为通过。最终102/102原生结果、源码哈希和健康门见implementation-20261008.md。
