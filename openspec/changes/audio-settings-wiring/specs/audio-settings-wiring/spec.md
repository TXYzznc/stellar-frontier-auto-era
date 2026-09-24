# 声音设置：五路音量必须是可持久化、立即生效的真实后端

## ADDED Requirements

### Requirement: 五路音量总线

声音页 SHALL 提供五路音量：主音量、音乐、环境、机器与生产、UI 与警报；每路取值 0..1 并显示百分比。
主音量 SHALL 是**总控**（乘到其余每一路上），SHALL NOT 被实现为与其余各路并列的第六路。

#### Scenario: 主音量是总控

- **WHEN** 主音量设为 50%、音乐设为 50%
- **THEN** 音乐的最终生效音量 SHALL 是 25%
- **AND** 未单独调整过的路 SHALL 同样被主音量缩放

#### Scenario: 调主音量不改各路的原始值

- **WHEN** 音乐原始值为 50%，随后把主音量改为 50%
- **THEN** 音乐滑条上的原始值 SHALL 仍然是 50%

### Requirement: 原始值与最终生效值必须分开存放

滑条原始值 SHALL 存在独立键上；框架既有的分组音量键 SHALL 存**最终生效值**（主音量已乘进去）。

#### Scenario: 启动恢复不需要知道主音量

- **WHEN** 主音量 40%、机器 50% 被写入本机设置
- **THEN** 机器分组对应的框架键 SHALL 是 20%
- **AND** 框架层只需要「把每一行读回来」就能恢复正确音量

#### Scenario: 重新读取不得把主音量乘两遍

- **WHEN** 主音量 50%、音乐 60% 已保存，随后重新读取设置
- **THEN** 音乐的原始值 SHALL 仍是 60%
- **AND** 最终生效音量 SHALL 是 30%（而不是 15%）

### Requirement: 写入立即生效并落盘

拖动滑条 SHALL 立即改变真实音频分组的音量，并写入本机设置。
写入失败时 SHALL 保留内存中的值，并给出「重启后可能丢失」的说明。

#### Scenario: 拖动立即改变真实分组音量

- **WHEN** 把音乐滑条拖到 50%
- **THEN** 运行时音乐音频分组的音量 SHALL 变为 50%

#### Scenario: 落盘失败

- **WHEN** 本机设置写入失败
- **THEN** 界面 SHALL 显示新值
- **AND** SHALL 说明写入失败、重启后可能丢失
- **AND** SHALL NOT 静默回退到旧值

### Requirement: 音频分组是数据，新增分组不得改代码

音频分组 SHALL 来自 `SoundGroupTable`；界面引用的每一路分组 SHALL 在该表中存在。
启动时 SHALL 按表**逐行**恢复各分组的音量与静音，SHALL NOT 只恢复写死的两组。

#### Scenario: 表里新增分组

- **WHEN** 在 SoundGroupTable 中新增一行分组
- **THEN** `Const.SoundGroup` SHALL 自动包含它（生成物）
- **AND** 其音量与静音 SHALL 在启动时按本机设置恢复
- **AND** SHALL NOT 需要修改 `PreloadProcedure`

#### Scenario: 代码引用的分组缺失

- **WHEN** 表里缺少 `Const.SoundGroup` 中的某个值
- **THEN** 启动 SHALL 响亮失败（抛出带分组名的异常），SHALL NOT 静默跳过

### Requirement: 恢复本页默认只影响本页

`恢复本页默认` SHALL 只把五路音量恢复为默认，SHALL NOT 改动显示与性能分页的设置。

#### Scenario: 声音页恢复默认

- **WHEN** 显示页已把帧率上限改为 30，随后在声音页点「恢复本页默认」
- **THEN** 五路音量 SHALL 全部回到 100%
- **AND** 帧率上限 SHALL 仍是 30

### Requirement: 未接线的分页必须明说自己的缺口

没有存储载体时，声音页 SHALL 给出与显示页、操作页**各不相同**的可辨原因。

#### Scenario: 存储缺失

- **WHEN** 本机设置组件不可用
- **THEN** 三页 SHALL 各自报告不可用且原因互不相同
- **AND** 声音页的原因 SHALL 指到「五路音量没有可读写的存储载体」
- **AND** 操作页的原因 SHALL 指到「可持久化的输入绑定表」
