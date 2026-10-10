# B43 实施前核验

用户已授权十一批实施，8091为已核验主工程，B40～B42增量已完成。G3完整正常新进度综合入口仍待B47，未回写xlsx或把B42子集作为G3签收。允许先推进无依赖的领域合同、边界修复与测试；正式阶段验收保留原门。

现状：MachineCargo只有按类型数量字典，MachineExecutionContext写死容量100；AutoEraWorldSession无资源余额/仓库服务。b07 CargoLot、唯一拥有者版本和原子交接均无代码/原生运行证据，b07 tasks全部未勾。按design D3，未经范围调整先只完成领域接口与契约测试，不创建临时竞争权威。已向用户提出最小前置补齐选项，答复前不实现该核心。

允许独立单元：Resources/ResourceTransferContracts.cs（AutoEra.Logistics命名空间，避免遮蔽UnityEngine.Resources）、Buildings/WarehouseClassification.cs、MachineCargo.cs及AutoEra测试、B43证据/设计/spec/任务文档。生产接入待b07核心授权及可验证实现后再登记精确文件。禁止ScriptsBuiltin、asmdef、依赖/FSR设置、场景/Prefab、xlsx、Git索引。

快速执行候选检查：资源权威与不重复结算的专业判断尚在本批实现边界内，不能作为冻结机械包交接；8091测试由主窗口独占，拆包/复核成本不低于自行完成。主窗口直接执行并复核。

容量独立单元补充范围：MachineExecutionContext.cs，引用既有MachineInstance.TotalCapacity与硬件拆卸UsedCapacity门禁。同步真实货舱占用，不创建货物归属权威，不新增容量数值或部件行为规则。

基线：AlgorithmCargoTaskQueryEditModeTests 5/5、MachineExecutionContextEditModeTests 3/3，JSON与同次原生XML保留。发现独立边界缺陷：TryLoad用Used+amount检查容量可整数溢出；Items返回可强制转换的可变字典，外部可绕过Used/Changed。

继承最新通用规则：14-第一版行为容量与结算规格明确组件入库进入组件库、载体进入机器库，不占实体仓库容量。B43示例“种子或组件等实体物品留仓”中的组件示例与正式来源不一致，按继承规则修正为种子/农产品，不另询问既有默认行为。
