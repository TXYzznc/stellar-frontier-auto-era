# Segment 52：动作结构纠偏

## 已冻结合同

- 双扇滑动门从闭合中线向左右相反方向开启。
- 传送带由上行载料段、下行回程段、两端滚筒及四个支承滚筒构成可读闭环；上、下带面反向运动。
- 固定旋转载体的固定地基与旋转台之间具有不旋转的承力柱。
- 货舱为被动容量效应器但拥有完整动作：左右对开、可见内部货斗、抽屉式装载托盘前移／保持／收回。它仍只反映机器唯一通用容器，不拥有独立库存或算法节点。
- 可替换效应器装卸由机器界面处理，只保留短暂溶解过渡；删除机械插拔／锁销动作图。

## 实施与验证

- 重建合同原型与动作图目录后，Unity 2022.3.62f3c1 正常完成编译，Console Error 为 0。
- `FunctionalRigPrototypeCatalogContractEditModeTests` — job `14ba5449`，2/2 通过。
- `FunctionalRigPrototypeCatalogEditModeTests` — job `4cbfca70`，1/1 通过。
- `FunctionalRigPrototypeStructureValidatorEditModeTests` — job `35b037d9`，2/2 通过。
- `FunctionalRigMotionGraphCatalogEditModeTests` — job `be2cebf4`，13/13 通过。
- `CargoBayPreviewEditModeTests` — job `27145201`，1/1 通过，覆盖打开和托盘收回绑定态。
- `FunctionalRigAcceptanceDemoDirectorEditModeTests` — job `3b45cbf9`，1/1 通过。

合计 20/20 通过，失败 0、跳过 0、不确定 0。测试结束时 Unity 非 PlayMode、非编译、非更新，且无待处理域重载；测试窗口已释放 8090。

## 后续验收

本段只完成合同、基础几何、动作图与自动回归。任务 5.4 仍保留为用户对更新后验收场可视效果的最终确认。
