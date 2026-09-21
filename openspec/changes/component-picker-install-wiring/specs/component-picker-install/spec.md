# 装入方向：组件选择器必须只交出候选，不自己安装

## ADDED Requirements

### Requirement: 组件选择器的候选是散件，兼容判据只有类别一致

候选 SHALL 来自机器花名册里**没有归属机器**的组件实例；某一件候选与目标槽位是否兼容
SHALL 只按「硬件类别与槽位类别一致」判定。
其余不可安装因素（可用性等）SHALL 照原样展示，SHALL NOT 被界面用来提前拦下。

#### Scenario: 不兼容的候选仍要列出来

- **WHEN** 库里有一件执行器、目标槽位是核心槽
- **THEN** 该执行器 SHALL 出现在候选列表里
- **AND** SHALL 给出「它是执行器，而本槽位是核心」这样的具体原因

#### Scenario: 兼容的候选排在前面

- **WHEN** 候选里既有兼容也有不兼容的组件
- **THEN** 兼容的 SHALL 排在前面

### Requirement: 确认只能交出候选身份，不得直接安装

`使用该组件` SHALL 把候选的稳定身份写回调用方的结果对象，并打开 17-硬件修改确认；
选择器 SHALL NOT 自行安装、替换或售卖任何组件。

#### Scenario: 确认之前机器没有被改动

- **WHEN** 在空的槽位上预选一件兼容候选并确认
- **THEN** 槽位 SHALL 仍然是空的
- **AND** 该组件 SHALL 仍然没有归属机器
- **AND** 17-硬件修改确认页 SHALL 被打开

#### Scenario: 没有预选就不能确认

- **WHEN** 还没有预选任何候选
- **THEN** `使用该组件` SHALL 不可点
- **AND** 页面 SHALL 说明「还没有预选组件」

#### Scenario: 预选了不兼容的候选

- **WHEN** 预选的候选与槽位类别不符
- **THEN** `使用该组件` SHALL 不可点
- **AND** SHALL 给出该候选自己的不兼容原因

### Requirement: 比较栏必须给当前值与装有之后的值

「与当前安装比较」SHALL 给出**当前值 → 装有之后的值**，SHALL NOT 只给一个孤立增量；
SHALL 包含算法绑定影响与不兼容原因。

#### Scenario: 预选一件兼容候选

- **WHEN** 预选一件有容量与算力加成的组件
- **THEN** 比较栏 SHALL 给出容量、算力与逻辑算力三行的「当前 → 装有后」
- **AND** SHALL 说明算法绑定不会自动重新绑定
- **AND** SHALL 报告兼容

#### Scenario: 槽位已被占用

- **WHEN** 目标槽位里已经装着组件
- **THEN** 比较栏 SHALL 提示需要先拆下
- **AND** `使用该组件` SHALL 仍然可用（先拆后装由玩家决定，占用与否由领域判定）

### Requirement: 预选项必须始终属于当前候选

预选项 SHALL 在每次读模型刷新时校验；不再属于候选（被装走、被移除）时 SHALL 自动作废。

#### Scenario: 候选被装到别处

- **WHEN** 已预选的组件被安装到某台机器上
- **THEN** 预选 SHALL 被清掉
- **AND** `使用该组件` SHALL 不可点

### Requirement: 整备页的安装入口按格子分流

整备页选中一个槽位后，`安装或拆卸` SHALL 按格子分流：
已占用的格子 SHALL 直接打开 17-硬件修改确认（拆卸）；空格子 SHALL 先打开 12-组件选择器。

#### Scenario: 选中空格位

- **WHEN** 整备页选中一个空槽位
- **THEN** `安装或拆卸` SHALL 可点
- **AND** 点击后 SHALL 打开组件选择器而不是确认页

#### Scenario: 选中已占用槽位

- **WHEN** 整备页选中一个装着组件的槽位
- **THEN** 点击 `安装或拆卸` SHALL 打开拆卸确认页

### Requirement: 装入全链必须由领域执行

从选择器确认到硬件真正装入，SHALL 经 17-硬件修改确认提交给 `MachineHardwareOperation`；
成功后组件 SHALL 归属该机器，SHALL NOT 再出现在候选里。

#### Scenario: 提交后领域真的装上

- **WHEN** 在确认页提交装入
- **THEN** 目标槽位 SHALL 装着那件组件
- **AND** 该组件 SHALL 归属这台机器
- **AND** 整备页再次读取时 SHALL 看到它
