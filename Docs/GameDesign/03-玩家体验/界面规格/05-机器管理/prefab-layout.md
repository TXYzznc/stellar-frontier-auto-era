# 05-机器管理 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## MachineOverview：机器现场概况

功能文档：[机器现场概况](MachineOverview.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageMachineOverview [Image]
  Txt_MachineOverviewTitle [TextMeshProUGUI]
  Grp_MachineOverviewActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_MachineOverviewActivate [Button + Image]
      Txt_MachineOverviewActivateLabel [TextMeshProUGUI]
    Btn_MachineOverviewSleep [Button + Image]
      Txt_MachineOverviewSleepLabel [TextMeshProUGUI]
    Btn_MachineOverviewPower [Button + Image]
      Txt_MachineOverviewPowerLabel [TextMeshProUGUI]
    Btn_MachineOverviewRename [Button + Image]
      Txt_MachineOverviewRenameLabel [TextMeshProUGUI]
    Btn_MachineOverviewFocus [Button + Image]
      Txt_MachineOverviewFocusLabel [TextMeshProUGUI]
    Btn_MachineOverviewHardware [Button + Image]
      Txt_MachineOverviewHardwareLabel [TextMeshProUGUI]
    Btn_MachineOverviewAlgorithm [Button + Image]
      Txt_MachineOverviewAlgorithmLabel [TextMeshProUGUI]
    Btn_MachineOverviewDiagnostic [Button + Image]
      Txt_MachineOverviewDiagnosticLabel [TextMeshProUGUI]
    Btn_MachineOverviewRecover [Button + Image]
      Txt_MachineOverviewRecoverLabel [TextMeshProUGUI]
  Panel_MachineOverviewIdentity [Image]
    Txt_MachineOverviewIdentityHeading [TextMeshProUGUI]
    List_MachineOverviewIdentity [ScrollRect vertical=true horizontal=false]
      Viewport_MachineOverviewIdentity [RectMask2D]
        Content_MachineOverviewIdentity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineOverviewIdentityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineOverviewIdentityTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineOverviewIdentityRow [Button + Image]
              Txt_MachineOverviewIdentityRowLabel [TextMeshProUGUI]
              Txt_MachineOverviewIdentityRowValue [TextMeshProUGUI]
  Panel_MachineOverviewCapacity [Image]
    Txt_MachineOverviewCapacityHeading [TextMeshProUGUI]
    List_MachineOverviewCapacity [ScrollRect vertical=true horizontal=false]
      Viewport_MachineOverviewCapacity [RectMask2D]
        Content_MachineOverviewCapacity [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineOverviewCapacityBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineOverviewCapacityTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineOverviewCapacityRow [Button + Image]
              Txt_MachineOverviewCapacityRowLabel [TextMeshProUGUI]
              Txt_MachineOverviewCapacityRowValue [TextMeshProUGUI]
  Grp_MachineOverviewLoadingState [无Graphic]
    Panel_MachineOverviewLoadingMessage [Image]
      Txt_MachineOverviewLoadingMessage [TextMeshProUGUI]
  Grp_MachineOverviewEmptyState [无Graphic]
    Panel_MachineOverviewEmptyMessage [Image]
      Txt_MachineOverviewEmptyMessage [TextMeshProUGUI]
  Grp_MachineOverviewErrorState [无Graphic]
    Panel_MachineOverviewErrorMessage [Image]
      Txt_MachineOverviewErrorMessage [TextMeshProUGUI]
  Grp_MachineOverviewSuccessState [无Graphic]
    Panel_MachineOverviewSuccessMessage [Image]
      Txt_MachineOverviewSuccessMessage [TextMeshProUGUI]
  Grp_MachineOverviewDisabledState [无Graphic]
    Panel_MachineOverviewDisabledMessage [Image]
      Txt_MachineOverviewDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachineOverview | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；机器现场概况；内部页面根 |
| Txt_MachineOverviewTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；机器现场概况 |
| Grp_MachineOverviewActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,272); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_MachineOverviewActivate | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；满足现场条件和激活服务检查后执行，不能远程首次激活 |
| Txt_MachineOverviewActivateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；激活或重新激活 |
| Btn_MachineOverviewSleep | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；切换整机请求运行状态，等待安全点时显示等待原因 |
| Txt_MachineOverviewSleepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；休眠或开机 |
| Btn_MachineOverviewPower | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；按供电管理合同执行，影响未完成行为时先确认 |
| Txt_MachineOverviewPowerLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；断电或恢复供电 |
| Btn_MachineOverviewRename | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开17-重命名 |
| Txt_MachineOverviewRenameLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重命名 |
| Btn_MachineOverviewFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦当前机器 |
| Txt_MachineOverviewFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；镜头聚焦 |
| Btn_MachineOverviewHardware | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；切换05-硬件 |
| Txt_MachineOverviewHardwareLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；硬件 |
| Btn_MachineOverviewAlgorithm | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；切换05-算法 |
| Txt_MachineOverviewAlgorithmLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；算法 |
| Btn_MachineOverviewDiagnostic | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；切换05-任务与诊断 |
| Txt_MachineOverviewDiagnosticLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；任务与诊断 |
| Btn_MachineOverviewRecover | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开17-机器回收确认，保留组件配置回到未部署库 |
| Txt_MachineOverviewRecoverLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；回收机器 |
| Panel_MachineOverviewIdentity | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,250); pos(0,-36) | absolute | Image；身份与开关 |
| Txt_MachineOverviewIdentityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；身份与开关 |
| List_MachineOverviewIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineOverviewIdentity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineOverviewIdentity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineOverviewIdentityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称／型号；激活、运行、供电、连接状态；主任务／等待原因；算法异常 |
| Item_MachineOverviewIdentityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineOverviewIdentityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineOverviewIdentityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineOverviewIdentityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineOverviewCapacity | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,250); pos(0,-298) | absolute | Image；容量与运行概况 |
| Txt_MachineOverviewCapacityHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；容量与运行概况 |
| List_MachineOverviewCapacity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineOverviewCapacity | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineOverviewCapacity | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineOverviewCapacityBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；算力；逻辑容量；功率；货舱货物与容量；传感器／效应器运行概况 |
| Item_MachineOverviewCapacityTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineOverviewCapacityRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineOverviewCapacityRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineOverviewCapacityRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachineOverviewLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,512); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineOverviewLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineOverviewLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachineOverviewEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,512); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineOverviewEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineOverviewEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachineOverviewErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,512); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineOverviewErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineOverviewErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachineOverviewSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,512); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineOverviewSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineOverviewSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachineOverviewDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,512); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineOverviewDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineOverviewDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## MachineHardware：机器硬件

