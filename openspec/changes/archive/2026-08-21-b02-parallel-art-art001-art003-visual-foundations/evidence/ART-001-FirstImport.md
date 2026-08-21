# ART-001 首次导入证据

> 日期：2026-08-21
> 初次验证包：`auto-era-art-contract-sample-v0.1.0.unitypackage`；最终路径修订包：`auto-era-art-contract-sample-v0.1.1.unitypackage`
> 导出大小：287512 bytes
> 主工程质量档：Balanced / URP-Balanced
> 结果：通过

## 身份与路径

包内9个业务资产直接使用主工程最终类型路径：

- `Assets/Game/Prefabs/ContractSample/`
- `Assets/Game/Materials/ContractSample/`
- `Assets/Game/Config/ContractSample/`

首次导入前主工程中不存在两个样例Prefab。导入后2个Prefab、6个Material和1个TextAsset均位于声明路径，9/9 GUID与`ArtResource`清单一致。

## 结构与依赖

- 两个Prefab已实例化到`Assets/Game/Scene/ArtValidation/ART001_ImportValidation.unity`，形成主工程侧既有绑定。
- 环境Prefab包含Ground、Foundation、BuildingBody、Roof、Entrance、WorkPoint、LoadPoint、VFX、StatusLight和SelectionAnchor。
- 机器Prefab包含独立VisualRoot、Anchors、CollisionEnvelope、两级Joint、随动Socket、VFX点与状态灯。
- 6个材质均解析为`Universal Render Pipeline/Lit`，Render Queue为2000，无粉色或缺失Shader。
- 验证场景保存后直接引用两个交付Prefab GUID；没有把主工程绑定回写到美术交付Prefab。

## 健康状态

导入与验证完成后清理历史Console噪声重新观察，Unity Console为0 Warning、0 Error。此前出现的FMOD输出设备错误和Unity Skills Scene UI的临时`MissingReferenceException`均不来自交付资产，清理后未复现。
