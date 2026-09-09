# 第47段：扩展基础几何验收场

日期：2026-09-03

## 实现

- `FunctionalRigAcceptanceDemo`新增水枪、旋转锯盘、旋转钻头、货舱和固定旋转载体，场景对象均使用中文名称。
- 验收场导演器驱动新增效应器机构、货舱双门和固定旋转Pivot；停止Play Mode时统一恢复绑定姿态。
- 实时截图：`Assets/Screenshots/FunctionalRigAcceptanceDemo-current.png`，用于本段可视核验，不进入玩法场景或权威数据。

## 验证

- Play Mode截图期间Console Error=0；结束后Unity非PlayMode、非编译。
- QA job `63cdd918`：`FunctionalRigAcceptanceDemoDirectorEditModeTests` 1/1通过。
- QA job `613cdd5b`：`EffectorWorkPresentationEditModeTests` 5/5通过。
- QA job `af22c54c`：`FunctionalRigPrototypeCatalogEditModeTests` 1/1通过。
- 合计7/7通过，失败0；Unity 2022.3.62f3c1为Bypass、未更新、无域重载待处理。
