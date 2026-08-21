## ADDED Requirements

### Requirement: 唯一业务启动入口
为完成原始任务 `P0-003`，系统 MUST 在框架 `PreloadProcedure` 完成后，由 `FrameworkReadyProcedure` 选择且只选择一个实现 `IFrameworkStartupProcedure` 的自动纪元启动 Procedure。其他自动纪元业务 Procedure MUST 可以登记到同一 Procedure FSM，但 MUST NOT 同时成为框架启动入口。

#### Scenario: 从 Launch 进入自动纪元主菜单
- **WHEN** 开发者从 `Launch` 场景启动且框架数据预加载成功
- **THEN** 系统依次进入自动纪元启动 Procedure 和主菜单 Procedure，并完成主菜单场景加载

#### Scenario: 启动入口配置不唯一
- **WHEN** AppConfigs 中没有自动纪元启动 Procedure 或存在多个 `IFrameworkStartupProcedure`
- **THEN** 启动诊断明确报告配置错误，且系统不会猜测或随机选择业务入口

### Requirement: 应用上下文与世界会话隔离
系统 MUST 使用应用上下文承载跨业务场景的应用级依赖，并使用世界会话承载单个存档世界的永久 ID、对象注册表和世界时钟。主菜单状态 MUST NOT 保留活动世界会话；返回主菜单、切换世界、框架重启或应用关闭 MUST 对称释放对应会话和订阅。

#### Scenario: 进入并退出空世界
- **WHEN** 玩家从主菜单进入第一版空世界场景后返回主菜单
- **THEN** 世界 Procedure 创建的会话被释放，主菜单中不存在可访问的旧世界时钟、注册表或场景订阅

#### Scenario: 重复进入世界
- **WHEN** 同一运行期间连续两次执行“进入世界—返回主菜单”流程
- **THEN** 第二次获得全新的世界会话，且第一次的对象、ID 注册和异步回调不会影响第二次

### Requirement: 显式服务装配
系统 MUST 由轻量组合根创建应用级和世界级服务，普通纯 C# 服务 MUST 通过构造函数获得依赖。系统 MUST NOT 新增第三方 DI 容器，也 MUST NOT 提供可由任意业务代码调用的通用全局 Service Locator。

#### Scenario: 独立测试世界服务
- **WHEN** EditMode 测试使用测试 UTC Provider、对象注册表和世界时钟创建世界会话
- **THEN** 测试无需加载 Unity 场景、访问全局运行时状态或修改静态服务注册

### Requirement: 产品场景切换可取消且可恢复
自动纪元场景切换 MUST 通过产品层协调 GF.Scene 的加载、卸载、进度、失败和订阅释放，并 MUST 使用配置的相对场景名而不是硬编码完整资产路径。过期回调 MUST NOT 改变当前 Procedure 或新会话状态。

#### Scenario: 世界场景加载失败
- **WHEN** 第一版世界场景缺失或 GF.Scene 返回加载失败
- **THEN** 系统记录包含目标场景和失败原因的稳定诊断，释放半初始化世界会话，并保持或恢复到可操作的主菜单状态

#### Scenario: 切换期间离开拥有者
- **WHEN** 场景尚在加载时对应 Procedure 因重启或退出而离开
- **THEN** 协调器解除订阅或使该请求失效，随后到达的旧回调不会激活场景或世界会话
