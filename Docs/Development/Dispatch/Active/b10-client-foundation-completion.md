# B10 基础剩余项连续补齐

2026-09-10 制作人处理客户端完成回传；延续用户已授权B10 1.2与原b02，不重开已完成B05/B08/B09。
任务ID：b10-client-foundation-completion；负责人client；优先级10。
输入：B10 evidence/client-foundation-gaps.md、当前b02 design/specs/tasks、B10现有有效QA证据。
只读对账已完成；B10 1.2整体仍未完成，不将表格对账等同功能交付。

## 连续执行顺序与DoD

1. 时间：复用WorldClock及现有配置，补b02 5.1/5.2/5.5的开发倍率边界、直接/不同分帧推进跨昼夜等价、实际GF管理UI打开期间继续推进。倍率仅开发入口，正常产品恒定规则不变，不新增玩家暂停/加速和离线调度器。
2. 最小数据贯通：补b02 2.7–2.9/7.1/7.3；先核对已有适配器，补三类最小Foundation输入与成功往返/失败回滚/正式GF读取证据。只服务已有启动、场景、时间；不创建P0-011全对象骨架，不复制第二套时间权威。字段/资源清单在自身证据先记录，既有合同内技术选型可自行决定；真正新增玩法或框架核心改动才请求裁定。
3. 现有启动异常：只补b02 6.5缺失的加载中重启、半初始化失败、会话和订阅释放。复用场景协调器，不重写启动系统；adb08994已覆盖的取消/双提交不再当新增成果，可作必要集成回归。
4. 分组降级：核验P0-012既有声音资源缺失/播放失败时可定位、不阻断其他组及启动。允许产品层最小失败夹具/必要安全处理；不造音频资产、不新增分组、不忽略所有Console错误。预期负例与恢复后普通Console分开取证。
5. 收口：交QA验证本次差异；正常编译、引用、产品配置模式纯度审计和项目边界检查。逐项注明覆盖/未覆盖，不以测试数量代替内容。保留物理硬件完整链等真实未覆盖项，不新增手动作业入口。

## 允许与禁止

允许主仓库本次必要 `Assets/Game/Scripts/AutoEra/` 产品实现、`Assets/Game/Tests/AutoEra/` 测试、b02自身tasks/evidence、B10 evidence/client-foundation-*.md。
延续既有启动/时间配置接入授权：`Assets/Game/ScriptableAssets/Core/AppConfigs.asset` 仅必要项目数据登记；`GameData/AIData/{DataTables,Configs,Languages}/Foundation/` JSON及必要GenerationProfiles配置。
正式数据、xlsx和C#产物只能由已有安全生成流程产生到对应Foundation目录/AutoEra.DataTable；不得手写生成物或直接编辑任何xlsx，任务表完全只读。Reverse先读当前逻辑指纹，冲突必须失败，不强制覆盖用户数据。
仅必要的既有Launch/MainMenu/InitialRegion与Operations UI测试接线沿原B10授权；不重做视觉、不移动/删除正式资产。
禁止新增ScriptsBuiltin改动、架构重写、新包、完整生产/算法/物流/存档实现，以及任何Git索引/提交请求。
8090按需与QA交接；无需同步2D。真实框架修改需求须报告精确文件/原因，期间继续其它独立安全单元。

## 执行与回传

按上述1→5连续推进，不在每单元结束等待制作人逐项放行。冻结的机械配置/测试可按RapidExecution规则直接拆包，专业负责人保留复核责任。
每单元增量记录b02实际证据并更新确实完成项，B10只链接，不伪造旧测试结果。整包完成必须实际通知制作人：完成项、证据、未覆盖项、占用释放与下一队列状态。
任务表哈希使用本次执行前后检查点；用户期间自行维护须如实注明外部差异，禁止恢复历史哈希或将差异直接归因AI。见b02 7.5修订。

## 2026-09-10 数据事务缺口：框架精确修复授权

制作人已只读确认：DataTable TryImportAIJson先写正式TXT再验证/生成；Config/Language Reverse只事务替换xlsx，随后JSON写入在事务外。现行ReplaceFilesTransactionally还在File.Copy成功后才登记回滚对象，且finally无条件清理备份，存在失败中途目标变化未恢复、恢复失败后备份被清除风险。
这些是原b02事务保证的实现缺口，不是新增数据系统。允许客户端在本Active内仅修以下框架文件：
- Assets/Game/ScriptsBuiltin/Editor/AIGameDataTableGenerator.cs
- Assets/Game/ScriptsBuiltin/Editor/AIData/AIGameDataSyncPipeline.cs
- Assets/Game/ScriptsBuiltin/Editor/AIData/AIConfigAdapter.cs
- Assets/Game/ScriptsBuiltin/Editor/AIData/AILanguageAdapter.cs

