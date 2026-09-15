# B10 可交互初始区域与静态资产并行派发

用户2026-09-10批准启动；权威为主仓库 openspec/changes/b10-interactive-world-and-static-art/。
主仓库：D:/unity/UnityProject/stellar-frontier-auto-era；美术库：D:/unity/UnityProject/ArtResource。
顺序：基础复用核对→P1交互；并行静态组件→建筑。环境全场景/完整物流/算法编辑器不在本次实施。
所有角色先重读最新AGENTS/TaskQueue/RoleRouting；技能是推荐不是限制。不自动Git，不改xlsx，
不因普通知悉或中间步骤完成停止。完整派发内安全下一步自动推进；真实视觉门禁仍等待用户。
任务表旧状态不作为重做理由；用户明确上一阶段模型/UI已完成。发现实际缺陷只记录并最小处理。

## client：b10-client-initial-region

### 作业验收边界纠正（2026-09-10）

现行《视角交互与信息呈现》明确禁止玩家直接作业按钮及右键作业，作业由机器算法触发；
B10又明确不实现算法编辑器/完整物流。因此制作人此前要求“真实输入触发队列移交”不适用于当前玩家入口，现撤回该错误测试前置。
不得为测试新增手动作业按钮或临时产品命令。B10仍须验证作业服务、拥有者释放/等待转交及HUD投影，
可使用明确标注的测试调用驱动真实服务，但不得宣称算法→作业端到端已完成。
算法触发全链保留为后续依赖项，不作为本批新按钮需求。真实输入验收仍覆盖本批已有选择、镜头、UI拦截与放置入口；
加载重复提交与过期回调防护继续本批验证。本边界纠正不等于G1或相关任务全部通过。

立即核对基础代码与b02/b05/b06/b09证据，写 evidence/client-baseline.md；完成B10任务1.2、2.1～2.6。
允许：主仓库 Assets/Game/Scripts/AutoEra/、Assets/Game/Tests/AutoEra/、Assets/Game/Scene/ 下本次独立初始区域场景、
Assets/Game/Prefabs/Entity/ 下本次对象代理、Assets/Game/Prefabs/UI/Operations/ 的最小接线、
GameData/AIData/ 下本次必要JSON（只能通过已授权工具数据流程）、本change evidence/client-*.md；b02原授权范围和自身tasks/evidence延续。
禁止：ScriptsBuiltin新增修改、重做已批准模型/UI、移动删除其它资源、直接改xlsx/生成代码、改其他角色证据。
重要结构实施前在client-baseline记录模块/精确文件清单，遵守现有架构，不新增框架或包。
美术只出缺项建议，主工程Prefab写入归客户端。8090按需取得，与QA明确交接。
验收：既有及新增EditMode/PlayMode、输入与失效、作业竞争、正常编译/引用/Console、边界审计；完成回传制作人和QA。

### 2026-09-10 选择层配置精确补充授权

客户端可通过 Unity Editor SerializedObject／正式编辑器 API 修改主仓库
`ProjectSettings/TagManager.asset` 与 `ProjectSettings/DynamicsManager.asset`：
仅在执行时重新核实未占用的用户层槽位（8–31）新增 `RegionSelection`，
并关闭该新层与所有层（含自身）的物理接触；其余既有层之间的碰撞关系、层名、SortingLayers及其它物理设置不变。
不得覆盖已占用槽位；若同名层已存在，先核验其用途，避免重复创建或改变他人用途。
选择代理仅作为独立 Trigger，不替换实体物理碰撞体；选择查询显式指定该层与 Trigger 查询方式，
不修改全局查询开关。导航采集明确排除选择代理，不以物理碰撞矩阵替代导航排除验证。
执行前后留存层索引及碰撞矩阵差异证据；QA 核验选择有效、实体阻挡不变、无额外物理接触、
导航不采集该代理、正常编译与 Console。属于既有 P1 选择合同的配置补齐，不新增玩法或框架核心修改。

本机同版本 Unity 正常保存导致的 DynamicsManager 序列化格式升级可保留，不手写 YAML 降级。
须附编辑器版本及升级前后有效物理设置对照；不能取得升级前有效值的项注明未验证，不以默认值猜测等价。

### 2026-09-10 启动与对象基础缺项收敛

