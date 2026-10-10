# B43 资源唯一归属与结算增量交付

状态：本批10项实施任务完成。用户批准的b07最小前置核心已由同一权威实现；不声明b07全量、完整P4/G4/G7完成。环境：Windows10、Unity2022.3.62f3c1、核验主工程UnitySkills8091；普通编译，非FSR。历史独立合同检查点见[checkpoint](checkpoint-independent-20261008.md)。

## 实际实现

- CargoOwnershipAuthority为世界唯一货物权威，登记永久批次/事务ID、唯一拥有者及版本、来源和目标容量预留、任务责任、精确幂等结果。竞争预留不能超卖；部分交付拆出新批次，来源剩余批次保持版本，整批交接保留ID并递增版本。
- Commit/Cancel只在安全边界修改；取消释放未提交部分，目标容量/可用性变化递增代次并解除相关预留。重复事务完成序号返回原结果，过期未见序号等待而不记账。结果先登记再通知；观察者异常不能把已提交事务变成可重复提交，通知期间禁止重入写入。
- ResourceWorldService随世界创建/释放。正式RegionMachineRuntimeRegistry借用其MachineCargo只读投影，UI或执行上下文释放不丢货。两台机器转移时，两侧投影和UsedCapacity先统一更新再通知。容量由真实载体与已装货舱硬件决定，满载不能拆卸扩容部件。
- ResourceItemCatalog读取FirstVersionObjects身份分类。普通资源实际入库时从物理批次扣除并增加世界余额，不能反向取出；种子/产品等保持所属仓库库存和永久ID。实体仓库容量120来自BuildingDefinitions50011，不按物品另建容量分区。
- 封装组件/载体使用实际MachineRoster实例ID；在货物中时不能同时作为库中可用件、安装件或部署载体。组件占运输单位，入仓后返回同一组件库实例且不占实体仓位；载体不能进入普通货舱，入仓后返回同一机器实例。禁止未验证、错型号、已安装或重复封装的payload。
- 初始Warehouse.prefab通过原生PrefabUtility菜单接入RegionWarehouseFacility，正式InitialRegionScene在场景初始化时建立绑定；只开关视图不改变库存。领域移除使新交付失效，已经交付的货物/余额仍归世界持有。迁移重复执行SHA256不变：80d62fbeb96c933637f68aa4c87c83726b5a35c9dda7f599324c2f595b5d1d5e。
- 预留产生责任关联，提交/取消发布带真实任务、事务、源/目标和数量的ResourceTransferFactEventArgs；重复完成不重复事实。ResourceInventorySnapshot在完整提交及通知之外取脱离运行时的只读副本，包含存活批次、payload、容器代次/容量、未提交预留、累计数量、幂等结果和余额。跨域保存/恢复继续由B47/B48完成。

## 本次原生验证

| 测试组 | 原生XML |
|---|---:|
| ResourceTransferSettlementEditModeTests | 19/19 |
| ResourceWorldCargoEditModeTests | 9/9 |
| ResourceTransferProductionPlayModeTests | 1/1 |
| ResourceTransferContractEditModeTests | 6/6 |
| AlgorithmCargoTaskQueryEditModeTests | 5/5 |
| MachineExecutionContextEditModeTests | 5/5 |
| HardwareRuntimeWiringEditModeTests | 10/10 |
| MachineRecoveryEditModeTests | 2/2 |
| MachineDeploymentRuntimeEditModeTests | 5/5 |
| MachineReadModelEditModeTests | 13/13 |
| ComponentReadModelEditModeTests | 14/14 |
| MachineHardwareUiEditModeTests | 6/6 |
| PersistentObjectRegistryEditModeTests | 3/3 |
| ProductionAlgorithmDriveEditModeTests | 8/8 |

合计**106/106**，14组JSON与同次Unity原生XML，机器可读索引见[native-test-index.json](native-test-index.json)。新增测试均重新发现。随机守恒为3个固定种子各400轮：初始生产输入=来源实体+机器实体+转换余额；覆盖部分提交、取消、重复回调和竞争容量。

普通Unity编译0错误；完整重编译存在2项原有未使用字段/变量警告，增量编译报告按实际程序集输出。2026-10-08 14:56项目健康检查5/5 PASS，引用无missing scripts/references，数据表12/12，原报告副本同目录。框架纯度与项目边界审计、OpenSpec strict通过。代码/文档diff检查通过；Unity原生Prefab序列化新增两个空字符串字段的尾空格，Prefab检查仅关闭blank-at-eol，不手改YAML结构。源码及资产SHA256见[source-sha256.json](source-sha256.json)。

## 逐场景对应

| spec Scenario | 实际证据 |
|---|---|
| Competing consumers | Settlement.CompetingConsumers、SourceCompetition；实际来源与共用容量预留，非DTO代替 |
| Repeated completion | Settlement.RepeatedCompletion；WorldCargo.QuantityAndTerminalFacts；FormalRegion重复交付不重复入余额 |
| Destination becomes full | 合法预留保护承诺容量；Settlement.GenerationChange验证容量变化取消未交付部分且保留已交付量 |
| Cancel after partial transfer | Settlement.PartialCancel、Randomized；WorldCargo真实终止事实保留累计量与任务ID |
| Ore enters warehouse | WorldCargo.RealCargo与FormalRegion：真实机器货舱10/12单位矿石交付，实体减少、余额等量增加、仓位不增加 |
| Physical item remains local | Settlement.WarehousePhysicalItems：同批次ID入所属仓库，另一仓库无法读取或直接取用 |
| Packaged component follows library routing | WorldCargo.PackagedComponent；组件读模型新增用例：运输中不可安装/选中，入仓后同ID回库 |
| Proxy is culled | FormalRegion在未提交交付预留已建立时隐藏真实机器视图，数量、owner/version、任务与剩余责任不变；释放区域后结果和余额仍保留 |

正式集成从Launch加载实际InitialRegion并由生产入口创建资源、仓库和机器运行时；测试只准备机器/初始货物输入并发送领域转移命令。未注入仓库服务、第二份库存或替身Executor。运行断言1920×1080，无新UI布局或其它分辨率检查。

## 边界、回滚与后续依赖

快速执行候选已核验：所有权、事件顺序及正式场景生命周期包含专业判断，Unity8091验证现场不可安全拆交；原窗口直接执行。不修改ScriptsBuiltin、asmdef、依赖、FSR设置、xlsx、Git索引；未自动提交或归档。

本批完成统一资源交接边界。机械臂实际作业/固定路线导航与持续生产闭环由B44/B45完成；未实现传送带状态机、下游拉取、物理代理制作、完整经济/农业/水泵，也未以测试准备货物证明真实采矿产出。金币继续遵守直接余额规则，本批不伪造经济发放入口。

资源快照目前提供安全不可变数据，不等于已写入玩家存档；B47/B48必须保存/恢复实际货物、预留、责任和幂等结果，不能仅恢复Machine.UsedCapacity。当前未对玩家存档做格式迁移；回滚先保存/识别新资源数据，不能通过清零占用处理未知货物。b07其余任务保留原状态。B44依赖的唯一归属核心已具备，可继续两类真实生产；完整阶段门仍保留任务表的外部前置。
