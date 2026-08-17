## ADDED Requirements

### Requirement: 产品代码根与命名空间
为完成原始任务 `P0-002`，自动纪元产品 C# 类型 MUST 位于 `Assets/Game/Scripts/AutoEra/`，并 MUST 声明 `AutoEra` 或 `AutoEra.*` 命名空间。该目录 MUST 继续由现有 `Hotfix` 程序集编译，不得为本变更新增、拆分或重命名 asmdef。

#### Scenario: 新增领域类型
- **WHEN** 后续任务新增世界领域类型 `WorldClock`
- **THEN** 源文件位于 `Assets/Game/Scripts/AutoEra/World/` 并声明 `AutoEra.World` 命名空间

#### Scenario: 产品根在版本控制中稳定存在
- **WHEN** `P0-002` 实施完成但尚未开始首个业务领域任务
- **THEN** `Scripts/AutoEra` 通过短小的局部 README 保持可追踪，且不存在无功能的占位 C# 类型或预建领域空目录

### Requirement: 业务领域渐进组织
产品代码 MUST 按业务领域组织，并 MUST 只在对应任务进入实施时创建领域目录。项目 MUST NOT 预建完整业务目录骨架，也 MUST NOT 在没有明确跨领域契约时建立通用杂物目录。

#### Scenario: 尚未开发机器系统
- **WHEN** 机器系统对应的任务尚未进入实施
- **THEN** 本变更不创建 `AutoEra/Machines` 空目录或机器占位类型

### Requirement: 产品资源沿用类型根目录
产品资源和 GameData MUST 沿用项目现有的资源类型根目录及框架数据接入规则，并在其下按业务类别组织。除产品代码根外，项目 MUST NOT 为单一产品归属机械增加名为 `AutoEra` 的资源目录层。

#### Scenario: 后续添加机器模型
- **WHEN** 美术集成任务向主工程添加机器模型
- **THEN** 模型按交付合同进入 `Models` 下的机器业务分类，而不是进入 `Models/AutoEra` 冗余层

#### Scenario: 后续添加产品数据表
- **WHEN** 配置任务新增自动纪元领域数据表
- **THEN** 数据表按 GameData 的框架接入规则和业务类别组织，而不是为了项目名称重复增加无语义目录层

### Requirement: 独立项目边界审计
项目 MUST 提供独立于 `audit_framework_purity.py` 的项目边界审计。该审计 MUST 验证产品目录与命名空间一致性、`ScriptsBuiltin` 不依赖 `AutoEra`、资源根不存在冗余 `AutoEra` 层以及项目基线入口完整性；框架纯度审计 MUST 保持领域无关。

#### Scenario: 正确的产品代码边界
- **WHEN** `Scripts/AutoEra/World/WorldClock.cs` 声明 `AutoEra.World`，三个入口均链接项目基线，且资源目录无冗余项目名称层
- **THEN** 项目边界审计不产生 finding

#### Scenario: 产品类型放在错误目录
- **WHEN** `Scripts/World/WorldClock.cs` 声明 `AutoEra.World`
- **THEN** 项目边界审计失败并报告命名空间声明位于产品代码根之外

#### Scenario: 产品根中的类型缺少产品命名空间
- **WHEN** `Scripts/AutoEra/World/WorldClock.cs` 使用全局命名空间或非 `AutoEra.*` 命名空间
- **THEN** 项目边界审计失败并报告产品目录与命名空间不一致

#### Scenario: 框架核心依赖产品代码
- **WHEN** `ScriptsBuiltin` 中的 C# 文件引用或声明 `AutoEra` 命名空间
- **THEN** 项目边界审计失败并报告框架核心越界

#### Scenario: 资源目录重复项目名称
- **WHEN** 受审计的产品资源类型根下出现 `AutoEra` 目录
- **THEN** 项目边界审计失败并报告冗余项目名称层

### Requirement: 结构变更完整验证
本变更 MUST 运行项目边界审计的自动化测试、框架纯度审计、项目边界审计和普通 Unity 编译检查。FSR MUST NOT 代替程序集和结构变更后的完整 Unity 编译。若 FSR 随附依赖导致普通编译控制台错误，项目 MUST 使用已确认且可追溯的 Editor-only 兼容依赖修复根因，并 MUST 在完整编译通过后单独验证 FSR Editor 功能。

#### Scenario: OpenSpec 实施验收
- **WHEN** `P0-001` 与 `P0-002` 的实现任务全部完成
- **THEN** 两个审计均通过、项目边界测试通过、Unity 不处于编译状态且控制台无编译错误，并确认本次实施没有修改 `ScriptsBuiltin` 或任务表

#### Scenario: FSR Harmony 依赖与 Burst 兼容
- **WHEN** Unity 完成普通脚本编译并由 Burst 扫描 FSR 编辑器程序集
- **THEN** 控制台不出现 Harmony 元数据或 `Failed to find entry-points` 错误，FSR 仍能在 Editor Play Mode 执行受支持的方法体热重载，且兼容依赖不进入 Player
