# 02-系统与设置 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## SystemMenu：游戏内系统菜单

功能文档：[游戏内系统菜单](SystemMenu.md)；归属 `SystemMenuForm`；内容 688×670。

```text
Panel_PageSystemMenu [Image]
  Txt_SystemMenuTitle [TextMeshProUGUI]
  Grp_SystemMenuActions [GridLayoutGroup fixedColumns=3 cell=(218,48) spacing=(8,8)]
    Btn_SystemMenuResume [Button + Image]
      Txt_SystemMenuResumeLabel [TextMeshProUGUI]
    Btn_SystemMenuSettings [Button + Image]
      Txt_SystemMenuSettingsLabel [TextMeshProUGUI]
    Btn_SystemMenuHelp [Button + Image]
      Txt_SystemMenuHelpLabel [TextMeshProUGUI]
    Btn_SystemMenuReturn [Button + Image]
      Txt_SystemMenuReturnLabel [TextMeshProUGUI]
    Btn_SystemMenuQuit [Button + Image]
      Txt_SystemMenuQuitLabel [TextMeshProUGUI]
  Panel_SystemMenuSession [Image]
    Txt_SystemMenuSessionHeading [TextMeshProUGUI]
    List_SystemMenuSession [ScrollRect vertical=true horizontal=false]
      Viewport_SystemMenuSession [RectMask2D]
        Content_SystemMenuSession [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SystemMenuSessionBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SystemMenuSessionTemplate [LayoutElement + Image；默认inactive]
            Btn_SystemMenuSessionRow [Button + Image]
              Txt_SystemMenuSessionRowLabel [TextMeshProUGUI]
              Txt_SystemMenuSessionRowValue [TextMeshProUGUI]
  Panel_SystemMenuCommands [Image]
    Txt_SystemMenuCommandsHeading [TextMeshProUGUI]
    List_SystemMenuCommands [ScrollRect vertical=true horizontal=false]
      Viewport_SystemMenuCommands [RectMask2D]
        Content_SystemMenuCommands [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SystemMenuCommandsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SystemMenuCommandsTemplate [LayoutElement + Image；默认inactive]
            Btn_SystemMenuCommandsRow [Button + Image]
              Txt_SystemMenuCommandsRowLabel [TextMeshProUGUI]
              Txt_SystemMenuCommandsRowValue [TextMeshProUGUI]
  Grp_SystemMenuLoadingState [无Graphic]
    Panel_SystemMenuLoadingMessage [Image]
      Txt_SystemMenuLoadingMessage [TextMeshProUGUI]
  Grp_SystemMenuEmptyState [无Graphic]
    Panel_SystemMenuEmptyMessage [Image]
      Txt_SystemMenuEmptyMessage [TextMeshProUGUI]
  Grp_SystemMenuErrorState [无Graphic]
    Panel_SystemMenuErrorMessage [Image]
      Txt_SystemMenuErrorMessage [TextMeshProUGUI]
  Grp_SystemMenuSuccessState [无Graphic]
    Panel_SystemMenuSuccessMessage [Image]
      Txt_SystemMenuSuccessMessage [TextMeshProUGUI]
  Grp_SystemMenuDisabledState [无Graphic]
    Panel_SystemMenuDisabledMessage [Image]
      Txt_SystemMenuDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageSystemMenu | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；游戏内系统菜单；内部页面根 |
| Txt_SystemMenuTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(672,32); pos(8,0) | absolute | TextMeshProUGUI；游戏内系统菜单 |
| Grp_SystemMenuActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=3 cell=(218,48) spacing=(8,8)；操作区 |
| Btn_SystemMenuResume | min(0,1) max(0,1); pivot(0,1); sizeDelta(218,48)初始化; pos(0,0)初始化; LayoutElement preferred(218,48); 最终位置/尺寸由组驱动 | group | Button + Image；关闭管理层并恢复仍有效的世界选择 |
| Txt_SystemMenuResumeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回游戏 |
| Btn_SystemMenuSettings | min(0,1) max(0,1); pivot(0,1); sizeDelta(218,48)初始化; pos(0,0)初始化; LayoutElement preferred(218,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入02-声音设置，返回时回系统菜单 |
| Txt_SystemMenuSettingsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；设置 |
| Btn_SystemMenuHelp | min(0,1) max(0,1); pivot(0,1); sizeDelta(218,48)初始化; pos(0,0)初始化; LayoutElement preferred(218,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开16-帮助 |
| Txt_SystemMenuHelpLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；帮助 |
| Btn_SystemMenuReturn | min(0,1) max(0,1); pivot(0,1); sizeDelta(218,48)初始化; pos(0,0)初始化; LayoutElement preferred(218,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入02-退出处理，目标为主菜单 |
| Txt_SystemMenuReturnLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保存并返回主菜单 |
| Btn_SystemMenuQuit | min(0,1) max(0,1); pivot(0,1); sizeDelta(218,48)初始化; pos(0,0)初始化; LayoutElement preferred(218,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入02-退出处理，目标为桌面 |
| Txt_SystemMenuQuitLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；保存并退出 |
| Panel_SystemMenuSession | min(0,1) max(0,1); pivot(0,1); sizeDelta(336,518); pos(0,-36) | absolute | Image；当前进度 |
| Txt_SystemMenuSessionHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(312,32); pos(12,-8) | absolute | TextMeshProUGUI；当前进度 |
| List_SystemMenuSession | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SystemMenuSession | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SystemMenuSession | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SystemMenuSessionBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(312,120)初始化; pos(0,0)初始化; LayoutElement preferred(312,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；槽位摘要；最近成功保存时间；自动保存状态 |
| Item_SystemMenuSessionTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(312,104)初始化; pos(0,0)初始化; LayoutElement preferred(312,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SystemMenuSessionRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SystemMenuSessionRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SystemMenuSessionRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_SystemMenuCommands | min(0,1) max(0,1); pivot(0,1); sizeDelta(336,518); pos(352,-36) | absolute | Image；系统操作说明 |
| Txt_SystemMenuCommandsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(312,32); pos(12,-8) | absolute | TextMeshProUGUI；系统操作说明 |
| List_SystemMenuCommands | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SystemMenuCommands | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SystemMenuCommands | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SystemMenuCommandsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(312,120)初始化; pos(0,0)初始化; LayoutElement preferred(312,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；打开菜单时世界继续运行；返回和退出会等待安全快照与写入 |
| Item_SystemMenuCommandsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(312,104)初始化; pos(0,0)初始化; LayoutElement preferred(312,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SystemMenuCommandsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SystemMenuCommandsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SystemMenuCommandsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_SystemMenuLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(688,518); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemMenuLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemMenuLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_SystemMenuEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(688,518); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemMenuEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemMenuEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_SystemMenuErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(688,518); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemMenuErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemMenuErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_SystemMenuSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(688,518); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemMenuSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemMenuSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_SystemMenuDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(688,518); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SystemMenuDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SystemMenuDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## AudioSettings：声音设置

功能文档：[声音设置](AudioSettings.md)；归属 `SettingsForm`；内容 1488×730。

```text
Panel_PageAudioSettings [Image]
  Txt_AudioSettingsTitle [TextMeshProUGUI]
  Grp_AudioSettingsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_AudioSettingsAdjust [Button + Image]
      Txt_AudioSettingsAdjustLabel [TextMeshProUGUI]
    Btn_AudioSettingsReset [Button + Image]
      Txt_AudioSettingsResetLabel [TextMeshProUGUI]
    Btn_AudioSettingsTabs [Button + Image]
      Txt_AudioSettingsTabsLabel [TextMeshProUGUI]
  Panel_AudioSettingsVolumes [Image]
    Txt_AudioSettingsVolumesHeading [TextMeshProUGUI]
    List_AudioSettingsVolumes [ScrollRect vertical=true horizontal=false]
      Viewport_AudioSettingsVolumes [RectMask2D]
        Content_AudioSettingsVolumes [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AudioSettingsVolumesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AudioSettingsVolumesTemplate [LayoutElement + Image；默认inactive]
            Btn_AudioSettingsVolumesRow [Button + Image]
              Txt_AudioSettingsVolumesRowLabel [TextMeshProUGUI]
              Txt_AudioSettingsVolumesRowValue [TextMeshProUGUI]
          Panel_AudioSettingsControls [Image + LayoutElement]
            Sld_AudioSettingsMainVolume [Slider]
              Bg_AudioSettingsMainVolumeTrack [Image]
              Bar_AudioSettingsMainVolumeFill [Image(Filled)]
              Img_AudioSettingsMainVolumeHandle [Image]
              Txt_AudioSettingsMainVolumeValue [TextMeshProUGUI]
            Sld_AudioSettingsMusicVolume [Slider]
              Bg_AudioSettingsMusicVolumeTrack [Image]
              Bar_AudioSettingsMusicVolumeFill [Image(Filled)]
              Img_AudioSettingsMusicVolumeHandle [Image]
              Txt_AudioSettingsMusicVolumeValue [TextMeshProUGUI]
            Sld_AudioSettingsAmbientVolume [Slider]
              Bg_AudioSettingsAmbientVolumeTrack [Image]
              Bar_AudioSettingsAmbientVolumeFill [Image(Filled)]
              Img_AudioSettingsAmbientVolumeHandle [Image]
              Txt_AudioSettingsAmbientVolumeValue [TextMeshProUGUI]
            Sld_AudioSettingsMachineVolume [Slider]
              Bg_AudioSettingsMachineVolumeTrack [Image]
              Bar_AudioSettingsMachineVolumeFill [Image(Filled)]
              Img_AudioSettingsMachineVolumeHandle [Image]
              Txt_AudioSettingsMachineVolumeValue [TextMeshProUGUI]
            Sld_AudioSettingsUiVolume [Slider]
              Bg_AudioSettingsUiVolumeTrack [Image]
              Bar_AudioSettingsUiVolumeFill [Image(Filled)]
              Img_AudioSettingsUiVolumeHandle [Image]
              Txt_AudioSettingsUiVolumeValue [TextMeshProUGUI]
  Panel_AudioSettingsPersistence [Image]
    Txt_AudioSettingsPersistenceHeading [TextMeshProUGUI]
    List_AudioSettingsPersistence [ScrollRect vertical=true horizontal=false]
      Viewport_AudioSettingsPersistence [RectMask2D]
        Content_AudioSettingsPersistence [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_AudioSettingsPersistenceBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_AudioSettingsPersistenceTemplate [LayoutElement + Image；默认inactive]
            Btn_AudioSettingsPersistenceRow [Button + Image]
              Txt_AudioSettingsPersistenceRowLabel [TextMeshProUGUI]
              Txt_AudioSettingsPersistenceRowValue [TextMeshProUGUI]
  Grp_AudioSettingsLoadingState [无Graphic]
    Panel_AudioSettingsLoadingMessage [Image]
      Txt_AudioSettingsLoadingMessage [TextMeshProUGUI]
  Grp_AudioSettingsEmptyState [无Graphic]
    Panel_AudioSettingsEmptyMessage [Image]
      Txt_AudioSettingsEmptyMessage [TextMeshProUGUI]
  Grp_AudioSettingsErrorState [无Graphic]
    Panel_AudioSettingsErrorMessage [Image]
      Txt_AudioSettingsErrorMessage [TextMeshProUGUI]
  Grp_AudioSettingsSuccessState [无Graphic]
    Panel_AudioSettingsSuccessMessage [Image]
      Txt_AudioSettingsSuccessMessage [TextMeshProUGUI]
  Grp_AudioSettingsDisabledState [无Graphic]
    Panel_AudioSettingsDisabledMessage [Image]
      Txt_AudioSettingsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageAudioSettings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；声音设置；内部页面根 |
| Txt_AudioSettingsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；声音设置 |
| Grp_AudioSettingsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_AudioSettingsAdjust | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；五个独立Sld控件修改对应值并立即预听；严重警报也服从音量但保留视觉反馈 |
| Txt_AudioSettingsAdjustLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；调整音量 |
| Btn_AudioSettingsReset | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只恢复声音分页；保留其它分页设置 |
| Txt_AudioSettingsResetLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；恢复本页默认 |
| Btn_AudioSettingsTabs | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入操作或显示与性能；保留本页焦点 |
| Txt_AudioSettingsTabsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换设置分页 |
| Panel_AudioSettingsVolumes | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(0,-36) | absolute | Image；音量 |
| Txt_AudioSettingsVolumesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；音量 |
| List_AudioSettingsVolumes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AudioSettingsVolumes | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AudioSettingsVolumes | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AudioSettingsVolumesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；主音量；音乐；环境；机器与生产；UI与警报。各自0%—100%，滑条旁显示百分比 |
| Item_AudioSettingsVolumesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AudioSettingsVolumesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AudioSettingsVolumesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AudioSettingsVolumesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_AudioSettingsControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,376)初始化; pos(0,0)初始化; LayoutElement preferred(712,376); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Sld_AudioSettingsMainVolume | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-8) | absolute | Slider；主音量 |
| Bg_AudioSettingsMainVolumeTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_AudioSettingsMainVolumeFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_AudioSettingsMainVolumeHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_AudioSettingsMainVolumeValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；主音量：— |
| Sld_AudioSettingsMusicVolume | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-80) | absolute | Slider；音乐 |
| Bg_AudioSettingsMusicVolumeTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_AudioSettingsMusicVolumeFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_AudioSettingsMusicVolumeHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_AudioSettingsMusicVolumeValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；音乐：— |
| Sld_AudioSettingsAmbientVolume | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-152) | absolute | Slider；环境 |
| Bg_AudioSettingsAmbientVolumeTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_AudioSettingsAmbientVolumeFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_AudioSettingsAmbientVolumeHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_AudioSettingsAmbientVolumeValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；环境：— |
| Sld_AudioSettingsMachineVolume | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-224) | absolute | Slider；机器与生产 |
| Bg_AudioSettingsMachineVolumeTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_AudioSettingsMachineVolumeFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_AudioSettingsMachineVolumeHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_AudioSettingsMachineVolumeValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；机器与生产：— |
| Sld_AudioSettingsUiVolume | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-296) | absolute | Slider；UI与警报 |
| Bg_AudioSettingsUiVolumeTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_AudioSettingsUiVolumeFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_AudioSettingsUiVolumeHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_AudioSettingsUiVolumeValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；UI与警报：— |
| Panel_AudioSettingsPersistence | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(752,-36) | absolute | Image；本机配置状态 |
| Txt_AudioSettingsPersistenceHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；本机配置状态 |
| List_AudioSettingsPersistence | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_AudioSettingsPersistence | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_AudioSettingsPersistence | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_AudioSettingsPersistenceBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；立即生效；保存成功或持久化失败；当前设置与默认值的区别 |
| Item_AudioSettingsPersistenceTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_AudioSettingsPersistenceRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_AudioSettingsPersistenceRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_AudioSettingsPersistenceRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_AudioSettingsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AudioSettingsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AudioSettingsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_AudioSettingsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AudioSettingsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AudioSettingsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_AudioSettingsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AudioSettingsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AudioSettingsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_AudioSettingsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AudioSettingsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AudioSettingsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_AudioSettingsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_AudioSettingsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_AudioSettingsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## ControlSettings：操作设置

