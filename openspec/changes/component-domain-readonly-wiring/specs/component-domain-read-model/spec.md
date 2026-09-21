# 组件域只读读模型

## ADDED Requirements

### Requirement: 组件目录必须暴露展示所需的字段

`MachineCatalog` SHALL 为 `ComponentDefinitions` 的**每一行**（含 `PendingConfiguration` 与
`PendingResource`）建立展示行，包含名字、类别、等级、承载／算力／逻辑算力、带行为、
购入价、回收价、待机与工作功耗、可用性；并 SHALL 提供按数据表行 Id 与按运行时定义
（`ModelId * 10 + Level`）两个查询入口。

不允许隐藏未就绪的型号：可用性本身就是界面要展示的信息。

#### Scenario: 未就绪型号仍在目录里

- **WHEN** 一行组件的 `Availability` 是 `PendingResource`
- **THEN** 它 SHALL 出现在 `ComponentRows` 中，且 `IsReady` 为 false

#### Scenario: 由运行时定义反查展示行

- **WHEN** 持有一份 `ComponentDefinition`（`Id` = ModelId，`Level` = 1..2）
- **THEN** `TryGetComponentRow` SHALL 用 `Id * 10 + Level` 命中对应的数据表行

### Requirement: 组件域读模型必须合并目录与花名册

读模型 SHALL 把「型号规格」（数据表）与「实例与安装位置」（机器花名册）合并成
散件列表、已安装列表与选中详情三部分；已安装项的机器名、类别与槽位号
SHALL 来自**机器槽位枚举**，而不是从实例反查。

#### Scenario: 未安装的组件是散件

- **WHEN** 花名册里存在一件 `OwnerId` 无效的组件实例
- **THEN** 它 SHALL 只出现在散件列表，且详情 SHALL 包含来自目录的规格

#### Scenario: 安装后跨分组跟随

- **WHEN** 一件已选中的散件被安装到某台机器的槽位
- **THEN** 它 SHALL 离开散件列表、出现在已安装列表，选中 SHALL 保留，
  且详情 SHALL 给出机器名、类别与槽位号

#### Scenario: 目录缺行时不得编造规格

- **WHEN** 某个实例的型号在目录里查不到
- **THEN** 详情 SHALL 明说「目录里没有这件组件的型号行」，而不是显示零值规格

### Requirement: 同类未安装组件必须合并显示且保留实例

组件库的散件列表 SHALL 把**类型、型号与等级三者都相同**的未安装组件合并成一行并显示数量；
合并后的组 SHALL 逐个保留成员的稳定身份，选中该组时详情 SHALL 列出成员实例。
已安装组件 SHALL NOT 参与合并：它们必须显示所在机器与槽位。

#### Scenario: 相同散件合并

- **WHEN** 花名册里有三件类型／型号／等级完全相同的未安装组件
- **THEN** 散件列表 SHALL 只有一行且数量为 3
- **AND** 该组 SHALL 保留三个不同的实例身份

#### Scenario: 同型号不同等级不合并

- **WHEN** 两件组件型号相同但等级不同
- **THEN** 散件列表 SHALL 显示为两行

#### Scenario: 选组与选实例互斥

- **WHEN** 先选中一整组散件，再选中其中某一件实例
- **THEN** 选中 SHALL 落在该实例上，组的选中 SHALL 被清掉

#### Scenario: 组消失后清理选中

- **WHEN** 被选中的组因为最后一件成员被装走而消失
- **THEN** 快照 SHALL NOT 报告该组仍被选中

### Requirement: 组件域读模型必须区分不可用与为空

读模型 SHALL 用 `Unavailable` 表示缺能力（没有会话／没有世界／花名册不可用／
组件定义表尚未加载），用 `Empty` 表示域已接线但这次没有内容；两者 SHALL 各自可辨。

#### Scenario: 花名册为空是 Empty

- **WHEN** 世界会话可用、目录可用，但花名册里一件组件都没有
- **THEN** 快照状态 SHALL 为 `Empty` 并带可展示原因

#### Scenario: 定义表未加载是 Unavailable

- **WHEN** 无法取得组件目录
- **THEN** 快照状态 SHALL 为 `Unavailable`，原因 SHALL 指向组件定义表

#### Scenario: 数据表格式错误不得被吞掉

- **WHEN** 数据表里的行违反目录校验规则
- **THEN** 目录 SHALL 抛 `FormatException`，读模型 SHALL NOT 把它吞成空目录

### Requirement: 组件库界面必须渲染真实数据

`ComponentLibraryForm` SHALL 从组件域读模型渲染散件页、已安装页与组件详情页，
列表 SHALL 走对象池（`SpawnItem` ＋ `Item_*Template`），点行 SHALL 按实例的稳定 Id 选中；
安装／卸载／改装等写动作 SHALL 保持禁用并说明原因。

#### Scenario: 有组件时状态为 Ready

- **WHEN** 花名册里至少有一件组件
- **THEN** 界面数据状态 SHALL 为 `Ready`，且对应列表 SHALL 渲染出真实行

#### Scenario: 写动作不可用但只读可用

- **WHEN** 打开组件库
- **THEN** 业务写按钮 SHALL 禁用，列表行 SHALL 仍可点击选中
