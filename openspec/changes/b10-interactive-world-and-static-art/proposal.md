## Why

用户已确认第一批动作模型与UI阶段完成，需要将既有成果用于可交互初始区域，同时推进尚缺静态资产。
任务表与队列残留旧状态，不能直接按未勾选条目重新开发。

## What Changes

- 核对并复用b02/b05/b06/b08/b09成果，只补工程基础缺项，再建立P1初始区域交互闭环。
- 并行制作ART-013静态组件、ART-015基础建筑的技术合同及后续原画和模型。
- UI复用已验收样式补齐实际缺项；QA和快速执行分别承担独立验证与固定扫描。
- 原画统一先交制作人检查结构/功能，再由用户依据报告决定通过或返工。
- 不在本批启动算法编辑器、完整物流、全场景环境生产或全面性能优化。

## Capabilities

### New Capabilities

- `initial-region-interaction`: 初始区域对象身份、选择、摘要、占地和作业申请。
- `static-art-production-gates`: 静态组件/建筑生产及原画制作人前审、用户最终裁决。

### Modified Capabilities

无；既有动作与UI公共合同继续复用，不重定义。

## Impact

主工程AutoEra产品代码、对应正式场景/Entity/UI资源、EditMode/PlayMode测试；ArtResource独立B10源与交付目录。
框架核心不在本批默认授权中。Git仅用户手动触发。任务表不在各角色可写范围。
