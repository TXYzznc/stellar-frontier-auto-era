# 第2段：测试场与空白基线

## 完成项

- `tasks.md` 1.3：已在 `ArtResource` 建立独立 ART-004／ART-005 测试场、近中远固定机位、平地／35°坡地／可调台阶测试位和空白截图基线。
- Unity Skills 8091 已核验目标为 `ArtResource`、Unity `2022.3.62f3c1`、`URP-HighFidelity`。
- 场景保存后只读核验 `isDirty=false`，包含 13 个对象、3 台相机和 1 盏灯；Unity 8091 已释放。

## ArtResource 证据

- 文字记录：`D:\unity\UnityProject\ArtResource\Docs\ArtPipeline\Evidence\b03-parallel-art-art004-art005-production-rnd\segment-01-testlab-baseline.md`
- 远景截图：`D:\unity\UnityProject\ArtResource\Assets\Art\Evidence\ART004_ART005_TestLab\Baseline_Far.png`
- 中景截图：`D:\unity\UnityProject\ArtResource\Assets\Art\Evidence\ART004_ART005_TestLab\Baseline_Mid.png`
- 近景截图：`D:\unity\UnityProject\ArtResource\Assets\Art\Evidence\ART004_ART005_TestLab\Baseline_Near.png`

制作人窗口于 2026-08-24 只读核对证据文件和三张 `1280×720` 截图。远、中景可同时辨认平地、坡面与台阶；近景提供坡面接触细节基线，后续正式资产验收不得只使用近景判断整体可读性。

## 边界

- 本段只建立测试条件与空白基线，不含 ART-004／005 生产资源、Blender 源、正式材质或玩法脚本。
- 未覆盖 ART-002／003 已冻结场景与证据。
- 未修改任务表或任何 xlsx，未占用 Blender、资源目录或 Git 索引。
