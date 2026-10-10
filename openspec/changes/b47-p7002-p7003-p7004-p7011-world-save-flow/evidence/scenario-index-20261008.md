# B47 场景与证据索引（实施中）

环境与允许路径见preflight；基线原生52/52见baseline-index.json。本索引随领域/入口接入更新，未验证的综合场景不标通过。

| spec场景 | 当前证据 / 后续入口 | 边界 |
|---|---|---|
| Save during work | 任务/算法/算力/导航/传感/行为、原生产与转移操作、区域/宿主组合及待停机硬件意图；增量02～04 | 保存原责任和参数，恢复原操作；完整数量/能源领域段待B48 |
| Recover without startup | AlgorithmPersistentSnapshot、EventServiceSnapshot、RegionExecutionSnapshot、WorldRestoreTransaction | 候选和宿主静默恢复、运行不重播Startup；正式菜单完整跨域往返待B48/B49 |
| World changes during write | WorldSaveCoordinatorEditModeTests.OldWriteCannotClearNewDirtyRevision_RequestsCoalesce | 人工延迟writer验证修订与串行合并；BackgroundWriter用实际SaveSlotService验证文件 |
| Interrupted file write | 既有SaveRollingBackup基线15/15；后续写入故障注入 | 保留最后有效文件；正式故障集成待实现 |
| Reference missing | 任务/导航/传感失效ID、WorldRestoreTransaction跨领域ID与候选失败测试 | 不按名匹配，失败候选不污染原世界；完整领域适配待B48 |
| Unsupported snapshot | WorldSnapshotRoundTripEditModeTests版本/缺段/重复目录/坏头测试 | 内部版本及完整必需段校验；领域内容由相应恢复器继续校验 |
| Save exit fails | WorldSaveExit6/6、WorldSaveFlowPlayModeTests实际GF表单和文件失败/重试1/1 | 返回游戏解锁、两次强退及写出有效存档已验证；边界输入为测试准备 |
| Offline work pending | WorldRestoreTransaction槽位门；后续B49正式续算 | 未就绪离线不能跳过，检查点恢复意图和锁定UTC已验证；完整推进待B49 |

checkpoint-header-uint64.xml保留首次最大永久ID转换失败；最新格式原生16/16通过，含领域必需字段校验。测试程序集不直接引用Newtonsoft，保持现有asmdef；通过产品格式API验证。增量02的17组146/146保留历史记录；最新增量03为18组156/156，本批去重290项，见[组合恢复记录](segment-03-region-and-runtime-composition.md)与[原生索引](segment-03-native-index.json)。区域/原操作/适配器/宿主责任已有专项验证；完整货物数量、能源和整世界正常入口仍待后续集成。

最新增量04为16组118/118，B47累计去重354项；实际1920×1080保存退出及文件IO通过，见[增量04](segment-04-world-transaction-and-exit.md)与[原生索引](segment-04-native-index.json)。完整数量/能源、正式开局和离线依赖继续保留，不勾选整批综合验收。
