# B46 实施前核验

用户已授权实施；主工程8091已核验、Play Mode关闭。B43～B45因b07前置未就绪进入阻塞，依总计划执行无强制计划内前置的B46。

已读取本批proposal/design/spec/tasks、conventions；机器负载、电网和区域接线均有生产代码及既有原生测试。开始前复跑EnergyGrid、MachineEnergyConsumer、RegionEnergyService、EnergyReadModel和RegionEnergyPlayMode组保存基线。历史证据不当作本次测试。G5商品/出售/升级等仍未在本批验收。

允许精确文件：EnergyGrid.cs、RegionEnergyService.cs、InitialRegionScene.cs、EnergyReadModel.cs；为生命周期通知允许MachineRoster.cs/MachineRosterSnapshot.cs，为无广播的活动状态读模型允许MachineInstance.cs（仅修订计数，不改功率规则）；AutoEra相关测试、Editor/Support/EnergyGridReference.cs；tools/benchmarks/energy_player_benchmark.py及其两个C#输入脚本；本批evidence/tasks和总计划状态。

性能构建使用Temp中的隔离Unity2022.3工程，复制真实EnergyGrid/MachineEnergyConsumer/MachineInstance及其最小定义闭包，不改变主工程PlayerSettings、asmdef、依赖或FSR。基准是领域电网Player，不等于完整产品或B50联合性能门。禁止ScriptsBuiltin、场景/Prefab、xlsx、Git索引。

快速执行候选检查：排序/历史快照及浮点等价仍需专业判断；8091测试现场由本窗口独占，拆包/交接/复核成本不低于直接执行。主窗口负责实现与复核。基准冻结后的重跑随本单元完成，不自动建立子agent或Git任务。
