# ART-002 统一LookDev基座

美术工程已建立`Assets/Art/LookDev/Scenes/ART002_LookDev_Base.unity`，固定使用High Fidelity / URP-HighFidelity / 4x MSAA和1920×1080输出。

场景包含相同的环境／建筑Prefab、可拆分机器Prefab、1米／2米尺度参照、4×4米占地、通行带、作业区和资源点占位。Far、Mid、Near三台透视评审相机分别覆盖全局层级、中距并置可读性和机器功能细节；正式游戏相机仍保持未决。

灯光固定为暖色Directional Key与冷色无阴影Fill，后处理固定为Neutral Tonemapping、轻微Color Adjustments和低强度Bloom。三台相机均开启URP Post Processing。

基座已生成三张1920×1080固定视角截图，最终美术工程Console为0 Warning、0 Error。完整参数见美术工程`Docs/ArtPipeline/ART-002-LookDevContract.md`。
