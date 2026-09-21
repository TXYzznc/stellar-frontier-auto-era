# 任务

## 1. 数据：三个新音频分组（受控通路）

- [x] 1.1 `GameData/AIData/DataTables/Core/SoundGroupTable.json` 增加三行：
      `Ambient`(Id 3, 2 个 SoundAgent)、`Machine`(Id 4, 4 个)、`Ui`(Id 5, 2 个)，均不静音、音量 1。
      备注列用 ASCII（`Ambient` / `Machine and production` / `UI and alerts`）：
      **实测该表的 txt 生成链路对非 ASCII 注释会乱码**（表头「备注」本来就是乱码），
      而注释列不参与解析，没必要往里塞中文。
- [x] 1.2 走受控通路：`Validate DataTables Json`(1012) → `Reverse DataTables Json To Excel`(1014) →
      `Refresh All Excels`(1001)，产出 `SoundGroupTable.txt`（5 行）与 `Const.Groups.cs`
      里新增的三个枚举值。**全程未手改任何 xlsx**。
      （1014 会重新序列化全部 12 张表与 JSON——实测哈希变化不等于内容变化；
      顺带把 `MachineDefinitions.txt` 补齐了上一批加的占地两列，那是该有的同步。）
- [x] 1.3 新增 `Tools/_unity_menu.py`：跑单个菜单并回读结果（域重载窗口内自动重试），
      免得每次都内联一段 python。

## 2. 框架层：按表逐行恢复（不再写死两组）

- [x] 2.1 `SettingExtension` 增加**按分组名**的 `SetMediaVolume` / `GetMediaVolume` /
      `SetMediaMute` / `GetMediaMute`；原来的枚举重载改为委托给它们。
      理由：分组名是数据表里的行，框架层不该被枚举绑死——否则每加一个分组都要改框架代码。
      `Sound.<分组>.Volume` 的键名与语义保持不变（它现在存的是**最终生效值**，见 3.2）。
- [x] 2.2 `PreloadProcedure.InitGameFrameworkSettings`：
      ① 保留「`Const.SoundGroup` 里每一个值都必须出现在表里」的响亮校验；
      ② 把恢复逻辑从**写死 Music/Sound 两组**改为**按表逐行恢复**静音与音量。
      不修 ② 的话，新分组每次启动都会被打回表里的默认值——等于「调了没用」。
- [x] 2.3 确认 `Const.Groups.cs` 由数据表生成（文件头就写着「由工具自动生成，请勿手动修改」），
      因此「加分组」是数据变更而不是代码变更。

## 3. 设置域：五路音量

- [x] 3.1 新增 `AutoEra.Settings.AutoEraAudioSettings`：`AudioBus` 五路
      （主音量／音乐／环境／机器与生产／UI 与警报，顺序即规格滑条顺序）、
      `GetBus` / `SetBus` / `EffectiveVolume` / `ResetToDefaults` / `DifferenceFromDefaults` /
      `Percent` / `BusLabel` / `GroupOf`。
- [x] 3.2 **两组键，各司其职**：
      `AutoEra.Audio.Bus.<总线>` 存滑条原始值；`Sound.<分组>.Volume` 存**最终生效值**（主音量已乘进去）。
      这样 `PreloadProcedure` 只要「把每一行读回来」就是对的——主音量是产品概念，
      在框架层出现就是分层倒置。反过来，**绝不能**把 `Sound.<分组>.Volume` 当原始值读回来。
- [x] 3.3 主音量是**总控**：`EffectiveVolume(非主音量路) ＝ 主音量 × 该路`；
      调主音量不改各路的原始值。「音效」分组没有独立滑条，只随主音量（页面明说这一点）。
- [x] 3.4 边界：越界夹到 0..1；NaN 视为默认值（不让滑条卡死或静音）。
- [x] 3.5 `ISettingsStore` 增加 `GetFloat` / `SetFloat`（`SettingComponent` 本来就有，只是接口没暴露）；
      `GfSettingsStore` 与测试内存存储同步实现。
