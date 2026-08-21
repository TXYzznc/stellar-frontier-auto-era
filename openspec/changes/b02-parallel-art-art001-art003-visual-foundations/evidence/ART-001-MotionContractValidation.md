# ART-001 动作资源接口证据

`ArtResource`已建立`ENV_ContractSample.prefab`、`MCH_ContractSample.prefab`和机器可读`MachineJointContract.json`。样例覆盖共享URP材质、环境锚点、机器独立锚点、两级刚性关节、随动末端Socket、VFX点、状态灯和独立主碰撞包络。

关节极值、安全姿态与默认姿态已在美术工程Unity实例中逐项驱动。测试期间Prefab根、`CollisionEnvelope`、独立静态Socket和状态灯的世界位置保持不变，只有关节子树和`Socket_Effector_Main`按声明范围移动；结束后已恢复默认姿态并保存场景。

详细坐标、范围和修复记录见美术工程`Docs/ArtPipeline/ART-001-MotionContractValidation.md`。首次搭建发现的世界／局部Transform误用已在交付前修复并应用回Prefab。
