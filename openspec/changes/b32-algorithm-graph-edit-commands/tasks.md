## 1. Domain commands

- [x] 1.1 Add document-level create/connect/disconnect mutations and type-safe port checks
  - `AlgorithmDocument.CreateNode`（Id 查重+种类/ValueType 校验+Revision++）、`Connect`（端口存在+`AlgorithmCatalog.Compatible`+目标输入唯一）、`Disconnect`（完整边身份精确移除）
- [x] 1.2 Add service-level revision-protected create/connect/disconnect commands
  - `AlgorithmInstanceService.CreateNode/Connect/Disconnect`：草稿修订匹配、`Applying` 拒绝、复制-改-回写、`Changed` 发布；`CreateNode` 分配稳定 Id 并跳过草稿已占用值；`AlgorithmCatalog.DefaultNode` 提供最小默认节点（值节点带有限默认值，不隐藏自动补线）

## 2. Read-model path

- [x] 2.1 Extend `IAlgorithmReadModel` and machine/template/unavailable implementations
  - 接口新增 `CreateNode`/`Connect`/`Disconnect`；模板域与不可用域返回 0/false 且不改数据
- [x] 2.2 Forward selected-instance commands using current draft revision
  - 机器域实现读 `ReadDraft(instanceId).Revision` 后转发服务命令（与 `MoveNode` 同模式）

## 3. Tests and verification

- [x] 3.1 Add instance-service tests for success, stale revision, incompatible ports, occupied input and exact disconnect
  - `AlgorithmInstanceEditModeTests` 12/12：新增 `CreateNode_AllocatesStableIdAndBumpsRevision_StaleRevisionRejects`、`Connect_StrongTypedAndInputExclusive_DisconnectExact`
- [x] 3.2 Add read-model dispatch tests
  - `AlgorithmReadModelEditModeTests` 29/29：新增 `MachineDomain_CreateConnectDisconnect_DispatchToService`（含库页拒绝、快照读写一致）
- [x] 3.3 Run relevant EditMode tests, compile check, `openspec validate --strict`, and update task plan
  - 回归：`AlgorithmGraphEditModeTests` 4/4、`AlgorithmExecutionEditModeTests` 8/8；编译 0 错误；`openspec validate b32-algorithm-graph-edit-commands --strict` 通过
  - `AlgorithmServicesIntegrationTests` 第 93 行失败经 git stash 基线复核：**无批次 A 改动时同点同败**，属存量问题（编辑器长会话状态退化或场景侧），与本变更无关
  - 任务表 P3-010/P3-011 备注已更新（命令层交付，UI 交互待批次 B，状态保持进行中）