- [x] 3.6 `SettingsReadModel`：声音页改为 Ready，给出音量栏（**原始值 ＋ 最终生效值**）
      与本机配置状态栏（立即生效／保存状态／与默认值差异）；初始页按规格页序落在**声音页**。
      没有存储时声音页单独一条原因，不再与显示页共用同一句。
- [x] 3.7 `SettingsForm`：五个滑条接 `onValueChanged` → 立即写入并生效；
      渲染用 `SetValueWithoutNotify` ＋ `_rendering` 兜底（`value` 赋值会触发回调，
      不挡住就会「渲染变写入」甚至来回抖动）；「恢复本页默认」只动声音页。
      音量栏正文写清换算关系（主音量 50% ＋ 音乐 50% ＝ 实际 25%）。

## 4. 验收

- [x] 4.1 编译 0 错。
- [x] 4.2 门1 契约自检全绿（本批不改契约）；`run_project_checks.py` 5/5（含 AIData 校验 12/12）。
- [x] 4.3 新增 `AudioSettingsEditModeTests`（**11/11**）：
      默认全满；**主音量缩放每一路**；框架键存的是最终值；重新读取不会把主音量乘两遍；
      夹取与 NaN；百分比取整；恢复默认只动声音页；写入失败保留内存值并说明后果；
      一次写入只落盘一次；标签与分组名逐条对齐规格；
      **界面引用的每个分组都真的在 SoundGroupTable 里**（分组是数据，这条断言守着数据与代码不脱节）。
- [x] 4.4 `SettingsReadModelEditModeTests` 更新为 **9/9**：三页原因互不相同（含声音页的新原因）、
      声音页 Ready 且给出 5 路 ＋「音效随主音量」共 6 行与 3 行配置状态、初始页落在声音页。
      顺带把内存存储抽成共享的 `MemorySettingsStore`（两处测试都要用，两份实现迟早走样）。
- [x] 4.5 `SettingsFormPlayModeTests` 改成端到端（**1/1**）：
      `GF.Sound.HasSoundGroup` 逐个确认五个分组真的被数据表建成；拖滑条 → 真实分组音量变化；
      主音量 50% ＋ 音乐 50% → 音乐分组真的 0.25，而音乐滑条的原始值仍是 0.5；
      未单独调过的路同样被缩放；「音效」随主音量；操作页仍然不可用且原因指到绑定表；
      结束时恢复默认，不把开发机的持久化音量留在测试改过的值上。
- [x] 4.6 回归：EditMode **25 类**全绿、PlayMode **12 套**全绿；进 Play Mode 的 EditMode 类全绿。

## 5. 边界

本变更不实现：操作分页（镜头速度需要拿到场景里的相机控制组件，键位重绑需要可持久化的输入绑定表）、
静音开关（本版规格的声音页没有静音控件）、音频素材本身（工程里本来就没有声音资源）。

不改界面契约与预制体结构；不改 `UIViews` 登记。

## 6. 一条反复出现的判据错误（第四次）

`SettingsForm` 的声音页当初被写成「未接线」，理由是本机音频分组只有两组。这次侦察发现：
**每路音量可读写、可持久化、立即生效的那条路早就在生产里**（`SettingExtension` 的
`SetMediaVolume`，`PreloadProcedure` 一直在用它恢复音乐与音效），真正缺的只有 3 个分组数据行，
外加一条「启动只恢复了两组」的路径。

前三次误判是 `ComponentLibraryForm`、`SettingsForm` 的显示页、`ComponentPickerForm`
（见 `ui-game-system-integration` 的 2.7）。四次全是同一类错误：
**把「还没有这一页的读模型／数据」当成了「这个域没有后端」**。

因此本批再次强化那条执行纪律：**在给任何界面写「未接入」之前先做代码侦察**——
判据是「该界面依赖的领域服务在生产里没有创建者」，而不是「这一页还没人写读模型」。
