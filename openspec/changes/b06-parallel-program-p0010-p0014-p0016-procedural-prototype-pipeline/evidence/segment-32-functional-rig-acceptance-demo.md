# 第 5.2 段：基础几何验收演示场

日期：2026-09-02

- 新建独立场景 `Assets/Game/Scenes/AutoEra/FunctionalRigAcceptanceDemo.unity`，不加载或改写正式场景。
- 场景以六个现有 Catalog Prefab 的显式根实例、固定相机、定向光和 `AcceptanceDemoDirector` 组成；导演仅绑定五个 `RigRoot`，复用四轮、机械臂、门与传送带的表现计算，并在退出 Play Mode 时恢复绑定姿态。
- 预览图：`FunctionalRigAcceptanceDemo-preview.png`；运行帧：`FunctionalRigAcceptanceDemo-playing.png`。
- 现场验证：Prefab 批量实例化 6/6 成功；场景检查无 Error／Warning（10 条为跨 Prefab 内部相同子节点名的信息）；短时 Play Mode 后已停止，编辑器非 PlayMode、场景非 Dirty、Console Error=0。
