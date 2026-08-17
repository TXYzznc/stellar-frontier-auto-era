## Context

本设计覆盖第一版开发任务表中的原始任务 `P0-001` 与 `P0-002`。项目由领域无关的 AI Friendly Frame 演进而来：框架核心位于 `Assets/Game/ScriptsBuiltin/`，通用扩展和当前热更新程序集入口位于 `Assets/Game/Scripts/`，正式产品设计位于 `Docs/GameDesign/`。当前 `Assets/Game/Scripts/Hotfix.asmdef` 将项目脚本编译为单一 `Hotfix` 程序集，并被 HybridCLR、Obfuz、编辑器工具和框架测试引用。

项目按单机独立游戏规模开发，当前不设计上线后的代码热更新颗粒度。业务代码、产品资源和开发流程需要与框架核心保持明确边界，但不应为了形式隔离而提前增加程序集、空目录或重复的项目名称层级。开发任务表由用户独占维护，AI 与自动化工具永久只读。

## Goals / Non-Goals

**Goals:**

- 为 `P0-001` 建立一个面向开发者和 AI 的项目基线入口，能够定位正式设计来源、冻结范围、排除项、任务表权限、OpenSpec 门禁和审计命令。
- 为 `P0-002` 建立 `Assets/Game/Scripts/AutoEra/` 产品代码边界和 `AutoEra.*` 命名空间约束。
- 采用按业务领域组织、按任务渐进创建的目录策略。
- 保持资源类型根目录直接承载本项目业务分类，不增加冗余 `AutoEra` 层。
- 通过独立项目审计验证稳定、可机械判断的边界，同时保持框架纯度审计领域无关。
- 使用普通 Unity 编译验证结构变更，并证明本次实施未写入用户维护的任务表。

**Non-Goals:**

- 不实现任何世界、机器、自动化、生产、UI、存档或输入业务能力。
- 不创建业务启动 Procedure、运行时服务、数据模型、公共接口或 DI 容器。
- 不新增、拆分或重命名 asmdef，不改变 `Hotfix` 程序集和 HybridCLR 加载链。
- 不调整 HybridCLR、Obfuz、资源构建或发布热更新策略；除经用户明确授权的 FSR/Harmony Burst 兼容修复外，不改变 FSR 功能、使用范围或热重载策略。
- 不修改 `Assets/Game/ScriptsBuiltin/`。
- 不创建尚未进入开发的业务领域空目录。
- 不创建或填写同批美术 OpenSpec；该变更必须单独讨论和确认。

## Decisions

### 1. 使用独立的项目基线索引

新增 `Docs/Development/ProjectBaseline.md`，由 `AGENTS.md`、`.claude/CLAUDE.md` 和 `README.md` 链接。该文件只汇总开发边界并链接 `Docs/GameDesign/` 中的权威内容，不复制完整产品规格。

**原因：** 单独索引同时适合普通开发者和不同 AI 入口，避免把项目治理内容全部堆入 `AGENTS.md`，也避免把 `开发执行约束.md` 扩展成通用工程入口。

**替代方案：**

- 只链接 `开发执行约束.md`：文件更少，但缺少冻结范围、框架边界和工具验收的统一入口。
- 全部写入 `AGENTS.md`：入口直接，但会混合机器指令与项目基线文档。

### 2. 保留单一 `Hotfix` 程序集

`Assets/Game/Scripts/AutoEra/` 继续由父级 `Hotfix.asmdef` 编译，不新增嵌套 asmdef。`Hotfix` 在本设计中只是现有编译与框架启动边界，不表示本期要实现发布后的热更新方案。

**原因：** 新增产品程序集会扩大到 HybridCLR 多程序集登记、加载顺序、裁剪、混淆和构建验证；重命名 `Hotfix` 还会触及 `ScriptsBuiltin`、编辑器工具和测试引用。二者均超出 `P0-001`/`P0-002`。

**替代方案：**

- 新增 `AutoEra.Runtime`：隔离更强，但需要重设构建和加载契约。
- 重命名 `Hotfix`：主要获得名称一致性，却产生广泛框架修改。

### 3. 代码使用独立产品根与命名空间

产品 C# 文件必须位于 `Assets/Game/Scripts/AutoEra/`，并声明 `AutoEra` 或 `AutoEra.*` 命名空间。产品根目录放置短小的 `README.md` 作为版本控制锚点和局部规则入口，不创建无功能的占位 C# 类型。

