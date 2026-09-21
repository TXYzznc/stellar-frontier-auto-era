# 能源离散事件与停机记录（让能源历史真的有记录）

## Why

能源域在批次 15／16 之后已经活着：区域自己持有电网，机器真的会因为没电停下来，中枢能源页也能
把供需读出来。但**这些状态跨越一件都没被记下来**：

- `EventDomain` 只有任务／算法／执行／资源／警报，**没有能源域**；
- 于是 `RecordReaderForm` 的能源停机记录页永远空着，页面里那句「能源历史暂不可用：事件分类里
  没有能源域」在能源系统接线之后就变成了**过时的谎话**；
- 中枢能源页的「查看能源事件」（`Btn_HubEnergyHistory`）没有入口，是一颗死按钮。

规格 06「第一版能源界面」写得很清楚：「第一版不制作连续功率曲线。能源历史只记录电量过低／耗尽、
缺电停机／恢复、燃料耗尽和负载首次超过供给等离散事件。」所以这一批要做的不是采样，而是
**把连续结算变成离散的状态跨越，并让它们出现在记录页上**。

## What Changes

### 能源域与离散事件识别

- `EventDomain` 追加 `Energy = 5`（**追加在末尾**：这些取值会写进日志记录并可能落盘，
  改既有取值会让旧存档里的记录改变含义）。
- 新增 `EnergyEventRecorder`（纯领域对象，不依赖 Unity、不写日志）：每次结算后看一遍快照与设施状态，
  只在**状态跨越**时产出一条事件。它刻意不放进 `EnergyGrid`——结算每帧都跑，记账不该成为结算的一部分。
- 事件种类：`缺电停机`／`缺电恢复`（被停机集合的进出）、`电量耗尽`／`电量恢复`（储能 0 与非 0 之间）、
  `燃料耗尽`（燃料设施的可燃生物质归零）、`负载首次超过供给`（`HasShortfall` 由假变真）。
- **`电量过低` 本批不实现**：文档没有给出「过低」的判据（没有百分比阈值，也没有续航口径），
  凭空定一个数字等于实现侧自造规则。「电量耗尽」（＝ 0）是无歧义的，先做它。
- `EnergyEventText`：日志动作名、界面标签、原因与影响的**唯一词汇表**。生产侧与读取侧共用它，
  因为日志记录里没有「事件种类」字段——读取侧必须靠动作名认回种类才能算出「活跃／已恢复」与「持续时长」。
  两边各写一份字面量，改一处就会让配对悄悄失效。

### 记账

- `AutoEraEventService` 新增 `PublishNotice(domain, source, action)`：记录**自发事实**——
  没有命令触发、没有关联链（电网的停机不是某条命令的结果，硬造一个关联 Id 会污染追溯链），
  也不经过事件总线（第一版没有订阅者）。
- `InitialRegionScene.AdvanceEnergy` 的顺序固定为「先对账 → 再结算 → 再落结论 → 再记账」：
  记录描述的是这一次结算的结论。没有事件服务的世界（单元测试、离线推演）照常推进电网，只是不记账。

### 记录页与入口

- `EventReadModel`：能源行只取 `EventDomain.Energy`（资源域不得冒充能源事件）；列表行补上对象名；
  事件详情追加规格要求的 对象／状态（活跃或已恢复）／持续时长／原因／影响——状态与时长靠
  **按（主体，事件种类）配对停供记录与它后面第一条恢复记录**得出。
- `RecordReaderForm`：能源历史页的空态说明从「没有数据来源」改成「这个区域还没有能源事件」。
- 中枢能源页新增 `Btn_HubEnergyHistory` 契约绑定与接线：进入 `RecordReaderForm` 的能源停机记录页。

## Capabilities

### New Capabilities

- `energy-discrete-events`：能源离散事件的识别、记账、读取与入口。

## Impact

- 产品层：`Events/EventClassification.cs`、`Events/AutoEraEventService.cs`（`PublishNotice`）、
  `Energy/EnergyEventRecorder.cs`（新）、`World/Region/InitialRegionScene.cs`、
  `UI/Integration/EventReadModel.cs`、`UI/Integration/AutoEraUiFormat.cs`（`Duration`）、
  `UI/RecordReaderForm.cs`、`UI/BaseCommandHubForm.cs`（入口接线）。
- 契约与资产：`Tools/ui_spec_to_contract.py` 的 `EXTRA_BINDINGS` 加 `_hubEnergyHistoryButton`；
  重新生成契约、字段脚本与预制体绑定（**结构不变**）。
- 验收：门1 必须持续全绿（33 契约）；`run_project_checks.py` 5/5；
  新增 `EnergyEventRecorderEditModeTests` **13/13**；`EventReadModelEditModeTests` 6 → **12/12**；
  `RegionEnergyPlayModeTests` 扩到第 ⑤ 步（真实缺电 → 事件入库 → 中枢入口 → 记录页读到真实行）。
- **不在本变更内**：`电量过低`（缺设计判据）、`Btn_HubEnergyLocate`（要的是尚未接入的跨对象定位通道）、
  警报域（`AlertForm` 仍是「未接线」页面）、能源事件的存档落盘与离线重放。
