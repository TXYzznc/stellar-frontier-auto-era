## ADDED Requirements

### Requirement: 正式 Entity SHALL 接管动作预览资产来源
动作预览、Catalog 构建器、固定演示场及其验证 SHALL 使用已验收的 `Assets/Game/Prefabs/Entity/` 与对应 B08 MotionContracts 作为视觉资产来源，不得继续依赖 `FunctionalPrototypes` 基础几何作为运行或验收视觉权威。

#### Scenario: 正式 Entity 运行预览
- **WHEN** 运行任一正式机器、效应器或建筑的兼容 MotionGraph 预览
- **THEN** 预览 SHALL 驱动该正式 Entity 的合同关节和视觉层，并通过既有动作回归

### Requirement: Operations 运行入口 SHALL 独立于候选链
`Assets/Game/Prefabs/UI/Operations/BaseCommandHubForm.prefab` 和 `FieldHudForm.prefab` SHALL 包含运行所需的完整视觉与逻辑绑定，不得依赖 `Assets/Game/Prefabs/UI/ART006_UI/` 的 V01–V04、StructurePrototype 或候选场景。

#### Scenario: 候选链移除前验证
- **WHEN** 两枚 Operations Prefab 完成独立化
- **THEN** Missing Reference 扫描、真实 GF UIForm 打开/关闭、输入焦点和状态回归 SHALL 通过

### Requirement: 过程资产 SHALL 在引用归零后原子退役
ContractSample、ART-006 候选链、FunctionalPrototypes、相应过程材质/动作图和旧验收场 SHALL 仅在 GUID 与文本引用扫描为零、替代正式路径已验证后，使用 AssetDatabase 成组删除。

#### Scenario: 仍有引用时阻止删除
- **WHEN** 任一候选退役组仍被 Prefab、场景、生成器、测试或正式配置引用
- **THEN** 系统 SHALL 停止该组删除并记录阻塞路径

### Requirement: 运行证据 SHALL 脱离正式资源根
过程截图 SHALL 在删除 Assets 内副本前归档至对应 OpenSpec evidence，并记录原路径、新路径和内容哈希；正式 Assets 根不得保留仅用于验收的截图。

#### Scenario: 归档截图后退役
- **WHEN** 截图已复制至 OpenSpec evidence 且哈希核对一致
- **THEN** Assets 内过程截图及空目录 SHALL 可随其所属过程组通过 AssetDatabase 删除

### Requirement: 每个退役批次 SHALL 通过回归门禁
每个删除批次 SHALL 在删除前后完成对应的普通编译、Missing Reference 扫描、目标 EditMode/PlayMode 回归和 Console Error=0 检查。

#### Scenario: 删除后的验证失败
- **WHEN** 任一删除后的回归、引用扫描或 Console 检查失败
- **THEN** 后续批次 SHALL 停止，且本批次 SHALL 按已记录路径恢复或保留待修复现场
