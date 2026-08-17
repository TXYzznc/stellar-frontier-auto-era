# AutoEra 产品代码根

- 自动纪元产品 C# 类型放在本目录下，并使用 `AutoEra` 或 `AutoEra.*` 命名空间。
- 按业务领域组织代码；只有对应任务进入实施时才创建子目录，不预建空骨架或无功能占位类型。
- 跨领域共享契约必须在对应 OpenSpec 中确认，不预设杂物型 `Common` 目录。
- 产品资源和 GameData 沿既有类型根目录按业务类别组织，不重复套 `AutoEra` 资源层。
- 完整边界与验证流程见[项目开发基线](../../../../Docs/Development/ProjectBaseline.md)。
