# 动作衔接与算法编辑器美术制作

2026-09-12 用户明确批准制作人建议：客户端制作导航／作业与动作系统衔接，2D制作算法编辑器素材及视觉Prefab。不是重新盘点或恢复已完成B15/B10。

## 客户端：client-motion-work-bridge-20260912

输入：Planning/G2-Movement-WorkQueue-Readiness-20260911.md（相对于本Dispatch目录）、现行GameDesign、B06/B11至B15合同和真实实现。准备文档不是新规则；其中疑问先对照最新已确认规则自行消歧，不能反复要求用户确认已有规则。

目标：沿用MachineNavigationMotionAdapter、MotionExecutor、RegionWorkQueue、EffectorBehaviorQueue等既有边界，接通导航准备/行进/到达/失败/取消与动作表现、作业预约/等待/安全取消/释放。任务与导航结果由已有服务权威产生，表现不得反向结算结果。沿用当前取消、安全点、断电和退出规则；不增加自动重试、玩家手动作业入口、完整交通系统、生产结算或物流功能。

先依据本授权建立独立OpenSpec并严格校验，随后直接连续实现，不停在“准备好了”。允许主仓库Assets/Game/Scripts/AutoEra/中的本次最小桥接代码及必要既有调用点、Assets/Game/Tests/AutoEra/对应测试、新change及其证据。实施前在change列明精确文件、真实API、结果权威和取消顺序。禁止ScriptsBuiltin、包/框架改造、xlsx/Git、正式美术几何材质、正式场景/Prefab改写。联合验证可在临时隔离测试现场实例化已验收正式模型与MotionGraph，不保存过程资产到正式目录。

验收：同一机器/任务/请求ID；真实区域和NavMesh导航到动作表现；一个既有效应器的作业排队/取消/释放；UI关闭不取消工作；断电与区域退出无残留；重复运行无漂移；既有B13/B15相关回归不倒退，普通编译、引用、Console与边界审计。若特定行为缺少真实已确认合同，仅隔离该差异并回传，继续其它安全工作，不杜撰生产端点。正式项目 `stellar-frontier-auto-era` 使用 Unity 8092；ArtResource 使用 Unity 8091。测试重跑按快速执行规则拆包。

## 2D：art2d-algorithm-assets-prefabs-20260912

> **状态更新（2026-09-16）**：本任务由用户决定标记为**未开始**，已从 art-2d 队列移除，生命周期状态登记为 `Cancelled`（未完成、未验收通过）。之前生成的相关临时产物已隔离，不作为后续依据。以下原有授权内容保留为历史记录，**不再作为执行依据**；2D 美术线在收到用户新的范围/派发授权前，不得重新入队或启动本任务。重新执行前需先由用户讨论“该任务需要如何修改后才能重跑”以及“当前执行存在什么问题”。

用户本次授权从已确认v02进入素材与视觉Prefab制作。输入为ArtResource Docs/ArtPipeline/UIRequirements/AlgorithmEditor/InteractionContract_20260911.md、VisualCandidates/v02/SourceManifest_v02.md及四张最终图；既有生产准备不重做。

顺序：确定最小复用切片与节点/状态映射→制作透明素材→素材自检→装配一个可复用算法编辑器视觉Prefab及代表状态→1920×1080截图与交付。保持常规编辑/集中绑定/问题定位/历史诊断，保留载体自有导航、明确传感器绑定、草稿与生效版本隔离及非交互等待状态条。文字用可编辑UI节点，不把整页截图当Prefab，不把完整文本烘入通用背景；仅复用已有字体，不开展字体工作。不得重画已通过整套风格；确需新视觉方向先交制作人报告供用户决定。

允许ArtResource Docs/ArtPipeline/UIRequirements/AlgorithmEditor/中的交付清单/资源及布局合同/证据，Assets/Art/Authoring/AlgorithmEditor/下独立Sprites、PrefabCandidates与验证场。使用适用拆图技能和现成合法素材；缺失组件按已确认风格最小补齐，不以简单脚本线框冒充美术。允许8091导入、Importer、视觉Prefab机械装配和截图；禁止主仓库正式UI迁移、业务C#/GF UIForm/输入接线、字体、多比例、xlsx/Git、已验收基地/机器UI修改。

验收：切片透明边缘/无背景残留、状态资源完整；真实可编辑节点与清晰层级、遮挡/raycast边界、四代表状态1920截图、无Missing Reference和新增Console错误。实际查看输出，不仅检查文件存在。完成整包先交制作人技术/功能检查，再由用户裁决视觉；不自动宣称完整算法UI运行时完成。可点对点向客户端核对B15公开字段，不能打断其动作桥接任务；本包不依赖客户端为其新增业务代码。

## 共用规则

两个任务独立入队，空闲领取后一次启动。普通阶段不暂停，真实外部阻塞block让位；完成实际回传制作人并complete --claim-next。开始每个冻结工作单元执行快速执行候选检查；低风险机械包可直接排入快速执行，专业负责人保留复核。无Git自动触发，无任务表/xlsx写入。