采用“只补本批八对象所需 GF Entity 逻辑根／最小配置”，不扩展完整第一版对象数据骨架。
正式模型只是视觉／Motion 子层，不代替 EntityBase 生命周期；复用模型，不改其几何或动作合同。
配置只能按既有 JSON→校验→正式生成流程处理；精确文件／类型／ID先记 client-baseline，避免已有ID冲突。
P0-010 仅在对应 GF.Entity 生命周期实际验证后记录覆盖；P0-011 完整骨架仍为后续未完成项，
不得以 B10 最小配置或纯 C# 注册代替 G0 整体验收。

为贯通 b02 已有最小 Startup→MainMenu→World 和失败重试合同，允许客户端补一个最小功能性
GF MainMenuForm：仅进入世界／重试、加载状态与错误说明，不制作正式主菜单视觉、不新增设置／存档等页面。
代码仍限 Assets/Game/Scripts/AutoEra/；最小稳定入口限
Assets/Game/Prefabs/UI/Startup/MainMenuForm.prefab 及其 .meta；复用既有字体与控件样式，
通过 Editor 预制，不以运行时临时拼页代替 Prefab。UIViews／表项由受控 JSON 与既有生成器产生，
不得手改生成文件。b02 原场景／AppConfigs 授权延续；补充文件清单和配置引用到 b02 原证据，B10只引用。
验收加载失败仍可操作、重复进出世界、异步回调失效、输入经过已有抽象、引用及Console；不触碰ScriptsBuiltin。

### 2026-09-10 启动场景构建注册补充授权

已只读核实两目标场景存在，当前构建列表仅含启用的 Launch。
允许客户端通过 `EditorBuildSettings.scenes` 向 `ProjectSettings/EditorBuildSettings.asset`
幂等追加并启用 `Assets/Game/Scene/MainMenu.unity`、`Assets/Game/Scene/InitialRegion.unity`。
执行前重新读取列表；保留所有既有条目、顺序、启用状态及 configObjects，不重复追加或手改YAML。
若目标已被其他工作新增但禁用，先报告冲突，不擅自覆盖该状态。
记录前后路径/GUID/开关，验证 Launch→MainMenu→InitialRegion 实际加载、失败可重试及重复进出；
不得以注册完成替代端到端验收，不改框架加载器。此为本轮启动配置补齐，不新增场景制作范围。

### 2026-09-10 框架纯度审计的产品实例适配

制作人已复跑确认9项失败来自旧框架专用规则：仅允许Launch以及将所有MainMenu文本判为样例。
允许客户端在当前B10内修正 `tools/audit_framework_purity.py`、
`tools/tests/test_audit_framework_purity.py`，新增 `tools/audit_product_profile.json`，
并仅更新 `Docs/Development/ProjectBaseline.md` 的审计用法说明。
通用脚本不得硬编码本产品路径或默认全局豁免MainMenu；采用显式产品配置与可显式选择的严格框架模式。
未提供产品配置时保持原严格行为，项目日常命令的产品模式用法需明确记录，并同步本批验证调用。
产品配置只列本次已授权精确场景、精确产品启动文件及必要生成UIViews入口、规则名与来源派发依据；
只针对MainMenu标识作受限例外，SampleScene及其他禁用模式继续检查，不跳过整个文件或AutoEra目录。
配置路径须为仓库内规范相对路径；错误/缺失/越界配置必须失败，不能静默退回宽松模式。
ScriptsBuiltin、Agent/SKILL、框架OpenSpec、其他目录/生成物与边界审计保持有效。
须增加正反例：严格框架仍拒绝产品场景/MainMenu，产品配置仅接受精确授权项，
未授权场景、框架核心MainMenu、同一已授权文件中的SampleScene、路径逃逸/无效配置仍失败。
保留原9项失败证据，附测试与产品模式/项目边界审计结果；不得仅以门禁变绿宣称实现正确。

## art-3d：b10-static-art-contracts-and-production

### 2026-09-11用户批准十二项材质制作及视觉门禁后正式交付

用户对制作人列出的两项建议明确“确认”，其中B10采用上一批用户最终选择的原合格材质标准，制作必要UV和贴图，提交渲染图后由用户决定，再进入正式交付。此条覆盖下方历史“无材质/UV/导出授权”的冻结，模型细化通过不需重新验收造型。