功能文档：[操作设置](ControlSettings.md)；归属 `SettingsForm`；内容 1488×730。

```text
Panel_PageControlSettings [Image]
  Txt_ControlSettingsTitle [TextMeshProUGUI]
  Grp_ControlSettingsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_ControlSettingsAdjust [Button + Image]
      Txt_ControlSettingsAdjustLabel [TextMeshProUGUI]
    Btn_ControlSettingsReset [Button + Image]
      Txt_ControlSettingsResetLabel [TextMeshProUGUI]
    Btn_ControlSettingsTabs [Button + Image]
      Txt_ControlSettingsTabsLabel [TextMeshProUGUI]
  Panel_ControlSettingsCamera [Image]
    Txt_ControlSettingsCameraHeading [TextMeshProUGUI]
    List_ControlSettingsCamera [ScrollRect vertical=true horizontal=false]
      Viewport_ControlSettingsCamera [RectMask2D]
        Content_ControlSettingsCamera [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ControlSettingsCameraBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ControlSettingsCameraTemplate [LayoutElement + Image；默认inactive]
            Btn_ControlSettingsCameraRow [Button + Image]
              Txt_ControlSettingsCameraRowLabel [TextMeshProUGUI]
              Txt_ControlSettingsCameraRowValue [TextMeshProUGUI]
          Panel_ControlSettingsControls [Image + LayoutElement]
            Sld_ControlSettingsPanSpeed [Slider]
              Bg_ControlSettingsPanSpeedTrack [Image]
              Bar_ControlSettingsPanSpeedFill [Image(Filled)]
              Img_ControlSettingsPanSpeedHandle [Image]
              Txt_ControlSettingsPanSpeedValue [TextMeshProUGUI]
            Sld_ControlSettingsRotateSpeed [Slider]
              Bg_ControlSettingsRotateSpeedTrack [Image]
              Bar_ControlSettingsRotateSpeedFill [Image(Filled)]
              Img_ControlSettingsRotateSpeedHandle [Image]
              Txt_ControlSettingsRotateSpeedValue [TextMeshProUGUI]
            Sld_ControlSettingsZoomSpeed [Slider]
              Bg_ControlSettingsZoomSpeedTrack [Image]
              Bar_ControlSettingsZoomSpeedFill [Image(Filled)]
              Img_ControlSettingsZoomSpeedHandle [Image]
              Txt_ControlSettingsZoomSpeedValue [TextMeshProUGUI]
            Tgl_ControlSettingsInvertHorizontal [Toggle]
              Img_ControlSettingsInvertHorizontalBox [Image]
                Icon_ControlSettingsInvertHorizontalCheck [Image]
              Txt_ControlSettingsInvertHorizontalLabel [TextMeshProUGUI]
            Tgl_ControlSettingsInvertVertical [Toggle]
              Img_ControlSettingsInvertVerticalBox [Image]
                Icon_ControlSettingsInvertVerticalCheck [Image]
              Txt_ControlSettingsInvertVerticalLabel [TextMeshProUGUI]
  Panel_ControlSettingsBindings [Image]
    Txt_ControlSettingsBindingsHeading [TextMeshProUGUI]
    List_ControlSettingsBindings [ScrollRect vertical=true horizontal=false]
      Viewport_ControlSettingsBindings [RectMask2D]
        Content_ControlSettingsBindings [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ControlSettingsBindingsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ControlSettingsBindingsTemplate [LayoutElement + Image；默认inactive]
            Btn_ControlSettingsBindingsRow [Button + Image]
              Txt_ControlSettingsBindingsRowLabel [TextMeshProUGUI]
              Txt_ControlSettingsBindingsRowValue [TextMeshProUGUI]
  Grp_ControlSettingsLoadingState [无Graphic]
    Panel_ControlSettingsLoadingMessage [Image]
      Txt_ControlSettingsLoadingMessage [TextMeshProUGUI]
  Grp_ControlSettingsEmptyState [无Graphic]
    Panel_ControlSettingsEmptyMessage [Image]
      Txt_ControlSettingsEmptyMessage [TextMeshProUGUI]
  Grp_ControlSettingsErrorState [无Graphic]
    Panel_ControlSettingsErrorMessage [Image]
      Txt_ControlSettingsErrorMessage [TextMeshProUGUI]
  Grp_ControlSettingsSuccessState [无Graphic]
    Panel_ControlSettingsSuccessMessage [Image]
      Txt_ControlSettingsSuccessMessage [TextMeshProUGUI]
  Grp_ControlSettingsDisabledState [无Graphic]
    Panel_ControlSettingsDisabledMessage [Image]
      Txt_ControlSettingsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageControlSettings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；操作设置；内部页面根 |
| Txt_ControlSettingsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；操作设置 |
| Grp_ControlSettingsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_ControlSettingsAdjust | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；滑条即时应用配置定义的合法区间；反转使用独立Toggle |
| Txt_ControlSettingsAdjustLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；调整镜头参数 |
| Btn_ControlSettingsReset | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只恢复操作参数 |
| Txt_ControlSettingsResetLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；恢复本页默认 |
| Btn_ControlSettingsTabs | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；返回声音或进入显示与性能 |
| Txt_ControlSettingsTabsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换设置分页 |
| Panel_ControlSettingsCamera | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(0,-36) | absolute | Image；镜头参数 |
| Txt_ControlSettingsCameraHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；镜头参数 |
| List_ControlSettingsCamera | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ControlSettingsCamera | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ControlSettingsCamera | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ControlSettingsCameraBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；平移速度；旋转速度／灵敏度；缩放速度；水平反转；垂直反转 |
| Item_ControlSettingsCameraTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ControlSettingsCameraRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ControlSettingsCameraRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ControlSettingsCameraRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ControlSettingsControls | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,376)初始化; pos(0,0)初始化; LayoutElement preferred(712,376); 最终位置/尺寸由组驱动 | group | Image + LayoutElement；真实输入字段 |
| Sld_ControlSettingsPanSpeed | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-8) | absolute | Slider；平移速度 |
| Bg_ControlSettingsPanSpeedTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_ControlSettingsPanSpeedFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_ControlSettingsPanSpeedHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_ControlSettingsPanSpeedValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；平移速度：— |
| Sld_ControlSettingsRotateSpeed | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-80) | absolute | Slider；旋转速度／灵敏度 |
| Bg_ControlSettingsRotateSpeedTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_ControlSettingsRotateSpeedFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_ControlSettingsRotateSpeedHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_ControlSettingsRotateSpeedValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；旋转速度／灵敏度：— |
| Sld_ControlSettingsZoomSpeed | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,56); pos(16,-152) | absolute | Slider；滚轮缩放速度 |
| Bg_ControlSettingsZoomSpeedTrack | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image；轨道 |
| Bar_ControlSettingsZoomSpeedFill | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,8); pos(0,8) | absolute | Image(Filled)；Slider.fillRect驱动 |
| Img_ControlSettingsZoomSpeedHandle | min(0,0) max(0,0); pivot(0.5,0.5); sizeDelta(24,24); pos(12,12) | absolute | Image；Slider.handleRect驱动位置 |
| Txt_ControlSettingsZoomSpeedValue | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,28); pos(0,0) | absolute | TextMeshProUGUI；滚轮缩放速度：— |
| Tgl_ControlSettingsInvertHorizontal | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,48); pos(16,-224) | absolute | Toggle；水平反转 |
| Img_ControlSettingsInvertHorizontalBox | min(0,1) max(0,1); pivot(0,1); sizeDelta(32,32); pos(0,-8) | absolute | Image；开关背景 |
| Icon_ControlSettingsInvertHorizontalCheck | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；Toggle.graphic |
| Txt_ControlSettingsInvertHorizontalLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-48,0); pos(24,0) | absolute | TextMeshProUGUI；水平反转 |
| Tgl_ControlSettingsInvertVertical | min(0,1) max(0,1); pivot(0,1); sizeDelta(680,48); pos(16,-296) | absolute | Toggle；垂直反转 |
| Img_ControlSettingsInvertVerticalBox | min(0,1) max(0,1); pivot(0,1); sizeDelta(32,32); pos(0,-8) | absolute | Image；开关背景 |
| Icon_ControlSettingsInvertVerticalCheck | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；Toggle.graphic |
| Txt_ControlSettingsInvertVerticalLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-48,0); pos(24,0) | absolute | TextMeshProUGUI；垂直反转 |
| Panel_ControlSettingsBindings | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(752,-36) | absolute | Image；当前按键 |
| Txt_ControlSettingsBindingsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；当前按键 |
| List_ControlSettingsBindings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ControlSettingsBindings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ControlSettingsBindings | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ControlSettingsBindingsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；InputModule实际映射：移动、旋转、缩放、聚焦、返回、放置旋转；只读显示 |
| Item_ControlSettingsBindingsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ControlSettingsBindingsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ControlSettingsBindingsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ControlSettingsBindingsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ControlSettingsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ControlSettingsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ControlSettingsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ControlSettingsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ControlSettingsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ControlSettingsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ControlSettingsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ControlSettingsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ControlSettingsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ControlSettingsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ControlSettingsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ControlSettingsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ControlSettingsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ControlSettingsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ControlSettingsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## DisplaySettings：显示与性能设置

功能文档：[显示与性能设置](DisplaySettings.md)；归属 `SettingsForm`；内容 1488×730。

```text
Panel_PageDisplaySettings [Image]
  Txt_DisplaySettingsTitle [TextMeshProUGUI]
  Grp_DisplaySettingsActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_DisplaySettingsMode [Button + Image]
      Txt_DisplaySettingsModeLabel [TextMeshProUGUI]
    Btn_DisplaySettingsQuality [Button + Image]
      Txt_DisplaySettingsQualityLabel [TextMeshProUGUI]
    Btn_DisplaySettingsReset [Button + Image]
      Txt_DisplaySettingsResetLabel [TextMeshProUGUI]
    Btn_DisplaySettingsTabs [Button + Image]
      Txt_DisplaySettingsTabsLabel [TextMeshProUGUI]
  Panel_DisplaySettingsDisplay [Image]
    Txt_DisplaySettingsDisplayHeading [TextMeshProUGUI]
    List_DisplaySettingsDisplay [ScrollRect vertical=true horizontal=false]
      Viewport_DisplaySettingsDisplay [RectMask2D]
        Content_DisplaySettingsDisplay [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DisplaySettingsDisplayBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DisplaySettingsDisplayTemplate [LayoutElement + Image；默认inactive]
            Btn_DisplaySettingsDisplayRow [Button + Image]
              Txt_DisplaySettingsDisplayRowLabel [TextMeshProUGUI]
              Txt_DisplaySettingsDisplayRowValue [TextMeshProUGUI]
          Grp_DisplaySettingsWindowModeChoices [VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112]
            Btn_DisplaySettingsWindowModeFullscreen [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsWindowModeFullscreenLabel [TextMeshProUGUI]
            Btn_DisplaySettingsWindowModeBorderless [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsWindowModeBorderlessLabel [TextMeshProUGUI]
          Grp_DisplaySettingsVSyncChoices [VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112]
            Btn_DisplaySettingsVSyncEnabled [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsVSyncEnabledLabel [TextMeshProUGUI]
            Btn_DisplaySettingsVSyncDisabled [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsVSyncDisabledLabel [TextMeshProUGUI]
          Grp_DisplaySettingsFrameLimitChoices [VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112]
            Btn_DisplaySettingsFrameLimitThirty [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsFrameLimitThirtyLabel [TextMeshProUGUI]
            Btn_DisplaySettingsFrameLimitSixty [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsFrameLimitSixtyLabel [TextMeshProUGUI]
            Btn_DisplaySettingsFrameLimitUnlimited [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsFrameLimitUnlimitedLabel [TextMeshProUGUI]
  Panel_DisplaySettingsQuality [Image]
    Txt_DisplaySettingsQualityHeading [TextMeshProUGUI]
    List_DisplaySettingsQuality [ScrollRect vertical=true horizontal=false]
      Viewport_DisplaySettingsQuality [RectMask2D]
        Content_DisplaySettingsQuality [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_DisplaySettingsQualityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_DisplaySettingsQualityTemplate [LayoutElement + Image；默认inactive]
            Btn_DisplaySettingsQualityRow [Button + Image]
              Txt_DisplaySettingsQualityRowLabel [TextMeshProUGUI]
              Txt_DisplaySettingsQualityRowValue [TextMeshProUGUI]
          Grp_DisplaySettingsPresetChoices [VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112]
            Btn_DisplaySettingsPresetLow [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsPresetLowLabel [TextMeshProUGUI]
            Btn_DisplaySettingsPresetMedium [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsPresetMediumLabel [TextMeshProUGUI]
            Btn_DisplaySettingsPresetHigh [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsPresetHighLabel [TextMeshProUGUI]
          Grp_DisplaySettingsShadowChoices [VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112]
            Btn_DisplaySettingsShadowLow [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsShadowLowLabel [TextMeshProUGUI]
            Btn_DisplaySettingsShadowMedium [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsShadowMediumLabel [TextMeshProUGUI]
            Btn_DisplaySettingsShadowHigh [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsShadowHighLabel [TextMeshProUGUI]
          Grp_DisplaySettingsAntialiasingChoices [VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112]
            Btn_DisplaySettingsAntialiasingLow [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsAntialiasingLowLabel [TextMeshProUGUI]
            Btn_DisplaySettingsAntialiasingMedium [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsAntialiasingMediumLabel [TextMeshProUGUI]
            Btn_DisplaySettingsAntialiasingHigh [Button + Image + LayoutElement preferredHeight=32]
              Txt_DisplaySettingsAntialiasingHighLabel [TextMeshProUGUI]
  Grp_DisplaySettingsLoadingState [无Graphic]
    Panel_DisplaySettingsLoadingMessage [Image]
      Txt_DisplaySettingsLoadingMessage [TextMeshProUGUI]
  Grp_DisplaySettingsEmptyState [无Graphic]
    Panel_DisplaySettingsEmptyMessage [Image]
      Txt_DisplaySettingsEmptyMessage [TextMeshProUGUI]
  Grp_DisplaySettingsErrorState [无Graphic]
    Panel_DisplaySettingsErrorMessage [Image]
      Txt_DisplaySettingsErrorMessage [TextMeshProUGUI]
  Grp_DisplaySettingsSuccessState [无Graphic]
    Panel_DisplaySettingsSuccessMessage [Image]
      Txt_DisplaySettingsSuccessMessage [TextMeshProUGUI]
  Grp_DisplaySettingsDisabledState [无Graphic]
    Panel_DisplaySettingsDisabledMessage [Image]
      Txt_DisplaySettingsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageDisplaySettings | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；显示与性能设置；内部页面根 |
| Txt_DisplaySettingsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；显示与性能设置 |
| Grp_DisplaySettingsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_DisplaySettingsMode | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；选择模式后临时应用并打开17-显示保留恢复；超时回滚 |
| Txt_DisplaySettingsModeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换显示模式 |
| Btn_DisplaySettingsQuality | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；使用档位按钮和Toggle即时应用，相关受限项显示原因 |
| Txt_DisplaySettingsQualityLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；修改质量 |
| Btn_DisplaySettingsReset | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；只恢复显示与性能；若模式变化仍需保留／恢复确认 |
| Txt_DisplaySettingsResetLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；恢复本页默认 |
| Btn_DisplaySettingsTabs | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；切至声音或操作 |
| Txt_DisplaySettingsTabsLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；切换设置分页 |
| Panel_DisplaySettingsDisplay | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(0,-36) | absolute | Image；显示模式 |
| Txt_DisplaySettingsDisplayHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；显示模式 |
| List_DisplaySettingsDisplay | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DisplaySettingsDisplay | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DisplaySettingsDisplay | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DisplaySettingsDisplayBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；全屏／无边框全屏；垂直同步；30／60／不限帧率 |
| Item_DisplaySettingsDisplayTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DisplaySettingsDisplayRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DisplaySettingsDisplayRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DisplaySettingsDisplayRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_DisplaySettingsQuality | min(0,1) max(0,1); pivot(0,1); sizeDelta(736,634); pos(752,-36) | absolute | Image；质量设置 |
| Txt_DisplaySettingsQualityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,32); pos(12,-8) | absolute | TextMeshProUGUI；质量设置 |
| List_DisplaySettingsQuality | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_DisplaySettingsQuality | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_DisplaySettingsQuality | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_DisplaySettingsQualityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,120)初始化; pos(0,0)初始化; LayoutElement preferred(712,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；低／中／高画质；阴影档位；抗锯齿档位；当前生效项与持久化状态 |
| Item_DisplaySettingsQualityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(712,104)初始化; pos(0,0)初始化; LayoutElement preferred(712,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_DisplaySettingsQualityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_DisplaySettingsQualityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_DisplaySettingsQualityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_DisplaySettingsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplaySettingsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplaySettingsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_DisplaySettingsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplaySettingsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplaySettingsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_DisplaySettingsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplaySettingsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplaySettingsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_DisplaySettingsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplaySettingsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplaySettingsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_DisplaySettingsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_DisplaySettingsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_DisplaySettingsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
| Grp_DisplaySettingsWindowModeChoices | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,112)初始化; pos(0,0)初始化 | group | VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112；显示模式；父Content驱动位置和宽度 |
| Btn_DisplaySettingsWindowModeFullscreen | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；显示模式=全屏；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsWindowModeFullscreenLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；全屏 |
| Btn_DisplaySettingsWindowModeBorderless | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；显示模式=无边框全屏；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsWindowModeBorderlessLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；无边框全屏 |
| Grp_DisplaySettingsVSyncChoices | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,112)初始化; pos(0,0)初始化 | group | VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112；垂直同步；父Content驱动位置和宽度 |
| Btn_DisplaySettingsVSyncEnabled | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；垂直同步=开启；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsVSyncEnabledLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；开启 |
| Btn_DisplaySettingsVSyncDisabled | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；垂直同步=关闭；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsVSyncDisabledLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；关闭 |
| Grp_DisplaySettingsFrameLimitChoices | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,112)初始化; pos(0,0)初始化 | group | VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112；帧率；父Content驱动位置和宽度 |
| Btn_DisplaySettingsFrameLimitThirty | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；帧率=30；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsFrameLimitThirtyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；30 |
| Btn_DisplaySettingsFrameLimitSixty | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；帧率=60；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsFrameLimitSixtyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；60 |
| Btn_DisplaySettingsFrameLimitUnlimited | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；帧率=不限；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsFrameLimitUnlimitedLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；不限 |
| Grp_DisplaySettingsPresetChoices | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,112)初始化; pos(0,0)初始化 | group | VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112；画质；父Content驱动位置和宽度 |
| Btn_DisplaySettingsPresetLow | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；画质=低；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsPresetLowLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；低 |
| Btn_DisplaySettingsPresetMedium | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；画质=中；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsPresetMediumLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；中 |
| Btn_DisplaySettingsPresetHigh | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；画质=高；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsPresetHighLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；高 |
| Grp_DisplaySettingsShadowChoices | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,112)初始化; pos(0,0)初始化 | group | VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112；阴影；父Content驱动位置和宽度 |
| Btn_DisplaySettingsShadowLow | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；阴影=低；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsShadowLowLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；低 |
| Btn_DisplaySettingsShadowMedium | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；阴影=中；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsShadowMediumLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；中 |
| Btn_DisplaySettingsShadowHigh | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；阴影=高；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsShadowHighLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；高 |
| Grp_DisplaySettingsAntialiasingChoices | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,112)初始化; pos(0,0)初始化 | group | VerticalLayoutGroup spacing=4 + LayoutElement preferredHeight=112；抗锯齿；父Content驱动位置和宽度 |
| Btn_DisplaySettingsAntialiasingLow | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；抗锯齿=低；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsAntialiasingLowLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；低 |
| Btn_DisplaySettingsAntialiasingMedium | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；抗锯齿=中；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsAntialiasingMediumLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；中 |
| Btn_DisplaySettingsAntialiasingHigh | min(0,1) max(0,1); pivot(0,1); sizeDelta(700,32)初始化; pos(0,0)初始化 | group | Button + Image + LayoutElement preferredHeight=32；抗锯齿=高；onClick→Mode或Quality意图；质量映射取实际配置 |
| Txt_DisplaySettingsAntialiasingHighLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | TextMeshProUGUI；高 |

## ExitFlow：保存返回与退出失败处理

功能文档：[保存返回与退出失败处理](ExitFlow.md)；归属 `ExitFlowForm`；内容 848×630。

```text
Panel_PageExitFlow [Image]
  Txt_ExitFlowTitle [TextMeshProUGUI]
  Grp_ExitFlowActions [GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)]
    Btn_ExitFlowRetry [Button + Image]
      Txt_ExitFlowRetryLabel [TextMeshProUGUI]
    Btn_ExitFlowResume [Button + Image]
      Txt_ExitFlowResumeLabel [TextMeshProUGUI]
    Btn_ExitFlowForce [Button + Image]
      Txt_ExitFlowForceLabel [TextMeshProUGUI]
  Panel_ExitFlowStage [Image]
    Txt_ExitFlowStageHeading [TextMeshProUGUI]
    List_ExitFlowStage [ScrollRect vertical=true horizontal=false]
      Viewport_ExitFlowStage [RectMask2D]
        Content_ExitFlowStage [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ExitFlowStageBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ExitFlowStageTemplate [LayoutElement + Image；默认inactive]
            Btn_ExitFlowStageRow [Button + Image]
              Txt_ExitFlowStageRowLabel [TextMeshProUGUI]
              Txt_ExitFlowStageRowValue [TextMeshProUGUI]
  Panel_ExitFlowFailure [Image]
    Txt_ExitFlowFailureHeading [TextMeshProUGUI]
    List_ExitFlowFailure [ScrollRect vertical=true horizontal=false]
      Viewport_ExitFlowFailure [RectMask2D]
        Content_ExitFlowFailure [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ExitFlowFailureBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ExitFlowFailureTemplate [LayoutElement + Image；默认inactive]
            Btn_ExitFlowFailureRow [Button + Image]
              Txt_ExitFlowFailureRowLabel [TextMeshProUGUI]
              Txt_ExitFlowFailureRowValue [TextMeshProUGUI]
  Grp_ExitFlowLoadingState [无Graphic]
    Panel_ExitFlowLoadingMessage [Image]
      Txt_ExitFlowLoadingMessage [TextMeshProUGUI]
  Grp_ExitFlowEmptyState [无Graphic]
    Panel_ExitFlowEmptyMessage [Image]
      Txt_ExitFlowEmptyMessage [TextMeshProUGUI]
  Grp_ExitFlowErrorState [无Graphic]
    Panel_ExitFlowErrorMessage [Image]
      Txt_ExitFlowErrorMessage [TextMeshProUGUI]
  Grp_ExitFlowSuccessState [无Graphic]
    Panel_ExitFlowSuccessMessage [Image]
      Txt_ExitFlowSuccessMessage [TextMeshProUGUI]
  Grp_ExitFlowDisabledState [无Graphic]
    Panel_ExitFlowDisabledMessage [Image]
      Txt_ExitFlowDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageExitFlow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；保存返回与退出失败处理；内部页面根 |
| Txt_ExitFlowTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(832,32); pos(8,0) | absolute | TextMeshProUGUI；保存返回与退出失败处理 |
| Grp_ExitFlowActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=4 cell=(202,48) spacing=(8,8)；操作区 |
| Btn_ExitFlowRetry | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；复用本次退出请求，防止重复提交；成功后到原定目标 |
| Txt_ExitFlowRetryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重试保存 |
| Btn_ExitFlowResume | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；失败时恢复模拟和输入，保留未保存脏状态 |
| Txt_ExitFlowResumeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回游戏 |
| Btn_ExitFlowForce | min(0,1) max(0,1); pivot(0,1); sizeDelta(202,48)初始化; pos(0,0)初始化; LayoutElement preferred(202,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅失败后可选；进入17-强退确认；确认后按原定退出目标处理 |
| Txt_ExitFlowForceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；强制退出 |
| Panel_ExitFlowStage | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(0,-36) | absolute | Image；退出阶段 |
| Txt_ExitFlowStageHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；退出阶段 |
| List_ExitFlowStage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ExitFlowStage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ExitFlowStage | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ExitFlowStageBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；停止新玩家输入→等待安全快照→停止模拟→写入与校验；退出目标 |
| Item_ExitFlowStageTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ExitFlowStageRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ExitFlowStageRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ExitFlowStageRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ExitFlowFailure | min(0,1) max(0,1); pivot(0,1); sizeDelta(416,534); pos(432,-36) | absolute | Image；失败详情 |
| Txt_ExitFlowFailureHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,32); pos(12,-8) | absolute | TextMeshProUGUI；失败详情 |
| List_ExitFlowFailure | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ExitFlowFailure | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ExitFlowFailure | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ExitFlowFailureBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,120)初始化; pos(0,0)初始化; LayoutElement preferred(392,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；最近成功保存时间；本次失败原因；重试、返回游戏与强退后果 |
| Item_ExitFlowFailureTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(392,104)初始化; pos(0,0)初始化; LayoutElement preferred(392,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ExitFlowFailureRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ExitFlowFailureRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ExitFlowFailureRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ExitFlowLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ExitFlowLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ExitFlowLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ExitFlowEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ExitFlowEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ExitFlowEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ExitFlowErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ExitFlowErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ExitFlowErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ExitFlowSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ExitFlowSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ExitFlowSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ExitFlowDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(848,534); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ExitFlowDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ExitFlowDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
