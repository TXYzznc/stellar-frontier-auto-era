# 第 5 段：FunctionalRigContract 读写与校验

日期：2026-09-02

## 实现

- `FunctionalRigContractJson` 使用 camelCase JSON 映射，按对象字段及具有 `stableId` 的数组进行序列化规范化；内容指纹从排除 `contentFingerprint` 的规范化负载计算 SHA-256。
- `FunctionalRigContractValidator` 校验 schema 版本、稳定标识、坐标约定、包络尺寸、兼容范围、重复 ID、相对层级路径、父关节引用、局部轴、关节范围和体积尺寸。
- 合同读写与校验只用于编辑器／导出／测试路径，不进入每帧动作热路径。

## 验证

- Unity 普通刷新后：`isCompiling=false`、`isUpdating=false`，Console Error=0。
- QA 队列任务 `b06-functional-rig-contract-editmode`：job `8104008e`，`AutoEra.Tests.Editor.FunctionalRigContractEditModeTests` EditMode 共 3 项，3 通过、0 失败、0 跳过、0 不确定，耗时 6 秒。
- 覆盖规范化排序与稳定指纹、合同反序列化、缺字段、重复 ID、路径越界、非法范围和不兼容 schema。

## 排除项

- 未实现 MotionRig、MotionGraph、Builder、Prefab、场景、正式模型或任何玩法结算。
- 未修改 `Assets/Game/ScriptsBuiltin/`、任务表、任何 `.xlsx` 或 b05 UI 文件。
