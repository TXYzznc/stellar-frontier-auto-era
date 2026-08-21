## Why

第一版正式美术生产即将开始，但独立美术项目与主工程之间尚缺少可验证的交付合同，视觉方向也没有通过统一场景和统一标准完成候选比较。现在需要先锁定跨项目边界、视觉基线和场景制作路线，避免正式资源批量生产后因比例、绑定、材质或场景方案不成立而返工。

## What Changes

- 对应任务表原始任务 `ART-001`、`ART-002`、`ART-003`，建立第二执行批次的并行美术提案。
- 将 `D:\unity\UnityProject\ArtResource` 定义为美术源文件与美术产出的唯一来源，建立与主工程之间的尺度、坐标、命名、Prefab、材质、锚点、程序化动作接口和最小 `.unitypackage` 往返合同。
- 在统一 LookDev 条件下制作并比较不少于三组完整视觉候选方向，每组覆盖环境、机器、建筑、色彩、材质、灯光与俯视可读性，并收敛为一套正式视觉基线。
- 使用同一代表性测试区验证 Terrain、模块化、手工布景与混合方式，细化“固定玩法骨架＋编辑器程序生成视觉环境＋烘焙保存＋人工修整”的第一版场景路线。
- 建立进入后续 Art Bible、程序化动作样机和正式资源批量生产前的证据、检查清单与阶段门。
- 不实现正式批量资源、运行时随机地图、无限区块、动物动画、可视化动作节点编辑器或 Motion Core／Editor 代码。

## Capabilities

### New Capabilities

- `art-resource-delivery-contract`: 独立美术项目的源权威、制作边界、资源技术合同、固定引用要求和最小交付包往返验证。
- `visual-direction-baseline`: 明亮风格化中模的候选方向生产、统一 LookDev 比较、评审矩阵和正式视觉基线收敛要求。
- `authored-environment-generation`: 固定玩法骨架与编辑器程序生成视觉环境的职责边界、代表性样片、可编辑烘焙结果和技术验证要求。

### Modified Capabilities

无。

## Impact

- 主要影响独立美术项目 `D:\unity\UnityProject\ArtResource` 的目录约定、LookDev 场景、试制资源、导入设置、场景样片和交付证据。
- 主工程仅承担最小交付包导入、比例／材质／锚点／GUID／引用验证和结果记录；产品业务代码仍受 `Assets/Game/Scripts/AutoEra/` 与 `AutoEra.*` 边界约束。
- 程序化动作在本变更中只形成美术资源接口合同；`P0-014`～`P0-016` 的运行时、编辑器工具与真实玩法状态适配由独立程序 OpenSpec 处理。
- `第一版开发任务表.xlsx` 永久保持 AI 只读；本变更不写回任务状态、实际工时或依赖。
- 不修改 `Assets/Game/ScriptsBuiltin/`、GF_X 框架核心、现有 asmdef、HybridCLR 或 FSR 配置。
