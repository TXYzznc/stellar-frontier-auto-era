# 启动失败、重启与分组降级证据

2026-09-10，B10基础补齐。范围沿用客户端派发单，不修改正式场景制造故障。

## 已取得的独立结果

QA job `3d2de294`：AutoEraStartupFlowEditModeTests 5/5通过，普通Console Error=0。

- 真实GF场景加载尚未完成时取消：旧回调0次，新请求完成1次，旧场景卸载。
- 无效世界场景：返回现有菜单并保留可定位失败信息，恢复配置后可进入。
- Launch出发连续两轮进入/返回：实体释放、重复点击只接受一次；实际GF中枢UI打开期间世界时间继续推进。
- 真实加载中调用GameEntry.Shutdown(Restart)：旧会话释放，重启菜单可再次进入。XML用例耗时8.284579秒。
- 半初始化缺失种子：Region清理、注册表清空、事件订阅恢复原计数，新会话可创建。XML用例耗时6.801066秒。

以上结果对应此前五用例程序集，不代表新增声音用例或正式数据加载验证已经通过。

## 本轮新增验证

请求不存在的测试音频，只接收本次userData匹配的PlaySoundFailure，检查资源路径、Sound组和错误描述；其他既有声音组仍存在，随后进入世界并检查实体/HUD及真实GF三类Foundation数据。

该负例不产生音频文件、不新增分组、不忽略全部日志。预期缺失资源诊断与恢复后普通Console分开记录。未覆盖真实声卡、扬声器及所有设备切换场景，不将资源失败验证等同硬件完整链验收。

本轮队列：`b10-foundation-final-regression`，QA已完成并释放8090。

- Startup Flow `53004793`：进入PlayMode后的权威XML 6/6通过，包含上述五项及声音负例；真实StartupProcedure同时验证GF已加载的Foundation DataTable/Config/Language。
- 普通八组52/52：DataTable `2c020366`31/31；Runtime `d28bf960`4/4；PersistentId `38506855`4/4；Registry `878a3f11`3/3；Reference `904ad607`3/3；UtcTime `845d8c96`3/3；WorldSession `aaed9584`2/2；ApplicationContext `c911a531`2/2。
- XML记录Sound组对指定缺失资源一次NotExist诊断，其他组继续；恢复后普通Console Error=0、Warning=0，非PlayMode、非编译。
- 后续仅DataTable文本类型校验补丁另跑32项，不将本轮旧程序集结果冒用为新类型门禁证据。
