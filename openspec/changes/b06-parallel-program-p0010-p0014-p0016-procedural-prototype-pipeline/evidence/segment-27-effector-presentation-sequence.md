# 第 4.4 段：效应器表现序列

日期：2026-09-02

- `EffectorPresentationSequence` 以显式、不可跳跃的表现阶段处理对位、接入、锁定、解锁与脱离；非法阶段迁移保持原状态。
- 取消仅允许在对位／接入期间发生；断电进入安全保持，供电恢复后须经过恢复阶段才能回到可对位状态。
- QA job `520d7bc0`：`EffectorPresentationSequenceEditModeTests` / EditMode 3/3 通过，失败 0；作业中的短暂服务重连已自行恢复，结束时 Unity 非 PlayMode、未编译。
