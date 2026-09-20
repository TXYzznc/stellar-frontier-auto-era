# 09 本地化 Key 约定

状态：2026-09-20 冻结约定。文案尚未录入本地化表，因此**本文件当前只冻结命名规则，不执行挂载**。

## 为什么现在不挂 UIStringKey

`UIFormBase.InitLocalization()`（`Assets/Game/Scripts/UI/Core/UIFormBase.cs`）在 `OnInit` 时做这件事：

```csharp
UIStringKey[] texts = GetComponentsInChildren<UIStringKey>(true);
foreach (var t in texts)
{
    if (t.TryGetComponent<TextMeshProUGUI>(out var textMeshCom))
    {
        textMeshCom.text = GF.Localization.GetString(t.Key);
    }
}
```

也就是说：**只要节点上挂了 `UIStringKey`，它的文本就会被 `GetString(Key)` 无条件覆盖**。
在文案尚未录入本地化表、Key 还是空串的时候挂上去，结果是原型里辛苦对好的所有占位文案
在界面打开的一瞬间被清空——而且看上去像是「界面坏了」，不容易联想到本地化。

所以挂载有一个明确的**时机条件**：该界面的文案已经录入本地化表，Key 能查到真实字符串。
在那之前，预制体里保留可读的中文占位文案，让原型仍然可以评审布局与层级。

## Key 命名规则（冻结）

```
UI.<家族>.<页面>.<节点语义>
```

| 段 | 取值 | 来源 |
|---|---|---|
| `UI` | 固定前缀 | — |
| `<家族>` | `Startup` / `System` / `Hud` / `Operations` | 界面规格家族目录（01 启动与存档 → Startup，02 系统与设置 → System，03 世界HUD → Hud，其余 → Operations） |
| `<页面>` | 契约里的页面 Id | 节点 `Panel_Page<PageId>` 去掉前缀 |
| `<节点语义>` | 节点名去掉 `Txt_<PageId>` 前缀后的剩余部分 | 节点名本身 |

示例：

| 节点 | Key |
|---|---|
| `Txt_HubObjectsIndexBody` | `UI.Operations.HubObjects.IndexBody` |
| `Txt_FarmPublicBody` | `UI.Hud.Farm.PublicBody` |
| `Txt_MainMenuIdentityBody` | `UI.Startup.MainMenu.IdentityBody` |
| `Txt_SystemMenuSessionBody` | `UI.System.SystemMenu.SessionBody` |

这条规则是**可机械推导**的：合约里已经有每个 `Txt_*` 节点的完整路径与名字，
因此将来可以写一个生成器步骤，按上表批量挂 `UIStringKey` 并填 Key，
不需要人工逐个改名（也就不会出现「同一个词两个 Key」）。

## 什么不该进本地化表

三条界线，避免把本地化表变成代码的字符串仓库：

1. **运行时拼接的文本不进表**。数量、时间、完整度这类由 `AutoEraUiFormat` 格式化的内容，
   连同它的连接词一起留在代码里；进表的只有「整句固定文案」。
2. **数据内容不进表**。对象名、玩家起的名字、型号名来自数据表，不是界面文案。
3. **开发诊断文案不进表**。例如「传感器与采样尚未接入运行路径」这类**未接入说明**，
   它们随领域接入就会被删掉，进表只会制造待清理的孤儿 Key。

第 3 条尤其需要注意：2.7 为 15 个未接入界面注入的 `NotWiredReason` 属于开发期诊断文案，
它们的存在意义恰恰是「提醒这里还没做完」，因此不本地化。

## 挂载时机与步骤

当某个界面的文案已经录入本地化表：

1. 在本地化表里按上表建立该界面的 Key，值填正式文案；
2. 运行生成器的挂载步骤（待实现），为该界面所有 `Txt_*` 节点补 `UIStringKey` 与 Key；
3. 用 `AutoEraAllUiFormsPlayModeTests` 打开一遍，确认没有文案被清空或漏翻；
4. 从「未挂载界面」清单里划掉它。

**不要**为了「先挂上再说」而填空 Key 或填占位 Key —— 那会在界面打开时清空文案，
并且这种故障看起来像布局问题，排查成本远高于收益。

## 与其他规范的关系

- 文本节点的命名与层级见 `03-Prefab与RectTransform规范.md`（`Txt_` 前缀，不承载交互）。
- 状态组与空态文案的归属见 `01-界面设计与交互规范.md`：区域级状态由
  `Grp_<区域><状态>State` 表达，其说明文本属于该区域，Key 的 `<节点语义>` 应与区域名一致。
- 可访问性描述（`AutoEraUiAccessibilityDescription`）目前用 Inspector 里手填的中文，
  将来是否本地化取决于是否随语言切换——它不在本约定的强制范围内。
