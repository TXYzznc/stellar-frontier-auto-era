# 下一批：算法技术方案、编辑交互合同与跨批次覆盖收口

2026-09-11用户要求：已完成当前任务的角色执行下一批，未完成者继续；仅真正需决定时提问。本文件授权下述独立工作包，不授权未确认的新系统实现方案或自动Git。

## 已完成输入与防重开

- 2026-09-11用户已确认客户端提案推荐路线与分段；原方案包已完成，后续S1–S3实施改从 `b15-algorithm-core-services.md` 领取。本文原“仅方案”权限仍只适用于已完成的准备任务，不覆盖B15新实施授权。2D当前交互草案任务范围不变。

- B11机器服务、硬件UI正式迁入与运行交互已获用户最终确认，制作人记录为 `openspec/changes/b11-machine-runtime-and-hardware-ui/evidence/producer-delivery-summary-20260911.md`。
- B12身份桥接、B13导航、B14传感器本批实现与回归已交付；不重开历史准备包。
- B10 QA旧任务 `b10-initial-region-independent-validation` 已完成；新任务只做跨批次差异覆盖，不因旧矩阵开头“待实现”重测历史结论。
- 2D `b11-art2d-algorithm-editor-design-preparation` 已完成。本次使用现有 `ArtResource/Docs/ArtPipeline/UIRequirements/AlgorithmEditor/DesignPreparation_20260910.md`，不得重新交付同一盘点。
- B10十二模型细化通过，但正式交付方式仍待决定；本文件不解除材质/UV/导出/Unity门禁。原画无新待绘资产，不为填满窗口重画已通过图。

## client：client-algorithm-implementation-proposal-20260911

目标：从已有六类接口缺口进入可讨论、可直接转OpenSpec的最小算法实施方案，不止重复缺口清单。

只读现行GameDesign算法、节点类型/单位、模板、行为/队列/算力与最新决策，任务表只读；查实际代码与B11–B14已完成边界。交付到 `Docs/Development/Dispatch/Planning/Algorithm-Implementation-Proposal-20260911.md`：最小垂直切片、图文档/节点目录/端口类型/验证应用/调试快照/模板的责任与数据草案、草稿和已应用版本分离、调度与传感器适配、失败/等待/取消、自动测试、按任务表真实ID映射、允许/禁止路径及分阶段建议。对比必要方案并给出推荐，只列真正未定问题；不得把MotionGraph作为玩家算法运行时，不冻结新语言或新增玩法。

与2D点对点同步可用字段、状态、意图及未定部分；不等待UI生产才能准备服务。若QA发现当前真实实现缺陷，记录并交制作人，先继续本包安全分析，不抢占另改旧代码。

范围仅上述方案文档，不写C#/配置/Prefab/场景/框架核心，不创建新OpenSpec artifacts，不操作Unity/Git/xlsx。方案整体须经用户确认后另派实施。

## art-2d：art2d-algorithm-interaction-contract-20260911

目标：基于已完成设计准备，产出下一深度的具体交互合同草案，而非重复目录盘点。

允许仅 ArtResource `Docs/ArtPipeline/UIRequirements/AlgorithmEditor/InteractionContract_20260911.md`。对五区画布、节点创建/选中/连接/删除、绑定集中处理、草稿/应用切换、错误定位、模板选择/保存/删除、返回与焦点，列具体操作顺序、字段、互斥状态和接受/拒绝条件；给出1920×1080布局草案和必要视觉差异清单。与客户端对齐接口字段，未定字段明确标注不虚构接口。继承最新统一规则，不把各节点做独立语言、不新增算法灯状态。

不重做机器UI/基地中枢，不处理字体和多比例；本包不出图、不切图、不创建Prefab/C#或Unity资产。后续视觉方案交制作人结构功能报告再由用户决定，不直接向用户报通过。

## qa：qa-cross-batch-coverage-closeout-20260911

目标：核对B10基础/区域与B11–B14已交付组成的G0/G1/G2覆盖，消除重复任务和真正遗漏的混淆，不重复整批回归。

允许仅主仓库 `openspec/changes/b10-interactive-world-and-static-art/evidence/qa-cross-batch-coverage-20260911.md`。读取旧QA矩阵的最新增量、客户端覆盖与各批次证据、任务表真实完成定义和当前实际入口/测试；逐项给出已覆盖/仅夹具/缺集成/待用户/范围外。为B10 1.3、2.7与G0/G1/G2分别列可收口或精确剩余原因；旧历史数据、缺字体、多比例、无法追回的Physics升级前快照不制造重做。

先只读；本包不驱动Unity、不重跑覆盖原生XML、不写测试/实现/资源/tasks.md。必要新差异测试列精确目标、现有用例是否足够和工具交接条件，再发客户端与制作人，不未经协调改实现。保留用户可查看的实际场景路径及最小步骤，不用新Demo替代产品入口。

## 执行与完成

三个包独立并行，开始前快速执行候选检查；含专业方案判断的主体由原负责人完成，机械独立核验可按既有规则直接入快速执行队列。没有合适冻结包不为占用快速窗口造任务。

完成必须实际回传制作人，客户端与2D互抄相关字段结论；随后complete --claim-next。普通知悉不等待，真实待决定任务block让位。队列成功入队且核实窗口空闲后才发送一次启动消息。不操作Git或xlsx，不越权判用户视觉。
