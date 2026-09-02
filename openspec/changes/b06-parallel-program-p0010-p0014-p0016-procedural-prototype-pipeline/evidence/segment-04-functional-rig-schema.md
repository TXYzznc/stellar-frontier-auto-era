# 第 4 段：FunctionalRigContract Schema

日期：2026-09-02

## 产物

- `Assets/Game/Scripts/AutoEra/Motion/Contracts/FunctionalRigContract.cs` 定义无 GF_X／玩法依赖的合同数据模型、schema 版本常量、坐标约定、关节、锚点、净空／碰撞包络、视觉槽、绑定／安全姿态和兼容元数据。
- `Assets/Game/Config/FunctionalRigContracts/FunctionalRigContract.schema.json` 定义跨项目 JSON Schema Draft 2020-12。JSON 的外部字段采用 camelCase；读写与规范化映射由下一任务实现。

## 验证

- Node `JSON.parse` 验证 schema JSON 有效。
- `asset_refresh` 后 Unity 2022.3.62f3c1 普通编译稳定：`isCompiling=false`、`isUpdating=false`。
- Console 仅返回既有 FMOD 输出设备初始化错误（设备环境，非 C# 编译错误）；未观察到本段脚本错误。

## 边界

- 旧 `Assets/Game/Config/ContractSample/MachineJointContract.json` 仅用于命名与 JSON 形态研究，未作为新合同输入或权威。
- 本段未创建正式模型、Prefab、场景、玩法状态、xlsx 或 `ScriptsBuiltin` 改动。
