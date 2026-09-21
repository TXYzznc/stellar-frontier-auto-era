# 声音设置接线（五路音量与音频分组）

## Why

设置页三页里，显示与性能在 `user-settings-display-wiring` 那一批就接线了；声音页当时留了缺口，
理由写在读模型里：「本机音频分组只定义了『音乐』与『音效』两组，而规格要求主音量、环境、
机器与生产、UI 与警报共五路独立音量——缺的那几路需要先新增音频分组与其资源」。

侦察后的结论是：这条理由**只说对了一半**，而另一半正是本批要补的——

| 规格要求 | 生产里的既有实现 | 结论 |
|---|---|---|
| 每路音量可读写、可持久化、立刻生效 | `SettingExtension.SetMediaVolume/GetMediaVolume`：既写 `GF.Setting`（落盘）也设实时分组音量 | **早就在生产里**，`PreloadProcedure` 一直在用它恢复音乐与音效 |
| 音量能落到实际播放上 | `GF.Sound` 的 `SoundGroup.Volume`，分组由 `SoundGroupTable` 建 | 真实后端 |
| 五路独立音量 | `SoundGroupTable` 只有 2 行 → `Const.SoundGroup` 只有 Music/Sound | **真缺口**：缺 3 个分组 |

所以这不是「界面没后端」，而是**一个数据缺口 ＋ 一条只恢复了两组的启动路径**：

1. 分组是**数据**（`SoundGroupTable`），不是代码——加 3 行即可，`Const.SoundGroup` 是它的生成物；
2. `PreloadProcedure` 把分组建出来之后，**只**恢复了 `Music` 与 `Sound` 两组的音量与静音；
   新分组每次启动都会被打回表里的默认值。这条不修，新分组等于「调了没用」。

顺带记一条判据（这是同一类误判的**第四次**）：`SettingsForm` 在
`ui-game-system-integration` 的 2.7 里曾整页被判「未接入」，随后显示页迁出、
声音页这次也迁出。前三次（`ComponentLibraryForm`、`SettingsForm` 显示页、`ComponentPickerForm`）
的错误形态完全一样：**把「还没有这一页的读模型/数据」当成了「这个域没有后端」**。

## What Changes

### 数据（受控通路）

- `GameData/AIData/DataTables/Core/SoundGroupTable.json` 增加三行：`Ambient`(3)、`Machine`(4)、`Ui`(5)。
  随后走受控通路 `Validate DataTables Json`(1012) → `Reverse DataTables Json To Excel`(1014) →
  `Refresh All Excels`(1001)，产出 `Assets/Game/DataTable/Core/SoundGroupTable.txt` 与
  `Const.Groups.cs` 里新增的三个枚举值。**禁止手改 xlsx**。
  备注列刻意用 ASCII（`Ambient` / `Machine and production` / `UI and alerts`）：
  实测该表的 txt 生成链路对非 ASCII 注释会乱码（表头「备注」本来就是乱码），
  而注释列不参与解析，没必要往里塞中文。

### 框架层（可改，需留痕）

- `SettingExtension`：新增**按分组名**的 `SetMediaVolume/GetMediaVolume/SetMediaMute/GetMediaMute`，
  原来的枚举重载改为委托给它们。分组名是数据表里的行，框架层不该被枚举绑死。
- `PreloadProcedure.InitGameFrameworkSettings`：
  - 保留「代码引用的每一组都必须在表里」的响亮校验（`Enum.GetValues(Const.SoundGroup)` 逐个查）；
  - 把恢复逻辑从**写死两组**改为**按表逐行恢复**。新分组从此不必再改这里。

### 设置域

- 新增 `AutoEra.Settings.AutoEraAudioSettings`：
  - `AudioBus` 五路（主音量／音乐／环境／机器与生产／UI 与警报），顺序即规格滑条顺序；
  - **两组键**：`AutoEra.Audio.Bus.<总线>` 存滑条原始值（0..1）；
    `Sound.<分组>.Volume` 存**最终生效值**（主音量已乘进去）。
    这样 `PreloadProcedure` 只要「把每一行读回来」就是对的——主音量是产品概念，
    不该出现在框架层。反过来说，**绝不能**把 `Sound.<分组>.Volume` 当原始值读回来（会把主音量乘两遍）。
  - `EffectiveVolume` ＝ 主音量 × 该路；`Main` 是总控，不是第六路。
  - NaN 与越界一律夹到 0..1；「音效」分组没有独立滑条，只随主音量（页面明说这一点）。
- `ISettingsStore` 增加 `GetFloat/SetFloat`（`SettingComponent` 本来就有，只是接口没暴露）；
  `GfSettingsStore` 与测试用的内存存储同步实现。
- `SettingsReadModel`：声音页改为 Ready，给出音量栏（原始值 ＋ 最终生效值）与本机配置状态栏；
  初始页按规格页序落在**声音页**（前两页都已接线，不再需要绕到显示页）。
  没有存储时声音页的原因单独一条（不再是整页共用一句）。
- `SettingsForm`：五个滑条接 `onValueChanged` → 立即写入并生效；
  渲染用 `SetValueWithoutNotify` ＋ `_rendering` 兜底（赋值会触发回调，不挡住就会「渲染变写入」甚至抖动）；
  「恢复本页默认」只动声音页。音量栏正文写清换算关系（主音量 50% ＋ 音乐 50% ＝ 实际 25%）。

## Capabilities

### New Capabilities

- `audio-settings-wiring`：五路音量总线与音频分组的接线（数据、启动恢复、界面写入）。

### Modified Capabilities

- `ui-game-system-integration` 的 2.7：设置页的声音分页从「未接入」变为已接线（留痕写在那一节）。

## Impact

- 数据：`GameData/AIData/DataTables/Core/SoundGroupTable.json`（＋ 受控通路重写的全部工作簿与 JSON——
  实测 1014 会重新序列化所有表，哈希变化不等于内容变化）。
- 框架层：`Assets/Game/Scripts/Extension/SettingExtension.cs`、`Assets/Game/Scripts/Procedures/PreloadProcedure.cs`。
- 产品层：`Assets/Game/Scripts/AutoEra/Settings/AutoEraAudioSettings.cs`（新）、
  `AutoEraDisplaySettings.cs`（`ISettingsStore` 加 float）、
  `UI/Integration/SettingsReadModel.cs`、`UI/SettingsForm.cs`。
- 验收：门1 必须持续全绿（本批不改契约）；`run_project_checks.py` 5/5（含 AIData 校验 12/12）；
  EditMode **25 类**、PlayMode **12 套**全绿；新增 `AudioSettingsEditModeTests` 11 项，
  并把 `SettingsFormPlayModeTests` 改成端到端断言真实音频分组音量。
- **不在本变更内**：操作分页（镜头速度需要能拿到场景里的相机控制组件，键位重绑需要可持久化的
  输入绑定表）、静音开关（本版规格的声音页没有静音控件）、音频资源本身（工程里本来就没有声音素材）。
- **风险点**：把「滑条原始值」与「最终生效值」混为一谈是实现这一页最容易犯的错，
  两种错法（当成并列第六路 / 读回来再乘一遍）都会让音量悄悄偏掉。用例分别在
  `MasterScalesEveryOtherBus_NotJustItsOwn` 与 `ReadingBack_DoesNotApplyTheMasterTwice` 上守着。
