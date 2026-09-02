# 第 2.4 段：受限 Motion 图组合

日期：2026-09-02

- 图组合严格限于 Sequence、Parallel、Loop、ConditionalWait、Branch；其余节点属于明确的基础原语集合。
- `MotionGraphAsset` 静态校验拒绝未定义或不受支持的节点类型，不存在任意脚本节点、场景对象或玩法组件引用入口。
- 普通编译：`isCompiling=false`、`isUpdating=false`、Console Error=0；QA job `05bc5787`，`MotionGraphCompositionEditModeTests` 1/1 通过。
