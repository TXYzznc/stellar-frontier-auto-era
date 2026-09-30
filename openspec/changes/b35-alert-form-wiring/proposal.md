## Why

警报领域的三件套——账本（`AutoEraAlertService`，同源合并／恢复转历史／排序）、监控（`RegionAlertMonitor`，读真值跨越式报／销）与读模型（`AlertReadModels`，选中／标记已读／不可用三态）——都已在生产里存在并有测试，但 `AlertForm` 仍是「整页未就绪」的 NOT_WIRED 空壳，成为唯一的 UI 调用方缺口。

## What Changes

- `AlertForm` 从 NOT_WIRED 移出、转手写（HANDWRITTEN），接线 `AlertReadModels.Create(session)`。
- 列表按筛选（全部／仅活跃／仅历史）渲染；点行选中并展开详情（原因／影响／真实恢复条件）。
- 四个操作按钮接线：筛选（循环）、标记已读（只改阅读状态）、定位（区域选中来源对象并回现场）；「相关详情」跨界面跳转留后续批次，按钮禁用。
- 读模型补 `UiAlertRow.Source`（来源身份），定位据此选中区域对象；区域口径（多储能合计）为 Invalid 时说明无单一对象。
- 预制体补 4 个按钮的序列化字段绑定（历史坑：脚本与契约加了字段但预制体未回写）。

## Capabilities

### Modified Capabilities

- `alert-form`: 警报页从「未接入」转为真实列表／详情／标记已读／定位。

## Impact

- `Assets/Game/Scripts/AutoEra/UI/Integration/AlertReadModel.cs`（Source 身份）
- `Assets/Game/Scripts/AutoEra/UI/AlertForm.cs`（接线，转手写）
- `Assets/Game/Scripts/AutoEra/UI/AlertForm.Fields.cs`（生成，+4 按钮）
- `Docs/Development/UI-PrefabLayouts/AlertForm.contract.json`（+4 绑定）
- `Assets/Game/Prefabs/UI/Operations/AlertForm.prefab`（+4 字段绑定，经 UnitySkills）
- `tools/ui_contract_to_form_script.py`（AlertForm 移出 NOT_WIRED、加入 HANDWRITTEN）
- 对应 EditMode 测试。

## Non-Goals

- 「相关详情」跨界面跳转（进入能源／算法／任务界面）：属后续跨界面导航批次，按钮保持禁用。
- 等级／来源分列筛选：首版只做活跃／历史循环，等级与来源细分属后续。
- 警报种类的扩展（队列溢出、算法异常等判据尚未落定，凭猜测阈值报警会误导玩家）。
