# 第43段：机械臂五自由度合同

日期：2026-09-03

## 实现

- 原先含伸缩通道的四节点简化臂已替换为五自由度父子链：`base_yaw`、`shoulder_pitch`、`elbow_pitch`、`wrist_pitch`、`wrist_roll`。
- `base_yaw` 为Y轴连续回转；后四个均为受限旋转关节，合同分别声明局部轴、稳定ID、父节点和角度范围。
- `ArmPresentationSolver` 改为两段式确定性到达求解，并由 `ArmPresentationConfiguration` 提供臂段长度、禁入半径、四关节限位与休息姿态；不再硬编码单一伸缩量或固定关节角。
- `FunctionalRigAcceptanceDemoDirector` 直接驱动五个实际关节，不再驱动旧的 `yaw/shoulder/extend/wrist` 简化节点。
- `multi_joint_arm` 合同版本提升至 `1.2.0`。现有Prefab/导出物不手改YAML，将由目录构建器在后续普通编译后可重复生成。

## 生成资产与导出

- 普通 `AssetDatabase.Refresh` 后，固定菜单 `AutoEra/Functional Prototypes/Build Representative Catalog` 重建了机械臂Prefab；`Export Contract Catalog` 重新导出合同清单。
- 导出的机械臂合同为 `1.2.0`，包含五级关节链、更新后的边界、锚点和确定性内容指纹。其余五类原型维持既有 `1.1.0` 合同版本。
- 编译过程中发现并修正四轮测试遗漏的 `UnityEngine` 引用；Unity随后为非编译、Console Error=0。

## 验证

- QA job `6a708785`：`ArmPresentationSolverEditModeTests` 3/3通过，覆盖可达/不可达、配置化限位与休息姿态。
- QA job `729fb4a3`：`FunctionalRigPrototypeCatalogContractEditModeTests` 2/2通过，覆盖五级机械臂合同父子关系与版本。
- 合计5/5通过，失败0、跳过0、不确定0。
- QA job `9bd72efa`：`FunctionalRigPrototypeCatalogEditModeTests` 1/1通过；job `b2de7874`：`FunctionalRigPrototypeStructureValidatorEditModeTests` 2/2通过；job `c03d8f43`：`ArmPresentationSolverEditModeTests` 3/3通过。
- QA job `1f2d0058`：`FunctionalRigContractCatalogExporterEditModeTests` 1/1通过，确认五个`1.1.0`合同及机械臂`1.2.0`合同均有64位内容指纹。
- Unity 2022.3.62f3c1结束时为Bypass、非PlayMode、未编译、未更新、无域重载待处理；测试窗口已释放8090。
