# 任务

## 1. 规范口径统一（两份 spec delta）

- [x] 1.1 `project-development-baseline`：MODIFIED「框架与产品职责边界」——框架核心可修改、需留痕、不得依赖 `AutoEra` 业务类型
- [x] 1.2 `project-development-baseline`：MODIFIED「正式开发基线入口」——移除「`ScriptsBuiltin` 禁止承载产品业务」的表述
- [x] 1.3 `project-boundary-audit`：MODIFIED「独立项目边界审计」——目的表述改为约束依赖方向
- [x] 1.4 `project-boundary-audit`：MODIFIED「结构变更完整验证」——验收断言不再要求「未修改 `ScriptsBuiltin`」

## 2. 其余权威文档同步

- [x] 2.1 `AGENTS.md`：项目约束 + 框架纯度两处对齐新口径
- [x] 2.2 `.claude/CLAUDE.md`：Unity/GF_X 约束对齐
- [x] 2.3 `.claude/conventions.md`：目录边界对齐
- [x] 2.4 `README.md`：纯度与提交前检查对齐
- [x] 2.5 `Docs/Development/ProjectBaseline.md`：框架与产品代码边界对齐（红线解除说明已在 L73）

## 3. 验证

- [x] 3.1 全仓检索旧口径残留（"不得混入产品代码"、"禁止写入"、"不得回写框架基线"等）
- [x] 3.2 `python tools/audit_project_boundaries.py` 通过（确认规范改动未影响审计规则）
      （2026-09-20 复跑：`[OK] project boundary audit passed`，exit 0。）
- [x] 3.3 `python tools/audit_framework_purity.py --product-profile tools/audit_product_profile.json` 通过
      （2026-09-20 复跑：`[OK] framework purity audit passed`，exit 0。）
- [x] 3.4 Unity 普通编译无新增错误（本变更不含代码改动，作为回归确认）
      （2026-09-20 复跑 `tools/_unity_compile.py`：触发 `Assets/Refresh` 后
      「编译通过：程序集已更新且 console 无 CS 错误」——程序集确实比源码新，不是只看 console 的假绿。
      同日 `python tools/run_project_checks.py` 5/5，其中 [1/5] 编译检查 compile errors = 0、
      [5/5] 框架纯度检查同时覆盖 `audit_framework_purity` 与 `audit_project_boundaries`。）
