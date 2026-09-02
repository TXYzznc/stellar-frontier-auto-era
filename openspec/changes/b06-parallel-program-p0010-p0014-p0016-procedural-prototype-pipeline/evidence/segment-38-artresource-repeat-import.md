# 第38段：ArtResource Motion Core 1.1.0 重复导入验证

日期：2026-09-02

- ArtResource 已通过本地 UPM 解析 `com.autoera.motion-core` 1.1.0；manifest/lock 固定为 `file:../../stellar-frontier-auto-era/Tools/Exports/AutoEra.MotionCore-1.1.0`、`source=local`。
- 首次导入后执行同路径、同版本 AssetDatabase 刷新和包列表刷新（job `d53255b6`）；包仍已安装，未产生重复包或 `Assets` 源码副本。
- 重复导入前后保持不变：合同 GUID `10dfac475bd5a9a4bbeacb3b1c5eba10`、MotionRig Prefab GUID `f4dfd5da764a76843bc745e5bdd9ce88`、MotionGraph GUID `b16d32a6ea6451b43b93f05893633db7`；Prefab 的 MotionRig 脚本 GUID `e1d5d9dc023da624e98ea2f976d73699` 与 `wheel_joint` 绑定，以及 MotionGraph 脚本 GUID `4601968ee1e000146a634dc327a552ce` 均未丢失。
- 合同内容指纹为 `53446c4b8954fa3f232048c8830e079877c540ce2f20666588d4fb66f088a143`；Runtime/Editor asmdef GUID 分别为 `67dc76c57f79a4b4bb323ab361b2680e` / `e6266b4304f7f27489dfe96877acfa63`。所有 `.cs` 保留 `.meta`，且不含 Adapter、场景或 xlsx。
- ArtResource Unity 2022.3.62f3c1：非 PlayMode、未编译、未更新，Console Error=0。外部原始证据：`D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\Evidence\b06-artresource-motioncore-repeat-import\verification.md`。
