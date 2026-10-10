# B50 增量01：UI生命周期、设置控件与证据收口

2026-10-09。用户已授权十一批实施；主工程Unity2022.3.62f3c1、8091，UI仅1920×1080。此增量完成现有UI可执行单元，**B50整批/P8原任务/G8未完成**。B47正式开局、B48完整领域、B49完整离线及G7/P8-001/P6文案前置尚未交付。

## 实际修改

- 产品导航跟踪异步打开参数所有权：根窗取消、父窗取消冷加载子页、失败/拒绝/迟到请求释放原参数；已打开参数仍由GF生命周期释放。三个既有子页入口转接产品OpenSub，保留GF子窗排序和父窗关闭合同。通用GF/UIFormBase/ScriptsBuiltin未修改。
- 设置页Ready不再显示Success占位卡；Unavailable卡显示真实原因。8个滑杆绑定填充/柄/目标图形，增加16个8/24高的专用区域，维持680×56输入根；2个反转开关补图形引用。根透明Image承担输入命中，被动Img不拦截输入；真实数值同步到标签，不触发写设置回调。
- 契约生成器与单一SettingsForm资产同步修复；门1按真实Slider引用识别驱动锚点，仍检查pivot、尺寸偏移、位置与绑定，未全局放宽Rect检查。未改生成Fields、程序集或依赖。
- HUD既有中枢/机器库入口注册一次点击监听并透传当前会话；导航测试按已提取的机器总览子Form检查诊断入口。其余未接入HUD功能不计为完成。
- 静态审查工具识别已接入UIDialog，节点数只筛选实测候选；保留历史实施段。健康脚本标签使用实际端口。更新交接、检查入口与功能证据索引，未写xlsx/Git索引、未提交或自动归档。

## 原生回归

10组去重 **51/51**，每组完成回调和同次Unity原生XML均核对。Editor程序集中的UiLifetime测试实际EnterPlayMode，其他3组为原生PlayMode；不能把fixture世界说成正常菜单新建的完整世界。

| 测试组 | job | 通过 | 原生XML |
|---|---|---:|---|
| UiLifetimeStressPlayModeTests | 6e1afd4d | 1/1 | [UiLifetimeStressPlayModeTests-6e1afd4d.xml](UiLifetimeStressPlayModeTests-6e1afd4d.xml) |
| UiStructureContractEditModeTests | f17cfc53 | 5/5 | [UiStructureContractEditModeTests-f17cfc53.xml](UiStructureContractEditModeTests-f17cfc53.xml) |
| AudioSettingsEditModeTests | ab259dba | 11/11 | [AudioSettingsEditModeTests-ab259dba.xml](AudioSettingsEditModeTests-ab259dba.xml) |
| ControlSettingsEditModeTests | 48b8776e | 10/10 | [ControlSettingsEditModeTests-48b8776e.xml](ControlSettingsEditModeTests-48b8776e.xml) |
| SettingsReadModelEditModeTests | 64949dcf | 12/12 | [SettingsReadModelEditModeTests-64949dcf.xml](SettingsReadModelEditModeTests-64949dcf.xml) |
| AlgorithmEditorFormBindingsEditModeTests | df457f97 | 5/5 | [AlgorithmEditorFormBindingsEditModeTests-df457f97.xml](AlgorithmEditorFormBindingsEditModeTests-df457f97.xml) |
| AutoEraRuntimeSettingsEditModeTests | ac7e8f58 | 4/4 | [AutoEraRuntimeSettingsEditModeTests-ac7e8f58.xml](AutoEraRuntimeSettingsEditModeTests-ac7e8f58.xml) |
| AutoEraUiNavigationPlayModeTests | 7451e398 | 1/1 | [AutoEraUiNavigationPlayModeTests-7451e398.xml](AutoEraUiNavigationPlayModeTests-7451e398.xml) |
| AutoEraOperationsUiFormPlayModeTests | 482f1b90 | 1/1 | [AutoEraOperationsUiFormPlayModeTests-482f1b90.xml](AutoEraOperationsUiFormPlayModeTests-482f1b90.xml) |
| AlgorithmEditorCanvasPlayModeTests | d0e91085 | 1/1 | [AlgorithmEditorCanvasPlayModeTests-d0e91085.xml](AlgorithmEditorCanvasPlayModeTests-d0e91085.xml) |

原始UTC和SHA256见[原生索引](segment-01-native-index.json)；收口代码/资产摘要见[源文件摘要](segment-01-source-sha256.json)。早期失败与中间通过结果均保留：e471b28d根取消、f94d32f4父窗取消泄漏；80a074a3/c0526bdd仅中间版本；1b4dcbb8为尚未启用分页的比例断言前提问题；d7001716缺HUD监听、1cb72af5为按钮已移到子Form后的过时父窗断言。最终UI结果为6e1afd4d，导航7451e398。

## 生命周期与截图

五页各100次，共500次；前50次旧世界、后50次新世界。另真实遍历根窗和两类子页冷加载取消、重复关闭、旧序列号关闭不影响替代窗。打开/关闭后焦点与输入锁恢复，关闭UI不释放世界。

