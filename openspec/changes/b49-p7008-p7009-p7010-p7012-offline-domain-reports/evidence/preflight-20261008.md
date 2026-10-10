# B49 实施前核验

2026-10-08，用户已授权全批实施。主对话承担客户端/QA专业职责，无子agent、Git索引/提交或xlsx写入。Unity2022.3.62f3c1，已核验8091；结构修改退出Play Mode、普通编译。沿用已读取角色、conventions、OpenSpec、Unity、保存序列化规范。

B48独立类型化调度/检查点及资源、生产、能源、公共缓存通过去重176项原生测试，五类健康门通过，具体源XML沿用该change/evidence/segment-04-native-index.json及前置各段索引。此次刚完成的相同版本证据可复用，不机械重复未改模块。

实际依赖：经济、成长、日常补给、农业/水泵、施工制造及完整b07未交付，B48整批依赖阻塞。B49仅可接已交付实际规则及缺失门、报告/检查点/进度合同，不用空模块或平均收益绕过完整白名单。活动机器、算法与导航必须有真实下一事件提供者才允许推进；没有提供者拒绝跳过，不静默当作闲置。

第一执行单元精确允许：Save/OfflineWorldDomainProvider.cs、OfflinePassiveWorldProviders.cs、OfflineSettlementReport.cs、OfflineContinuationSnapshot.cs、OfflineWorldCheckpoint.cs；ResourcePoints/ResourceProductionOfflineEvents.cs及ForestMineralProductionConfig.cs（复用生长解析模型求下一个阈值）；Energy/EnergyOfflineEvents.cs；World/Region/RegionEnergyPersistence.cs（从区间起点积分，再在边界重判日照/供电）；Save/WorldSaveCoordinator.cs、SaveSlotService.cs（复用同槽串行锁、原子备份和外层离线标记，保存原锁定UTC水位）；必要新增持久化partial。UI后续在核对绑定合同与具体节点后列精确文件，不手改YAML。

只在1920×1080进行实际UI运行检查；结构仍检查anchor/pivot/stretch和已有统一缩放。测试Temp目录必须绝对父目录与专用前缀双重校验，禁止真实玩家档故障注入。域测试与实际UI/文件IO分别报告，不把接口fixture当作正式开局/G7证据。

快速执行候选：领域阈值、同刻顺序、恢复事务、输出/报告防重和共享Unity现场仍涉及专业判断，不能安全拆交；主对话实施并复核。自然增量边界不结束执行；完成安全范围后登记真实依赖并继续B47/B50。

2026-10-09增量：实际本单元为领域能力门、报告与原子检查点合同，未接通实际生产/机器/导航提供者。新增测试精确文件：Assets/Game/Tests/AutoEra/Editor/OfflineSettlementReportEditModeTests.cs、OfflineProviderContractEditModeTests.cs、OfflineCheckpointEditModeTests.cs。真实文件IO在系统临时目录AutoEraOfflineCheckpoint-随机后缀下，删除前核对绝对父路径和专用前缀。未来阈值/进度UI文件尚未创建，不将允许清单当作已经实施的文件清单。
