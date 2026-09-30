## 1. 审计

- [x] 1.1 审计显示与性能页：窗口模式／垂直同步／帧率上限／基础画质全部接线 `Screen`／`QualitySettings`／`Application`，含恢复默认
- [x] 1.2 审计声音页：五路音量（主／音乐／环境／机器／UI）接线 `GF.Sound` 分组，主音量乘数语义正确，含恢复默认
- [x] 1.3 审计操作页：镜头三参数＋水平／垂直反转接线 `RegionCameraParameters`，按键栏只读且来源 InputModule 实际映射
- [x] 1.4 审计保存边界：读写只经 `ISettingsStore`（生产 `GF.Setting` 适配器、测试 `MemorySettingsStore`）
- [x] 1.5 审计 DoD：按本机保存（GF.Setting 落盘）、世界不暂停（无 Time.timeScale／暂停机制）、按键显示来自 InputModule——程序侧均满足

## 2. 回写

- [x] 2.1 `openspec validate --strict` 通过；任务表 P8-007 → 进行中（程序部分已交付，音频资源与动态音频待 P8-008／P8-009）

## 3. 说明

- 设置回归测试在当前编辑器会话测试运行器陷入「starting」未返回（长会话已知退化，非本批引入）；三份设置为既有纯单元测试、代码未变，不构成本批阻塞项。
