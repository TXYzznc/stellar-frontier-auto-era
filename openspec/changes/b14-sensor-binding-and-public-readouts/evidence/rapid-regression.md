# B14 冻结回归执行包

- 执行包：b14-sensor-final-regression
- 原负责人／缺陷接收：client，线程01a0319e-4025-7993-ab3e-0a1f1e5992c2。
- 目标：串行验证已冻结传感器实现及既有机器兼容；不修代码。
- 依据：本change specs、DEC-197与B14派发。
- 允许修改：仅本机队列自身状态；测试工具自己的临时结果。项目文件全部只读。
- 禁止：Git、xlsx、场景/Prefab保存、脚本、配置、其它Unity项目。
- 8090：客户端交出测试驱动权。先health/project核验主工程；Bypass，非PlayMode、普通编译完成，场景无未保存修改再测试。客户端不并行刷新/驱动。

## 串行测试

均使用 `test_run_by_name`、`testMode=EditMode`、完整前缀 `AutoEra.Tests.Editor.`：

1. MachineSensorEditModeTests（预期11个；少于11表明程序集旧，不能签收）。
2. SensorPublicDataEditModeTests（预期3个）。
3. MachineCatalogEditModeTests。
4. MachineSchedulingEditModeTests。
5. MachineManagementEditModeTests。
6. MachineExecutionContextEditModeTests。

每项完成后再启动下一项。记录逐项jobId、计数、失败详情。最后确认Console Error=0、普通编译无错误、非PlayMode。此包不进入PlayMode；客户端另有正式集成验证。

## 完成

直接回传客户端逐项结果和8090释放状态，再complete --claim-next。失败只回传，不编辑实现、不清理错误以伪造通过。不等待制作人已阅。
