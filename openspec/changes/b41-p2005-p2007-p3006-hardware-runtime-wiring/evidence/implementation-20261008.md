# B41 实施与验证（2026-10-08）

用户已授权实施，主工程UnitySkills8091，Unity2022.3.62f3c1。B40为已完成前置。实际写范围及基线见[preflight](preflight-20261008.md)。本批没有改场景、Prefab、ScriptsBuiltin、asmdef、用户工作簿或Git索引。

## 已接通的生产路径

- 正式InitialRegionScene加载SensorCatalog并创建区域提供者目录。区域公开状态提供者按真实对象身份自动注册、删除与释放，支持生产域替换通用提供者，通用实现不合成土壤、作物或资源余额。
- RegionHardwareRuntime按已安装组件永久ID幂等持有MachineSensor和每效应器行为队列；复用MachineExecutionContext、MachineComputePool和MachineSensorSet。传感位置来自区域机器事实，不依赖表现对象是否存在。
- 正式区域世界步推进传感与行为；组件关闭、休眠、目标删除、合法拆卸和撤收走既有权限与安全点。提供者不支持的能力报告UnsupportedTarget，缺失数值字段以IsValid=false传入，未知字段不再回退到resource。
- 算法适配器验证真实传感/效应器身份、目标与代次；重绑定清除已排队旧源事件和旧源延迟，其他源不受影响。行为请求携带算法实例身份，目标类别来自真实区域对象；回传accepted/started与实际结果。同效应器串行、不同效应器并行。
- IRegionEffectorExecutor/IRegionEffectorOperation为B43/B44提供动作权威接入边界；它负责校验、提交、暂停时钟和同步安全取消。未注册动作在创建任务前明确拒绝。表现缺失有可读原因，不影响领域队列，也不能触发结算。已有导航绑定继续使用MotionWorkBridge单向投影，未把表现预览当作生产执行器。

## 场景到证据

| spec Scenario | 自动证据 | 结论 |
|---|---|---|
| Repeated reconciliation | RepeatedReconciliation_UsesOneInstalledEndpoint_AndLiveProviderDirectory | 50次重复对账仍为同一传感实例/行为队列，区域删除清理目录 |
| Hardware removed | SafeStopAndHardwareRemoval_DrainOnce_ReleaseLease_KeepPeerQueue | 真实MachineHardwareOperation在安全停机后拆卸，租约归还，另一队列保留，可继续接新请求 |
| Provider unavailable | ProductionSensorStep_ReadsRealPublicState_AndUnsupportedSoilIsUnavailable；MissingField_IsInvalidAndDoesNotExecuteDownstreamAction | 公开余额按真实状态更新；无土壤提供者不可读；缺余额不作为有效零值执行下游 |
| Old generation arrives | TargetRebind_InvalidatesOldAppliedGeneration_UntilDraftIsRebound；AlreadyQueuedOldSample_IsDiscardedWhenEndpointRebinds | 旧图/旧排队通知不驱动新绑定；重新应用后按新代次采样 |
| Two effectors receive work | TwoEffectors_ExecuteInParallel_SameEffectorWaits_OneCompletionPerRequest | 每队列独立，同队列第二请求等待；每请求仅一次开始、完成与释放 |
| Unknown action | UnknownAction_IsRejectedBeforeQueueOrTaskCreation | 缺执行器有原因，队列/任务均不创建，不回报完成 |
| Visual instance absent | TwoEffectors_ExecuteInParallel_SameEffectorWaits_OneCompletionPerRequest | 无视图有缺失说明，领域执行边界仍推进，公共资源余额未被表现或测试执行器改写 |
| 暂停及迟到结果补充 | PowerResume_DoesNotCatchUpPausedWorkTime；PowerPauseAndTargetRemoval_DoNotCommitLateOperation | 断电暂停不追补工作时间；目标删除释放操作和算力，恢复后无迟到提交 |
| 正式宿主 | HardwareRuntimeWiringPlayModeTests.FormalHost_CreatesAndDrivesInstalledHardware_WithoutFixtureServices | 加载真实Launch/InitialRegion及配置；生产创建传感/队列/提供者；只准备安装、公开输入和绑定命令，只调用entry.Advance；关闭HUD仍采样，目标删除及退出释放 |

并行、取消与暂停测试中的TestDomainExecutor只检验公开执行器边界和真实算力租约，不改变产品库存，不能作为B43/B44资源生产完成证据。旧MachineSensor/MotionWorkBridge集成测试仍为服务组合回归；生产接通证据由新正式宿主测试提供。

## 实际检查

14组唯一测试，**97/97通过**，每组最终JSON及Unity原生XML在本目录：

| 测试组 | 通过 |
|---|---:|
| HardwareRuntimeWiringEditModeTests | 10 |
| HardwareRuntimeWiringPlayModeTests | 1 |
| MachineSensorEditModeTests | 13 |
| SensorPublicDataEditModeTests | 3 |
| MachineExecutionContextEditModeTests | 3 |
| MotionWorkBridgeEditModeTests | 5 |
| ProductionAlgorithmDriveEditModeTests | 8 |
| MachineDeploymentRuntimeEditModeTests | 5 |
| AlgorithmInstanceEditModeTests | 13 |
| AlgorithmReadModelEditModeTests | 32 |
| AlgorithmServicesIntegrationTests | 1 |
| ProductionAlgorithmDrivePlayModeTests | 1 |
| MachineSensorIntegrationTests | 1 |
| MotionWorkBridgeIntegrationTests | 1 |

首次领域测试6/8，两个失败为断言早于逐事件处理/应用安全点完成，初始失败JSON/XML另存为initial-failures；修正步次后通过，再增加旧排队通知与暂停时钟用例为10/10。新结构使用停止Play Mode后的普通Unity编译，0错误；既有未使用字段告警未扩展本批范围。

- `python tools/run_project_checks.py --port 8091`：5/5通过，0编译错误，0缺脚本/缺引用；AppConfigs12表/1配置/3语言/6流程；AIData校验12成功、0失败、0警告，报告validate-data-tables-json_20261008_124334.json。脚本显示的8092是旧硬编码文案，实际命令和工程核验均为8091。
- 产品模式框架纯度、项目边界、`git diff --check`：通过。
- `openspec validate b41-p2005-p2007-p3006-hardware-runtime-wiring --strict`：通过。

## 边界与回退

本批完成硬件生命周期、正式传感接线及真实行为权威入口。资源转移、切割/钻探结算与专用提供者待B43/B44，不报告P4生产或整个G3/G4/G7/G8已通过。无UI资产改动，无截图视觉通过声明；B42运行验收仅1920×1080并另验结构适配。独立Player性能证据在后续性能批次处理。

回退先释放区域运行时，再撤销注册接入；不得直接删除运行中的上下文。停用表现投影不会撤销已提交领域余额。本批未归档change或同步主规格，未提交Git，未回写xlsx。
