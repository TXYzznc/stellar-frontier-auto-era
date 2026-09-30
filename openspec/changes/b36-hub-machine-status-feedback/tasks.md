## 1. 审计

- [x] 1.1 审计 MachineReadModel：列表（UiMachineRow 含 Deployed 标志）／详情（含整备三栏）／选中／退订，均完整且有 12 个 EditMode 测试
- [x] 1.2 审计 BaseCommandHubForm：对象页（机器列表＋运行时详情）、统计页、能源页已接线；总览页聚合属 P6-009，不属本批
- [x] 1.3 审计 FieldHudForm：机器详情、区域页、定位（FocusSelection）、选中同步（SyncSelectedMachine）已接线
- [x] 1.4 审计 P2-012 程序部分：选中轮廓（RegionInputModule.SetHighlight）、轮组运动（MachineNavigationMotionAdapter）已交付；视觉 VFX 属美术批次

## 2. 修复

- [x] 2.1 `BaseCommandHubForm.RenderObjectsIndex` 过滤到「已部署」机器，行点击映射改到 `_deployedMachines` 缓冲

## 3. 验收

- [x] 3.1 编译 0 错误；`MachineReadModelEditModeTests` 12/12（含 Deployed 标志分页测试）；`openspec validate --strict`
- [x] 3.2 任务表 P2-010 → 已完成；P2-012 → 更新备注（程序部分完成，VFX 待美术）
