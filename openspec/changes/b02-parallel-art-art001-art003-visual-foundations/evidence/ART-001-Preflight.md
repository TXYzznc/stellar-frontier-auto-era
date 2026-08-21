# ART-001 Preflight Evidence

检查日期：2026-08-21。

## 路由结论

- 当前主工程实际使用Unity Skills端口8090，`project_get_info`返回项目“星际拓荒：自动纪元”、产品名“星际拓荒：自动纪元”、公司名“ZZNC”和当前仓库路径。
- 美术工程使用端口8092，返回项目`ArtResource`。
- 用户先前提供的8091当前连接独立项目`GameDesinger`；该目录与当前仓库不是联接，后续操作禁止使用8091。

## 环境结论

- 两个目标项目均为Unity 2022.3.62f3c1和URP 14.0.12。
- 主工程当前质量档为Balanced／URP-Balanced；美术工程当前为High Fidelity／URP-HighFidelity。
- 两边Console统计均为0 Warning、0 Error，检查时无编译、更新或Domain Reload挂起。
- 主工程非Play Mode，可以在后续明确步骤执行资源导入验证。
- 美术工程不包含GF_X、FSR、HybridCLR、Obfuz或AutoEra业务代码，符合独立制作边界。

## 风险

- `ArtResource`当前没有Git仓库，不能依赖Git回滚；正式资源量产前需由用户决定是否初始化独立版本库。
- LookDev可以保留High Fidelity近景检查，但统一候选对比必须使用固定质量条件，主工程最终验收至少覆盖Balanced。

ArtResource侧完整预检、交付合同、检查清单和最小包清单位于其`Docs/ArtPipeline`目录。
