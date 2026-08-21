## Context

当前框架已提供 `LaunchProcedure → PreloadProcedure → FrameworkReadyProcedure`、`IFrameworkStartupProcedure` 唯一入口选择、GF 场景和数据组件、AppConfigs 以及 Excel 生成工具。当前 `AppConfigs` 只登记通用 Procedure 和 Core 数据，工程只有 `Launch` 场景，`Assets/Game/Scripts/AutoEra/` 也尚无运行时代码。

本设计覆盖 P0-003～P0-006。后续 UI、输入、实体、存档与离线系统都会依赖本批建立的生命周期、对象引用、世界时间和项目数据契约，因此这些基础不能由场景查找、静态残留或临时全局单例拼接。产品代码继续编译进现有 Hotfix 程序集，不新增 asmdef 或第三方 DI 容器；任务表永久只读。

## Goals / Non-Goals

**Goals:**

- 建立从框架就绪态到主菜单、再到第一版空世界场景的可重复业务流程。
- 用应用上下文和世界会话隔离跨场景应用状态与单个存档世界状态。
- 提供可保存、可恢复、可稳定排序的永久实例 ID 和注册表契约。
- 提供在线与未来离线结算可共用的整数世界时间，并隔离现实 UTC 来源。
- 让项目表代码、配置和语言数据沿现有 GameData 工具链生成、加载和校验，同时遵守 `AutoEra.*` 边界。

**Non-Goals:**

- 不实现存档槽、序列化文件、离线事件队列、服务器登录或时间防作弊。
- 不实现正式主菜单 UI、HUD、输入、实体、对象内容、完整配置骨架或玩法逻辑。
- 不修改 HybridCLR、Obfuz、FSR、Hotfix asmdef 或发布热更新策略。
- 不把 ScriptableObject、AppConfigs 或静态工具当作运行时世界数据库。

## Decisions

### 1. 使用应用上下文与世界会话两级生命周期

`AutoEraApplicationContext` 在业务启动时创建，持有现实 UTC Provider、场景切换协调器和世界会话工厂；它不持有具体存档世界状态。玩家进入新世界或读取世界时创建一个 `AutoEraWorldSession`，后者独占永久 ID 分配器、对象注册表和世界时钟。返回主菜单、切换存档、重启框架或关闭应用时必须对称释放世界会话；应用退出时再释放应用上下文。

Procedure 由现有 `HotfixEntry` 以无参构造创建，因此组合根不依赖构造注入 Procedure。`AutoEraStartupProcedure` 将受控的 Procedure 上下文放入当前 Procedure FSM 的专用数据槽；后续产品 Procedure 只通过该显式流程上下文交接应用上下文，不提供通用 `Get<T>()` 或任意全局服务查询。纯 C# 服务仍使用构造函数注入，未来场景入口由世界 Procedure 显式初始化。

选择该方案而不是单一长生命周期，是为了防止三个存档槽之间串状态；不选择场景 MonoBehaviour 所有权，是为了让离线结算和 EditMode 测试不依赖场景。VContainer 当前收益不足以覆盖新增包和 Hotfix/FSR 适配成本。

### 2. 业务 Procedure 与产品场景切换分离

`AppConfigs.Procedures` 登记 `AutoEraStartupProcedure`、`AutoEraMainMenuProcedure` 和 `AutoEraWorldProcedure`，其中只有启动 Procedure 实现 `IFrameworkStartupProcedure`。启动 Procedure 建立应用上下文并转入主菜单；主菜单 Procedure 保证不存在活动世界会话；世界 Procedure 创建或接管一个世界会话，并在离开世界时释放它。

产品层场景切换协调器封装 GF.Scene 的订阅、加载、卸载、取消和失败收口。场景逻辑使用配置中的相对场景名，不硬编码完整资产路径。它不修改通用 `ChangeSceneProcedure`，也不让一个长期启动 Procedure 同时承担主菜单、世界、存档和加载职责。加载失败必须留下稳定诊断并回收半初始化会话，不能保留假成功状态。

### 3. 永久 ID 使用世界内统一递增的无符号 64 位值

`PersistentId` 是包裹 `ulong` 的不可变值类型；`0` 永远表示无效。一个世界会话只拥有一个分配器，机器、建筑、资源点、任务和行为共享同一序列。新 ID 单调递增，注销或对象消失后永不复用；恢复既有对象时允许登记保存的 ID，并将下一序号推进到所有已恢复 ID 之后。溢出必须明确失败，不允许回绕。

注册表以 `PersistentId` 为键，条目同时记录对象类别与受控引用。重复 ID、无效 ID、类别不匹配和重复注销必须产生明确结果或异常，不能静默覆盖。持久引用只保存 ID 与预期类别；目标缺失时保持未解析状态，不按名称、型号或新对象自动重绑。日志和跨世界诊断组合世界/槽位标识、对象类别与 ID，不要求 ID 自身跨世界唯一。

选择该方案而不是 Guid，是为了获得紧凑存储和天然稳定排序；不按类型分配独立序列，是为了保持统一责任链和注册表键空间。

### 4. 世界时间使用整数毫秒，现实 UTC 通过接口提供

