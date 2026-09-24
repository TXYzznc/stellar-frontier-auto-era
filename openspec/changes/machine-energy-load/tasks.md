# 任务

## 1. 把已经存在的数据接通

- [x] 1.1 侦察结论：`MachineDefinitions`／`ComponentDefinitions` 早有 `IdlePower`／`WorkingPower`
      两列，`MachineCatalog.ValidateCommon` 也在校验它们（非负、有限、`working >= idle`），
      **只是构造运行时定义时没有传下去**。所以本批不改数据、只改解析。
- [x] 1.2 `MachineDefinition` 增加 `IdlePower`／`WorkingPower`（构造参数带默认值，
      既有调用点全部不受影响）；`ComponentDefinition` 同理。定义构造函数同样校验功率。
- [x] 1.3 `MachineCatalog` 把两列传进去。

## 2. 机器耗电：逐部件求和

- [x] 2.1 `MachineInstance.IsMoving` 与 `UpdateNavigationActivity(bool)`；
      `SetComponentActivity(componentId, active)` / `IsComponentActive`（每个效应器各自的执行状态）。
- [x] 2.2 `IdlePowerDraw`（全部部件按待机）与 `CurrentPowerDraw`（按各部件当前活动求和）：
      载体在移动时用稳定功率；传感器在未休眠且启用时持续采样用稳定功率；
      计算核心在有算力占用时用稳定功率；效应器在正在执行动作时用稳定功率；
      休眠时全部按待机；已损坏时为 0。
- [x] 2.3 `CurrentPowerDraw` **不看当前供电状态**：按「现在有没有电」算功率会让断电的机器
      请求 0 功率、于是永远拿不回供电（闭环死锁）。「断电机器能耗归零」是**实际消耗**，
      由电网结算时置 0。
- [x] 2.4 `MachineInstance.UpdateSupply(bool)`：只写供电，不碰区域信号
      （两者来自不同来源，用 `UpdateEnvironment` 会顺手覆盖信号，是一个看不出来的越权）。
- [x] 2.5 `MachineExecutionContext.Synchronize()` 推送移动状态与每个效应器绑定的执行状态。
      两处推送**刻意不发 `MachineInstance.Changed`**：它们是能耗模型按需读取的内部事实，
      广播只会让执行上下文自己（订阅了 Changed）与现场运行时、读模型白跑一遍。
- [x] 2.6 顺带修掉一条**测试预算**问题：`AlgorithmServicesIntegrationTests` 用 Debug.Log 打点实测，
      在长时间运行的编辑器上光「进入 Play Mode ＋ 等框架 preload 就绪」就要 175 秒，
      而 NUnit 默认上限是 180 秒——于是整条用例在自己的第一步就被掐掉，看起来像功能退化。
      给它显式 `[Timeout(600000)]`（真正的断言失败照常失败，放宽的只是等待环境就绪的容忍度）。

## 3. 电网契约的一处修正

- [x] 3.1 `IEnergyConsumer` 从「待机功率 ＋ 稳定功率 ＋ 是否在工作」改为**单一 `RequestedPower`**。
      理由：规格明确禁止「整机一个倍率」，而两个点表示不了「核心在跑算法、效应器待机、
      传感器持续采样」这种混合状态；旧契约会把实现推向规格禁止的做法。
- [x] 3.2 `EnergyGrid` 的负载与耗电都改用 `RequestedPower`；`EnergyConsumer` 保留两个字段并派生。
- [x] 3.3 `EnergyGridEditModeTests` 17/17 保持全绿（契约改了，用例只改了取功率的方式）。

## 4. 适配层

- [x] 4.1 新增 `MachineEnergyConsumer`：
      `IsDemandActive` ＝ 已部署 ＋ 现场电源开关未关；
      `RequestedPower` 直接取自 `MachineInstance.CurrentPowerDraw`，适配层只转交、不重新解释。
- [x] 4.2 `ApplySupply()` 把电网结论写回机器，**只在结论变化时**写并返回是否变化
      （机器上挂着一堆订阅者，无条件通知会把「供电没变」变成一次全量刷新）。
- [x] 4.3 `IdlePower` 暴露给界面用于显示「待机／工作」对比。

## 5. 验收

- [x] 5.1 编译 0 错。
- [x] 5.2 门1 契约自检全绿（**33/33**）；`run_project_checks.py` 5/5。
- [x] 5.3 新增 `MachineEnergyConsumerEditModeTests`（**17/17**），断言对齐设计里已经算过的数：
      休眠 1.9、核心＋两传感器＋效应器待机 8.2、执行动作 10.9；
      混合状态不是整机倍率；移动只换载体；休眠时全部待机；损坏为 0；
      功率来自数据表；停用的组件退回待机；库中机器与现场断电机器的请求为 0；
      **断电的机器仍然请求功率**（否则永远无法恢复）；回写不动区域信号；
      **缺电停机真的落到机器上**。
- [x] 5.4 回归：EditMode 全类、PlayMode **13 套**全绿；门1 33/33 持续绿。

## 6. 边界

本变更**不把电网建成区域服务、也不接进场景**：初始基地那台太阳能与生物质发电机目前还不是
世界对象，在它们存在之前把电网设成供电权威，会让所有机器一夜之间断电——
那会是一次「为了让新系统上线而先破坏现有玩法」的改动。

不实现建筑的耗电（建筑域还没有运行时对象）、不做能源界面、不做离线推进的能源事件。

## 7. 一条复用的侦察手法

「任务名当清单」之外又验证了一条更省事的判据：**先看数据表里有没有这一列**。
这一批原本按「要先给数据表加功率列」估算，侦察后发现列早就有、校验也早就有，
缺口只在最后一步没有传递——于是整批从「数据变更 ＋ 解析 ＋ 适配」缩成「解析 ＋ 适配」，
省掉了整条受控数据通路（改 JSON → 1012 → 1014 → 1001）。