恢复现有任务，从B10-ModelAcceptance-Closeout-20260911.md及B10-RefinementReview-20260911.md列明的十二项Refinement_v02已接受源派生独立材质候选；保留原源与全部既有证据。按ART013六组件→ART015六建筑顺序连续实施，不重做几何、安装位置、地形适配，不增加LOD或自动减面。需要UV拆缝/材质职责时保留稳定节点、父子、Pivot/轴/尺度和可见造型。

允许ArtResource既有 `ArtSource/FirstVersion/B10_StaticAssets/` 下逐资产独立材质阶段目录、`Docs/ArtPipeline/Preproduction/b10-interactive-world-and-static-art/`、`Assets/Art/Authoring/B10_StaticAssets/` 的候选打包。先从当前正式上一批资源及有效交付记录定位用户接受的Control材质/贴图标准，登记精确参考路径与职责，不恢复被否决的材质实验；历史证据路径缺失先查当前实际正式资源，不据此重做B08。使用Blender并按已验证可用路线结合Material Maker，不将生成图纯色/工具烟测成功冒充可用贴图。

沿用单资产单Unity主材质＋多功能贴图规范，透明或真正独立动态资产例外按既有规范；BaseColor/Normal/Metallic-Smoothness/AO/Emission按实际需要配置，金属粗糙度通道须转换为Unity消费格式，sRGB/Linear职责准确。避免无关对象共享UV产生串色/误发光；发光仅限有功能依据部分，不能用预览语义色充当已烘焙PBR。

每资产完成DCC自验与纹理导出可用性检查，提供1920×1080完整模型轴测全景＋关键近景，分批完成可先提交但其他安全制作不停。附源/贴图/通道/分辨率/参考和自验清单，先实际发制作人技术视觉报告，再用户最终裁决。不自行批准视觉或因细节偏好重做已接受几何。

当前阶段不导入正式项目、不替换正式Entity/Prefab；用户渲染视觉通过后按已同意顺序先一个代表资产验证轴/大小/贴图，再全量正式交付。正式目标以AssetOrganization.md类型/业务目录为准，届时补精确目标清单并由客户端承担主工程接入，不把过程源/截图/测试材质放入主工程。准备阶段可导出到ArtResource候选目录验证，不越过用户视觉门禁。

主美在本批OpenSpec现有3.6及设计/spec中同步新增范围与未完成交付边界，按OpenSpec技能严格验证后连续制作。3.6不提前勾选。无Git、xlsx或8090占用；DCC/8091按需协调。真实视觉等待block让位，不等待普通已阅。

### 2026-09-11十二项Refinement_v02用户视觉通过

用户对制作人细化报告明确回复“确认通过”。本次覆盖 B10-RefinementReview-20260911.md 列明的十二项 Refinement_v02 模型细化候选；保留现有精度、造型和细节，不继续因大面留白或紧固件分段感返工。结构基线与旧证据继续保留。

解除本轮用户视觉等待，主美同步本地报告、候选索引与检查点，并依据完整自验/交付证据收口3.5。3.6中的用户模型验收已完成，但正式交付尚未完成，维持未勾选并注明差异；不得将本次视觉通过扩大为材质/UV/LOD/Unity正式导入或全组合动作验证通过。当前恢复范围仅为验收记录与生产阶段收口，不启动新材质或导出流程。若剩余正式交付需新授权，记录精确交付准备情况与范围后block让位，不丢失剩余任务，不占Active等待普通已阅。收口后实际回传制作人，不自动Git、不改xlsx。

### 2026-09-11其余七项结构通过，恢复精雕细化

用户查看七项候选后明确：“保留，这些结构和功能没问题，可以继续精雕和细化了。”对象为 ObjectStateSensor v07、ExplorationScanner v03、CommunicationReceiver v03、BasicWarehouse v03、BiomassGenerator v02、SolarGenerator v01、BasicBattery v01。保留这些候选的整体造型、比例与功能结构，不从零重做；此条解除之前的七项视觉决定等待，并覆盖下面历史记录中的精雕冻结。

