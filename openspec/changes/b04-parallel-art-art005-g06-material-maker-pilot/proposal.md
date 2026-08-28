## Why

G06旧V19单主材质候选暴露了UV语义重叠、发光泄漏和Unity通道映射错误，继续依赖人工节点搭建与重复导出既慢且难以稳定复现。现在需要以Material Maker建立一条由AI完成至少90%工作的高效率、高质量程序化材质试点，并用G06验证它是否能够成为后续机器资产的量产基线。

## What Changes

- 为Material Maker建立受约束、可版本化、可批处理的材质配方层，使AI通过配方JSON生成或更新`.ptex`并批量导出PNG，而不是依赖逐次GUI操作。
- 为G06建立Blender侧唯一UV0、语义／功能遮罩和单主材质制作合同；不修改已通过白模几何、Pivot、关节、Socket或锚点。
- 建立Unity侧确定性导入与材质装配门禁，正确处理BaseColor、Normal、Metallic(R)／Smoothness(A)、AO和受限Emission Mask。
- 建立AI自动诊断、主美自验和连续迭代闭环；普通失败不得暂停等待制作人或用户。
- 在冻结的中性LookDev条件下输出一张完整模型轴测全景和一张关键材质特写，必须取得用户最终视觉验收后才允许将方案推广到其它资产。
- 保留旧G06 V19结果为失败证据，不覆盖V17／V18结构、既有RuntimePreview或用户维护的任务表xlsx。

## Capabilities

### New Capabilities

- `ai-procedural-material-pipeline`: 约束AI驱动的Material Maker配方、Blender遮罩输入、Unity材质装配、自动诊断、视觉迭代及G06试点验收。

### Modified Capabilities

无。试点通过前不修改全项目美术交付合同；推广规则将在用户通过G06最终候选后另行决定。

## Impact

- 外部工具：`D:\APP\material-maker-master`，可能增加最小批处理适配器、固定版本运行入口和试点模板，但不把第三方源码复制进Unity项目。
- ArtResource：G06的Blender UV／遮罩源、Material Maker配方与输出、Unity主材质、导入配置、LookDev证据和迭代记录。
- 主项目：仅保存OpenSpec、流程合同和验收证据；不接收未通过的材质候选，不修改任务表xlsx。
- 协作：工具工程负责批处理能力，主美负责G06资产输入、材质调优与自验，制作人负责范围与技术门禁，用户负责最终两张渲染图的视觉验收。
