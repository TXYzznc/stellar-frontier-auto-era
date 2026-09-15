# B15 服务接口对齐（实际代码，非UI实施授权）

2026-09-11。对应2D `InteractionContract_20260911.md`。以下服务经过本批17/17回归（含真实Play联合），已实际点对点发送2D；不代表五区玩家画布/Prefab已接入。

| 责任 | 当前实际入口/字段 | 边界 |
|---|---|---|
| 图 | AlgorithmDocument.DocumentId/Revision/LanguageVersion/SchemaVersion；Nodes/Edges/Bindings | Node.Id永久ID；Edge以From/Output/To/Input定位，不假称已有EdgeId；LayoutX/Y仅布局 |
| 固定目录 | AlgorithmCatalog.Inputs/Outputs/Compatible/Cost；AlgorithmNodeKind/AlgorithmOperator | 首批固定子集，不按作业目标注册新语言。集合/通信/农林端点未实现时明确错误，不假造结构数据 |
| 验证 | AlgorithmValidator.TryCompile；AlgorithmIssue.Code/Severity/NodeId/PortId/CopyPath | Error阻应用；Warning要求明确确认；单位冲突不覆盖原草稿；执行计划内部深副本 |
| 草稿/保存 | AlgorithmInstanceService.ReadDraft/Edit/SaveDraft/SavedDraftRevision | expectedRevision门禁；SaveDraft是本服务内存保存，**不是磁盘保存成功**；上层必须准确标注 |
| 应用 | Apply/ReadRequest/ConfirmWarnings/CancelApply；RequestId/DraftRevision/ExpectedAppliedRevision/HardwareRevision/State/Reason | WaitingSafePoint及警告阶段可取消；Applying不可取消；关闭观察者不取消请求；拒绝保留旧计划 |
| 状态 | AlgorithmRuntime.Revision/Generation/Invalid/Paused/LastReason/WaitingCount；CopyState | 等待算力/输入暂不可读不是红色计算错误；结构发布期间同机其他实例暂停并保留状态，发布后恢复；参数更新不触发结构重启 |
| 历史 | History、AlgorithmRunRecord.RunId/Cost/Error/FailedNode/CopyPath/CopyTrigger | trigger含RootNodeId、NodeId、Revision、Generation、Sequence、Time、TaskId、SourceId、SourceTargetId、BindingGeneration、Inputs；值为当时快照，不冒充当前值；保留50条 |
| 模板 | AlgorithmTemplateLibrary.Save/Instantiate/CopyAsPlayer/Rename/Delete/List；Info.Id/Version/Name/IsSystem | 内存库；系统不可改删；实例化重分配永久节点ID、清实际绑定；无五生产模板/外部导入导出 |
| 恢复 | InstanceService.Capture/Restore；AlgorithmInstanceCheckpoint | 无在途物理任务时的内存边界；保存状态/草稿/待申请身份/剩余延迟；不冒充磁盘、离线或任务责任恢复 |

## 未冻结为玩家UI接口的部分

NameKey/本地化参数、完整节点分类呈现、历史中间值/旧图绘制、完整绑定候选提供者、画布手势、CanEdit/CanSave等聚合展示DTO及焦点句柄仍属S4适配。不得根据本文声称这些已有现成接口或生成生产界面。

## 真实端点

AlgorithmMachineAdapter以同一MachineExecutionContext/InitialRegion/MachineNavigation构造；BindResourceAmount须匹配已应用图的组件ID、目标ID和绑定Generation。ValidateBindings可供安全应用前权威复验，暂时超距不自动重绑。导航输出accepted/started/completed/failed/partial/targetInvalid/preempted/cancelled/rejected；结果在后续批次处理，独立根不自动合并任务。

UI只观察服务，不拥有Runtime.Dispose或请求生命周期；区域拥有者退出时按算法→适配器→导航/传感→Context的顺序释放。当前没有新增正式UI注册/配置或改变已验收面板。

暂停由AlgorithmPauseReason.User/Machine/Application独立持有，不允许应用完成覆盖用户或停机暂停。AppliedChanged仅表示明确Replace/Restore，适配器只对已注册并匹配新应用绑定的传感器重新订阅。MachineAdapter.Pump(worldMilliseconds,navigationSeconds)两种时钟明确分离，不从世界加速时钟推导导航实时超时。