| 指标 | 预热后 | 500次后 |
|---|---:|---:|
| 场景Form对象（含缓存） | 6 | 6 |
| Form层级Transform节点 | 2436 | 2436 |
| UIParams占用 | 0 | 0 |
| 已加载活动Form | 0 | 0 |
| 旧世界MachineRoster.Changed监听 | 0 | 0 |
| 新世界MachineRoster.Changed监听 | 0 | 0 |

JSON中的OldWorldListeners/NewWorldListeners具体统计MachineRoster.Changed，不代表已测所有领域事件；fixture中未接通的业务域仍未覆盖。节点计数覆盖行/装饰节点，不能单独替代内存/Player长时分析。

设置页三分页真实读模型的正常状态、8个滑杆尺寸/填充比例、2个开关显隐及控件根射线命中已检查，不写本机设置。截图：[声音](settings-page-0-1920x1080.png)、[操作](settings-page-1-1920x1080.png)、[显示](settings-page-2-1920x1080.png)，对照[原异常截图](settings-before-fix-1920x1080.png)。另四页warm截图仅证明fixture/默认状态：现场详情无选中对象、操作弹窗无请求、报告占位、中枢未接完整P6；不称正式业务关键状态全量通过。

## Editor诊断采样

| 页面 | 初始样本ms | 重复p50 ms | p95 ms | p99 ms |
|---|---:|---:|---:|---:|
| FieldHudDetailForm | 8.207 | 5.905 | 9.680 | 10.138 |
| OperationDialogForm | 53.192 | 4.312 | 7.096 | 7.849 |
| ProgressReportForm | 38.614 | 3.527 | 7.145 | 9.309 |
| BaseCommandHubForm | 4.342 | 4.360 | 8.159 | 10.928 |
| SettingsForm | 140.952 | 12.421 | 19.205 | 43.975 |

包含Editor调度及等帧，不是UI纯CPU耗时，也不是Development Player预算。初始样本在取消场景之后，部分父窗已缓存，不叫冷启动。完整原始100次样本在[测量JSON](ui-lifetime-editor-measurement.json)。没有依据此表或静态节点数新增拆页、冻结CPU/GPU预算或声称达标。

## 场景与门状态

| spec Scenario | 本次证据 | 边界 |
|---|---|---|
| Inspect reference layout | 三设置分页截图、射线命中及既有五页截图 | 正常设置状态通过；其他未接业务关键状态未全覆盖 |
| Inspect prefab sizing ownership | 五页结构5例、Settings真实Slider驱动及根命中 | 根stretch/offset、统一GF缩放、模板inactive通过；全量门1有仓储基线 |
| Close while loading | 根及2类子页真实异步取消 | 通过；迟到无Form/UIParams残留 |
| Repeated open close | 五页各100次、跨2会话 | 通过上述对象/节点/花名册监听/焦点/输入范围 |
| Idle unchanged world view | 既有Changed驱动与B46能源读模型证据 | 全量UI常驻刷新Player分析未执行，2.3未完成 |
| Load tiers and soak | [B46独立电网Player](../../b46-p5006-p5009-p5010-energy-hotpath/evidence/implementation-20261008.md)仅引用原范围 | B50联合10/50/100、50台60分钟未执行 |
| Old report disagrees | 最新交接及功能索引，历史保留日期 | 文档更新完成；全部P6/P7文案ID门仍等真实接入 |

[健康记录](project-health-20261009.txt)5/5：编译0错误、缺失脚本/引用0、AppConfigs齐全、数据表12/12、纯度/边界通过；11批OpenSpec strict与diff检查通过。历史CS0219/CS0414仍按实际编译程序集出现，不称全部历史警告已修复。

全量门1从26项降为6项：本次Settings新增20项清零；WarehouseForm既有6项L5宽度声明问题保留，原失败[报告](gate1-before-driver-support.txt)及当前[报告](gate1-after-settings-fix.txt)均存档。归属仓储UI布局合同维护，不以健康5/5掩盖，不称全量门1通过。

## 剩余项与恢复条件

B50的2.1/2.2/2.3/2.6涉及同一正式Development Player及完整负载；2.4/2.5有已交付的独立子集，全部状态/文案依赖仍缺；3.2整批签收未完成。前置领域交付后按B48完整快照→B49真实离线提供者/报告→B47正式开局往返→B50联合性能及60分钟稳定性恢复。不新增未授权经济、农业、水泵、施工制造或成长规则，不用GM/空余额/手动Pump绕过门。

回滚按精确允许清单恢复本地变更：导航及其3个子页入口配套回退；Settings逻辑、生成器、Checker、合同及单一Prefab配套回退；HUD两监听与导航测试配套回退。未实际执行回滚，也未用git checkout/reset覆盖共享工作树。已存native/原截图可复核行为；未碰玩家真实存档。

OpenSpec状态补充：status的isComplete=true仅指四类文档artifact齐全，不代表实施完成；当前tasks为3/10已勾。完整G8和本change整体保持未完成。
