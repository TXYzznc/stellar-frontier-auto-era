# B40实施记录（2026-10-08）

用户已明确授权实施B40～B50。当前Active为B40，其余按依赖顺序记录于本地client队列。UI仅1920×1080运行验收，Prefab结构考虑适配。

## 工程和前置核验

UnitySkills8091返回工程路径`D:/unity/UnityProject/stellar-frontier-auto-era/Assets`，Unity2022.3.62f3c1，instanceId`_1D033F4D`，full/bypass。实施前非Play Mode、非编译，控制台0错误。初始工作区仅有本轮规划文档；任务表只读，不操作Git索引。

原始任务依赖和b15/b17/b25/b26合同已核对。工作簿G0/G1/G2未登记用户阶段验收，不能宣称阶段门通过；本轮完成明确领域与宿主接线及回归，完整阶段验收独立保留。

实际修改：RegionMachineRuntime.cs、RegionMachineRuntimeRegistry.cs、InitialRegionScene.cs、AlgorithmMachineAdapter.cs、AlgorithmInstanceService.cs、AlgorithmRuntime.cs、AlgorithmEvaluation.cs、AlgorithmReadModel.cs；新增两个正式驱动回归类并修正现有读模型/传感集成测试准备。相关回归位于Assets/Game/Tests/AutoEra/Editor/，以及本change证据/任务状态。无新程序集/第三方依赖，不修改ScriptsBuiltin、Unity场景/Prefab或Excel。

快速执行候选检查：原子激活、实例隔离和回调时序尚需按现有API落实专业判断；验证与当前Unity现场不可安全分离，由主对话直接执行并复核。

## 基线

代码缺口：区域未推进算法实例与适配器；读模型先Attach再CompileDraft；一个适配器限制一个实例。旧集成测试自行创建/Pump服务，不能证明生产入口接通。

组合过滤请求17aad04d未匹配UnitySkills单一组名过滤，没有有效结果，不计为基线通过。完整发现1598个EditMode测试后，按明确类名串行运行。

- AlgorithmInstanceEditModeTests：e5e66126，13/13通过。
- AlgorithmExecutionEditModeTests：a7e97296，8/8通过。
- 其余实际结果及原生XML随后补齐，未实测场景不提前勾选。

## Scenario映射

| 场景 | 验证入口 | 状态 |
|---|---|---|
| Close observer while running | ProductionAlgorithmDrivePlayModeTests.FormalHost_DrivesRealNavigation_AfterObserverCloses | PASS，真实区域创建服务，仅Advance推进，关闭HUD后到达目标且任务只完成一次 |
| Stable iteration during removal | CallbackChangesRegistry_UseStableOrderAndStepBoundary | PASS，反向登记仍按永久ID推进，撤收在步末释放，新部署下一步启动 |
| Activation rejected | CapacityRejection / RevisionChangesDuringCommit，读模型虚构绑定拒绝 | PASS，修订及容量失败无挂接/占用，修正后可重试，重复激活不拆旧实例 |
| Startup is not duplicated | WorldStep_StartsOnce_AndRestoringDoesNotReplayStartup | PASS，首次运行一次；恢复及后续世界步不重放 |
| Invalid edit preserves running plan | AlgorithmInstanceEditModeTests.DraftAndPendingAreIsolated_StaleAndHardwareRevisionReject | PASS，陈旧修订/硬件变化拒绝，旧应用版本保留 |
| Independent pause reasons | ApplicationPause_DoesNotClearPowerPause_AndDraftPeerDoesNotBlockSafePoint及原领域暂停回归 | PASS，结构应用完成不清除断电暂停，未激活草稿不阻塞整机安全点 |
| Machine leaves region | Detach_ClearsOwnedRequestsAndCompute_AndLateAdvanceIsHarmless、MultipleInstances_DeleteOneWithoutCancellingPeer | PASS，撤收释放占用，后续推进无效；删除一个实例保留另一个实例 |

本证据只据实际测试结果更新，不宣称完整G3或玩家生产闭环完成。

## 实现结论

