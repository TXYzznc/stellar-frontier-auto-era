# 机器整备只读读数

## ADDED Requirements

### Requirement: 整备页三栏必须来自同一份机器快照

整备页的载体概况、组件整备与部署准备三栏 SHALL 由机器域读模型从同一份快照派生，
SHALL NOT 各自查询机器花名册。

#### Scenario: 未选中机器

- **WHEN** 读模型没有选中任何机器
- **THEN** 三栏 SHALL 都为空

#### Scenario: 槽位逐个列出

- **WHEN** 选中的机器有传感器 2、核心 1、执行器 2 个槽位
- **THEN** 组件整备栏 SHALL 为每一个槽位出一行
- **AND** 空格 SHALL 标为「空」，占用的格子 SHALL 给出型号名与工作状态

#### Scenario: 目录缺行时不得编造名字

- **WHEN** 槽位里的组件型号在目录里查不到
- **THEN** 该行 SHALL 退化为型号编号，SHALL NOT 编造一个名字

### Requirement: 出售资格必须由载体自身状态判定

出售资格 SHALL 按规格「出售空载完好载体」判定：已部署、装有组件或完整度受损时 SHALL 说明原因；
三者都通过时 SHALL 报告资格成立，且 SHALL NOT 给出价格（价格属于经济域）。

#### Scenario: 空载完好的库中载体

- **WHEN** 机器未部署、无已装组件、完整度为满
- **THEN** 出售资格 SHALL 报告资格成立

#### Scenario: 装有组件的载体

- **WHEN** 载体上装着组件
- **THEN** 出售资格 SHALL 说明需要先一键卸下

#### Scenario: 已部署的载体

- **WHEN** 载体已部署
- **THEN** 出售资格 SHALL 说明必须先撤收回库

### Requirement: 未就绪的域必须明说而不是编造

部署解锁条件 SHALL 在成长解锁域没有创建者时明说该缺口，SHALL NOT 显示「已解锁」或「未解锁」。

#### Scenario: 解锁域未接入

- **WHEN** 读取部署准备栏
- **THEN** 解锁条件一行 SHALL 指向成长解锁域尚未创建这一事实

### Requirement: 整备页必须可达且写动作说明下一步

`Btn_UndeployedMachinesPrepare` SHALL 在选中库中机器时可点并进入整备页；
整备页的写动作在前置对话框未接线时 SHALL 禁用并说明各自的下一步。

#### Scenario: 从库中机器进入整备

- **WHEN** 在未部署机器页选中一台机器并点击整备
- **THEN** 界面 SHALL 落在整备页
- **AND** 三栏 SHALL 渲染出该机器的真实内容

#### Scenario: 已部署机器不可整备

- **WHEN** 选中的机器已部署
- **THEN** 整备按钮 SHALL 不可点
