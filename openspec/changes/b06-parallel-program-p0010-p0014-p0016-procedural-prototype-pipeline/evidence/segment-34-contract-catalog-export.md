# 第 5.5 段：首批功能合同导出

日期：2026-09-02

- `FunctionalRigContractCatalogExporter` 从 Catalog 合同权威生成六类 JSON 与 `functional-rig-contract-manifest.json`，输出路径为 `Assets/Game/Config/FunctionalRigContracts/Exports`。
- 清单覆盖 wheeled_carrier、four_wheel_module、multi_joint_arm、replaceable_effector、sliding_door 与 conveyor；全部合同版本为 1.0.0，均有 64 位内容指纹。
- QA job `888d24db`：`FunctionalRigContractCatalogExporterEditModeTests` / EditMode 1/1 通过，失败 0；结束时 Unity 非 PlayMode、未编译、无更新或域重载待处理。
