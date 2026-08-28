# Segment 12：G01技术合同与建模路线首次验收

## 审阅输入

- `ArtResource/Docs/ArtPipeline/Preproduction/b03-parallel-art-art004-art005-production-rnd/ART004-G01-Workshop-TechnicalDesign-v1.md`
- `ArtResource/Docs/ArtPipeline/Preproduction/b03-parallel-art-art004-art005-production-rnd/G01-Workshop-ModelingRoute-v1.md`
- 用户批准视觉：`G01-workshop-axonometric-precision-v1.png`
- 权威输入：`AssetInventory.md`、`FrozenTaskPackage.md`、OpenSpec设计与方案讨论增量归档。

## 首轮结论

状态：二次修订后已通过；制作人已授权G01按Blender路线进入独立白模开发，不重复请求用户。

1. 正面布局须保持批准效果图的“入口／门洞→制造状态区→输出缓存01→输出缓存02”关系；不得改成
   状态区居中、两个缓存左右对称。
2. 可见组合表补入口／门洞白模职责，但不扩张为G03的4m门机构或玩家可建设门系统。
3. 将8×6m明确为主体壳体网格占地，并另列外伸物流／等待平台的最大可见包络与交互包络，消除
   “外包固定8×6m”与锚点／平台位于`X < -4`的矛盾。
4. `ART004-MOD-WORKSHOP-SHELL`须定义为装配／组合根，不是单一可渲染大网格；白模继续使用G02的
   1m网格和2m／4m模块拼装。
5. 两份文件改用制作人审批链，并明确5类锚点共6个不可渲染对象；保留输出满对应黄色阻塞的后续
   状态语义。

## 已通过项

- 1m网格与Blender优先路线；
- 不采用AI 3D；
- 制造状态区、物流口、双输出缓存与5类锚点的功能集合；
- 传送带R&D、玩家可建设物流、管线物流和内部工序的排除边界；
- 无悬浮／游离件、自验门与首次白模交付边界。

## 最终复核

- 正面顺序已固定为入口／门洞`X=-3.0`、状态区`X=-0.85`、缓存01`X=+1.10`、缓存02`X=+2.80`。
- 物流口及物流／等待锚点已统一到`Z=+1.25`。
- 8×6m已明确为主体壳体网格占地；外伸平台按技术合同的可见／交互包络验收。
- `WORKSHOP-SHELL`为G02模块拼装根，不得生产单一可渲染大网格。
- 5类共6个不可渲染锚点、输出满黄色阻塞语义、R&D／玩家物流排除边界均已明确。
- 建模路线：Blender白模；三视图／生产说明并行完成，进入中模前完成一致性核对。
