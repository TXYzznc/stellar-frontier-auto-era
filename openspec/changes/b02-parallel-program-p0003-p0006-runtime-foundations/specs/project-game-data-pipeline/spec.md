## ADDED Requirements

### Requirement: 项目生成代码遵守产品边界
为完成原始任务 `P0-006`，GameData 生成工具 MUST 支持可选、领域无关的项目生成 Profile，根据源相对路径配置代码输出根与 C# namespace。本项目的产品表生成代码 MUST 位于 `Assets/Game/Scripts/AutoEra/DataTable/` 并使用 `AutoEra.DataTable` namespace；生成器实现 MUST NOT 硬编码产品名称或固定项目绝对路径。

#### Scenario: 生成自动纪元产品表
- **WHEN** 生成器处理匹配自动纪元项目规则的 DataTable 源文件
- **THEN** 生成文件进入产品代码根、声明 `AutoEra.DataTable` namespace，并能在现有 Hotfix 程序集中编译

#### Scenario: 生成现有 Core 表
- **WHEN** Profile 缺失或源表不匹配任何项目规则
- **THEN** Core 表继续使用变更前的输出位置、类型名和运行时加载行为

### Requirement: 命名空间数据行类型解析唯一
运行时 DataTable 加载 MUST 支持命名空间中的 `DataRowBase` 类型，同时保持现有全局类型兼容。短类名回退搜索 MUST 只接受唯一匹配；没有匹配或存在多个匹配时 MUST 明确失败，不能选择第一个类型。

#### Scenario: 加载唯一 namespaced 表类型
- **WHEN** AppConfigs 登记的产品表对应唯一的 `AutoEra.DataTable` 数据行类型
- **THEN** GF.DataTable 创建正确类型的数据表并读取生成数据

#### Scenario: 存在重名数据行类型
- **WHEN** 多个 `DataRowBase` 派生类型具有相同短类名且加载配置未能唯一定位
- **THEN** 预加载失败并报告歧义类型，不随机绑定其中一个

### Requirement: 项目 DataTable Config Language 统一预加载
自动纪元最小 DataTable、Config 和 Language 输入 MUST 沿现有 GameData 工具链生成，并 MUST 在 AppConfigs 中使用相对资源名登记。预加载完成后，GF.DataTable、GF.Config 和 GF.Localization MUST 能读取对应项目数据；任一必需输入失败 MUST 阻断业务启动并显示明确诊断。

#### Scenario: 项目数据全部有效
- **WHEN** 从 Launch 启动且自动纪元 DataTable、Config、Language 生成物均存在并有效
- **THEN** PreloadProcedure 成功读取三类项目数据后才进入自动纪元启动 Procedure

#### Scenario: 项目语言资源缺失
- **WHEN** AppConfigs 登记的自动纪元语言资源不存在
- **THEN** 预加载报告资源名和加载阶段，且不会进入半初始化的业务主菜单

### Requirement: 项目数据错误可定位
生成、导入和启动验证 MUST 对缺失必填字段、重复 ID、非法字段值、非法引用以及生成 Profile 越界提供明确失败。诊断 MUST 至少包含数据种类、资源或表名以及可获得的行/字段上下文。

#### Scenario: DataTable 包含重复 ID
- **WHEN** 产品 DataTable 源文件包含两个相同 ID
- **THEN** 生成或验证失败并定位重复 ID，不能产生被运行时静默覆盖的有效结果

#### Scenario: 配置引用不存在的资源
- **WHEN** 最小项目配置包含无法解析的必需资源引用
- **THEN** 启动验证明确报告非法引用，业务 Procedure 不会以默认空值继续运行

### Requirement: P0-006 不提前承担完整内容配置
本变更 MUST 只建立验证运行时基础和三类数据接入所需的最小真实项目数据，MUST NOT 提前创建 P0-011 的完整对象 ID、分类、等级、能力、UI、实体和声音引用骨架。

#### Scenario: 复核本批 GameData 范围
- **WHEN** 开发者检查本变更新增的 GameData 输入
- **THEN** 每个字段都服务于启动、场景、世界时间或接入验证，不包含尚未进入实施的完整第一版对象目录
