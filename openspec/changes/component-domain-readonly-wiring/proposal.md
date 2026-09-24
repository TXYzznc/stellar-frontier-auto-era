# 组件域只读接入

## Why

`ui-game-system-integration` 的 2.7 把 15 个界面按「该界面依赖的领域服务在生产里没有创建者」
圈成了「未接入」，组件域三页是其中之一。但代码侦察的结论与这个笼统判定不一致：

| 零件 | 状态 | 生产调用者 |
|---|---|---|
| `ComponentDefinitions` 数据表（22 行：型号、类别、等级、承载／算力／逻辑算力、带行为、价格、功率、可用性） | 已交付，随 `Refresh All Excels` 生成 | **无消费方**（`MachineCatalog` 只取了装配需要的最小子集） |
| `MachineCatalog` 的组件解析与校验 | 完整（含 `Kind` 合法性、`ModelId*10+Level` 身份规则、可用性枚举） | 机器/组件相关的运行路径 |
| `MachineRoster.Components`（全部组件实例）与每台机器的槽位（`GetComponent(kind,index)`） | 完整（安装／卸载／容量与算力闸门都有实现与测试） | 机器整备的写路径（界面尚未接） |
| 组件库三页（散件／已安装／组件详情） | 契约与预制体齐备，字段已生成 | 只有 `NOT_WIRED` 注入的「整页未接入」 |

也就是说，**这一域的两半都已经在生产里存在**：型号规格在数据表、实例与安装位置在花名册。
它显示「未接入」的真正原因是**界面没有观察入口**——花名册把组件当成内部结构，
没有任何读模型去枚举它，目录也没有对外暴露展示所需的字段（名字、价格、功率、可用性）。

直接后果：玩家在组件库看到的是「组件域尚未接入运行路径」，而实际上他手上的机器里
已经装着组件、库里也可能有散件；`ComponentPickerForm` 与 `UpgradeForm` 同理被一句话挡住。

## What Changes

- **目录对外暴露组件展示行**：`MachineCatalog` 为**每一行**（含 `PendingConfiguration` /
  `PendingResource`）建立 `ComponentDisplayRow`（名字、类别等级、承载／算力／逻辑算力、
  带行为、购入价、回收价、功耗、可用性），并提供「按行 Id」与「按运行时定义反查」两个入口。
  反查关系就是目录自己的身份规则 `ModelId * 10 + Level`，因此是权威的，不是按名字猜的。
  刻意**不隐藏未就绪的型号**：那条可用性本身就是信息。
- **新增组件域读模型**：散件（花名册里没有归属机器的实例）＋ 已安装（**按机器槽位枚举**，
  因此一次拿到机器名、类别与槽位号）＋ 选中详情（目录给规格、花名册给实例与位置）。
  订阅 `MachineRoster.Changed`；值类型快照在每次变化时重建。
- **`ComponentLibraryForm` 从「未接入」转为手写接入**（连同生成器的 `NOT_WIRED` → `HANDWRITTEN`
  与回归围栏名单一起迁移）：三页按 `Ready`/`Empty`/`Unavailable` 三态渲染，
  列表走对象池，点行按稳定 Id 选中。
- **写动作仍然禁用并写明原因**：安装／卸载要选目标机器槽位并过容量与算力占用闸门，
  改装还没有规则载体——只读部分是真实数据，写部分是明确不可用，两者不混。

## Capabilities

### New Capabilities

- `component-domain-read-model`：组件目录（数据表）与组件实例（花名册）的只读合并视图。

### Modified Capabilities

- 无。本变更不改界面契约、不改预制体结构、不改 `UIViews` 登记、不改数据表。

## Impact

- 代码：`Assets/Game/Scripts/AutoEra/Machines/MachineCatalog.cs`、
  `Assets/Game/Scripts/AutoEra/Machines/MachineDefinition.cs`（新增展示行类型）、
  `Assets/Game/Scripts/AutoEra/UI/Integration/ComponentReadModel.cs`（新增）、
  `Assets/Game/Scripts/AutoEra/UI/ComponentLibraryForm.cs`（转为手写）、
  `Tools/ui_contract_to_form_script.py`（`NOT_WIRED` → `HANDWRITTEN`）。
- 验收：门1 必须持续 33/33；`run_project_checks.py` 5/5；EditMode/PlayMode 回归全绿；
  新增「组件域读模型」EditMode 用例与「组件库渲染真实行、安装后跨页跟随」的 PlayMode 用例。
- **不在本变更内**：安装／卸载／改装的写路径与界面入口、`ComponentPickerForm`（候选过滤需要
  编辑器或机器槽位上下文）、`UpgradeForm`（改装规则尚无设计载体）、组件经济（购入／回收）。
- **风险点**：目录与花名册不一致时（有实例、目录缺行）必须**明说**而不是编造规格——这条已按
  「不伪造数据」的既有口径实现，并由用例守着。
