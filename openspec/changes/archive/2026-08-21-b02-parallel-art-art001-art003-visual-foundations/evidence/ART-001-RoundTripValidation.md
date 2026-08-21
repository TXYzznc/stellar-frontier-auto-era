# ART-001 最小包往返验证

ART-001合同已冻结为1.0.0。最终包`auto-era-art-contract-sample-v0.1.1.unitypackage`包含2个Prefab、6个URP/Lit材质和1个机器关节JSON，共9个业务资产，包大小287621 bytes；0.1.1移除资源路径中的冗余项目名目录层，原GUID保持不变。

首次导入后9/9 GUID与`ArtResource`清单一致，并在主工程建立`Assets/Game/Scene/ArtValidation/ART001_ImportValidation.unity`，实例化环境和机器Prefab作为主工程侧既有绑定。

同一包版本第二次导入后：

- 两个交付Prefab仍各只有一个资产搜索结果；
- 9个交付文件SHA-256全部不变；
- 验证场景SHA-256保持`89BEEF4FECD56F64F9E0331904EF1CA0A03E137F45434B633D745F72E18C2105`；
- 场景中的Prefab实例仍解析到原GUID；
- 最终Unity Console为0 Warning、0 Error。

联合边界审计发现早期`Assets/Game/<类型>/AutoEra/`路径违反项目“资源按业务类别直接组织”的约定。美术工程与主工程已同步移动到`Assets/Game/<类型>/ContractSample`，验证场景移动到`Assets/Game/Scene/ArtValidation`，随后导出0.1.1并在主工程连续导入两次。9个交付文件和独立验证场景的SHA-256均保持不变，原GUID与Prefab实例引用继续有效，冗余空目录已删除。

过程中修复了交付路径与最终路径不一致可能导致重复导入分叉的问题，以及机器关节子树世界／局部Transform误用问题。详细记录位于美术工程`Docs/ArtPipeline/ART-001-RoundTripValidation.md`。
