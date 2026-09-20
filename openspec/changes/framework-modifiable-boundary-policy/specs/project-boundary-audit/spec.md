## MODIFIED Requirements

### Requirement: 独立项目边界审计
项目 MUST 提供独立于 `audit_framework_purity.py` 的项目边界审计。该审计 MUST 验证产品目录与命名空间一致性、`ScriptsBuiltin` 不依赖 `AutoEra`、资源根不存在冗余 `AutoEra` 层以及项目基线入口完整性；框架纯度审计 MUST 保持领域无关。

本审计的约束对象是**依赖方向**，而不是框架核心的可修改性：允许为项目需要修改 `ScriptsBuiltin`，但框架核心 MUST NOT 依赖 `AutoEra` 业务类型。审计 MUST NOT 因框架核心被修改本身而产生 finding。

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
- **WHEN** `ScriptsBuiltin` 中的 C# 文件声明或引用 `AutoEra` 命名空间或 `AutoEra.*` 类型
- **THEN** 项目边界审计失败并报告框架核心越界

#### Scenario: 框架核心被修改但不依赖产品代码
- **WHEN** `ScriptsBuiltin` 中的文件为项目接入被修改（模板路径常量、创建菜单、目录常量），且未声明或引用任何 `AutoEra` 业务类型
- **THEN** 项目边界审计不产生 finding，该修改的合法性由 OpenSpec 或决策记录留痕保证

#### Scenario: 资源目录重复项目名称
- **WHEN** 受审计的产品资源类型根下出现 `AutoEra` 目录
- **THEN** 项目边界审计失败并报告冗余项目名称层

### Requirement: 结构变更完整验证
本变更 MUST 运行项目边界审计的自动化测试、框架纯度审计、项目边界审计和普通 Unity 编译检查。FSR MUST NOT 代替程序集和结构变更后的完整 Unity 编译。若 FSR 随附依赖导致普通编译控制台错误，项目 MUST 使用已确认且可追溯的 Editor-only 兼容依赖修复根因，并 MUST 在完整编译通过后单独验证 FSR Editor 功能。

#### Scenario: OpenSpec 实施验收
- **WHEN** 一个批次任务的实现任务全部完成
- **THEN** 两个审计均通过、项目边界测试通过、Unity 不处于编译状态且控制台无编译错误，框架核心未引入 `AutoEra` 业务类型依赖，本次实施未修改任务表，且对 `ScriptsBuiltin` 的任何修改均已在本批次 OpenSpec 或决策记录中留痕

#### Scenario: FSR Harmony 依赖与 Burst 兼容
- **WHEN** Unity 完成普通脚本编译并由 Burst 扫描 FSR 编辑器程序集
- **THEN** 控制台不出现 Harmony 元数据或 `Failed to find entry-points` 错误，FSR 仍能在 Editor Play Mode 执行受支持的方法体热重载，且兼容依赖不进入 Player
