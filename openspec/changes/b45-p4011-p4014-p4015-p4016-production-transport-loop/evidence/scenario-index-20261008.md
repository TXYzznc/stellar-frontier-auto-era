# B45 场景与证据索引

基线：Unity2022.3.62f3c1／Windows10，主工程8091；ResourceTransferSettlement19、AlgorithmCargoTaskQuery5、AlgorithmInitialTemplates6共30/30原生EditMode。最终专项ProductionTransport13/13；其他最终回归见implementation文档。

| spec场景 | 验证入口 | 证据边界 |
|---|---|---|
| Repeat trigger | RepeatedStartDoesNotReplaceActiveResponsibility；正式循环结束后再次推进 | 专项替代接触判定；正式循环用真实工具与模板 |
| Arrive without work slot | ArrivalWithoutLegalDock_DoesNotReserveOrLoad、UnreachableActualContact_DoesNotReserveOrLoad | 同一库存权威，未到合法停靠点不产生预留或装载 |
| Path becomes blocked | DestinationDisappears_PreservesLoadedCargo、NavigationFailure_PreservesCargoAndShowsResponsibilityUntilDelivered；MachineNavigationEditModeTests.BlockedPath_WarnsAtTenAndFailsAtThirty | 目标失效、导航失败责任记录与真实无路径由各自测试覆盖，不冒充同一纵切 |
| Capacity recovers | FullLocalWarehouse_ResponsibilitySurvivesAndRecovers | 使用测试Seed身份占本地容量；正式木材/矿石按既有规则入全局余额，不被仓库本地容量限制 |
| Two resource loops | ProductionTransportLoopPlayModeTests.FormalHost_WoodAndOre_ProduceLoadNavigateDeliver_WithClosedUi | 正式宿主、实际锯盘/钻头/机械臂、真实导航、两种生产产出与结算；最终原生结果见XML |
| UI closed | 正式循环中关闭两个UIForm、再打开；WarehouseReadModel_DefersCaptureAndUnsubscribes、MachineDetails_ShowUnresolvedRouteAndUnsubscribeWhenClosed | 关闭退订和延期快照不会暂停领域 |
| Missing other production domains | implementation文档的G4依赖表 | 农业、水泵、完整b07/P4-020与正常新进度流程仍分别归属后续批次；不得把本批子集视为完整G4 |

补充安全单位用例：准备时间、速率、取消保留单位、跨多个批次、暂停贡献不补算、来源不足、机器容量、目标失效、公开观察时货舱和责任一致。

正式测试的机器装配、图参数与初始世界会话属于测试输入准备；木材和矿石必须由实际生产工具生成，不调用TryMint作为纵切产出。

失败检查点：checkpoint-arm-yaw-marker.xml记录批准base_yaw是VisualModel兄弟节点时的诚实就绪失败；修正前不降低关节祖先验证门槛。
