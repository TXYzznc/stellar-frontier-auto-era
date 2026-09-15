## Why

B10已交付基础运行与初始区域，ART-006已有通用视觉，但正式机器状态、硬件管理和业务UI尚未贯通。用户明确要求空闲客户端与2D继续后续工作，而不是重做既有成果。

## What Changes

- 按P0-011→P2-001～006→P2-009/010推进配置、机器实例/名称、组件槽位、状态、队列与算力，再接真实UI。
- 2D并行设计机器概况、硬件槽位、组件选择及安装/拆卸反馈完整流程；制作人检查后交用户视觉决定，通过后连续完成必要素材及Prefab。
- 复用已有ID、世界时间、GF生命周期、输入、UI样式、正式模型和Motion合同；不重复B05/B08/B09。
- 算法编辑器为之后的独立设计任务，不并入本次程序算法运行时实现。

## Capabilities

### New Capabilities

- `machine-runtime-management`: 第一版机器配置、实例、硬件、状态、队列与算力管理及可测试接口。
- `machine-hardware-ui-delivery`: 机器现场/中枢UI增量设计、视觉阶段门、资源装配及真实业务接入。

### Modified Capabilities

无；已有框架与动作合同沿用。

## Impact

产品代码/测试、受控GameData JSON与工具生成物、现有产品AppConfigs必要登记、机器UI稳定Prefab及ArtResource新UI设计包。新框架核心改动、Git及手工xlsx写入均不在授权内。静态美术B10独立推进，不阻塞纯业务服务和UI设计。
