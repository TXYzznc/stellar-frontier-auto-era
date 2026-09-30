## 1. 读模型与契约

- [x] 1.1 `UiAlertRow` 补 `Source`（PersistentId）身份，供定位选中区域对象；区域口径保持 Invalid
- [x] 1.2 契约 JSON 补 4 个按钮绑定（`_alertsFilterButton`/`_alertsReadButton`/`_alertsLocateButton`/`_alertsDetailsButton`，均在 `Grp_AlertsActions` 下）
- [x] 1.3 生成器：AlertForm 移出 NOT_WIRED、加入 HANDWRITTEN；重跑生成器，仅保留 AlertForm.Fields.cs、还原其余 6 份 EOL 噪声

## 2. 预制体与表单

- [x] 2.1 UnitySkills 补 4 个按钮的序列化字段绑定并 prefab_apply 回写、文本核验
- [x] 2.2 手写 AlertForm.cs：接线 `AlertReadModels.Create(session)`；列表按筛选（全部／活跃／历史）渲染；点行选中展开详情；筛选循环；标记已读；定位（区域选中来源对象并回现场，区域口径/已移除时说明）；「相关详情」禁用；空／不可用状态

## 3. 测试与验收

- [x] 3.1 EditMode：`AlertFormBindingsEditModeTests` 1/1（10 个字段绑定非空）；`AlertReadModelEditModeTests` 10/10（新增 Source 身份测试）
- [x] 3.2 回归：`AutoEraNotWiredFormsEditModeTests` 12/12（AlertForm 移出且确认无未接入脚手架）；`AutoEraAlertServiceEditModeTests` 9/9；`RegionAlertMonitorEditModeTests` 9/9
- [x] 3.3 编译 0 错误；`openspec validate --strict`；任务表 P6-011 → 已完成、P6-012 → 更新备注（列表/详情/定位已接入，相关详情跳转待后续）
