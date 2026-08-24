# 第 19 段：昼夜规则值对象

## 实施范围

- 增加 `WorldDayNightRules` 与 `WorldDayNightPhase`，以显式周期和有效日照时长计算世界时刻的有日照／无日照阶段。
- 构造时拒绝非法周期和日照时长，查询时拒绝负世界时刻。
- 本段使用已确认的一版周期（1,440,000 毫秒）和日照窗口（960,000 毫秒）验证边界；不自行确定新世界初始时刻，也不读取或写入项目配置。

## 验证

- Unity 8090 已核验为“星际拓荒：自动纪元”/ 2022.3.62f3c1，非 PlayMode、未编译。
- `WorldDayNightRulesEditModeTests.FirstVersionBoundaries_SwitchAtTheConfiguredSunlightEnd`：1/1 通过。
- `WorldDayNightRulesEditModeTests.ConstructionAndQueries_RejectInvalidDurationsAndTime`：1/1 通过。
- 本段未创建或修改任何 `.xlsx`。

## 后续边界

- 新世界初始时刻仍需由已校验项目配置提供；开发倍率边界和玩家 UI 不在本段实现。
