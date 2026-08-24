## Why

第一批次只建立了产品代码与框架边界，当前工程仍只能停留在通用 `FrameworkReadyProcedure`，没有可重复进入和退出的自动纪元运行时、业务场景或项目数据贯通。P0-003～P0-006 需要作为同一批运行时基础完成，以便后续 UI、输入、实体、事件追踪和存档系统建立在稳定且可测试的生命周期、标识、时间与配置契约之上。

## What Changes

- 覆盖开发任务表中的 P0-003、P0-004、P0-005、P0-006；任务表继续由用户独占修改，OpenSpec 不回写其状态或工时。
- 建立唯一业务启动 Procedure、主菜单 Procedure、世界 Procedure，以及不污染通用 `ChangeSceneProcedure` 的产品场景切换协调能力。
- 建立应用上下文与世界会话两级生命周期，并以轻量组合根和显式注入装配产品服务。
- 建立世界内统一、单调递增且永不复用的永久实例 ID 与对象注册表。
- 建立整数毫秒世界时钟、昼夜阶段、稳定同刻排序基础，以及可替换现实 UTC 来源和无状态 `TimeUtil`。
- 为现有 GameData 生成器增加领域无关的项目输出配置，使自动纪元生成代码进入产品目录和命名空间；建立覆盖 DataTable、Config、Language 的 AI JSON 中间层、安全反向生成与冲突保护，并贯通三类最小数据加载及错误诊断。
- 不在本变更中实现完整存档、离线事件结算、服务器时间同步、正式对象配置、UI、输入、实体或玩法系统。

## Capabilities

### New Capabilities

- `auto-era-runtime-lifecycle`: 自动纪元业务 Procedure、场景流转、应用上下文与世界会话的创建和释放契约。
- `persistent-instance-registry`: 世界内永久实例 ID 的分配、注册、注销、查询和失效引用语义。
- `deterministic-world-time`: 整数世界时间、昼夜阶段、现实 UTC 抽象、离线时长计算和稳定排序基础。
- `project-game-data-pipeline`: 项目 DataTable／Config／Language 的生成边界、加载登记和输入校验。

### Modified Capabilities

无。

## Impact

- 产品代码与测试：`Assets/Game/Scripts/AutoEra/` 下新增运行时、标识、时间、数据接入和 Editor 配置代码。
- 产品资源：新增主菜单与第一版空世界场景，以及按业务类别组织的项目 GameData JSON 中间层、工具生成的 xlsx 正式数据源与最小生成物；AI 不直接创建或修改 xlsx。
- 框架接入：更新 `AppConfigs` 的 Procedure、DataTable、Config、Language 登记。
- 通用工具：DataTable 生成器增加领域无关的可配置输出根和命名空间能力；AI GameData 工具增加三类数据适配、逻辑内容指纹、冲突拒绝、事务替换和回滚，不包含自动纪元业务判断；若触及 `ScriptsBuiltin`，必须通过框架纯度与回归测试。
- 依赖：不新增第三方运行时或 DI 容器；继续使用现有 GF_X、Unity 2022.3 和标准库能力。
