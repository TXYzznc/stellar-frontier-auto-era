# 存档：三份滚动备份、内容校验与不静默回退

## ADDED Requirements

### Requirement: 每个槽位保留三份滚动备份

每个进度槽 SHALL 保存 1 份正式存档与最近 3 份滚动备份；备份 SHALL 只在损坏恢复时使用，
SHALL NOT 作为普通读取项显示。删除进度时 SHALL 同时删除正式存档与全部备份。

#### Scenario: 连续保存只保留最近三份

- **WHEN** 同一个槽位连续保存 5 次
- **THEN** 正式存档 SHALL 是第 5 版
- **AND** 三份备份 SHALL 分别是第 4、3、2 版（序号 1 最新）
- **AND** 第 1 版 SHALL 已被滚动丢弃

#### Scenario: 第一次保存不产生备份

- **WHEN** 在一个空槽位第一次保存
- **THEN** SHALL NOT 产生备份文件

#### Scenario: 删除进度

- **WHEN** 删除一个槽位
- **THEN** 正式存档、临时文件与三份备份 SHALL 全部被删除

### Requirement: 每份存档带校验值与身份信息

每份存档 SHALL 包含格式版本、槽位ID、保存UTC时间、数据长度、校验值与离线结算进度标记。
校验 SHALL 覆盖内容；内容被改动或长度对不上时 SHALL 判定为损坏。

#### Scenario: 内容被手改

- **WHEN** 正式存档的内容被手工修改而校验值未变
- **THEN** 读取 SHALL 判定为损坏
- **AND** SHALL NOT 把它当作有效存档返回

#### Scenario: 长度字段与内容不一致

- **WHEN** 记录的数据长度与内容实际长度不一致
- **THEN** 读取 SHALL 判定为损坏

#### Scenario: 文件属于别的槽位

- **WHEN** 把某个槽位的存档文件复制到另一个槽位
- **THEN** 读取该槽位 SHALL 判定为损坏

#### Scenario: 旧格式存档

- **WHEN** 读取一个没有校验值的旧格式存档
- **THEN** SHALL 按迁移入口接受并正常读取
- **AND** SHALL NOT 因为格式升级而判为损坏

### Requirement: 使用备份时必须告知，不得静默回退

正式存档不可读时 SHALL 依次尝试三份备份（由新到旧）；
用备份读取时 SHALL 以**独立状态**报告，并给出用的是哪一份与它的保存时间。

#### Scenario: 回退读备份

- **WHEN** 正式存档损坏而某一份备份可读
- **THEN** 读取结果 SHALL 标记为「已从备份读取」并带备份序号
- **AND** 界面 SHALL NOT 把它显示成「正常」
- **AND** SHALL 给出该备份的保存时间

#### Scenario: 跳过不可用的备份

- **WHEN** 最新一份备份不可读而更旧的一份可读
- **THEN** SHALL 使用更旧的那一份
- **AND** SHALL 报告实际使用的是哪一份

#### Scenario: 正式存档缺失但备份存在

- **WHEN** 正式存档文件不存在而备份可读
- **THEN** 结果 SHALL 是可恢复，而 SHALL NOT 报告为空槽位

### Requirement: 恢复使用最新可用备份

「恢复」SHALL 只在存在**可用**备份时可行；恢复 SHALL 把最新一份可用备份提升为正式存档。

#### Scenario: 最新备份不可用

- **WHEN** 最新一份备份不可读，第二份可读
- **THEN** 「恢复」SHALL 仍然可行
- **AND** 提升的 SHALL 是第二份
- **AND** 不可读的那一份 SHALL 从备份链上移除
- **AND** 更旧的备份 SHALL 依次前移，序号 1 仍为最新

#### Scenario: 没有可用备份

- **WHEN** 三份备份都不可读
- **THEN** 恢复 SHALL 失败
- **AND** 正式存档文件 SHALL 保持原样

#### Scenario: 恢复说明

- **WHEN** 打开损坏存档的恢复界面
- **THEN** SHALL 说明正式存档无法读取
- **AND** SHALL 给出可恢复到的备份序号与时间点
- **AND** SHALL 给出预计损失范围

### Requirement: 版本过新是确定结论

存档格式版本高于当前构建时，读取 SHALL 报告版本过新，SHALL NOT 用备份替代。

#### Scenario: 版本过新

- **WHEN** 正式存档由更新的版本写入
- **THEN** 读取 SHALL 报告版本过新
- **AND** SHALL NOT 回退到某份旧备份并报告成功
