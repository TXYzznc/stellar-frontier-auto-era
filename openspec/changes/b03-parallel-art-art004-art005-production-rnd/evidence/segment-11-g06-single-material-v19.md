# Segment 11：G06 V19单主材质验证

> 状态：候选验证记录，尚未最终通过。后续外观差异诊断确认URP Lit要求
> Metallic(R)／Smoothness(A)，而当前MetallicRoughness的粗糙度绿色通道未被正确消费；同时
> Blender仍有12个深蓝灰辅助材质需要合并到单主材质纹理职责。后续图集诊断又确认状态对象与
> 非状态可见件共享UV区域，受限Emission Mask会投射到非状态壳体。任务3.8已重新打开，并批准
> 仅对G06 V19执行UV0唯一展开／重新打包与对应单主材质重烘焙；本段图片仍只作失败候选证据。

- 范围：仅验证G06 V19候选的单资产单主材质＋多功能贴图交付，不修改已通过的V17／V18结构、
  Pivot、锚点或`RuntimePreview`。
- Unity主材质候选：`M_G06_Master_V19`。
- Renderer：候选全部220个Renderer由编辑器绑定工具显式指向同一Unity主材质。
- 贴图：BaseColor、Normal、Metallic／Roughness、Emission Mask已绑定；AO烘入BaseColor。
- 场景：`Assets/Art/LookDev/ART005_G06_FormalCarrier_Validation.unity`已保存，`isDirty=false`。
- 视觉自验：车轮与弧形轮框连续，无可见悬浮或游离部件。
- Unity验证：Console Error=0。
- 近中远证据：
  - `Assets/Art/Evidence/ART004_ART005_VerticalSlice/G06_V19_MaterialValidation_Near_Final.png`
  - `Assets/Art/Evidence/ART004_ART005_VerticalSlice/G06_V19_MaterialValidation_Mid_Final.png`
  - `Assets/Art/Evidence/ART004_ART005_VerticalSlice/G06_V19_MaterialValidation_Far_Final.png`
- 资源状态：ArtResource Unity Skills 8091已释放，2D窗口已收到点对点通知。
