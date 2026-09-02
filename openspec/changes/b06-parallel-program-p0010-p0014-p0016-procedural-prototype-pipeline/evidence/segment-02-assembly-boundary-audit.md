# 第 2 段：程序集与热更新边界核验

日期：2026-09-02

## 现状

- 现有产品代码统一编译进 `Assets/Game/Scripts/Hotfix.asmdef` 的 `Hotfix` 程序集。
- `ProjectSettings/HybridCLRSettings.asset` 当前仅登记 `Hotfix` 为热更新程序集；新增独立 Motion asmdef 将改变 HybridCLR 产物、引用和发布配置。
- FSR 仅适用于 Play Mode 内已有非结构方法体的短迭代；本 change 的新类型、字段、Prefab、程序集和资产变更必须退出 Play Mode 后做普通 Unity 编译。
- 现有测试使用 `Assets/Game/Tests/AutoEra/Editor/AutoEra.Editor.Tests.asmdef`，并已引用 `Hotfix`、`Builtin.Editor`、`GameFramework` 与 `UnityGameFramework.Runtime`。

## 冻结布局

| 源代码层 | 物理位置 | 编译边界 | 依赖规则 |
| --- | --- | --- | --- |
| Motion Core | `Assets/Game/Scripts/AutoEra/Motion/Core/` | 现有 `Hotfix` | 源代码不得引用 GF_X、AutoEra Adapter、玩法、场景或业务组件 |
| AutoEra Adapter | `Assets/Game/Scripts/AutoEra/Motion/Adapter/` | 现有 `Hotfix` | 仅将权威状态转换为 Core 强类型参数，不直接写关节 |
| Motion Editor | `Assets/Game/Editor/AutoEra/Motion/` | Unity 项目级 Editor 边界 | 仅依赖 Core／合同与 UnityEditor，不进入运行时或工具包业务层 |
| Motion 测试 | `Assets/Game/Tests/AutoEra/Editor/` | 现有 `AutoEra.Editor.Tests` | 使用确定性输入，不以场景或玩法状态作为单元测试前提 |

## 结论

- 本 change **不新增或修改 asmdef**；以目录和引用纪律实现 Core／Adapter／Editor 分层，避免改变当前 Hotfix、HybridCLR 和 FSR 配置。
- 后续跨项目 Motion 工具包发布通过显式清单筛选 Core／Editor 文件及固定 GUID，不把 AutoEra Adapter、GF_X、产品场景或美术资源带入 ArtResource。
- 未修改 `Assets/Game/ScriptsBuiltin/`、HybridCLR、FSR、asmdef、任务表或任何 `.xlsx`。
