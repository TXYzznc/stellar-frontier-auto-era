# 第 21 段：组合根与受控 Procedure 上下文槽

日期：2026-08-25

## 实施范围

- 新增 `AutoEraApplicationCompositionRoot`，仅负责构造 `AutoEraApplicationContext` 所需的 UTC Provider 和世界会话工厂；纯 C# 服务继续通过构造函数显式取得依赖。
- 新增 `AutoEraProcedureContextSlot`，以唯一固定的 FSM 数据键保存类型化 `AutoEraApplicationContext`；它只提供设置、读取、移交和清理，未提供任意类型查询或全局访问器。
- 上下文槽拒绝已释放上下文与重复写入；移交前取得上下文，随后移除 FSM 数据以让框架回收 `VarObject`，不释放应用上下文本身。

## 验证状态

- 已用专用 `asset_refresh` 完成结构变更后的普通刷新；此前由本段引入的 `CS0234`（`Application.dataPath` 命名空间遮蔽）和测试程序集 `CS0246`（缺少 `GameFramework` 引用）均已修复，重新诊断为非编译、`0` 个编译错误。
- 历史 job `ca65afdc` 发生在本次真实刷新和修复之前，不能作为本段回归证据；当前等待 `AutoEra.Tests.Editor.AutoEraProcedureContextSlotEditModeTests` 的新 job。
- 新回归将覆盖独立组合根装配、唯一槽位、上下文移交、释放上下文拒绝和幂等清理。
- Unity Skills 当前为 `auto`，`test_run_by_name` 被 `MODE_FORBIDDEN` 阻止；临时切换 Bypass 后由测试窗口运行。本段未创建或修改任何 `.xlsx`。
