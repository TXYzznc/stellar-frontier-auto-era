# 第50段：四轮机构具名动作预览

日期：2026-09-03

## 实现

- 为独立 `four_wheel_module` 增加 `wheel_module_steer_roll` MotionGraphAsset，声明转向、有限悬挂平移和持续轮转三项原型动作。
- 图固定筛选 `four_wheel_module_prototype`，因此选择单个轮组Rig时也能在统一动作预览窗口看到可播放动作，而不会错误复用整车合同。

## 验证

- QA job `c422ed75`：`FunctionalRigMotionGraphCatalogEditModeTests` 12/12 通过，覆盖 `wheel_module_steer_roll` 与 `four_wheel_module_prototype` 合同兼容性。
- QA job `fd2c9fdb`：`FunctionalRigMotionGraphCatalogEditModeTests` 13/13 通过，覆盖全 `FunctionalRigPrototypeCatalog.AssetFamilyIds`；每一合同家族至少存在一张兼容预览动作图。
- 结束状态：Unity 2022.3.62f3c1，Bypass，非 PlayMode、未编译、未更新、无域重载待处理，队列请求 0。