限定目标：按单次受控操作先列完整读写集合，在临时位置生成并校验，再统一提交该操作实际产生的xlsx/TXT/bytes/C#/JSON指纹；不适用的输出不强造。不得先改正式文件再宣称预校验通过；现有其他通用生成器只能调用，不改其源码，若无法安全重定向输出则报告精确剩余入口。
提交前重核源指纹与目标变化，冲突无force；防止并发写入被回滚覆盖。保留既有meta/GUID，禁止全量RefreshAll带入未授权文件；必要刷新只在受控提交后进行。
回滚须覆盖正在失败的那次写入；新目标失败后移除仅本次创建的内容，既有文件恢复原字节；回滚失败必须保留恢复备份并报告位置，不能finally删除唯一恢复来源。不得声称多文件复制具操作系统级原子性；如未覆盖进程崩溃/断电，明确记录该边界。
测试沿产品Editor授权：三类成功往返、校验失败零正式改动、生成失败、提交中途失败、JSON指纹提交失败、恢复失败保留备份、新文件清理、源并发冲突、Core兼容/Profile路径边界。普通编译及QA差异回归，禁止FSR替代。
精确文件之外的框架改动另报，不降低原事务DoD；不改任务表、不手工写xlsx、不操作Git。受影响正式数据写入在修复及门禁前保持停止，其它独立单元继续。

### 暂存生成入口补充授权

制作人只读确认：DataTableGenerator.GenerateCodeFile从正式Profile推导输出，模板委托/上下文private；Config bytes入口private且按输入后缀决定输出，Group枚举入口读取固定三表。允许额外修改以下两文件，仅添加必要显式暂存输入/输出重载：
- Assets/Game/ScriptsBuiltin/Editor/DataTableGenerator/DataTableGenerator.cs
- Assets/Game/ScriptsBuiltin/Editor/GameDataGenerator.cs

旧入口签名和默认行为保留，委托给同一生成实现；不复制模板、不用反射绕过private、不改文件格式/编码/枚举值/排序/命名空间。暂存文件物理位置不得改变表的逻辑名与Profile解析，输出映射继续验证批准根目录，返回明确成功/失败。
范围限DataTable C#、Config bytes、既有UIViews/Group枚举生成。Group聚合应读取本次暂存更新项与未变更兄弟表的一致快照，并把全部读取依赖纳入提交前并发检查；不能顺带修改兄弟表或注册新组。
不为本批拒绝全部Core写入来替代兼容修复；可在入口未安全完成前暂时阻止受影响操作并报明确错误，不能宣称全覆盖。兼容测试在隔离暂存输入上完成，不以验收为由重写当前正式Core数据。
补新旧入口生成内容等价、失败不触碰正式输出、临时路径不污染类型名/Profile、Core特殊表事务回滚和聚合依赖变化拒绝测试。其余框架不扩权，禁止RefreshAll。

### 分组输出常量单行纠正

2026-09-10制作人核实 `Assets/Game/Scripts/Common/Const.Groups.cs` 存在，`Common/Core/Const.Groups.cs` 不存在；常规与事务入口均引用同一错误常量。
精确允许修改 `Assets/Game/ScriptsBuiltin/Editor/Common/ConstEditor.cs` 中 `ConstGroupScriptFileFullName` 一行，将路径改为 `Assets/Game/Scripts/Common/Const.Groups.cs`。
不搬移文件、不改变分组内容/编号、不创建Common/Core、不删除其它文件。重跑失败的CoreSpecialScripts用例，确认两入口目标相同、既有meta/GUID不变、无重复Const.Groups生成及普通编译通过。此前25测24过不得计为全过。

### 自动监听与精确刷新修复

制作人只读确认DataTableUpdater监听LastAccess、后台直接写List、映射为空仍刷新；RefreshAllDataTable无论传入集合仍遍历AppConfigs全部DataTables生成代码。允许额外修改 `Assets/Game/ScriptsBuiltin/Editor/DataTableUpdater.cs`，并在已授权 `GameDataGenerator.cs` 内修正该方法的有限列表行为。
移除LastAccess；采用线程安全事件收集、主线程去重/消费，Unity API不在watcher线程调用；避免清空时丢失新事件。保留正常用户保存及原子替换/重命名保存的变更检测，处理临时锁文件、删除诊断、重载释放，不能全局关闭监听。
空列表必须无写入、无全量生成；null保持原显式全量入口。非空集合只生成明确目标及其既有AB主表映射依赖，未知/越界输入不得扩大到全部表。前序生成失败不得继续用旧正式TXT生成新C#。
事务仅抑制完整成功提交且输出已齐备的精确文件版本；用规范路径加内容指纹核对，不以目录/时间窗口忽略用户修改。提交事件到达与主线程消费有竞态时保证不会先非事务刷新；失败/回滚不登记成功，后续用户修改指纹不同必须正常处理。同一事务重复事件可去重，不能永久忽略路径。不得声称此修复提供断电原子性。
测试增加：空列表零输出、单表不碰其它表、null兼容、纯读取零生成、重复/并发事件不丢用户变化、成功事务无二次生成、事务后同路径用户修改仍刷新、原子保存、失败/回滚不被误标成功；保留3条Core错误证据，不清错冒充修复。
Const.Groups注释头差异可从语义兼容断言中排除，但必须验证类型/成员名称、数值、顺序和条件编译语义；新增重载与旧生成器应另比较同输入生成结果。不为通过测试改正式分组文件。
