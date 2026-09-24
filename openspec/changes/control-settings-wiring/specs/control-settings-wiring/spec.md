# 操作设置：镜头速度与反转必须是可持久化、真正生效的后端

## ADDED Requirements

### Requirement: 镜头速度可读写、可持久化

操作页 SHALL 提供平移速度、旋转速度／灵敏度、缩放速度三项可调参数；
读取值 SHALL 被夹到该参数自己的可用区间内；越界或 NaN 的写入 SHALL 被拒绝并给出原因。

#### Scenario: 越界写入被拒绝

- **WHEN** 把平移速度设为区间上界以外的值
- **THEN** 写入 SHALL 失败并说明「平移速度超出可用区间」
- **AND** 当前值 SHALL 保持原样（不得留下半份状态）

#### Scenario: 重新打开页面看到已保存的值

- **WHEN** 平移速度改为 22 后重新读取设置
- **THEN** 平移速度 SHALL 仍是 22

### Requirement: 区间只有一个来源

滑条区间、本机设置的夹取、镜头自身的取值 SHALL 来自同一处定义。

#### Scenario: 滑条能拖到上界

- **WHEN** 把平移速度设到区间上界
- **THEN** 该值 SHALL 被接受
- **AND** 镜头读到的 SHALL 就是该值（不得在更小处封顶）

### Requirement: 参数必须真正落到现场镜头上

保存的参数 SHALL 被应用到正在使用的镜头；反转 SHALL 由镜头自身消费，SHALL NOT 只停留在界面与存储里。

#### Scenario: 勾选反转后镜头行为改变

- **WHEN** 勾选水平反转
- **THEN** 现场镜头的水平反转 SHALL 为真
- **AND** 轨道旋转与平移的水平分量 SHALL 取反

#### Scenario: 世界外打开设置没有镜头

- **WHEN** 在没有现场镜头的情况下应用参数
- **THEN** SHALL 静默通过（不是错误）
- **AND** 参数 SHALL 已保存在本机设置中，待进入区域建立镜头时取用

### Requirement: 当前按键栏只读

操作页 SHALL 从现场输入模块读取当前按键映射并**只读显示**，SHALL NOT 提供改键入口，
SHALL NOT 存在可持久化的输入绑定表。

#### Scenario: 只读显示当前绑定

- **WHEN** 打开操作页
- **THEN** 移动／旋转／聚焦／返回／放置旋转 SHALL 显示现场输入模块的当前按键
- **AND** 缩放 SHALL 说明它是设备通道（鼠标滚轮）而非可改的按键

#### Scenario: 读不到现场输入模块

- **WHEN** 世界外打开设置，读不到现场输入模块的映射
- **THEN** SHALL 如实说明读不到
- **AND** SHALL NOT 用默认映射冒充现场绑定

#### Scenario: 不存在改键入口

- **WHEN** 检查操作设置的公开接口
- **THEN** SHALL NOT 存在任何绑定／重绑写入 API

### Requirement: 恢复本页默认只影响本页

`恢复本页默认` SHALL 只把三项镜头速度恢复为默认值、两项反转关闭，
SHALL NOT 改动显示与性能分页、声音分页的设置。

#### Scenario: 操作页恢复默认

- **WHEN** 显示页已把帧率上限改为 30、声音页已把音乐改为 30%，随后在操作页点「恢复本页默认」
- **THEN** 三项速度 SHALL 回到默认、反转 SHALL 关闭
- **AND** 帧率上限 SHALL 仍是 30，音乐 SHALL 仍是 30%

### Requirement: 写入失败不得静默

写入失败时 SHALL 保留内存中的值，并给出「重启后可能丢失」的说明。

#### Scenario: 落盘失败

- **WHEN** 本机设置写入失败
- **THEN** 界面 SHALL 显示新值
- **AND** SHALL 说明写入失败、重启后可能丢失
- **AND** SHALL NOT 静默回退到旧值
