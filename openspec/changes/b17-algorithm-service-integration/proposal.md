## Why

B15 已交付算法域完整服务层（`AlgorithmDocument` / `AlgorithmValidator` / `AlgorithmRuntime` / `AlgorithmInstanceService` / `AlgorithmTemplateLibrary` / `AlgorithmMachineAdapter`，26/26 测试通过），但这些服务只在测试里被 `new` 出来，生产运行路径从未创建它们；三个算法界面（`AlgorithmLibraryForm` / `AlgorithmEditorForm` / `AlgorithmBindingForm`）的视觉预制体已做好，但读模型固定返回「不可用」。本变更把算法服务接入世界/机器运行路径，让界面展示真实模板与实例，并录入五套开局系统模板，兑现 G3 算法系统的 UI 收尾（P3-010/011/012/014/015）。

## What Changes

- `AutoEraWorldSession` 创建并持有世界级 `AlgorithmTemplateLibrary`（模板列表/详情/实例化的数据源）。
- 机器级接入 `AlgorithmInstanceService`（草稿/应用/运行实例），关联 `MachineExecutionContext` 与既有导航/传感器适配。
- `AlgorithmReadModels.Create` 返回真实读模型（模板列表 + 详情 + 实例状态），替换 `UnavailableAlgorithmReadModel` 空实现。
- 录入五套系统模板：基础灌溉、农田基础作业、生长资源采集、储量资源开采、固定路线运输。
- 编辑/诊断/绑定三个界面从「整页不可用」变为可用，展示真实数据。

## Capabilities

### New Capabilities

- `algorithm-service-wiring`: 算法服务接入世界与机器运行路径，UI 读模型返回真实数据（模板列表/详情/实例）。
- `initial-algorithm-templates`: 五套开局系统算法模板的录入、列表与实例化。

### Modified Capabilities

（无。B15 的 `algorithm-graph-core` / `algorithm-service-execution` / `algorithm-instance-lifecycle` 尚未同步进 `openspec/specs/`，本变更不修改它们的服务层需求，只做接线。）

## Impact

- 修改 `AutoEraWorldSession`（加模板库属性）、机器级宿主（加实例服务）、`AlgorithmReadModel`（真实读模型）。
- 新增五套系统模板定义（纯数据，`AlgorithmDocument`）。
- 不改 B15 服务层本身（`AlgorithmDocument` / `AlgorithmRuntime` / `AlgorithmInstanceService` 等），不改界面预制体与 `.Fields.cs`，不改 `ScriptsBuiltin`/框架，不改 xlsx。
