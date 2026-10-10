# 当前功能与实施证据索引（2026-10-09）

用户已授权B40～B50十一批改进实施，并追加b07最小唯一归属核心、B44候选试玩初值。当前**7批增量完成，4批已交付独立核心/子集但整批依赖阻塞**。这是代码和验收证据索引，不是第二份任务台账；原任务与G3/G4/G7/G8不得因子集验证被全量标完成，不回写用户工作簿。

## 已完成的改进批次

| 批次/功能 | 状态 | 实际运行范围 | 证据 |
|---|---|---|---|
| B40 算法正式运行驱动 | 本批增量完成 | 真实世界时钟驱动算法、应用/激活/恢复语义与释放路径 | [94项原生验证及边界](../../openspec/changes/b40-p3003-p3005-p3007-p3008-algorithm-runtime-drive/evidence/implementation-20261008.md) |
| B41 传感器与效应器接线 | 本批增量完成 | 按实际组件版本绑定只读输入与权威命令，失效/换装/退订 | [97项原生验证及边界](../../openspec/changes/b41-p2005-p2007-p3006-hardware-runtime-wiring/evidence/implementation-20261008.md) |
| B42 算法工作台闭环 | 本批增量完成 | 真实画布、编辑/应用反馈、模板与参数工作流；1920×1080 | [102项原生验证及边界](../../openspec/changes/b42-p3010-p3011-p3013-p3014-algorithm-workbench-closure/evidence/implementation-20261008.md) |
| B43 资源权威与结算 | 本批增量完成 | 资源库存/扣减/运输入库；追加b07最小批次/拥有者版本/预留/幂等交接 | [106项原生验证及边界](../../openspec/changes/b43-p4001-p4010-p4012-p4013-resource-settlement/evidence/implementation-20261008.md) |
| B44 林木与矿脉生产 | 本批增量完成 | 用户批准试玩初值下的森林/矿脉真实作业，不新增其它平衡规则 | [102项原生验证及边界](../../openspec/changes/b44-p4002-p4003-p4009-p4017-forest-mineral-production/evidence/implementation-20261008.md) |
| B45 生产运输观察 | 本批增量完成 | 锯盘/钻头真实产出→机械臂部分运输→断电保留→卸载结算；真实UI | [114项原生验证及边界](../../openspec/changes/b45-p4011-p4014-p4015-p4016-production-transport-loop/evidence/implementation-20261008.md) |
| B46 能源热路径 | 本批增量完成 | 排序/快照缓存与无变化刷新；独立Windows Development Player电网测量 | [90项原生验证及边界](../../openspec/changes/b46-p5006-p5009-p5010-energy-hotpath/evidence/implementation-20261008.md) |

各行测试数为该批原时间/版本范围，批间复用测试不能相加成当前不同案例总数。B46独立电网Player27组测量不等于B50完整游戏性能通过。B43追加仅最小唯一归属事务核心，未交付完整传送带/物理b07。

## 已实现但整批未完成

| 批次 | 已交付/已验证 | 未满足的前置/验收 | 当前证据 |
|---|---|---|---|
| B47 存读档入口 | 安全格式/写入、隔离恢复、保存退出及真实UI/IO；核心去重354项，B49另复跑共享33项 | 正式IWorldProgressSetup、完整域与正常菜单往返 | [证据](../../openspec/changes/b47-p7002-p7003-p7004-p7011-world-save-flow/evidence/segment-05-dependency-recheck.md) |
| B48 跨域快照/调度 | 资源运输、林矿、能源、公共缓存、跨域门和类型调度核心；176项原生验证 | 真实经济、任务成长、补给及G5/G6完整合同 | [证据](../../openspec/changes/b48-p7005-p7006-p7007-offline-event-foundation/evidence/segment-04-scheduler-and-domain-contracts.md) |
| B49 离线报告/检查点 | 报告、真实提供者合同与原子检查点；91项原生验证 | 全部真实白名单领域提供者、正常入口离线驱动/UI | [证据](../../openspec/changes/b49-p7008-p7009-p7010-p7012-offline-domain-reports/evidence/segment-01-report-and-checkpoint-contracts.md) |
| B50 UI/性能收口 | 冷加载参数释放、设置控件、HUD两入口；51项原生回归，五页500次开关与结构检查 | 完整业务状态/文案、正式Player联合10/50/100与50台60分钟；G7/P8-001前置 | [证据](../../openspec/changes/b50-p8007-p8011-p8012-ui-performance-closure/evidence/segment-01-ui-lifecycle-and-evidence.md) |

生产、运输、存档和UI测试使用明确fixture输入/临时存档目录。真实领域驱动/真实GF界面/文件IO已经验证的部分见各自证据；它们不能证明正式菜单从零开局的完整玩家链。接口、只读合同或检查点不等于所有离线领域已执行，不用空余额/GM/手动Pump代替正式服务。

## 关键代码入口

| 功能 | 项目入口/权威 |
|---|---|
| 算法/硬件 | `Assets/Game/Scripts/AutoEra/Algorithms/`、`Machines/`；B40/B41运行证据 |
| 资源/生产/运输 | `Resources/`、`ResourcePoints/`及Cargo/机器运输领域；B43～B45证据 |
| 能源 | `Assets/Game/Scripts/AutoEra/Energy/`；B46等价与Player基准 |
| 存档及正式入口门 | `Save/WorldSlotFlow.cs`、`WorldSaveCoordinator.cs`、`WorldRestoreTransaction.cs`及域组合；B47/B48 |
| 离线合同 | `Save/OfflineWorldDomainProvider.cs`、`OfflineSettlementReport.cs`、`OfflineWorldCheckpoint.cs`；B49 |
| UI生命周期 | `UI/AutoEraUiNavigator.cs`、`AutoEraUiRuntime.cs`及原生UiLifetimeStressPlayModeTests；B50 |
| UI结构/设置控件 | SettingsForm逻辑/契约/Prefab与UiProto生成器/门1；B50 |

相对代码目录均以`Assets/Game/Scripts/AutoEra/`为根，命名空间`AutoEra.*`。本轮没有改ScriptsBuiltin、程序集/依赖、共享Git索引，没有提交或自动归档。

## 当前验证和已知边界

B50相关10组去重51/51原生回归、五类健康5/5、11批OpenSpec严格校验、纯度/项目边界及diff检查通过。仅1920×1080运行UI；五页根stretch/offset、统一GF缩放、模板inactive及Settings真实控件命中已核对。五页500次后Form6→6、节点2436→2436、UIParams0→0、旧/新MachineRoster监听0→0。

全量门1保留WarehouseForm六项L5宽度声明基线问题；Settings本次新增20项已清零。正式业务未接入页仍有不可用/占位状态，G8与全量文案验收未通过。CS0219/CS0414历史编译警告仍可能随程序集重编译出现。

后续恢复顺序：交付所缺经济/农业水务/施工制造/任务成长等权威服务及正式开局配置→B48完整域→B49真实离线→B47正常新建/继续往返→B50联合Player性能与60分钟稳定性。超出本计划NonGoals的业务需另立范围，不能在本轮擅自补建。

正式来源：[项目基线](ProjectBaseline.md)、[改进总计划](ImprovementPlan-20261008.md)、[用户确认摘要](ImprovementPlanDecisionSummary-20261008.md)。历史交接内容保留日期，最新状态以本索引和各change的tasks/evidence交叉复核。
