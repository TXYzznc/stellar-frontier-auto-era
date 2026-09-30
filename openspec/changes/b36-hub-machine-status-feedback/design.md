## Context

批次 E 是「中枢机器状态与程序反馈收口」的审计批次。领域（MachineRoster）、读模型（MachineReadModel：列表／详情／整备三栏）与现场（FieldHudForm：选中同步／定位／现场页）都已交付并有测试。逐项审计后唯一真实缺口是中枢机器索引未过滤库中蓝图。

## Decisions

### 中枢按「已部署」过滤，不按严格 Connected

DoD 原文「只显示已连接机器」的意图是「中枢是现场机器的远程视图，不含库中蓝图」，而非「失电就从中枢消失」。若按 `Connected = Activated ∧ Powered ∧ SignalAvailable` 严格过滤，一台因缺电而停机的机器会从中枢列表消失，玩家反而看不到「这台机器出问题了」。故按 `Deployed` 过滤（库中蓝图移出），「已断电／无连接」等状态由行摘要（`AutoEraUiFormat.MachineSummary`）呈现，机器仍在列表里、状态可辨。

### 行点击映射改到过滤后列表

`RenderListRows` 的行序是过滤后列表的下标，`OnMachineRowClicked` 必须用同一份 `_deployedMachines` 缓冲反查稳定 Id，否则点击会选到「全量列表第 N 台」而不是「已部署列表第 N 台」。缓冲在 `RenderObjectsIndex` 重建，详情渲染不触碰它，无重入问题。

### 机器库分页不受影响

`MachineLibraryForm` 继续用读模型的 `Deployed` 标志分「库中／已部署」两页——同一份数据、两个不同切面，过滤留在各自页面而不是改读模型，保持读模型对多页面通用。

## Risks / Trade-offs

- 全部机器都是库中蓝图时，索引为空但领域态仍是 Ready（非 Empty）：正文写「还没有已部署到现场的机器。」而不点亮空态卡——空态卡是不透明浮层，用它反而盖住说明文字。
- 过滤逻辑只有十行且依赖的 `Deployed` 标志已被 `Rows_CarryDeploymentState_SoLibraryCanSplitPages` 钉住，不再加重复断言。
