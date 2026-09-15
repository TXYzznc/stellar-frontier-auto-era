# 启动与初始区域贯通（实施中）

2026-09-10，沿b02与B10追加授权。禁止直接编辑xlsx、ScriptsBuiltin、生成代码或Git索引。

## 精确清单与稳定ID

- Application/AutoEraRuntimeSettings.cs、AutoEraSceneFlow.cs及AutoEraApplicationContext.cs：配置和可取消场景流程；产品代码根均为Assets/Game/Scripts/AutoEra/。
- Procedures/AutoEraStartupProcedure.cs、AutoEraMainMenuProcedure.cs、AutoEraWorldProcedure.cs：唯一启动、菜单及世界生命周期。
- UI/MainMenuForm.cs；Editor/AutoEraStartupAssetBuilder.cs；唯一新UI Prefab为Assets/Game/Prefabs/UI/Startup/MainMenuForm.prefab。
- Scenes使用类型根Assets/Game/Scene/MainMenu.unity、InitialRegion.unity；不恢复Scenes目录。
- UITable新ID采用6000=MainMenuForm，已核对现有6001/6002为Operations，互不覆盖；UIPrefab=Startup/MainMenuForm，Default组1，不允许Esc关闭。
- 仅编辑GameData/AIData/DataTables/Core/UITable.json，经现有精确Export→Validate→Reverse→RefreshAllDataTable/GenerateUIFormNamesScript；禁止手改UIViews。
- GameData/AIData/Configs/Foundation/Runtime.json已经工具生成正式配置；AppConfigs只追加登记。

## 最小菜单状态与结构

Editor预制全屏Canvas/MenuPanel/Title/Status/EnterButton，字体和按钮视觉复用Operations资源。
idle或失败：进入/重试可用；loading：禁用防重复；离开：解除事件与取消场景请求；失败不保留世界会话。
没有设置/存档/正式菜单美术，不在运行时动态拼页。

## 当前证据

配置解析QA 60e4f4f9 2/2；区域/HUD证据见B10 client-baseline与qa-g0-g1-matrix。
正式入口PlayMode尚未通过，不勾选6.*或整批完成。

## 本轮实际增量

最小MainMenu Prefab、MainMenu场景与三Procedure登记已由AutoEraStartupAssetBuilder创建，旧Operations未重建。
Core/UITable先精确导出基线，再JSON增加6000，经既有Validate/Reverse/生成入口产生UIViews.MainMenuForm；未手改生成代码或xlsx。
首次Launch预加载暴露旧LanguagesTable.txt末尾空LanguageIcon列缺失（6列而解析器需要7列）。
通过GameDataGenerator.RefreshAllDataTable仅重新导出既有Core/LanguagesTable.xlsx后文本恢复7列；未编辑工作簿或ScriptsBuiltin。
新增AutoEraStartupAssetsEditModeTests覆盖正式菜单引用/表路径及语言空末列解析，独立QA尚待执行。

Build Settings经最新授权用EditorBuildSettings.scenes追加后调用SaveAssets持久化：
原Launch（18868d941a71f02498051516873367ce）保持首位enabled；随后MainMenu
（1b40a8f9f6c53e348bf1c276dae1f096）和InitialRegion（4ad44ff196b49de40ac1996b8b84d3e7）均enabled，configObjects仍为空。
已从保存的Launch实际进入MainMenuForm，通过EnterButton.onClick进入InitialRegion并由GF.UI加载FieldHudForm，Console Error=0。
已退出PlayMode；独立QA及重复进出/失败重试仍待验证。八对象当前仍为场景视图，GF.Entity逻辑根补齐未完成，不当作P0-010通过。

后续已补八对象GF.Entity逻辑根与受控Prefab；运行回归仍在收敛，以上为此前阶段状态。
运行测试历史：f2bb3b07在夹具前置Launch路径断言失败（TestRunner空隔离场景），已显式additive打开保存Launch。
9bcb76a0的XML显示进入等待后region为空；已修正菜单打开瞬间错误启用按钮而Procedure仍拒绝点击的竞争窗口。
同时移除本批InitialRegion多余AudioListener，仅保留Launch已有listener。均待新job证实，不能沿用早期通过计数宣称完成。
此前Console两条CS0117/CS0012来自测试GF.Procedure程序集访问，已改GameEntry组件访问，非音频环境问题；原错误记录保留。

## 后续真实结果与边界

3093292a对应本轮NUnit XML Passed 1/1，两轮菜单→世界八个GF Entity/HUD→返回释放，Console Error/Warning=0。
该结果替代上述待验证状态，不表示整个G0完成。
后续真实故障注入发现返回仍加载的菜单时重复Load，已在AutoEraSceneFlow复用自身仍加载的场景，避免覆盖原世界失败原因。
运行图由B10 evidence/client-menu-failure-fixed.png、client-menu-retry-recovered.png记录缺失路径失败和恢复配置后进入区域。
新增运行测试使用GameEntry.GetComponent<ConfigComponent>和MainMenuForm.StatusText，不引入测试程序集对Builtin.Runtime或TMP的新依赖。
最初测试直接访问GF.Config/TMP导致编译失败，已修正；没有使用那次未成功PlayMode的结果。
本次故障仅覆盖缺失/未注册世界路径、现有菜单复用和可操作重试；加载中框架重启/全部迟到事件及三类Foundation数据完整回归仍未覆盖，6.5不因此整项勾选。
