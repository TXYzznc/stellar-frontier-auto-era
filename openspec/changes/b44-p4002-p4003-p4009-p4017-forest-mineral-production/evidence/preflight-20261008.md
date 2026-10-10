# B44 实施前核验

2026-10-08，主工程D:/unity/UnityProject/stellar-frontier-auto-era；Unity 2022.3.62f3c1，UnitySkills8091。已读取conventions及客户端/主程/QA职责，结构变更使用普通编译，停止Play Mode。B41/B43已交付增量证据；原始完整阶段门和正式视觉门仍独立保留。

数值依赖已由用户批准，见proposed-numerics-20261008.md。逐树规则继承DEC-172～179/182～184/187/192/203；旧固定产速、矿脉悬崖钻探和缓存满停止规则不恢复。

快速执行候选检查：本单元包含尚需落实的生产原子性、树引用、可达性与安全点专业判断，并使用当前独占Unity现场；不能安全拆成机械执行包。主对话按专业职责直接实施，不启动子agent。

允许精确路径（新增文件落在对应目录，随实现记录文件清单）：

- Assets/Game/Scripts/AutoEra/ResourcePoints/：配置、逐树/矿脉状态、生产服务和实际效应器。
- Assets/Game/Scripts/AutoEra/Resources/CargoOwnershipAuthority.cs、ResourceInventorySnapshot.cs及生产回执合同：扩展B43同一权威，生产幂等提交与地面合批。
- Assets/Game/Scripts/AutoEra/World/AutoEraWorldSession.cs、World/Identity/PersistentObjectRegistry.cs。
- Assets/Game/Scripts/AutoEra/World/Region/InitialRegionScene.cs、SensorPublicDataContracts.cs、RegionSensorReadProvider.cs及必要作业接入。
- Assets/Game/Scripts/AutoEra/Machines/MachineInstance.cs、MachineScheduling.cs与实际功率/强类型目标接入。
- Assets/Game/Scripts/AutoEra/Algorithms/：仅固定类型树网格读取/选择及稳定目标参数的最小合同，不开放脚本或任意对象。
- Assets/Game/Scripts/AutoEra/Motion/MotionWorkBridge.cs与实际作业表现接入。
- Assets/Game/Scripts/AutoEra/Editor/ForestMineralProductionMigration.cs。
- Assets/Game/Config/ResourceProduction/ForestMineralProduction.asset（新配置，由Unity原生API创建）。
- Assets/Game/Prefabs/Entity/InitialRegion/Forest.prefab、MineralVein.prefab（Unity原生API修改）。
- Assets/Game/Prefabs/Entity/Machines/WheeledCarrier.prefab、FixedRotaryCarrier.prefab：显式ProductionToolMounts安装锚点，复用批准工具资产。
- Assets/Game/Prefabs/Entity/Machines/RotarySaw.prefab、RotaryDrill.prefab：修复旧mount_yaw为空兄弟节点的问题，使批准VisualModel由既有转向关节控制；保留批准绑定姿态、行程、网格与Stable ID，通过Unity原生API修改。
- Assets/Game/Scripts/AutoEra/World/Region/RegionNavigation.cs、RegionHardwareRuntime.cs、InitialRegion.cs、RegionObject.cs：逐树障碍、作业等待原因、显式附属设施清退。
- Assets/Game/Scripts/AutoEra/World/Region/InitialRegionEntity.cs：GF实体池的生产视图解绑；不释放世界权威树/矿脉/货物。
- Assets/Game/Scripts/AutoEra/UI/Integration/AlgorithmReadModel.cs、Assets/Game/Tests/AutoEra/Editor/AlgorithmInitialTemplatesEditModeTests.cs：固定树网格节点标签与正式模板费用14合同回归。
- Assets/Game/Models/植株/ForestTree01_Optimized.fbx.meta：仅通过ModelImporter启用已有批准模型的可读网格，支持运行时真实断面裁切；不修改模型源文件。
- Assets/Game/Tests/AutoEra/Editor/ForestMineralProductionEditModeTests.cs及本批正式入口集成测试。
- 本change证据及总计划/决策摘要的增量同步。

禁止：ScriptsBuiltin、asmdef、第三方依赖/FSR、xlsx写入、Git索引/提交、其他资源点玩法；不手改生成C#或Unity YAML。配置资产是不可变配置来源，不承载运行状态。

基线：首次误用不存在的简写过滤名HardwareRuntimeWiringTests导致Test Runner零发现挂起；已终止该脚本并普通重编译恢复，零测试不能作为通过证据。后续过滤名先从实际类定义/发现结果核验。

既有基线27项原生测试：HardwareRuntimeWiringEditModeTests（10）、ResourceWorldCargoEditModeTests（9）、ProductionAlgorithmDriveEditModeTests（8）。EnterPlayMode测试的REST重连可早于测试完成而超时；以启动后生成、匹配fixture的最终原生XML为测试证据，REST故障独立保留。