**原因：** `Assets/Game/Scripts/` 已包含框架通用扩展、UI/Entity 接入和 `HotfixEntry`；单独产品根能够在不拆程序集的情况下表达代码所有权。命名空间提供编译期类型归属，目录提供源码所有权，两者必须一致。

### 4. 按业务领域渐进组织

后续代码按业务领域直接生长为 `AutoEra.World`、`AutoEra.Machines`、`AutoEra.Automation` 等模块；只有对应任务进入实施时才创建目录。不得预建完整领域骨架，也不预设容易成为杂物区的 `AutoEra.Common`。

**原因：** 领域优先使同一功能的模型、服务和 Unity 适配保持邻近；按需创建可避免空目录污染和过早固化尚未设计的模块边界。

**替代方案：**

- 全局按 Models/Services/Controllers/Views 分层：同一领域会跨多个目录。
- 全部平铺：早期简单，但不能支撑任务表中的多系统规模。

### 5. 资源不重复增加项目名称层

模型、材质、贴图、Prefab、音频、动画、VFX 和 GameData 沿用现有类型根与框架接入规则，并在其下按业务类别组织。除代码根外，不为了标识项目归属机械创建名为 `AutoEra` 的资源目录。

**原因：** 当前仓库只承载一个游戏产品，资源类型根已经隐含项目归属；重复项目名称会增加路径长度但不增加隔离价值。独立美术项目的交付路径和命名细节由并行美术 OpenSpec 另行确认。

### 6. 框架审计与项目审计分离

保留 `tools/audit_framework_purity.py` 不含 `AutoEra` 专属规则；新增 `tools/audit_project_boundaries.py`，沿用可测试的 `audit(root: Path) -> list[Finding]` 和 `--root` 命令行形态。项目审计只验证可稳定机械判断的规则：

- `Scripts/AutoEra` 内的 C# 文件声明 `AutoEra` 或 `AutoEra.*` 命名空间；
- `Scripts/AutoEra` 外的 C# 文件不得声明 `AutoEra` 或 `AutoEra.*` 命名空间；
- `ScriptsBuiltin` 文本不得依赖或引用 `AutoEra` 命名空间；
- 已约定的资源类型根下不得出现冗余 `AutoEra` 目录；
- 项目基线文件及三个入口链接存在。

审计不根据类名、文件名或自然语言猜测“业务代码”，避免误报。`tools/tests/test_audit_project_boundaries.py` 使用临时目录覆盖正常路径、错误命名空间、`ScriptsBuiltin` 越界、冗余资源层和缺失入口。

**替代方案：**

- 扩展框架纯度审计：会把具体产品名写入领域无关工具。
- 只依靠人工评审：无法为 `P0-002` 提供可重复验收证据。

### 7. 任务表完整性与 Unity 验证

实施开始时记录 `第一版开发任务表.xlsx` 的文件哈希，结束前再次计算并要求一致；不得通过复制、另存或重算替换原文件。由于本变更包含目录、脚本和测试结构变更，最终必须退出 Play Mode 走普通 Unity 刷新/编译并检查控制台，不能使用 FSR 结果代替完整编译。

### 8. 使用官方 Thin Harmony 修复 FSR/Burst 兼容性

Unity 2022.3 当前使用 Burst 1.8.21。FSR 上游最新提交 `51140b71d9e5df1de231b33ec20ee089b18bebec` 随附的 Fat `0Harmony.dll` 虽声明 `0Harmony, Version=2.4.2.0`，但其 TypeReference 元数据包含越界 AssemblyReference；普通 Domain Reload 后 Burst 因而持续报告 `Failed to find entry-points`。当前 Fat DLL 的 SHA-256 为 `77E6901ECC606AEC66C2A972782A3779E4F50C037D2D165EB7ECECDD4D8F794D`。

经用户明确授权，改用 NuGet 官方 `Lib.Harmony.Thin 2.4.2` 的 net48 `0Harmony.dll`，并固定 Mono.Cecil 0.11.6、MonoMod.Backports 1.1.2、MonoMod.Core 1.3.3、MonoMod.ILHelpers 1.1.0、MonoMod.Utils 25.0.11 依赖组。Thin DLL 保持相同 Harmony 程序集名、版本与 API，候选 SHA-256 为 `657D779DD07781CC04D95EEFDFECC6B209AC2B9B21F66B7A6B395732CC28C129`，且已通过逐 TypeReference/AssemblyReference 元数据读取验证。Unity 2022.3 首次导入证明 .NET Framework 版 Backports 会与引擎自带 `ReadOnlySpan` 重复，因此 Backports 与 ILHelpers 采用同一 NuGet 版本和程序集标识的 netstandard2.1 变体，其余依赖使用 net48 解析结果。

