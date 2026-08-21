# 多窗口职能与Agent路由

工作窗口固定长期职能域，具体派发单分别指定方案、实施和协作阶段使用的Agent职责。
窗口名称不等于单个全能Agent；Agent不得越过自己的职责与SKILL白名单承包跨职能工作。

## 已确认路由

| 工作窗口 | 方案阶段 | 实施阶段 | 常见协作 |
|---|---|---|---|
| 制作人 | `producer` | 通常不实施 | 各专业负责人 |
| 主美（3D） | `art-director` | `art-3d` | `client-ta`、`art-anim` |
| 美术（2D） | `art-director` | `art-2d`、`art-ui`、`art-font` | `client-unity` |
| 客户端 | `client-lead` | `client-unity`、`tools-engineer`、`client-ta` | `qa-engineer`只负责独立验证 |
| 后端 | `net-lead` | `net-backend`、`net-db` | `devops-engineer`、`qa-engineer` |
| 测试 | `qa-engineer` | 默认只诊断和报告 | 缺陷交回原实现窗口 |
| 专项策划 | `game-designer` | `game-designer`产出规则、数值、配置语义和验收规格 | `producer`、对应技术／美术负责人 |

## 项目级`game-designer`方向

用户已确认新增一个项目级`game-designer`，不预建数值策划、动作策划、系统策划等多个
常驻Agent。派发单使用“专项方向”限定当前上下文，例如“数值与经济”或“程序化动作策划”。

初始计划只复用现有`grill-with-docs`、`xlsx`和`state-machine`能力；只有真实任务证明存在
稳定能力缺口后，才单独设计数值平衡或动作策划SKILL。

`game-designer`总体负责玩法循环、系统规则、边界条件、数值公式与资源流、成长解锁、
动作策划规格、配置语义和策划验收场景。它不负责制作排期、Unity代码、3D／动画资源
实现或独立测试执行。

## 尚待确认

- `game-designer`可直接修改哪些正式设计文档、配置表和策划资产。
- 已确认设计发生变化时，它是直接修改、先提案，还是按影响等级采用不同门禁。
- 与`producer`、`client-lead`、`art-director`发生职责交叉时的权威和交回规则。
- 最终Agent配置、SKILL白名单和项目Agent审计兼容方式。

以上待决项确认并形成完整决策摘要前，不创建Agent配置或修改SKILL矩阵。
