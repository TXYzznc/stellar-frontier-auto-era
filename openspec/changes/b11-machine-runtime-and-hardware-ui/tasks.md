## 1. 复用与数据

- [x] 1.1 客户端核对现行设计、P0-011/P2范围与既有代码，输出client-baseline精确文件/ID/依赖清单（evidence/client-baseline.md；旧售价/单载体与当前DEC差异已记录）
- [x] 1.2 建立第一版配置覆盖及真实资源引用清单，区分尚未交付与非法引用（client-config-coverage.md；Catalog/FirstVersionObjects.json 40项资源覆盖元数据，不代表全部系统数值已就绪）
- [x] 1.3 通过安全JSON工具生成配置并验证GF读取、重复/非法值/引用及事务兼容（client-config-coverage.md、client-machine-services.md；GF 698bc778 1/1、Catalog 6853fba8 14/14；沿用既有单表指纹/事务工具，不声明全目录可实例化或多表整体原子事务）

## 2. 机器业务

- [x] 2.1 实现机器型号序号、唯一名称、永久引用与可恢复数据，验证跨回收不复用（client-machine-services.md；内存安全配置恢复ac85bbbc 2/2，非磁盘/活动行为存档）
- [x] 2.2 实现两载体槽位、组件实例归属与停止/现场安装拆卸门禁（client-machine-services.md；9f3e7372、110806c3；服务验证，不替代4.x UI）
- [x] 2.3 实现激活/运行/休眠/供电/损坏/连接及来源权限状态，验证恢复不丢配置（client-machine-services.md；9667c0d0、9f3e7372、e69ee397；外部电力/信号为适配输入）
- [x] 2.4 实现机器五档优先级与有界FIFO队列、取消和结果追踪（client-machine-services.md；cc72bb4c、4807cfc4）
- [x] 2.5 实现效应器独立行为队列、安全切换和恰好一次终止结果（client-machine-services.md；4807cfc4、c83afcf4、21e3b72c、bfd86376；物理执行器仍需报告安全点，不伪造物理完成）
- [x] 2.6 实现共享算力池、逻辑容量、等待/释放/合并及确定性测试（client-machine-services.md；4807cfc4、e69ee397；未实现算法求值器）

## 3. 并行UI设计与生产

- [x] 3.1 2D与客户端对齐机器概况/槽位/选择/安装拆卸字段、意图和状态矩阵（AssemblyReview_V02节点/状态合同及客户端接线记录）
- [x] 3.2 完成本批交互文档与效果图，复用风格，实际提交制作人结构功能报告（四视图及V02报告已回传，见producer-delivery-summary-20260911）
- [x] 3.3 用户确认本批视觉，未确认不得以技术检查代替通过（2026-09-11四视图及V02整包用户明确通过，正式迁入获准；非用户运行态验收）
- [x] 3.4 视觉通过后连续完成必要新素材及独立候选Prefab，无资源缺口直接复用（26 Sprite与Panel V02，精确清单CandidateManifest_V02）
- [x] 3.5 2D自验1920×1080、实际资源引用/状态/无叠字并回传制作人和客户端（19状态截图、AssemblyAudit_V02及实际回传；增量启停显示问题由客户端修复）

## 4. 接线与交付

- [x] 4.1 客户端接入实际GF机器现场概况/硬件与中枢机器摘要，复用既有Form/InputModule（client-v02-runtime-binding.md；正式FieldHud子层/真实Roster与中枢计数，非完整机器购买部署UI）
- [x] 4.2 验证重命名、停止/权限、安装拆卸、状态与算力/队列真实投影；无手动作业入口（08de1e8a 7/7、fd75feb2 2/2、ece7e7dc 2/2及70ec395b新鲜XML1/1；来源/环境/物理完成为显式适配输入，不冒充真实能源或相机命中验收）
- [x] 4.3 QA进行差异回归、正常编译/Console/引用、产品纯度与边界检查，留可查看入口（显示修复后41511996/25e87701/f3135234共11/11；d750572d新鲜原生XML1/1获QA独立签收，Missing Scripts/Console Error均0；strict/purity/boundary通过。见client-v02-runtime-binding.md，不代替用户视觉或完整G2验收）
- [x] 4.4 制作人汇总实际任务覆盖、视觉用户决定和未覆盖依赖，不将本批等同完整G2/P0-011（producer-delivery-summary-20260911；2026-09-11用户已明确确认本批运行界面与交互通过，未覆盖范围保持）

## 5. 后续设计衔接

- [x] 5.1 机器UI交付收口后2D领取独立算法编辑器设计准备包，仅盘点现行端口/节点/画布规则与已有素材，不实现算法运行时（实际按外部阻塞让位于2026-09-10先行完成；ArtResource AlgorithmEditor/DesignPreparation_20260910.md与已完成队列记录。2026-09-11巡查补记，不重复执行；后续出图/算法实现未授权）
