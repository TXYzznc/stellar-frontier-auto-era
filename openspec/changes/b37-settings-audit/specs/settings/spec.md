# settings

## MODIFIED Requirements

### Requirement: 第一版设置三页接线

SettingsForm MUST 提供声音（五路音量＋主音量乘数）、操作（镜头三参数＋水平／垂直反转＋只读按键）与显示与性能（窗口模式／垂直同步／帧率上限／基础画质）三页；读写只经 `ISettingsStore`（生产 `GF.Setting` 适配器、测试内存实现），修改立即生效并落盘本机，写入失败保留内存值并说明。

#### Scenario: 设置按本机保存

- **WHEN** 玩家修改任一设置
- **THEN** 值经 ISettingsStore 写入本机（GF.Setting 落盘），世界不暂停

#### Scenario: 按键显示来自现场输入模块

- **WHEN** 玩家在操作页查看按键
- **THEN** 显示现场 RegionInputModule 的实际映射（只读）；不在现场时说明读不到，而非冒充默认映射

#### Scenario: 主音量是乘数

- **WHEN** 主音量设为 50% 且音乐设为 50%
- **THEN** 音乐最终生效音量为 25%（主音量乘到每一路），滑条上的原始值保持 50%
