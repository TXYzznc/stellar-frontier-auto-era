## Why

B11 机器名册和 B10 区域分别分配永久 ID，已有名册机器无法直接进入区域作业通道。用户已批准补齐同一身份的部署、查询与生命周期衔接，不扩展导航或 UI。

## What Changes

- 区域绑定已有 MachineInstance，保留名册为唯一永久身份所有者，不重新注册或分配 ID。
- 部署失败不留下标志或占地；重复绑定不移动对象；回收、区域退出和世界退出对称清理。
- 复用 RegionWorkQueue 优先级/FIFO，增加不修改队列的请求状态查询，区分预约与抵达。
- 添加同身份、竞争、失败及卸载回归，保留原有区域代理路径兼容性。

## Capabilities

### New Capabilities

- `machine-region-identity`: 名册机器的同 ID 区域表示、部署及释放生命周期。
- `machine-region-work-channel`: 同 ID 作业预约、查询、等待与释放。

### Modified Capabilities

无；不更改现有主规格的产品规则。

## Impact

仅 AutoEra/Machines、AutoEra/World/Region、既有 Editor Tests 和本 change。使用现有程序集和标准库；不改注册表核心、世界组合根、UI、Motion、模型、Prefab、场景、xlsx 或 Git。B11 视觉依赖仍独立挂起；本变更不代表 P2-008 或 G2 整体完成。
