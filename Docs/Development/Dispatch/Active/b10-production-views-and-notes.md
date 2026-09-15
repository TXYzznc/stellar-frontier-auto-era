# B10 已确认分支的生产图与说明续接

## 2026-09-11 用户最终裁决（覆盖下文历史等待与禁建模阶段说明）

用户明确“接受全部仅作辅助示意，以轴测和合同为准”。五项三视图保留原样，不派发返修，不宣称严格正交一致；已确认轴测负责造型，现行技术合同/配合补充/生产说明负责尺寸、结构、接口与安装。工坊混合投影、水泵平台边界歧义、传感器局部斜视不得按像素复制进模型；发射器固定宿主安装由已确认轴测辅图与MatingAddendum-v02 F承接。

依据原B10用户已授权的门禁后生产范围，主美可解除生产输入等待，按组件→建筑顺序续接3.5模型候选与结构自验；仅使用原派发授权的ArtResource源/Authoring目录。七份说明不重做，保留原图及审查证据；新资产尺寸按已确认的后续装配适配口径处理，地形适配仍排除。不得重开B08、改玩法、增加动作/LOD或覆盖正式项目已验收资源。模型最终仍交用户视觉验收，正式交付遵循后续授权。

生产输入门禁已解除，不等于模型完成或整批B10完成。主美同步索引/检查点并按既有OpenSpec流程登记3.4用户决定；3.5、3.6不得提前勾选。不自动Git或修改xlsx。

2026-09-10：用户已确认轴测、5+7分支，尺寸后续装配调整，地形适配不作为美术前置（DEC-196）。制作人已复核MinimalDocumentRevision-v02、合同与配合补充；现在续接B10 3.4，不再等待重复开工确认。不授权建模、Unity或Git。

## 3D原画：b10-five-production-threeviews

完整读取ArtResource `Docs/ArtPipeline/Preproduction/b10-interactive-world-and-static-art/` 的收口、MinimalDocumentRevision-v02、ART013/ART015、MatingAddendum及Manifest。
仅五项：SoilSensor v01、ExplorationScanner v02、CommunicationTransmitter v02、ManufacturingWorkshop v02、ShoreWaterPump v01；精确已确认轴测路径以Manifest的accepted_axonometric字段为准，不使用rejected或旧版本。
基于已确认轴测制作结构一致的正/侧/顶三视图，纯白背景、无文字/数字/尺寸/边框，保持原画细节精度。无新增动作、状态或功能；不为地形添加岸线剖面、水面、岸面或改水泵造型。接口尺寸以技术说明为准，不以生成图像素冻结工程参数。
写入ArtResource `ArtSource/Concept/b10-interactive-world-and-static-art/ThreeView/v01/` 与同目录索引/提示词/自检，逐文件保存后打开核对，整批实际回传制作人及主美。不得直接向用户求验收；制作人报告后用户决定。不动其余七项轴测、不覆盖原图、不操作DCC/Unity/AI3D或Git。

## 主美现有B10 Active续接

在现有Preproduction授权目录，完成其余七项的轴测加尺寸/安装结构说明：ObjectStateSensor、CommunicationReceiver、BasicComputeCore、BasicWarehouse、BiomassGenerator、SolarGenerator、BasicBattery。复用已确认图，不另画图，不建模。尺寸建议性质与后续装配可调整保留，实体接触、自身支撑、两宿主挂接、静态归属与完整包络说清即可，不新加精密加工/地形门槛。
并行整理五项三视图的技术对应关系；原画交付到位后检查结构与功能一致性，实际回传问题与证据，不自行推翻视觉认可、不启动建模。所有安全独立说明连续完成，不因水泵或原画尚在生成而整体等待。

## 完成与后续门禁

生产图/说明完成不等于用户视觉通过。制作人整批报告、用户决定之后才转入建模；本次不得重开B08。若真实工具失败，报告精确错误/检查点，继续其它安全工作；不得写“生成中”却结束回合且未交付文件。
