# 算法正式运行驱动

原始任务映射：P3-003、P3-005、P3-007、P3-008。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Production host drives runtime
正式区域宿主 SHALL 独立于UI驱动机器实例服务和命令适配器，并保持导航单一推进者。

#### Scenario: Close observer while running
- **WHEN** 玩家应用一个合法算法后关闭所有管理Form
- **THEN** 后续世界步仍产生真实任务/导航结果，且测试没有直接调用实例或适配器Pump。

#### Scenario: Stable iteration during removal
- **WHEN** 某机器的回调请求撤收另一台机器
- **THEN** 本步不抛集合修改异常、不重复推进，撤收对象在约定边界释放。

### Requirement: Activation is atomic
首次激活 SHALL 原子建立实例、适配器归属与容量占用，失败不得留下部分挂接。

#### Scenario: Activation rejected
- **WHEN** 提交前硬件修订或逻辑容量发生变化
- **THEN** 申请带原因失败，旧状态不变，临时对象与租约清零，修正条件后可重新激活。

#### Scenario: Startup is not duplicated
- **WHEN** 首次激活成功并经历多个世界步或UI重新打开
- **THEN** Startup恰好提交一次；恢复快照不会再提交一次。

### Requirement: Apply respects safe points
草稿应用 SHALL 使用现有修订检查和安全点规则，关闭UI不能取消已接受申请。

#### Scenario: Invalid edit preserves running plan
- **WHEN** 运行中算法收到不兼容绑定或容量超限的草稿
- **THEN** 旧版本继续按原规则运行，申请显式拒绝。

#### Scenario: Independent pause reasons
- **WHEN** 断电发生在结构应用暂停期间，随后应用完成但尚未恢复供电
- **THEN** 机器保持因断电暂停；仅供电恢复且无其他暂停原因后继续。

### Requirement: Lifecycle releases owned work
实例与机器释放 SHALL 清理其订阅、事件、任务归属与算力，且不取消其他实例的合法工作。

#### Scenario: Machine leaves region
- **WHEN** 带有等待任务和应用请求的机器被撤收或世界退出
- **THEN** 迟到回调被生命周期版本拒绝，无残留租约或对已释放对象的调用。


