# B44 林木矿脉生产增量交付

状态：本批10项实施任务完成，追加试玩初值按用户2026-10-08授权录入。Windows10、Unity2022.3.62f3c1、核验主工程8091；普通编译，未使用FSR。B43同一资源权威承接真实产出；不声明完整b07、P4-017、G4或正式矿脉美术完成。

## 实际实现

- 世界会话拥有逐树/矿脉权威，视图隐藏或释放不重置永久树ID、储量和已产生货物。树生长使用分段解析积分，受伤恢复为高度增长量两倍；砍伐比例0～25%、木材按真实上半段长度、整数与小数余量、120秒原位树桩恢复分别验证。
- 普通伤害按稳定树ID/命中序号判定低血量倒伏，精确锯盘与普通伤害合同分开；重复命中拒绝冲突，永久失效树不自动替换为邻树。固定树网格Filter/Select/Read与Object(Tree)引用进入算法类型系统，不开放任意对象或用户脚本。
- 矿量100/250/500与显示6/8/10继承DEC-203；按稳定种子/ceil映射显示。真实钻探有效贡献按伤害/阻力累积，0～1功率伤害线性、耗电平方；暂停保留未完成贡献且不补算断电时间。耗尽后下一世界日只清退资源点及显式附属设施，保留地面货物和无关建筑。
- 正式宿主创建真实效应器、GF工具实体、MotionRig与MotionWorkBridge作业通道。机器必须停稳、在合法作业区、取得通道、满足真实锯盘转向/升降/进给或钻头压钻范围及净空后才贡献产出。错误绑定或未就绪工具显示真实原因。
- 修复批准工具Prefab旧mount_yaw为空兄弟节点的问题：通过PrefabUtility将既有VisualModel挂到既有转向关节，保留网格、绑定姿态、行程与Stable ID；运行时验证关节确实控制头部几何。工具安装点由载体Prefab显式配置，无运行时假锚点。
- 批准树模型通过原生ModelImporter启用可读网格，并按真实断裂高度裁切上下模型。树根源场景坐标与LOD4宽大impostor不能作树干测量，已归零内容实例位置并按LOD0测量树干。上半段以真实截面为枢轴，ConfigurableJoint保持连接，在只含批准地面和倒伏上半段的独立PhysicsScene中倒伏；不伤机器、不影响导航、不决定资源数量。落稳/10秒安全超时才交B43事务提交。
- 同类地面产物合并为永久货物批次，显示封顶不阻塞权威数量；分数贡献不制造每帧货物回执。生产回执与领域状态先原子提交，再发布带真实机器、组件、任务和行为的事实。观察者异常/重复提交不会增产，合批保留既有预留承诺。
- 真实传感器读取储量/缓存/逐树集合；采伐与钻探初始模板费用均按现行统一数值模型14核验。新的SO只承载批准初值，加载后复制成不可变ProductionRules；不手改生成数据表C#。

## 原生验证

| 测试组 | Unity原生XML | REST job |
|---|---:|---|
| ForestMineralProductionEditModeTests | 25/25 | 04d3c479 |
| AlgorithmInitialTemplatesEditModeTests | 6/6 | ac86dea0 |
| AlgorithmEffectorNodeEditModeTests | 5/5 | 720c3317 |
| HardwareRuntimeWiringEditModeTests | 10/10 | 3cf24731 |
| ResourceWorldCargoEditModeTests | 9/9 | d9ed9a7e |
| ProductionAlgorithmDriveEditModeTests | 8/8 | f61c2a96 |
| SensorPublicDataEditModeTests | 3/3 | 38d5e058 |
| MachineEnergyConsumerEditModeTests | 17/17 | bfc38c0a |
| MotionWorkBridgeEditModeTests | 5/5 | 25f0fe46 |
| FunctionalRigMotionGraphCatalogEditModeTests | 12/12 | 675402f0 |
| ForestMineralProductionPlayModeTests | 1/1 | be693c2e |
| RegionNavigationIntegrationTests | 1/1 | 0363eec7 |

合计102/102，12组匹配fixture的原生XML，索引见[native-test-index.json](native-test-index.json)。长EnterPlayMode运行的REST回调可能在域重载后失去原job，原始JSON保留该失败；结论来自启动后生成的最终Unity原生XML，而非REST零项结果。

真实宿主集成只准备装配、部署、绑定及算法输入，entry.Advance负责能源、导航、队列、生产和采样；未注入执行器或另建库存。实际矿石2单位、剩余98，传感器与任务事实一致。锯盘断电恢复、取消当前行为并以新算法实例继续同一棵树、真实上下网格截面/独立物理枢轴、倒伏后木材2单位与单次提交均断言。释放现场后资源仍由世界持有。原生导航回归覆盖重建和反复入场。

项目健康5/5，missing scripts/references=0，数据表12/12；原报告副本与健康输出同目录。普通编译0错误，完整编译两项既有未使用字段/变量警告单列，不声称零警告。框架纯度、项目边界、OpenSpec strict与diff检查通过。源码/资产哈希见[source-sha256.json](source-sha256.json)，重复迁移哈希不变见[migration-idempotence.json](migration-idempotence.json)。

## 视觉与范围边界

实际世界视口截图[钻探](drill-production-1920.png)、[倒伏](tree-felling-1920.png)均1920×1080，已查看。它们仅证明本批场景表现，不代替正式UI操作或美术验收。批准树模型已接真实裁切；断面封口材质、矿脉正式模型、b07三档物理货物代理和整套G4视觉验收仍未交付。本批不新增UI Prefab或运行其他分辨率；现场/仓库UI由B45接入并检查结构适配。

失败过程保留checkpoint XML：真实工具型号/OnShow、刀头范围、树模型源坐标与树干半径、GF池重绑定、机器作业区、yaw标记与刀盘范围等；修复后的证据另存，未将失败改成通过。

快速执行候选按preflight核验：作业、安全边界及真实工具/物理现场包含专业判断，独占8091无法安全拆交，原窗口直接执行。未修改ScriptsBuiltin、asmdef、依赖、FSR、xlsx或Git索引，未自动归档。

回滚：撤收视图/行为先释放动作和作业占用，世界已提交货物与资源状态保留；不能清零库存或重新生成树/矿量。新生产快照尚未进玩家存档，由B47/B48保存树阶段、小数贡献、货物与责任；未做玩家存档迁移。本批只接两类生产，运输闭环/仓库与现场页继续B45，农业、水泵、传送带和完整G4前置保留原任务边界。
