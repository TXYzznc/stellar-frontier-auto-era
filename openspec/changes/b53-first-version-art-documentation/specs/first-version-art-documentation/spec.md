# First Version Art Documentation

## ADDED Requirements

### Requirement: Reference-grade production documents

文档 MUST 按“总规范 → 共用素材批次 → 页面族/世界资产族 → 组合效果图 → 验收”组织；每份制作文档 SHALL 自包含固定风格、硬约束、逐项提示词、文件名、引用、出图顺序和验收。

#### Scenario: Standalone handoff

- **WHEN** 美术人员只打开一个共用批次或页面族文档
- **THEN** 该文档仍能确定视觉语言、画布、尺寸、状态、提示词、交付文件名和验收，不需要回忆聊天上下文。

### Requirement: State-level assets

每个需要绘制的状态 MUST 有唯一 ID、文件名、完整提示词和验收；程序绘制项 MUST 标记为 runtime，不要求生图。

#### Scenario: Same-shape states

- **WHEN** 一个按钮有 normal、hover、pressed、disabled
- **THEN** 四个状态保持画布、外轮廓、Pivot 和文字槽一致，并分别登记状态级输出。

### Requirement: Current project traceability

页面族文档 MUST 回链当前 Form、Prefab、脚本和 `UI-PrefabLayouts/*.contract.json`；世界资产文档 MUST 回链第一版对象合同和 1m 尺度。

#### Scenario: Contract mismatch

- **WHEN** 效果图布局与当前 contract 不一致
- **THEN** 文档验收失败，不能把图片当作布局变更。

### Requirement: Full remake policy

旧图片、模型、贴图、材质、VFX、动画、音频、Prefab 截图和旧效果图 MUST NOT 作为本批生产内容或生图参考；参考游戏只允许高层分析。

#### Scenario: Old input rejection

- **WHEN** 交付中提交旧资产或旧截图变体
- **THEN** 标记为 REJECTED，要求从本批提示词重新生成并登记源文件指纹。
