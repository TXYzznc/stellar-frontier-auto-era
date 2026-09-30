# 设计

## 目标结构

`FieldHudForm` 退化为轻量协调入口，常驻摘要与现场内容分别由独立 UIForm 承载：

- `HudStatusForm`
- `HudTrackerForm`
- `HudAlertsForm`
- `HudNavigationForm`
- `HudSaveForm`
- `MachineOverviewForm`
- `MachineHardwareForm`
- `MachineAlgorithmForm`
- `MachineDiagnosticsForm`
- `ResourcePointDetailForm`
- `BuildingOverviewForm`

第一批实现可以保留现有读模型和渲染方法，通过适配层迁移字段；不得复制第二套领域数据源。

## 输入策略

- 常驻 HUD 和现场半屏页 `BlocksWorldInput = false`。
- 中枢、设置、商店和其它全屏管理页继续使用默认 `BlocksWorldInput = true`。
- `FieldHudForm` 不再把 `AutoEraUiRuntime.BlocksWorldInput` 回写为自身阻塞状态。
- 现场页关闭后由路由重新计算输入占用，不依赖旧状态缓存。

## 导航与会话

- 所有拆分 Form 通过 `AutoEraUiNavigator.Open(source, view, request)` 打开。
- `AutoEraUiSession` 只能从打开参数读取。
- 选中对象身份通过 `AutoEraUiPageRequest` 或会话区域选择状态传递。
- 返回由目标 Form 自己关闭，来源 Form 负责恢复焦点和选择状态。

## 兼容边界

- 先保证现有 FieldHudForm 的入口行为不变，再逐页迁移。
- 未迁移的现场页暂时继续由兼容壳承载，并明确记录迁移状态。
- 不修改任务表；实现状态写入本 OpenSpec、测试证据和派发记录。

