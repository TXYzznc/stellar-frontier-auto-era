# 编译与框架纯度检查入口

本文件是 P0-013 交付的「编译与框架纯度检查」可重复执行入口说明。它把项目层的五类
检查串成一条命令，作为每次提交前、派发批次收尾和 CI 前的统一健康门。

## 前置条件

- Unity Editor 已启动并监听 REST 端口 8092（`GET http://127.0.0.1:8092/health` 返回 200）。
- Python 3 已安装，且具备 `yaml`（PyYAML）依赖（框架纯度审计使用）。
- 工作目录为仓库根。

## 入口命令

```powershell
python tools/run_project_checks.py
```

可选 `--port <n>` 指定 Unity REST 端口（默认 8092）。退出码 0 表示五类检查全部通过，
非 0 表示至少一项未通过（详见下文「既有发现」）。

## 五类检查

| # | 检查 | 手段 | 通过判据 |
|---|------|------|----------|
| 1 | 编译检查 | Unity 8092 `debug_get_errors` | 编译错误数 = 0 |
| 2 | 引用检查 | Unity 8092 `cleaner_find_missing_references` | 缺失脚本/引用 = 0 |
| 3 | AppConfigs 检查 | 本地解析 `Assets/Game/ScriptableAssets/Core/AppConfigs.asset` | 全部数据表/配置/语言引用文件存在，流程非空 |
| 4 | 资源表检查 | Unity 菜单「Game Framework/GameTools/AI Data/Validate DataTables Json」+ 读取 `GameData/AIData/Reports/` 最新校验报告 | failureCount = 0 且 successCount > 0 |
| 5 | 框架纯度检查 | `tools/audit_framework_purity.py`（产品模式）+ `tools/audit_project_boundaries.py` | 两项审计均无发现 |

### 单项命令（手工执行）

```powershell
# 1 编译检查
python .agents/skills/unity-skills/scripts/unity_skills.py debug_get_errors --port=8092
python .agents/skills/unity-skills/scripts/unity_skills.py debug_check_compilation --port=8092

# 2 引用检查
python .agents/skills/unity-skills/scripts/unity_skills.py cleaner_find_missing_references --port=8092

# 3 AppConfigs 检查（并入入口脚本，无独立命令；由 run_project_checks.py 解析 .asset）

# 4 资源表检查
python .agents/skills/unity-skills/scripts/unity_skills.py editor_execute_menu "menuPath=Game Framework/GameTools/AI Data/Validate DataTables Json" --port=8092
# 之后读取 GameData/AIData/Reports/validate-data-tables-json_*.json 的 successCount/failureCount

# 5 框架纯度检查
python tools/audit_framework_purity.py --product-profile tools/audit_product_profile.json
python tools/audit_project_boundaries.py
```

## 检查语义与边界

- **编译检查**：以 Unity 当前编译诊断为准，0 error 才算通过。入口脚本会先等编译
  空闲（`debug_check_compilation` 的 `isCompiling=false`）再取错误数。
- **引用检查**：`cleaner_find_missing_references` 在项目范围内查找缺失脚本与缺失
  资产引用（含断链 GUID 表现），`issueCount=0` 才算通过。
- **AppConfigs 检查**：校验 `mDataTables`（映射 `Assets/Game/DataTable/<entry>.txt`）、
  `mConfigs`（映射 `Assets/Game/Config/<entry>.txt`）、`mLanguages`（映射
  `Assets/Game/Language/<entry>.json`）逐一存在，且 `mProcedures` 非空。流程类型
  是否可编译由第 1 项编译检查兜底。
- **资源表检查**：触发既有「校验 AI 数据表」菜单，复用 `ImportDataTablesFromAIJson`
  的只读校验（`syncExcel=false, writeGeneratedFiles=false`），并读取最新一份报告；
  该菜单会新增一份 `validate-data-tables-json_<时间戳>.json` 到 `GameData/AIData/Reports/`。
- **框架纯度检查**：`audit_framework_purity.py` 负责领域无关框架、Agent/SKILL、禁止
  内容、构建场景与生成物边界；`audit_project_boundaries.py` 负责 `AutoEra.*` 目录/
  命名空间、`ScriptsBuiltin` 反向依赖、冗余资源目录与基线入口。产品实例必须使用
  `--product-profile tools/audit_product_profile.json`，该配置只豁免派发依据列明的
  精确启动文件中的 MainMenu 标识与精确场景，不豁免 SampleScene、ScriptsBuiltin 或
  其他规则（见 [ProjectBaseline.md](./ProjectBaseline.md)）。

## 框架纯度历史债务（已于 2026-09-18 清零）

P0-013 交付时第 5 项框架纯度审计报告了 5 项既有发现（unity-skills 文档中的
`MainMenu` 示例、ScriptsBuiltin 迁移工具箱截图路径 `b10-mainmenu-current.png`、
`unity-sample` 示例技能目录）。用户于 2026-09-18 决策：框架部分内容此前已有备份，
本仓库按具体业务项目管理，授权清零这 5 项历史债务并解除原「框架层禁改」红线
（产品代码仍限 `Assets/Game/Scripts/AutoEra/` + `AutoEra.*` 命名空间，并读 `.claude/conventions.md`）。

处置结果：
- 3 处 `MainMenu` 示例名改为中性名（`YourScene.unity` / `HudCanvas`）。
- `ResourceExportSettings.asset` 截图路径改为 `export-current.png`（仅该单字段值）。
- `unity-sample` 示例技能目录（含索引引用）删除。

框架纯度审计（`audit_framework_purity.py`）与项目边界审计（`audit_project_boundaries.py`）
均作为自动回归围栏保留：此后若暴露新的发现路径，按 [ProjectBaseline.md](./ProjectBaseline.md)
的边界判断是否属于产品回归并另行处置，不自行扩权。

## 结果解读

- 第 1–5 项全绿 = 编译、引用、AppConfigs、资源表、框架纯度全部健康，退出码 0。
- 第 5 项为 FAIL 时，先核对是否为已知回归；出现新路径时按
  [ProjectBaseline.md](./ProjectBaseline.md) 的边界判断是否属于产品回归并另行处置。
