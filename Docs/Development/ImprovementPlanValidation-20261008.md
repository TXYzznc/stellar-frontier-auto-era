# 项目改进规划校验记录（2026-10-08）

关联：[总计划](ImprovementPlan-20261008.md)、[用户已确认的决策摘要](ImprovementPlanDecisionSummary-20261008.md)。

## 验证范围

本记录仅验证B40～B50规划产物及仓库文档边界。没有修改或运行Unity产品代码、Prefab和场景；没有改写开发任务表，没有执行Git暂存或提交。OpenSpec的artifactsComplete表示规划文档齐备，不代表实现完成。

## OpenSpec校验

对下列每个change执行 `openspec validate <change-id> --strict`，并用 `openspec status --change <change-id> --json`检查产物状态；所有命令退出码为0。

| 批次 | change ID | strict | 规划产物 | 未实施任务 |
|---|---|---|---|---:|
| B40 | `b40-p3003-p3005-p3007-p3008-algorithm-runtime-drive` | PASS | 完整 | 9 |
| B41 | `b41-p2005-p2007-p3006-hardware-runtime-wiring` | PASS | 完整 | 9 |
| B42 | `b42-p3010-p3011-p3013-p3014-algorithm-workbench-closure` | PASS | 完整 | 9 |
| B43 | `b43-p4001-p4010-p4012-p4013-resource-settlement` | PASS | 完整 | 10 |
| B44 | `b44-p4002-p4003-p4009-p4017-forest-mineral-production` | PASS | 完整 | 10 |
| B45 | `b45-p4011-p4014-p4015-p4016-production-transport-loop` | PASS | 完整 | 9 |
| B46 | `b46-p5006-p5009-p5010-energy-hotpath` | PASS | 完整 | 9 |
| B47 | `b47-p7002-p7003-p7004-p7011-world-save-flow` | PASS | 完整 | 10 |
| B48 | `b48-p7005-p7006-p7007-offline-event-foundation` | PASS | 完整 | 9 |
| B49 | `b49-p7008-p7009-p7010-p7012-offline-domain-reports` | PASS | 完整 | 10 |
| B50 | `b50-p8007-p8011-p8012-ui-performance-closure` | PASS | 完整 | 10 |

合计11份proposal、11份design、11份增量spec、11份tasks；46条Requirement、79个Scenario、104项未勾选任务、0项已勾选任务。

## 内容与边界检查

- 40个不同原始任务ID均在只读读取的开发任务表中存在；每批映射3～4个原始任务，保留原始前置和显式外部依赖。
- 估算合计546～1048人时，仅覆盖本计划增量实现、测试和证据工作；各细项上限不超过18小时，不代表完整第一版剩余预算。
- 新规划文档的本地Markdown链接逐项检查。发现的三处历史change没有design.md，已改为其实际proposal.md；总计划中的本记录链接也已补齐。
- UI运行/截图门统一为1920×1080；Prefab结构仍检查语义锚点、Stretch、pivot/offset、布局组件尺寸责任和GF根Canvas缩放。不增加其他分辨率运行门。
- `git diff --check`通过。
- `python tools/audit_framework_purity.py --product-profile tools/audit_product_profile.json`：退出码0，framework purity audit passed。采用仓库已有产品授权profile，未更改豁免规则。
- `python tools/audit_project_boundaries.py`：退出码0，project boundary audit passed。
- 工作区变更限于新OpenSpec规划和相关Markdown决策/规范/索引。未修改产品实现、Unity资源、Excel或Git索引。

## 未执行与证据限制

没有执行Unity编译、EditMode、PlayMode、Player构建、1920×1080交互或性能测试；这轮交付是方案和计划。先前端口身份核查中的8090指向另一个工程，未在该实例执行目标项目测试。未来实施前须重新核验目标工程和可用端口。

现状判断来自代码与文档阅读。已有测试数量或历史通过记录不代替本次正式运行链验证；本记录也不证明G3/G4/G7/G8阶段完成。实施证据应按各change的tasks和总计划验收矩阵另行产生。