结合此前计算核心、通信发射器、土壤传感器、水泵阶段通过及工坊v04贯通门洞通过，当前B10结构阶段决策已齐。主美按既定ART013组件→ART015建筑顺序继续本批精雕与细化；以已确认轴测和合同补硬表面层次、装配连接、护角、检修盖、紧固件等真实细节。探索扫描器缺少的窄竖灯条可局部补齐，不新增功能或动作。保留已通过源和证据，另存细化版本；安装位置留用户后续微调，不反复调整已认可基线。

继续排除地形适配、额外LOD、运行时新动作、内部玩法与历史动作资产重做。此次批准不是最终精修、材质或正式导入交付验收；不据此扩大到材质/Unity正式导出。完成本批细化与自身结构核验后，实际回传制作人并附轴测和关键近景，由用户决定最终视觉；中间安全步骤连续执行，不等待普通已阅，不自动Git。

### 2026-09-11工坊v04贯通门洞用户通过

用户查看左右InsideWide贯通证据后明确“通过”。仅确认ManufacturingWorkshop v04两侧输出门洞最小返修；保留v04闭合源、原门叶、输出台大小/位置及其它主体。与此前1～4阶段通过共同留档，不扩大成整批12项、精雕、全组合动作净空、材质或正式交付通过。

主美可完成该局部通过状态的报告/索引/检查点同步，不再返修门洞。其它尚未交用户决定的候选保持待审，不据本条启动整批精雕或重做。完成记录后若无其它已授权安全工作，按真实剩余门禁维护blocked，不为普通已阅占Active；不自动Git。

### 2026-09-11用户结构候选裁决：1～4阶段通过，工坊输出门最小返修

用户明确暂时通过本轮报告的1计算核心、2通信发射器、3土壤传感器、4水泵；安装位置后续由用户按实际画面微调。保留四项当前造型和安装阶段基线，不再要求前罩变薄、安装后移裁决或水泵布管返工。此为阶段通过，不代表精雕/全组合运动净空/正式导出验收，也不连带批准其余七项。

第5项工坊是功能结构错误，不是输出台偏小：两侧输出门必须对应墙体真实开口，打开后能够连通内外，不能把门贴在完整墙外、门后仍是实墙。撤销制作人上一条“适当增大输出台”的建议，不擅改大小。恢复当前Active仅做该最小结构返修及必要文档/证据收口。

主美先在最新独立ManufacturingWorkshop候选中定位两侧输出门及遮挡墙段，修正贯通门洞、洞口侧壁厚度、门框与门叶配合；门洞后不可残留整墙或紧贴假背板。保留两独立输出位、主入口及已确认主体尺寸/造型，不重做整栋、不增加内部生产线或可进入建筑玩法。为静态模型提供门叶移开/隐藏的非发布检查状态即可，不新增运行时门动画系统或冻结新动作参数。

在新版本保存，不覆盖旧证据；补闭合全景、两侧各自打开/隐藏门叶后的斜向贯通近景，核验开口与承力连接无误。同步ART015技术合同和整批报告对两输出门的结构表述，明确用户新增的贯通要求。完成实际回传制作人供用户确认，不据此自动勾选整批3.5/3.6，不启动其它模型精雕/材质/Unity/Git。

### 2026-09-11地形要求排除确认

用户再次明确暂不考虑地形，地形自适应由用户后续程序实现；沿用DEC-196，不新增设计分支。制作人及主美验收清单均排除地面贴合、坡度适应、岸形/水位适配及为此新增关节/支腿的要求。不将缺少地形代理或地面接触效果视为本批返工理由，也不据此扩大/重做模型。

模型自身装配、连续承力连接、既定运动链与内部净空仍保留；“不做地形适配”不豁免模型自身游离零件或组件与宿主的错误挂接。已存在的静态支撑不用删除。当前B10-BatchCandidateReview-20260911已按此排除地形问题；固定核心悬挑、发射器安装及外观比例属于模型自身问题，应提供证据交用户决定，不自动裁决返工。

立即开展任务3.1；只先做技术文档，先ART-013静态组件，再ART-015仓库/工坊/能源建筑/岸边水泵。
允许ArtResource：Docs/ArtPipeline/Preproduction/b10-interactive-world-and-static-art/、
ArtSource/FirstVersion/B10_StaticAssets/、Assets/Art/Authoring/B10_StaticAssets/。
阶段门：文档交制作人核对→对应原画→制作人报告→用户决策→生产图同链→建模，不能提前DCC。
原画任务由制作人合同结论后入队，主美接收交付并维持技术权威。模型自行结构自验，最终视觉用户决定。
严禁重做既有动作资产，禁止仓库内部库存/容量四档、泵机运动、追日、额外LOD和玩法。尺寸来自当前设计，缺失仅在合同列建议。
文档完成实际发送制作人，不仅在本窗口写请求。可继续下一批合同准备，不等待普通知悉。
仅按需使用8091/DCC，不占用8090；完成回传制作人。