功能文档：[机器硬件](MachineHardware.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageMachineHardware [Image]
  Txt_MachineHardwareTitle [TextMeshProUGUI]
  Grp_MachineHardwareActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_MachineHardwareSelect [Button + Image]
      Txt_MachineHardwareSelectLabel [TextMeshProUGUI]
    Btn_MachineHardwareReplace [Button + Image]
      Txt_MachineHardwareReplaceLabel [TextMeshProUGUI]
    Btn_MachineHardwareRemove [Button + Image]
      Txt_MachineHardwareRemoveLabel [TextMeshProUGUI]
    Btn_MachineHardwareUpgrade [Button + Image]
      Txt_MachineHardwareUpgradeLabel [TextMeshProUGUI]
    Btn_MachineHardwareRun [Button + Image]
      Txt_MachineHardwareRunLabel [TextMeshProUGUI]
    Btn_MachineHardwareModify [Button + Image]
      Txt_MachineHardwareModifyLabel [TextMeshProUGUI]
  Panel_MachineHardwareSlots [Image]
    Txt_MachineHardwareSlotsHeading [TextMeshProUGUI]
    List_MachineHardwareSlots [ScrollRect vertical=true horizontal=false]
      Viewport_MachineHardwareSlots [RectMask2D]
        Content_MachineHardwareSlots [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineHardwareSlotsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineHardwareSlotsTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineHardwareSlotsRow [Button + Image]
              Txt_MachineHardwareSlotsRowLabel [TextMeshProUGUI]
              Txt_MachineHardwareSlotsRowValue [TextMeshProUGUI]
  Panel_MachineHardwareComponent [Image]
    Txt_MachineHardwareComponentHeading [TextMeshProUGUI]
    List_MachineHardwareComponent [ScrollRect vertical=true horizontal=false]
      Viewport_MachineHardwareComponent [RectMask2D]
        Content_MachineHardwareComponent [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineHardwareComponentBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineHardwareComponentTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineHardwareComponentRow [Button + Image]
              Txt_MachineHardwareComponentRowLabel [TextMeshProUGUI]
              Txt_MachineHardwareComponentRowValue [TextMeshProUGUI]
  Grp_MachineHardwareLoadingState [无Graphic]
    Panel_MachineHardwareLoadingMessage [Image]
      Txt_MachineHardwareLoadingMessage [TextMeshProUGUI]
  Grp_MachineHardwareEmptyState [无Graphic]
    Panel_MachineHardwareEmptyMessage [Image]
      Txt_MachineHardwareEmptyMessage [TextMeshProUGUI]
  Grp_MachineHardwareErrorState [无Graphic]
    Panel_MachineHardwareErrorMessage [Image]
      Txt_MachineHardwareErrorMessage [TextMeshProUGUI]
  Grp_MachineHardwareSuccessState [无Graphic]
    Panel_MachineHardwareSuccessMessage [Image]
      Txt_MachineHardwareSuccessMessage [TextMeshProUGUI]
  Grp_MachineHardwareDisabledState [无Graphic]
    Panel_MachineHardwareDisabledMessage [Image]
      Txt_MachineHardwareDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachineHardware | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；机器硬件；内部页面根 |
| Txt_MachineHardwareTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；机器硬件 |
| Grp_MachineHardwareActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,160); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_MachineHardwareSelect | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；展示该安装位，槽位数来自型号配置 |
| Txt_MachineHardwareSelectLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；选中槽位 |
| Btn_MachineHardwareReplace | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入12-组件选择器，确认后等待安全停机 |
| Txt_MachineHardwareReplaceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；安装或替换 |
| Btn_MachineHardwareRemove | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入17-硬件修改确认，不能抢先改库存 |
| Txt_MachineHardwareRemoveLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；拆卸组件 |
| Btn_MachineHardwareUpgrade | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开11-组件升级 |
| Txt_MachineHardwareUpgradeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；升级组件 |
| Btn_MachineHardwareRun | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；传感器和效应器可切换，计算核心不提供 |
| Txt_MachineHardwareRunLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；组件启动或待机 |
| Btn_MachineHardwareModify | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入11-改装影响，只展示已获业务许可的变更 |
| Txt_MachineHardwareModifyLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看改装影响 |
| Panel_MachineHardwareSlots | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,306); pos(0,-36) | absolute | Image；实际槽位 |
| Txt_MachineHardwareSlotsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；实际槽位 |
| List_MachineHardwareSlots | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineHardwareSlots | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineHardwareSlots | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineHardwareSlotsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；计算核心、传感器、效应器各实际安装位；空槽；组件名／等级；运行状态 |
| Item_MachineHardwareSlotsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineHardwareSlotsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineHardwareSlotsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineHardwareSlotsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineHardwareComponent | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,306); pos(0,-354) | absolute | Image；选中组件详情 |
| Txt_MachineHardwareComponentHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；选中组件详情 |
| List_MachineHardwareComponent | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineHardwareComponent | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineHardwareComponent | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineHardwareComponentBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；属性；能耗；算力；当前行为；兼容性；改动后算法绑定受影响列表 |
| Item_MachineHardwareComponentTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineHardwareComponentRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineHardwareComponentRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineHardwareComponentRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachineHardwareLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHardwareLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHardwareLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachineHardwareEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHardwareEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHardwareEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachineHardwareErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHardwareErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHardwareErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachineHardwareSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHardwareSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHardwareSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachineHardwareDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineHardwareDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineHardwareDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## MachineAlgorithm：机器算法

