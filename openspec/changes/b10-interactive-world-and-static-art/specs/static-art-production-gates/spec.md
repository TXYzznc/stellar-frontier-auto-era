## ADDED Requirements

### Requirement: Accepted material standard and rendering gate
十二项已验收模型 SHALL 保留Refinement_v02源与造型，沿用当前正式上一批Control合格材质标准，另存必要UV与真实PBR贴图候选。单资产单主材质、多功能通道、正确色彩空间及职责隔离 SHALL 自验，不新增LOD/减面或地形要求。

#### Scenario: Material candidate ready
- **WHEN** 单资产完成材质与纹理自验
- **THEN** 输出1920×1080完整轴测与近景并实际发送制作人报告，由用户决定视觉；其他安全制作继续，不将预览色或烟测结果认定为合格贴图

#### Scenario: Formal delivery after visual decision
- **WHEN** 用户明确通过材质渲染视觉
- **THEN** 先代表资产验证轴、尺寸与贴图再全量正式交付，冻结AssetOrganization对应目标并由客户端接入主工程；视觉通过前不替换正式Entity/Prefab，正式交付完成前3.6保持未勾选

### Requirement: Frozen static production inputs
静态组件与建筑 SHALL 先有可追溯对象清单、尺寸/接口与排除范围；不得更改已批准动作资产或新增机械行为。

#### Scenario: Requested animated detail
- **WHEN** 静态资产方案出现新的活动机构
- **THEN** 将其列为范围差异，不在静态生产中擅自实现

### Requirement: Producer review before user decision
原画 SHALL 将轴测、三视图和返修候选先交制作人并同步主美。制作人 SHALL 实际查看图片并提交结构/功能问题报告，用户保留通过与返工裁决权。

#### Scenario: Concept batch delivered
- **WHEN** 原画完成一个候选批次
- **THEN** 先发送路径/版本/偏差给制作人，不直接请求用户批准，也不自行进入下游建模

### Requirement: Approved references before modeling
建模 SHALL 依据用户批准轴测、相应三视图或简化说明及制作人核验的合同进行，不得先建模后补图。

#### Scenario: Unapproved production views
- **WHEN** 三视图已生成但用户尚未依据制作人报告决定
- **THEN** 保留候选，不启动对应模型生产，可继续无依赖准备工作

### Requirement: Reuse UI assets
UI补齐 SHALL 复用已验收样式，仅制作真实缺项；不重设计整套界面，不将多比例适配列为本批AI门禁。

#### Scenario: Missing object field
- **WHEN** P1对象摘要需要现有布局未提供的字段
- **THEN** 客户端与美术明确节点/绑定职责，在既有样式下最小补齐并验收新增部分
