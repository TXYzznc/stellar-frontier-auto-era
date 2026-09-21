# 硬件修改链

## ADDED Requirements

### Requirement: 硬件修改必须先经确认，界面不得直接改硬件

整备环境的安装／拆卸 SHALL 经 17-硬件修改确认页；
界面 SHALL NOT 在整备页直接调用机器花名册的安装／拆卸。
真正的执行 SHALL 由 `MachineHardwareOperation` 负责（来源门禁、等待安全停机、失败判定）。

#### Scenario: 未选择槽位时入口不可点

- **WHEN** 整备页没有选中任何槽位
- **THEN** 「安装或拆卸」SHALL 不可点
- **AND** 页面 SHALL 说明要先选一格

#### Scenario: 选中已占用槽位

- **WHEN** 选中一个装着组件的槽位
- **THEN** 「安装或拆卸」SHALL 可点
- **AND** 点击后 SHALL 打开 17-硬件修改确认页
- **AND** 此时机器 SHALL 尚未被改动

#### Scenario: 选中空槽位

- **WHEN** 选中一个空槽位
- **THEN** 界面 SHALL 说明装入方向需要 12-组件选择器
- **AND** SHALL NOT 静默什么都不做

### Requirement: 硬件修改请求不得携带来源

请求 SHALL 只描述「哪台机器、哪一类硬件的第几格、装还是拆、装的是谁」；
来源 SHALL 由确认页按机器的部署状态推导（未部署 → 整备环境，已部署 → 现场）。

#### Scenario: 库中机器提交

- **WHEN** 对一台未部署的机器确认拆卸
- **THEN** 提交 SHALL 使用整备环境来源
- **AND** 领域 SHALL 接受并完成（而非以来源不允许拒绝）

#### Scenario: 确认页说明运行影响

- **WHEN** 打开硬件修改确认页
- **THEN** 页面 SHALL 列出本次卸下／装入的内容
- **AND** SHALL 说明算法绑定不会自动重新绑定

### Requirement: 槽位选择必须与机器绑定

槽位选中态 SHALL 属于某一台机器；切换选中的机器后 SHALL 自动作废，
SHALL NOT 把上一台机器的槽位套到新选中的机器上。

#### Scenario: 换机器后选中态作废

- **WHEN** 在 A 机器上选中「核心槽 0」，随后改选 B 机器
- **THEN** 整备页 SHALL 报告没有选中槽位
- **AND** 「安装或拆卸」SHALL 回到不可点

### Requirement: 槽位名字与拒绝原因只有一个来源

槽位的显示名与硬件修改被拒绝时的可展示原因 SHALL 由显示层统一给出，
整备页与确认页 SHALL NOT 各写一份。

#### Scenario: 不同的拒绝原因不得显示成同一句话

- **WHEN** 领域以 `InvalidOrigin`、`InvalidSlot`、`Occupied`、`MissingComponent` 等不同原因拒绝
- **THEN** 界面 SHALL 给出互不相同的可展示原因
- **AND** SHALL NOT 直接把枚举名端给玩家
