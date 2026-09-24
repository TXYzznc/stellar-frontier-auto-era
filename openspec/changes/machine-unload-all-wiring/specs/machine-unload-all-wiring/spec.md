# 一键卸下全部：整备环境的原子回库

## ADDED Requirements

### Requirement: 一键卸下把整台机器上的组件一起卸下

整备环境 SHALL 提供「一键卸下」，把选中机器上装着的**全部**组件卸下；
卸下的组件 SHALL 回到组件库成为可再安装的散件。
卸下的组件 SHALL NOT 连带影响载体本身（载体的完好度与身份不变）。

#### Scenario: 三类槽位一次清空

- **WHEN** 一台未部署机器在传感器、核心、执行器槽位上各装着一件，玩家提交一键卸下
- **THEN** 三个槽位 SHALL 全部变空
- **AND** 机器 SHALL 仍在其状态索引与消耗中保留（只是配置被清空）
- **AND** 容量 SHALL 回到载体基础容量，算力与逻辑容量 SHALL 为 0

#### Scenario: 组件回到组件库

- **WHEN** 一键卸下完成
- **THEN** 每一件被卸下的组件 SHALL 不再归属于任何机器
- **AND** 它们 SHALL NOT 被连带出售

### Requirement: 一键卸下必须是原子操作

一键卸下 SHALL 全部成功或全部不发生；SHALL NOT 出现「卸了一部分」的结果。

#### Scenario: 容量装不下时整体拒绝

- **WHEN** 容器中的占用超过「全部卸下之后」的剩余容量
- **THEN** 一键卸下 SHALL 被拒绝并说明容量仍在被使用
- **AND** 任何一件组件 SHALL NOT 被卸下

#### Scenario: 算力或逻辑仍被占用时整体拒绝

- **WHEN** 机器上仍有被占用的算力或逻辑容量
- **THEN** 一键卸下 SHALL 被拒绝
- **AND** 任何一件组件 SHALL NOT 被卸下

### Requirement: 一键卸下属于整备环境

一键卸下 SHALL 只在整备环境（未部署机器）成立；
已部署机器 SHALL 拒绝该动作（现场硬件修改逐项完成）。

#### Scenario: 已部署机器

- **WHEN** 对一台已部署机器提交一键卸下
- **THEN** SHALL 被拒绝并说明来源不符
- **AND** 它的组件 SHALL NOT 被卸下

#### Scenario: 中枢来源

- **WHEN** 以中枢来源提交一键卸下
- **THEN** SHALL 被拒绝（中枢不能远程停机改硬件）

#### Scenario: 整备页的入口可点性

- **WHEN** 选中的机器未部署且装着一件以上组件
- **THEN** 「一键卸下」SHALL 可用
- **WHEN** 机器已部署，或一件组件都没装
- **THEN** 「一键卸下」SHALL 不可用

### Requirement: 提交前必须说清会卸下什么、回到哪里

确认页 SHALL 在提交之前逐槽位列出将要卸下的每一件组件，并说明它们的去向；
SHALL NOT 只给一个件数汇总。

#### Scenario: 变更清单

- **WHEN** 打开一键卸下的确认页
- **THEN** 变更清单 SHALL 包含每一个将被清空的槽位及其中的组件实例
- **AND** SHALL 说明这些组件回到组件库
- **AND** SHALL 说明本次是整台清空而非某一格

#### Scenario: 没有可卸下的组件

- **WHEN** 机器上一件组件都没装
- **THEN** 提交 SHALL 被拒绝，SHALL NOT 报告成功

### Requirement: 未成功前组件归属不变

确认页提交之前、以及等待安全停机期间，组件归属 SHALL 保持不变；
关闭确认页 SHALL NOT 撤销已提交的等待，也 SHALL NOT 造成任何修改。

#### Scenario: 只确认不提交

- **WHEN** 打开确认页后直接关闭
- **THEN** 机器的槽位 SHALL 与打开之前完全一致

#### Scenario: 有进行中的行为

- **WHEN** 机器上有正在执行的行为
- **THEN** 提交 SHALL 先等待安全停机
- **AND** 在到达安全点之前 SHALL NOT 卸下任何组件
- **AND** 取消一个尚未提交的意图 SHALL NOT 修改任何硬件