功能文档：[机器算法](MachineAlgorithm.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageMachineAlgorithm [Image]
  Txt_MachineAlgorithmTitle [TextMeshProUGUI]
  Grp_MachineAlgorithmActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_MachineAlgorithmEdit [Button + Image]
      Txt_MachineAlgorithmEditLabel [TextMeshProUGUI]
    Btn_MachineAlgorithmEnable [Button + Image]
      Txt_MachineAlgorithmEnableLabel [TextMeshProUGUI]
    Btn_MachineAlgorithmParameters [Button + Image]
      Txt_MachineAlgorithmParametersLabel [TextMeshProUGUI]
    Btn_MachineAlgorithmReset [Button + Image]
      Txt_MachineAlgorithmResetLabel [TextMeshProUGUI]
    Btn_MachineAlgorithmTemplate [Button + Image]
      Txt_MachineAlgorithmTemplateLabel [TextMeshProUGUI]
  Panel_MachineAlgorithmInstances [Image]
    Txt_MachineAlgorithmInstancesHeading [TextMeshProUGUI]
    List_MachineAlgorithmInstances [ScrollRect vertical=true horizontal=false]
      Viewport_MachineAlgorithmInstances [RectMask2D]
        Content_MachineAlgorithmInstances [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineAlgorithmInstancesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineAlgorithmInstancesTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineAlgorithmInstancesRow [Button + Image]
              Txt_MachineAlgorithmInstancesRowLabel [TextMeshProUGUI]
              Txt_MachineAlgorithmInstancesRowValue [TextMeshProUGUI]
  Panel_MachineAlgorithmParameters [Image]
    Txt_MachineAlgorithmParametersHeading [TextMeshProUGUI]
    List_MachineAlgorithmParameters [ScrollRect vertical=true horizontal=false]
      Viewport_MachineAlgorithmParameters [RectMask2D]
        Content_MachineAlgorithmParameters [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineAlgorithmParametersBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineAlgorithmParametersTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineAlgorithmParametersRow [Button + Image]
              Txt_MachineAlgorithmParametersRowLabel [TextMeshProUGUI]
              Txt_MachineAlgorithmParametersRowValue [TextMeshProUGUI]
  Grp_MachineAlgorithmLoadingState [无Graphic]
    Panel_MachineAlgorithmLoadingMessage [Image]
      Txt_MachineAlgorithmLoadingMessage [TextMeshProUGUI]
  Grp_MachineAlgorithmEmptyState [无Graphic]
    Panel_MachineAlgorithmEmptyMessage [Image]
      Txt_MachineAlgorithmEmptyMessage [TextMeshProUGUI]
  Grp_MachineAlgorithmErrorState [无Graphic]
    Panel_MachineAlgorithmErrorMessage [Image]
      Txt_MachineAlgorithmErrorMessage [TextMeshProUGUI]
  Grp_MachineAlgorithmSuccessState [无Graphic]
    Panel_MachineAlgorithmSuccessMessage [Image]
      Txt_MachineAlgorithmSuccessMessage [TextMeshProUGUI]
  Grp_MachineAlgorithmDisabledState [无Graphic]
    Panel_MachineAlgorithmDisabledMessage [Image]
      Txt_MachineAlgorithmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachineAlgorithm | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；机器算法；内部页面根 |
| Txt_MachineAlgorithmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；机器算法 |
| Grp_MachineAlgorithmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,160); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_MachineAlgorithmEdit | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入13-编辑模式 |
| Txt_MachineAlgorithmEditLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；编辑算法 |
| Btn_MachineAlgorithmEnable | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；依实例有效性和权限提交，异常实例说明不可启用原因 |
| Txt_MachineAlgorithmEnableLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；启用或停用 |
| Btn_MachineAlgorithmParameters | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开参数编辑区域并走完整验证与应用确认 |
| Txt_MachineAlgorithmParametersLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；修改公开参数 |
| Btn_MachineAlgorithmReset | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入17-算法重置强确认 |
| Txt_MachineAlgorithmResetLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重置算法状态 |
| Btn_MachineAlgorithmTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入09-算法库，再12-集中待绑定 |
| Txt_MachineAlgorithmTemplateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；从模板创建 |
| Panel_MachineAlgorithmInstances | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,306); pos(0,-36) | absolute | Image；本机算法实例 |
| Txt_MachineAlgorithmInstancesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；本机算法实例 |
| List_MachineAlgorithmInstances | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineAlgorithmInstances | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineAlgorithmInstances | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineAlgorithmInstancesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称；启用；有效／异常状态；逻辑成本；最近运行 |
| Item_MachineAlgorithmInstancesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineAlgorithmInstancesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineAlgorithmInstancesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineAlgorithmInstancesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineAlgorithmParameters | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,306); pos(0,-354) | absolute | Image；选中实例 |
| Txt_MachineAlgorithmParametersHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；选中实例 |
| List_MachineAlgorithmParameters | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineAlgorithmParameters | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineAlgorithmParameters | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineAlgorithmParametersBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；公开运行参数；绑定状态；当前版本；草稿或等待应用状态 |
| Item_MachineAlgorithmParametersTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineAlgorithmParametersRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineAlgorithmParametersRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineAlgorithmParametersRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachineAlgorithmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineAlgorithmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineAlgorithmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachineAlgorithmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineAlgorithmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineAlgorithmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachineAlgorithmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineAlgorithmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineAlgorithmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachineAlgorithmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineAlgorithmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineAlgorithmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachineAlgorithmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,624); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineAlgorithmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineAlgorithmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## MachineDiagnostics：机器任务与诊断

