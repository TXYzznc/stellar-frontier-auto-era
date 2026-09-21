# 用户设置持久化

## ADDED Requirements

### Requirement: 设置读写必须经过可注入的存储边界

设置服务 SHALL 通过 `ISettingsStore` 读写本机设置，生产实现 SHALL 是框架 `SettingComponent`
的适配器；界面 SHALL NOT 直接访问框架全局组件。

落盘接口 SHALL 在注释与行为上承认：框架的 `Save()` 不返回结果，
因此只能把异常当作失败信号，不得声称已验证落盘。

#### Scenario: 存储可用时显示与性能分页就绪

- **WHEN** 注入一个可用的设置存储
- **THEN** 显示与性能分页 SHALL 为 `Ready`，且给出显示模式、垂直同步、帧率上限、画质、阴影、抗锯齿六类行

#### Scenario: 存储不可用时给可辨原因

- **WHEN** 设置存储不可用（框架尚未就绪）
- **THEN** 显示与性能分页 SHALL 为 `Unavailable` 并给出指向「设置组件未就绪」的原因

### Requirement: 三个分页的接线程度必须分别可辨

设置域 SHALL 用三个**互不相同**的原因表示各分页的缺口，SHALL NOT 用一句话覆盖整页。
显示与性能 SHALL 为 `Ready`；声音与操作在各自的前置（音频分组、可持久化输入绑定表）就位前
SHALL 为 `Unavailable`。

#### Scenario: 原因互不相同

- **WHEN** 读取设置快照
- **THEN** 三个分页的原因 SHALL 两两不同
- **AND** 声音页的原因 SHALL 指向音频分组，操作页的原因 SHALL 指向输入绑定

#### Scenario: 初始页落在已接线的分页

- **WHEN** 打开设置且调用方未指定分页
- **THEN** 界面 SHALL 落在第一个已接线的分页上

### Requirement: 垂直同步与帧率上限不得显示矛盾状态

帧率上限是否生效 SHALL 由服务统一判定并作为可展示文本给出：
垂直同步开启时上限不生效，界面 SHALL 说明它不会被使用。

#### Scenario: 开启垂直同步

- **WHEN** 垂直同步开启且帧率上限设为 30
- **THEN** 「上限是否生效」SHALL 说明上限不会被使用
- **AND** 关掉垂直同步后上限值 SHALL 仍然保留

### Requirement: 写入必须拒绝非法值并报告失败

写入 SHALL 只接受规格列出的取值；非法值 SHALL 返回失败并给出原因，且 SHALL NOT 改动已存值。
落盘失败时 SHALL 保留内存中的值并给出可展示原因（含「重启后可能丢失」）。

#### Scenario: 非法帧率上限

- **WHEN** 请求把帧率上限设为 45
- **THEN** 写入 SHALL 失败并给出原因
- **AND** 已存值 SHALL 不被改动

#### Scenario: 落盘失败

- **WHEN** 存储的落盘报失败
- **THEN** 写入 SHALL 返回失败并给出含「重启后可能丢失」的原因
- **AND** 读回的值 SHALL 是新值而不是旧值

### Requirement: 编辑器非播放模式不得把设置写回引擎

在编辑器非播放模式下，设置服务 SHALL 只落盘设置值，SHALL NOT 调用会改写工程资产的
`QualitySettings.SetQualityLevel`。

#### Scenario: 编辑模式下改画质

- **WHEN** 在非播放模式下请求切换画质档位
- **THEN** 设置值 SHALL 写入存储
- **AND** `QualitySettings.GetQualityLevel()` SHALL 保持不变

### Requirement: 恢复本页默认只影响本页

「恢复本页默认」SHALL 只写入显示与性能分页的键，SHALL NOT 改动其它分页的键。

#### Scenario: 恢复默认

- **WHEN** 显示与性能分页存在与默认值的差异，且存储里另有一个属于其它分页的键
- **THEN** 恢复后差异数 SHALL 为 0
- **AND** 其它分页的键 SHALL 保持不变
