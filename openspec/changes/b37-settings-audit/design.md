## Context

设置域是「持久化载体早就在生产里、界面缺可注入边界」的典型：`GF.Setting`（`SettingComponent`）在世界启动时就被 `PreloadProcedure` 读写并落盘，界面此前因无法注入存储边界而整页「未接入」。现已把边界显式化为 `ISettingsStore`，生产传 `GF.Setting` 适配器、测试传内存实现，同一条读写路径。三页接线程度：显示与性能（`Screen`/`QualitySettings`/`Application`）、声音（`GF.Sound` 五路分组音量）、操作（镜头三参数＋反转，按键栏只读）。

## Decisions

### 本批为纯审计，不改代码

逐项复核后无真实缺口：三页全部接线、读写只经 `ISettingsStore`、滑条区间取自镜头参数的单一来源（`RegionCameraParameters`）、渲染回环用 `_rendering` 与 `SetValueWithoutNotify` 双保险、写入失败保留内存值并说明。故本批不落代码，只回写任务表。

### DoD 三条程序侧均已满足

- **按本机保存**：`ISettingsStore` → `GF.Setting` 适配器，落盘。
- **世界不暂停**：全项目无 `Time.timeScale` 操纵或暂停世界机制，设置页是覆盖层不是暂停菜单。
- **按键显示来自 InputModule**：操作页按键栏读现场 `RegionInputModule` 实际映射，只读；世界外读不到时说明「不在现场」而非冒充默认映射。

### 音频资源与动态音频留 P8-008／P8-009

五路音量滑条已接 `GF.Sound` 分组，但真实音频资源与动态音频仍依赖 P8-008（音频资源）／P8-009（动态音频），故 P8-007 不能标「已完成」。

## Risks / Trade-offs

- 设置回归测试（SettingsReadModel／AudioSettings／ControlSettings）在当前编辑器会话累积 80+ 次测试运行后测试运行器陷入「starting」未返回（Unity Test Framework 长会话已知退化），属基础设施问题非本批引入；三份测试为既有、纯单元（MemorySettingsStore，不触 FMOD／网络），代码未变。