传递依赖需要的 `System.ValueTuple, Version=4.0.3.0` 已由项目 Editor 插件提供，避免再导入同名程序集。所有新增依赖仅用于 `UNITY_EDITOR || LiveScriptReload_IncludeInBuild_Enabled`，而项目继续禁止启用 `LiveScriptReload_IncludeInBuild_Enabled`，因此本修复不进入 Player，也不构成发布热更新方案。FSR 源码、行为开关和产品代码均不改动。

Thin Harmony 通过 Burst 元数据扫描后，又暴露出 FSR 2021+ Roslyn 文件的文件名与程序集名不一致：`Microsoft.CodeAnalysis.FSR.dll` 内部程序集名仍为 `Microsoft.CodeAnalysis`，CSharp 文件同理。Burst 不支持该分发方式，因此保留原二进制、版本、GUID 和 Editor-only 导入设置，只把两个文件名及 FSR 内部 asmdef 引用恢复为程序集实际名称。该调整不更换 Roslyn 版本或代码。

**替代方案：**

- 全局关闭 Editor Burst：能隐藏错误，但降低其他程序集的真实开发环境覆盖，拒绝采用。
- 升降 Burst：Unity 官方将该问题归因于 Fat Harmony 无效元数据，Burst 1.8.x 不修复，不能解决根因。
- 保留控制台错误：违反本变更的普通 Unity 编译零错误门禁。

## Risks / Trade-offs

- **[单一 `Hotfix` 程序集仍包含通用扩展和产品代码]** → 先用目录与命名空间建立所有权；只有出现明确的独立构建、编译性能或发布需求时才另开 OpenSpec 讨论拆分。
- **[基于正则的命名空间检查可能漏掉复杂生成代码]** → 第一版只允许常规块级或文件级命名空间；生成代码在引入时单独定义审计例外，不预先放宽。
- **[禁止资源根下的 `AutoEra` 目录可能与第三方包同名]** → 审计仅覆盖已声明的 `Assets/Game` 产品资源根，不扫描 Packages 或第三方插件目录。
- **[入口文档与设计文档可能漂移]** → 项目基线只保存链接和不可变边界，具体规格继续以 `Docs/GameDesign/` 为准。
- **[任务表在实施前已由用户修改]** → 以实施开始时的哈希作为只读基线，只判断本次实施期间是否变化，不回滚用户已有修改。
- **[Thin Harmony 引入多个显式依赖]** → 只采用官方 NuGet net48 解析结果，记录版本、来源和哈希，并通过 Unity 普通编译及 FSR 冒烟验证依赖闭包。
- **[本地 FSR 子模块偏离上游 Fat 分发]** → 将兼容改动限制在依赖分发和导入元数据，不修改 FSR 源码；后续升级 FSR 时先复查上游是否已改用 Burst 兼容分发。

## Migration Plan

1. 记录任务表实施前哈希和当前 Git 差异，保留用户已有修改。
2. 创建项目基线索引，并补充三个开发入口链接。
3. 创建 `Scripts/AutoEra` 产品根和局部 README，不创建领域子目录。
4. 实现项目边界审计及其自动化测试。
5. 运行项目边界测试、框架纯度审计和项目边界审计。
6. 将 FSR 的 Fat Harmony 替换为固定来源的 Thin Harmony 及依赖，记录版本与哈希。
7. 通过 Unity 普通刷新/编译检查程序集和控制台，并执行 FSR Editor 冒烟验证。
8. 复核任务表哈希、`ScriptsBuiltin` 差异和 OpenSpec 校验结果。

回滚时删除本变更新增的项目基线、产品根 README、项目审计和测试，并移除三个入口链接；FSR 兼容修复通过恢复原 Fat `0Harmony.dll`、移除新增依赖与导入元数据回滚，不涉及运行时数据迁移。

## Open Questions

无。程序集、命名空间、目录、资源组织、审计职责和基线入口均已由用户确认；FSR/Harmony 兼容修复也已在发现 Burst 元数据错误后由用户单独明确授权。若实施中还需要改变这些契约，必须暂停并重新确认后更新本 OpenSpec。
