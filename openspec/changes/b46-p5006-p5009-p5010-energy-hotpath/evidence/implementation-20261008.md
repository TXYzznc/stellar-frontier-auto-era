# B46 实施与验证（2026-10-08）

本批9项增量实现完成。无完整G5或B50联合性能门签收，无Git提交/暂存、xlsx写入或自动归档。

## 实现

- Plan改值类型；每Tick只读取一次各机器按部件求和的请求功率。仍按原登记顺序累加浮点数，保留缺口决策的计算顺序，不用减法近似替换。
- 参与者/优先级/QueueOrder变化重建缓存排序；稳定缺电使用游标选择下一停机对象。相同优先级及队列值继续按原登记先后决策。参与者集合只读，重复ID拒绝。
- 停机名单在顺序集合不变时复用不可变视图，变化时才创建新副本；旧快照不被工作缓冲改写。Grid修订仅在可观察结算变化时增加。
- RegionEnergyService首次对账后订阅机器部署/撤收/配置恢复与花名册释放，不逐帧扫描。显式force仍可完整对账；切换世界时移除旧负载并退订，原机器队列顺序不重排。区域Release调用Dispose。
- 能源读模型按电网/花名册修订、活动状态与燃料/策略输入判断变化；无变化Refresh不分配或通知。机器无广播的活动状态通过独立修订读取，优先级展示读取实际值；UI查询不推进电量。

## 正确性与门禁

8个测试组90/90，逐组JSON与同次Unity原生XML在本目录；优化前5组62/62保存为-baseline。新增随机等价系列覆盖0/1/10/50/100负载、5固定种子×80步、零/小/大步长、日照、燃料耗尽/补充、储能与动态优先级，数值容差1e-5，停机ID顺序/供电标记精确比较。单独覆盖删增参与者、同QueueOrder、历史快照、稳定缺电/一次排序变化后零分配，以及恢复/换世界/释放退订。

EnergyReadModel专项16/16包含稳定Refresh的阳性校准后零分配和无额外结算。正式RegionEnergyPlayModeTests1/1从Launch/InitialRegion读取真实设施、部署机器、区域驱动与能源UI，断言运行分辨率1920×1080；无Prefab布局更改或其它分辨率运行门。

主工程8091普通编译0错误；健康检查5/5（资源校验validate-data-tables-json_20261008_140458.json，12/12），框架纯度/项目边界、diff检查和OpenSpec strict通过。输出历史标签8092不代表实际连接端口。

## 独立Development Player

Unity2022.3.62f3c1 / Mono2x / Development / Windows64，Windows10，Intel Core Ultra7 265KF（20逻辑核）。构建0错误0警告。Temp隔离工程使用生产EnergyGrid、MachineEnergyConsumer、MachineInstance及最小定义闭包；显式相同的合成负载配置，未改主工程设置或装配依赖。构建/源码SHA256/原生运行JSON均在本目录，参考实现冻结在测试侧，生产不调用它。

每组预热512 Tick，采样6000 Tick，3轮交换前后执行顺序；标记AutoEra.Energy.Tick/ReferenceTick内包含结算及供电回写。下表为3轮分位数的中位数，时间单位微秒；末列是校准GC.Alloc事件数/Tick。

| 机器 | 状态 | 优化前p50 / p95 / p99 | 优化后p50 / p95 / p99 | p50改善 | 分配事件/Tick |
|---:|---|---:|---:|---:|---:|
| 10 | normal | 0.80 / 0.90 / 1.10 | 0.70 / 0.80 / 1.00 | 12.5% | 1 → 0 |
| 10 | shortage | 3.50 / 3.90 / 9.70 | 1.70 / 1.90 / 2.10 | 51.4% | 12 → 0 |
| 10 | recovery | 0.80 / 0.90 / 1.00 | 0.70 / 0.80 / 0.80 | 12.5% | 1 → 0 |
| 50 | normal | 3.10 / 3.40 / 4.10 | 2.90 / 3.10 / 3.20 | 6.5% | 1 → 0 |
| 50 | shortage | 50.00 / 63.10 / 192.00 | 15.30 / 16.90 / 20.40 | 69.4% | 52 → 0 |
| 50 | recovery | 3.10 / 4.00 / 5.40 | 2.90 / 3.10 / 3.40 | 6.5% | 1 → 0 |
| 100 | normal | 6.00 / 6.70 / 7.50 | 5.60 / 6.30 / 7.20 | 6.7% | 1 → 0 |
| 100 | shortage | 188.30 / 310.00 / 388.80 | 48.00 / 53.20 / 66.10 | 74.5% | 102 → 0 |
| 100 | recovery | 5.90 / 6.50 / 7.20 | 5.60 / 6.00 / 6.80 | 5.1% | 1 → 0 |

全部27组稳定生产采样均为0分配事件。使用Unity2022.3官方ProfilerRecorder的GC.Alloc当前线程计数，并先以1MiB已知分配校准为阳性。此Marker的Sample.Value是计时原值，不当成分配字节；正分配组allocationBytes=-1表示字节数未测，零事件组为0。最初GetAllocatedBytesForCurrentThread返回恒零的结果保存为before-uncalibrated，不能证明零分配。参考组每Tick正常/恢复1次分配、全缺电N+2次分配，可交叉验证计数。

参考API：[Unity2022.3 CollectOnlyOnCurrentThread](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorderOptions.CollectOnlyOnCurrentThread.html)。不是完整产品FPS/GPU/内存或50台60分钟稳定性结果；这些门保留在B50。

复现：python tools/benchmarks/energy_player_benchmark.py --unity <2022.3.62f3c1 Unity.exe> --label before / after。before使用冻结参考方法体，不要求回退当前生产文件；输出保留全部54条测量含各轮采样时长。快照/排序/计划可分单元回退，无存档格式变化。

## Scenario索引

| 场景 | 证据 |
|---|---|
| Shortage priority / Daylight and fuel transitions | SeededSeries五组、ParticipantAndPriorityChanges；既有EnergyGrid/MachineEnergyConsumer/RegionEnergyPlayMode |
| Stable shortage tick / Participant changed | StableShortage；校准后独立Player27组零分配；区域Lifecycle/Restore/SwitchingRoster |
| Keep old snapshot | PublishedStops不可变与复用测试 |
| Compare implementations | energy-player-before/after.json、performance-comparison.json、Player build/source JSON |