## art-concept-3d：b10-concept-input-readiness

立即只读核对已批准风格参考、静态候选设计来源和主美输入要求，写ArtResource
ArtSource/Concept/b10-interactive-world-and-static-art/InputReadiness.md。
允许仅上述新目录；禁止任何旧图改写、Blender/Unity/建模。本首包不生成图片，不冻结新尺寸。
依据当前GameDesign与已验收B08原画，列参考绝对路径、用途与待合同字段，不使用rejected图。
交付主美与制作人后complete；后续出图必须由已核验合同的独立任务入队。
所有轴测/三视图候选先发制作人并抄主美；不直接向用户请求通过/返工。制作人看图报告后由用户决定。

## art-2d：b10-ui-incremental-fields

立即对P1对象侧栏/HUD与现有Operations做缺项核对，先与客户端明确公开摘要和节点职责。
允许ArtResource Docs/ArtPipeline/UIRequirements/B10_InitialRegion/、Assets/Art/Authoring/ART006_UI/B10_Incremental/。
无缺项不强造资源；有缺项复用已验收样式，在独立候选补齐并按1920×1080提供证据。
禁止重设计整套UI、多比例适配、写主工程Prefab/C#、重做已批准页面。只向客户端交付最小候选与映射。
可按需协调8091；当前先文档不驱动Unity。完成回传客户端和制作人。

### 2D后续独立包：b10-ui-runtime-increment-review

由art-2d执行，只核对本轮新增的FieldHud真实摘要绑定与最小Startup/MainMenuForm。
现有B10缺项清单已完成，不重开；只读主工程当前Prefab/绑定代码及客户端实际1920×1080截图，
可写ArtResource `Docs/ArtPipeline/UIRequirements/B10_InitialRegion/RuntimeIncrementReview.md`。
先核对已有静态节点与字段，向客户端点对点取得运行证据；需要补图由当前8090持有人捕获，2D不抢占8090。
核验加载/可点击/失败重试、无选择与三类对象摘要、长文本、无假数值/叠字/遮挡、状态切换；
没有运行证据的项明确待验，不拿旧ART006截图代替。缺陷直接交客户端最小修正，不改C#/Prefab、
不重做整套UI、不制作正式主菜单视觉、不做多比例。完成后实际通知制作人及客户端。

## qa：b10-initial-region-independent-validation

先只读现有证据建立G0/G1用例矩阵与缺失覆盖，不将陈旧未勾选当作失败。
允许主change evidence/qa-*.md及Assets/Game/Tests/AutoEra/下明确QA命名的新测试文件；新增测试先与客户端确认避免重叠。
禁止修改实现/资源/xlsx/其他角色证据。8090测试须与客户端点对点取得，不默认长期占用。
客户端基础/初始区域可测后复跑实际回归；等待期间完善独立边界用例，结果回传客户端，阶段报告给制作人。

## rapid-executor：b10-baseline-path-audit

单一目标：只读扫描现有正式Entity、Operations、MotionContracts及b02/b05/b06/b08/b09任务证据路径。
仅可写主change evidence/rapid-path-audit.json 和 evidence/rapid-path-audit.md。
输入：现有上述change tasks.md、Assets/Game/Prefabs/Entity、Assets/Game/Prefabs/UI/Operations、
Assets/Game/Config（只扫描Motion合同相关）、Assets/Game/MotionGraphs；找不到的目录明确missing，不猜名称。
输出每个入口路径、存在性、.meta是否存在、各change勾选/未勾选计数；不读取凭据，不裁决完成状态。
自动门禁：JSON可解析、每条exists/meta与磁盘匹配、统计与tasks文本匹配；不改源、不运行Unity、不修改队列外状态。
完成/失败均回传制作人和客户端。该清单完成即收尾，不需要等待其它角色。

## Git

无派发，保持空闲；仅用户手动触发。
