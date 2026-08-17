本清单覆盖第一版开发任务表中的原始任务 `P0-001` 与 `P0-002`。加入经用户授权的 FSR/Harmony 兼容修复后，重新估算总工时为 **10～14 小时**；任务表仍由用户维护，本估算不得自动回写工作簿。

## 1. 只读基线保护

- [x] 1.1 记录 `第一版开发任务表.xlsx` 的实施前 SHA-256 和当前 Git 状态，区分并保留用户已有修改（P0-001、P0-002）
- [x] 1.2 复核本 OpenSpec 的任务ID、前置依赖、方案确认记录和 `b01` 批次命名均与当前开发约束一致（P0-001）

## 2. 项目开发基线入口

- [x] 2.1 创建 `Docs/Development/ProjectBaseline.md`，链接正式设计来源、第一版冻结范围与排除项、任务表权限、框架边界、OpenSpec 门禁和两个审计命令（P0-001）
- [x] 2.2 在 `AGENTS.md`、`.claude/CLAUDE.md` 和 `README.md` 添加项目基线入口，并修正与 `Scripts/AutoEra` 产品代码边界冲突的旧入口表述（P0-001、P0-002）
- [x] 2.3 检查项目基线只承担导航和稳定边界声明，不复制或重定义 `Docs/GameDesign/` 的产品规格（P0-001）

## 3. 产品代码与资源边界

- [x] 3.1 建立 `Assets/Game/Scripts/AutoEra/` 产品代码根及短 README，声明 `AutoEra.*` 命名空间、领域渐进目录和禁止占位类型规则，不创建任何业务领域空目录（P0-002）
- [x] 3.2 保持现有 `Hotfix.asmdef`、HybridCLR、Obfuz、FSR 功能开关和发布热更新策略不变，并确认 `ScriptsBuiltin` 没有本变更造成的修改（P0-002）
- [x] 3.3 在项目基线和产品根 README 中记录资源沿类型根目录按业务分类、不得重复套 `AutoEra` 资源层的规则（P0-002）
- [x] 3.4 将 FSR 的 Fat Harmony 替换为官方 `Lib.Harmony.Thin 2.4.2` net48 及其固定版本依赖，修正 2021+ Roslyn 文件名与程序集名不一致的问题，保留许可证、来源、版本、GUID 和 SHA-256 记录，并确保依赖仅在 Editor 条件下可用（P0-002）

## 4. 项目边界审计

- [x] 4.1 实现 `tools/audit_project_boundaries.py`，提供 `audit(root)`、稳定 finding 输出和 `--root` 命令行入口（P0-002）
- [x] 4.2 审计产品目录内外的 `AutoEra.*` 命名空间声明、`ScriptsBuiltin` 对产品命名空间的引用、受控资源根中的冗余 `AutoEra` 目录以及三个项目基线入口（P0-001、P0-002）
- [x] 4.3 创建 `tools/tests/test_audit_project_boundaries.py`，使用临时项目覆盖成功、目录/命名空间错位、框架核心越界、冗余资源层和缺失入口场景（P0-001、P0-002）

## 5. 验证与交付

- [x] 5.1 运行项目边界审计测试，并确认全部测试通过（P0-002）
- [x] 5.2 运行 `python tools/audit_framework_purity.py` 与 `python tools/audit_project_boundaries.py`，确认两个审计均通过（P0-001、P0-002）
- [x] 5.3 让 Unity 通过普通刷新/编译流程验证结构变更，确认编译结束且控制台无编译错误，再单独执行 FSR Editor 冒烟验证；不得以 FSR 结果代替完整编译（P0-002）
- [x] 5.4 重新计算任务表 SHA-256 并与步骤 1.1 一致，检查 Git 差异中没有本次实施造成的任务表、`ScriptsBuiltin`、项目 asmdef、HybridCLR/Obfuz 或发布热更新策略修改；FSR 差异必须只包含已授权的兼容依赖修复（P0-001、P0-002）
- [x] 5.5 运行严格 OpenSpec 校验，汇总建议任务状态、实际工时和发现的问题供用户手动维护任务表（P0-001、P0-002）
