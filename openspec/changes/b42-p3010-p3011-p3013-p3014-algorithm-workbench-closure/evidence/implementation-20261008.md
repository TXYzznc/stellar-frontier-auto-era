# B42 工作台实施与验收（2026-10-08）

本change **9/9任务完成**；相关测试 **102/102**，项目健康 **5/5**，OpenSpec strict与diff检查通过。正式运行验收为Unity2022.3.62f3c1 Editor，主工程REST8091，Windows10；未据此宣称整个G3或Player性能验收通过。

## 交付

- 首次图缩放使用节点实际尺寸，不再用0.68下限裁掉高节点；宽300，高max(100,58+最大端口数×28)。端口行宽高由父组控制，内容Fitter仅控制自身高度；socket与曲线端点一致为±12。
- Form根/GF缩放保持统一，工作台Frame/PageHost和中部面板使用语义锚点。工具条按钮按32高布局，不再溢出；诊断隐藏草稿工具。独立Item结构、手工视觉资源与业务图坐标保留。
- NodeComponentPicker复用现有列表分两步选择已安装组件与支持的真实区域目标；生产入口取得代次，不存在的身份被拒绝，暂缺目标明确提示。
- 运行记录共享不可变执行计划，读时提供深拷贝文档/输入/节点值。诊断按稳定RunId选旧记录并显示其r#、来源、代次和真实任务结果；当前草稿修订不同则不冒充已执行。节点接入绿色执行/红色失败提示，池化Bind重置状态。
- 编辑视图端口、诊断展示、玩家原因码分别局部拆出；命令经B40宿主处理。无效草稿创建Rejected请求并保留旧配置；可重试且清除旧操作错误。运行通知由服务转发，关闭UI不释放宿主实例/请求。
- 权威13-算法工作台prefab-layout.md同步本Form局部差异；通用转换器读取显式覆盖块，无业务硬编码，不改变其他Form共享外壳。契约→历史独立Item/视觉接线迁移→布局迁移是当前复现链，未声称基础生成器能复刻历史人工视觉加工。

## 场景证据

| spec Scenario | 验证 | 结论 |
|---|---|---|
| Reference resolution review | 正式宿主工作台测试断言Screen和PNG均1920×1080，六节点实际世界边界在视口内；人工检查空图/复杂图/任务诊断截图 | 通过；滚动列表的视口裁切是正常列表行为，未新增其他分辨率运行验收 |
| Layout structure review | Bindings测试5/5，14端口无内嵌滚动/裁切、单轴尺寸所有权、根Stretch/无额外CanvasScaler；layout-before/after和迁移SHA256幂等检查 | 通过，两Prefab再次迁移hash均不变 |
| Invalid connection or apply | InvalidDraftApply_PreservesRunningRevisionAndReportsReadableReason + 原画布强类型/占用/取消交互 | 通过，旧r1保留且问题说明可读 |
| Close after applying | 正式UI首次应用后关闭，导航由entry.Advance驱动；再打开编辑后提交WaitingSafePoint并关闭，由宿主完成Succeeded | 通过，无手工Pump或伪造完成回调 |
| Inspect older run | DiagnosticRecord_RetainsExecutedRevisionAndValues_WhenDraftChanges + 正式UI历史按钮选任务#完成，节点诊断已接到视觉，草稿r4/运行与记录r3 | 通过，记录文档拷贝不能改历史，旧值不误贴当前草稿 |
| Run template end to end | AlgorithmWorkbenchProductionPlayModeTests：真实模板库创建按钮→绑定页→组件/对象选择→应用→真实导航→任务历史/输入诊断 | 通过；玩家模板与初始机器/目标属于测试输入准备，运行服务/传感器/任务均由正式生产入口创建 |

## 原生结果

下表每组都有同次返回JSON及Unity原生XML；Editor目录的EnterPlayMode测试按EditMode运行，原Canvas测试按PlayMode运行。新增后重新发现测试，避免缓存遗漏重命名或新增用例。