世界时钟内部只保存非负、单调递增的 `long WorldMilliseconds`。在线驱动累计帧间真实时间并只提交整数毫秒，保留不足一毫秒的余数；未来离线系统直接把时钟跳到下一个有效事件时刻。视觉表现可以按帧插值，但玩法计时、昼夜、存档和排序只使用整数世界时间。

第一版完整昼夜为 1,440,000 世界毫秒，其中 960,000 毫秒有日照、480,000 毫秒无日照；新世界初始时刻和这些数值来自经过校验的项目配置。正式 UI 不暂停或减速世界；开发调试倍率只改变提交给时钟的推进量。相同时刻的排序键预留固定阶段、永久 ID 和单调事件序号，提供比较契约但本批不实现完整事件调度器。

`IUtcTimeProvider` 是应用级依赖，当前实现返回 `DateTimeOffset.UtcNow`；未来服务器实现可以在登录同步后用服务器 UTC 锚点加单调计时器计算当前可信 UTC。`TimeUtil` 只包含无状态换算和差值计算。离线时长等于当前可信 UTC 减去存档 UTC，结果为负时按零处理。世界时钟不读取系统时钟，也不知道时间来自本地还是服务器。

选择整数毫秒而不是 `double` 秒，是为了消除边界容差和长期浮点漂移；不采用逐固定 Tick 的离线重播，是为了给后续事件跳跃结算保留单一路径。

### 5. 项目数据沿现有工具链生成，但输出边界可配置

项目 GameData 使用 `AutoEra/` 业务子路径组织；P0-006 只创建验证 DataTable、Config、Language 贯通所需的最小真实配置，不提前建立 P0-011 的完整对象表。`AppConfigs` 登记运行时要加载的相对资源名，预加载失败沿现有诊断路径明确阻断启动。

现有生成器把代码固定写入 `Assets/Game/Scripts/DataTable/` 且生成全局类型，和产品边界冲突。生成工具增加可选、领域无关的项目生成 Profile：按源相对路径匹配规则指定代码输出根和 C# namespace；无匹配规则时保持 Core 表现不变。本项目规则把产品表输出到 `Assets/Game/Scripts/AutoEra/DataTable/` 并生成 `AutoEra.DataTable` 类型。Profile 属于 Editor 生成输入，不是运行时数据库，生成器实现中不得硬编码 `AutoEra`。

运行时 DataTable 类型解析先保持现有完整名/全局短名路径；若未找到，再按短类名搜索唯一的 `DataRowBase` 派生类型。零个或多个匹配都明确失败，避免命名空间支持导致类型误绑。字段格式、重复 ID、非法枚举/引用和生成 Profile 越界必须在生成、导入或启动验证阶段给出表名、行和字段上下文。

选择通用 Profile 而不是 AutoEra 专用后处理器，是为了保证自动刷新、手动生成和 CI 使用同一路径；不接受全局命名空间例外，因为它会破坏第一批次的产品边界。

## Risks / Trade-offs

- [Procedure 无参构造使上下文无法直接构造注入] → 只在受控 FSM 数据槽传递 Procedure 上下文，普通服务仍构造注入，并通过重复进入测试验证释放。
- [GF.Scene 事件回调可能在 Procedure 离开后到达] → 场景协调器为每次切换绑定拥有者令牌/代次，离开时取消订阅并忽略过期回调。
- [静态状态或关闭 Domain Reload 导致编辑器残留] → 产品运行时不以静态字段保存世界状态；测试覆盖连续创建、释放和再次创建会话。
- [64 位 ID 恢复时 next 值错误造成碰撞] → 恢复注册统一更新高水位并拒绝重复；保存契约保留下一序号，加载测试覆盖乱序恢复。
- [整数毫秒由浮点帧时间换算仍可能产生累计误差] → 使用双精度余数累积、只提交整数部分，并用分帧方式不同但总时长相同的测试比较结果。
- [生成器通用改动影响 Core 表] → Profile 缺失时必须保持原有输出；为 Core 与 namespaced 产品表分别建立生成和加载回归测试。
- [当前本地 UTC 可被修改] → 按已确认范围只处理时间倒退为零并保留 Provider 替换点，不在独立游戏阶段提前实现复杂防作弊。

## Migration Plan

1. 记录任务表实施前哈希和当前工作树，实施期间持续只读保护工作簿。
2. 先以测试锁定现有 Core 数据生成与加载行为，再加入可选生成 Profile 和命名空间类型解析。
3. 创建最小项目数据输入、生成物与 AppConfigs 登记，验证通用预加载链。
4. 实现永久 ID、注册表、世界时钟、UTC Provider、TimeUtil 和两级上下文的纯 C# 测试。
5. 实现产品 Procedure、场景协调器及主菜单/空世界场景，完成重复进出验证。
6. 通过普通 Unity 编译、EditMode/PlayMode 测试、数据诊断、项目边界与框架纯度审计；FSR 不替代结构编译。

回滚时删除产品新增场景、产品运行时代码和项目 GameData 登记，并撤回通用生成 Profile/类型解析改动；Core 数据生成与原有 `Launch → FrameworkReady` 链必须恢复到变更前行为。

## Open Questions

无。正式存档 DTO、服务器时间同步策略、完整同刻事件调度和 P0-011 对象表字段将在各自后续 OpenSpec 中确定。
