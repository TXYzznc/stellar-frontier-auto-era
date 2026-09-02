# 第 2.2 段：MotionGraph 静态配置资产

日期：2026-09-02

## 交付

- 新增版本化 `MotionGraphAsset`，只保存 schema、图 ID／版本、强类型参数、稳定节点 ID 和显式连接表。
- 节点类型是受限枚举，覆盖动作原语及后续组合节点；资产不保存场景对象、玩法组件、委托或运行时执行状态。
- 静态校验拒绝不支持 schema、缺失图身份、重复参数／节点 ID、自环和引用不存在节点的连接。

## 验证

- 普通 Unity 刷新编译后：`isCompiling=false`、`isUpdating=false`，Console Error 为 0。
- QA 队列任务 `b06-motion-graph-editmode` 已完成；job `0cae6417` 运行 `AutoEra.Tests.Editor.MotionGraphAssetEditModeTests`（EditMode），总计 2，通过 2，失败 0，跳过 0，不确定 0，耗时 6 秒。
