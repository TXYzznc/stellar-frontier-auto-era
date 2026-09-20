# 06-资源点观察 — prefab-layout

状态：2026-09-19完整设计候选，待用户集中验收；未据此修改Prefab。

继承[通用合同](../00-通用合同.md)和[共享外壳](../00-共享外壳-prefab-layout.md)。下列子树预制在所属Form的Grp_PageHost中；公共外壳由共享文档唯一声明。跨家族同属一个Form的子树合并，禁止重复根节点。表中所有Image／文本颜色、资源、射线、默认字号与状态继承通用合同，列出的数据是字段说明，不是示例业务数值。

## Farm：农田详情

功能文档：[农田详情](Farm.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageFarm [Image]
  Txt_FarmTitle [TextMeshProUGUI]
  Grp_FarmActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_FarmRecord [Button + Image]
      Txt_FarmRecordLabel [TextMeshProUGUI]
    Btn_FarmKnowledge [Button + Image]
      Txt_FarmKnowledgeLabel [TextMeshProUGUI]
    Btn_FarmRule [Button + Image]
      Txt_FarmRuleLabel [TextMeshProUGUI]
    Btn_FarmFocus [Button + Image]
      Txt_FarmFocusLabel [TextMeshProUGUI]
  Panel_FarmPublic [Image]
    Txt_FarmPublicHeading [TextMeshProUGUI]
    List_FarmPublic [ScrollRect vertical=true horizontal=false]
      Viewport_FarmPublic [RectMask2D]
        Content_FarmPublic [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_FarmPublicBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_FarmPublicTemplate [LayoutElement + Image；默认inactive]
            Btn_FarmPublicRow [Button + Image]
              Txt_FarmPublicRowLabel [TextMeshProUGUI]
              Txt_FarmPublicRowValue [TextMeshProUGUI]
  Panel_FarmSensor [Image]
    Txt_FarmSensorHeading [TextMeshProUGUI]
    List_FarmSensor [ScrollRect vertical=true horizontal=false]
      Viewport_FarmSensor [RectMask2D]
        Content_FarmSensor [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_FarmSensorBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_FarmSensorTemplate [LayoutElement + Image；默认inactive]
            Btn_FarmSensorRow [Button + Image]
              Txt_FarmSensorRowLabel [TextMeshProUGUI]
              Txt_FarmSensorRowValue [TextMeshProUGUI]
  Panel_FarmRecord [Image]
    Txt_FarmRecordHeading [TextMeshProUGUI]
    List_FarmRecord [ScrollRect vertical=true horizontal=false]
      Viewport_FarmRecord [RectMask2D]
        Content_FarmRecord [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_FarmRecordBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_FarmRecordTemplate [LayoutElement + Image；默认inactive]
            Btn_FarmRecordRow [Button + Image]
              Txt_FarmRecordRowLabel [TextMeshProUGUI]
              Txt_FarmRecordRowValue [TextMeshProUGUI]
  Grp_FarmLoadingState [无Graphic]
    Panel_FarmLoadingMessage [Image]
      Txt_FarmLoadingMessage [TextMeshProUGUI]
  Grp_FarmEmptyState [无Graphic]
    Panel_FarmEmptyMessage [Image]
      Txt_FarmEmptyMessage [TextMeshProUGUI]
  Grp_FarmErrorState [无Graphic]
    Panel_FarmErrorMessage [Image]
      Txt_FarmErrorMessage [TextMeshProUGUI]
  Grp_FarmSuccessState [无Graphic]
    Panel_FarmSuccessMessage [Image]
      Txt_FarmSuccessMessage [TextMeshProUGUI]
  Grp_FarmDisabledState [无Graphic]
    Panel_FarmDisabledMessage [Image]
      Txt_FarmDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageFarm | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；农田详情；内部页面根 |
| Txt_FarmTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；农田详情 |
| Grp_FarmActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_FarmRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；在本面板展开对应对象记录，保留对象选择 |
| Txt_FarmRecordLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看记录 |
| Btn_FarmKnowledge | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；农田打开16-作物图鉴；其它资源类型未配置知识时隐藏 |
| Txt_FarmKnowledgeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看知识 |
| Btn_FarmRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开16-通用规则 |
| Txt_FarmRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Btn_FarmFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦当前资源点 |
| Txt_FarmFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；聚焦 |
| Panel_FarmPublic | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-36) | absolute | Image；公开状态 |
| Txt_FarmPublicHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；公开状态 |
| List_FarmPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_FarmPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_FarmPublic | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_FarmPublicBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；作物；种植量／容量；生长阶段；进度；成熟数量；缓存；总体产出反馈；图鉴解锁状态 |
| Item_FarmPublicTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_FarmPublicRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_FarmPublicRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_FarmPublicRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_FarmSensor | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-266) | absolute | Image；关联传感数据 |
| Txt_FarmSensorHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；关联传感数据 |
| List_FarmSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_FarmSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_FarmSensor | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_FarmSensorBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；有效采样；来源机器／传感器；采样时刻；失效说明 |
| Item_FarmSensorTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_FarmSensorRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_FarmSensorRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_FarmSensorRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_FarmRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-496) | absolute | Image；生产记录与规则 |
| Txt_FarmRecordHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；生产记录与规则 |
| List_FarmRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_FarmRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_FarmRecord | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_FarmRecordBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；阻塞；生产事件；自然语言作业描述；规则描述ID |
| Item_FarmRecordTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_FarmRecordRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_FarmRecordRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_FarmRecordRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_FarmLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FarmLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FarmLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_FarmEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FarmEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FarmEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_FarmErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FarmErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FarmErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_FarmSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FarmSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FarmSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_FarmDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_FarmDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_FarmDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Forest：人工林详情

功能文档：[人工林详情](Forest.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageForest [Image]
  Txt_ForestTitle [TextMeshProUGUI]
  Grp_ForestActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_ForestRecord [Button + Image]
      Txt_ForestRecordLabel [TextMeshProUGUI]
    Btn_ForestKnowledge [Button + Image]
      Txt_ForestKnowledgeLabel [TextMeshProUGUI]
    Btn_ForestRule [Button + Image]
      Txt_ForestRuleLabel [TextMeshProUGUI]
    Btn_ForestFocus [Button + Image]
      Txt_ForestFocusLabel [TextMeshProUGUI]
  Panel_ForestPublic [Image]
    Txt_ForestPublicHeading [TextMeshProUGUI]
    List_ForestPublic [ScrollRect vertical=true horizontal=false]
      Viewport_ForestPublic [RectMask2D]
        Content_ForestPublic [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ForestPublicBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ForestPublicTemplate [LayoutElement + Image；默认inactive]
            Btn_ForestPublicRow [Button + Image]
              Txt_ForestPublicRowLabel [TextMeshProUGUI]
              Txt_ForestPublicRowValue [TextMeshProUGUI]
  Panel_ForestSensor [Image]
    Txt_ForestSensorHeading [TextMeshProUGUI]
    List_ForestSensor [ScrollRect vertical=true horizontal=false]
      Viewport_ForestSensor [RectMask2D]
        Content_ForestSensor [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ForestSensorBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ForestSensorTemplate [LayoutElement + Image；默认inactive]
            Btn_ForestSensorRow [Button + Image]
              Txt_ForestSensorRowLabel [TextMeshProUGUI]
              Txt_ForestSensorRowValue [TextMeshProUGUI]
  Panel_ForestRecord [Image]
    Txt_ForestRecordHeading [TextMeshProUGUI]
    List_ForestRecord [ScrollRect vertical=true horizontal=false]
      Viewport_ForestRecord [RectMask2D]
        Content_ForestRecord [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_ForestRecordBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_ForestRecordTemplate [LayoutElement + Image；默认inactive]
            Btn_ForestRecordRow [Button + Image]
              Txt_ForestRecordRowLabel [TextMeshProUGUI]
              Txt_ForestRecordRowValue [TextMeshProUGUI]
  Grp_ForestLoadingState [无Graphic]
    Panel_ForestLoadingMessage [Image]
      Txt_ForestLoadingMessage [TextMeshProUGUI]
  Grp_ForestEmptyState [无Graphic]
    Panel_ForestEmptyMessage [Image]
      Txt_ForestEmptyMessage [TextMeshProUGUI]
  Grp_ForestErrorState [无Graphic]
    Panel_ForestErrorMessage [Image]
      Txt_ForestErrorMessage [TextMeshProUGUI]
  Grp_ForestSuccessState [无Graphic]
    Panel_ForestSuccessMessage [Image]
      Txt_ForestSuccessMessage [TextMeshProUGUI]
  Grp_ForestDisabledState [无Graphic]
    Panel_ForestDisabledMessage [Image]
      Txt_ForestDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageForest | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；人工林详情；内部页面根 |
| Txt_ForestTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；人工林详情 |
| Grp_ForestActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_ForestRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；在本面板展开对应对象记录，保留对象选择 |
| Txt_ForestRecordLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看记录 |
| Btn_ForestKnowledge | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；农田打开16-作物图鉴；其它资源类型未配置知识时隐藏 |
| Txt_ForestKnowledgeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看知识 |
| Btn_ForestRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开16-通用规则 |
| Txt_ForestRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Btn_ForestFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦当前资源点 |
| Txt_ForestFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；聚焦 |
| Panel_ForestPublic | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-36) | absolute | Image；公开状态 |
| Txt_ForestPublicHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；公开状态 |
| List_ForestPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ForestPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ForestPublic | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ForestPublicBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；阶段；生长进度；可采集数量；恢复速度；缓存及阻塞 |
| Item_ForestPublicTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ForestPublicRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ForestPublicRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ForestPublicRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ForestSensor | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-266) | absolute | Image；关联传感数据 |
| Txt_ForestSensorHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；关联传感数据 |
| List_ForestSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ForestSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ForestSensor | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ForestSensorBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；有效采样；来源机器／传感器；采样时刻；失效说明 |
| Item_ForestSensorTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ForestSensorRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ForestSensorRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ForestSensorRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_ForestRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-496) | absolute | Image；生产记录与规则 |
| Txt_ForestRecordHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；生产记录与规则 |
| List_ForestRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_ForestRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_ForestRecord | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_ForestRecordBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；阻塞；生产事件；自然语言作业描述；规则描述ID |
| Item_ForestRecordTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_ForestRecordRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_ForestRecordRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_ForestRecordRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_ForestLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForestLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForestLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_ForestEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForestEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForestEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_ForestErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForestErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForestErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_ForestSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForestSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForestSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_ForestDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_ForestDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_ForestDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Mineral：地表矿脉详情

功能文档：[地表矿脉详情](Mineral.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageMineral [Image]
  Txt_MineralTitle [TextMeshProUGUI]
  Grp_MineralActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_MineralRecord [Button + Image]
      Txt_MineralRecordLabel [TextMeshProUGUI]
    Btn_MineralKnowledge [Button + Image]
      Txt_MineralKnowledgeLabel [TextMeshProUGUI]
    Btn_MineralRule [Button + Image]
      Txt_MineralRuleLabel [TextMeshProUGUI]
    Btn_MineralFocus [Button + Image]
      Txt_MineralFocusLabel [TextMeshProUGUI]
  Panel_MineralPublic [Image]
    Txt_MineralPublicHeading [TextMeshProUGUI]
    List_MineralPublic [ScrollRect vertical=true horizontal=false]
      Viewport_MineralPublic [RectMask2D]
        Content_MineralPublic [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MineralPublicBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MineralPublicTemplate [LayoutElement + Image；默认inactive]
            Btn_MineralPublicRow [Button + Image]
              Txt_MineralPublicRowLabel [TextMeshProUGUI]
              Txt_MineralPublicRowValue [TextMeshProUGUI]
  Panel_MineralSensor [Image]
    Txt_MineralSensorHeading [TextMeshProUGUI]
    List_MineralSensor [ScrollRect vertical=true horizontal=false]
      Viewport_MineralSensor [RectMask2D]
        Content_MineralSensor [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MineralSensorBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MineralSensorTemplate [LayoutElement + Image；默认inactive]
            Btn_MineralSensorRow [Button + Image]
              Txt_MineralSensorRowLabel [TextMeshProUGUI]
              Txt_MineralSensorRowValue [TextMeshProUGUI]
  Panel_MineralRecord [Image]
    Txt_MineralRecordHeading [TextMeshProUGUI]
    List_MineralRecord [ScrollRect vertical=true horizontal=false]
      Viewport_MineralRecord [RectMask2D]
        Content_MineralRecord [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_MineralRecordBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_MineralRecordTemplate [LayoutElement + Image；默认inactive]
            Btn_MineralRecordRow [Button + Image]
              Txt_MineralRecordRowLabel [TextMeshProUGUI]
              Txt_MineralRecordRowValue [TextMeshProUGUI]
  Grp_MineralLoadingState [无Graphic]
    Panel_MineralLoadingMessage [Image]
      Txt_MineralLoadingMessage [TextMeshProUGUI]
  Grp_MineralEmptyState [无Graphic]
    Panel_MineralEmptyMessage [Image]
      Txt_MineralEmptyMessage [TextMeshProUGUI]
  Grp_MineralErrorState [无Graphic]
    Panel_MineralErrorMessage [Image]
      Txt_MineralErrorMessage [TextMeshProUGUI]
  Grp_MineralSuccessState [无Graphic]
    Panel_MineralSuccessMessage [Image]
      Txt_MineralSuccessMessage [TextMeshProUGUI]
  Grp_MineralDisabledState [无Graphic]
    Panel_MineralDisabledMessage [Image]
      Txt_MineralDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageMineral | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；地表矿脉详情；内部页面根 |
| Txt_MineralTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；地表矿脉详情 |
| Grp_MineralActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_MineralRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；在本面板展开对应对象记录，保留对象选择 |
| Txt_MineralRecordLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看记录 |
| Btn_MineralKnowledge | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；农田打开16-作物图鉴；其它资源类型未配置知识时隐藏 |
| Txt_MineralKnowledgeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看知识 |
| Btn_MineralRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开16-通用规则 |
| Txt_MineralRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Btn_MineralFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦当前资源点 |
| Txt_MineralFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；聚焦 |
| Panel_MineralPublic | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-36) | absolute | Image；公开状态 |
| Txt_MineralPublicHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；公开状态 |
| List_MineralPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MineralPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MineralPublic | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MineralPublicBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；剩余储量；可开采数量；耗尽状态；缓存容量及阻塞 |
| Item_MineralPublicTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MineralPublicRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MineralPublicRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MineralPublicRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MineralSensor | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-266) | absolute | Image；关联传感数据 |
| Txt_MineralSensorHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；关联传感数据 |
| List_MineralSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MineralSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MineralSensor | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MineralSensorBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；有效采样；来源机器／传感器；采样时刻；失效说明 |
| Item_MineralSensorTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MineralSensorRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MineralSensorRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MineralSensorRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_MineralRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-496) | absolute | Image；生产记录与规则 |
| Txt_MineralRecordHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；生产记录与规则 |
| List_MineralRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_MineralRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_MineralRecord | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_MineralRecordBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；阻塞；生产事件；自然语言作业描述；规则描述ID |
| Item_MineralRecordTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_MineralRecordRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_MineralRecordRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_MineralRecordRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_MineralLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MineralLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MineralLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_MineralEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MineralEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MineralEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_MineralErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MineralErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MineralErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_MineralSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MineralSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MineralSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_MineralDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_MineralDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_MineralDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## Water：水域详情

功能文档：[水域详情](Water.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageWater [Image]
  Txt_WaterTitle [TextMeshProUGUI]
  Grp_WaterActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_WaterRecord [Button + Image]
      Txt_WaterRecordLabel [TextMeshProUGUI]
    Btn_WaterKnowledge [Button + Image]
      Txt_WaterKnowledgeLabel [TextMeshProUGUI]
    Btn_WaterRule [Button + Image]
      Txt_WaterRuleLabel [TextMeshProUGUI]
    Btn_WaterFocus [Button + Image]
      Txt_WaterFocusLabel [TextMeshProUGUI]
  Panel_WaterPublic [Image]
    Txt_WaterPublicHeading [TextMeshProUGUI]
    List_WaterPublic [ScrollRect vertical=true horizontal=false]
      Viewport_WaterPublic [RectMask2D]
        Content_WaterPublic [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WaterPublicBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WaterPublicTemplate [LayoutElement + Image；默认inactive]
            Btn_WaterPublicRow [Button + Image]
              Txt_WaterPublicRowLabel [TextMeshProUGUI]
              Txt_WaterPublicRowValue [TextMeshProUGUI]
  Panel_WaterSensor [Image]
    Txt_WaterSensorHeading [TextMeshProUGUI]
    List_WaterSensor [ScrollRect vertical=true horizontal=false]
      Viewport_WaterSensor [RectMask2D]
        Content_WaterSensor [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WaterSensorBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WaterSensorTemplate [LayoutElement + Image；默认inactive]
            Btn_WaterSensorRow [Button + Image]
              Txt_WaterSensorRowLabel [TextMeshProUGUI]
              Txt_WaterSensorRowValue [TextMeshProUGUI]
  Panel_WaterRecord [Image]
    Txt_WaterRecordHeading [TextMeshProUGUI]
    List_WaterRecord [ScrollRect vertical=true horizontal=false]
      Viewport_WaterRecord [RectMask2D]
        Content_WaterRecord [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_WaterRecordBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_WaterRecordTemplate [LayoutElement + Image；默认inactive]
            Btn_WaterRecordRow [Button + Image]
              Txt_WaterRecordRowLabel [TextMeshProUGUI]
              Txt_WaterRecordRowValue [TextMeshProUGUI]
  Grp_WaterLoadingState [无Graphic]
    Panel_WaterLoadingMessage [Image]
      Txt_WaterLoadingMessage [TextMeshProUGUI]
  Grp_WaterEmptyState [无Graphic]
    Panel_WaterEmptyMessage [Image]
      Txt_WaterEmptyMessage [TextMeshProUGUI]
  Grp_WaterErrorState [无Graphic]
    Panel_WaterErrorMessage [Image]
      Txt_WaterErrorMessage [TextMeshProUGUI]
  Grp_WaterSuccessState [无Graphic]
    Panel_WaterSuccessMessage [Image]
      Txt_WaterSuccessMessage [TextMeshProUGUI]
  Grp_WaterDisabledState [无Graphic]
    Panel_WaterDisabledMessage [Image]
      Txt_WaterDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageWater | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；水域详情；内部页面根 |
| Txt_WaterTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；水域详情 |
| Grp_WaterActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_WaterRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；在本面板展开对应对象记录，保留对象选择 |
| Txt_WaterRecordLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看记录 |
| Btn_WaterKnowledge | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；农田打开16-作物图鉴；其它资源类型未配置知识时隐藏 |
| Txt_WaterKnowledgeLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；查看知识 |
| Btn_WaterRule | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；打开16-通用规则 |
| Txt_WaterRuleLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；规则说明 |
| Btn_WaterFocus | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；聚焦当前资源点 |
| Txt_WaterFocusLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；聚焦 |
| Panel_WaterPublic | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-36) | absolute | Image；公开状态 |
| Txt_WaterPublicHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；公开状态 |
| List_WaterPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WaterPublic | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WaterPublic | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WaterPublicBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；水域有效性；无限储量标识；允许关联的作业说明 |
| Item_WaterPublicTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WaterPublicRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WaterPublicRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WaterPublicRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WaterSensor | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-266) | absolute | Image；关联传感数据 |
| Txt_WaterSensorHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；关联传感数据 |
| List_WaterSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WaterSensor | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WaterSensor | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WaterSensorBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；有效采样；来源机器／传感器；采样时刻；失效说明 |
| Item_WaterSensorTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WaterSensorRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WaterSensorRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WaterSensorRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_WaterRecord | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,218); pos(0,-496) | absolute | Image；生产记录与规则 |
| Txt_WaterRecordHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；生产记录与规则 |
| List_WaterRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_WaterRecord | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_WaterRecord | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_WaterRecordBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；阻塞；生产事件；自然语言作业描述；规则描述ID |
| Item_WaterRecordTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_WaterRecordRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_WaterRecordRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_WaterRecordRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_WaterLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WaterLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WaterLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_WaterEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WaterEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WaterEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_WaterErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WaterErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WaterErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_WaterSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WaterSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WaterSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_WaterDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_WaterDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_WaterDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |

## SensorRecords：关联传感数据与资源记录

功能文档：[关联传感数据与资源记录](SensorRecords.md)；归属 `FieldHudForm`；内容 520×832。

```text
Panel_PageSensorRecords [Image]
  Txt_SensorRecordsTitle [TextMeshProUGUI]
  Grp_SensorRecordsActions [GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)]
    Btn_SensorRecordsFilter [Button + Image]
      Txt_SensorRecordsFilterLabel [TextMeshProUGUI]
    Btn_SensorRecordsSource [Button + Image]
      Txt_SensorRecordsSourceLabel [TextMeshProUGUI]
    Btn_SensorRecordsBack [Button + Image]
      Txt_SensorRecordsBackLabel [TextMeshProUGUI]
  Panel_SensorRecordsSamples [Image]
    Txt_SensorRecordsSamplesHeading [TextMeshProUGUI]
    List_SensorRecordsSamples [ScrollRect vertical=true horizontal=false]
      Viewport_SensorRecordsSamples [RectMask2D]
        Content_SensorRecordsSamples [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SensorRecordsSamplesBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SensorRecordsSamplesTemplate [LayoutElement + Image；默认inactive]
            Btn_SensorRecordsSamplesRow [Button + Image]
              Txt_SensorRecordsSamplesRowLabel [TextMeshProUGUI]
              Txt_SensorRecordsSamplesRowValue [TextMeshProUGUI]
  Panel_SensorRecordsEvents [Image]
    Txt_SensorRecordsEventsHeading [TextMeshProUGUI]
    List_SensorRecordsEvents [ScrollRect vertical=true horizontal=false]
      Viewport_SensorRecordsEvents [RectMask2D]
        Content_SensorRecordsEvents [VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained]
          Txt_SensorRecordsEventsBody [TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高]
          Item_SensorRecordsEventsTemplate [LayoutElement + Image；默认inactive]
            Btn_SensorRecordsEventsRow [Button + Image]
              Txt_SensorRecordsEventsRowLabel [TextMeshProUGUI]
              Txt_SensorRecordsEventsRowValue [TextMeshProUGUI]
  Grp_SensorRecordsLoadingState [无Graphic]
    Panel_SensorRecordsLoadingMessage [Image]
      Txt_SensorRecordsLoadingMessage [TextMeshProUGUI]
  Grp_SensorRecordsEmptyState [无Graphic]
    Panel_SensorRecordsEmptyMessage [Image]
      Txt_SensorRecordsEmptyMessage [TextMeshProUGUI]
  Grp_SensorRecordsErrorState [无Graphic]
    Panel_SensorRecordsErrorMessage [Image]
      Txt_SensorRecordsErrorMessage [TextMeshProUGUI]
  Grp_SensorRecordsSuccessState [无Graphic]
    Panel_SensorRecordsSuccessMessage [Image]
      Txt_SensorRecordsSuccessMessage [TextMeshProUGUI]
  Grp_SensorRecordsDisabledState [无Graphic]
    Panel_SensorRecordsDisabledMessage [Image]
      Txt_SensorRecordsDisabledMessage [TextMeshProUGUI]
```

| 节点 | RectTransform | 控制 | 组件／展示／行为 |
|---|---|---|---|
| Panel_PageSensorRecords | min(1,1) max(1,1); pivot(1,1); sizeDelta(520,832); pos(-24,-120) | absolute | Image；关联传感数据与资源记录；内部页面根 |
| Txt_SensorRecordsTitle | min(0,1) max(0,1); pivot(0,1); sizeDelta(432,32); pos(8,0) | absolute | TextMeshProUGUI；关联传感数据与资源记录 |
| Grp_SensorRecordsActions | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-16,104); pos(0,0) | absolute | GridLayoutGroup fixedColumns=2 cell=(248,48) spacing=(8,8)；操作区 |
| Btn_SensorRecordsFilter | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；只筛已有可见数据，不推算未解锁变量 |
| Txt_SensorRecordsFilterLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；筛选字段或土地单元 |
| Btn_SensorRecordsSource | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；来源机器有效时进入现场或中枢摘要 |
| Txt_SensorRecordsSourceLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；定位数据来源 |
| Btn_SensorRecordsBack | min(0,1) max(0,1); pivot(0,1); sizeDelta(248,48)初始化; pos(0,0)初始化; LayoutElement preferred(248,48); 最终位置/尺寸由组驱动 | group | Button + Image；恢复原对象和滚动位置 |
| Txt_SensorRecordsBackLabel | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-16,-8); pos(0,0) | absolute | TextMeshProUGUI；返回资源详情 |
| Panel_SensorRecordsSamples | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-36) | absolute | Image；传感记录 |
| Txt_SensorRecordsSamplesHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；传感记录 |
| List_SensorRecordsSamples | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SensorRecordsSamples | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SensorRecordsSamples | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SensorRecordsSamplesBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；土地单元或对象ID；字段名；数值和单位；来源；采样时间；有效性 |
| Item_SensorRecordsSamplesTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SensorRecordsSamplesRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SensorRecordsSamplesRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SensorRecordsSamplesRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Panel_SensorRecordsEvents | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,334); pos(0,-382) | absolute | Image；对象事件 |
| Txt_SensorRecordsEventsHeading | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,32); pos(12,-8) | absolute | TextMeshProUGUI；对象事件 |
| List_SensorRecordsEvents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-52); pos(0,-18) | absolute | ScrollRect vertical=true horizontal=false；正文／真实记录 |
| Viewport_SensorRecordsEvents | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | RectMask2D；裁切 |
| Content_SensorRecordsEvents | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(0,0); pos(0,0) | absolute | VerticalLayoutGroup spacing=8 + ContentSizeFitter vertical=Preferred horizontal=Unconstrained；单层自适应 |
| Txt_SensorRecordsEventsBody | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,120)初始化; pos(0,0)初始化; LayoutElement preferred(496,120); 最终位置/尺寸由组驱动 | group | TextMeshProUGUI + LayoutElement minHeight=120 preferredHeight=实际文本行高；生产完成；缓存阻塞；恢复或耗尽；事件时间及原因 |
| Item_SensorRecordsEventsTemplate | min(0,1) max(0,1); pivot(0,1); sizeDelta(496,104)初始化; pos(0,0)初始化; LayoutElement preferred(496,104); 最终位置/尺寸由组驱动 | group | LayoutElement + Image；默认inactive；真实记录／候选条目模板 |
| Btn_SensorRecordsEventsRow | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(0,0); pos(0,0) | absolute | Button + Image；选中当前行稳定ID，更新详情；纯段落行禁用选择 |
| Txt_SensorRecordsEventsRowLabel | min(0,1) max(1,1); pivot(0.5,1); sizeDelta(-24,32); pos(0,-8) | absolute | TextMeshProUGUI；名称：— |
| Txt_SensorRecordsEventsRowValue | min(0,0) max(1,0); pivot(0.5,0); sizeDelta(-24,48); pos(0,8) | absolute | TextMeshProUGUI；值、状态、时间：— |
| Grp_SensorRecordsLoadingState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SensorRecordsLoadingMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SensorRecordsLoadingMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Loading：— |
| Grp_SensorRecordsEmptyState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SensorRecordsEmptyMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SensorRecordsEmptyMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Empty：— |
| Grp_SensorRecordsErrorState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SensorRecordsErrorMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SensorRecordsErrorMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Error：— |
| Grp_SensorRecordsSuccessState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SensorRecordsSuccessMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SensorRecordsSuccessMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Success：— |
| Grp_SensorRecordsDisabledState | min(0,1) max(0,1); pivot(0,1); sizeDelta(520,680); pos(0,-36) | absolute | 无Graphic；默认inactive；区域状态互斥，不覆盖底部返回 |
| Panel_SensorRecordsDisabledMessage | min(0.5,0.5) max(0.5,0.5); pivot(0.5,0.5); sizeDelta(504,120); pos(0,0) | absolute | Image；状态背景 |
| Txt_SensorRecordsDisabledMessage | min(0,0) max(1,1); pivot(0.5,0.5); sizeDelta(-24,-24); pos(0,0) | absolute | TextMeshProUGUI；Disabled：— |