区域业务步先同步时间、结算能源和推进唯一导航入口，再采样、驱动按机器永久ID排序的算法集合。算法结果缓冲后在可控批次回传；区域和机器推进均防重入，回调的集合变更延后处理。稳定步不复制机器集合。

激活/应用由机器入口承担。激活验证剩余逻辑容量、实际绑定与插槽修订，提交过程中再次核验；失败归还容量并清理候选。算法触发携带独立实例身份，共享导航和任务权威；删除实例只取消自身工作。硬件修订仅追踪插槽身份和组件开关，不把正常算力占用/供电/名称变化当作硬件变化。

修正两类旧测试准备：读模型测试的虚构硬件ID不再算有效绑定；传感集成测试移除与现有读取合同不符的resource单位。保留初次失败JSON/XML，修正后原断言链复跑通过，没有放宽业务结果要求。测试发现缓存曾只包含新增类的6个用例，重新发现后确认8个均实际执行。

## 实测结果与原生证据

| 类 | 最终结果 | 证据 |
|---|---:|---|
| ProductionAlgorithmDriveEditModeTests | 8/8 | [XML](ProductionAlgorithmDriveEditModeTests.xml) / [JSON](ProductionAlgorithmDriveEditModeTests.json) |
| ProductionAlgorithmDrivePlayModeTests | 1/1 | [XML](ProductionAlgorithmDrivePlayModeTests.xml) / [JSON](ProductionAlgorithmDrivePlayModeTests.json) |
| AlgorithmExecutionEditModeTests | 8/8 | [XML](AlgorithmExecutionEditModeTests.xml) |
| AlgorithmInstanceEditModeTests | 13/13 | [XML](AlgorithmInstanceEditModeTests.xml) |
| AlgorithmReadModelEditModeTests | 32/32 | [XML](AlgorithmReadModelEditModeTests.xml) |
| AlgorithmEffectorNodeEditModeTests | 5/5 | [XML](AlgorithmEffectorNodeEditModeTests.xml) |
| AlgorithmCargoTaskQueryEditModeTests | 5/5 | [XML](AlgorithmCargoTaskQueryEditModeTests.xml) |
| MachineNavigationEditModeTests | 15/15 | [XML](MachineNavigationEditModeTests.xml) |
| MachineDeploymentRuntimeEditModeTests | 5/5 | [XML](MachineDeploymentRuntimeEditModeTests.xml) |
| AlgorithmServicesIntegrationTests | 1/1 | [XML](AlgorithmServicesIntegrationTests.xml) |
| MachineNavigationIntegrationTests | 1/1 | [XML](MachineNavigationIntegrationTests.xml) / [导航指标](navigation-regression/playmode-metrics.json) |
| **合计** | **94/94** | 原生XML与REST结果独立核对，未计重复复跑 |

普通Unity编译0错误；已有UiPanelTestPanel未使用局部变量、AlgorithmNodeItem未使用字段警告保留，均非本批修改。StableIdleWorldSteps_DoNotAllocate预热后1000步托管分配0；这是本机领域步的Editor测试，不是完整Player性能预算结论。导航回归写入的旧B13证据先备份，当前结果复制到本批navigation-regression后恢复历史文件。

`python tools/run_project_checks.py --port 8091`：5/5 PASS，引用0缺失脚本/0缺失引用，AppConfigs为12张表/1配置/3语言/6流程，AIData验证12成功/0失败/0警告，报告`validate-data-tables-json_20261008_122233.json`。脚本输出中的8092为其旧固定标签，实际命令和工程核验均为8091。

框架纯度(product profile)、项目边界、git diff --check及本change OpenSpec strict均通过。没有新增UI布局，B40不以截图代替业务结果；UI视觉与结构验收保留在B42/B50，运行分辨率仅1920×1080。

回滚边界经代码复核：撤掉区域新驱动和读模型委托可回到旧宿主接线；不改变磁盘格式或业务内容，不删除机器/草稿。新增实例身份仅属于算法运行数据；磁盘恢复仍由B47接入。当前变更未暂存/提交，未归档OpenSpec，未回写工作簿或申请完整阶段通过。