| 测试组 | 通过 | 原生XML |
|---|---|---|
| AlgorithmReadModelEditModeTests | 34/34 | [AlgorithmReadModelEditModeTests.xml](AlgorithmReadModelEditModeTests.xml) |
| AlgorithmEditorFormBindingsEditModeTests | 5/5 | [AlgorithmEditorFormBindingsEditModeTests.xml](AlgorithmEditorFormBindingsEditModeTests.xml) |
| AlgorithmInstanceEditModeTests | 13/13 | [AlgorithmInstanceEditModeTests.xml](AlgorithmInstanceEditModeTests.xml) |
| AlgorithmExecutionEditModeTests | 8/8 | [AlgorithmExecutionEditModeTests.xml](AlgorithmExecutionEditModeTests.xml) |
| AlgorithmServicesIntegrationTests | 1/1 | [AlgorithmServicesIntegrationTests.xml](AlgorithmServicesIntegrationTests.xml) |
| ProductionAlgorithmDriveEditModeTests | 8/8 | [ProductionAlgorithmDriveEditModeTests.xml](ProductionAlgorithmDriveEditModeTests.xml) |
| HardwareRuntimeWiringEditModeTests | 10/10 | [HardwareRuntimeWiringEditModeTests.xml](HardwareRuntimeWiringEditModeTests.xml) |
| MachineSensorEditModeTests | 13/13 | [MachineSensorEditModeTests.xml](MachineSensorEditModeTests.xml) |
| SensorPublicDataEditModeTests | 3/3 | [SensorPublicDataEditModeTests.xml](SensorPublicDataEditModeTests.xml) |
| MotionWorkBridgeEditModeTests | 5/5 | [MotionWorkBridgeEditModeTests.xml](MotionWorkBridgeEditModeTests.xml) |
| AlgorithmWorkbenchProductionPlayModeTests | 1/1 | [AlgorithmWorkbenchProductionPlayModeTests.xml](AlgorithmWorkbenchProductionPlayModeTests.xml) |
| AlgorithmEditorCanvasPlayModeTests | 1/1 | [AlgorithmEditorCanvasPlayModeTests.xml](AlgorithmEditorCanvasPlayModeTests.xml) |

## 截图与状态

- [空图](workbench-empty-1920x1080.png)、[复杂图](workbench-complex-1920x1080.png)、[旧修订任务诊断](workbench-diagnostic-1920x1080.png)：实际1920×1080原生捕获，无缩放伪造。自动隐藏并恢复GF调试窗；截图同时具备真实任务断言。
- baseline两PNG及三组JSON/XML另存；基线PNG核验也是1920×1080。Canvas回归生成的历史Assets/Screenshots文件先备份，证据复制后恢复，未覆盖历史截图库。
- [结构导出](layout-after.json)、[项目健康](project-health-20261008.txt)。健康脚本第一项标题字面仍写Unity8092，但实际命令`--port 8091`，网络与项目身份核验均为8091；未连接其他工程。
- 普通Unity编译0错误；两条已有未使用变量警告保留。缺脚本/缺引用=0，数据表12成功0失败0警告；框架纯度与业务边界审计通过，`git diff --check`无错误。

## 失败、恢复与边界

首次新增测试失败已保留JSON/XML：触发时间不是结束时间、Apply返回请求创建而非业务通过、Editor协程不能同步CaptureScreenshotAsTexture，分别改为读取捕获触发、Rejected状态断言和原生异步截图；绑定编辑两次使真实修订成为r3，不写死r1。

一次错误的测试组名MachineMotionWorkBridgeEditModeTests匹配0项，工具job未完成且不支持硬取消；普通编译后工具自动判定原runner未恢复，保存其failed/0项JSON，无原生XML，不计入102项。随后使用真实MotionWorkBridgeEditModeTests完成5/5。未修改UnitySkills包、伪造XML或把工具任务错误计为产品通过。

本批证明工作台与真实导航闭环，不代表农业/采矿/砍伐系统模板业务全部完成。完整正常新进度入口/G3综合复验仍按B47及正式初始配置追踪，P4生产运输在后续批次；未回写xlsx、未归档change、未操作Git索引或提交。回滚需一起恢复UI源/契约/两Prefab与对应视图代码，玩家草稿结构未改变；历史运行记录只是内存证据，持久化由B47/B48处理。
