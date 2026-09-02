## Why

现有美术流程先制作模型、再由程序适配动作，已经造成结构、Pivot、Socket、活动包络与运行时需求反复返工。下一阶段需要把可验证的程序结构原型和通用动作系统前置，使正式模型成为对稳定功能合同的视觉替换，而不是动作系统的结构输入。

## What Changes

- 对应任务表 `P0-010`、`P0-014`、`P0-015`、`P0-016`，建立程序优先的结构原型、动作核心、动作工具与代表性贯通流程。
- 使用不同尺寸、位置和角度的基础几何构建合同级功能原型；原型必须先冻结尺寸、层级、Pivot、Socket、`WorkPoint_*`、`KeepOut`、碰撞边界、绑定姿态和运动限制。
- 首批纵向切片覆盖轮式载体、四轮机构、多关节机械臂、可替换效应器、滑动门和传送带，用少量对象覆盖旋转、伸缩、循环、接地、目标求解、安全中断和恢复。
- 建立数据驱动的 `MotionRig`、动作图／配置资产、强类型参数、基础运动原语和集中式执行器；对象专用 Adapter 只能写入表现参数，不得直接操纵关节或决定玩法结算。
- 从通过验收的程序原型导出版本化 `FunctionalRigContract`，由美术技术合同引用；正式模型只能替换视觉子对象，功能结构变更必须先修改程序原型和合同。
- 建立自动结构校验、确定性动作回归、固定测试面板和独立演示场的三层验收；用户只承担最终可视结果验收，常规结构与数值问题由专业窗口自治处理。
- **BREAKING**：对于可动或关键装配对象，废止“先完成正式模型，再由程序适配动画”的生产顺序。此前美术 OpenSpec 中与该顺序冲突的建模前置规则必须在实施前按本变更同步。

## Capabilities

### New Capabilities

- `functional-rig-prototype`: 定义合同级基础几何原型、逻辑根／表现根边界、稳定关节与锚点标识、版本化功能合同及正式模型替换规则。
- `procedural-motion-runtime`: 定义数据驱动 MotionRig、动作配置、强类型参数、运动原语、集中执行、中断恢复、Adapter 权威边界与运行时性能要求。
- `motion-authoring-acceptance`: 定义动作配置、预览、静态校验、Gizmo、工具包发布、代表性纵向切片和自动／可视三层验收。

### Modified Capabilities

- `art-resource-delivery-contract`: 将可动机器与关键机构的生产门调整为程序功能原型和 `FunctionalRigContract` 先通过，再制作并替换正式视觉模型。

## Impact

- 主工程产品代码进入 `Assets/Game/Scripts/AutoEra/`，Editor与测试代码进入既有项目级Editor／Tests边界；不得把 AutoEra 业务写入 `Assets/Game/ScriptsBuiltin/`。
- ArtResource 消费独立版本化的 Motion Core／Editor 工具包和 `FunctionalRigContract`，不依赖 GF_X 或 AutoEra 玩法代码。
- 影响现有 `b03-parallel-art-art004-art005-production-rnd` 的生产顺序，以及暂停中的 `b03-parallel-program-art004-art005-animation-demo` 依赖门；后者只能在本变更纵向切片和替代视觉层通过后恢复。
- 不实现完整物理车辆、力反馈、玩法结算、导航权威、正式模型、材质、VFX或正式UI；结构原型只提供可替换的表现层和验收输入。
- 运行热路径不得持续产生GC分配；所有循环、中断、恢复和对象池复用必须可重复且无累积漂移。
- `第一版开发任务表.xlsx` 永久保持AI只读；状态与依赖调整只能形成用户手动修改建议。
