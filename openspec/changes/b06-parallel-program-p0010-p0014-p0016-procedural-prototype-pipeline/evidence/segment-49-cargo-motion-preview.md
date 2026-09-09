# 第49段：货舱具名动作预览

日期：2026-09-03

## 实现

- 为被动货舱新增 `cargo_transfer_open` MotionGraphAsset，包含左右门叶的 `OpenClose` 声明式节点。
- 图的目标合同固定为 `cargo_bay_prototype`，因此统一“动作预览”窗口可在选择货舱Rig后列出、播放并加入临时组合。
- 此图只表现传输期门叶开合；容量、货物归属、传输仲裁和存档仍不属于Motion Core。

## 验证

- QA job `37681968`：`FunctionalRigMotionGraphCatalogEditModeTests` 11/11 通过，覆盖 `cargo_transfer_open` 与 `cargo_bay_prototype` 合同兼容性。
- 结束状态：Unity 2022.3.62f3c1，Bypass，非 PlayMode、未编译、未更新、无域重载待处理，队列请求 0。
