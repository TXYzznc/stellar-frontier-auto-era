# 安全快照与存读档入口

原始任务映射：P7-002、P7-003、P7-004、P7-011。本文件为增量合同；正式规则引用design.md，不覆盖未完成的原任务范围。

## ADDED Requirements

### Requirement: Safe snapshot preserves world state
保存 SHALL 在完整业务提交边界捕获不可变领域快照，包含恢复进行中机器与算法责任所需状态。

#### Scenario: Save during work
- **WHEN** 机器有部分完成行为、延迟事件和待应用草稿时请求保存
- **THEN** 保存完整阶段/已提交量/剩余量/版本/事件责任，恢复只继续未完成部分。

#### Scenario: Recover without startup
- **WHEN** 加载已运行算法的有效快照
- **THEN** 恢复原实例ID、计时和责任链，不重新发送Startup或重复任务结果。

### Requirement: Writes are revision safe
单槽写入 SHALL 串行合并请求且不能清除快照之后的新脏状态。

#### Scenario: World changes during write
- **WHEN** 旧快照在后台写入时发生新的合法业务变化
- **THEN** 旧写入成功只确认其修订，新变化仍等待后续保存。

#### Scenario: Interrupted file write
- **WHEN** 写临时文件或原子替换前发生中断
- **THEN** 最后有效主文件或备份仍可读，不把未校验文件变成正式档。

### Requirement: Restore validates before publishing
恢复 SHALL 在替换活动世界之前验证ID、引用与领域版本。

#### Scenario: Reference missing
- **WHEN** 快照指向已失效目标
- **THEN** 保留失效引用和原因，不按名称自动绑定别的对象。

#### Scenario: Unsupported snapshot
- **WHEN** 快照缺必需领域段或版本不兼容
- **THEN** 拒绝进入并说明原因，既有世界和文件不被空默认状态覆盖。

### Requirement: Menu and exit use real saves
新建、继续与保存退出 SHALL 使用真实世界进度流程和既有恢复确认规则。

#### Scenario: Save exit fails
- **WHEN** 玩家保存并退出时IO失败
- **THEN** 留在可处理状态，允许重试/返回游戏，强制退出需明确警告确认。

#### Scenario: Offline work pending
- **WHEN** 有效存档尚有离线结算进度
- **THEN** 进入结算流程，完成前不得开放世界操作或悄悄忽略离线时间。