功能文档：[机器任务与诊断](MachineDiagnostics.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageMachineDiagnostics [Image]
  Txt_MachineDiagnosticsTitle [TextMeshProUGUI]
  Grp_MachineDiagnosticsActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_MachineDiagnosticsTaskRecord [Button + Image]
      Txt_MachineDiagnosticsTaskRecordLabel [TextMeshProUGUI]
    Btn_MachineDiagnosticsRunRecord [Button + Image]
      Txt_MachineDiagnosticsRunRecordLabel [TextMeshProUGUI]
    Btn_MachineDiagnosticsTrace [Button + Image]
      Txt_MachineDiagnosticsTraceLabel [TextMeshProUGUI]
    Btn_MachineDiagnosticsLocate [Button + Image]
      Txt_MachineDiagnosticsLocateLabel [TextMeshProUGUI]
  Panel_MachineDiagnosticsTasks [Image]
    Txt_MachineDiagnosticsTasksHeading [TextMeshProUGUI]
    List_MachineDiagnosticsTasks [ScrollRect vertical=true horizontal=false]
      Viewport_MachineDiagnosticsTasks [RectMask2D]
        Content_MachineDiagnosticsTasks [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineDiagnosticsTasksBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineDiagnosticsTasksTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineDiagnosticsTasksRow [Button + Image]
              Txt_MachineDiagnosticsTasksRowLabel [TextMeshProUGUI]
              Txt_MachineDiagnosticsTasksRowValue [TextMeshProUGUI]
  Panel_MachineDiagnosticsCompute [Image]
    Txt_MachineDiagnosticsComputeHeading [TextMeshProUGUI]
    List_MachineDiagnosticsCompute [ScrollRect vertical=true horizontal=false]
      Viewport_MachineDiagnosticsCompute [RectMask2D]
        Content_MachineDiagnosticsCompute [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachineDiagnosticsComputeBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachineDiagnosticsComputeTemplate [LayoutElement + Image；默认inactive]
            Btn_MachineDiagnosticsComputeRow [Button + Image]
              Txt_MachineDiagnosticsComputeRowLabel [TextMeshProUGUI]
              Txt_MachineDiagnosticsComputeRowValue [TextMeshProUGUI]
  Grp_MachineDiagnosticsLoadingState [无Graphic]
    Panel_MachineDiagnosticsLoadingMessage [Image]
      Txt_MachineDiagnosticsLoadingMessage [TextMeshProUGUI]
  Grp_MachineDiagnosticsEmptyState [无Graphic]
    Panel_MachineDiagnosticsEmptyMessage [Image]
      Txt_MachineDiagnosticsEmptyMessage [TextMeshProUGUI]
  Grp_MachineDiagnosticsErrorState [无Graphic]
    Panel_MachineDiagnosticsErrorMessage [Image]
      Txt_MachineDiagnosticsErrorMessage [TextMeshProUGUI]
  Grp_MachineDiagnosticsSuccessState [无Graphic]
    Panel_MachineDiagnosticsSuccessMessage [Image]
      Txt_MachineDiagnosticsSuccessMessage [TextMeshProUGUI]
  Grp_MachineDiagnosticsDisabledState [无Graphic]
    Panel_MachineDiagnosticsDisabledMessage [Image]
      Txt_MachineDiagnosticsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachineDiagnostics | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；机器任务与诊断；内部页面根 |
| Txt_MachineDiagnosticsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；机器任务与诊断 |
| Grp_MachineDiagnosticsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_MachineDiagnosticsTaskRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入15-机器任务历史 |
| Txt_MachineDiagnosticsTaskRecordLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看任务记录 |
| Btn_MachineDiagnosticsRunRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入15-算法运行记录 |
| Txt_MachineDiagnosticsRunRecordLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看算法记录 |
| Btn_MachineDiagnosticsTrace | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；携带历史运行ID进入13-诊断模式 |
| Txt_MachineDiagnosticsTraceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；诊断因果路径 |
| Btn_MachineDiagnosticsLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；有效时聚焦；失效时保持历史引用 |
| Txt_MachineDiagnosticsLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位关联对象 |
| Panel_MachineDiagnosticsTasks | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；任务与效应器队列 |
| Txt_MachineDiagnosticsTasksHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；任务与效应器队列 |
| List_MachineDiagnosticsTasks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineDiagnosticsTasks | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineDiagnosticsTasks | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineDiagnosticsTasksBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；当前任务；父子关系；当前行为；等待原因；每个效应器执行与排队状态 |
| Item_MachineDiagnosticsTasksTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineDiagnosticsTasksRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineDiagnosticsTasksRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineDiagnosticsTasksRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachineDiagnosticsCompute | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；算力与历史 |
| Txt_MachineDiagnosticsComputeHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；算力与历史 |
| List_MachineDiagnosticsCompute | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachineDiagnosticsCompute | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachineDiagnosticsCompute | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachineDiagnosticsComputeBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；本机算力占用；等待算力项；任务历史；算法最近运行；因果关联 |
| Item_MachineDiagnosticsComputeTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachineDiagnosticsComputeRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachineDiagnosticsComputeRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachineDiagnosticsComputeRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachineDiagnosticsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDiagnosticsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDiagnosticsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachineDiagnosticsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDiagnosticsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDiagnosticsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachineDiagnosticsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDiagnosticsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDiagnosticsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachineDiagnosticsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDiagnosticsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDiagnosticsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachineDiagnosticsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachineDiagnosticsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachineDiagnosticsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## RemoteMachine：中枢机器详情

功能文档：[中枢机器详情](RemoteMachine.md)；归属 `BaseCommandHubForm`；内容 1488×730。

```text
Panel_PageRemoteMachine [Image]
  Txt_RemoteMachineTitle [TextMeshProUGUI]
  Grp_RemoteMachineActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_RemoteMachineSleep [Button + Image]
      Txt_RemoteMachineSleepLabel [TextMeshProUGUI]
    Btn_RemoteMachineComponent [Button + Image]
      Txt_RemoteMachineComponentLabel [TextMeshProUGUI]
    Btn_RemoteMachineEdit [Button + Image]
      Txt_RemoteMachineEditLabel [TextMeshProUGUI]
    Btn_RemoteMachineHistory [Button + Image]
      Txt_RemoteMachineHistoryLabel [TextMeshProUGUI]
    Btn_RemoteMachineLocate [Button + Image]
      Txt_RemoteMachineLocateLabel [TextMeshProUGUI]
  Panel_RemoteMachineOverview [Image]
    Txt_RemoteMachineOverviewHeading [TextMeshProUGUI]
    List_RemoteMachineOverview [ScrollRect vertical=true horizontal=false]
      Viewport_RemoteMachineOverview [RectMask2D]
        Content_RemoteMachineOverview [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RemoteMachineOverviewBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RemoteMachineOverviewTemplate [LayoutElement + Image；默认inactive]
            Btn_RemoteMachineOverviewRow [Button + Image]
              Txt_RemoteMachineOverviewRowLabel [TextMeshProUGUI]
              Txt_RemoteMachineOverviewRowValue [TextMeshProUGUI]
  Panel_RemoteMachineHardware [Image]
    Txt_RemoteMachineHardwareHeading [TextMeshProUGUI]
    List_RemoteMachineHardware [ScrollRect vertical=true horizontal=false]
      Viewport_RemoteMachineHardware [RectMask2D]
        Content_RemoteMachineHardware [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RemoteMachineHardwareBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RemoteMachineHardwareTemplate [LayoutElement + Image；默认inactive]
            Btn_RemoteMachineHardwareRow [Button + Image]
              Txt_RemoteMachineHardwareRowLabel [TextMeshProUGUI]
              Txt_RemoteMachineHardwareRowValue [TextMeshProUGUI]
  Panel_RemoteMachineAlgorithms [Image]
    Txt_RemoteMachineAlgorithmsHeading [TextMeshProUGUI]
    List_RemoteMachineAlgorithms [ScrollRect vertical=true horizontal=false]
      Viewport_RemoteMachineAlgorithms [RectMask2D]
        Content_RemoteMachineAlgorithms [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_RemoteMachineAlgorithmsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_RemoteMachineAlgorithmsTemplate [LayoutElement + Image；默认inactive]
            Btn_RemoteMachineAlgorithmsRow [Button + Image]
              Txt_RemoteMachineAlgorithmsRowLabel [TextMeshProUGUI]
              Txt_RemoteMachineAlgorithmsRowValue [TextMeshProUGUI]
  Grp_RemoteMachineLoadingState [无Graphic]
    Panel_RemoteMachineLoadingMessage [Image]
      Txt_RemoteMachineLoadingMessage [TextMeshProUGUI]
  Grp_RemoteMachineEmptyState [无Graphic]
    Panel_RemoteMachineEmptyMessage [Image]
      Txt_RemoteMachineEmptyMessage [TextMeshProUGUI]
  Grp_RemoteMachineErrorState [无Graphic]
    Panel_RemoteMachineErrorMessage [Image]
      Txt_RemoteMachineErrorMessage [TextMeshProUGUI]
  Grp_RemoteMachineSuccessState [无Graphic]
    Panel_RemoteMachineSuccessMessage [Image]
      Txt_RemoteMachineSuccessMessage [TextMeshProUGUI]
  Grp_RemoteMachineDisabledState [无Graphic]
    Panel_RemoteMachineDisabledMessage [Image]
      Txt_RemoteMachineDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageRemoteMachine | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；中枢机器详情；内部页面根 |
| Txt_RemoteMachineTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；中枢机器详情 |
| Grp_RemoteMachineActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_RemoteMachineSleep | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；按远程权限切换请求状态 |
| Txt_RemoteMachineSleepLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；休眠或开机 |
| Btn_RemoteMachineComponent | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；仅传感器／效应器，权限不足显示原因 |
| Txt_RemoteMachineComponentLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；组件启动或待机 |
| Btn_RemoteMachineEdit | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入13并携带机器ID和中枢返回路径 |
| Txt_RemoteMachineEditLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；打开本地算法 |
| Btn_RemoteMachineHistory | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入15对应记录 |
| Txt_RemoteMachineHistoryLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看记录 |
| Btn_RemoteMachineLocate | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦后满足条件才开放现场硬件功能 |
| Txt_RemoteMachineLocateLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；前往现场 |
| Panel_RemoteMachineOverview | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；远程概况 |
| Txt_RemoteMachineOverviewHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；远程概况 |
| List_RemoteMachineOverview | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RemoteMachineOverview | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RemoteMachineOverview | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RemoteMachineOverviewBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；名称、状态、位置；任务与等待；本机算力；电能；连接与数据新鲜度 |
| Item_RemoteMachineOverviewTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RemoteMachineOverviewRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RemoteMachineOverviewRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RemoteMachineOverviewRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_RemoteMachineHardware | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；远程硬件查看 |
| Txt_RemoteMachineHardwareHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；远程硬件查看 |
| List_RemoteMachineHardware | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RemoteMachineHardware | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RemoteMachineHardware | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RemoteMachineHardwareBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实际槽位；组件属性；启动／待机状态；不可现场操作说明 |
| Item_RemoteMachineHardwareTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RemoteMachineHardwareRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RemoteMachineHardwareRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RemoteMachineHardwareRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_RemoteMachineAlgorithms | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；算法与记录 |
| Txt_RemoteMachineAlgorithmsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；算法与记录 |
| List_RemoteMachineAlgorithms | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_RemoteMachineAlgorithms | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_RemoteMachineAlgorithms | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_RemoteMachineAlgorithmsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实例列表；公开参数；最近任务；算力等待；日志入口 |
| Item_RemoteMachineAlgorithmsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_RemoteMachineAlgorithmsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_RemoteMachineAlgorithmsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_RemoteMachineAlgorithmsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_RemoteMachineLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RemoteMachineLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RemoteMachineLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_RemoteMachineEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RemoteMachineEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RemoteMachineEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_RemoteMachineErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RemoteMachineErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RemoteMachineErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_RemoteMachineSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RemoteMachineSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RemoteMachineSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_RemoteMachineDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_RemoteMachineDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_RemoteMachineDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## MachinePreparation：未部署机器整备

功能文档：[未部署机器整备](MachinePreparation.md)；归属 `MachineLibraryForm`；内容 1488×730。

```text
Panel_PageMachinePreparation [Image]
  Txt_MachinePreparationTitle [TextMeshProUGUI]
  Grp_MachinePreparationActions [GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)]
    Btn_MachinePreparationRename [Button + Image]
      Txt_MachinePreparationRenameLabel [TextMeshProUGUI]
    Btn_MachinePreparationInstall [Button + Image]
      Txt_MachinePreparationInstallLabel [TextMeshProUGUI]
    Btn_MachinePreparationUnload [Button + Image]
      Txt_MachinePreparationUnloadLabel [TextMeshProUGUI]
    Btn_MachinePreparationUpgrade [Button + Image]
      Txt_MachinePreparationUpgradeLabel [TextMeshProUGUI]
    Btn_MachinePreparationSell [Button + Image]
      Txt_MachinePreparationSellLabel [TextMeshProUGUI]
    Btn_MachinePreparationDeploy [Button + Image]
      Txt_MachinePreparationDeployLabel [TextMeshProUGUI]
  Panel_MachinePreparationCarrier [Image]
    Txt_MachinePreparationCarrierHeading [TextMeshProUGUI]
    List_MachinePreparationCarrier [ScrollRect vertical=true horizontal=false]
      Viewport_MachinePreparationCarrier [RectMask2D]
        Content_MachinePreparationCarrier [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachinePreparationCarrierBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachinePreparationCarrierTemplate [LayoutElement + Image；默认inactive]
            Btn_MachinePreparationCarrierRow [Button + Image]
              Txt_MachinePreparationCarrierRowLabel [TextMeshProUGUI]
              Txt_MachinePreparationCarrierRowValue [TextMeshProUGUI]
  Panel_MachinePreparationAssembly [Image]
    Txt_MachinePreparationAssemblyHeading [TextMeshProUGUI]
    List_MachinePreparationAssembly [ScrollRect vertical=true horizontal=false]
      Viewport_MachinePreparationAssembly [RectMask2D]
        Content_MachinePreparationAssembly [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachinePreparationAssemblyBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachinePreparationAssemblyTemplate [LayoutElement + Image；默认inactive]
            Btn_MachinePreparationAssemblyRow [Button + Image]
              Txt_MachinePreparationAssemblyRowLabel [TextMeshProUGUI]
              Txt_MachinePreparationAssemblyRowValue [TextMeshProUGUI]
  Panel_MachinePreparationReadiness [Image]
    Txt_MachinePreparationReadinessHeading [TextMeshProUGUI]
    List_MachinePreparationReadiness [ScrollRect vertical=true horizontal=false]
      Viewport_MachinePreparationReadiness [RectMask2D]
        Content_MachinePreparationReadiness [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MachinePreparationReadinessBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MachinePreparationReadinessTemplate [LayoutElement + Image；默认inactive]
            Btn_MachinePreparationReadinessRow [Button + Image]
              Txt_MachinePreparationReadinessRowLabel [TextMeshProUGUI]
              Txt_MachinePreparationReadinessRowValue [TextMeshProUGUI]
  Grp_MachinePreparationLoadingState [无Graphic]
    Panel_MachinePreparationLoadingMessage [Image]
      Txt_MachinePreparationLoadingMessage [TextMeshProUGUI]
  Grp_MachinePreparationEmptyState [无Graphic]
    Panel_MachinePreparationEmptyMessage [Image]
      Txt_MachinePreparationEmptyMessage [TextMeshProUGUI]
  Grp_MachinePreparationErrorState [无Graphic]
    Panel_MachinePreparationErrorMessage [Image]
      Txt_MachinePreparationErrorMessage [TextMeshProUGUI]
  Grp_MachinePreparationSuccessState [无Graphic]
    Panel_MachinePreparationSuccessMessage [Image]
      Txt_MachinePreparationSuccessMessage [TextMeshProUGUI]
  Grp_MachinePreparationDisabledState [无Graphic]
    Panel_MachinePreparationDisabledMessage [Image]
      Txt_MachinePreparationDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMachinePreparation | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Image；未部署机器整备；内部页面根 |
| Txt_MachinePreparationTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(1472,32); pos(8,0) | absolute | TextMeshProUGUI；未部署机器整备 |
| Grp_MachinePreparationActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,48); pos(0,0) | absolute | GridLayoutGroup fixedColumns=8 cell=(177,48) spacing=(8,8)；操作区 |
| Btn_MachinePreparationRename | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入17-重命名 |
| Txt_MachinePreparationRenameLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；重命名 |
| Btn_MachinePreparationInstall | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入12选择器与17影响确认，整备权限允许操作 |
| Txt_MachinePreparationInstallLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；安装或拆卸 |
| Btn_MachinePreparationUnload | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；确认全部卸下影响并原子回库 |
| Txt_MachinePreparationUnloadLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；一键卸下 |
| Btn_MachinePreparationUpgrade | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入11-载体升级 |
| Txt_MachinePreparationUpgradeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；升级载体 |
| Btn_MachinePreparationSell | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；资格成立才进入17交易确认 |
| Txt_MachinePreparationSellLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；出售空载完好载体 |
| Btn_MachinePreparationDeploy | min(0,1) max(0,1); pivot(0,1); sizeDelta(177,48)初始化; pos(0,0)初始化; LayoutElement preferred(177,48); 最终位置/尺寸由组驱动 | group | Button + Image；进入14-机器部署，保留机器实例ID |
| Txt_MachinePreparationDeployLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；进入部署 |
| Panel_MachinePreparationCarrier | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(0,-36) | absolute | Image；载体概况 |
| Txt_MachinePreparationCarrierHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；载体概况 |
| List_MachinePreparationCarrier | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachinePreparationCarrier | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachinePreparationCarrier | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachinePreparationCarrierBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；独立实例名称、型号、等级、部署状态；容量与兼容安装位 |
| Item_MachinePreparationCarrierTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachinePreparationCarrierRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachinePreparationCarrierRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachinePreparationCarrierRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachinePreparationAssembly | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(501,-36) | absolute | Image；组件整备 |
| Txt_MachinePreparationAssemblyHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；组件整备 |
| List_MachinePreparationAssembly | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachinePreparationAssembly | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachinePreparationAssembly | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachinePreparationAssemblyBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；实际槽位及组件；库存候选摘要；一键卸下影响 |
| Item_MachinePreparationAssemblyTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachinePreparationAssemblyRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachinePreparationAssemblyRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachinePreparationAssemblyRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MachinePreparationReadiness | min(0,1) max(0,1); pivot(0,1); sizeDelta(485,634); pos(1002,-36) | absolute | Image；部署准备 |
| Txt_MachinePreparationReadinessHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,32); pos(12,-8) | absolute | TextMeshProUGUI；部署准备 |
| List_MachinePreparationReadiness | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MachinePreparationReadiness | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MachinePreparationReadiness | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MachinePreparationReadinessBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,120)初始化; pos(0,0)初始化; LayoutElement preferred(461,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；硬件配置；算法能力需求；部署解锁条件；出售资格 |
| Item_MachinePreparationReadinessTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(461,104)初始化; pos(0,0)初始化; LayoutElement preferred(461,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MachinePreparationReadinessRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MachinePreparationReadinessRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MachinePreparationReadinessRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MachinePreparationLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachinePreparationLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachinePreparationLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MachinePreparationEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachinePreparationEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachinePreparationEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MachinePreparationErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachinePreparationErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachinePreparationErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MachinePreparationSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachinePreparationSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachinePreparationSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MachinePreparationDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(1488,634); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MachinePreparationDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(520,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MachinePreparationDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
