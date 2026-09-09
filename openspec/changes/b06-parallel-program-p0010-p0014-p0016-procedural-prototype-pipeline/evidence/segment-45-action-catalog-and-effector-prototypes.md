# 第45段：动作图目录与效应器机构原型

日期：2026-09-03

## 实现

- 合同目录增加水枪、旋转锯盘、旋转钻头和被动货舱四类可辨认基础几何；每类分别声明关节、局部轴、范围、锚点、净空、碰撞与视觉槽。
- 目录原型Builder为每个合同生成`MotionRig`，并把合同ID写入Rig；动作预览按合同ID筛选，避免同名关节导致跨对象显示或驱动。
- 水枪、锯盘和钻头Prefab绑定`EffectorWorkRigPreview`；关节行程、轴向与转速均为可调序列化配置，预览只表达机构姿态，不结算灌溉、伤害或采矿。
- 新增可重复生成的正式动作图目录：轮式行驶／蟹行、机械臂工作空间扫描、水枪瞄准喷射、锯盘切割、钻头钻探、滑动门开启、传送带运行与可替换效应器锁定。
- 货舱保持被动容量效应器，不生成算法可调用的作业动作图。

## 验证

- QA job `1e456713`：`FunctionalRigMotionGraphCatalogEditModeTests` 9/9通过。
- QA job `643b5f0e`：`MotionGraphAssetEditModeTests` 3/3通过。
- QA job `eeacc3db`：`FunctionalRigPrototypeCatalogEditModeTests` 1/1通过。
- QA job `93e8c2e8`：`EffectorWorkPresentationEditModeTests` 5/5通过。
- 合计18/18通过，失败0；Unity 2022.3.62f3c1为Bypass、非PlayMode、未编译、未更新、无域重载待处理。
