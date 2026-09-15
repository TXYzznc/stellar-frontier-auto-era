# B14 客户端实施证据

日期：2026-09-11。状态：授权基础服务与正式配置实施/验证完成，已通过窗口消息实际回传制作人；不代表农业/林业生产已实现。

## 实现边界

- 纯C#单目标传感器，共享机器Compute Sampling租约；世界毫秒驱动、不追赶历史采样。
- Profile按组件ModelId＋Level匹配，配置表按稳定行ID检索；避免把21012误当ComponentDefinition.Id（实际为2101）。
- 对象/土壤两类、各等级1000ms、6/4m、10算力。合法锚点到提供者公开区域最近点。
- 停止、禁用、休眠、断电、卸载、缺目标/提供者释放；超距持续租约；边界让出、单等待、回调换绑/释放门禁。
- RegionSensorReadProvider仅提供RegionObject实际公开状态。只读土壤/作物格网合同有测试夹具；未创建农业/林业生产者、假格网、真实扫描或通信。
- InitialRegionScene显式AttachSensors/Advance/Release；未自动安装传感器或修改场景/Prefab。

## 配置与受控生成

使用新SensorDataSetup菜单，仅调用既有ImportDataTablesFromAIJson：false/false校验，再true/true受控Reverse/生成。没有AI直接写xlsx、没有手改生成C#、没有全表Reverse。工具事务暂存在Temp并自行清理；菜单直接返回报告，不调用全局WriteReport。AppConfigs仅幂等追加Sensors/SensorDefinitions。

SensorDefinitions四行21011/21012/21021/21022。逻辑指纹：`F832310BCF88020A3C7CD5A540D704473F6C5E5856D0B8B59BBF343649873425`。

ComponentDefinitions保持22行。与本轮修改前读取快照逐字段比较：除授权的六个功率字段及四条L2注释外，所有values不变；等级/价格/资源路径与PendingResource状态保留。

| 行ID | IdlePower变化 | WorkingPower变化 |
|---|---|---|
|21012|0.2不变|2.4→2|
|21022|0.2不变|2.4→2|
|21032|0.6→0.5|6→5|
|21042|0.4→0.3|3.6→3|

ComponentDefinitions逻辑指纹：`144CC3D6C817EA7AAC30E27C10283753C2A7F087BE3FC6D84CE48F5278ED9666`→`2D6534B787290BE428423E1F0566C1D06E06B56674CC279BD55E9CC72686B273`。重复生成已执行且无报错，AppConfigs登记恰一次。最终重复生成前后，两份txt、两份生成C#和AppConfigs五文件SHA-256全部一致；不以工作簿ZIP时间戳作为逻辑幂等判据。

## 已取得验证（非最终全量结果）

- MachineSensorEditModeTests：job `4adf4c79`，9/9；后来补入两个边界测试，最终应为11个。
- MachineSensorIntegrationTests：job `9e908736`因Enter/ExitPlayMode域重载不再可轮询；Unity原生TestResults.xml明确方法`LoadedConfiguration_AndFormalRegionLifecycle`，2026-09-11 06:34:37Z–06:34:40Z，1/1 Passed。
- 集成实际从GF加载新表与四类两级功率，验证4个Profile、扫描/通信不启用，正式InitialRegion runtime初始化、显式锚点绑定、10算力及Release清空。
- 曾因新集成测试直接GF.DataTable导致测试程序集引用错误，改用已有GameEntry/DataTableComponent公共接口，未改asmdef。此前`270222e2`和`8060287d`的7/7属于旧程序集，不用于本次验收。
- strict通过；`audit_framework_purity.py --product-profile tools/audit_product_profile.json`通过；`audit_project_boundaries.py`通过。默认严格框架模式报告9条既有产品入口/场景问题，必须使用已批准产品profile，不假称默认模式通过。
- 快速执行六组回归见rapid-regression.md，已回传并经客户端复核；以下最终结果覆盖后续精修与计数更正。

## 最终结果

| 测试 | jobId | 通过数 |
|---|---|---|
|MachineSensorEditModeTests|60e96a7b|13/13|
|SensorPublicDataEditModeTests|7c6f50bc|3/3|
|MachineCatalogEditModeTests|42a3b4d3|16/16|
|MachineSchedulingEditModeTests|1ec6a6df|3/3|
|MachineManagementEditModeTests|12ceb8a8|4/4|
|MachineExecutionContextEditModeTests|22422145|3/3|
|MachineSensorIntegrationTests|49ac159b；原生XML|1/1|

共42个EditMode断言用例及1个跨PlayMode实际集成用例通过；无失败/跳过。管理测试快速执行初始回传e7d52a11为3/3，与源码4个Test不符，客户端重跑12ceb8a8并逐个核对原生XML方法名，以4/4替换，不计原结果。

最终集成原生XML：2026-09-11 06:44:18Z–06:44:20Z，`AutoEra.Tests.Editor.MachineSensorIntegrationTests.LoadedConfiguration_AndFormalRegionLifecycle`，Passed 1/1；域重载后的旧job内存不作为唯一证据。

额外边界：对象重复发送同值Changed不增加公开读数版本；集合稳定顺序缓存避免每帧树形集合枚举分配，预热后1000次非采样边界Tick实测0字节分配。该测量不宣称真实农田大格网性能。

最终Unity2022.3.62f3c1非PlayMode/非编译/非暂停，Launch isDirty=false；Console Error=0；脚本编译反馈0错误；Launch Missing Script/Prefab检查0问题。没有改写或保存正式场景。

strict、产品profile纯度、项目边界审计均再次通过；精确候选路径见candidate-paths.md。8090测试驱动释放；Git未触发。

## 保护与占用

没有Git索引操作、提交或自动Git请求。开发任务表仅SHA-256只读核验：`DFBE954F3907E965B25EEE5C769413C6B5176483D1982C582FD318E57068EEB5`。当前表允许工具生成的两张game-data xlsx与任务表不同，不扩大其它工作簿授权。

工作区含大量既有未提交内容；候选清单仅界定本轮授权路径，不代表其中所有既有差异都属于本轮。B11只待完整UI素材/Prefab交付。
